export interface Subject {
    idAsignatura: string;
    idPlan: string;
    codigo: string;
    nombre: string;
    creditos: number;
    semestre: number;
    minEstudiantes: number;
    esFijaTapsi?: boolean;
    esOpcionalTapsiDiurna?: boolean;
}
