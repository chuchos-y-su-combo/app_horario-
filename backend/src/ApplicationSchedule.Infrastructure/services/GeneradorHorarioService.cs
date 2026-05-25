using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApplicationSchedule.Infrastructure.Services;

public class GeneradorHorarioService : IGeneradorHorarioService
{
    private const string EstadoPropuesta = "Propuesta";

    private readonly AppDbContext _context;

    public GeneradorHorarioService(AppDbContext context)
    {
        _context = context;
    }

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

            int numSesiones = ObtenerSesionesPorCreditos(opcion.Creditos);
            if (numSesiones == 0)
            {
                motivosFallidos.Add($"{opcion.Codigo} - {opcion.Nombre}: Práctica empresarial sin sesiones de clase");
                continue;
            }

            List<ResultadoAsignacion> resultados = IntentarAsignarTodasSesiones(
                escenario, plan, opcion, numSesiones, docentes, asignacionesExistentes, bloqueosFranja, periodo);

            if (resultados.Count < numSesiones)
            {
                motivosFallidos.Add($"{opcion.Codigo} - {opcion.Nombre}: " +
                    (resultados.Count == 0
                        ? "Sin docente disponible con horario libre"
                        : $"Solo {resultados.Count}/{numSesiones} sesiones ubicadas"));
                continue;
            }

            foreach (ResultadoAsignacion res in resultados)
            {
                _context.Asignaciones.Add(res.Asignacion!);
                asignacionesExistentes.Add(res.Asignacion!);
                response.Propuestas.Add(CrearPropuestaResponse(escenario, plan, opcion, res));
                resumen.TotalPropuestasCreadas++;
            }

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
        int numSesiones = ObtenerSesionesPorCreditos(asignatura.Creditos);

        if (numSesiones == 0)
        {
            // Práctica Empresarial (9 créditos): omitir silenciosamente
            return;
        }

        resumen.TotalAsignaturasEvaluadas++;

        List<ResultadoAsignacion> resultados = IntentarAsignarTodasSesiones(
            escenario, plan, asignatura, numSesiones, docentes, asignacionesExistentes, bloqueosFranja, periodo);

        if (resultados.Count < numSesiones)
        {
            response.NoAsignadas.Add(new AsignaturaNoAsignadaResponse
            {
                Escenario = escenario,
                IdPlan = plan.IdPlan,
                NombrePlan = plan.NombrePlan,
                IdAsignatura = asignatura.IdAsignatura,
                CodigoAsignatura = asignatura.Codigo,
                NombreAsignatura = asignatura.Nombre,
                Motivo = resultados.Count == 0
                    ? "No se encontró un docente con disponibilidad y horario sin cruces."
                    : $"Solo se pudo ubicar {resultados.Count} de {numSesiones} sesiones."
            });

            resumen.TotalNoAsignadas++;
            return;
        }

        foreach (ResultadoAsignacion resultado in resultados)
        {
            _context.Asignaciones.Add(resultado.Asignacion!);
            asignacionesExistentes.Add(resultado.Asignacion!);
            response.Propuestas.Add(CrearPropuestaResponse(escenario, plan, asignatura, resultado));
        }

