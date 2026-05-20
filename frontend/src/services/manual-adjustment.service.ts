import api from "./api";

export const manualAdjustmentService = {
  obtenerAsignaciones: async () => {
    const response = await api.get("/asignaciones");
    return response.data;
  },

  obtenerPropuestas: async (periodo: string) => {
    const response = await api.get(
      `/asignaciones/propuestas?periodo=${periodo}`
    );

    return response.data;
  },

  ajustarAsignacion: async (
    idAsignacion: string,
    data: {
      idDocente?: string;
      idAsignatura?: string;
      dia?: number;
      horaInicio?: string;
      horaFin?: string;
      periodo?: string;
    }
  ) => {
    const response = await api.patch(
      `/asignaciones/${idAsignacion}/ajustar`,
      data
    );

    return response.data;
  },

  cancelarAsignacion: async (idAsignacion: string) => {
    const response = await api.patch(
      `/asignaciones/${idAsignacion}/cancelar`
    );

    return response.data;
  },

  asignarDia: async (
    idAsignacion: string,
    data: {
      dia: number;
      horaInicio?: string;
      horaFin?: string;
    }
  ) => {
    const response = await api.patch(
      `/asignaciones/${idAsignacion}/asignar-dia`,
      data
    );

    return response.data;
  },
};
