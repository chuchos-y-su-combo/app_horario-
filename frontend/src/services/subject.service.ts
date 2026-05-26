import { CreateSubjectRequest } from '../interfaces/subject/CreateSubjectRequest';
import { Subject } from '../interfaces/subject/Subject';

import api from './api';

/**
 * Servicio CRUD para asignaturas del plan de estudios.
 * Consume los endpoints de /asignaturas del backend.
 */
export const subjectService = {
    /** Obtiene todas las asignaturas de todos los planes de estudio. */
    getAll: async (): Promise<Subject[]> => {
    const response = await api.get('/asignaturas');
    return response.data;
    },

    /** Obtiene el detalle de una asignatura por su ID. */
    getById: async (id: string): Promise<Subject> => {
    const response = await api.get(`/asignaturas/${id}`);
    return response.data;
    },

    /** Filtra asignaturas por el ID del plan de estudios. */
    getByPlan: async (idPlan: string): Promise<Subject[]> => {
    const response = await api.get(`/asignaturas/plan/${idPlan}`);
    return response.data;
    },

    /** Crea una nueva asignatura y la asocia al plan indicado en el request. */
    create: async (data: CreateSubjectRequest): Promise<Subject> => {
    const response = await api.post('/asignaturas', data);
    return response.data;
    },

    /** Actualiza parcialmente los campos de una asignatura existente. */
    update: async (id: string, data: Partial<CreateSubjectRequest>): Promise<void> => {
    await api.put(`/asignaturas/${id}`, data);
    },

    /** Elimina permanentemente una asignatura por su ID. */
    delete: async (id: string): Promise<void> => {
    await api.delete(`/asignaturas/${id}`);
    }
};
