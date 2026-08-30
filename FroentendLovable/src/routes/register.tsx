import { createFileRoute, Link } from "@tanstack/react-router";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AuthCard } from "@/components/AuthCard";
import { registerUser } from "@/lib/api";
import { useState, type FormEvent } from "react";

export const Route = createFileRoute("/register")({
  head: () => ({
    meta: [
      { title: "Create Account — eLibrary" },
      { name: "description", content: "Register for an eLibrary account to borrow books online." },
      { property: "og:title", content: "Create Account — eLibrary" },
      {
        property: "og:description",
        content: "Register for an eLibrary account to borrow books online.",
      },
    ],
  }),
  component: RegisterPage,
});

function RegisterPage() {
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    const formData = new FormData(event.currentTarget);
    try {
      await registerUser(
        String(formData.get("username") ?? ""),
        String(formData.get("email") ?? ""),
        String(formData.get("password") ?? ""),
      );
      window.location.assign("/login");
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to register.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard title="Create account" description="New accounts get the User role">
      <form className="space-y-4" onSubmit={handleSubmit}>
        <div className="space-y-2">
          <Label htmlFor="username">Username</Label>
          <Input id="username" name="username" autoComplete="username" />
        </div>
        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input id="email" name="email" type="email" autoComplete="email" />
        </div>
        <div className="space-y-2">
          <Label htmlFor="password">Password</Label>
          <Input id="password" name="password" type="password" autoComplete="new-password" />
        </div>
        {error && <p className="text-sm text-destructive">{error}</p>}
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? "Registering..." : "Register"}
        </Button>
      </form>

      <p className="mt-4 text-sm text-muted-foreground">
        Already registered?{" "}
        <Link to="/login" className="text-primary hover:underline">
          Sign in
        </Link>
      </p>
    </AuthCard>
  );
}
