import { createFileRoute, Link } from "@tanstack/react-router";
import { BookOpen, ArrowLeft, ArrowLeftRight, BookMarked } from "lucide-react";
import { UserLayout } from "@/components/UserLayout";
import { SectionCard } from "@/components/SectionCard";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { sampleBooks } from "@/lib/placeholder-data";

export const Route = createFileRoute("/books/$bookId")({
  head: () => ({
    meta: [
      { title: "Book Details — eLibrary" },
      { name: "description", content: "Book details, availability and borrowing options." },
      { property: "og:title", content: "Book Details — eLibrary" },
      { property: "og:description", content: "Book details, availability and borrowing options." },
    ],
  }),
  component: BookDetailsPage,
});

function BookDetailsPage() {
  const { bookId } = Route.useParams();
  const book = sampleBooks.find((item) => String(item.id) === bookId) ?? sampleBooks[0]!;

  return (
    <UserLayout title={book.title} subtitle={`by ${book.author}`}>
      <Button asChild variant="ghost" size="sm">
        <Link to="/catalog">
          <ArrowLeft className="h-4 w-4" /> Back to catalog
        </Link>
      </Button>

      <div className="grid gap-5 lg:grid-cols-3">
        <SectionCard className="lg:col-span-1">
          <div className="grid h-64 place-items-center rounded-lg bg-accent text-accent-foreground">
            <BookOpen className="h-12 w-12" />
          </div>
        </SectionCard>

        <SectionCard title={book.title} description={`by ${book.author}`} className="lg:col-span-2">
          <div className="flex flex-wrap items-center gap-2">
            {book.available > 0 ? (
              <Badge variant="secondary">{book.available} of {book.total} available</Badge>
            ) : (
              <Badge variant="destructive">Currently unavailable</Badge>
            )}
            <Badge variant="outline">Price {book.price}</Badge>
          </div>

          <dl className="mt-5 grid gap-4 sm:grid-cols-2">
            <div>
              <dt className="text-xs text-muted-foreground">Author</dt>
              <dd className="text-sm font-medium">{book.author}</dd>
            </div>
            <div>
              <dt className="text-xs text-muted-foreground">Copies in library</dt>
              <dd className="text-sm font-medium">{book.total}</dd>
            </div>
            <div>
              <dt className="text-xs text-muted-foreground">Loan period</dt>
              <dd className="text-sm font-medium">7 days</dd>
            </div>
            <div>
              <dt className="text-xs text-muted-foreground">Late fee</dt>
              <dd className="text-sm font-medium">10 per overdue day</dd>
            </div>
          </dl>

          <div className="mt-6 flex gap-2">
            <Button disabled={book.available === 0}>
              <ArrowLeftRight className="h-4 w-4" /> Borrow this book
            </Button>
            <Button asChild variant="outline">
              <Link to="/my-loans">
                <BookMarked className="h-4 w-4" /> My loans
              </Link>
            </Button>
          </div>
        </SectionCard>
      </div>
    </UserLayout>
  );
}
