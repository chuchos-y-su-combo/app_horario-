import { CalendarioResponse } from '../interfaces/Calendario/CalendarioResponse';
import api from './api';

export const calendarioService = {
    getCalendarioSemanal: async (
        semestre: string,
        idPlan?: string,
        jornada?: string,
        semestreAsignatura?: number,
    ): Promise<CalendarioResponse> => {
        const params = new URLSearchParams();
        params.append('semestre', semestre);
        if (idPlan) params.append('idPlan', idPlan);
        if (jornada) params.append('jornada', jornada);
        if (semestreAsignatura && semestreAsignatura > 0)
            params.append('semestreAsignatura', String(semestreAsignatura));
        const response = await api.get(`/horarios/calendario?${params}`);
        return response.data;
    },

    getCalendarioDocente: async (idDocente: string, semestre: string) => {
    const response = await api.get(`/horarios/calendario/docente/${idDocente}?semestre=${semestre}`);
    return response.data;
    },

    getDocentes: async () => {
    const response = await api.get('/profesores');
    return response.data;
    },

    generarPropuestas: async (periodo: string, escenarios: string[], semestreIngenieria: number) => {
    const response = await api.post('/horarios/generar-propuestas', {
        periodo,
        escenarios,
        semestreIngenieria,
        borrarPropuestasPrevias: true
    });
    return response.data;
    },

    getAsignaturas: async () => {
    const response = await api.get('/asignaturas');
    return response.data;
    }
};