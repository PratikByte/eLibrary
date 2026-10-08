import type { ReactNode } from "react";
import { Search, SlidersHorizontal } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

/** Horizontal filter bar used on every list page (admin books, users, catalog…). */
export function FilterBar({
  searchPlaceholder = "Search…",
  searchValue,
  onSearchChange,
  inline = false,
  children,
  onClear,
  onApply,
}: {
  searchPlaceholder?: string | undefined;
  searchValue?: string | undefined;
  onSearchChange?: ((value: string) => void) | undefined;
  inline?: boolean | undefined;
  /** One <FilterField> per filter control. */
  children?: ReactNode | undefined;
  onClear?: (() => void) | undefined;
  onApply?: (() => void) | undefined;
}) {
  if (inline) {
    return (
      <section className="rounded-xl border border-border bg-card p-4 shadow-card">
        <div className="grid gap-4 sm:grid-cols-2 lg:flex lg:items-end">
          <div className="relative col-span-full min-w-0 lg:flex-1">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              className="pl-9"
              placeholder={searchPlaceholder}
              aria-label={searchPlaceholder}
              value={searchValue}
              onChange={onSearchChange ? (event) => onSearchChange(event.target.value) : undefined}
            />
          </div>
          {children ? (
            <div className="col-span-full grid grid-cols-1 gap-4 sm:grid-cols-2 lg:contents">
              {children}
            </div>
          ) : null}
          {onClear ? (
            <Button
              variant="ghost"
              size="sm"
              className="col-span-full justify-self-end text-primary lg:shrink-0"
              onClick={onClear}
            >
              Clear filters
            </Button>
          ) : null}
        </div>
      </section>
    );
  }

  return (
    <section className="rounded-xl border border-border bg-card p-4 shadow-card">
      <div className="flex items-center gap-3">
        <div className="relative min-w-0 flex-1">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            className="pl-9"
            placeholder={searchPlaceholder}
            aria-label={searchPlaceholder}
            value={searchValue}
            onChange={onSearchChange ? (event) => onSearchChange(event.target.value) : undefined}
          />
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
    <div className="grid min-w-0 gap-2 lg:max-w-60 lg:flex-[0_1_170px]">
      <label htmlFor={htmlFor} className="text-sm font-medium text-muted-foreground">
        {label}
      </label>
      {children}
    </div>
  );
}
