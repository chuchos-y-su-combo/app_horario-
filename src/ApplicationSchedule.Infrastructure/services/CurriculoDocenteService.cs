using System.Globalization;
using System.Text;
using ApplicationSchedule.Application.DTOs.Curriculos;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Servicio encargado de leer archivos Excel de currículos/horarios docentes
/// y determinar automáticamente las asignaturas que cada docente puede dictar.
/// </summary>
public class CurriculoDocenteService : ICurriculoDocenteService
{
    private const string HojaDisponibilidad = "Disponibilidad";
    private const string FuenteExcel = "Excel";

    private readonly AppDbContext _context;

    public CurriculoDocenteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ImportarCurriculoResponse> ImportarDesdeExcelAsync(
        Stream archivo,
        string nombreArchivo,
        CancellationToken cancellationToken = default)
    {
        if (archivo.Length == 0)
        {
            throw new InvalidOperationException("El archivo Excel está vacío.");
        }

        List<Docente> docentes = await _context.Docentes.ToListAsync(cancellationToken);
        List<Asignatura> asignaturas = await _context.Asignaturas.ToListAsync(cancellationToken);

        var response = new ImportarCurriculoResponse
        {
            Archivo = nombreArchivo
        };

        using var workbook = new XLWorkbook(archivo);

        foreach (IXLWorksheet worksheet in workbook.Worksheets)
        {
            if (EsHojaIgnorada(worksheet.Name))
            {
                continue;
            }

            response.TotalHojasProcesadas++;

            CurriculoDocenteImportadoResponse detalleDocente = await ProcesarHojaDocenteAsync(
                worksheet,
                docentes,
                asignaturas,
                cancellationToken
            );

            response.Detalle.Add(detalleDocente);

            if (detalleDocente.DocenteEncontrado)
            {
                response.TotalDocentesEncontrados++;
            }
            else
            {
                response.DocentesNoEncontrados.Add(detalleDocente.NombreDocenteDetectado);
            }

            response.TotalRelacionesCreadas += detalleDocente.Asignaturas.Count(a => a.Estado == "Creada");
            response.TotalRelacionesExistentes += detalleDocente.Asignaturas.Count(a => a.Estado == "Existente");
            response.TotalAsignaturasNoEncontradas += detalleDocente.Asignaturas.Count(a => a.Estado == "Asignatura no encontrada");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<List<AsignaturaHabilitadaDocenteResponse>> ObtenerAsignaturasHabilitadasAsync(
        string idDocente,
        CancellationToken cancellationToken = default)
    {
        bool docenteExiste = await _context.Docentes
            .AnyAsync(d => d.IdDocente == idDocente, cancellationToken);

        if (!docenteExiste)
        {
            throw new InvalidOperationException("Docente no encontrado.");
        }

        return await _context.DocentesHabilitados
            .Include(dh => dh.Asignatura)
            .Where(dh => dh.IdDocente == idDocente)
            .OrderBy(dh => dh.Asignatura!.Semestre)
            .ThenBy(dh => dh.Asignatura!.Nombre)
            .Select(dh => new AsignaturaHabilitadaDocenteResponse
            {
                IdAsignatura = dh.IdAsignatura,
                Codigo = dh.Asignatura!.Codigo,
                Nombre = dh.Asignatura.Nombre,
                Creditos = dh.Asignatura.Creditos,
                Semestre = dh.Asignatura.Semestre,
                Fuente = dh.Fuente,
                FechaHabilitacion = dh.FechaHabilitacion
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<CurriculoDocenteImportadoResponse> ProcesarHojaDocenteAsync(
        IXLWorksheet worksheet,
        List<Docente> docentes,
        List<Asignatura> asignaturas,
        CancellationToken cancellationToken)
    {
        string nombreDocenteDetectado = ObtenerNombreDocente(worksheet);

        Docente? docente = docentes.FirstOrDefault(d =>
            NormalizarTexto(d.Nombre) == NormalizarTexto(nombreDocenteDetectado));

        var detalle = new CurriculoDocenteImportadoResponse
        {
            Hoja = worksheet.Name,
            NombreDocenteDetectado = nombreDocenteDetectado,
            IdDocente = docente?.IdDocente,
            DocenteEncontrado = docente is not null
        };

        List<(string Nombre, string? Codigo, int Fila)> asignaturasDetectadas = ExtraerAsignaturasDeHoja(worksheet);

        detalle.TotalAsignaturasDetectadas = asignaturasDetectadas.Count;

        if (docente is null)
        {
            foreach ((string nombre, string? codigo, int fila) in asignaturasDetectadas)
            {
                detalle.Asignaturas.Add(new CurriculoAsignaturaDetectadaResponse
                {
                    Hoja = worksheet.Name,
                    Fila = fila,
                    CodigoDetectado = codigo,
                    NombreDetectado = nombre,
                    FueHabilitada = false,
                    Estado = "Docente no encontrado",
                    Mensaje = "No existe un docente registrado con el nombre detectado en la hoja."
                });
            }

            return detalle;
        }

        var clavesProcesadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach ((string nombre, string? codigo, int fila) in asignaturasDetectadas)
        {
            Asignatura? asignatura = BuscarAsignatura(asignaturas, nombre, codigo);

            if (asignatura is null)
            {
                detalle.Asignaturas.Add(new CurriculoAsignaturaDetectadaResponse
                {
                    Hoja = worksheet.Name,
                    Fila = fila,
                    CodigoDetectado = codigo,
                    NombreDetectado = nombre,
                    FueHabilitada = false,
                    Estado = "Asignatura no encontrada",
                    Mensaje = "No existe una asignatura registrada con el código o nombre detectado."
                });

                continue;
            }

            string clave = $"{docente.IdDocente}|{asignatura.IdAsignatura}";

            if (!clavesProcesadas.Add(clave))
            {
                continue;
            }

            bool relacionExiste = await _context.DocentesHabilitados
                .AnyAsync(dh =>
                    dh.IdDocente == docente.IdDocente &&
                    dh.IdAsignatura == asignatura.IdAsignatura,
                    cancellationToken
                );

            if (!relacionExiste)
            {
                _context.DocentesHabilitados.Add(new DocenteHabilitado
                {
                    IdDocente = docente.IdDocente,
                    IdAsignatura = asignatura.IdAsignatura,
                    FechaHabilitacion = DateTime.UtcNow,
                    Fuente = FuenteExcel
                });
            }

            detalle.Asignaturas.Add(new CurriculoAsignaturaDetectadaResponse
            {
                Hoja = worksheet.Name,
                Fila = fila,
                CodigoDetectado = codigo,
                NombreDetectado = nombre,
                IdAsignatura = asignatura.IdAsignatura,
                NombreAsignaturaBaseDatos = asignatura.Nombre,
                FueHabilitada = true,
                Estado = relacionExiste ? "Existente" : "Creada",
                Mensaje = relacionExiste
                    ? "El docente ya estaba habilitado para esta asignatura."
                    : "Asignatura habilitada correctamente para el docente."
            });
        }

        detalle.TotalAsignaturasHabilitadas = detalle.Asignaturas.Count(a => a.FueHabilitada);

        return detalle;
    }

    private static bool EsHojaIgnorada(string nombreHoja)
    {
        return string.Equals(nombreHoja.Trim(), HojaDisponibilidad, StringComparison.OrdinalIgnoreCase);
    }

    private static string ObtenerNombreDocente(IXLWorksheet worksheet)
    {
        string valorProfesor = worksheet.Cell("B3").GetString().Trim();

        if (valorProfesor.StartsWith("Profesor:", StringComparison.OrdinalIgnoreCase))
        {
            string nombre = valorProfesor.Replace("Profesor:", "", StringComparison.OrdinalIgnoreCase).Trim();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                return nombre;
            }
        }

        return worksheet.Name.Trim();
    }

    private static List<(string Nombre, string? Codigo, int Fila)> ExtraerAsignaturasDeHoja(IXLWorksheet worksheet)
    {
        var resultado = new List<(string Nombre, string? Codigo, int Fila)>();

        int filaEncabezados = BuscarFilaEncabezadosAsignatura(worksheet);

        if (filaEncabezados == 0)
        {
            return resultado;
        }

        List<int> columnasAsignatura = ObtenerColumnasAsignatura(worksheet, filaEncabezados);

        int ultimaFila = worksheet.LastRowUsed()?.RowNumber() ?? filaEncabezados;

        foreach (int columna in columnasAsignatura)
        {
            for (int fila = filaEncabezados + 1; fila <= ultimaFila; fila++)
            {
                string nombreAsignatura = worksheet.Cell(fila, columna).GetString().Trim();

                if (!EsNombreAsignaturaValido(nombreAsignatura))
                {
                    continue;
                }

                string? codigo = ObtenerCodigoAsignaturaCercano(worksheet, fila, columna);

                resultado.Add((nombreAsignatura, codigo, fila));
            }
        }

        return resultado
            .GroupBy(a => $"{NormalizarTexto(a.Nombre)}|{a.Codigo}")
            .Select(g => g.First())
            .ToList();
    }

    private static int BuscarFilaEncabezadosAsignatura(IXLWorksheet worksheet)
    {
        int ultimaFila = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        int ultimaColumna = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        for (int fila = 1; fila <= ultimaFila; fila++)
        {
            for (int columna = 1; columna <= ultimaColumna; columna++)
            {
                string valor = worksheet.Cell(fila, columna).GetString().Trim();

                if (string.Equals(valor, "ASIGNATURA", StringComparison.OrdinalIgnoreCase))
                {
                    return fila;
                }
            }
        }

        return 0;
    }

    private static List<int> ObtenerColumnasAsignatura(IXLWorksheet worksheet, int filaEncabezados)
    {
        int ultimaColumna = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var columnas = new List<int>();

        for (int columna = 1; columna <= ultimaColumna; columna++)
        {
            string valor = worksheet.Cell(filaEncabezados, columna).GetString().Trim();

            if (string.Equals(valor, "ASIGNATURA", StringComparison.OrdinalIgnoreCase))
            {
                columnas.Add(columna);
            }
        }

        return columnas;
    }

    private static string? ObtenerCodigoAsignaturaCercano(IXLWorksheet worksheet, int fila, int columna)
    {
        string posibleCodigo = worksheet.Cell(fila + 1, columna).GetString().Trim();

        if (EsCodigoAsignatura(posibleCodigo))
        {
            return posibleCodigo;
        }

        return null;
    }

    private static bool EsCodigoAsignatura(string valor)
    {
        return !string.IsNullOrWhiteSpace(valor)
            && valor.Length <= 20
            && valor.All(char.IsDigit);
    }

    private static bool EsNombreAsignaturaValido(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        string normalizado = NormalizarTexto(valor);

        string[] valoresIgnorados =
        {
            "HORA",
            "AULA",
            "GRUPO",
            "LUNES",
            "MARTES",
            "MIERCOLES",
            "JUEVES",
            "VIERNES",
            "FAC INGENIERIA",
            "FACULTAD DE INGENIERIA"
        };

        if (valoresIgnorados.Contains(normalizado))
        {
            return false;
        }

        if (valor.Contains("am", StringComparison.OrdinalIgnoreCase) ||
            valor.Contains("pm", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (valor.All(char.IsDigit))
        {
            return false;
        }

        return true;
    }

    private static Asignatura? BuscarAsignatura(
        List<Asignatura> asignaturas,
        string nombreDetectado,
        string? codigoDetectado)
    {
        if (!string.IsNullOrWhiteSpace(codigoDetectado))
        {
            Asignatura? porCodigo = asignaturas.FirstOrDefault(a =>
                string.Equals(a.Codigo.Trim(), codigoDetectado.Trim(), StringComparison.OrdinalIgnoreCase));

            if (porCodigo is not null)
            {
                return porCodigo;
            }
        }

        string nombreNormalizado = NormalizarTexto(nombreDetectado);

        return asignaturas.FirstOrDefault(a =>
            NormalizarTexto(a.Nombre) == nombreNormalizado);
    }

    private static string NormalizarTexto(string texto)
    {
        string textoSinEspaciosDobles = string.Join(' ', texto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        string textoNormalizado = textoSinEspaciosDobles.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (char caracter in textoNormalizado)
        {
            UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);

            if (categoria != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(caracter);
            }
        }

        return builder.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant();
    }
}