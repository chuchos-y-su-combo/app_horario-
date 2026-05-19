import { DiaCalendarioAPI } from "./DiaCalendarioAPI";

export interface CalendarioResponse {
    semestre: string;
    idPlanFiltro?: string;
    jornadaFiltro?: string;
    dias: DiaCalendarioAPI[];
}
