// services/excel.service.ts - CORRECTO, no modificar
import api from './api';

export const excelService = {
    importarCurriculo: async (file: File): Promise<{ procesadas: number; errores: string[] }> => {
        const formData = new FormData();
        formData.append('archivo', file);
        
        const response = await api.post('/profesores/curriculos/importar-excel', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        return response.data;
    },

    getAsignaturasHabilitadas: async (idProfesor: string) => {
        const response = await api.get(`/profesores/${idProfesor}/asignaturas-habilitadas`);
        return response.data;
    },

    getDisponibilidadDocente: async (idProfesor: string) => {
        const response = await api.get(`/profesores/${idProfesor}/disponibilidad`);
        return response.data;
    },

    reducirDisponibilidad: async (idDocente: string, idAsignatura: string, periodo: string) => {
        const response = await api.post(`/profesores/${idDocente}/reducir-disponibilidad?idAsignatura=${idAsignatura}&periodo=${periodo}`);
        return response.data;
    }
};