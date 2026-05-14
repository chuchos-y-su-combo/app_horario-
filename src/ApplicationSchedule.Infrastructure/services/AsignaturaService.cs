using System.Globalization;
using System.Text;
using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using ApplicationSchedule.Application.DTOs.Tapsi;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

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

    public AsignaturaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AsignaturaResponse>> ObtenerTodasAsync()
    {
        return await _context.Asignaturas
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

    public async Task<List<AsignaturaResponse>> ObtenerPorPlanAsync(string idPlan)
    {
        return await _context.Asignaturas
            .Where(a => a.IdPlan == idPlan)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

    public async Task<AsignaturaResponse?> ObtenerPorIdAsync(string idAsignatura)
    {
        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura);

        return asignatura is null ? null : ToResponse(asignatura);
    }

    public async Task<List<AsignaturaResponse>> ObtenerFijasTapsiAsync()
    {
        return await _context.Asignaturas
            .Where(a => a.EsFijaTapsi)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

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

    private static bool EsAsignaturaObligatoriaTapsi(string codigo, string nombre)
    {
        string codigoNormalizado = codigo.Trim().ToUpperInvariant();
        string nombreNormalizado = NormalizarTexto(nombre);

        return CodigosFijosTapsi.Contains(codigoNormalizado)
            || NombresFijosTapsi.Contains(nombreNormalizado);
    }

    private static bool EsAsignaturaOpcionalTapsiDiurna(string codigo, string nombre)
    {
        string codigoNormalizado = codigo.Trim().ToUpperInvariant();
        string nombreNormalizado = NormalizarTexto(nombre);

        return CodigosOpcionalesTapsiDiurna.Contains(codigoNormalizado)
            || NombresOpcionalesTapsiDiurna.Contains(nombreNormalizado);
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
   
    public async Task<List<AsignaturaResponse>> ObtenerOpcionalesTapsiDiurnaAsync()
    {
        return await _context.Asignaturas
            .Where(a => a.EsOpcionalTapsiDiurna)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

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