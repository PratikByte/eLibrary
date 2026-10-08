import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AuthCard } from "@/components/AuthCard";
import { registerUser } from "@/lib/api";
import { Eye, EyeOff } from "lucide-react";
import { useState, type FormEvent } from "react";

export const Route = createFileRoute("/register")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
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
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [password, setPassword] = useState("");

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
      await navigate({ to: "/login" });
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
          <div className="relative">
            <Input
              id="password"
              name="password"
              type={showPassword ? "text" : "password"}
              autoComplete="new-password"
              className="pr-10"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
            {password ? (
              <button
                type="button"
                className="absolute inset-y-0 right-0 z-10 flex w-10 items-center justify-center text-foreground opacity-100 transition-colors hover:text-primary focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
                onClick={() => setShowPassword((visible) => !visible)}
                aria-label={showPassword ? "Hide password" : "Show password"}
                title={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            ) : null}
          </div>
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
