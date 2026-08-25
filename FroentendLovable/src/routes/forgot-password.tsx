import { createFileRoute, Link } from "@tanstack/react-router";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AuthCard } from "@/components/AuthCard";

export const Route = createFileRoute("/forgot-password")({
  head: () => ({
    meta: [
      { title: "Reset Password — eLibrary" },
      { name: "description", content: "Request a one-time code to reset your eLibrary password." },
      { property: "og:title", content: "Reset Password — eLibrary" },
      {
        property: "og:description",
        content: "Request a one-time code to reset your eLibrary password.",
      },
    ],
  }),
  component: ForgotPasswordPage,
});

function ForgotPasswordPage() {
  return (
    <AuthCard title="Reset password" description="We email a six-digit code to your address">
      <form className="space-y-4">
        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input id="email" name="email" type="email" autoComplete="email" />
        </div>
        <Button type="submit" className="w-full">
          Send code
        </Button>
      </form>

      <p className="mt-4 text-sm text-muted-foreground">
        <Link to="/login" className="text-primary hover:underline">
          Back to sign in
        </Link>
      </p>
    </AuthCard>
  );
}
