export interface ConflictoAlerta {
    tipoConflicto: string;
    severidad: string;
    descripcion: string;
    idDocente?: string;
    nombreDocente?: string;
    idAsignacion1?: string;
    idAsignacion2?: string;
    nombreAsignatura?: string;
    detalleHorario?: string;
}
