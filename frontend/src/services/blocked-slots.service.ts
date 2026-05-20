import api from "./api";

export const blockedSlotsService = {
  obtenerBloqueos: async (periodo: string) => {
    const response = await api.get(
      `/asignaturas/bloqueos-franja?periodo=${periodo}`
    );

    return response.data;
  },

  crearBloqueo: async (
    idAsignatura: string,
    data: {
      periodo: string;
      dia: number;
      horaInicio: string;
      horaFin: string;
      motivo?: string;
    }
  ) => {
    const response = await api.post(
      `/asignaturas/${idAsignatura}/bloqueos-franja`,
      data
    );

    return response.data;
  },

  eliminarBloqueo: async (idBloqueo: string) => {
    await api.delete(`/asignaturas/bloqueos-franja/${idBloqueo}`);
  },
};