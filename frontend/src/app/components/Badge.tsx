import { cn } from "../utils/cn";

interface BadgeProps {
  children: React.ReactNode;
  variant?: "success" | "error" | "warning" | "inactive" | "primary" | "secondary" | "info";
  className?: string;
}

export function Badge({ children, variant = "secondary", className }: BadgeProps) {
  const variants = {
    success: "bg-[#1A7A4A]/10 text-[#1A7A4A] border-[#1A7A4A]/20",
    error: "bg-[#C0392B]/10 text-[#C0392B] border-[#C0392B]/20",
    warning: "bg-[#E8A020]/10 text-[#E8A020] border-[#E8A020]/20",
    inactive: "bg-[#595959]/10 text-[#595959] border-[#595959]/20",
    primary: "bg-[#1A6BBF]/10 text-[#1A6BBF] border-[#1A6BBF]/20",
    secondary: "bg-[#F5F5F5] text-[#666666] border-[#CCCCCC]/20",
    info: "bg-[#003087]/10 text-[#003087] border-[#003087]/20",
  };

  return (
    <span
      className={cn(
        "inline-flex items-center px-2.5 py-0.5 rounded border text-sm font-medium",
        variants[variant],
        className
      )}
    >
      {children}
    </span>
  );
}
