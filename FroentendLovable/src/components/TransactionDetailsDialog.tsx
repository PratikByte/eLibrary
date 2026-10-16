import {
  BookOpen,
  CalendarDays,
  CircleAlert,
  CircleCheck,
  Clock3,
  Coins,
  Info,
  Printer,
  UserRound,
  X,
  type LucideIcon,
} from "lucide-react";
import type { ReactNode } from "react";
import { createPortal } from "react-dom";
import type { BorrowTransactionDto } from "@/lib/api";
import { Dialog, DialogClose, DialogContent, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

export type TransactionStatus = "Active" | "Overdue" | "Returned";

const statusStyles: Record<
  TransactionStatus,
  { border: string; badge: string; icon: LucideIcon; helper: (days: number) => string }
> = {
  Active: {
    border: "border-amber-400",
    badge: "border-amber-300 bg-amber-50 text-amber-800",
    icon: Clock3,
    helper: () => "This book is currently borrowed.",
  },
  Returned: {
    border: "border-emerald-500",
    badge: "border-emerald-300 bg-emerald-50 text-emerald-700",
    icon: CircleCheck,
    helper: () => "This book has been returned successfully.",
  },
  Overdue: {
    border: "border-red-500",
    badge: "border-red-300 bg-red-50 text-red-700",
    icon: CircleAlert,
    helper: (days) => `This book is overdue by ${days} days.`,
  },
};

function formatTransactionDate(value: string) {
  const date = new Date(`${value.slice(0, 10)}T00:00:00Z`);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    timeZone: "UTC",
  }).format(date);
}

function formatFine(value: number) {
  return new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 0,
  }).format(value);
}

function TransactionPrintBill({
  transaction,
  status,
}: {
  transaction: BorrowTransactionDto;
  status: TransactionStatus;
}) {
  return (
    <article className="transaction-print-bill" data-print-bill>
      <header className="transaction-print-header">
        <p>eLibrary | Digital Library</p>
        <h1>Borrow / Return Bill</h1>
      </header>
      <section className="transaction-print-details" aria-label="Transaction details">
        <div className="transaction-print-row">
          <span>Book Title</span>
          <strong>{transaction.bookName}</strong>
        </div>
        <div className="transaction-print-row">
          <span>Author</span>
          <strong>{transaction.author}</strong>
        </div>
        <div className="transaction-print-row">
          <span>Member Name</span>
          <strong>{transaction.userName}</strong>
        </div>
        <div className="transaction-print-row">
          <span>Borrow Date</span>
          <strong>{formatTransactionDate(transaction.borrowDate)}</strong>
        </div>
        <div className="transaction-print-row">
          <span>Due Date</span>
          <strong>{formatTransactionDate(transaction.dueDate)}</strong>
        </div>
        <div className="transaction-print-row">
          <span>Return Date</span>
          <strong>
            {status === "Returned" && transaction.returnDate
              ? formatTransactionDate(transaction.returnDate)
              : "Not returned yet"}
          </strong>
        </div>
        <div className="transaction-print-row">
          <span>Status</span>
          <strong>{status}</strong>
        </div>
        <div className="transaction-print-row transaction-print-fine">
          <span>Fine</span>
          <strong>{formatFine(transaction.fine)}</strong>
        </div>
      </section>
    </article>
  );
}

function DetailRow({
  icon: Icon,
  label,
  children,
}: {
  icon: LucideIcon;
  label: string;
  children: ReactNode;
}) {
  return (
    <div className="grid grid-cols-[1.25rem_minmax(7rem,1fr)_minmax(8rem,1fr)] items-center gap-3 py-2 text-sm sm:grid-cols-[1.5rem_1fr_1fr] sm:gap-4 sm:text-base">
      <Icon aria-hidden="true" className="h-5 w-5 text-slate-500" />
      <span className="text-muted-foreground">{label}</span>
      <span className="font-medium text-foreground">{children}</span>
    </div>
  );
}

