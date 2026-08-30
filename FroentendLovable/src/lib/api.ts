import { API_BASE_URL } from "./api-config";
import type { Book, Member } from "./placeholder-data";

export interface AuthSession {
  userId: number;
  username: string;
  email: string;
  role: string;
  token: string;
}

export const AUTH_SESSION_KEY = "eLibrary.session";
export const DEFAULT_PAGE_SIZE = 5;

export type ApiResponse<T> = {
  success: boolean;
  message?: string | null;
  warning?: string | null;
  data?: T | null;
  errors?: string[] | null;
};

export type PagedResponse<T> = ApiResponse<T[]> & {
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
};

export type BookDto = {
  id: number;
  title: string;
  author: string;
  availableCount: number;
  coverImageUrl?: string | null;
  price?: number;
};

export type UserDto = {
  userId: number;
  username: string;
  email: string;
  role: string;
};

export type UserProfileResponseDto = UserDto & {
  profileImageUrl?: string | null;
};

export type BorrowRecordDto = {
  id: number;
  userId: number;
  userName?: string | null;
  bookId: number;
  bookName?: string | null;
  borrowDate: string;
  dueDate: string;
  returnDate?: string | null;
  isReturned: boolean;
  fine?: number | null;
};

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

export function setAuthSession(session: AuthSession) {
  localStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session));
  localStorage.setItem("eLibrary.token", session.token);
}

export function clearAuthSession() {
  localStorage.removeItem(AUTH_SESSION_KEY);
  localStorage.removeItem("eLibrary.token");
}

export function isAdminSession(session: AuthSession | null): boolean {
  return session?.role.trim().toLowerCase() === "admin";
}

export function mapBookDto(dto: BookDto): Book {
  const book: Book = {
    id: dto.id,
    title: dto.title,
    author: dto.author,
    total: dto.availableCount,
    available: dto.availableCount,
  };
  if (dto.price != null) book.price = dto.price;
  if (dto.coverImageUrl) book.cover = dto.coverImageUrl;
  return book;
}

export function mapUserDto(dto: UserDto): Member {
  return {
    id: dto.userId,
    name: dto.username,
    email: dto.email,
    role: dto.role.trim().toLowerCase() === "admin" ? "Admin" : "Member",
    loans: 0,
    joined: "—",
  };
}

export function unwrapApi<T>(response: ApiResponse<T>): T {
  if (!response.success) {
    throw new Error(formatApiError(response));
  }
  return response.data as T;
}

function formatApiError(error: {
  message?: string | null;
  title?: string | null;
  errors?: string[] | Record<string, string[]> | null;
}): string {
  if (Array.isArray(error.errors) && error.errors[0]) {
    return error.errors[0];
  }
  if (error.errors && typeof error.errors === "object") {
    const first = Object.values(error.errors)[0];
    if (Array.isArray(first) && first[0]) return first[0];
  }
  return error.message ?? error.title ?? "Request failed.";
}

function readProperty(source: Record<string, unknown>, ...keys: string[]) {
  for (const key of keys) {
    if (source[key] !== undefined && source[key] !== null) {
      return source[key];
    }
  }
  return undefined;
}

export function normalizeAuthSession(payload: unknown): AuthSession {
  const source = (payload ?? {}) as Record<string, unknown>;
  const token = String(readProperty(source, "token", "Token") ?? "");
  if (!token) {
    throw new Error("Login succeeded but no token was returned.");
  }

  return {
    userId: Number(readProperty(source, "userId", "UserId") ?? 0),
    username: String(readProperty(source, "username", "Username") ?? ""),
    email: String(readProperty(source, "email", "Email") ?? ""),
    role: String(readProperty(source, "role", "Role") ?? "User"),
    token,
  };
}

type ApiRequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
};

