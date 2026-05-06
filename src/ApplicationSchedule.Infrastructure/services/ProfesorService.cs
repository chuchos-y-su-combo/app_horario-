using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.services;

public class ProfesorService : IProfesorService
{
    private readonly AppDbContext _context;

    public ProfesorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProfesorResponse>> ObtenerTodosAsync()
    {
        return await _context.Profesores
            .Select(p => new ProfesorResponse
            {
                IdProfesor = p.IdProfesor,
                Nombre = p.Nombre,
                Identificacion = p.Identificacion,
                TipoContrato = p.TipoContrato
            })
            .ToListAsync();
    }

    public async Task<ProfesorResponse?> ObtenerPorIdAsync(int idProfesor)
    {
        var profesor = await _context.Profesores.FindAsync(idProfesor);

        if (profesor is null) return null;

        return new ProfesorResponse
        {
            IdProfesor = profesor.IdProfesor,
            Nombre = profesor.Nombre,
            Identificacion = profesor.Identificacion,
            TipoContrato = profesor.TipoContrato
        };
    }

    public async Task<ProfesorResponse> CrearAsync(CrearProfesorRequest request)
    {
        bool existeIdentificacion = await _context.Profesores
            .AnyAsync(p => p.Identificacion == request.Identificacion);

        if (existeIdentificacion)
        {
            throw new InvalidOperationException("Ya existe un profesor con esta identificación.");
        }

        var profesor = new Profesor
        {
            Nombre = request.Nombre,
            Identificacion = request.Identificacion,
            TipoContrato = request.TipoContrato
        };

        _context.Profesores.Add(profesor);
        await _context.SaveChangesAsync();

        return new ProfesorResponse
        {
            IdProfesor = profesor.IdProfesor,
            Nombre = profesor.Nombre,
            Identificacion = profesor.Identificacion,
            TipoContrato = profesor.TipoContrato
        };
    }

    public async Task<bool> ActualizarAsync(int idProfesor, ActualizarProfesorRequest request)
    {
        var profesor = await _context.Profesores.FindAsync(idProfesor);

        if (profesor is null) return false;

        bool existeIdentificacion = await _context.Profesores
            .AnyAsync(p => p.Identificacion == request.Identificacion && p.IdProfesor != idProfesor);

        if (existeIdentificacion)
        {
            throw new InvalidOperationException("A otro profesor ya le pertenece esta identificación.");
        }

        profesor.Nombre = request.Nombre;
        profesor.Identificacion = request.Identificacion;
        profesor.TipoContrato = request.TipoContrato;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int idProfesor)
    {
        var profesor = await _context.Profesores.FindAsync(idProfesor);

        if (profesor is null) return false;

        _context.Profesores.Remove(profesor);
        await _context.SaveChangesAsync();

        return true;
    }
}