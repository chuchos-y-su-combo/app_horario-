import { DiaCalendario } from "./DiaCalendario";

export interface CalendarioResponse {
    semestre: string;
    idPlanFiltro?: string;
    jornadaFiltro?: string;
    dias: DiaCalendario[];
}