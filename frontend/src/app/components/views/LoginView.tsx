import { useState } from "react";
import { Input } from "../Input";
import { Button } from "../Button";
import { loginRequest } from "../../../services/authService";

interface LoginViewProps {
  onLogin: () => void;
  onForgotPassword: () => void;
}

export function LoginView({ onLogin, onForgotPassword }: LoginViewProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!email || !password) {
            setError("Por favor complete todos los campos");
            return;
        }

        try {
            setLoading(true);
            setError("");

            const data = await loginRequest(email, password);

            console.log(data);

            localStorage.setItem("token", data.token);
            localStorage.setItem("usuario", JSON.stringify({
                nombreCompleto: data.nombreCompleto,
                correo: data.correo,
                rol: data.rol,
            }));

            onLogin();
        } catch (err: any) {
            console.error(err);

            setError(
                err.response?.data?.message ||
                "Credenciales incorrectas"
            );
        } finally {
            setLoading(false);
        }
    };

  return (
    <div className="w-screen h-screen flex">
      {/* Left Panel - Blue with Logo */}
      <div className="w-1/2 bg-[#003087] flex flex-col items-center justify-center px-16">
        <div className="max-w-md text-center">
          <div className="w-24 h-24 bg-white rounded-2xl flex items-center justify-center text-[#003087] font-bold text-4xl mb-8 mx-auto">
            UAM
          </div>
          <h1 className="text-white text-3xl font-medium mb-4">
            Sistema de Horarios Académicos
          </h1>
          <p className="text-white/80 text-lg">
            Gestión integral de asignación de materias y generación de horarios para la Universidad Autónoma de Manizales
          </p>
        </div>
      </div>

      {/* Right Panel - White with Form */}
      <div className="w-1/2 bg-white flex items-center justify-center px-16">
        <div className="w-full max-w-md">
          <h2 className="text-2xl font-medium text-[#333333] mb-2">
            Iniciar sesión
          </h2>
          <p className="text-[#666666] mb-8">
            Ingrese sus credenciales institucionales
          </p>

          <form onSubmit={handleSubmit} className="space-y-6">
            <Input
              label="Correo institucional"
              type="email"
              placeholder="usuario@autonoma.edu.co"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              error={error && !email ? "El correo es requerido" : undefined}
            />

            <Input
              label="Contraseña"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              error={error && !password ? "La contraseña es requerida" : undefined}
            />

            {error && (
              <div className="bg-[#C0392B]/10 border border-[#C0392B]/20 rounded p-3">
                <p className="text-sm text-[#C0392B]">{error}</p>
              </div>
            )}

            <button
              type="button"
              onClick={onForgotPassword}
              className="text-sm text-[#1A6BBF] hover:underline"
            >
              ¿Olvidó su contraseña?
            </button>

            <Button
                type="submit"
                className="w-full"
                size="lg"
                disabled={loading}
            >
                {loading ? "Ingresando..." : "Ingresar"}
            </Button>
          </form>

          <div className="mt-8 pt-8 border-t border-[#CCCCCC]">
            <p className="text-xs text-[#999999] text-center">
              © 2026 Universidad Autónoma de Manizales. Todos los derechos reservados.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
