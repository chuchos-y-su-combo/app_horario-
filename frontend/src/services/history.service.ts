import {  SemesterDetail } from '../interfaces/history/SemesterDetail';
import { SemesterHistory } from '../interfaces/history/SemesterHistory';
import api from './api';

export const historyService = {
    getPeriodosHistoricos: async (): Promise<SemesterHistory[]> => {
    const response = await api.get('/asignaciones/periodos-historicos');
    return response.data;
    },

    getDetallePeriodo: async (periodo: string): Promise<SemesterDetail> => {
    const response = await api.get(`/asignaciones/consulta-historica?periodo=${periodo}`);
    return response.data;
    },

    getEstadisticasGlobales: async () => {
    const [asignaturas, periodos] = await Promise.all([
        api.get('/asignaturas'),
        api.get('/asignaciones/periodos-historicos')
    ]);
    return {
        totalAsignaturas: asignaturas.data?.length || 0,
        maxDocentes: 0,
        totalConflictos: 0,
        totalExportaciones: 0,
        periodos: periodos.data?.length || 0
    };
    },

    exportarHistorico: async (periodo: string, formato: 'excel' | 'pdf'): Promise<Blob> => {
    const response = await api.get(`/horarios/exportar?periodo=${periodo}`, { responseType: 'blob' });
    return response.data;
    }
};