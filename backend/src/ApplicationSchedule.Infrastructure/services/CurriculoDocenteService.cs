using System.Globalization;
using System.Text;
using ApplicationSchedule.Application.DTOs.Curriculos;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ApplicationSchedule.Application.DTOs.Disponibilidades;
using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Servicio encargado de leer archivos Excel de currículos/horarios docentes
/// y determinar automáticamente las asignaturas que cada docente puede dictar.
/// </summary>
public class CurriculoDocenteService : ICurriculoDocenteService
{
    private const string HojaDisponibilidad = "Disponibilidad";
    private const string HojaMiDisponibilidad = "Mi Disponibilidad";
    private const string FuenteExcel = "Excel";

    private readonly AppDbContext _context;

    /// <summary>
    /// Crea una instancia de <see cref="CurriculoDocenteService"/> con el contexto proporcionado.
    /// </summary>
    public CurriculoDocenteService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Importa currículos desde un archivo Excel, procesando hojas de disponibilidad
    /// y hojas por docente para habilitar asignaturas y disponibilidades.
    /// </summary>
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
        List<PlanEstudio> planes = await _context.PlanesEstudio.ToListAsync(cancellationToken);

        var response = new ImportarCurriculoResponse
        {
            Archivo = nombreArchivo
        };

        using var workbook = new XLWorkbook(archivo);

        IXLWorksheet? hojaDisponibilidad = workbook.Worksheets
            .FirstOrDefault(w => string.Equals(w.Name.Trim(), HojaDisponibilidad, StringComparison.OrdinalIgnoreCase));

        if (hojaDisponibilidad is not null)
        {
            List<DisponibilidadImportadaResponse> disponibilidades =
                await ProcesarHojaDisponibilidadAsync(hojaDisponibilidad, docentes, cancellationToken);

            response.Disponibilidades.AddRange(disponibilidades);
            response.TotalDisponibilidadesCreadas = disponibilidades.Sum(d => d.RegistrosCreados);
        }

        IXLWorksheet? hojaMiDisponibilidad = workbook.Worksheets
            .FirstOrDefault(w => string.Equals(w.Name.Trim(), HojaMiDisponibilidad, StringComparison.OrdinalIgnoreCase));

        if (hojaMiDisponibilidad is not null)
        {
            (DisponibilidadImportadaResponse dispo, CurriculoDocenteImportadoResponse curriculo) =
                await ProcesarHojaMiDisponibilidadAsync(
                    hojaMiDisponibilidad, docentes, asignaturas, planes, cancellationToken);

            response.Disponibilidades.Add(dispo);
            response.TotalDisponibilidadesCreadas += dispo.RegistrosCreados;
            response.Detalle.Add(curriculo);
            response.TotalHojasProcesadas++;

            if (curriculo.DocenteEncontrado)
                response.TotalDocentesEncontrados++;
            else
                response.DocentesNoEncontrados.Add(curriculo.NombreDocenteDetectado);

            response.TotalRelacionesCreadas += curriculo.Asignaturas.Count(a => a.Estado == "Creada");
            response.TotalRelacionesExistentes += curriculo.Asignaturas.Count(a => a.Estado == "Existente");
            response.TotalAsignaturasNoEncontradas += curriculo.Asignaturas.Count(
                a => a.Estado == "Asignatura no encontrada" || a.Estado == "Sin plan compatible");
        }

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

    /// <summary>
    /// Devuelve las asignaturas para las que el docente está habilitado.
    /// </summary>
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

