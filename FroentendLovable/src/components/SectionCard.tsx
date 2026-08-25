import type { ReactNode } from "react";

/** Rounded card with soft shadow used across all pages. */
export function SectionCard({
  title,
  description,
  action,
  children,
  className,
}: {
  title?: string | undefined;
  description?: string | undefined;
  action?: ReactNode | undefined;
  children?: ReactNode | undefined;
  className?: string | undefined;
}) {
  return (
    <section
      className={`rounded-xl border border-border bg-card p-5 shadow-card ${className ?? ""}`}
    >
      {title || action ? (
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            {title ? <h2 className="text-base font-semibold">{title}</h2> : null}
            {description ? (
              <p className="mt-1 text-sm text-muted-foreground">{description}</p>
            ) : null}
          </div>
          {action}
        </div>
      ) : null}
      {children ? <div className={title || action ? "mt-4" : ""}>{children}</div> : null}
    </section>
  );
}
