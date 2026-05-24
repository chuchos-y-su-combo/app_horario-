using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Servicio encargado de la generación automática de propuestas de horarios.
/// Implementa la lógica para evaluar asignaturas, seleccionar docentes candidatos,
/// validar disponibilidades, evitar cruces y crear propuestas en estado "Propuesta".
/// </summary>
public class GeneradorHorarioService : IGeneradorHorarioService
{
    private const string EstadoPropuesta = "Propuesta";

    private readonly AppDbContext _context;

    /// <summary>
    /// Crea una instancia de <see cref="GeneradorHorarioService"/> con el contexto de datos inyectado.
    /// </summary>
    public GeneradorHorarioService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Genera propuestas automáticas de asignaciones para los escenarios indicados en la petición.
    /// Puede borrar propuestas previas si se solicita y devuelve un resumen con propuestas y no-asignadas.
    /// </summary>
    public async Task<GenerarPropuestasHorarioResponse> GenerarPropuestasAsync(
        GenerarPropuestasHorarioRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarRequest(request);

        var response = new GenerarPropuestasHorarioResponse
        {
            Periodo = request.Periodo
        };

        List<string> escenarios = ObtenerEscenariosObjetivo(request);

        List<PlanEstudio> planes = await _context.PlanesEstudio
            .ToListAsync(cancellationToken);

        if (request.BorrarPropuestasPrevias)
        {
            await BorrarPropuestasPreviasAsync(request.Periodo, escenarios, cancellationToken);
        }

        List<Asignacion> asignacionesExistentes = await _context.Asignaciones
            .Where(a => a.Periodo == request.Periodo)
            .ToListAsync(cancellationToken);

        List<BloqueoFranjaAsignatura> bloqueosFranja = await _context.BloqueosFranjaAsignatura
            .Where(b => b.Periodo == request.Periodo)
            .ToListAsync(cancellationToken);

        List<Docente> docentes = await _context.Docentes
            .Include(d => d.Disponibilidades)
            .Include(d => d.AsignaturasHabilitadas)
            .ToListAsync(cancellationToken);

        foreach (string escenario in escenarios)
        {
            PlanEstudio? plan = ObtenerPlanParaEscenario(escenario, planes);

            if (plan is null)
            {
                response.Mensajes.Add($"No se encontró un plan de estudio para el escenario {escenario}.");
                continue;
            }

           List<Asignatura> asignaturasPlan = await _context.Asignaturas
                .Where(a => a.IdPlan == plan.IdPlan)
                .OrderBy(a => a.Semestre)
                .ThenBy(a => a.Nombre)
                .ToListAsync(cancellationToken);

            if (!EscenarioGeneracion.EsDiurno(escenario) && asignaturasPlan.Count == 0)
            {
                asignaturasPlan = await _context.Asignaturas
                    .OrderBy(a => a.Semestre)
                    .ThenBy(a => a.Nombre)
                    .ToListAsync(cancellationToken);

                response.Mensajes.Add(
                    $"El escenario {escenario} no tenía asignaturas asociadas al plan nocturno. Se usó el catálogo académico existente para generar la propuesta."
                );
            }

            List<Asignatura> asignaturasEscenario = ObtenerAsignaturasParaEscenario(
                escenario,
                asignaturasPlan,
                request.SemestreIngenieria
            );

            var resumen = new ResumenEscenarioHorarioResponse
            {
                Escenario = escenario,
                IdPlan = plan.IdPlan,
                NombrePlan = plan.NombrePlan
            };

            int limiteCreditos = EscenarioGeneracion.EsDiurno(escenario) ? 18 : 15;

            if (escenario == EscenarioGeneracion.TapsiDiurna)
            {
                await GenerarTapsiDiurnaAsync(
                    escenario,
                    plan,
                    asignaturasEscenario,
                    docentes,
                    asignacionesExistentes,
                    bloqueosFranja,
                    request.Periodo,
                    response,
                    resumen,
                    limiteCreditos,
                    cancellationToken
                );
            }
            else
            {
                int creditosAcumulados = 0;

                foreach (Asignatura asignatura in asignaturasEscenario)
                {
                    if (creditosAcumulados + asignatura.Creditos > limiteCreditos)
                    {
                        resumen.TotalAsignaturasEvaluadas++;
                        resumen.TotalNoAsignadas++;
                        response.NoAsignadas.Add(new AsignaturaNoAsignadaResponse
                        {
                            Escenario = escenario,
                            IdPlan = plan.IdPlan,
                            NombrePlan = plan.NombrePlan,
                            IdAsignatura = asignatura.IdAsignatura,
                            CodigoAsignatura = asignatura.Codigo,
                            NombreAsignatura = asignatura.Nombre,
                            Motivo = $"Excede límite de créditos por jornada ({limiteCreditos} créditos)"
                        });
                        continue;
                    }

                    creditosAcumulados += asignatura.Creditos;
                    ProcesarAsignatura(
                        escenario,
                        plan,
                        asignatura,
                        docentes,
                        asignacionesExistentes,
                        bloqueosFranja,
                        request.Periodo,
                        response,
                        resumen
                    );
                }
            }

            response.ResumenEscenarios.Add(resumen);
        }

        await _context.SaveChangesAsync(cancellationToken);

        response.TotalEscenariosProcesados = response.ResumenEscenarios.Count;
        response.TotalAsignaturasEvaluadas = response.ResumenEscenarios.Sum(r => r.TotalAsignaturasEvaluadas);
        response.TotalPropuestasCreadas = response.Propuestas.Count;
        response.TotalAsignaturasNoAsignadas = response.NoAsignadas.Count;

        response.Mensajes.Add("Generación automática de propuestas finalizada.");

        return response;
    }