    /// <summary>
    /// Devuelve la disponibilidad horaria del docente como una lista ordenada de bloques.
    /// </summary>
    public async Task<List<DisponibilidadDocenteResponse>> ObtenerDisponibilidadDocenteAsync(
        string idDocente,
        CancellationToken cancellationToken = default)
    {
        bool docenteExiste = await _context.Docentes
            .AnyAsync(d => d.IdDocente == idDocente, cancellationToken);

        if (!docenteExiste)
        {
            throw new InvalidOperationException("Docente no encontrado.");
        }

        return await _context.Disponibilidades
            .Where(d => d.IdDocente == idDocente)
            .OrderBy(d => d.DiaSemana)
            .ThenBy(d => d.HoraInicio)
            .Select(d => new DisponibilidadDocenteResponse
            {
                IdDisponibilidad = d.IdDisponibilidad,
                IdDocente = d.IdDocente,
                DiaSemana = d.DiaSemana,
                DiaNombre = ObtenerNombreDia(d.DiaSemana),
                HoraInicio = d.HoraInicio,
                HoraFin = d.HoraFin
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<DisponibilidadImportadaResponse>> ProcesarHojaDisponibilidadAsync(
        IXLWorksheet worksheet,
        List<Docente> docentes,
        CancellationToken cancellationToken)
    {
        List<DisponibilidadExcelItem> items = DisponibilidadExcelParser.LeerHoja(worksheet);

        var resultado = new List<DisponibilidadImportadaResponse>();

        foreach (DisponibilidadExcelItem item in items)
        {
            Docente? docente = docentes.FirstOrDefault(d =>
                NormalizarTexto(d.Nombre) == NormalizarTexto(item.NombreDocente)
                || NormalizarTexto(d.Nombre).Contains(NormalizarTexto(item.NombreDocente)));

            var detalle = new DisponibilidadImportadaResponse
            {
                NombreDocenteDetectado = item.NombreDocente,
                IdDocente = docente?.IdDocente,
                DocenteEncontrado = docente is not null,
                TextoOriginal = item.TextoOriginal,
                Mensajes = item.Mensajes
            };

            if (docente is null)
            {
                detalle.Mensajes.Add("No existe un docente registrado con ese nombre.");
                resultado.Add(detalle);
                continue;
            }

            if (item.Bloques.Count == 0)
            {
                detalle.Mensajes.Add("No se encontraron bloques de disponibilidad válidos.");
                resultado.Add(detalle);
                continue;
            }

            List<Disponibilidad> disponibilidadesAnteriores = await _context.Disponibilidades
                .Where(d => d.IdDocente == docente.IdDocente)
                .ToListAsync(cancellationToken);

            _context.Disponibilidades.RemoveRange(disponibilidadesAnteriores);

            foreach (DisponibilidadBloque bloque in item.Bloques)
            {
                var disponibilidad = new Disponibilidad
                {
                    IdDisponibilidad = Guid.NewGuid().ToString(),
                    IdDocente = docente.IdDocente,
                    DiaSemana = bloque.DiaSemana,
                    HoraInicio = bloque.HoraInicio,
                    HoraFin = bloque.HoraFin
                };

                _context.Disponibilidades.Add(disponibilidad);

                detalle.Disponibilidades.Add(new DisponibilidadDocenteResponse
                {
                    IdDisponibilidad = disponibilidad.IdDisponibilidad,
                    IdDocente = disponibilidad.IdDocente,
                    DiaSemana = disponibilidad.DiaSemana,
                    DiaNombre = ObtenerNombreDia(disponibilidad.DiaSemana),
                    HoraInicio = disponibilidad.HoraInicio,
                    HoraFin = disponibilidad.HoraFin
                });
            }

            detalle.RegistrosCreados = detalle.Disponibilidades.Count;

            resultado.Add(detalle);
        }

        return resultado;
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

    /// <summary>
    /// Procesa la hoja vertical "Mi Disponibilidad": crea o actualiza el docente,
    /// habilita materias por nombre parcial según jornada y registra disponibilidad.
    /// </summary>
    private async Task<(DisponibilidadImportadaResponse Disponibilidad, CurriculoDocenteImportadoResponse Curriculo)>
        ProcesarHojaMiDisponibilidadAsync(
            IXLWorksheet worksheet,
            List<Docente> docentes,
            List<Asignatura> asignaturas,
            List<PlanEstudio> planes,
            CancellationToken cancellationToken)
    {
        MiDisponibilidadExcelItem item = DisponibilidadExcelParser.LeerHojaMiDisponibilidad(worksheet);

        var detalleDispo = new DisponibilidadImportadaResponse
        {
            NombreDocenteDetectado = item.NombreDocente,
            TextoOriginal = item.TextoDisponibilidad,
            Mensajes = item.Mensajes
        };

        var detalleCurriculo = new CurriculoDocenteImportadoResponse
        {
            Hoja = worksheet.Name,
            NombreDocenteDetectado = item.NombreDocente
        };

        if (string.IsNullOrWhiteSpace(item.NombreDocente))
        {
            detalleDispo.Mensajes.Add("No se encontró nombre de docente en la celda B6.");
            return (detalleDispo, detalleCurriculo);
        }

        // 1. Crear o actualizar docente
        int maxAsignaturas = item.TipoContrato == "TC" ? 5 : 3;
        string nombreNorm = NormalizarTexto(item.NombreDocente);

        Docente? docente = docentes.FirstOrDefault(d =>
        {
            string dNorm = NormalizarTexto(d.Nombre);
            return dNorm == nombreNorm
                || dNorm.Contains(nombreNorm)
                || nombreNorm.Contains(dNorm);
        });

        if (docente is null)
        {
            docente = new Docente
            {
                IdDocente = Guid.NewGuid().ToString(),
                Identificacion = string.Empty,
                Nombre = item.NombreDocente,
                TipoContrato = item.TipoContrato,
                MaxAsignaturas = maxAsignaturas
            };
            _context.Docentes.Add(docente);
            docentes.Add(docente);
            detalleDispo.Mensajes.Add(
                $"Docente creado: {item.NombreDocente} ({item.TipoContrato}, máx. {maxAsignaturas} asignaturas).");
        }
        else
        {
            docente.TipoContrato = item.TipoContrato;
            docente.MaxAsignaturas = maxAsignaturas;
            detalleDispo.Mensajes.Add(
                $"Docente actualizado: TipoContrato={item.TipoContrato}, MaxAsignaturas={maxAsignaturas}.");
        }

        detalleDispo.IdDocente = docente.IdDocente;
        detalleDispo.DocenteEncontrado = true;
        detalleCurriculo.IdDocente = docente.IdDocente;
        detalleCurriculo.DocenteEncontrado = true;

        // 2. Determinar jornada a partir de los bloques parseados
        (bool tieneDiurno, bool tieneNocturno) = DeterminarJornada(item.Bloques);

        // 3. Matching de materias y creación de DocenteHabilitado
        detalleCurriculo.TotalAsignaturasDetectadas = item.NombresMaterias.Count;
        var clavesProcesadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string nombreMateria in item.NombresMaterias)
        {
            List<Asignatura> candidatas = BuscarTodasAsignaturasPorNombre(asignaturas, nombreMateria);

            List<Asignatura> filtradas = candidatas.Where(a =>
            {
                PlanEstudio? plan = planes.FirstOrDefault(p => p.IdPlan == a.IdPlan);
                if (plan is null) return true;
                bool esDiurno = plan.Jornada.Equals("Diurna", StringComparison.OrdinalIgnoreCase);
                return esDiurno ? tieneDiurno : tieneNocturno;
            }).ToList();

            if (filtradas.Count == 0)
            {
                detalleCurriculo.Asignaturas.Add(new CurriculoAsignaturaDetectadaResponse
                {
                    Hoja = worksheet.Name,
                    NombreDetectado = nombreMateria,
                    FueHabilitada = false,
                    Estado = candidatas.Count > 0 ? "Sin plan compatible" : "Asignatura no encontrada",
                    Mensaje = candidatas.Count > 0
                        ? "La asignatura existe pero no hay plan compatible con la jornada del docente."
                        : "No existe una asignatura registrada con ese nombre."
                });
                continue;
            }

            foreach (Asignatura asignatura in filtradas)
            {
                string clave = $"{docente.IdDocente}|{asignatura.IdAsignatura}";
                if (!clavesProcesadas.Add(clave))
                    continue;

                bool relacionExiste = await _context.DocentesHabilitados
                    .AnyAsync(dh =>
                        dh.IdDocente == docente.IdDocente &&
                        dh.IdAsignatura == asignatura.IdAsignatura,
                        cancellationToken);

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

                detalleCurriculo.Asignaturas.Add(new CurriculoAsignaturaDetectadaResponse
                {
                    Hoja = worksheet.Name,
                    NombreDetectado = nombreMateria,
                    IdAsignatura = asignatura.IdAsignatura,
                    NombreAsignaturaBaseDatos = asignatura.Nombre,
                    FueHabilitada = true,
                    Estado = relacionExiste ? "Existente" : "Creada",
                    Mensaje = relacionExiste
                        ? "El docente ya estaba habilitado para esta asignatura."
                        : "Asignatura habilitada correctamente para el docente."
                });
            }
        }

        detalleCurriculo.TotalAsignaturasHabilitadas = detalleCurriculo.Asignaturas.Count(a => a.FueHabilitada);

        // 4. Reemplazar disponibilidad
        if (item.Bloques.Count > 0)
        {
            List<Disponibilidad> anteriores = await _context.Disponibilidades
                .Where(d => d.IdDocente == docente.IdDocente)
                .ToListAsync(cancellationToken);

            _context.Disponibilidades.RemoveRange(anteriores);

            foreach (DisponibilidadBloque bloque in item.Bloques)
            {
                var disponibilidad = new Disponibilidad
                {
                    IdDisponibilidad = Guid.NewGuid().ToString(),
                    IdDocente = docente.IdDocente,
                    DiaSemana = bloque.DiaSemana,
                    HoraInicio = bloque.HoraInicio,
                    HoraFin = bloque.HoraFin
                };

                _context.Disponibilidades.Add(disponibilidad);

                detalleDispo.Disponibilidades.Add(new DisponibilidadDocenteResponse
                {
                    IdDisponibilidad = disponibilidad.IdDisponibilidad,
                    IdDocente = disponibilidad.IdDocente,
                    DiaSemana = disponibilidad.DiaSemana,
                    DiaNombre = ObtenerNombreDia(disponibilidad.DiaSemana),
                    HoraInicio = disponibilidad.HoraInicio,
                    HoraFin = disponibilidad.HoraFin
                });
            }

            detalleDispo.RegistrosCreados = item.Bloques.Count;
        }
        else
        {
            detalleDispo.Mensajes.Add(
                "No se pudieron parsear bloques de disponibilidad del texto. Se requiere revisión manual.");
        }

        return (detalleDispo, detalleCurriculo);
    }

    /// <summary>
    /// Determina si los bloques de disponibilidad cubren jornada diurna, nocturna o ambas.
    /// Diurno: HoraInicio antes de las 18:00. Nocturno: HoraFin después de las 18:00 o inicio desde las 18:00.
    /// Sin bloques → habilita en todos los planes por defecto.
    /// </summary>
    private static (bool TieneDiurno, bool TieneNocturno) DeterminarJornada(
        List<DisponibilidadBloque> bloques)
    {
        if (bloques.Count == 0)
            return (true, true);

        var limite = TimeSpan.FromHours(18);

        bool tieneDiurno = bloques.Any(b =>
            TimeSpan.TryParse(b.HoraInicio, out TimeSpan hi) && hi < limite);

        bool tieneNocturno = bloques.Any(b =>
            (TimeSpan.TryParse(b.HoraFin, out TimeSpan hf) && hf > limite) ||
            (TimeSpan.TryParse(b.HoraInicio, out TimeSpan hi2) && hi2 >= limite));

        if (!tieneDiurno && !tieneNocturno)
            return (true, true);

        return (tieneDiurno, tieneNocturno);
    }

    /// <summary>
    /// Busca todas las instancias de una asignatura por nombre usando coincidencia parcial
    /// e ignorando mayúsculas y tildes. Retorna todas las coincidencias (una por plan).
    /// </summary>
    private static List<Asignatura> BuscarTodasAsignaturasPorNombre(
        List<Asignatura> asignaturas,
        string nombreDetectado)
    {
        string nombreNorm = NormalizarTexto(nombreDetectado);

        if (string.IsNullOrWhiteSpace(nombreNorm))
            return new List<Asignatura>();

        return asignaturas.Where(a =>
        {
            string bdNorm = NormalizarTexto(a.Nombre);
            return bdNorm == nombreNorm
                || bdNorm.Contains(nombreNorm)
                || nombreNorm.Contains(bdNorm);
        }).ToList();
    }

    private static bool EsHojaIgnorada(string nombreHoja)
    {
        string nombre = nombreHoja.Trim();
        return string.Equals(nombre, HojaDisponibilidad, StringComparison.OrdinalIgnoreCase)
            || string.Equals(nombre, HojaMiDisponibilidad, StringComparison.OrdinalIgnoreCase);
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

    /// <summary>
    /// Habilita manualmente una asignatura para un docente, registrando la fuente como "Manual".
    /// Lanza excepción si el docente o la asignatura no existen, o si la relación ya existe.
    /// </summary>
    public async Task<AsignaturaHabilitadaDocenteResponse> HabilitarAsignaturaAsync(
        string idDocente,
        string idAsignatura,
        CancellationToken cancellationToken = default)
    {
        bool docenteExiste = await _context.Docentes
            .AnyAsync(d => d.IdDocente == idDocente, cancellationToken);

        if (!docenteExiste)
            throw new InvalidOperationException("Docente no encontrado.");

        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura, cancellationToken);

        if (asignatura is null)
            throw new InvalidOperationException("Asignatura no encontrada.");

        bool relacionExiste = await _context.DocentesHabilitados
            .AnyAsync(dh =>
                dh.IdDocente == idDocente &&
                dh.IdAsignatura == idAsignatura,
                cancellationToken);

        if (relacionExiste)
            throw new InvalidOperationException("El docente ya está habilitado para esta asignatura.");

        var habilitado = new DocenteHabilitado
        {
            IdDocente = idDocente,
            IdAsignatura = idAsignatura,
            FechaHabilitacion = DateTime.UtcNow,
            Fuente = "Manual"
        };

        _context.DocentesHabilitados.Add(habilitado);
        await _context.SaveChangesAsync(cancellationToken);

        return new AsignaturaHabilitadaDocenteResponse
        {
            IdAsignatura = asignatura.IdAsignatura,
            Codigo = asignatura.Codigo,
            Nombre = asignatura.Nombre,
            Creditos = asignatura.Creditos,
            Semestre = asignatura.Semestre,
            Fuente = habilitado.Fuente,
            FechaHabilitacion = habilitado.FechaHabilitacion
        };
    }

    /// <summary>
    /// Elimina la habilitación de una asignatura para un docente.
    /// Lanza excepción si la relación no existe.
    /// </summary>
    public async Task DesvincularAsignaturaAsync(
        string idDocente,
        string idAsignatura,
        CancellationToken cancellationToken = default)
    {
        DocenteHabilitado? habilitado = await _context.DocentesHabilitados
            .FirstOrDefaultAsync(dh =>
                dh.IdDocente == idDocente &&
                dh.IdAsignatura == idAsignatura,
                cancellationToken);

        if (habilitado is null)
            throw new InvalidOperationException("La relación docente-asignatura no existe.");

        _context.DocentesHabilitados.Remove(habilitado);
        await _context.SaveChangesAsync(cancellationToken);
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
    private static string ObtenerNombreDia(int diaSemana)
    {
        return diaSemana switch
        {
            1 => "Lunes",
            2 => "Martes",
            3 => "Miércoles",
            4 => "Jueves",
            5 => "Viernes",
            6 => "Sábado",
            _ => "Desconocido"
        };
    }
    public async Task<ReduccionDisponibilidadResponse> ReducirDisponibilidadPorDobleJornadaAsync(
    string idDocente,
    string idAsignatura,
    string periodo,
    CancellationToken cancellationToken = default)
    {
        string periodoLimpio = periodo.Trim();

        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idDocente, cancellationToken);

        if (docente is null)
            throw new InvalidOperationException("Docente no encontrado.");

        Asignatura? asignatura = await _context.Asignaturas
            .Include(a => a.PlanEstudio)
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura, cancellationToken);

        if (asignatura is null)
            throw new InvalidOperationException("Asignatura no encontrada.");

        // Buscar asignaciones de este docente con esta asignatura en el periodo,
        // separadas por jornada según el escenario
        List<Asignacion> asignacionesDiurnas = await _context.Asignaciones
            .Where(a =>
                a.IdDocente == idDocente &&
                a.IdAsignatura == idAsignatura &&
                a.Periodo == periodoLimpio &&
                (a.Escenario == EscenarioGeneracion.IngDiurna ||
                 a.Escenario == EscenarioGeneracion.TapsiDiurna) &&
                a.HoraInicio != string.Empty)
            .ToListAsync(cancellationToken);

        List<Asignacion> asignacionesNocturnas = await _context.Asignaciones
            .Where(a =>
                a.IdDocente == idDocente &&
                a.IdAsignatura == idAsignatura &&
                a.Periodo == periodoLimpio &&
                (a.Escenario == EscenarioGeneracion.IngNocturna ||
                 a.Escenario == EscenarioGeneracion.TapsiNocturna))
            .ToListAsync(cancellationToken);

        // Si no dicta en ambas jornadas, no hay nada que reducir
        if (asignacionesDiurnas.Count == 0 || asignacionesNocturnas.Count == 0)
        {
            return new ReduccionDisponibilidadResponse
            {
                IdDocente = idDocente,
                NombreDocente = docente.Nombre,
                NombreAsignatura = asignatura.Nombre,
                Periodo = periodoLimpio,
                BloquesEliminados = 0,
                Mensaje = "El docente no dicta esta asignatura en ambas jornadas. No se realizó ninguna reducción."
            };
        }

        // Obtener disponibilidad actual del docente
        List<Disponibilidad> disponibilidades = await _context.Disponibilidades
            .Where(d => d.IdDocente == idDocente)
            .ToListAsync(cancellationToken);

        // Eliminar los bloques de disponibilidad que se solapan con alguna asignación diurna
        List<Disponibilidad> bloquesAEliminar = new();

        foreach (Disponibilidad bloque in disponibilidades)
        {
            bool seSolapa = asignacionesDiurnas.Any(a =>
                a.Dia == bloque.DiaSemana &&
                HorariosSeSolapan(a.HoraInicio, a.HoraFin, bloque.HoraInicio, bloque.HoraFin));

            if (seSolapa)
                bloquesAEliminar.Add(bloque);
        }

        List<string> bloquesAfectados = bloquesAEliminar
            .Select(b => $"Día {b.DiaSemana} {b.HoraInicio}-{b.HoraFin}")
            .ToList();

        if (bloquesAEliminar.Count > 0)
        {
            _context.Disponibilidades.RemoveRange(bloquesAEliminar);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new ReduccionDisponibilidadResponse
        {
            IdDocente = idDocente,
            NombreDocente = docente.Nombre,
            NombreAsignatura = asignatura.Nombre,
            Periodo = periodoLimpio,
            BloquesEliminados = bloquesAEliminar.Count,
            BloquesAfectados = bloquesAfectados,
            Mensaje = bloquesAEliminar.Count > 0
                ? $"Se eliminaron {bloquesAEliminar.Count} bloque(s) de disponibilidad por doble jornada."
                : "No se encontraron bloques de disponibilidad que se solapen con la jornada diurna."
        };
    }

    private static bool HorariosSeSolapan(
        string inicioA, string finA,
        string inicioB, string finB)
    {
        if (string.IsNullOrEmpty(inicioA) || string.IsNullOrEmpty(finA))
            return false;

        TimeSpan a1 = TimeSpan.Parse(inicioA);
        TimeSpan a2 = TimeSpan.Parse(finA);
        TimeSpan b1 = TimeSpan.Parse(inicioB);
        TimeSpan b2 = TimeSpan.Parse(finB);

        return a1 < b2 && b1 < a2;
    }
}