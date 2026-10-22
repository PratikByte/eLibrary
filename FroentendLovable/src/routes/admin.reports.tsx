import { useEffect, useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { BookCheck, AlertTriangle, Coins } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { ReportSection } from "@/components/ReportSection";
import { StatCard } from "@/components/StatCard";
import { ListPagination } from "@/components/list-pagination";
import {
  DEFAULT_PAGE_SIZE,
  getAvailableBooksReport,
  getOverdueBooksReport,
  mapBookDto,
  type BorrowRecordDto,
} from "@/lib/api";
import type { Book } from "@/lib/placeholder-data";

export const Route = createFileRoute("/admin/reports")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
      { name: "description", content: "Availability and overdue reports for the library collection." },
      { property: "og:title", content: "Reports — eLibrary Admin" },
      {
        property: "og:description",
        content: "Availability and overdue reports for the library collection.",
      },
    ],
  }),
  component: AdminReportsPage,
});

function AdminReportsPage() {
  const [available, setAvailable] = useState<Book[]>([]);
  const [availablePage, setAvailablePage] = useState(1);
  const [availableTotalPages, setAvailableTotalPages] = useState(1);
  const [availableTotal, setAvailableTotal] = useState(0);
  const [overdue, setOverdue] = useState<BorrowRecordDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  async function loadAvailable(pageNumber = 1) {
    const response = await getAvailableBooksReport(pageNumber, DEFAULT_PAGE_SIZE);
    setAvailable(response.items.map(mapBookDto));
    setAvailablePage(response.pageNumber || pageNumber);
    setAvailableTotalPages(response.totalPages || 1);
    setAvailableTotal(response.totalRecords);
  }

  useEffect(() => {
    let cancelled = false;
    Promise.all([loadAvailable(1), getOverdueBooksReport()])
      .then(([, overdueRows]) => {
        if (cancelled) return;
        setOverdue(overdueRows);
      })
      .catch((requestError) => {
        if (!cancelled) {
          setError(requestError instanceof Error ? requestError.message : "Unable to load reports.");
        }
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const sortedByAvailability = [...available].sort((a, b) => b.available - a.available);
  const mostAvailable = sortedByAvailability[0];
  const leastAvailable = sortedByAvailability[sortedByAvailability.length - 1];
  const overdueFines = overdue.reduce((sum, loan) => sum + (loan.fine ?? 0), 0);

  return (
    <AdminLayout title="Reports" subtitle="Library insights at a glance">
      {error ? <p className="text-sm text-destructive">{error}</p> : null}

      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard
          label="Available titles"
          value={String(availableTotal)}
          icon={BookCheck}
          iconBgColor="#DFF3F5"
          iconColor="#087F8C"
        />
        <StatCard
          label="Overdue books"
          value={String(overdue.length)}
          icon={AlertTriangle}
          iconBgColor="#FFE8E5"
          iconColor="#D94A38"
        />
        <StatCard
          label="Fines collected"
          value="1,280"
          hint="This month"
          icon={Coins}
          iconBgColor="#DDF5EE"
          iconColor="#008A72"
        />
      </div>

      <ReportSection
        title="Available books"
        description="Titles currently on the shelves"
        rows={available.map((book) => ({
          title: book.title,
          author: book.author,
          available: book.available,
          total: book.total ?? book.available,
        }))}
        columns={[
          { key: "title", label: "Title" },
          { key: "author", label: "Author" },
          { key: "available", label: "Available" },
          { key: "total", label: "Total copies" },
        ]}
        chartLabelKey="title"
        chartValueKey="available"
        fileName="available-books"
        summary={[
          { label: "Total titles", value: String(availableTotal) },
          {
            label: "Most copies",
            value: mostAvailable ? `${mostAvailable.title} (${mostAvailable.available})` : "—",
          },
          {
            label: "Fewest copies",
            value: leastAvailable ? `${leastAvailable.title} (${leastAvailable.available})` : "—",
          },
        ]}
      />
      <ListPagination
        page={availablePage}
        pageSize={DEFAULT_PAGE_SIZE}
        totalRecords={availableTotal}
        totalPages={availableTotalPages}
        onPageChange={(next) => void loadAvailable(next).catch((requestError) => {
          setError(requestError instanceof Error ? requestError.message : "Unable to load available books.");
        })}
      />

      <ReportSection
        title="Overdue Books"
        description="Books that should have been returned"
        rows={overdue.map((loan) => ({
          title: loan.bookName ?? "Unknown book",
          member: loan.userName ?? `User ${loan.userId}`,
          due: loan.dueDate,
          fine: loan.fine ?? 0,
        }))}
        columns={[
          { key: "title", label: "Title" },
          { key: "member", label: "Member" },
          { key: "due", label: "Due" },
          { key: "fine", label: "Fine" },
        ]}
        chartLabelKey="title"
        chartValueKey="fine"
        fileName="overdue-books"
        summary={[
          { label: "Overdue books", value: String(overdue.length) },
          { label: "Total fines", value: String(overdueFines) },
          {
            label: "Members affected",
            value: String(new Set(overdue.map((loan) => loan.userName ?? loan.userId)).size),
          },
        ]}
      />
    </AdminLayout>
  );
}