    /// <summary>
    /// Genera propuestas específicas para el escenario TAPSI en jornada diurna,
    /// priorizando las obligatorias y buscando una opción adicional compatible.
    /// </summary>
    private async Task GenerarTapsiDiurnaAsync(
        string escenario,
        PlanEstudio plan,
        List<Asignatura> asignaturasEscenario,
        List<Docente> docentes,
        List<Asignacion> asignacionesExistentes,
        List<BloqueoFranjaAsignatura> bloqueosFranja,
        string periodo,
        GenerarPropuestasHorarioResponse response,
        ResumenEscenarioHorarioResponse resumen,
        int limiteCreditos,
        CancellationToken cancellationToken)
    {
        List<Asignatura> obligatorias = asignaturasEscenario;

        int creditosAcumulados = 0;

        foreach (Asignatura asignatura in obligatorias)
        {
            if (creditosAcumulados + asignatura.Creditos > limiteCreditos)
            {
                resumen.TotalAsignaturasEvaluadas++;
                resumen.TotalNoAsignadas++;
                response.NoAsignadas.Add(new AsignaturaNoAsignadaResponse
                {
                    Escenario = escenario,
                    IdPlan = plan.IdPlan,
                    NombrePlan = plan.NombrePlan,
                    IdAsignatura = asignatura.IdAsignatura,
                    CodigoAsignatura = asignatura.Codigo,
                    NombreAsignatura = asignatura.Nombre,
                    Motivo = $"Excede límite de créditos por jornada ({limiteCreditos} créditos)"
                });
                continue;
            }

            creditosAcumulados += asignatura.Creditos;
            ProcesarAsignatura(
                escenario,
                plan,
                asignatura,
                docentes,
                asignacionesExistentes,
                bloqueosFranja,
                periodo,
                response,
                resumen
            );
        }

        List<Asignatura> opcionesAdicionales = await _context.Asignaturas
            .Where(a =>
                a.IdPlan == plan.IdPlan &&
                a.EsOpcionalTapsiDiurna
            )
            .OrderBy(a => a.Nombre)
            .ToListAsync(cancellationToken);

        bool adicionalAsignada = false;
        var motivosFallidos = new List<string>();

        foreach (Asignatura opcion in opcionesAdicionales)
        {
            resumen.TotalAsignaturasEvaluadas++;

            if (creditosAcumulados + opcion.Creditos > limiteCreditos)
            {
                motivosFallidos.Add($"{opcion.Codigo} - {opcion.Nombre}: Excede límite de créditos por jornada ({limiteCreditos} créditos)");
                continue;
            }

            ResultadoAsignacion resultado = IntentarAsignarAsignatura(
                escenario,
                plan,
                opcion,
                docentes,
                asignacionesExistentes,
                bloqueosFranja,
                periodo
            );

            if (resultado.Asignacion is null)
            {
                motivosFallidos.Add($"{opcion.Codigo} - {opcion.Nombre}: {resultado.Motivo}");
                continue;
            }

            _context.Asignaciones.Add(resultado.Asignacion);
            asignacionesExistentes.Add(resultado.Asignacion);

            response.Propuestas.Add(CrearPropuestaResponse(
                escenario,
                plan,
                opcion,
                resultado
            ));

            resumen.TotalPropuestasCreadas++;
            adicionalAsignada = true;
            break;
        }

        if (!adicionalAsignada)
        {
            response.NoAsignadas.Add(new AsignaturaNoAsignadaResponse
            {
                Escenario = escenario,
                IdPlan = plan.IdPlan,
                NombrePlan = plan.NombrePlan,
                IdAsignatura = string.Empty,
                CodigoAsignatura = "OPCIONAL_TAPSI_DIURNA",
                NombreAsignatura = "Opción adicional TAPSI diurna",
                Motivo = motivosFallidos.Count == 0
                    ? "No existen opciones adicionales TAPSI diurna registradas en la base de datos."
                    : string.Join(" | ", motivosFallidos)
            });

            resumen.TotalNoAsignadas++;
        }
    }

