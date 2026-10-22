import { Button } from "@/components/ui/button";

export type ListPaginationProps = {
  page: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export function ListPagination({
  page,
  pageSize,
  totalRecords,
  totalPages,
  onPageChange,
}: ListPaginationProps) {
  const firstRecord = totalRecords === 0 ? 0 : (page - 1) * pageSize + 1;
  const lastRecord = Math.min(page * pageSize, totalRecords);
  const recordLabel = totalRecords === 1 ? "record" : "records";

  return (
    <nav
      aria-label="List pagination"
      className="mt-4 flex flex-wrap items-center justify-between gap-2"
    >
      <span aria-live="polite" className="text-sm text-muted-foreground">
        Showing {firstRecord}-{lastRecord} of {totalRecords} {recordLabel}
      </span>
      {totalPages > 1 ? (
        <div className="flex items-center justify-end gap-2">
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={page <= 1}
            onClick={() => onPageChange(page - 1)}
          >
            Previous
          </Button>
          <span className="text-sm text-muted-foreground">
            Page {page} of {totalPages}
          </span>
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={page >= totalPages}
            onClick={() => onPageChange(page + 1)}
          >
            Next
          </Button>
        </div>
      ) : null}
    </nav>
  );
}
