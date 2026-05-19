import { cn } from "../utils/cn";

interface ProgressBarProps {
  value: number;
  max?: number;
  showLabel?: boolean;
  variant?: "primary" | "success" | "warning" | "error";
  className?: string;
  size?: "sm" | "md" | "lg";
}

export function ProgressBar({
  value,
  max = 100,
  showLabel = false,
  variant = "primary",
  className,
  size = "md",
}: ProgressBarProps) {
  const percentage = Math.min(Math.max((value / max) * 100, 0), 100);

  const variants = {
    primary: "bg-[#1A6BBF]",
    success: "bg-[#1A7A4A]",
    warning: "bg-[#E8A020]",
    error: "bg-[#C0392B]",
  };

  const sizes = {
    sm: "h-1.5",
    md: "h-2.5",
    lg: "h-4",
  };

  return (
    <div className={cn("w-full", className)}>
      <div className={cn("w-full bg-[#F5F5F5] rounded-full overflow-hidden", sizes[size])}>
        <div
          className={cn("h-full rounded-full transition-all duration-300", variants[variant])}
          style={{ width: `${percentage}%` }}
        />
      </div>
      {showLabel && (
        <div className="mt-1 text-xs text-[#666666]">
          {value} / {max} ({percentage.toFixed(0)}%)
        </div>
      )}
    </div>
  );
}
