export interface CreateSubjectRequest {
    idPlan: string;
    codigo: string;
    nombre: string;
    creditos: number;
    semestre: number;
    minEstudiantes: number;
    aula?: string;
    esFijaTapsi?: boolean;
    esOpcionalTapsiDiurna?: boolean;
}
