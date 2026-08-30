import type { ReactNode } from "react";
import { Search, SlidersHorizontal } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

/**
 * Horizontal filter bar used on every list page (admin books, users, catalog…).
 * Purely visual for now — no filtering logic is wired up yet.
 */
export function FilterBar({
  searchPlaceholder = "Search…",
  children,
  onClear,
  onApply,
}: {
  searchPlaceholder?: string | undefined;
  /** One <FilterField> per filter control. */
  children?: ReactNode | undefined;
  onClear?: (() => void) | undefined;
  onApply?: (() => void) | undefined;
}) {
  return (
    <section className="rounded-xl border border-border bg-card p-4 shadow-card">
      <div className="flex items-center gap-3">
        <div className="relative min-w-0 flex-1">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input className="pl-9" placeholder={searchPlaceholder} aria-label={searchPlaceholder} />
        </div>
        <span className="hidden items-center gap-2 text-sm font-medium text-primary sm:inline-flex">
          <SlidersHorizontal className="h-4 w-4" /> Filters
        </span>
      </div>

      {children ? (
        <>
          <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">{children}</div>
          <div className="mt-4 flex justify-end gap-2">
            {onApply ? (
              <Button size="sm" onClick={onApply}>
                Apply filters
              </Button>
            ) : null}
            <Button variant="ghost" size="sm" className="text-primary" onClick={onClear}>
              Clear filters
            </Button>
          </div>
        </>
      ) : null}
    </section>
  );
}

/** Label + control pair inside the FilterBar grid. */
export function FilterField({
  label,
  htmlFor,
  children,
}: {
  label: string;
  htmlFor?: string | undefined;
  children: ReactNode;
}) {
  return (
    <div className="grid gap-2">
      <label htmlFor={htmlFor} className="text-sm font-medium text-muted-foreground">
        {label}
      </label>
      {children}
    </div>
  );
}
