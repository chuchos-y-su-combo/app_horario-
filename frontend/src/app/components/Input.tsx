import { cn } from "../utils/cn";

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  className?: string;
}

export function Input({ label, error, className, ...props }: InputProps) {
  return (
    <div className="flex flex-col gap-2">
      {label && (
        <label className="text-sm font-medium text-[#333333]">
          {label}
        </label>
      )}
      <input
        className={cn(
          "px-3 py-2 border border-[#CCCCCC] rounded bg-white text-[#333333] placeholder:text-[#999999]",
          "focus:outline-none focus:ring-2 focus:ring-[#1A6BBF] focus:border-transparent",
          error && "border-[#C0392B] focus:ring-[#C0392B]",
          className
        )}
        {...props}
      />
      {error && (
        <span className="text-sm text-[#C0392B]">{error}</span>
      )}
    </div>
  );
}
