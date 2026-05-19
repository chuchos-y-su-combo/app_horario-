import axios from 'axios';
import { CalendarioResponse } from '../interfaces/CalendarioResponse';

const API_URL = import.meta.env.VITE_URL_API_HORARIO; 

export const calendarioService = {
    // Obtener calendario semanal
    getCalendarioSemanal: async (semestre: string, idPlan?: string, jornada?: string): Promise<CalendarioResponse> => {
        const params = new URLSearchParams();
        params.append('semestre', semestre);
        if (idPlan) params.append('idPlan', idPlan);
        if (jornada) params.append('jornada', jornada);
        
        const response = await axios.get(`${API_URL}/calendario/semanal?${params}`);
        return response.data;
    },

        // Obtener calendario de un docente específico
    getCalendarioDocente: async (idDocente: string, semestre: string) => {
        const response = await axios.get(`${API_URL}/calendario/docente/
        ${idDocente}?semestre=${semestre}`);
        return response.data;
    },

    // Generar propuestas de horario
    generarPropuestas: async (periodo: string, escenarios: string[], semestreIngenieria: number) => {
    const response = await axios.post(`${API_URL}/horarios/generar-propuestas`, {
        periodo,
        escenarios,
        semestreIngenieria,
        borrarPropuestasPrevias: true
    });
    return response.data;
    }
};