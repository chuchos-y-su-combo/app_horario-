import { CreateSubjectRequest } from '../interfaces/subject/CreateSubjectRequest';
import { Subject } from '../interfaces/subject/Subject';

import api from './api';

export const subjectService = {
    getAll: async (): Promise<Subject[]> => {
    const response = await api.get('/asignaturas');
    return response.data;
    },

    getById: async (id: string): Promise<Subject> => {
    const response = await api.get(`/asignaturas/${id}`);
    return response.data;
    },

    getByPlan: async (idPlan: string): Promise<Subject[]> => {
    const response = await api.get(`/asignaturas/plan/${idPlan}`);
    return response.data;
    },

    create: async (data: CreateSubjectRequest): Promise<Subject> => {
    const response = await api.post('/asignaturas', data);
    return response.data;
    },

    update: async (id: string, data: Partial<CreateSubjectRequest>): Promise<void> => {
    await api.put(`/asignaturas/${id}`, data);
    },

    delete: async (id: string): Promise<void> => {
    await api.delete(`/asignaturas/${id}`);
    }
};