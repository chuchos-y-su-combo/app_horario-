import {
  LayoutDashboard,
  Users,
  GraduationCap,
  BookOpen,
  Sparkles,
  PenTool,
  Ban,
  Calendar,
  AlertTriangle,
  FileText,
  History,
  LucideIcon,
} from "lucide-react";
import { cn } from "../utils/cn";

interface SidebarItemProps {
  icon: LucideIcon;
  label: string;
  active?: boolean;
  onClick?: () => void;
}

function SidebarItem({ icon: Icon, label, active, onClick }: SidebarItemProps) {
  return (
    <button
      onClick={onClick}
      className={cn(
        "w-full flex items-center gap-3 px-5 py-3 text-white text-sm font-normal transition-colors",
        active ? "bg-[#1A6BBF]" : "hover:bg-[#1A6BBF]/50"
      )}
    >
      <Icon size={20} />
      <span>{label}</span>
    </button>
  );
}

interface SidebarProps {
  currentView: string;
  onNavigate: (view: string) => void;
}

function getRolFromToken(): string {
  const token = localStorage.getItem("token");
  if (!token) return "";
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    // .NET serializa ClaimTypes.Role como "role" en el JWT
    return (payload["role"] as string) ?? "";
  } catch {
    return "";
  }
}

export function Sidebar({ currentView, onNavigate }: SidebarProps) {
  const rol = getRolFromToken();

  const allMenuItems = [
    { id: "dashboard", icon: LayoutDashboard, label: "Dashboard", soloAdmin: false },
    { id: "usuarios", icon: Users, label: "Usuarios y roles", soloAdmin: true },
    { id: "docentes", icon: GraduationCap, label: "Docentes", soloAdmin: false },
    { id: "asignaturas", icon: BookOpen, label: "Asignaturas", soloAdmin: false },
    { id: "generacion", icon: Sparkles, label: "Generación", soloAdmin: false },
    { id: "ajuste", icon: PenTool, label: "Ajuste manual", soloAdmin: false },
    { id: "bloqueos", icon: Ban, label: "Franjas bloqueadas", soloAdmin: false },
    { id: "calendario", icon: Calendar, label: "Calendario", soloAdmin: false },
    { id: "alertas", icon: AlertTriangle, label: "Alertas", soloAdmin: false },
    { id: "reportes", icon: FileText, label: "Reportes", soloAdmin: false },
    { id: "historial", icon: History, label: "Historial", soloAdmin: false },
  ];

  const menuItems = allMenuItems.filter(
    (item) => !item.soloAdmin || rol === "Administrador"
  );

  return (
    <aside className="w-[260px] h-screen bg-[#003087] flex flex-col fixed left-0 top-0">
      {/* Logo and Title */}
      <div className="px-5 py-6 border-b border-white/20">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-white rounded flex items-center justify-center text-[#003087] font-bold text-lg">
            UAM
          </div>
          <div className="flex flex-col">
            <span className="text-white font-medium text-base">Sistema de Horarios</span>
            <span className="text-white/70 text-xs">Académicos</span>
          </div>
        </div>
      </div>

      {/* Navigation Items */}
      <nav className="flex-1 py-4 overflow-y-auto">
        {menuItems.map((item) => (
          <SidebarItem
            key={item.id}
            icon={item.icon}
            label={item.label}
            active={currentView === item.id}
            onClick={() => onNavigate(item.id)}
          />
        ))}
      </nav>

      {/* User Info */}
      <div className="px-5 py-4 border-t border-white/20">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 bg-[#1A6BBF] rounded-full flex items-center justify-center text-white text-sm font-medium">
            JD
          </div>
          <div className="flex flex-col flex-1 min-w-0">
            <span className="text-white text-sm font-medium truncate">Juan Docente</span>
            <span className="text-white/70 text-xs truncate">Administrador</span>
          </div>
        </div>
      </div>
    </aside>
  );
}
