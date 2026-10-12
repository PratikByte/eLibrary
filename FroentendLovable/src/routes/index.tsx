import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { BookMarked, Clock, Coins, Library, BookOpen, ArrowLeftRight, UserCircle } from "lucide-react";
import { UserLayout } from "@/components/UserLayout";
import { SectionCard } from "@/components/SectionCard";
import { StatCard } from "@/components/StatCard";
import { Loader } from "@/components/Loader";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { sampleLoans } from "@/lib/placeholder-data";
import { getAuthSession, getBooksPaged, getUserProfile, mapBookDto } from "@/lib/api";
import type { Book } from "@/lib/placeholder-data";
import { useEffect, useState } from "react";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
      {
        name: "description",
        content: "Your borrowed books, due dates and reading recommendations in one place.",
      },
      { property: "og:title", content: "My Dashboard — eLibrary" },
      {
        property: "og:description",
        content: "Your borrowed books, due dates and reading recommendations in one place.",
      },
    ],
  }),
  component: UserDashboardPage,
});

function UserDashboardPage() {
  const navigate = useNavigate();
  const [displayName, setDisplayName] = useState("reader");
  const [hasVerifiedSession, setHasVerifiedSession] = useState(false);
  const [sessionError, setSessionError] = useState<string | null>(null);
  const myLoans = sampleLoans.filter((loan) => loan.member === "Aarav Shah");
  const [recentBooks, setRecentBooks] = useState<Book[]>([]);

  useEffect(() => {
    const session = getAuthSession();
    if (!session) {
      void navigate({ to: "/login", replace: true });
      return;
    }

    let cancelled = false;
    async function verifySessionAndLoadBooks() {
      try {
        const profile = await getUserProfile(session.userId);
        if (cancelled) return;

        setDisplayName(profile.username);
        setHasVerifiedSession(true);

        try {
          const response = await getBooksPaged(1, 3);
          if (!cancelled) setRecentBooks(response.items.map(mapBookDto));
        } catch {
          if (!cancelled) setRecentBooks([]);
        }
      } catch (requestError) {
        if (!cancelled) {
          setSessionError(
            requestError instanceof Error ? requestError.message : "Unable to verify your session.",
          );
        }
      }
    }

    void verifySessionAndLoadBooks();
    return () => {
      cancelled = true;
    };
  }, [navigate]);

  if (!hasVerifiedSession) {
    if (sessionError) {
      return (
        <main className="flex min-h-screen flex-col items-center justify-center gap-4 p-6">
          <p className="text-sm text-destructive" role="alert">
            Could not verify your session: {sessionError}
          </p>
          <Button asChild variant="outline">
            <Link to="/login">Go to sign in</Link>
          </Button>
        </main>
      );
    }
    return <Loader label="Checking your session..." />;
  }

  return (
    <UserLayout title="Dashboard" subtitle={`Welcome back, ${displayName}`} searchPlaceholder="Search the catalog…">
      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard
          label="Books borrowed"
          value="2"
          hint="Limit: 5 at a time"
          icon={BookMarked}
          iconBgColor="#DFF3F5"
          iconColor="#087F8C"
        />
        <StatCard
          label="Due this week"
          value="1"
          hint="Return by 22 Aug"
          icon={Clock}
          iconBgColor="#DDF5EE"
          iconColor="#008A72"
        />
        <StatCard
          label="Outstanding fines"
          value="20"
          icon={Coins}
          iconBgColor="#FFE8E5"
          iconColor="#D94A38"
        />
      </div>

      <div className="grid gap-5 lg:grid-cols-3">
        <SectionCard
          title="Current loans"
          description="Books you have at home"
          className="lg:col-span-2"
          action={
            <Button asChild variant="outline" size="sm">
              <Link to="/my-loans">View all</Link>
            </Button>
          }
        >
          <ul className="divide-y divide-border">
            {myLoans.map((loan) => (
              <li key={loan.id} className="flex items-center justify-between gap-3 py-3">
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium">{loan.title}</p>
                  <p className="text-xs text-muted-foreground">Due {loan.due}</p>
                </div>
                {loan.status === "Overdue" ? (
                  <Badge variant="destructive">Overdue</Badge>
                ) : (
                  <Badge variant="secondary">Active</Badge>
                )}
              </li>
            ))}
          </ul>
        </SectionCard>

        <SectionCard title="Quick actions">
          <div className="grid gap-2">
            <Button asChild>
              <Link to="/catalog">
                <Library className="h-4 w-4" /> Browse catalog
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/my-loans">
                <ArrowLeftRight className="h-4 w-4" /> Return a book
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/profile">
                <UserCircle className="h-4 w-4" /> Edit profile
              </Link>
            </Button>
          </div>
        </SectionCard>
      </div>

      <SectionCard title="New in the library" description="Recently added titles">
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {recentBooks.map((book) => (
            <div key={book.id} className="flex gap-3 rounded-lg border border-border p-3">
              <span className="grid h-16 w-12 shrink-0 place-items-center overflow-hidden rounded-md bg-accent text-accent-foreground">
                {book.cover ? (
                  <img src={book.cover} alt={book.title} className="h-full w-full object-cover" />
                ) : (
                  <BookOpen className="h-5 w-5" />
                )}
              </span>
              <div className="min-w-0">
                <p className="truncate text-sm font-medium">{book.title}</p>
                <p className="truncate text-xs text-muted-foreground">{book.author}</p>
                <Button asChild variant="link" size="sm" className="h-auto p-0">
                  <Link to="/books/$bookId" params={{ bookId: String(book.id) }}>
                    View details
                  </Link>
                </Button>
              </div>
            </div>
          ))}
        </div>
      </SectionCard>
    </UserLayout>
  );
}
