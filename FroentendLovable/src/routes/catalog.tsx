import { createFileRoute, Link } from "@tanstack/react-router";
import { Library } from "lucide-react";
import { UserLayout } from "@/components/UserLayout";
import { Loader } from "@/components/Loader";
import { FilterBar, FilterField } from "@/components/FilterBar";
import { ListPagination } from "@/components/list-pagination";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  borrowBook,
  DEFAULT_PAGE_SIZE,
  getAuthSession,
  getBooksByAuthor,
  getBooksByPriceRange,
  getBooksPaged,
  mapBookDto,
} from "@/lib/api";
import type { Book } from "@/lib/placeholder-data";
import { useCallback, useEffect, useState } from "react";

export const Route = createFileRoute("/catalog")({
  head: () => ({
    meta: [
      { title: "Book Catalog — eLibrary" },
      {
        name: "description",
        content: "Browse the eLibrary collection, filter by author or price, and borrow titles.",
      },
      { property: "og:title", content: "Book Catalog — eLibrary" },
      {
        property: "og:description",
        content: "Browse the eLibrary collection, filter by author or price, and borrow titles.",
      },
    ],
  }),
  component: CatalogPage,
});

function CatalogPage() {
  const [books, setBooks] = useState<Book[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [author, setAuthor] = useState("");
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [borrowingId, setBorrowingId] = useState<number | null>(null);

  const loadBooks = useCallback(
    async (
      pageNumber = 1,
      nextAuthor = author,
      nextMin = minPrice,
      nextMax = maxPrice,
    ) => {
      setIsLoading(true);
      setError(null);
      try {
        const trimmedAuthor = nextAuthor.trim();
        const min = Number(nextMin);
        const max = Number(nextMax);
        const hasPriceRange =
          nextMin !== "" && nextMax !== "" && !Number.isNaN(min) && !Number.isNaN(max);

        if (trimmedAuthor) {
          const response = await getBooksByAuthor(trimmedAuthor, pageNumber, DEFAULT_PAGE_SIZE);
          setBooks(response.items.map(mapBookDto));
          setPage(response.pageNumber || pageNumber);
          setTotalPages(response.totalPages || 1);
        } else if (hasPriceRange) {
          const data = await getBooksByPriceRange(min, max);
          setBooks(data.map(mapBookDto));
          setPage(1);
          setTotalPages(1);
        } else {
          const response = await getBooksPaged(pageNumber, DEFAULT_PAGE_SIZE);
          setBooks(response.items.map(mapBookDto));
          setPage(response.pageNumber || pageNumber);
          setTotalPages(response.totalPages || 1);
        }
      } catch (requestError) {
        setError(requestError instanceof Error ? requestError.message : "Unable to load books.");
        setBooks([]);
      } finally {
        setIsLoading(false);
      }
    },
    [author, maxPrice, minPrice],
  );

  useEffect(() => {
    void loadBooks(1, "", "", "");
    // Load the first page once on mount; later loads are triggered by filters and pagination.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function handleBorrow(book: Book) {
    const session = getAuthSession();
    if (!session) {
      window.location.assign("/login");
      return;
    }

    setBorrowingId(book.id);
    setError(null);
    setInfo(null);
    try {
      const record = await borrowBook(session.userId, book.id);
      setInfo(`Borrowed "${record.bookName ?? book.title}". Due ${record.dueDate}.`);
      await loadBooks(page);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to borrow this book.");
    } finally {
      setBorrowingId(null);
    }
  }

  return (
    <UserLayout title="Catalog" subtitle="Browse and borrow books" searchPlaceholder="Search by title…">
      <FilterBar
        searchPlaceholder="Search by title, author or ISBN…"
        onApply={() => void loadBooks(1)}
        onClear={() => {
          setAuthor("");
          setMinPrice("");
          setMaxPrice("");
          void loadBooks(1, "", "", "");
        }}
      >
        <FilterField label="Author" htmlFor="filter-author">
          <Input
            id="filter-author"
            placeholder="Any author"
            value={author}
            onChange={(event) => setAuthor(event.target.value)}
          />
        </FilterField>
        <FilterField label="Availability">
          <Select defaultValue="all">
            <SelectTrigger aria-label="Availability">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All books</SelectItem>
              <SelectItem value="available">Available</SelectItem>
              <SelectItem value="out">Unavailable</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Min price" htmlFor="filter-min">
          <Input
            id="filter-min"
            placeholder="Min"
            value={minPrice}
            onChange={(event) => setMinPrice(event.target.value)}
          />
        </FilterField>
        <FilterField label="Max price" htmlFor="filter-max">
          <Input
            id="filter-max"
            placeholder="Max"
            value={maxPrice}
            onChange={(event) => setMaxPrice(event.target.value)}
          />
        </FilterField>
      </FilterBar>

      {error ? <p className="text-sm text-destructive">{error}</p> : null}
      {info ? <p className="text-sm text-muted-foreground">{info}</p> : null}
      {isLoading ? <Loader label="Loading books..." /> : null}

      <div>
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {books.map((book) => (
            <div
              key={book.id}
              className="flex flex-col rounded-xl border border-border bg-card p-4 shadow-card"
            >
              <div className="grid h-36 place-items-center overflow-hidden rounded-lg bg-accent text-accent-foreground">
                {book.cover ? (
                  <img src={book.cover} alt={book.title} className="h-full w-full object-cover" />
                ) : (
                  <Library className="h-8 w-8" />
                )}
              </div>
              <p className="mt-3 truncate font-medium">{book.title}</p>
              <p className="truncate text-sm text-muted-foreground">{book.author}</p>
              <div className="mt-3 flex items-center justify-between">
                <span className="text-sm font-semibold">{book.price ?? "—"}</span>
                {book.available > 0 ? (
                  <Badge variant="secondary">{book.available} available</Badge>
                ) : (
                  <Badge variant="destructive">Unavailable</Badge>
                )}
              </div>
              <div className="mt-4 flex gap-2">
                <Button
                  className="flex-1"
                  disabled={book.available === 0 || borrowingId === book.id}
                  onClick={() => void handleBorrow(book)}
                >
                  {borrowingId === book.id ? "Borrowing..." : "Borrow"}
                </Button>
                <Button asChild variant="outline">
                  <Link to="/books/$bookId" params={{ bookId: String(book.id) }}>
                    Details
                  </Link>
                </Button>
              </div>
            </div>
          ))}
        </div>
        <ListPagination page={page} totalPages={totalPages} onPageChange={(next) => void loadBooks(next)} />
      </div>
    </UserLayout>
  );
}
