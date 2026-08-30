import { createFileRoute, Link } from "@tanstack/react-router";
import { Library, Users, ArrowLeftRight, AlertTriangle, Plus, BarChart3 } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { SectionCard } from "@/components/SectionCard";
import { StatCard } from "@/components/StatCard";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { recentActivity } from "@/lib/placeholder-data";
import { getBooksPaged, getOverdueBooksReport, getUsersPaged, type BorrowRecordDto } from "@/lib/api";
import { useEffect, useState } from "react";

export const Route = createFileRoute("/admin/")({
  head: () => ({
    meta: [
      { title: "Admin Dashboard — eLibrary" },
      { name: "description", content: "Library overview: inventory, members, loans and overdue books." },
      { property: "og:title", content: "Admin Dashboard — eLibrary" },
      {
        property: "og:description",
        content: "Library overview: inventory, members, loans and overdue books.",
      },
    ],
  }),
  component: AdminDashboardPage,
});

function AdminDashboardPage() {
  const [bookCount, setBookCount] = useState<string>("—");
  const [memberCount, setMemberCount] = useState<string>("—");
  const [overdue, setOverdue] = useState<BorrowRecordDto[]>([]);

  useEffect(() => {
    let cancelled = false;
    Promise.all([getBooksPaged(1, 1), getUsersPaged(1, 1), getOverdueBooksReport()])
      .then(([books, users, overdueRows]) => {
        if (cancelled) return;
        setBookCount(String(books.totalRecords ?? books.data?.length ?? 0));
        setMemberCount(String(users.totalRecords ?? users.data?.length ?? 0));
        setOverdue(overdueRows);
      })
      .catch(() => {
        if (!cancelled) {
          setBookCount("—");
          setMemberCount("—");
        }
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const overdueFines = overdue.reduce((sum, loan) => sum + (loan.fine ?? 0), 0);

  return (
    <AdminLayout title="Dashboard" subtitle="Overview of your library" searchPlaceholder="Search library…">
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard
          label="Total books"
          value={bookCount}
          hint="From the catalog"
          icon={Library}
          iconBgColor="#DFF3F5"
          iconColor="#087F8C"
        />
        <StatCard
          label="Members"
          value={memberCount}
          hint="Registered accounts"
          icon={Users}
          iconBgColor="#EEE8FF"
          iconColor="#6B4FD3"
        />
        <StatCard
          label="Active loans"
          value="87"
          hint="12 due this week"
          icon={ArrowLeftRight}
          iconBgColor="#DDF5EE"
          iconColor="#008A72"
        />
        <StatCard
          label="Overdue"
          value={String(overdue.length)}
          hint={`Fines pending: ${overdueFines}`}
          icon={AlertTriangle}
          iconBgColor="#FFE8E5"
          iconColor="#D94A38"
        />
      </div>

      <div className="grid gap-5 lg:grid-cols-3">
        <SectionCard title="Recent activity" description="Latest borrowing and catalog changes" className="lg:col-span-2">
          <ul className="divide-y divide-border">
            {recentActivity.map((item) => (
              <li key={item.id} className="flex items-center justify-between gap-3 py-3 text-sm">
                <span className="min-w-0 truncate">{item.text}</span>
                <span className="shrink-0 text-xs text-muted-foreground">{item.time}</span>
              </li>
            ))}
          </ul>
        </SectionCard>

        <SectionCard title="Quick actions" description="Common librarian tasks">
          <div className="grid gap-2">
            <Button asChild>
              <Link to="/admin/books">
                <Plus className="h-4 w-4" /> Add a book
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/admin/borrow-return">
                <ArrowLeftRight className="h-4 w-4" /> Issue or return a book
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/admin/users">
                <Users className="h-4 w-4" /> Manage members
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/admin/reports">
                <BarChart3 className="h-4 w-4" /> Open reports
              </Link>
            </Button>
          </div>
        </SectionCard>
      </div>

      <SectionCard title="Overdue loans" description="Books past their return date">
        <ul className="divide-y divide-border">
          {overdue.map((loan) => (
            <li key={loan.id} className="flex items-center justify-between gap-3 py-3 text-sm">
              <div className="min-w-0">
                <p className="truncate font-medium">{loan.bookName ?? "Unknown book"}</p>
                <p className="truncate text-xs text-muted-foreground">
                  {loan.userName ?? `User ${loan.userId}`} · due {loan.dueDate}
                </p>
              </div>
              <Badge variant="destructive">Fine {loan.fine ?? 0}</Badge>
            </li>
          ))}
        </ul>
      </SectionCard>
    </AdminLayout>
  );
}
