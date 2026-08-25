import { createFileRoute } from "@tanstack/react-router";
import { BookCheck, AlertTriangle, Coins } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { ReportSection } from "@/components/ReportSection";
import { StatCard } from "@/components/StatCard";
import { sampleBooks, sampleLoans } from "@/lib/placeholder-data";

export const Route = createFileRoute("/admin/reports")({
  head: () => ({
    meta: [
      { title: "Reports — eLibrary Admin" },
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
  const available = sampleBooks.filter((book) => book.available > 0);
  const overdue = sampleLoans.filter((loan) => loan.status === "Overdue");
  const sortedByAvailability = [...available].sort((a, b) => b.available - a.available);
  const mostAvailable = sortedByAvailability[0];
  const leastAvailable = sortedByAvailability[sortedByAvailability.length - 1];

  return (
    <AdminLayout title="Reports" subtitle="Library insights at a glance">
      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard
          label="Available titles"
          value={String(available.length)}
          icon={BookCheck}
          iconBgColor="#DFF3F5"
          iconColor="#087F8C"
        />
        <StatCard
          label="Overdue loans"
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
          total: book.total,
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
          { label: "Total titles", value: String(available.length) },
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

      <ReportSection
        title="Overdue loans"
        description="Books that should have been returned"
        rows={overdue.map((loan) => ({
          title: loan.title,
          member: loan.member,
          due: loan.due,
          fine: loan.fine,
        }))}
        columns={[
          { key: "title", label: "Title" },
          { key: "member", label: "Member" },
          { key: "due", label: "Due" },
          { key: "fine", label: "Fine" },
        ]}
        chartLabelKey="title"
        chartValueKey="fine"
        fileName="overdue-loans"
        summary={[
          { label: "Overdue loans", value: String(overdue.length) },
          {
            label: "Total fines",
            value: String(overdue.reduce((sum, loan) => sum + loan.fine, 0)),
          },
          { label: "Members affected", value: String(new Set(overdue.map((l) => l.member)).size) },
        ]}
      />
    </AdminLayout>
  );
}
