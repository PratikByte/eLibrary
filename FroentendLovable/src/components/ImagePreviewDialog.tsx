import { useRef } from "react";
import { ImageIcon, Upload } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";

/**
 * Enlarged image preview.
 * When `allowChange` is true a "Change image" button lets the admin pick a new
 * file. The file is only previewed locally until the API is connected.
 */
export function ImagePreviewDialog({
  open,
  onOpenChange,
  title,
  imageUrl,
  allowChange = false,
  onImageSelected,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  imageUrl?: string | undefined;
  allowChange?: boolean | undefined;
  onImageSelected?: ((previewUrl: string, file: File) => void) | undefined;
}) {
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFile = (file: File | undefined) => {
    if (!file) return;
    onImageSelected?.(URL.createObjectURL(file), file);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>
            {allowChange
              ? "Preview the current image or upload a new one."
              : "Enlarged image preview."}
          </DialogDescription>
        </DialogHeader>

        <div className="grid place-items-center rounded-lg border bg-muted/40 p-4">
          {imageUrl ? (
            <img
              src={imageUrl}
              alt={title}
              className="max-h-72 w-auto rounded-md object-contain"
            />
          ) : (
            <div className="flex h-48 w-full flex-col items-center justify-center gap-2 text-muted-foreground">
              <ImageIcon className="h-8 w-8" />
              <p className="text-sm">No image yet</p>
            </div>
          )}
        </div>

        <DialogFooter>
          {allowChange ? (
            <>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                className="hidden"
                onChange={(event) => handleFile(event.target.files?.[0])}
              />
              <Button variant="outline" onClick={() => onOpenChange(false)}>
                Close
              </Button>
              <Button type="button" onClick={() => fileInputRef.current?.click()}>
                <Upload className="h-4 w-4" /> Change image
              </Button>
            </>
          ) : (
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Close
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
