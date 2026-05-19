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

  if (authState === "recover") {
    return <RecoverPasswordView onBack={() => setAuthState("login")} />;
  }

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