export async function apiRequest<T>(path: string, options: ApiRequestOptions = {}): Promise<T> {
  const headers = new Headers(options.headers);
  headers.set("Accept", "application/json");

  let body: BodyInit | undefined;
  if (options.body instanceof FormData) {
    body = options.body;
  } else if (options.body !== undefined) {
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

  const raw = await response.text();
  let parsed: unknown = undefined;
  if (raw) {
    try {
      parsed = JSON.parse(raw);
    } catch {
      parsed = raw;
    }
  }

  if (response.status === 401 && !path.startsWith("/api/Auth/")) {
    clearAuthSession();
    window.location.assign("/login");
  }

  if (!response.ok) {
    const fallback = `Request failed with status ${response.status}.`;
    if (parsed && typeof parsed === "object") {
      throw new Error(formatApiError(parsed as { message?: string; title?: string; errors?: string[] }));
    }
    if (typeof parsed === "string" && parsed.trim()) {
      throw new Error(parsed);
    }
    throw new Error(fallback);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return parsed as T;
}

export async function login(username: string, password: string): Promise<AuthSession> {
  const response = await apiRequest<unknown>("/api/Auth/login", {
    method: "POST",
    body: { username, password },
  });
  const session = normalizeAuthSession(response);
  setAuthSession(session);
  return session;
}

export async function registerUser(username: string, email: string, password: string) {
  return apiRequest("/api/Auth/register", {
    method: "POST",
    body: { username, email, password },
  });
}

export async function requestPasswordOtp(email: string) {
  return apiRequest<string>("/api/Auth/forget-password", {
    method: "POST",
    body: { email },
  });
}

export async function resetPasswordWithOtp(email: string, otp: string, newPassword: string) {
  return apiRequest<string>("/api/Auth/reset-password", {
    method: "POST",
    body: { email, otp, newPassword },
  });
}

export async function getBooksPaged(pageNumber = 1, pageSize = DEFAULT_PAGE_SIZE) {
  return apiRequest<PagedResponse<BookDto>>(
    `/api/Book/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`,
  );
}

export async function getBookById(bookId: number) {
  const response = await apiRequest<ApiResponse<BookDto>>(`/api/Book/${bookId}`);
  return unwrapApi(response);
}

export async function getBooksByAuthor(author: string, pageNumber = 1, pageSize = DEFAULT_PAGE_SIZE) {
  const query = new URLSearchParams({
    author,
    pageNumber: String(pageNumber),
    pageSize: String(pageSize),
  });
  return apiRequest<PagedResponse<BookDto>>(`/api/Book/author?${query.toString()}`);
}

export async function getBooksByPriceRange(minPrice: number, maxPrice: number) {
  const query = new URLSearchParams({
    minPrice: String(minPrice),
    maxPrice: String(maxPrice),
  });
  const response = await apiRequest<ApiResponse<BookDto[]>>(`/api/Book/price-range?${query.toString()}`);
  return unwrapApi(response) ?? [];
}

export async function addBook(input: {
  title: string;
  author: string;
  price: number;
  publishedYear: number;
  availableCount: number;
  coverImage?: File | null;
}) {
  const form = new FormData();
  form.append("Title", input.title);
  form.append("Author", input.author);
  form.append("Price", String(input.price));
  form.append("PublishedYear", String(input.publishedYear));
  form.append("AvailableCount", String(input.availableCount));
  if (input.coverImage) {
    form.append("CoverImage", input.coverImage);
  }
  const response = await apiRequest<ApiResponse<number>>("/api/Book/add-book", {
    method: "POST",
    body: form,
  });
  return unwrapApi(response);
}

export async function updateBookPrice(bookId: number, newPrice: number) {
  const response = await apiRequest<ApiResponse<BookDto>>("/api/Book/update-price", {
    method: "PUT",
    body: { bookId, newPrice },
  });
  return unwrapApi(response);
}

export async function updateBookCover(bookId: number, coverImage: File) {
  const form = new FormData();
  form.append("coverImage", coverImage);
  const response = await apiRequest<ApiResponse<BookDto>>(`/api/Book/${bookId}/cover`, {
    method: "PUT",
    body: form,
  });
  return unwrapApi(response);
}

export async function deleteBookCover(bookId: number) {
  const response = await apiRequest<ApiResponse<boolean>>(`/api/Book/${bookId}/cover`, {
    method: "DELETE",
  });
  return unwrapApi(response);
}

export async function deleteBook(bookId: number) {
  const response = await apiRequest<ApiResponse<boolean>>(`/api/Book/${bookId}`, {
    method: "DELETE",
  });
  return unwrapApi(response);
}

export async function borrowBook(userId: number, bookId: number) {
  const response = await apiRequest<ApiResponse<BorrowRecordDto>>("/api/BorrowReturnBook/borrow", {
    method: "POST",
    body: { userId, bookId },
  });
  return unwrapApi(response);
}

export async function returnBook(borrowId: number) {
  const response = await apiRequest<ApiResponse<BorrowRecordDto>>("/api/BorrowReturnBook/return", {
    method: "POST",
    body: { borrowId },
  });
  return unwrapApi(response);
}

export async function getUsersPaged(pageNumber = 1, pageSize = DEFAULT_PAGE_SIZE) {
  return apiRequest<PagedResponse<UserDto>>(`/api/User?pageNumber=${pageNumber}&pageSize=${pageSize}`);
}

export async function getUserById(userId: number) {
  const response = await apiRequest<ApiResponse<UserDto>>(`/api/User/${userId}`);
  return unwrapApi(response);
}

export async function deleteUser(userId: number) {
  const response = await apiRequest<ApiResponse<boolean>>(`/api/User?id=${userId}`, {
    method: "DELETE",
  });
  return unwrapApi(response);
}

export async function getUserProfile(userId: number) {
  const response = await apiRequest<ApiResponse<UserDto>>(`/api/UserProfile/${userId}`);
  return unwrapApi(response);
}

export async function updateUserProfile(
  userId: number,
  input: { username?: string; email?: string; profileImage?: File | null },
) {
  const form = new FormData();
  if (input.username) form.append("Username", input.username);
  if (input.email) form.append("Email", input.email);
  if (input.profileImage) form.append("ProfileImage", input.profileImage);
  const response = await apiRequest<ApiResponse<UserProfileResponseDto>>(`/api/UserProfile/${userId}`, {
    method: "PUT",
    body: form,
  });
  return unwrapApi(response);
}

export async function getAvailableBooksReport(pageNumber = 1, pageSize = DEFAULT_PAGE_SIZE) {
  return apiRequest<PagedResponse<BookDto>>(
    `/api/Report/Avail-books?pageNumber=${pageNumber}&pageSize=${pageSize}`,
  );
}

export async function getOverdueBooksReport() {
  const response = await apiRequest<ApiResponse<BorrowRecordDto[]>>("/api/Report/OverDue-books");
  return unwrapApi(response) ?? [];
}
