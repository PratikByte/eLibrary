import { useEffect, useState, type FormEvent } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { ArrowLeftRight, ChevronsUpDown, Eye, Repeat, X } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { SectionCard } from "@/components/SectionCard";
import { ListPagination } from "@/components/list-pagination";
import {
  TransactionDetailsDialog,
  type TransactionStatus,
} from "@/components/TransactionDetailsDialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Command, CommandEmpty, CommandItem, CommandList } from "@/components/ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { type Member } from "@/lib/placeholder-data";
import {
  borrowBook,
  DEFAULT_PAGE_SIZE,
  getActiveBorrowRecords,
  getBooksPaged,
  getPagedBorrowTransactions,
  getUsersPaged,
  mapBookDto,
  mapUserDto,
  returnBook,
  type BorrowTransactionDto,
} from "@/lib/api";

export const Route = createFileRoute("/admin/borrow-return")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
      {
        name: "description",
        content: "Issue books to members and record returns with due dates and fines.",
      },
      { property: "og:title", content: "Borrow & Return — eLibrary Admin" },
      {
        property: "og:description",
        content: "Issue books to members and record returns with due dates and fines.",
      },
    ],
  }),
  component: AdminBorrowReturnPage,
});

async function loadMemberOptions(searchTerm: string): Promise<PickerOption[]> {
  const response = await getUsersPaged(1, 10, searchTerm, "User");
  return (response.data ?? []).map((dto) => {
    const member: Member = mapUserDto(dto);
    return {
      id: member.id,
      label: member.name,
    };
  });
}

async function loadBookOptions(searchTerm: string): Promise<PickerOption[]> {
  const response = await getBooksPaged(1, 10, searchTerm, true);
  return response.items.map((dto) => {
    const book = mapBookDto(dto);
    return {
      id: dto.id,
      label: `${book.title} — ${book.author}`,
    };
  });
}

async function loadActiveLoanOptions(): Promise<PickerOption[]> {
  const records = await getActiveBorrowRecords();
  return records.map((record) => ({
    id: record.id,
    label: `${record.bookName} — ${record.userName} — due ${formatShortDate(record.dueDate)}${record.isOverdue ? " (overdue)" : ""}`,
    minDate: record.borrowDate.slice(0, 10),
  }));
}

function formatShortDate(value: string) {
  const [year, month, day] = value.slice(0, 10).split("-");
  return year && month && day ? `${day}-${month}-${year.slice(-2)}` : value;
}

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

function getTransactionStatus(transaction: BorrowTransactionDto): TransactionStatus {
  if (transaction.isReturned) return "Returned";
  return transaction.isOverdue ? "Overdue" : "Active";
}

function getUtcDateInputValue() {
  return new Date().toISOString().slice(0, 10);
}

