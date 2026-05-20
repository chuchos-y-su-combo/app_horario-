import api from "./api";

export const generationService = {
  generarPropuestas: async (data: {
    periodo: string;
    escenarios: string[];
    semestreIngenieria: number;
    borrarPropuestasPrevias: boolean;
  }) => {
    const response = await api.post("/horarios/generar-propuestas", data);
    return response.data;
  },

  obtenerPropuestas: async (periodo: string) => {
    const response = await api.get(`/asignaciones/propuestas?periodo=${periodo}`);
    return response.data;
  },

  confirmarAsignaciones: async (idsAsignacion: string[]) => {
    const response = await api.post("/asignaciones/confirmar", {
      idsAsignacion,
    });
    return response.data;
  },
};