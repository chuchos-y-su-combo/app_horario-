import api from './api';
import { ReporteCargaDocenteResponse } from '../interfaces/reports/ReporteCargaDocenteResponse';

/**
 * Servicio de reportes y exportaciones del sistema de horarios.
 * Agrupa las consultas de carga docente, exportación a Excel/PDF
 * y obtención de conflictos de asignación para el módulo de Reportes.
 */
export const reportsService = {
  /**
   * Obtiene el reporte de carga docente para un periodo académico.
   * Resume cuántas horas y asignaturas tiene asignadas cada docente,
   * permitiendo identificar sub o sobre-carga antes de confirmar el horario.
   * @param semestre Código del periodo académico, p.ej. "2025-1".
   */
  getReporteCarga: async (semestre: string): Promise<ReporteCargaDocenteResponse> => {
    const response = await api.get(`/reportes/carga-docente?semestre=${semestre}`);
    return response.data;
  },

  /**
   * Descarga el reporte de carga docente en formato Excel (.xlsx).
   * El blob resultante se convierte en un archivo descargable en el cliente.
   * @param semestre Código del periodo académico.
   * @returns Blob con el contenido del archivo Excel.
   */
  exportarReporteCargaExcel: async (semestre: string): Promise<Blob> => {
    const response = await api.get(`/excel/reporte-carga?semestre=${semestre}`, {
      responseType: 'blob'
    });
    return response.data;
  },

  /**
   * Descarga el horario semanal en formato PDF, filtrado por semestre
   * y opcionalmente por escenarios (ING_DIURNA, ING_NOCTURNA, TAPSI_DIURNA, TAPSI_NOCTURNA).
   * @param semestre Código del periodo académico.
   * @param escenarios Lista de escenarios a incluir; si se omite, incluye todos.
   * @returns Blob con el contenido del archivo PDF.
   */
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

  /**
   * Descarga el reporte de conflictos de asignación en formato Excel.
   * Los conflictos incluyen choques de docente, aula o franja en el periodo indicado.
   * @param semestre Código del periodo académico.
   * @returns Blob con el contenido del archivo Excel.
   */
  exportarConflictosExcel: async (semestre: string): Promise<Blob> => {
    const response = await api.get(`/excel/conflictos?semestre=${semestre}`, {
      responseType: 'blob'
    });
    return response.data;
  },

  /**
   * Obtiene la lista de conflictos detectados en el horario de un periodo.
   * Se usa para renderizarlos en la tabla de la vista de Reportes
   * antes de exportarlos o tomar acciones correctivas.
   * @param semestre Código del periodo académico.
   */
  getConflictos: async (semestre: string): Promise<any> => {
    const response = await api.get(`/reportes/conflictos?periodo=${semestre}`);
    return response.data;
  }
};