    /// <summary>
    /// Procesa una asignatura específica, intentando asignarla a un docente disponible
    /// y agregando la propuesta o registrando la no-asignada en el resumen.
    /// </summary>
    private void ProcesarAsignatura(
        string escenario,
        PlanEstudio plan,
        Asignatura asignatura,
        List<Docente> docentes,
        List<Asignacion> asignacionesExistentes,
        List<BloqueoFranjaAsignatura> bloqueosFranja,
        string periodo,
        GenerarPropuestasHorarioResponse response,
        ResumenEscenarioHorarioResponse resumen)
    {
        resumen.TotalAsignaturasEvaluadas++;

        ResultadoAsignacion resultado = IntentarAsignarAsignatura(
            escenario,
            plan,
            asignatura,
            docentes,
            asignacionesExistentes,
            bloqueosFranja,
            periodo
        );

        if (resultado.Asignacion is null)
        {
            response.NoAsignadas.Add(new AsignaturaNoAsignadaResponse
            {
                Escenario = escenario,
                IdPlan = plan.IdPlan,
                NombrePlan = plan.NombrePlan,
                IdAsignatura = asignatura.IdAsignatura,
                CodigoAsignatura = asignatura.Codigo,
                NombreAsignatura = asignatura.Nombre,
                Motivo = resultado.Motivo
            });

            resumen.TotalNoAsignadas++;
            return;
        }

        _context.Asignaciones.Add(resultado.Asignacion);
        asignacionesExistentes.Add(resultado.Asignacion);

        response.Propuestas.Add(CrearPropuestaResponse(
            escenario,
            plan,
            asignatura,
            resultado
        ));

        resumen.TotalPropuestasCreadas++;
    }

