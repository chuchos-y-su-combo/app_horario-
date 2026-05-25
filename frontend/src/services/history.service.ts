import { AsignacionHistorial } from '../interfaces/history/AsignacionHistorial';
import { SemesterDetail } from '../interfaces/history/SemesterDetail';
import { SemesterHistory } from '../interfaces/history/SemesterHistory';
import api from './api';

/**
 * El backend retorna un string[] desde /asignaciones/periodos-historicos.
 * Este servicio transforma esa lista en SemesterHistory[] cargando el detalle
 * de cada período desde /asignaciones/consulta-historica.
 */

// Carga el detalle de asignaciones para un período y construye un SemesterHistory.
async function buildSemesterHistory(periodo: string): Promise<SemesterHistory> {
    try {
        const response = await api.get<AsignacionHistorial[]>(
            `/asignaciones/consulta-historica?periodo=${periodo}`
        );
        const asignaciones: AsignacionHistorial[] = response.data ?? [];

        const docentes   = new Set(asignaciones.map(a => a.nombreDocente)).size;
        const asignaturas = new Set(asignaciones.map(a => a.nombreAsignatura)).size;
        const escenarios  = new Set(asignaciones.map(a => a.escenario)).size;

        return {
            periodo,
            escenarios,
            docentes,
            asignaturas,
            exportaciones: 0,   // sin seguimiento de exportaciones en backend
            estado: 'Cerrado',  // si aparece en histórico ya está cerrado
        };
    } catch {
        // Si falla la consulta de detalle, devolver fila vacía.
        return { periodo, escenarios: 0, docentes: 0, asignaturas: 0, exportaciones: 0, estado: 'Cerrado' };
    }
}

export type { SemesterHistory, SemesterDetail };

export const historyService = {
    /**
     * Obtiene la lista de períodos históricos y sus métricas básicas.
     * El backend devuelve string[] → se enriquece con una llamada por período.
     */
    getPeriodosHistoricos: async (): Promise<SemesterHistory[]> => {
        const response = await api.get<string[]>('/asignaciones/periodos-historicos');
        const periodos: string[] = response.data ?? [];
        // Cargar detalles en paralelo (normalmente son pocos períodos)
        return Promise.all(periodos.map(buildSemesterHistory));
    },

    /**
     * Obtiene el detalle completo de asignaciones de un período.
     */
    getDetallePeriodo: async (periodo: string): Promise<SemesterDetail> => {
        const response = await api.get<AsignacionHistorial[]>(
            `/asignaciones/consulta-historica?periodo=${periodo}`
        );
        const asignaciones: AsignacionHistorial[] = response.data ?? [];

        const docentes   = [...new Set(asignaciones.map(a => a.nombreDocente))];
        const asignaturas = [...new Set(asignaciones.map(a => a.nombreAsignatura))];
        const escenarios  = [...new Set(asignaciones.map(a => a.escenario))];

        return { periodo, asignaturas, docentes, escenarios, asignaciones };
    },

    /**
     * Estadísticas globales derivadas de todos los períodos históricos.
     */
    getEstadisticasGlobales: async () => {
        const response = await api.get<string[]>('/asignaciones/periodos-historicos');
        const periodos: string[] = response.data ?? [];

        if (periodos.length === 0) {
            return { totalAsignaturas: 0, maxDocentes: 0, totalConflictos: 0, totalExportaciones: 0, periodos: 0 };
        }

        // Cargar detalles de todos los períodos
        const detalles = await Promise.all(
            periodos.map(p =>
                api.get<AsignacionHistorial[]>(`/asignaciones/consulta-historica?periodo=${p}`)
                    .then(r => r.data ?? [])
                    .catch(() => [] as AsignacionHistorial[])
            )
        );

        const allAsignaturas = new Set<string>();
        let maxDocentes = 0;

        for (const asignaciones of detalles) {
            const docentesPeriodo = new Set(asignaciones.map(a => a.nombreDocente)).size;
            if (docentesPeriodo > maxDocentes) maxDocentes = docentesPeriodo;
            asignaciones.forEach(a => allAsignaturas.add(a.nombreAsignatura));
        }

        return {
            totalAsignaturas: allAsignaturas.size,
            maxDocentes,
            totalConflictos: 0,   // sin endpoint de conflictos históricos
            totalExportaciones: 0,
            periodos: periodos.length,
        };
    },

    /**
     * Exporta el horario de un período como archivo Excel.
     * Reutiliza el endpoint de exportación de horarios del backend.
     */
    exportarHistorico: async (periodo: string, _formato: 'excel' | 'pdf' = 'excel'): Promise<Blob> => {
        const response = await api.get(`/horarios/exportar?periodo=${periodo}`, { responseType: 'blob' });
        return response.data;
    },
};
