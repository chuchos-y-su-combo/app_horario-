import api from "./api";

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