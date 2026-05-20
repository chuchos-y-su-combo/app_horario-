import { BloqueCalendario } from "./BloqueCalendario";

export interface DiaCalendario {
    numeroDia: number;
    nombreDia: string;
    bloques: BloqueCalendario[];
}
