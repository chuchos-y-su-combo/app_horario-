import { AsignacionHistorial } from './AsignacionHistorial';   

export interface SemesterDetail {
    periodo: string;
    asignaturas: any[];
    docentes: any[];
    escenarios: any[];
    asignaciones: AsignacionHistorial[];
}
