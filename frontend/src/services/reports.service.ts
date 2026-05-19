import api from './api';
import { ReporteCargaDocenteResponse } from '../interfaces/reports/ReporteCargaDocenteResponse';


export const reportsService = {
    // Obtener reporte de carga docente (usando endpoint existente)
    getReporteCarga: async (semestre: string): Promise<ReporteCargaDocenteResponse> => {
    const response = await api.get(`/reportes/carga-docente?semestre=${semestre}`);
    return response.data;
    },

    // Exportar reporte de carga a Excel
    exportarReporteCargaExcel: async (semestre: string): Promise<Blob> => {
    const response = await api.get(`/excel/reporte-carga?semestre=${semestre}`, {
        responseType: 'blob'
    });
    return response.data;
    },

    // Exportar horario a PDF
    exportarHorarioPDF: async (semestre: string, escenarios?: string[]): Promise<Blob> => {
    const params = new URLSearchParams();
    params.append('semestre', semestre);
    if (escenarios?.length) {
        escenarios.forEach(e => params.append('escenarios', e));
    }
    const response = await api.get(`/reportes/horario-pdf?${params}`, {
        responseType: 'blob'
    });
    return response.data;
    },

    // Exportar conflictos a Excel
    exportarConflictosExcel: async (semestre: string): Promise<Blob> => {
    const response = await api.get(`/excel/conflictos?semestre=${semestre}`, {
        responseType: 'blob'
    });
    return response.data;
    },

    // Obtener conflictos
    getConflictos: async (semestre: string): Promise<any> => {
    const response = await api.get(`/reportes/conflictos?periodo=${semestre}`);
    return response.data;
    }
};