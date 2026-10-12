import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AuthCard } from "@/components/AuthCard";
import { Loader } from "@/components/Loader";
import { validatePassword } from "@/lib/auth-validation";
import { login } from "@/lib/api";
import { Eye, EyeOff } from "lucide-react";
import { useState, type FormEvent } from "react";

export const Route = createFileRoute("/login")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
      {
        name: "description",
        content: "Sign in to your eLibrary account to borrow and manage books.",
      },
      { property: "og:title", content: "Sign In — eLibrary" },
      {
        property: "og:description",
        content: "Sign in to your eLibrary account to borrow and manage books.",
      },
    ],
  }),
  component: LoginPage,
});

function LoginPage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [password, setPassword] = useState("");
  const [passwordError, setPasswordError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    const formData = new FormData(event.currentTarget);
    const submittedPassword = String(formData.get("password") ?? "");
    const validationError = validatePassword(submittedPassword);
    setPasswordError(validationError);
    if (validationError) return;

    setIsSubmitting(true);
    try {
      const response = await login(
        String(formData.get("username") ?? ""),
        submittedPassword,
      );
      if (response.role.toLowerCase() === "admin") {
        await navigate({ to: "/admin" });
      } else {
        await navigate({ to: "/" });
      }
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : "Unable to sign in.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard title="Sign in" description="Use your library username and password">
      {isSubmitting ? (
        <Loader label="Login..." />
      ) : (
        <form className="space-y-4" onSubmit={handleSubmit}>
        <div className="space-y-2">
          <Label htmlFor="username">Username</Label>
          <Input id="username" name="username" autoComplete="username" required />
        </div>
        <div className="space-y-2">
          <Label htmlFor="password">Password</Label>
          <div className="relative">
            <Input
              id="password"
              name="password"
              type={showPassword ? "text" : "password"}
              autoComplete="current-password"
              className="pr-10"
              value={password}
              onChange={(event) => {
                const value = event.target.value;
                setPassword(value);
                setPasswordError(validatePassword(value));
              }}
              required
              aria-invalid={Boolean(passwordError)}
              aria-describedby={passwordError ? "login-password-error" : undefined}
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
          {passwordError ? (
            <p id="login-password-error" className="text-sm text-destructive">
              {passwordError}
            </p>
          ) : null}
        </div>
        {error && <p className="text-sm text-destructive">{error}</p>}
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? "Signing in..." : "Sign in"}
        </Button>
        </form>
      )}

      {!isSubmitting ? (
        <div className="mt-4 flex justify-between text-sm">
          <Link to="/forgot-password" className="text-primary hover:underline">
            Forgot password
          </Link>
          <Link to="/register" className="text-primary hover:underline">
            Create account
          </Link>
        </div>
      ) : null}
    </AuthCard>
  );
}
