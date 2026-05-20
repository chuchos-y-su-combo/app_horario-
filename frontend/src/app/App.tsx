import { useEffect, useState } from "react";
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
import axios from "axios";

type AuthState = "login" | "recover" | "authenticated";
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

export default function App() {
    const [authState, setAuthState] = useState<AuthState>("authenticated");
  const [currentView, setCurrentView] = useState<View>("dashboard");

  if (authState === "login") {
    return (
      <LoginView
        onLogin={() => setAuthState("authenticated")}
        onForgotPassword={() => setAuthState("recover")}
      />
    );
  }
  const API_URL = import.meta.env.VITE_URL_API_HORARIO
  if (authState === "recover") {
    return <RecoverPasswordView onBack={() => setAuthState("login")} />;
  }
      useEffect(() => {
    axios.get(API_URL) // No funcionará por el manejo de autenticación, pero es solo para probar la conexión con el backend porque no hay frontend aú integrado correctamente
      .then(res => {
        console.log("Backend se estpas intentando conectar. (Caso exitoso):", res.data);
      })
      .catch(err => {
        console.error("Backend no responde. (Caso fallido):", err); // Esto es esperado si el backend tiene autenticación, pero al menos confirma que el frontend puede comunicarse con el backend. Si hay un error de conexión (como CORS o el backend no está corriendo), entonces sabremos que hay un problema de conexión.
      });
  }, []);

  return (
    <div className="w-screen h-screen flex bg-[#F5F5F5]">
      <Sidebar currentView={currentView} onNavigate={(view) => setCurrentView(view as View)} />
      <div className="flex-1 ml-[260px]">
        {currentView === "dashboard" && <DashboardView />}
        {currentView === "usuarios" && <UsersView />}
        {currentView === "docentes" && <TeachersListView />}
        {currentView === "asignaturas" && <SubjectsView />}
        {currentView === "generacion" && <GenerationView />}
        {currentView === "ajuste" && <ManualAdjustmentView />}
        {currentView === "bloqueos" && <BlockedSlotsView />}
        {currentView === "calendario" && <CalendarView />}
        {currentView === "alertas" && <AlertsView />}
        {currentView === "reportes" && <ReportsView />}
        {currentView === "historial" && <HistoryView />}
      </div>
    </div>
  );
}