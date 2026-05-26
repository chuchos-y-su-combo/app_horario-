using ApplicationSchedule.Application.DTOs.Disponibilidades;
using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IProfesorService"/> que gestiona la entidad <see cref="Docente"/>.
/// Contiene validaciones de unicidad y reglas relacionadas con el tipo de contrato.
/// </summary>
public class ProfesorService : IProfesorService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Crea una instancia de <see cref="ProfesorService"/> con el contexto de datos inyectado.
    /// </summary>
    public ProfesorService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Recupera todos los docentes ordenados por nombre.
    /// </summary>
    /// <returns>Lista de <see cref="ProfesorResponse"/>.</returns>
    public async Task<List<ProfesorResponse>> ObtenerTodosAsync()
    {
        return await _context.Docentes
            .OrderBy(d => d.Nombre)
            .Select(d => ToResponse(d))
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un docente por su identificador.
    /// </summary>
    /// <param name="idProfesor">Identificador del docente.</param>
    /// <returns>DTO del docente o null si no existe.</returns>
    public async Task<ProfesorResponse?> ObtenerPorIdAsync(string idProfesor)
    {
        Docente? docente = await _context.Docentes
            .FirstOrDefaultAsync(d => d.IdDocente == idProfesor);

        return docente is null ? null : ToResponse(docente);
    }

    /// <summary>
    /// Crea un nuevo docente validando unicidad de identificación y normalizando el tipo de contrato.
    /// </summary>
    /// <param name="request">Datos para crear el docente.</param>
    /// <returns>DTO del docente creado.</returns>
    /// <exception cref="InvalidOperationException">Si la identificación ya existe.</exception>
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

    /// <summary>
    /// Actualiza un docente existente tras validaciones de unicidad.
    /// </summary>
    /// <param name="idProfesor">Identificador del docente.</param>
    /// <param name="request">Datos a actualizar.</param>
    /// <returns>True si se actualizó; false si no se encontró.</returns>
    /// <exception cref="InvalidOperationException">Si la identificación pertenece a otro docente.</exception>
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

    /// <summary>
    /// Elimina un docente si no posee asignaciones registradas.
    /// </summary>
    /// <param name="idProfesor">Identificador del docente a eliminar.</param>
    /// <returns>True si se eliminó; false si no se encontró.</returns>
    /// <exception cref="InvalidOperationException">Si el docente tiene asignaciones y no puede eliminarse.</exception>
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

    /// <summary>
    /// Normaliza el texto de tipo de contrato a valores aceptados por el dominio.
    /// Acepta variantes legibles y las mapea a los códigos internos (TC, TP).
    /// Lanza <see cref="InvalidOperationException"/> si el valor no es reconocido.
    /// </summary>
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

    /// <summary>
    /// Devuelve el máximo de asignaturas permitido según el tipo de contrato.
    /// Lanza <see cref="InvalidOperationException"/> si el tipo de contrato es inválido.
    /// </summary>
    private static int ObtenerMaxAsignaturasPorContrato(string tipoContrato)
    {
        return tipoContrato switch
        {
            "TC" => 5,
            "TP" => 3,
            _ => throw new InvalidOperationException("Tipo de contrato no válido. Use TC o TP.")
        };
    }

    /// <summary>
    /// Retorna las franjas de disponibilidad horaria registradas para un docente.
    /// </summary>
    public async Task<List<DisponibilidadDocenteResponse>> ObtenerDisponibilidadAsync(string idProfesor)
    {
        return await _context.Disponibilidades
            .Where(d => d.IdDocente == idProfesor)
            .OrderBy(d => d.DiaSemana)
            .ThenBy(d => d.HoraInicio)
            .Select(d => new DisponibilidadDocenteResponse
            {
                IdDisponibilidad = d.IdDisponibilidad,
                IdDocente       = d.IdDocente,
                DiaSemana       = d.DiaSemana,
                DiaNombre       = ObtenerNombreDia(d.DiaSemana),
                HoraInicio      = d.HoraInicio,
                HoraFin         = d.HoraFin,
            })
            .ToListAsync();
    }

    /// <summary>
    /// Elimina TODOS los docentes y sus asignaciones, disponibilidades y materias habilitadas.
    /// </summary>
    public async Task<int> EliminarTodosAsync()
    {
        // Eliminar asignaciones
        var asignaciones = await _context.Asignaciones.ToListAsync();
        _context.Asignaciones.RemoveRange(asignaciones);

        // Eliminar disponibilidades
        var disponibilidades = await _context.Disponibilidades.ToListAsync();
        _context.Disponibilidades.RemoveRange(disponibilidades);

        // Eliminar docentes habilitados (relación M:M)
        var habilitados = await _context.DocentesHabilitados.ToListAsync();
        _context.DocentesHabilitados.RemoveRange(habilitados);

        // Eliminar docentes
        var docentes = await _context.Docentes.ToListAsync();
        int total = docentes.Count;
        _context.Docentes.RemoveRange(docentes);

        await _context.SaveChangesAsync();
        return total;
    }

    /// <summary>
    /// Retorna los identificadores de asignaturas que el docente está habilitado para dictar.
    /// </summary>
    public async Task<List<string>> ObtenerHabilitadosAsync(string idProfesor)
    {
        return await _context.DocentesHabilitados
            .Where(h => h.IdDocente == idProfesor)
            .Select(h => h.IdAsignatura)
            .ToListAsync();
    }

    private static string ObtenerNombreDia(int dia) => dia switch
    {
        1 => "Lunes",
        2 => "Martes",
        3 => "Miércoles",
        4 => "Jueves",
        5 => "Viernes",
        6 => "Sábado",
        _ => "Desconocido",
    };

    /// <summary>
    /// Mappea la entidad <see cref="Docente"/> a su DTO <see cref="ProfesorResponse"/>.
    /// </summary>
    private static ProfesorResponse ToResponse(Docente docente) => new()
    {
        IdProfesor = docente.IdDocente,
        Nombre = docente.Nombre,
        Identificacion = docente.Identificacion,
        TipoContrato = docente.TipoContrato,
        MaxAsignaturas = docente.MaxAsignaturas
    };
}