        resumen.TotalPropuestasCreadas += resultados.Count;
    }

    /// <summary>
    /// Intenta asignar todas las sesiones semanales de una asignatura a un único docente
    /// en días distintos. Devuelve la lista de asignaciones creadas (vacía si no fue posible).
    /// </summary>
    private List<ResultadoAsignacion> IntentarAsignarTodasSesiones(
        string escenario,
        PlanEstudio plan,
        Asignatura asignatura,
        int numSesiones,
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
            return [];

        foreach (Docente docente in docentesCandidatos)
        {
            if (!DocenteTieneCargaDisponible(docente, asignacionesExistentes, periodo, asignatura.IdAsignatura))
                continue;

            List<Disponibilidad> disponibilidades = docente.Disponibilidades
                .OrderBy(d => d.DiaSemana)
                .ThenBy(d => d.HoraInicio)
                .ToList();

            if (disponibilidades.Count == 0)
                continue;

            var sesiones = new List<ResultadoAsignacion>();
            var diasUsados = new HashSet<int>();
            // Lista temporal para verificar cruces entre sesiones de esta misma asignatura
            var asignacionesTmp = new List<Asignacion>(asignacionesExistentes);

            foreach (Disponibilidad disponibilidad in disponibilidades)
            {
                if (sesiones.Count == numSesiones) break;
                if (diasUsados.Contains(disponibilidad.DiaSemana)) continue;

                string horaInicio = disponibilidad.HoraInicio;
                string horaFin = CalcularHoraFin(horaInicio);

                if (!BloqueCabeEnDisponibilidad(horaInicio, horaFin, disponibilidad)) continue;

                // Respetar rango de jornada: diurna 07:00-18:00, nocturna 18:30-22:30
                if (!EstaEnRangoJornada(escenario, horaInicio, horaFin)) continue;

                if (ExisteBloqueoFranjaAsignatura(
                    asignatura.IdAsignatura, periodo,
                    disponibilidad.DiaSemana, horaInicio, horaFin, bloqueosFranja)) continue;

                if (ExisteCruceDocente(
                    docente.IdDocente, disponibilidad.DiaSemana, horaInicio, horaFin,
                    asignacionesTmp, periodo)) continue;

                if (ExisteCruceEscenario(
                    escenario, disponibilidad.DiaSemana, horaInicio, horaFin,
                    asignacionesTmp, periodo)) continue;

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

                sesiones.Add(ResultadoAsignacion.Exitoso(asignacion, docente.Nombre));
                diasUsados.Add(disponibilidad.DiaSemana);
                asignacionesTmp.Add(asignacion);
            }

            if (sesiones.Count == numSesiones)
                return sesiones;

            // Este docente no pudo cubrir todas las sesiones; intentar con el siguiente
        }

        return [];
    }

    /// <summary>
    /// Devuelve el número de sesiones semanales (bloques de 2h) según los créditos de la asignatura.
    /// </summary>
    private static int ObtenerSesionesPorCreditos(int creditos) => creditos switch
    {
        1 => 2,  // Inglés: 2 sesiones/semana
        2 => 1,  // 1 sesión/semana
        3 => 2,  // 2 sesiones/semana
        4 => 3,  // 3 sesiones/semana
        9 => 0,  // Práctica Empresarial: sin sesiones
        _ => 1
    };

    private static List<Asignatura> ObtenerAsignaturasParaEscenario(
        string escenario,
        List<Asignatura> asignaturasPlan,
        int semestreIngenieria)  // parámetro mantenido por compatibilidad; ya no filtra por semestre
    {
        if (escenario == EscenarioGeneracion.IngDiurna ||
            escenario == EscenarioGeneracion.IngNocturna)
        {
            // Regla: semestre 1 → todas las asignaturas del plan
            //        semestres 2+ → solo las de área profesional (EsAreaProfesional = true)
            return asignaturasPlan
                .Where(a => a.Semestre == 1 || (a.Semestre > 1 && a.EsAreaProfesional))
                .OrderBy(a => a.Semestre)
                .ThenBy(a => a.Nombre)
                .ToList();
        }

        // TAPSI: solo las asignaturas fijas del plan (EsFijaTapsi = true)
        return asignaturasPlan.Where(a => a.EsFijaTapsi).OrderBy(a => a.Nombre).ToList();
    }

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
            return true;

        return asignaturasActuales < docente.MaxAsignaturas;
    }

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
    /// Verifica que la franja propuesta cae dentro del rango horario permitido para la jornada.
    /// Diurna  → 07:00–18:00 · Nocturna → 18:30–22:30
    /// </summary>
    private static bool EstaEnRangoJornada(string escenario, string horaInicio, string horaFin)
    {
        TimeSpan inicio = TimeSpan.Parse(horaInicio);
        TimeSpan fin    = TimeSpan.Parse(horaFin);

        if (EscenarioGeneracion.EsDiurno(escenario))
        {
            // Jornada diurna: 07:00 – 18:00
            return inicio >= new TimeSpan(7, 0, 0) && fin <= new TimeSpan(18, 0, 0);
        }
        else
        {
            // Jornada nocturna: 18:30 – 22:30
            return inicio >= new TimeSpan(18, 30, 0) && fin <= new TimeSpan(22, 30, 0);
        }
    }

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
    /// Calcula hora de fin: siempre 2 horas después del inicio (cada sesión dura 2h).
    /// </summary>
    private static string CalcularHoraFin(string horaInicio)
    {
        TimeSpan inicio = TimeSpan.Parse(horaInicio);
        TimeSpan fin = inicio.Add(TimeSpan.FromHours(2));
        return $"{fin.Hours:00}:{fin.Minutes:00}";
    }

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
