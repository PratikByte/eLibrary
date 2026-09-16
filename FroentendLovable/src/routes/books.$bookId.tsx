import { createFileRoute, Link } from "@tanstack/react-router";
import { BookOpen, ArrowLeft, ArrowLeftRight, BookMarked } from "lucide-react";
import { UserLayout } from "@/components/UserLayout";
import { Loader } from "@/components/Loader";
import { SectionCard } from "@/components/SectionCard";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { borrowBook, getAuthSession, getBookById, mapBookDto } from "@/lib/api";
import type { Book } from "@/lib/placeholder-data";
import { useEffect, useState } from "react";

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
  const [book, setBook] = useState<Book | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [isBorrowing, setIsBorrowing] = useState(false);

  useEffect(() => {
    let cancelled = false;
    getBookById(Number(bookId))
      .then((dto) => {
        if (!cancelled) setBook(mapBookDto(dto));
      })
      .catch((requestError) => {
        if (!cancelled) {
          setError(requestError instanceof Error ? requestError.message : "Unable to load this book.");
        }
      });
    return () => {
      cancelled = true;
    };
  }, [bookId]);

  async function handleBorrow() {
    if (!book) return;
    const session = getAuthSession();
    if (!session) {
      window.location.assign("/login");
      return;
    }

    setIsBorrowing(true);
    setError(null);
    setInfo(null);
    try {
      const record = await borrowBook(session.userId, book.id);
      setInfo(`Borrowed "${record.bookName ?? book.title}". Due ${record.dueDate}.`);
      const refreshed = await getBookById(book.id);
      setBook(mapBookDto(refreshed));
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to borrow this book.");
    } finally {
      setIsBorrowing(false);
    }
  }

  return (
    <UserLayout title={book?.title ?? "Book details"} subtitle={book ? `by ${book.author}` : undefined}>
      <Button asChild variant="ghost" size="sm">
        <Link to="/catalog">
          <ArrowLeft className="h-4 w-4" /> Back to catalog
        </Link>
      </Button>

      {error ? <p className="text-sm text-destructive">{error}</p> : null}
      {info ? <p className="text-sm text-muted-foreground">{info}</p> : null}
      {!book && !error ? <Loader label="Loading book..." /> : null}

      {book ? (
        <div className="grid gap-5 lg:grid-cols-3">
          <SectionCard className="lg:col-span-1">
            <div className="grid h-64 place-items-center overflow-hidden rounded-lg bg-accent text-accent-foreground">
              {book.cover ? (
                <img src={book.cover} alt={book.title} className="h-full w-full object-cover" />
              ) : (
                <BookOpen className="h-12 w-12" />
              )}
            </div>
          </SectionCard>

          <SectionCard title={book.title} description={`by ${book.author}`} className="lg:col-span-2">
            <div className="flex flex-wrap items-center gap-2">
              {book.available > 0 ? (
                <Badge variant="secondary">{book.available} available</Badge>
              ) : (
                <Badge variant="destructive">Currently unavailable</Badge>
              )}
              <Badge variant="outline">Price {book.price ?? "—"}</Badge>
            </div>

            <dl className="mt-5 grid gap-4 sm:grid-cols-2">
              <div>
                <dt className="text-xs text-muted-foreground">Author</dt>
                <dd className="text-sm font-medium">{book.author}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Copies available</dt>
                <dd className="text-sm font-medium">{book.available}</dd>
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
              <Button disabled={book.available === 0 || isBorrowing} onClick={() => void handleBorrow()}>
                <ArrowLeftRight className="h-4 w-4" />
                {isBorrowing ? "Borrowing..." : "Borrow this book"}
              </Button>
              <Button asChild variant="outline">
                <Link to="/my-loans">
                  <BookMarked className="h-4 w-4" /> My loans
                </Link>
              </Button>
            </div>
          </SectionCard>
        </div>
      ) : null}
    </UserLayout>
  );
}
