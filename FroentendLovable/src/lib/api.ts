import { API_BASE_URL } from "./api-config";

export interface AuthSession {
  userId: number;
  username: string;
  email: string;
  role: string;
  token: string;
}

export const AUTH_SESSION_KEY = "eLibrary.session";

export function getAuthSession(): AuthSession | null {
  const value = localStorage.getItem(AUTH_SESSION_KEY);
  if (!value) return null;

  try {
    return JSON.parse(value) as AuthSession;
  } catch {
    localStorage.removeItem(AUTH_SESSION_KEY);
    return null;
  }
}

export function isAdminSession(session: AuthSession | null): boolean {
  return session?.role.trim().toLowerCase() === "admin";
}

type ApiRequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
};

export async function apiRequest<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const headers = new Headers(options.headers);
  headers.set("Accept", "application/json");

  let body: BodyInit | undefined;
  if (options.body !== undefined) {
    headers.set("Content-Type", "application/json");
    body = JSON.stringify(options.body);
  }

  const session = getAuthSession();
  const token = session?.token ?? localStorage.getItem("eLibrary.token");
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
    body: body ?? null,
  });

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`;
    try {
      const error = (await response.json()) as { message?: string; title?: string };
      message = error.message ?? error.title ?? message;
    } catch {
      // Keep the status-based message when the API has no JSON error body.
    }
    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
