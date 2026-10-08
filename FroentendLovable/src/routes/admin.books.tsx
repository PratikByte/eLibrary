import { useCallback, useEffect, useRef, useState, type FormEvent } from "react";
import { createFileRoute } from "@tanstack/react-router";
import { Download, Plus, Pencil, Trash2, Library, Eye } from "lucide-react";
import { AdminLayout } from "@/components/AdminLayout";
import { BookDetailsDialog, type BookDialogMode } from "@/components/BookDetailsDialog";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { SectionCard } from "@/components/SectionCard";
import { FilterBar, FilterField } from "@/components/FilterBar";
import { ListPagination } from "@/components/list-pagination";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
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
import type { Book } from "@/lib/placeholder-data";
import { exportToExcel } from "@/lib/export-excel";
import {
  addBook,
  DEFAULT_PAGE_SIZE,
  deleteBook,
  deleteBookCover,
  getBooksPaged,
  mapBookDto,
  updateBookCover,
  updateBookPrice,
} from "@/lib/api";

export const Route = createFileRoute("/admin/books")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
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
  const [books, setBooks] = useState<Book[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);
  const [searchTerm, setSearchTerm] = useState("");
  const [availability, setAvailability] = useState("all");
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [selectedBook, setSelectedBook] = useState<Book | null>(null);
  const [dialogMode, setDialogMode] = useState<BookDialogMode>("view");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [bookToDelete, setBookToDelete] = useState<Book | null>(null);
  const [addOpen, setAddOpen] = useState(false);
  const booksRequestId = useRef(0);

  const loadBooks = useCallback(
    async (
      pageNumber = 1,
      nextSearchTerm = searchTerm,
      nextAvailability = availability,
      nextMin = minPrice,
      nextMax = maxPrice,
    ) => {
      const requestId = ++booksRequestId.current;
      setError(null);
      try {
        const min = Number(nextMin);
        const max = Number(nextMax);
        const response = await getBooksPaged(
          pageNumber,
          DEFAULT_PAGE_SIZE,
          nextSearchTerm,
          nextAvailability === "all" ? undefined : nextAvailability === "available",
          nextMin === "" || Number.isNaN(min) ? undefined : min,
          nextMax === "" || Number.isNaN(max) ? undefined : max,
        );
        if (requestId !== booksRequestId.current) return;
        setBooks(response.items.map(mapBookDto));
        setPage(response.pageNumber || pageNumber);
        setTotalPages(response.totalPages || 1);
        setTotalRecords(response.totalRecords);
      } catch (requestError) {
        if (requestId !== booksRequestId.current) return;
        setError(requestError instanceof Error ? requestError.message : "Unable to load books.");
        setBooks([]);
      }
    },
    [availability, maxPrice, minPrice, searchTerm],
  );

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadBooks(1), 250);
    return () => window.clearTimeout(timeoutId);
  }, [loadBooks]);

  const openDialog = (book: Book, mode: BookDialogMode) => {
    setSelectedBook(book);
    setDialogMode(mode);
    setDialogOpen(true);
  };

  const handleDelete = async () => {
    if (!bookToDelete) return;
    try {
      await deleteBook(bookToDelete.id);
      setInfo(`Deleted "${bookToDelete.title}".`);
      setBookToDelete(null);
      await loadBooks(page);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to delete this book.");
      setBookToDelete(null);
    }
  };

  const handleExport = async () => {
    setError(null);
    try {
      const min = Number(minPrice);
      const max = Number(maxPrice);
      const exportBooks: Book[] = [];
      const firstPage = await getBooksPaged(
        1,
        100,
        searchTerm,
        availability === "all" ? undefined : availability === "available",
        minPrice === "" || Number.isNaN(min) ? undefined : min,
        maxPrice === "" || Number.isNaN(max) ? undefined : max,
      );
      exportBooks.push(...firstPage.items.map(mapBookDto));
      for (let nextPage = 2; nextPage <= firstPage.totalPages; nextPage += 1) {
        const response = await getBooksPaged(
          nextPage,
          100,
          searchTerm,
          availability === "all" ? undefined : availability === "available",
          minPrice === "" || Number.isNaN(min) ? undefined : min,
          maxPrice === "" || Number.isNaN(max) ? undefined : max,
        );
        exportBooks.push(...response.items.map(mapBookDto));
      }

      exportToExcel(
        exportBooks.map((book) => ({
          ID: book.id,
          Title: book.title,
          Author: book.author,
          Price: book.price ?? "",
          "Available copies": book.available,
        })),
        "elibrary-books",
        "Books",
      );
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to export books.");
    }
  };

  return (
    <AdminLayout title="Books" subtitle="Manage the library inventory">
      <FilterBar
        inline
        searchPlaceholder="Search by title or author…"
        searchValue={searchTerm}
        onSearchChange={setSearchTerm}
        onClear={() => {
          setSearchTerm("");
          setAvailability("all");
          setMinPrice("");
          setMaxPrice("");
        }}
      >
        <FilterField label="Availability">
          <Select value={availability} onValueChange={setAvailability}>
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
          <Input
            id="book-min"
            placeholder="Min"
            type="number"
            min="0"
            value={minPrice}
            onChange={(event) => setMinPrice(event.target.value)}
          />
        </FilterField>
        <FilterField label="Max price" htmlFor="book-max">
          <Input
            id="book-max"
            placeholder="Max"
            type="number"
            min="0"
            value={maxPrice}
            onChange={(event) => setMaxPrice(event.target.value)}
          />
        </FilterField>
      </FilterBar>

      {error ? <p className="text-sm text-destructive">{error}</p> : null}
      {info ? <p className="text-sm text-muted-foreground">{info}</p> : null}

      <SectionCard
        title="All books"
        description={`${totalRecords} titles in the catalog`}
        action={
          <div className="flex items-center gap-2">
            <Button size="sm" onClick={() => setAddOpen(true)}>
              <Plus className="h-4 w-4" /> Add book
            </Button>
            <Button variant="outline" size="sm" onClick={() => void handleExport()}>
              <Download className="h-4 w-4" /> Export
            </Button>
          </div>
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
                  <TableCell>{book.price ?? "—"}</TableCell>
                  <TableCell>
                    {book.available > 0 ? (
                      <Badge variant="secondary">{book.available} available</Badge>
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
                        onClick={() => setBookToDelete(book)}
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
        <ListPagination page={page} totalPages={totalPages} onPageChange={(next) => void loadBooks(next)} />
      </SectionCard>

      <BookDetailsDialog
        book={selectedBook}
        mode={dialogMode}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onSave={async (updated, coverFile) => {
          try {
            if (updated.price != null && updated.price > 0) {
              await updateBookPrice(updated.id, updated.price);
            }
            if (coverFile) {
              await updateBookCover(updated.id, coverFile);
            }
            setInfo(`Updated "${updated.title}".`);
            await loadBooks(page);
          } catch (requestError) {
            setError(requestError instanceof Error ? requestError.message : "Unable to update this book.");
          }
        }}
        onRemoveCover={async (book) => {
          try {
            await deleteBookCover(book.id);
            setInfo(`Removed the cover for "${book.title}".`);
            setDialogOpen(false);
            await loadBooks(page);
          } catch (requestError) {
            setError(requestError instanceof Error ? requestError.message : "Unable to remove the cover.");
          }
        }}
      />

      <AddBookDialog
        open={addOpen}
        onOpenChange={setAddOpen}
        onCreated={async () => {
          setInfo("Book added.");
          await loadBooks(1);
        }}
        onError={setError}
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
        onConfirm={() => void handleDelete()}
      />
    </AdminLayout>
  );
}

function AddBookDialog({
  open,
  onOpenChange,
  onCreated,
  onError,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated: () => Promise<void>;
  onError: (message: string) => void;
}) {
  const [isSaving, setIsSaving] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [pendingBook, setPendingBook] = useState<{
    title: string;
    author: string;
    price: number;
    publishedYear: number;
    availableCount: number;
    coverImage: File | null;
  } | null>(null);
  const formRef = useRef<HTMLFormElement | null>(null);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const coverImage = formData.get("coverImage");
    setPendingBook({
      title: String(formData.get("title") ?? ""),
      author: String(formData.get("author") ?? ""),
      price: Number(formData.get("price") ?? 0),
      publishedYear: Number(formData.get("publishedYear") ?? new Date().getFullYear()),
      availableCount: Number(formData.get("availableCount") ?? 0),
      coverImage: coverImage instanceof File && coverImage.size > 0 ? coverImage : null,
    });
    setConfirmOpen(true);
  }

  async function handleConfirmedAdd() {
    if (!pendingBook) return;
    setIsSaving(true);
    try {
      await addBook(pendingBook);
      formRef.current?.reset();
      onOpenChange(false);
      setPendingBook(null);
      await onCreated();
    } catch (requestError) {
      onError(requestError instanceof Error ? requestError.message : "Unable to add this book.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Add book</DialogTitle>
          <DialogDescription>Create a title in the library inventory.</DialogDescription>
        </DialogHeader>
        <form ref={formRef} className="grid gap-4" onSubmit={handleSubmit}>
          <div className="grid gap-2">
            <Label htmlFor="add-title">Title</Label>
            <Input id="add-title" name="title" required />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="add-author">Author</Label>
            <Input id="add-author" name="author" required />
          </div>
          <div className="grid gap-4 sm:grid-cols-3">
            <div className="grid gap-2">
              <Label htmlFor="add-price">Price</Label>
              <Input id="add-price" name="price" type="number" min="0" step="0.01" required />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="add-year">Published year</Label>
              <Input
                id="add-year"
                name="publishedYear"
                type="number"
                defaultValue={new Date().getFullYear()}
                required
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="add-count">Available copies</Label>
              <Input id="add-count" name="availableCount" type="number" min="0" defaultValue={1} required />
            </div>
          </div>
          <div className="grid gap-2">
            <Label htmlFor="add-cover">Cover image</Label>
            <Input id="add-cover" name="coverImage" type="file" accept="image/*" />
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSaving}>
              {isSaving ? "Adding..." : "Add book"}
            </Button>
          </DialogFooter>
        </form>
        <ConfirmDialog
          open={confirmOpen}
          onOpenChange={setConfirmOpen}
          title="Add book"
          description={
            pendingBook
              ? `Are you sure you want to add "${pendingBook.title}"?`
              : "Are you sure you want to add this book?"
          }
          confirmLabel="Add book"
          variant="default"
          onConfirm={() => void handleConfirmedAdd()}
        />
      </DialogContent>
    </Dialog>
  );
}
