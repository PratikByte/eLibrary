const SPECIAL_CHARACTERS = "!@#$%^&*()_+-=[]{}|;':\",./<>?";

export function validatePassword(password: string): string | null {
  if (!password) return "Password is required.";
  if (password.length < 8 || password.length > 12) {
    return "Password must be 8-12 characters long.";
  }
  if (!/[A-Z]/.test(password)) {
    return "Password must contain at least one uppercase letter.";
  }
  if (!/[a-z]/.test(password)) {
    return "Password must contain at least one lowercase letter.";
  }
  if (!/[0-9]/.test(password)) {
    return "Password must contain at least one number.";
  }
  if (![...SPECIAL_CHARACTERS].some((character) => password.includes(character))) {
    return "Password must contain at least one special character.";
  }
  return null;
}

export function validateSixDigitCode(value: string): string | null {
  if (!value) return "One-time code is required.";
  if (/[^0-9]/.test(value)) {
    return "Only numeric values are allowed.";
  }
  if (!/^[0-9]{6}$/.test(value)) {
    return "Code must be exactly 6 numeric digits.";
  }
  return null;
}

export function sanitizeSixDigitCode(value: string): string {
  return value.replace(/[^0-9]/g, "").slice(0, 6);
}