function AdminBorrowReturnPage() {
  const today = getUtcDateInputValue();
  const [selectedMember, setSelectedMember] = useState<PickerOption | null>(null);
  const [selectedBook, setSelectedBook] = useState<PickerOption | null>(null);
  const [selectedLoan, setSelectedLoan] = useState<PickerOption | null>(null);
  const [borrowError, setBorrowError] = useState<string | null>(null);
  const [returnError, setReturnError] = useState<string | null>(null);
  const [returnInfo, setReturnInfo] = useState<string | null>(null);
  const [isBorrowing, setIsBorrowing] = useState(false);
  const [isReturning, setIsReturning] = useState(false);
  const [recentTransactions, setRecentTransactions] = useState<BorrowTransactionDto[]>([]);
  const [recentTransactionsPage, setRecentTransactionsPage] = useState(1);
  const [recentTransactionsTotalPages, setRecentTransactionsTotalPages] = useState(1);
  const [recentTransactionsError, setRecentTransactionsError] = useState<string | null>(null);
  const [isLoadingRecentTransactions, setIsLoadingRecentTransactions] = useState(true);
  const [recentTransactionsRefreshKey, setRecentTransactionsRefreshKey] = useState(0);
  const [selectedTransaction, setSelectedTransaction] = useState<BorrowTransactionDto | null>(null);

  useEffect(() => {
    let isCurrent = true;
    setIsLoadingRecentTransactions(true);
    setRecentTransactionsError(null);

    getPagedBorrowTransactions(recentTransactionsPage, DEFAULT_PAGE_SIZE)
      .then((response) => {
        if (isCurrent) {
          setRecentTransactions(response.items);
          setRecentTransactionsPage(response.pageNumber);
          setRecentTransactionsTotalPages(response.totalPages || 1);
        }
      })
      .catch((requestError) => {
        if (isCurrent) {
          setRecentTransactionsError(
            requestError instanceof Error
              ? requestError.message
              : "Unable to load recent transactions.",
          );
        }
      })
      .finally(() => {
        if (isCurrent) setIsLoadingRecentTransactions(false);
      });

    return () => {
      isCurrent = false;
    };
  }, [recentTransactionsPage, recentTransactionsRefreshKey]);

  async function handleBorrow(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedMember || !selectedBook) return;
    setBorrowError(null);
    setIsBorrowing(true);
    try {
      await borrowBook(selectedMember.id, selectedBook.id);
      setSelectedMember(null);
      setSelectedBook(null);
      setRecentTransactionsPage(1);
      setRecentTransactionsRefreshKey((key) => key + 1);
    } catch (requestError) {
      setBorrowError(
        requestError instanceof Error ? requestError.message : "Unable to issue this book.",
      );
    } finally {
      setIsBorrowing(false);
    }
  }

  async function handleReturn(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const formData = new FormData(form);
    setReturnError(null);
    setReturnInfo(null);
    setIsReturning(true);
    try {
      const borrowId = selectedLoan?.id;
      const returnDate = String(formData.get("returnDate") ?? "");
      if (!borrowId || !returnDate) {
        throw new Error("Select an active loan and return date.");
      }
      const record = await returnBook(borrowId, returnDate);
      const fineText = record.fine && record.fine > 0 ? ` Fine ${record.fine}.` : "";
      setReturnInfo(`Returned "${record.bookName ?? "book"}".${fineText}`);
      form.reset();
      setSelectedLoan(null);
      setRecentTransactionsPage(1);
      setRecentTransactionsRefreshKey((key) => key + 1);
    } catch (requestError) {
      setReturnError(
        requestError instanceof Error ? requestError.message : "Unable to record this return.",
      );
    } finally {
      setIsReturning(false);
    }
  }

  return (
    <AdminLayout title="Borrow / Return" subtitle="Issue and receive books">
      <div className="grid gap-5 lg:grid-cols-2">
        <SectionCard title="Issue a book" description="Loans run for 7 days from the borrow date">
          <form className="grid gap-4" onSubmit={handleBorrow}>
            <div className="grid gap-2">
              <Label htmlFor="borrow-member">Member</Label>
              <SearchablePicker
                id="borrow-member"
                placeholder="Select member"
                selected={selectedMember}
                onSelect={setSelectedMember}
                onClear={() => setSelectedMember(null)}
                loadOptions={loadMemberOptions}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="borrow-book">Book</Label>
              <SearchablePicker
                id="borrow-book"
                placeholder="Select book"
                selected={selectedBook}
                onSelect={setSelectedBook}
                onClear={() => setSelectedBook(null)}
                loadOptions={loadBookOptions}
              />
            </div>
            {borrowError ? <p className="text-sm text-destructive">{borrowError}</p> : null}
            <Button
              className="w-full"
              type="submit"
              disabled={isBorrowing || !selectedMember || !selectedBook}
            >
              <ArrowLeftRight className="h-4 w-4" /> {isBorrowing ? "Issuing..." : "Issue book"}
            </Button>
          </form>
        </SectionCard>

        <SectionCard title="Return a book" description="Late returns are fined per overdue day">
          <form className="grid gap-4" onSubmit={handleReturn}>
            <div className="grid gap-2">
              <Label htmlFor="return-loan">Active borrowed book</Label>
              <SearchablePicker
                id="return-loan"
                placeholder="Select active loan"
                selected={selectedLoan}
                onSelect={setSelectedLoan}
                onClear={() => setSelectedLoan(null)}
                loadOptions={loadActiveLoanOptions}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="return-date">Return date</Label>
              <Input
                id="return-date"
                name="returnDate"
                type="date"
                defaultValue={today}
                min={selectedLoan?.minDate}
                max={today}
                required
                className="h-11 bg-background px-4 text-base"
              />
            </div>
            {returnError ? <p className="text-sm text-destructive">{returnError}</p> : null}
            {returnInfo ? <p className="text-sm text-muted-foreground">{returnInfo}</p> : null}
            <Button
              variant="outline"
              className="w-full"
              type="submit"
              disabled={isReturning || !selectedLoan}
            >
              <Repeat className="h-4 w-4" /> {isReturning ? "Recording..." : "Record return"}
            </Button>
          </form>
        </SectionCard>
      </div>

      <SectionCard title="Recent transactions" description="Latest borrow and return records">
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Book</TableHead>
                <TableHead>Member</TableHead>
                <TableHead>Borrowed</TableHead>
                <TableHead>Due</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="w-12 px-0 text-left">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {recentTransactionsError ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center text-destructive">
                    {recentTransactionsError}
                  </TableCell>
                </TableRow>
              ) : isLoadingRecentTransactions ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center text-muted-foreground">
                    Loading recent transactions...
                  </TableCell>
                </TableRow>
              ) : recentTransactions.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center text-muted-foreground">
                    No recent transactions.
                  </TableCell>
                </TableRow>
              ) : (
                recentTransactions.map((transaction) => (
                  <TableRow key={transaction.id}>
                    <TableCell className="font-medium">{transaction.bookName}</TableCell>
                    <TableCell className="text-muted-foreground">{transaction.userName}</TableCell>
                    <TableCell>{formatTransactionDate(transaction.borrowDate)}</TableCell>
                    <TableCell>{formatTransactionDate(transaction.dueDate)}</TableCell>
                    <TableCell>
                      <LoanStatus status={getTransactionStatus(transaction)} />
                    </TableCell>
                    <TableCell className="w-12 px-0 text-left">
                      {/* Transactions support viewing details only. */}
                      <div className="flex justify-start">
                        <Button
                          type="button"
                          variant="ghost"
                          size="icon"
                          onClick={() => setSelectedTransaction(transaction)}
                          aria-label={`View transaction for ${transaction.bookName}`}
                          title="View transaction"
                          className="text-primary hover:bg-accent hover:text-primary"
                        >
                          <Eye aria-hidden="true" className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
        <ListPagination
          page={recentTransactionsPage}
          totalPages={recentTransactionsTotalPages}
          onPageChange={setRecentTransactionsPage}
        />
      </SectionCard>
      <TransactionDetailsDialog
        transaction={selectedTransaction}
        status={selectedTransaction ? getTransactionStatus(selectedTransaction) : null}
        onClose={() => setSelectedTransaction(null)}
      />
    </AdminLayout>
  );
}

type PickerOption = {
  id: string;
  label: string;
  minDate?: string;
};

function SearchablePicker({
  id,
  placeholder,
  selected,
  onSelect,
  onClear,
  loadOptions,
}: {
  id: string;
  placeholder: string;
  selected: PickerOption | null;
  onSelect: (option: PickerOption) => void;
  onClear: () => void;
  loadOptions: (searchTerm: string) => Promise<PickerOption[]>;
}) {
  const [open, setOpen] = useState(false);
  const [options, setOptions] = useState<PickerOption[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) return;
    let cancelled = false;
    setOptions([]);
    setIsLoading(true);
    setError(null);
    const timeoutId = window.setTimeout(() => {
      void loadOptions("")
        .then((results) => {
          if (!cancelled) setOptions(results);
        })
        .catch((requestError) => {
          if (!cancelled) {
            setOptions([]);
            setError(
              requestError instanceof Error ? requestError.message : "Unable to load options.",
            );
          }
        })
        .finally(() => {
          if (!cancelled) setIsLoading(false);
        });
    }, 250);
    return () => {
      cancelled = true;
      window.clearTimeout(timeoutId);
    };
  }, [loadOptions, open]);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <div className="flex w-full">
        <PopoverTrigger asChild>
          <Button
            id={id}
            type="button"
            variant="outline"
            role="combobox"
            aria-expanded={open}
            className={`h-11 min-w-0 flex-1 justify-between bg-background px-4 text-base font-normal shadow-sm ${selected ? "rounded-r-none" : ""}`}
          >
            <span className={selected ? "" : "text-muted-foreground"}>
              {selected?.label ?? placeholder}
            </span>
            <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
          </Button>
        </PopoverTrigger>
        {selected ? (
          <Button
            type="button"
            variant="outline"
            aria-label="Clear selection"
            title="Clear selection"
            className="h-11 rounded-l-none border-l-0 px-3"
            onClick={onClear}
          >
            <X className="h-4 w-4" />
          </Button>
        ) : null}
      </div>
      <PopoverContent
        align="start"
        className="w-[var(--radix-popover-trigger-width)] rounded-lg p-0 shadow-md"
      >
        <Command shouldFilter={false}>
          <CommandList>
            {isLoading ? (
              <p className="py-6 text-center text-sm text-muted-foreground">Searching...</p>
            ) : error ? (
              <p className="px-4 py-6 text-center text-sm text-destructive">{error}</p>
            ) : options.length === 0 ? (
              <CommandEmpty>No matching records found.</CommandEmpty>
            ) : (
              options.map((option) => (
                <CommandItem
                  key={option.id}
                  value={option.id}
                  className="min-h-11 px-4 py-3 text-base"
                  onSelect={() => {
                    onSelect(option);
                    setOpen(false);
                  }}
                >
                  <span className="truncate">{option.label}</span>
                </CommandItem>
              ))
            )}
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}

function LoanStatus({ status }: { status: "Active" | "Overdue" | "Returned" }) {
  if (status === "Overdue") {
    return (
      <Badge variant="outline" className="border-red-600 bg-red-50 text-red-700 hover:bg-red-50">
        Overdue
      </Badge>
    );
  }
  if (status === "Returned") {
    return (
      <Badge variant="outline" className="border-green-600 bg-green-50 text-green-700">
        Returned
      </Badge>
    );
  }
  return (
    <Badge
      variant="outline"
      className="border-yellow-400 bg-yellow-50 text-yellow-800 hover:bg-yellow-50"
    >
      Active
    </Badge>
  );
}
