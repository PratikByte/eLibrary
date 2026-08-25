import { useState } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Plus, Pencil, Trash2, Library, Eye } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { BookDetailsDialog, type BookDialogMode } from "@/components/BookDetailsDialog";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { SectionCard } from "@/components/SectionCard";
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { sampleBooks, type Book } from "@/lib/placeholder-data";


export const Route = createFileRoute("/admin/books")({
  head: () => ({
    meta: [
      { title: "Manage Books — eLibrary Admin" },
      { name: "description", content: "Browse the inventory, add new titles and keep book details up to date." },
      { property: "og:title", content: "Manage Books — eLibrary Admin" },
      {
        property: "og:description",
        content: "Browse the inventory, add new titles and keep book details up to date.",
      },
    ],
  }),
  component: AdminBooksPage,
});

function AdminBooksPage() {
  // Temporary local copy of the placeholder data so edits are visible in the UI.
  const [books, setBooks] = useState<Book[]>(sampleBooks);
  const [selectedBook, setSelectedBook] = useState<Book | null>(null);
  const [dialogMode, setDialogMode] = useState<BookDialogMode>("view");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [bookToDelete, setBookToDelete] = useState<Book | null>(null);

  const openDialog = (book: Book, mode: BookDialogMode) => {
    setSelectedBook(book);
    setDialogMode(mode);
    setDialogOpen(true);
  };

  const confirmDelete = (book: Book) => {
    setBookToDelete(book);
  };

  const handleDelete = () => {
    if (!bookToDelete) return;
    setBooks((current) => current.filter((item) => item.id !== bookToDelete.id));
    setBookToDelete(null);
  };

  return (
    <AdminLayout title="Books" subtitle="Manage the library inventory" searchPlaceholder="Search books…">
      <FilterBar searchPlaceholder="Search by title, author or ISBN…">
        <FilterField label="Author" htmlFor="book-author">
          <Input id="book-author" placeholder="Any author" />
        </FilterField>
        <FilterField label="Availability">
          <Select defaultValue="all">
            <SelectTrigger aria-label="Availability">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All books</SelectItem>
              <SelectItem value="available">Available</SelectItem>
              <SelectItem value="out">Out of stock</SelectItem>
            </SelectContent>
          </Select>
        </FilterField>
        <FilterField label="Min price" htmlFor="book-min">
          <Input id="book-min" placeholder="Min" />
        </FilterField>
        <FilterField label="Max price" htmlFor="book-max">
          <Input id="book-max" placeholder="Max" />
        </FilterField>
      </FilterBar>

      <SectionCard

        title="All books"
        description={`${books.length} titles in the catalog`}
        action={
          <Button size="sm">
            <Plus className="h-4 w-4" /> Add book
          </Button>
        }
      >
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Book</TableHead>
                <TableHead>Author</TableHead>
                <TableHead>Price</TableHead>
                <TableHead>Availability</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {books.map((book) => (
                <TableRow key={book.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      {book.cover ? (
                        <img
                          src={book.cover}
                          alt={book.title}
                          className="h-11 w-9 shrink-0 rounded-md object-cover"
                        />
                      ) : (
                        <span className="grid h-11 w-9 shrink-0 place-items-center rounded-md bg-accent text-accent-foreground">
                          <Library className="h-4 w-4" />
                        </span>
                      )}
                      <span className="font-medium">{book.title}</span>
                    </div>
                  </TableCell>
                  <TableCell className="text-muted-foreground">{book.author}</TableCell>
                  <TableCell>{book.price}</TableCell>
                  <TableCell>
                    {book.available > 0 ? (
                      <Badge variant="secondary">
                        {book.available} of {book.total} available
                      </Badge>
                    ) : (
                      <Badge variant="destructive">Out of stock</Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-1">
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label="View book"
                        className="text-primary hover:bg-accent hover:text-primary"
                        onClick={() => openDialog(book, "view")}
                      >
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label="Edit book"
                        className="text-foreground hover:bg-accent hover:text-foreground"
                        onClick={() => openDialog(book, "edit")}
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label="Delete book"
                        className="text-destructive hover:bg-destructive/10 hover:text-destructive"
                        onClick={() => confirmDelete(book)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>

                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </SectionCard>

      <BookDetailsDialog
        book={selectedBook}
        mode={dialogMode}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onSave={(updated) =>
          setBooks((current) =>
            current.map((item) => (item.id === updated.id ? updated : item)),
          )
        }
      />

      <ConfirmDialog
        open={bookToDelete !== null}
        onOpenChange={(open) => {
          if (!open) setBookToDelete(null);
        }}
        title="Delete book"
        description={
          bookToDelete
            ? `Are you sure you want to delete "${bookToDelete.title}"? This action cannot be undone.`
            : "Are you sure you want to delete this book? This action cannot be undone."
        }
        confirmLabel="Delete"
        cancelLabel="Cancel"
        variant="destructive"
        onConfirm={handleDelete}
      />
    </AdminLayout>
  );
}
