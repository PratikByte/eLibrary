import { createFileRoute, Link } from "@tanstack/react-router";
import { Library } from "lucide-react";
import { UserLayout } from "@/components/UserLayout";
import { FilterBar, FilterField } from "@/components/FilterBar";
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
import { sampleBooks } from "@/lib/placeholder-data";


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
  return (
    <UserLayout title="Catalog" subtitle="Browse and borrow books" searchPlaceholder="Search by title…">
      <FilterBar searchPlaceholder="Search by title, author or ISBN…">
        <FilterField label="Author" htmlFor="filter-author">
          <Input id="filter-author" placeholder="Any author" />
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
          <Input id="filter-min" placeholder="Min" />
        </FilterField>
        <FilterField label="Max price" htmlFor="filter-max">
          <Input id="filter-max" placeholder="Max" />
        </FilterField>
      </FilterBar>

      <div>
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">

          {sampleBooks.map((book) => (
            <div
              key={book.id}
              className="flex flex-col rounded-xl border border-border bg-card p-4 shadow-card"
            >
              <div className="grid h-36 place-items-center rounded-lg bg-accent text-accent-foreground">
                <Library className="h-8 w-8" />
              </div>
              <p className="mt-3 truncate font-medium">{book.title}</p>
              <p className="truncate text-sm text-muted-foreground">{book.author}</p>
              <div className="mt-3 flex items-center justify-between">
                <span className="text-sm font-semibold">{book.price}</span>
                {book.available > 0 ? (
                  <Badge variant="secondary">{book.available} available</Badge>
                ) : (
                  <Badge variant="destructive">Unavailable</Badge>
                )}
              </div>
              <div className="mt-4 flex gap-2">
                <Button className="flex-1" disabled={book.available === 0}>
                  Borrow
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
      </div>
    </UserLayout>
  );
}