export function TransactionDetailsDialog({
  transaction,
  status,
  onClose,
}: {
  transaction: BorrowTransactionDto | null;
  status: TransactionStatus | null;
  onClose: () => void;
}) {
  const isOpen = transaction !== null && status !== null;
  const styles = status ? statusStyles[status] : statusStyles.Active;
  const StatusIcon = styles.icon;
  const helperText = status && transaction ? styles.helper(transaction.overdueDays) : "";

  return (
    <>
      <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
        {transaction && status ? (
          <DialogContent className="w-[calc(100%-2rem)] max-h-[90vh] max-w-[36rem] gap-0 overflow-y-auto rounded-2xl border-0 bg-white p-1.5 shadow-xl [&>button]:hidden">
            <div className={`overflow-hidden rounded-xl border-2 ${styles.border}`}>
              <header className="flex items-center justify-between border-b px-7 py-5">
                <DialogTitle className="text-xl font-bold sm:text-2xl">
                  Borrow Transaction Details
                </DialogTitle>
                <DialogClose asChild>
                  <button
                    type="button"
                    aria-label="Close transaction details"
                    className="rounded-md p-1 text-slate-500 transition hover:bg-slate-100 hover:text-slate-900"
                  >
                    <X className="h-5 w-5" />
                  </button>
                </DialogClose>
              </header>

              <div className="space-y-5 px-7 py-6">
                <div className="flex items-start gap-4">
                  <div className="flex h-20 w-16 shrink-0 items-center justify-center rounded-md border bg-slate-50 text-slate-500">
                    {transaction.coverImageUrl ? (
                      <img
                        src={transaction.coverImageUrl}
                        alt={`${transaction.bookName} cover`}
                        className="h-full w-full rounded-md object-cover"
                      />
                    ) : (
                      <BookOpen aria-hidden="true" className="h-8 w-8" />
                    )}
                  </div>
                  <div className="min-w-0 space-y-1">
                    <h2 className="text-lg font-bold leading-tight sm:text-xl">
                      {transaction.bookName}
                    </h2>
                    <p className="text-sm text-muted-foreground">{transaction.author}</p>
                    <Badge variant="outline" className={`mt-1 gap-1.5 px-3 py-1 ${styles.badge}`}>
                      <StatusIcon aria-hidden="true" className="h-4 w-4" />
                      {status}
                    </Badge>
                    <p className="text-sm text-muted-foreground">{helperText}</p>
                  </div>
                </div>

                <div className="border-t border-dashed pt-3">
                  <DetailRow icon={BookOpen} label="Book Title">
                    {transaction.bookName}
                  </DetailRow>
                  <DetailRow icon={Info} label="Author">
                    {transaction.author}
                  </DetailRow>
                  <DetailRow icon={UserRound} label="Member Name">
                    {transaction.userName}
                  </DetailRow>
                  <DetailRow icon={CalendarDays} label="Borrow Date">
                    {formatTransactionDate(transaction.borrowDate)}
                  </DetailRow>
                  <DetailRow icon={CalendarDays} label="Due Date">
                    {formatTransactionDate(transaction.dueDate)}
                  </DetailRow>
                  <DetailRow icon={CalendarDays} label="Return Date">
                    {status === "Returned" && transaction.returnDate
                      ? formatTransactionDate(transaction.returnDate)
                      : "Not returned yet"}
                  </DetailRow>
                  <DetailRow icon={Info} label="Status">
                    <Badge variant="outline" className={`gap-1.5 ${styles.badge}`}>
                      <StatusIcon aria-hidden="true" className="h-4 w-4" />
                      {status}
                    </Badge>
                  </DetailRow>
                  <DetailRow icon={Coins} label="Fine">
                    <span className={status === "Overdue" ? "text-red-600" : ""}>
                      {formatFine(transaction.fine)}
                    </span>
                  </DetailRow>
                </div>
              </div>

              <footer className="flex items-center justify-between border-t px-7 py-5">
                <Button type="button" variant="outline" onClick={() => window.print()}>
                  <Printer aria-hidden="true" className="h-4 w-4" />
                  Print Bill
                </Button>
                <DialogClose asChild>
                  <Button
                    type="button"
                    variant="secondary"
                    className="bg-slate-100 text-slate-800 hover:bg-slate-200"
                  >
                    Close
                  </Button>
                </DialogClose>
              </footer>
            </div>
          </DialogContent>
        ) : null}
      </Dialog>
      {transaction && status && typeof document !== "undefined"
        ? createPortal(
            <TransactionPrintBill transaction={transaction} status={status} />,
            document.body,
          )
        : null}
    </>
  );
}
