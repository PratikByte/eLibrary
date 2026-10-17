import { createFileRoute, Link } from "@tanstack/react-router";
import { Library, Users, ArrowLeftRight, AlertTriangle, Plus, BarChart3 } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { SectionCard } from "@/components/SectionCard";
import { StatCard } from "@/components/StatCard";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  getBooksPaged,
  getOverdueBooksReport,
  getRecentActivity,
  getUsersPaged,
  type BorrowRecordDto,
  type RecentActivityDto,
} from "@/lib/api";
import { useEffect, useState } from "react";

export const Route = createFileRoute("/admin/")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
      {
        name: "description",
        content: "Library overview: inventory, members, loans and overdue books.",
      },
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
  const [recentActivity, setRecentActivity] = useState<RecentActivityDto[]>([]);
  const [isLoadingActivity, setIsLoadingActivity] = useState(true);
  const [activityError, setActivityError] = useState<string | null>(null);

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

  useEffect(() => {
    let cancelled = false;
    setIsLoadingActivity(true);
    setActivityError(null);

    getRecentActivity(4)
      .then((events) => {
        if (cancelled) return;
        setRecentActivity(events);
      })
      .catch((requestError) => {
        if (cancelled) return;
        setActivityError(
          requestError instanceof Error ? requestError.message : "Unable to load recent activity.",
        );
      })
      .finally(() => {
        if (!cancelled) setIsLoadingActivity(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  const overdueFines = overdue.reduce((sum, loan) => sum + (loan.fine ?? 0), 0);

  return (
    <AdminLayout
      title="Dashboard"
      subtitle="Overview of your library"
      searchPlaceholder="Search library…"
    >
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
        <SectionCard
          title="Recent activity"
          description="Latest borrowing and catalog changes"
          className="lg:col-span-2"
        >
          <ul className="divide-y divide-border">
            {isLoadingActivity ? (
              <li className="py-3 text-sm text-muted-foreground">Loading activity…</li>
            ) : activityError ? (
              <li className="py-3 text-sm text-destructive">{activityError}</li>
            ) : recentActivity.length === 0 ? (
              <li className="py-3 text-sm text-muted-foreground">
                There is no activity to show yet.
              </li>
            ) : (
              recentActivity.map((item) => (
                <li key={item.id} className="flex items-center justify-between gap-3 py-3 text-sm">
                  <span className="min-w-0 truncate">{getActivityText(item)}</span>
                  <span className="shrink-0 text-xs text-muted-foreground">
                    {formatActivityTime(item.occurredAt)}
                    {item.isApproximate ? " (approx.)" : ""}
                  </span>
                </li>
              ))
            )}
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

function getActivityText(activity: RecentActivityDto) {
  const memberName = activity.userName ?? "A member";
  switch (activity.eventType) {
    case "book-added":
      return `Book added: ${activity.bookName}`;
    case "book-borrowed":
      return `${memberName} borrowed ${activity.bookName}`;
    case "book-returned":
      return `${memberName} returned ${activity.bookName}`;
    case "loan-overdue":
      return `Loan became overdue: ${activity.bookName} (${memberName})`;
  }
}

function formatActivityTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Time unavailable";

  const seconds = Math.round((date.getTime() - Date.now()) / 1000);
  const elapsed = Math.abs(seconds);
  const relativeTime = new Intl.RelativeTimeFormat("en", { numeric: "auto" });
  if (elapsed < 60) return relativeTime.format(Math.round(seconds), "second");
  if (elapsed < 3600) return relativeTime.format(Math.round(seconds / 60), "minute");
  if (elapsed < 86400) return relativeTime.format(Math.round(seconds / 3600), "hour");
  if (elapsed < 2_592_000) return relativeTime.format(Math.round(seconds / 86400), "day");
  if (elapsed < 31_536_000) return relativeTime.format(Math.round(seconds / 2_592_000), "month");
  return relativeTime.format(Math.round(seconds / 31_536_000), "year");
}
