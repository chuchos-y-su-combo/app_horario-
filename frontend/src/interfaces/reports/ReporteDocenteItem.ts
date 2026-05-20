import { DetalleAsignaturaReporte } from "./DetalleAsignaturaReporte";

export interface ReporteDocenteItem {
    idDocente: string;
    nombreDocente: string;
    identificacion: string;
    tipoContrato: string;
    maxAsignaturas: number;
    asignaturasAsignadas: number;
    totalHorasSemanales: number;
    horasContractuales: number;
    diferenciaHoras: number;
    porcentajeCarga: number;
    estadoCarga: string;
    semestre: string;
    asignaturas: DetalleAsignaturaReporte[];
}
