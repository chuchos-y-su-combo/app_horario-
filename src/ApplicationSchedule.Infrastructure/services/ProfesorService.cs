using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class ProfesorService : IProfesorService
{
    private readonly AppDbContext _context;

    public ProfesorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProfesorResponse>> ObtenerTodosAsync()
    {
        return await _context.Docentes
            .OrderBy(d => d.Nombre)
            .Select(d => ToResponse(d))
            .ToListAsync();
    }

    public async Task<ProfesorResponse?> ObtenerPorIdAsync(string idProfesor)
    {
        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idProfesor);

        return docente is null ? null : ToResponse(docente);
    }

    public async Task<ProfesorResponse> CrearAsync(CrearProfesorRequest request)
    {
        string identificacionNormalizada = request.Identificacion.Trim();
        string tipoContratoNormalizado = NormalizarTipoContrato(request.TipoContrato);

        bool existeIdentificacion = await _context.Docentes
            .AnyAsync(d => d.Identificacion == identificacionNormalizada);

        if (existeIdentificacion)
        {
            throw new InvalidOperationException("Ya existe un docente con esta identificación.");
        }

        var docente = new Docente
        {
            IdDocente = Guid.NewGuid().ToString(),
            Nombre = request.Nombre.Trim(),
            Identificacion = identificacionNormalizada,
            TipoContrato = tipoContratoNormalizado,
            MaxAsignaturas = ObtenerMaxAsignaturasPorContrato(tipoContratoNormalizado)
        };

        _context.Docentes.Add(docente);
        await _context.SaveChangesAsync();

        return ToResponse(docente);
    }

    public async Task<bool> ActualizarAsync(string idProfesor, ActualizarProfesorRequest request)
    {
        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idProfesor);

        if (docente is null)
        {
            return false;
        }

        string identificacionNormalizada = request.Identificacion.Trim();
        string tipoContratoNormalizado = NormalizarTipoContrato(request.TipoContrato);

        bool identificacionUsadaPorOtro = await _context.Docentes
            .AnyAsync(d => d.Identificacion == identificacionNormalizada && d.IdDocente != idProfesor);

        if (identificacionUsadaPorOtro)
        {
            throw new InvalidOperationException("A otro docente ya le pertenece esta identificación.");
        }

        docente.Nombre = request.Nombre.Trim();
        docente.Identificacion = identificacionNormalizada;
        docente.TipoContrato = tipoContratoNormalizado;
        docente.MaxAsignaturas = ObtenerMaxAsignaturasPorContrato(tipoContratoNormalizado);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(string idProfesor)
    {
        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idProfesor);

        if (docente is null)
        {
            return false;
        }

        bool tieneAsignaciones = await _context.Asignaciones
            .AnyAsync(a => a.IdDocente == idProfesor);

        if (tieneAsignaciones)
        {
            throw new InvalidOperationException("No se puede eliminar un docente que tiene asignaciones registradas.");
        }

        _context.Docentes.Remove(docente);
        await _context.SaveChangesAsync();

        return true;
    }

    private static string NormalizarTipoContrato(string tipoContrato)
    {
        string contrato = tipoContrato.Trim().ToUpperInvariant();

        return contrato switch
        {
            "TC" => "TC",
            "TIEMPO COMPLETO" => "TC",
            "TP" => "TP",
            "PARCIAL" => "TP",
            _ => throw new InvalidOperationException("Tipo de contrato no válido. Use TC, TP, Tiempo Completo o Parcial.")
        };
    }

    private static int ObtenerMaxAsignaturasPorContrato(string tipoContrato)
    {
        return tipoContrato switch
        {
            "TC" => 5,
            "TP" => 3,
            _ => throw new InvalidOperationException("Tipo de contrato no válido. Use TC o TP.")
        };
    }

    private static ProfesorResponse ToResponse(Docente docente) => new()
    {
        IdProfesor = docente.IdDocente,
        Nombre = docente.Nombre,
        Identificacion = docente.Identificacion,
        TipoContrato = docente.TipoContrato,
        MaxAsignaturas = docente.MaxAsignaturas
    };
}