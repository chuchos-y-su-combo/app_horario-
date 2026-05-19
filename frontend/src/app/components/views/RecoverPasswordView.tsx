import { useState } from "react";
import { Input } from "../Input";
import { Button } from "../Button";
import { ArrowLeft } from "lucide-react";

interface RecoverPasswordViewProps {
  onBack: () => void;
}

export function RecoverPasswordView({ onBack }: RecoverPasswordViewProps) {
  const [email, setEmail] = useState("");
  const [code, setCode] = useState("");
  const [error, setError] = useState("");
  const [step, setStep] = useState<"email" | "code">("email");

  const handleSendCode = (e: React.FormEvent) => {
    e.preventDefault();
    if (!email) {
      setError("Por favor ingrese su correo institucional");
      return;
    }
    setError("");
    setStep("code");
  };

  const handleVerifyCode = (e: React.FormEvent) => {
    e.preventDefault();
    if (!code) {
      setError("Por favor ingrese el código de verificación");
      return;
    }
    // Simular error de código vencido
    setError("El código de verificación ha expirado. Por favor solicite uno nuevo.");
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
          <button
            onClick={onBack}
            className="flex items-center gap-2 text-[#666666] hover:text-[#333333] mb-6 transition-colors"
          >
            <ArrowLeft size={20} />
            <span>Volver al inicio de sesión</span>
          </button>

          <h2 className="text-2xl font-medium text-[#333333] mb-2">
            Recuperar contraseña
          </h2>
          <p className="text-[#666666] mb-8">
            {step === "email"
              ? "Ingrese su correo institucional para recibir el código de recuperación"
              : "Ingrese el código de verificación enviado a su correo"}
          </p>

          {step === "email" ? (
            <form onSubmit={handleSendCode} className="space-y-6">
              <Input
                label="Correo institucional"
                type="email"
                placeholder="usuario@autonoma.edu.co"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                error={error && !email ? error : undefined}
              />

              <Button type="submit" className="w-full" size="lg">
                Enviar código
              </Button>
            </form>
          ) : (
            <form onSubmit={handleVerifyCode} className="space-y-6">
              <Input
                label="Correo institucional"
                type="email"
                value={email}
                disabled
                className="bg-[#F5F5F5]"
              />

              <Input
                label="Código de verificación"
                type="text"
                placeholder="Ingrese el código de 6 dígitos"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                maxLength={6}
              />

              {error && (
                <div className="bg-[#C0392B]/10 border border-[#C0392B]/20 rounded p-3">
                  <p className="text-sm text-[#C0392B] font-medium mb-2">
                    Código vencido
                  </p>
                  <p className="text-sm text-[#C0392B]">{error}</p>
                </div>
              )}

              <div className="flex gap-3">
                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => {
                    setError("");
                    setCode("");
                  }}
                  className="flex-1"
                >
                  Reenviar enlace
                </Button>
                <Button type="submit" className="flex-1" size="lg">
                  Verificar código
                </Button>
              </div>
            </form>
          )}

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
