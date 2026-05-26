import api from "./api";

/**
 * Envía las credenciales al endpoint de login y devuelve el JWT con datos del usuario.
 * @param email Correo institucional del usuario.
 * @param password Contraseña en texto plano (transmitida por HTTPS).
 * @returns Token JWT, nombreCompleto, correo y rol del usuario autenticado.
 * @throws AxiosError con status 401 si las credenciales son incorrectas.
 */
export const loginRequest = async (
    email: string,
    password: string
) => {
    const response = await api.post("/auth/login", {
        correo: email,
        password,
    });

    return response.data;
};
