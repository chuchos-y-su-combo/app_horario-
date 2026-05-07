using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class AsignaturaService : IAsignaturaService
{
    private readonly AppDbContext _context;

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

    public async Task<List<AsignaturaResponse>> ObtenerPorPlanEstudiosAsync(int idPlanEstudios)
    {
        return await _context.Asignaturas
            .Where(a => a.IdPlanEstudios == idPlanEstudios)
            .OrderBy(a => a.Semestre)
            .ThenBy(a => a.Nombre)
            .Select(a => ToResponse(a))
            .ToListAsync();
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
            Nombre = request.Nombre.Trim(),
            Creditos = request.Creditos,
            Semestre = request.Semestre
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

        bool codigoUsadoPorOtra = await _context.Asignaturas
            .AnyAsync(a => a.Codigo == codigoNormalizado && a.IdAsignatura != idAsignatura);

        if (codigoUsadoPorOtra)
        {
            throw new InvalidOperationException("El código ya está siendo usado por otra asignatura.");
        }

        asignatura.IdPlanEstudios = request.IdPlanEstudios;
        asignatura.Codigo = codigoNormalizado;
        asignatura.Nombre = request.Nombre.Trim();
        asignatura.Creditos = request.Creditos;
        asignatura.Semestre = request.Semestre;

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

        _context.Asignaturas.Remove(asignatura);
        await _context.SaveChangesAsync();

        return true;
    }

    private static AsignaturaResponse ToResponse(Asignatura a) => new()
    {
        IdAsignatura = a.IdAsignatura,
        IdPlanEstudios = a.IdPlanEstudios,
        Codigo = a.Codigo,
        Nombre = a.Nombre,
        Creditos = a.Creditos,
        Semestre = a.Semestre
    };
}
