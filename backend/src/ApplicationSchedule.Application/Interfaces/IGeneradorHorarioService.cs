using ApplicationSchedule.Application.DTOs.Horarios;

namespace ApplicationSchedule.Application.Interfaces;

/// <summary>
/// Contrato para servicios que generan propuestas automáticas de horarios.
/// Implementaciones deben producir propuestas y resumir no asignadas según reglas de negocio.
/// </summary>
public interface IGeneradorHorarioService
{
    /// <summary>
    /// Genera propuestas de asignaciones para el periodo y escenarios indicados.
    /// </summary>
    /// <param name="request">Parámetros de generación (periodo, filtros y opciones).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Resumen con propuestas y asignaturas no asignadas.</returns>
    Task<GenerarPropuestasHorarioResponse> GenerarPropuestasAsync(
        GenerarPropuestasHorarioRequest request,
        CancellationToken cancellationToken = default
    );
}