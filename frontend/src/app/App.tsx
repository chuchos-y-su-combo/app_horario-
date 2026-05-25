import { useState } from "react";
import { Sidebar } from "./components/Sidebar";
import { LoginView } from "./components/views/LoginView";
import { RecoverPasswordView } from "./components/views/RecoverPasswordView";
import { DashboardView } from "./components/views/DashboardView";
import { UsersView } from "./components/views/UsersView";
import { TeachersListView } from "./components/views/TeachersListView";
import { SubjectsView } from "./components/views/SubjectsView";
import { GenerationView } from "./components/views/GenerationView";
import { AlertsView } from "./components/views/AlertsView";
import { CalendarView } from "./components/views/CalendarView";
import { ReportsView } from "./components/views/ReportsView";
import { ManualAdjustmentView } from "./components/views/ManualAdjustmentView";
import { BlockedSlotsView } from "./components/views/BlockedSlotsView";
import { HistoryView } from "./components/views/HistoryView";

type AuthState = "login" | "recover" | "authenticated";

/** Lee el rol del usuario desde localStorage (fuente primaria) o JWT (fallback). */
function getRolFromStorage(): string {
  try {
    const raw = localStorage.getItem("usuario");
    if (raw && raw !== "undefined") {
      const u = JSON.parse(raw);
      if (u?.rol) return u.rol as string;
    }
  } catch {}
  try {
    const token = localStorage.getItem("token");
    if (!token) return "";
    const b64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    const payload = JSON.parse(atob(b64));
    const role =
      (payload["role"] as string | undefined) ??
      (payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] as string | undefined);
    if (role) return role;
    const idRol = String(payload["idRol"] ?? "");
    if (idRol === "1") return "Administrador";
    if (idRol === "2") return "Coordinador";
  } catch {}
  return "";
}
type View =
  | "dashboard"
  | "usuarios"
  | "docentes"
  | "asignaturas"
  | "generacion"
  | "ajuste"
  | "bloqueos"
  | "calendario"
  | "alertas"
  | "reportes"
  | "historial";

function getInitialAuthState(): AuthState {
  const token = localStorage.getItem("token");
  if (!token) return "login";
  try {
    // JWT usa base64url (- y _); atob requiere base64 estándar (+ y /)
    const b64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    const payload = JSON.parse(atob(b64));
    if (typeof payload.exp === "number" && payload.exp * 1000 > Date.now()) {
      return "authenticated";
    }
  } catch {
    // token malformado o expirado
  }
  localStorage.removeItem("token");
  localStorage.removeItem("usuario");
  return "login";
}

export default function App() {
  const [authState, setAuthState] = useState<AuthState>(getInitialAuthState);
  const [currentView, setCurrentView] = useState<View>("dashboard");
  const [navContext, setNavContext] = useState<Record<string, string>>({});

  // Se calcula una vez al montar y no cambia durante la sesión
  const [esAdmin] = useState(() => getRolFromStorage() === "Administrador");

  const navigate = (view: string, state?: Record<string, string>) => {
    // Proteger ruta "usuarios": solo Administrador puede acceder
    if (view === "usuarios" && !esAdmin) return;
    setCurrentView(view as View);
    setNavContext(state ?? {});
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("usuario");
    setAuthState("login");
  };

  if (authState === "login") {
    return (
      <LoginView
        onLogin={() => setAuthState("authenticated")}
        onForgotPassword={() => setAuthState("recover")}
      />
    );
  }

  if (authState === "recover") {
    return <RecoverPasswordView onBack={() => setAuthState("login")} />;
  }

  return (
    <div className="w-screen h-screen flex bg-[#F5F5F5]">
      <Sidebar currentView={currentView} onNavigate={navigate} onLogout={logout} />
      <div className="flex-1 ml-[260px]">
        {currentView === "dashboard" && <DashboardView />}
        {currentView === "usuarios" && esAdmin && <UsersView />}
        {currentView === "docentes" && <TeachersListView />}
        {currentView === "asignaturas" && <SubjectsView />}
        {currentView === "generacion" && <GenerationView />}
        {currentView === "ajuste" && <ManualAdjustmentView />}
        {currentView === "bloqueos" && <BlockedSlotsView />}
        {currentView === "calendario" && <CalendarView onNavigate={navigate} />}
        {currentView === "alertas" && <AlertsView />}
        {currentView === "reportes" && <ReportsView initialSemestre={navContext.semestre} initialDocenteId={navContext.idDocente} />}
        {currentView === "historial" && <HistoryView />}
      </div>
    </div>
  );
}
