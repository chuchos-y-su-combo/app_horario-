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
  LogOut,
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
  onNavigate: (view: string, state?: Record<string, string>) => void;
  onLogout: () => void;
}

function getRolFromStorage(): string {
  // Fuente primaria: objeto usuario guardado al hacer login
  try {
    const raw = localStorage.getItem("usuario");
    if (raw && raw !== "undefined") {
      const u = JSON.parse(raw);
      if (u?.rol) return u.rol as string;
    }
  } catch {}
  // Fuente secundaria: JWT — .NET puede serializar ClaimTypes.Role
  // como "role" (corto) o como la URL larga según la versión de la librería
  try {
    const token = localStorage.getItem("token");
    if (!token) return "";
    const b64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
    const payload = JSON.parse(atob(b64));
    const role =
      (payload["role"] as string | undefined) ??
      (payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] as string | undefined);
    if (role) return role;
    // "idRol" es un claim personalizado sin mapeo: "1" = Admin, "2" = Coord
    const idRol = String(payload["idRol"] ?? "");
    if (idRol === "1") return "Administrador";
    if (idRol === "2") return "Coordinador";
  } catch {}
  return "";
}

function getNombreFromStorage(): string {
  try {
    const raw = localStorage.getItem("usuario");
    if (raw && raw !== "undefined") {
      const u = JSON.parse(raw);
      return (u?.nombreCompleto as string) || "";
    }
  } catch {}
  return "";
}

export function Sidebar({ currentView, onNavigate, onLogout }: SidebarProps) {
  const rol = getRolFromStorage();
  const nombre = getNombreFromStorage();

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
          <div className="w-8 h-8 bg-[#1A6BBF] rounded-full flex items-center justify-center text-white text-sm font-medium shrink-0">
            {nombre ? nombre.charAt(0).toUpperCase() : "?"}
          </div>
          <div className="flex flex-col flex-1 min-w-0">
            <span className="text-white text-sm font-medium truncate">{nombre || "Usuario"}</span>
            <span className="text-white/70 text-xs truncate">{rol || "Sin rol"}</span>
          </div>
          <button
            onClick={onLogout}
            title="Cerrar sesión"
            className="p-1.5 rounded hover:bg-white/20 transition-colors text-white/70 hover:text-white shrink-0"
          >
            <LogOut size={16} />
          </button>
        </div>
      </div>
    </aside>
  );
}
