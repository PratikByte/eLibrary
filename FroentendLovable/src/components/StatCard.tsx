import type { LucideIcon } from "lucide-react";

/** Small summary tile used on the dashboards. */
export function StatCard({
  label,
  value,
  hint,
  icon: Icon,
  iconBgColor,
  iconColor,
}: {
  label: string;
  value: string;
  hint?: string | undefined;
  icon: LucideIcon;
  iconBgColor?: string | undefined;
  iconColor?: string | undefined;
}) {
  return (
    <div className="rounded-xl border border-border bg-card p-5 shadow-card">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">{label}</p>
        <span
          className="grid h-9 w-9 place-items-center rounded-lg"
          style={{
            backgroundColor: iconBgColor ?? "var(--accent)",
            color: iconColor ?? "var(--accent-foreground)",
          }}
        >
          <Icon className="h-4 w-4" />
        </span>
      </div>
      <p className="mt-3 text-2xl font-semibold">{value}</p>
      {hint ? <p className="mt-1 text-xs text-muted-foreground">{hint}</p> : null}
    </div>
  );
}
