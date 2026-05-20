using System.Globalization;
using System.Text;
using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using ApplicationSchedule.Application.DTOs.Tapsi;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Servicio que gestiona las operaciones sobre la entidad <see cref="Asignatura"/>.
/// Implementa reglas de negocio específicas como detección de materias TAPSI,
/// validaciones de unicidad y transformación a DTOs de respuesta.
/// </summary>
public class AsignaturaService : IAsignaturaService
{
    private readonly AppDbContext _context;
    private const int TopeCreditosDiurna = 18;
    private const int TopeCreditosJornadaExtendida = 15;
    private const int CreditosAdicionalesTapsiDiurna = 3;

    private static readonly HashSet<string> CodigosFijosTapsi = new(StringComparer.OrdinalIgnoreCase)
    {
        "104030",
        "103007",
        "103018",
        "103004",
        "103027"
    };

    private static readonly HashSet<string> NombresFijosTapsi = new(StringComparer.OrdinalIgnoreCase)
    {
        NormalizarTexto("Cálculo diferencial"),
        NormalizarTexto("Técnicas de programación"),
        NormalizarTexto("Programación orientada a objetos"),
        NormalizarTexto("Teoría de sistemas"),
        NormalizarTexto("Sistemas operativos")
    };

    /// <summary>
    /// Crea una instancia de <see cref="AsignaturaService"/> con el contexto de datos inyectado.
    /// </summary>
    public AsignaturaService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Recupera todas las asignaturas del catálogo académico.
    /// </summary>
    /// <returns>Lista de <see cref="AsignaturaResponse"/> ordenadas por semestre y nombre.</returns>
    public async Task<List<AsignaturaResponse>> ObtenerTodasAsync()
    {
        return await _context.Asignaturas
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene las asignaturas asociadas a un plan de estudio.
    /// </summary>
    /// <param name="idPlan">Identificador del plan de estudio.</param>
    /// <returns>Lista de asignaturas del plan.</returns>
    public async Task<List<AsignaturaResponse>> ObtenerPorPlanAsync(string idPlan)
    {
        return await _context.Asignaturas
            .Where(a => a.IdPlan == idPlan)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene una asignatura por su identificador.
    /// </summary>
    /// <param name="idAsignatura">Identificador de la asignatura.</param>
    /// <returns>DTO de la asignatura o null si no existe.</returns>
    public async Task<AsignaturaResponse?> ObtenerPorIdAsync(string idAsignatura)
    {
        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura);

        return asignatura is null ? null : ToResponse(asignatura);
    }

    /// <summary>
    /// Recupera las asignaturas marcadas como fijas para TAPSI.
    /// </summary>
    /// <returns>Listado de asignaturas fijas TAPSI.</returns>
    public async Task<List<AsignaturaResponse>> ObtenerFijasTapsiAsync()
    {
        return await _context.Asignaturas
            .Where(a => a.EsFijaTapsi)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

    /// <summary>
    /// Crea una nueva asignatura aplicando validaciones de plan y código único.
    /// Detecta automáticamente si la asignatura corresponde a reglas TAPSI.
    /// </summary>
    /// <param name="request">DTO con los datos de la asignatura.</param>
    /// <returns>DTO de la asignatura creada.</returns>
    /// <exception cref="InvalidOperationException">Si el plan no existe o el código ya está en uso.</exception>
    public async Task<AsignaturaResponse> CrearAsync(CrearAsignaturaRequest request)
    {
        string idPlan = request.IdPlan.Trim();
        string codigoNormalizado = request.Codigo.Trim().ToUpperInvariant();
        string nombreLimpio = request.Nombre.Trim();

        bool planExiste = await _context.PlanesEstudio
            .AnyAsync(p => p.IdPlan == idPlan);

        if (!planExiste)
        {
            throw new InvalidOperationException("El plan de estudios seleccionado no existe.");
        }

        bool codigoExiste = await _context.Asignaturas
            .AnyAsync(a => a.Codigo == codigoNormalizado);

        if (codigoExiste)
        {
            throw new InvalidOperationException("Ya existe una asignatura registrada con ese código.");
        }

        var asignatura = new Asignatura
        {
            IdAsignatura = Guid.NewGuid().ToString(),
            IdPlan = idPlan,
            Codigo = codigoNormalizado,
            Nombre = nombreLimpio,
            Creditos = request.Creditos,
            Semestre = request.Semestre,
            MinEstudiantes = request.MinEstudiantes,
            EsFijaTapsi = request.EsFijaTapsi || EsAsignaturaObligatoriaTapsi(codigoNormalizado, nombreLimpio),
            EsOpcionalTapsiDiurna = request.EsOpcionalTapsiDiurna || EsAsignaturaOpcionalTapsiDiurna(codigoNormalizado, nombreLimpio)
        };

        _context.Asignaturas.Add(asignatura);
        await _context.SaveChangesAsync();

        return ToResponse(asignatura);
    }

    /// <summary>
    /// Actualiza una asignatura tras validar plan y unicidad de código.
    /// </summary>
    /// <param name="idAsignatura">Identificador de la asignatura a actualizar.</param>
    /// <param name="request">DTO con los campos a actualizar.</param>
    /// <returns>True si se actualizó; false si no se encontró la asignatura.</returns>
    public async Task<bool> ActualizarAsync(string idAsignatura, ActualizarAsignaturaRequest request)
    {
        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura);

        if (asignatura is null)
        {
            return false;
        }

        string idPlan = request.IdPlan.Trim();
        string codigoNormalizado = request.Codigo.Trim().ToUpperInvariant();
        string nombreLimpio = request.Nombre.Trim();

        bool planExiste = await _context.PlanesEstudio
            .AnyAsync(p => p.IdPlan == idPlan);

        if (!planExiste)
        {
            throw new InvalidOperationException("El plan de estudios seleccionado no existe.");
        }

        bool codigoUsadoPorOtra = await _context.Asignaturas
            .AnyAsync(a => a.Codigo == codigoNormalizado && a.IdAsignatura != idAsignatura);

        if (codigoUsadoPorOtra)
        {
            throw new InvalidOperationException("El código ya está siendo usado por otra asignatura.");
        }

        asignatura.IdPlan = idPlan;
        asignatura.Codigo = codigoNormalizado;
        asignatura.Nombre = nombreLimpio;
        asignatura.Creditos = request.Creditos;
        asignatura.Semestre = request.Semestre;
        asignatura.MinEstudiantes = request.MinEstudiantes;
        asignatura.EsFijaTapsi = request.EsFijaTapsi || EsAsignaturaObligatoriaTapsi(codigoNormalizado, nombreLimpio);
        asignatura.EsOpcionalTapsiDiurna = request.EsOpcionalTapsiDiurna || EsAsignaturaOpcionalTapsiDiurna(codigoNormalizado, nombreLimpio);

        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Marca las asignaturas detectadas como obligatorias TAPSI como fijas en el catálogo.
    /// </summary>
    /// <returns>Cantidad de asignaturas marcadas.</returns>
    public async Task<int> MarcarObligatoriasTapsiComoFijasAsync()
    {
        List<Asignatura> asignaturas = await _context.Asignaturas.ToListAsync();

        List<Asignatura> obligatoriasTapsi = asignaturas
            .Where(a => EsAsignaturaObligatoriaTapsi(a.Codigo, a.Nombre))
            .ToList();

        foreach (Asignatura asignatura in obligatoriasTapsi)
        {
            asignatura.EsFijaTapsi = true;
        }

        await _context.SaveChangesAsync();

        return obligatoriasTapsi.Count;
    }

    /// <summary>
    /// Elimina una asignatura si no está marcada como fija TAPSI.
    /// </summary>
    /// <param name="idAsignatura">Identificador de la asignatura a eliminar.</param>
    /// <returns>True si se eliminó; false si no existe.</returns>
    /// <exception cref="InvalidOperationException">Si la asignatura está marcada como fija TAPSI.</exception>
    public async Task<bool> EliminarAsync(string idAsignatura)
    {
        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura);

        if (asignatura is null)
        {
            return false;
        }

        if (asignatura.EsFijaTapsi)
        {
            throw new InvalidOperationException("No se puede eliminar una asignatura obligatoria TAPSI marcada como fija.");
        }

        _context.Asignaturas.Remove(asignatura);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Determina si una asignatura corresponde a la lista de obligatorias TAPSI por código o nombre.
    /// </summary>
    private static bool EsAsignaturaObligatoriaTapsi(string codigo, string nombre)
    {
        string codigoNormalizado = codigo.Trim().ToUpperInvariant();
        string nombreNormalizado = NormalizarTexto(nombre);

        return CodigosFijosTapsi.Contains(codigoNormalizado)
            || NombresFijosTapsi.Contains(nombreNormalizado);
    }

    /// <summary>
    /// Determina si una asignatura es una opción adicional TAPSI para jornada diurna.
    /// </summary>
    private static bool EsAsignaturaOpcionalTapsiDiurna(string codigo, string nombre)
    {
        string codigoNormalizado = codigo.Trim().ToUpperInvariant();
        string nombreNormalizado = NormalizarTexto(nombre);

        return CodigosOpcionalesTapsiDiurna.Contains(codigoNormalizado)
            || NombresOpcionalesTapsiDiurna.Contains(nombreNormalizado);
    }
    /// <summary>
    /// Normaliza texto para comparaciones insensibles a acentos y mayúsculas.
    /// </summary>
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

        return builder.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
    }

    private static readonly HashSet<string> CodigosOpcionalesTapsiDiurna = new(StringComparer.OrdinalIgnoreCase)
    {
        "103093",
        "103126",
        "109183"
    };

    private static readonly HashSet<string> NombresOpcionalesTapsiDiurna = new(StringComparer.OrdinalIgnoreCase)
    {
        NormalizarTexto("Ingeniería de Software II"),
        NormalizarTexto("Ingenieria de Software II"),
        NormalizarTexto("Redes LAN"),
        NormalizarTexto("Programación Back End"),
        NormalizarTexto("Programación Backend")
    };
   
    /// <summary>
    /// Recupera las asignaturas marcadas como opcionales TAPSI para jornada diurna.
    /// </summary>
    /// <returns>Listado de asignaturas opcionales TAPSI diurna.</returns>
    public async Task<List<AsignaturaResponse>> ObtenerOpcionalesTapsiDiurnaAsync()
    {
        return await _context.Asignaturas
            .Where(a => a.EsOpcionalTapsiDiurna)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

    /// <summary>
    /// Construye y devuelve el plan TAPSI para la jornada diurna con reglas y límites de créditos.
    /// </summary>
    /// <returns>Objeto con el resumen del plan TAPSI diurno.</returns>
    public async Task<TapsiDiurnaPlanResponse> ObtenerPlanTapsiDiurnaAsync()
    {
        List<AsignaturaResponse> fijas = await ObtenerFijasTapsiAsync();
        List<AsignaturaResponse> opciones = await ObtenerOpcionalesTapsiDiurnaAsync();

        int creditosFijos = fijas.Sum(a => a.Creditos);
        int creditosTotales = creditosFijos + CreditosAdicionalesTapsiDiurna;

        return new TapsiDiurnaPlanResponse
        {
            Jornada = "Diurna",
            TopeCreditosDiurna = TopeCreditosDiurna,
            TopeCreditosJornadaExtendida = TopeCreditosJornadaExtendida,
            CreditosFijosTapsi = creditosFijos,
            CreditosAdicionalesRequeridos = CreditosAdicionalesTapsiDiurna,
            CreditosTotalesRequeridosDiurna = creditosTotales,
            Regla = "Para TAPSI jornada diurna se deben tomar las 5 materias fijas sin cruce y adicionar exactamente una opción entre Ingeniería de Software II, Redes LAN o Programación Back End.",
            AsignaturasFijas = fijas,
            OpcionesAdicionalesDiurna = opciones
        };
    }

    /// <summary>
    /// Marca en el catálogo las asignaturas detectadas como opcionales TAPSI diurna.
    /// </summary>
    /// <returns>Cantidad de asignaturas marcadas.</returns>
    public async Task<int> MarcarOpcionalesTapsiDiurnaAsync()
    {
        List<Asignatura> asignaturas = await _context.Asignaturas.ToListAsync();

        List<Asignatura> opcionalesTapsiDiurna = asignaturas
            .Where(a => EsAsignaturaOpcionalTapsiDiurna(a.Codigo, a.Nombre))
            .ToList();

        foreach (Asignatura asignatura in opcionalesTapsiDiurna)
        {
            asignatura.EsOpcionalTapsiDiurna = true;
        }

        await _context.SaveChangesAsync();

        return opcionalesTapsiDiurna.Count;
    }

    /// <summary>
    /// Mapea la entidad <see cref="Asignatura"/> a su DTO de respuesta.
    /// </summary>
     private static AsignaturaResponse ToResponse(Asignatura a) => new()
    {
        IdAsignatura = a.IdAsignatura,
        IdPlan = a.IdPlan,
        Codigo = a.Codigo,
        Nombre = a.Nombre,
        Creditos = a.Creditos,
        Semestre = a.Semestre,
        MinEstudiantes = a.MinEstudiantes,
        EsFijaTapsi = a.EsFijaTapsi,
        EsOpcionalTapsiDiurna = a.EsOpcionalTapsiDiurna
    };

}