    /// <summary>
    /// Intenta asignar una asignatura buscando docentes candidatos, validando cargas,
    /// disponibilidades, bloqueos y cruces, y devuelve el resultado con la asignación propuesta.
    /// </summary>
    private ResultadoAsignacion IntentarAsignarAsignatura(
        string escenario,
        PlanEstudio plan,
        Asignatura asignatura,
        List<Docente> docentes,
        List<Asignacion> asignacionesExistentes,
        List<BloqueoFranjaAsignatura> bloqueosFranja,
        string periodo)
    {
        List<Docente> docentesCandidatos = docentes
            .Where(d => d.AsignaturasHabilitadas.Any(h => h.IdAsignatura == asignatura.IdAsignatura))
            .OrderBy(d => ContarAsignaturasDocente(d.IdDocente, asignacionesExistentes, periodo))
            .ThenBy(d => d.Nombre)
            .ToList();

        if (docentesCandidatos.Count == 0)
        {
            return ResultadoAsignacion.Fallido("No hay docentes habilitados para esta asignatura.");
        }

        foreach (Docente docente in docentesCandidatos)
        {
            if (!DocenteTieneCargaDisponible(docente, asignacionesExistentes, periodo, asignatura.IdAsignatura))
            {
                continue;
            }

            List<Disponibilidad> disponibilidades = docente.Disponibilidades
                .OrderBy(d => d.DiaSemana)
                .ThenBy(d => d.HoraInicio)
                .ToList();

            if (disponibilidades.Count == 0)
            {
                continue;
            }


            foreach (Disponibilidad disponibilidad in disponibilidades)
            {
                string horaInicio = disponibilidad.HoraInicio;
                string horaFin = CalcularHoraFin(horaInicio, asignatura.Creditos);

                if (!BloqueCabeEnDisponibilidad(horaInicio, horaFin, disponibilidad))
                {
                    continue;
                }

                if (ExisteBloqueoFranjaAsignatura(
                    asignatura.IdAsignatura,
                    periodo,
                    disponibilidad.DiaSemana,
                    horaInicio,
                    horaFin,
                    bloqueosFranja))
                {
                    continue;
                }

                if (ExisteCruceDocente(
                    docente.IdDocente,
                    disponibilidad.DiaSemana,
                    horaInicio,
                    horaFin,
                    asignacionesExistentes,
                    periodo))
                {
                    continue;
                }

                if (ExisteCruceEscenario(
                    escenario,
                    disponibilidad.DiaSemana,
                    horaInicio,
                    horaFin,
                    asignacionesExistentes,
                    periodo))
                {
                    continue;
                }

                var asignacion = new Asignacion
                {
                    IdAsignacion = Guid.NewGuid().ToString(),
                    IdDocente = docente.IdDocente,
                    IdAsignatura = asignatura.IdAsignatura,
                    Dia = disponibilidad.DiaSemana,
                    HoraInicio = horaInicio,
                    HoraFin = horaFin,
                    Periodo = periodo,
                    Estado = EstadoPropuesta,
                    Escenario = escenario
                };

                return ResultadoAsignacion.Exitoso(asignacion, docente.Nombre);
            }
        }

        return ResultadoAsignacion.Fallido(
            "No se encontró un docente con disponibilidad, carga disponible y horario sin cruces."
        );
    }

    /// <summary>
    /// Filtra la lista de asignaturas según el escenario de generación (Ingeniería, TAPSI, etc.).
    /// </summary>
    private static List<Asignatura> ObtenerAsignaturasParaEscenario(
        string escenario,
        List<Asignatura> asignaturasPlan,
        int semestreIngenieria)
    {
        if (escenario == EscenarioGeneracion.IngDiurna ||
            escenario == EscenarioGeneracion.IngNocturna)
        {
            return semestreIngenieria == 1
                ? asignaturasPlan.Where(a => a.Semestre == 1).OrderBy(a => a.Nombre).ToList()
                : asignaturasPlan.Where(a => a.Semestre == semestreIngenieria && a.EsAreaProfesional).OrderBy(a => a.Nombre).ToList();
        }

        return asignaturasPlan.Where(a => a.EsFijaTapsi).OrderBy(a => a.Nombre).ToList();
    }

    /// <summary>
    /// Normaliza y valida los escenarios solicitados en la petición.
    /// </summary>
    private static List<string> ObtenerEscenariosObjetivo(GenerarPropuestasHorarioRequest request)
    {
        if (request.Escenarios.Count == 0)
        {
            return EscenarioGeneracion.Todos.ToList();
        }

        var escenarios = new List<string>();

        foreach (string escenario in request.Escenarios)
        {
            string escenarioNormalizado = escenario.Trim().ToUpperInvariant();

            if (!EscenarioGeneracion.Todos.Contains(escenarioNormalizado))
            {
                throw new InvalidOperationException($"Escenario inválido: {escenario}");
            }

            escenarios.Add(escenarioNormalizado);
        }

        return escenarios.Distinct().ToList();
    }

    /// <summary>
    /// Obtiene el plan de estudio asociado al escenario a partir de la lista de planes.
    /// </summary>
    private static PlanEstudio? ObtenerPlanParaEscenario(
        string escenario,
        List<PlanEstudio> planes)
    {
        return escenario switch
        {
            EscenarioGeneracion.IngDiurna => planes.FirstOrDefault(p =>
                p.Jornada.Equals("Diurna", StringComparison.OrdinalIgnoreCase) &&
                !p.NombrePlan.Contains("TAPSI", StringComparison.OrdinalIgnoreCase)),
            EscenarioGeneracion.IngNocturna => planes.FirstOrDefault(p =>
                p.Jornada.Equals("Nocturna", StringComparison.OrdinalIgnoreCase) &&
                !p.NombrePlan.Contains("TAPSI", StringComparison.OrdinalIgnoreCase)),
            EscenarioGeneracion.TapsiDiurna => planes.FirstOrDefault(p =>
                p.NombrePlan.Contains("TAPSI", StringComparison.OrdinalIgnoreCase) &&
                p.Jornada.Equals("Diurna", StringComparison.OrdinalIgnoreCase)),
            EscenarioGeneracion.TapsiNocturna => planes.FirstOrDefault(p =>
                p.NombrePlan.Contains("TAPSI", StringComparison.OrdinalIgnoreCase) &&
                p.Jornada.Equals("Nocturna", StringComparison.OrdinalIgnoreCase)),
            _ => null
        };
    }

