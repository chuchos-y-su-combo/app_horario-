import { ReporteDocenteItem } from "./ReporteDocenteItem";

export interface ReporteCargaDocenteResponse {
    semestre: string;
    totalDocentes: number;
    docentesConCargaCompleta: number;
    docentesConCargaParcial: number;
    docentesConCargaExcedida: number;
    docentesSinAsignaciones: number;
    docentes: ReporteDocenteItem[];
}
