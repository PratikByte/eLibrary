import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { AuthCard } from "@/components/AuthCard";
import {
  sanitizeSixDigitCode,
  validatePassword,
  validateSixDigitCode,
} from "@/lib/auth-validation";
import { requestPasswordOtp, resetPasswordWithOtp, verifyPasswordOtp } from "@/lib/api";
import { Eye, EyeOff } from "lucide-react";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";

export const Route = createFileRoute("/forgot-password")({
  head: () => ({
    meta: [
      { title: "eLibrary | Digital Library" },
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
  const navigate = useNavigate();
  const [step, setStep] = useState<"email" | "otp" | "password">("email");
  const [email, setEmail] = useState("");
  const [otp, setOtp] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSendCode(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setInfo(null);
    setIsSubmitting(true);
    try {
      const result = await requestPasswordOtp(email);
      setInfo(typeof result === "string" ? result : "A security code has been sent to your email.");
      setStep("otp");
    } catch {
      // requestPasswordOtp displays request failures with the API toast.
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleVerifyCode(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (validateSixDigitCode(otp)) {
      toast.error("Enter correct One-time code get on email");
      return;
    }

    setIsSubmitting(true);
    try {
      await verifyPasswordOtp(email, otp);
      setStep("password");
    } catch {
      // verifyPasswordOtp displays the failure with the API toast.
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleReset(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setInfo(null);

    const passwordValidationError = validatePassword(newPassword);
    setPasswordError(passwordValidationError);
    if (passwordValidationError) return;

    setIsSubmitting(true);
    try {
      const result = await resetPasswordWithOtp(email, otp, newPassword);
      setInfo(typeof result === "string" ? result : "Password has been reset.");
      await navigate({ to: "/login" });
    } catch {
      // resetPasswordWithOtp displays request failures with the API toast.
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthCard title="Reset password" description="We email a six-digit code to your address">
      {step === "email" ? (
        <form className="space-y-4" onSubmit={handleSendCode}>
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </div>
          {info ? <p className="text-sm text-muted-foreground">{info}</p> : null}
          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? "Sending..." : "Send code"}
          </Button>
        </form>
      ) : step === "otp" ? (
        <form className="space-y-4" onSubmit={handleVerifyCode}>
          <div className="space-y-2">
            <Label htmlFor="otp">One-time code</Label>
            <Input
              id="otp"
              name="otp"
              type="text"
              inputMode="numeric"
              autoComplete="one-time-code"
              value={otp}
              onChange={(event) => {
                const enteredValue = event.target.value;
                const numericValue = sanitizeSixDigitCode(enteredValue);
                setOtp(numericValue);
              }}
            />
          </div>
          {info ? <p className="text-sm text-muted-foreground">{info}</p> : null}
          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? "Verifying..." : "Verify code"}
          </Button>
        </form>
      ) : (
        <form className="space-y-4" onSubmit={handleReset}>
          <div className="space-y-2">
            <Label htmlFor="new-password">New password</Label>
            <div className="relative">
              <Input
                id="new-password"
                name="newPassword"
                type={showPassword ? "text" : "password"}
                autoComplete="new-password"
                className="pr-10"
                value={newPassword}
                onChange={(event) => {
                  const value = event.target.value;
                  setNewPassword(value);
                  setPasswordError(validatePassword(value));
                }}
                required
                aria-invalid={Boolean(passwordError)}
                aria-describedby={passwordError ? "reset-password-error" : undefined}
              />
              {newPassword ? (
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
              <p id="reset-password-error" className="text-sm text-destructive">
                {passwordError}
              </p>
            ) : null}
          </div>
          {info ? <p className="text-sm text-muted-foreground">{info}</p> : null}
          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? "Resetting..." : "Reset password"}
          </Button>
        </form>
      )}

      <p className="mt-4 text-sm text-muted-foreground">
        <Link to="/login" className="text-primary hover:underline">
          Back to sign in
        </Link>
      </p>
    </AuthCard>
  );
}
