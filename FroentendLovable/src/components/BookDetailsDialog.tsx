import { useEffect, useState } from "react";
import { Library } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ImagePreviewDialog } from "@/components/ImagePreviewDialog";
import type { Book } from "@/lib/placeholder-data";

export type BookDialogMode = "view" | "edit";

/**
 * Book information card shown in a dialog.
 * - mode "view": read-only details
 * - mode "edit": price and cover can be updated; other fields are read-only
 */
export function BookDetailsDialog({
  book,
  mode,
  open,
  onOpenChange,
  onSave,
  onRemoveCover,
}: {
  book: Book | null;
  mode: BookDialogMode;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSave?: (book: Book, coverFile?: File | null) => void;
  onRemoveCover?: (book: Book) => void;
}) {
  const [draft, setDraft] = useState<Book | null>(book);
  const [coverFile, setCoverFile] = useState<File | null>(null);
  const [coverPreviewOpen, setCoverPreviewOpen] = useState(false);

  useEffect(() => {
    setDraft(book);
    setCoverFile(null);
  }, [book, mode, open]);

  if (!book || !draft) return null;

  const isEdit = mode === "edit";

  const update = (patch: Partial<Book>) =>
    setDraft((current) => (current ? { ...current, ...patch } : current));

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit book" : "Book details"}</DialogTitle>
          <DialogDescription>
            {isEdit
              ? "Update the book information and save your changes."
              : "Full information for this title."}
          </DialogDescription>
        </DialogHeader>

        <div className="rounded-lg border bg-card p-4">
          <div className="flex items-start gap-3">
            {isEdit ? (
              <button
                type="button"
                aria-label="Enlarge book cover"
                className="shrink-0 rounded-md ring-offset-background transition hover:opacity-90 focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:outline-none"
                onClick={() => setCoverPreviewOpen(true)}
              >
                {draft.cover ? (
                  <img
                    src={draft.cover}
                    alt={draft.title}
                    className="h-14 w-11 rounded-md object-cover"
                  />
                ) : (
                  <span className="grid h-14 w-11 place-items-center rounded-md bg-accent text-accent-foreground">
                    <Library className="h-5 w-5" />
                  </span>
                )}
              </button>
            ) : draft.cover ? (
              <img
                src={draft.cover}
                alt={draft.title}
                className="h-14 w-11 shrink-0 rounded-md object-cover"
              />
            ) : (
              <span className="grid h-14 w-11 shrink-0 place-items-center rounded-md bg-accent text-accent-foreground">
                <Library className="h-5 w-5" />
              </span>
            )}
            <div className="min-w-0">
              <p className="truncate text-base font-semibold">{draft.title}</p>
              <p className="text-sm text-muted-foreground">by {draft.author}</p>
              <div className="mt-2">
                {draft.available > 0 ? (
                  <Badge variant="secondary">
                    {draft.total != null && draft.total !== draft.available
                      ? `${draft.available} of ${draft.total} available`
                      : `${draft.available} available`}
                  </Badge>
                ) : (
                  <Badge variant="destructive">Out of stock</Badge>
                )}
              </div>
            </div>
          </div>

          {isEdit ? (
            <div className="mt-5 grid gap-4 sm:grid-cols-2">
              <div className="grid gap-2 sm:col-span-2">
                <Label htmlFor="book-edit-title">Title</Label>
                <Input id="book-edit-title" value={draft.title} disabled />
              </div>
              <div className="grid gap-2 sm:col-span-2">
                <Label htmlFor="book-edit-author">Author</Label>
                <Input id="book-edit-author" value={draft.author} disabled />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="book-edit-price">Price</Label>
                <Input
                  id="book-edit-price"
                  type="number"
                  value={draft.price ?? ""}
                  onChange={(event) => update({ price: Number(event.target.value) })}
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="book-edit-available">Available copies</Label>
                <Input id="book-edit-available" type="number" value={draft.available} disabled />
              </div>
            </div>
          ) : (
            <dl className="mt-5 grid gap-4 sm:grid-cols-2">
              <div>
                <dt className="text-xs text-muted-foreground">Author</dt>
                <dd className="text-sm font-medium">{draft.author}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Price</dt>
                <dd className="text-sm font-medium">{draft.price ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Available copies</dt>
                <dd className="text-sm font-medium">{draft.available}</dd>
              </div>
            </dl>
          )}
        </div>

        <DialogFooter>
          {isEdit ? (
            <>
              <Button variant="outline" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              {draft.cover && onRemoveCover ? (
                <Button variant="ghost" onClick={() => onRemoveCover(draft)}>
                  Remove cover
                </Button>
              ) : null}
              <Button
                onClick={() => {
                  onSave?.(draft, coverFile);
                  onOpenChange(false);
                }}
              >
                Save changes
              </Button>
            </>
          ) : (
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Close
            </Button>
          )}
        </DialogFooter>

        <ImagePreviewDialog
          open={coverPreviewOpen}
          onOpenChange={setCoverPreviewOpen}
          title={draft.title}
          imageUrl={draft.cover}
          allowChange={isEdit}
          onImageSelected={(previewUrl, file) => {
            setCoverFile(file);
            update({ cover: previewUrl });
          }}
        />
      </DialogContent>
    </Dialog>
  );
}
