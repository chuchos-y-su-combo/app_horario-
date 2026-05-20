import { ConflictoAlerta } from "./ConflictoAlerta";

export interface ConflictoAsignacionResponse {
    semestre: string;
    totalConflictos: number;
    conflictos: ConflictoAlerta[];
}