    /// <summary>
    /// Elimina propuestas previas en estado "Propuesta" para los escenarios indicados.
    /// </summary>
    private async Task BorrarPropuestasPreviasAsync(
        string periodo,
        List<string> escenarios,
        CancellationToken cancellationToken)
    {
        List<Asignacion> propuestasPrevias = await _context.Asignaciones
            .Where(a =>
                a.Periodo == periodo &&
                a.Estado == EstadoPropuesta &&
                escenarios.Contains(a.Escenario))
            .ToListAsync(cancellationToken);

        _context.Asignaciones.RemoveRange(propuestasPrevias);

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Determina si un docente dispone de carga disponible en el periodo para una asignatura nueva.
    /// </summary>
    private static bool DocenteTieneCargaDisponible(
        Docente docente,
        List<Asignacion> asignaciones,
        string periodo,
        string idAsignaturaNueva)
    {
        int asignaturasActuales = asignaciones
            .Where(a => a.IdDocente == docente.IdDocente && a.Periodo == periodo)
            .Select(a => a.IdAsignatura)
            .Distinct()
            .Count();

        bool yaTieneLaAsignatura = asignaciones.Any(a =>
            a.IdDocente == docente.IdDocente &&
            a.Periodo == periodo &&
            a.IdAsignatura == idAsignaturaNueva
        );

        if (yaTieneLaAsignatura)
        {
            return true;
        }

        return asignaturasActuales < docente.MaxAsignaturas;
    }

    /// <summary>
    /// Cuenta asignaturas distintas asignadas a un docente en un periodo dentro de una lista de asignaciones.
    /// </summary>
    private static int ContarAsignaturasDocente(
        string idDocente,
        List<Asignacion> asignaciones,
        string periodo)
    {
        return asignaciones
            .Where(a => a.IdDocente == idDocente && a.Periodo == periodo)
            .Select(a => a.IdAsignatura)
            .Distinct()
            .Count();
    }

    /// <summary>
    /// Valida si un bloque horario cabe dentro de la disponibilidad de un docente.
    /// </summary>
    private static bool BloqueCabeEnDisponibilidad(
        string horaInicio,
        string horaFin,
        Disponibilidad disponibilidad)
    {
        TimeSpan inicio = TimeSpan.Parse(horaInicio);
        TimeSpan fin = TimeSpan.Parse(horaFin);
        TimeSpan disponibleInicio = TimeSpan.Parse(disponibilidad.HoraInicio);
        TimeSpan disponibleFin = TimeSpan.Parse(disponibilidad.HoraFin);

        return inicio >= disponibleInicio && fin <= disponibleFin;
    }

    /// <summary>
    /// Verifica si existe un bloqueo para la franja horaria de la asignatura en el periodo.
    /// </summary>
    private static bool ExisteBloqueoFranjaAsignatura(
        string idAsignatura,
        string periodo,
        int dia,
        string horaInicio,
        string horaFin,
        List<BloqueoFranjaAsignatura> bloqueosFranja)
    {
        return bloqueosFranja.Any(b =>
            b.IdAsignatura == idAsignatura &&
            b.Periodo == periodo &&
            b.Dia == dia &&
            HorariosSeCruzan(horaInicio, horaFin, b.HoraInicio, b.HoraFin)
        );
    }
    /// <summary>
    /// Verifica cruces de horarios para un docente dado.
    /// </summary>
    private static bool ExisteCruceDocente(
        string idDocente,
        int dia,
        string horaInicio,
        string horaFin,
        List<Asignacion> asignaciones,
        string periodo)
    {
        return asignaciones.Any(a =>
            a.IdDocente == idDocente &&
            a.Periodo == periodo &&
            a.Dia == dia &&
            HorariosSeCruzan(horaInicio, horaFin, a.HoraInicio, a.HoraFin)
        );
    }

    /// <summary>
    /// Verifica cruces dentro del mismo escenario.
    /// </summary>
    private static bool ExisteCruceEscenario(
        string escenario,
        int dia,
        string horaInicio,
        string horaFin,
        List<Asignacion> asignaciones,
        string periodo)
    {
        return asignaciones.Any(a =>
            a.Escenario == escenario &&
            a.Periodo == periodo &&
            a.Dia == dia &&
            HorariosSeCruzan(horaInicio, horaFin, a.HoraInicio, a.HoraFin)
        );
    }

    /// <summary>
    /// Determina si dos intervalos horarios se solapan.
    /// </summary>
    private static bool HorariosSeCruzan(
        string inicioA,
        string finA,
        string inicioB,
        string finB)
    {
        TimeSpan aInicio = TimeSpan.Parse(inicioA);
        TimeSpan aFin = TimeSpan.Parse(finA);
        TimeSpan bInicio = TimeSpan.Parse(inicioB);
        TimeSpan bFin = TimeSpan.Parse(finB);

        return aInicio < bFin && bInicio < aFin;
    }

    /// <summary>
    /// Calcula la hora de fin a partir de la hora de inicio y la duración en horas (créditos).
    /// </summary>
    private static string CalcularHoraFin(string horaInicio, int creditos)
    {
        TimeSpan inicio = TimeSpan.Parse(horaInicio);
        TimeSpan fin = inicio.Add(TimeSpan.FromHours(creditos));

        return $"{fin.Hours:00}:{fin.Minutes:00}";
    }

    /// <summary>
    /// Devuelve el nombre del día en español a partir de su índice.
    /// </summary>
    private static string ObtenerNombreDia(int dia)
    {
        return dia switch
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

    /// <summary>
    /// Crea el DTO de propuesta a partir de la entidad <see cref="Asignacion"/> y datos contextuales.
    /// </summary>
    private static PropuestaAsignacionResponse CrearPropuestaResponse(
        string escenario,
        PlanEstudio plan,
        Asignatura asignatura,
        ResultadoAsignacion resultado)
    {
        Asignacion asignacion = resultado.Asignacion!;

        return new PropuestaAsignacionResponse
        {
            IdAsignacion = asignacion.IdAsignacion,
            Escenario = escenario,
            IdPlan = plan.IdPlan,
            NombrePlan = plan.NombrePlan,
            IdAsignatura = asignatura.IdAsignatura,
            CodigoAsignatura = asignatura.Codigo,
            NombreAsignatura = asignatura.Nombre,
            IdDocente = asignacion.IdDocente,
            NombreDocente = resultado.NombreDocente,
            Dia = asignacion.Dia,
            DiaNombre = ObtenerNombreDia(asignacion.Dia),
            HoraInicio = asignacion.HoraInicio,
            HoraFin = asignacion.HoraFin,
            Estado = asignacion.Estado
        };
    }

    /// <summary>
    /// Valida parámetros mínimos del request de generación (periodo y semestre).
    /// Lanza <see cref="InvalidOperationException"/> si faltan o son inválidos.
    /// </summary>
    private static void ValidarRequest(GenerarPropuestasHorarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Periodo))
        {
            throw new InvalidOperationException("El periodo es obligatorio.");
        }

        if (request.SemestreIngenieria < 1 || request.SemestreIngenieria > 12)
        {
            throw new InvalidOperationException("El semestre de Ingeniería debe estar entre 1 y 12.");
        }
    }

    private class ResultadoAsignacion
    {
        public Asignacion? Asignacion { get; private set; }

        public string NombreDocente { get; private set; } = string.Empty;

        public string Motivo { get; private set; } = string.Empty;

        public static ResultadoAsignacion Exitoso(Asignacion asignacion, string nombreDocente)
        {
            return new ResultadoAsignacion
            {
                Asignacion = asignacion,
                NombreDocente = nombreDocente
            };
        }

        public static ResultadoAsignacion Fallido(string motivo)
        {
            return new ResultadoAsignacion
            {
                Motivo = motivo
            };
        }
    }
}