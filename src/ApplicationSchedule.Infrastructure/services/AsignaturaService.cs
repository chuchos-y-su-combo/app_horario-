using System.Globalization;
using System.Text;
using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class AsignaturaService : IAsignaturaService
{
    private readonly AppDbContext _context;

    private static readonly HashSet<string> CodigosFijosTapsi = new(StringComparer.OrdinalIgnoreCase)
    {
        "104030", // Cálculo Diferencial
        "103007", // Técnicas de Programación
        "103018", // Programación Orientada a Objetos
        "103004", // Teoría de Sistemas
        "103027"  // Sistemas Operativos
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

    public async Task<List<AsignaturaResponse>> ObtenerFijasTapsiAsync()
    {
        return await _context.Asignaturas
            .Where(a => a.EsFijaTapsi)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
    }

    public async Task<List<AsignaturaResponse>> ObtenerPorPlanEstudiosAsync(int idPlanEstudios)
    {
        return await _context.Asignaturas
            .Where(a => a.IdPlanEstudios == idPlanEstudios)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
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

    public async Task<AsignaturaResponse?> ObtenerPorIdAsync(int idAsignatura)
    {
        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura);

        return asignatura is null ? null : ToResponse(asignatura);
    }

    public async Task<AsignaturaResponse> CrearAsync(CrearAsignaturaRequest request)
    {
        string codigoNormalizado = request.Codigo.Trim().ToUpper();
        string nombreLimpio = request.Nombre.Trim();

        bool codigoExiste = await _context.Asignaturas
            .AnyAsync(a => a.Codigo == codigoNormalizado);

        if (codigoExiste)
        {
            throw new InvalidOperationException("Ya existe una asignatura registrada con ese código.");
        }

        var asignatura = new Asignatura
        {
            IdPlanEstudios = request.IdPlanEstudios,
            Codigo = codigoNormalizado,
            Nombre = nombreLimpio,
            Creditos = request.Creditos,
            Semestre = request.Semestre,
            EsFijaTapsi = request.EsFijaTapsi || EsAsignaturaObligatoriaTapsi(codigoNormalizado, nombreLimpio)
        };

        _context.Asignaturas.Add(asignatura);
        await _context.SaveChangesAsync();

        return ToResponse(asignatura);
    }

    public async Task<bool> ActualizarAsync(int idAsignatura, ActualizarAsignaturaRequest request)
    {
        Asignatura? asignatura = await _context.Asignaturas
            .FirstOrDefaultAsync(a => a.IdAsignatura == idAsignatura);

        if (asignatura is null)
        {
            return false;
        }

        string codigoNormalizado = request.Codigo.Trim().ToUpper();
        string nombreLimpio = request.Nombre.Trim();

        bool codigoUsadoPorOtra = await _context.Asignaturas
            .AnyAsync(a => a.Codigo == codigoNormalizado && a.IdAsignatura != idAsignatura);

        if (codigoUsadoPorOtra)
        {
            throw new InvalidOperationException("El código ya está siendo usado por otra asignatura.");
        }

        asignatura.IdPlanEstudios = request.IdPlanEstudios;
        asignatura.Codigo = codigoNormalizado;
        asignatura.Nombre = nombreLimpio;
        asignatura.Creditos = request.Creditos;
        asignatura.Semestre = request.Semestre;
        asignatura.EsFijaTapsi = request.EsFijaTapsi || EsAsignaturaObligatoriaTapsi(codigoNormalizado, nombreLimpio);

        await _context.SaveChangesAsync();

        return true;
    }

   public async Task<bool> EliminarAsync(int idAsignatura)
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
        string codigoNormalizado = codigo.Trim().ToUpper();
        string nombreNormalizado = NormalizarTexto(nombre);

        return CodigosFijosTapsi.Contains(codigoNormalizado)
            || NombresFijosTapsi.Contains(nombreNormalizado);
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

    private static AsignaturaResponse ToResponse(Asignatura a) => new()
    {
        IdAsignatura = a.IdAsignatura,
        IdPlanEstudios = a.IdPlanEstudios,
        Codigo = a.Codigo,
        Nombre = a.Nombre,
        Creditos = a.Creditos,
        Semestre = a.Semestre,
        EsFijaTapsi = a.EsFijaTapsi
    };
}
