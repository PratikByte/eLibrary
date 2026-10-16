import { API_BASE_URL } from "./api-config";
import type { Book, Member } from "./placeholder-data";
import { toast } from "sonner";

export interface AuthSession {
  userId: string;
  username: string;
  email: string;
  role: string;
  token: string;
}

export const AUTH_SESSION_KEY = "eLibrary.session";
export const DEFAULT_PAGE_SIZE = 5;

function isJwtExpired(token: string): boolean {
  const payload = token.split(".")[1];
  if (!payload) return true;

  try {
    const normalizedPayload = payload.replace(/-/g, "+").replace(/_/g, "/");
    const paddedPayload = normalizedPayload.padEnd(
      Math.ceil(normalizedPayload.length / 4) * 4,
      "=",
    );
    const bytes = Uint8Array.from(atob(paddedPayload), (character) => character.charCodeAt(0));
    const claims = JSON.parse(new TextDecoder().decode(bytes)) as { exp?: unknown };
    return typeof claims.exp !== "number" || claims.exp <= Date.now() / 1000;
  } catch {
    return true;
  }
}

export type ApiResponse<T> = {
  success: boolean;
  message?: string | null;
  warning?: string | null;
  data?: T | null;
  errors?: string[] | null;
};

export type PagedResponse<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  success: boolean;
  message?: string | null;
  errors?: string[] | null;
};

export type DataPagedResponse<T> = ApiResponse<T[]> & {
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
};

export type BookDto = {
  id: string;
  title: string;
  author: string;
  availableCount: number;
  coverImageUrl?: string | null;
  price?: number;
};

export type UserDto = {
  id?: string;
  userId?: string;
  username: string;
  email: string;
  role: string;
  profileImageUrl?: string | null;
};

export type UserProfileResponseDto = UserDto & {
  profileImageUrl?: string | null;
};

export type BorrowRecordDto = {
  id: string;
  userId: string;
  userName?: string | null;
  bookId: string;
  bookName?: string | null;
  borrowDate: string;
  dueDate: string;
  returnDate?: string | null;
  isReturned: boolean;
  fine?: number | null;
};

export type ActiveBorrowRecordDto = {
  id: string;
  userId: string;
  userName: string;
  bookId: string;
  bookName: string;
  borrowDate: string;
  dueDate: string;
  isOverdue: boolean;
};

export type BorrowTransactionDto = {
  id: string;
  userId: string;
  userName: string;
  bookId: string;
  bookName: string;
  author: string;
  coverImageUrl?: string | null;
  borrowDate: string;
  dueDate: string;
  returnDate?: string | null;
  fine: number;
  overdueDays: number;
  isReturned: boolean;
  isOverdue: boolean;
};

type BorrowTransactionResponseDto = Omit<BorrowTransactionDto, "userName">;

export function getAuthSession(): AuthSession | null {
  const value = localStorage.getItem(AUTH_SESSION_KEY);
  if (!value) return null;

  try {
    const session = JSON.parse(value) as Partial<AuthSession>;
    if (
      typeof session.userId !== "string" ||
      !session.userId ||
      typeof session.token !== "string" ||
      !session.token ||
      isJwtExpired(session.token)
    ) {
      clearAuthSession();
      return null;
    }
    return session as AuthSession;
  } catch {
    clearAuthSession();
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
  if (dto.coverImageUrl) {
    book.cover = dto.coverImageUrl.startsWith("http")
      ? dto.coverImageUrl
      : `${API_BASE_URL}${dto.coverImageUrl}`;
  }
  return book;
}

export function mapUserDto(dto: UserDto): Member {
  const id = dto.id ?? dto.userId;
  if (!id) {
    throw new Error("User response is missing its ID.");
  }
  const member = {
    id,
    name: dto.username,
    email: dto.email,
    role: dto.role.trim().toLowerCase() === "admin" ? "Admin" : "Member",
    loans: 0,
    joined: "—",
  } as Member;
  if (dto.profileImageUrl) {
    member.avatar = dto.profileImageUrl.startsWith("http")
      ? dto.profileImageUrl
      : `${API_BASE_URL}${dto.profileImageUrl}`;
  }
  return member;
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

function getApiMessage(response: unknown): string | undefined {
  if (typeof response === "string" && response.trim()) return response.trim();
  if (!response || typeof response !== "object") return undefined;
  const message = Reflect.get(response, "message");
  if (typeof message === "string" && message.trim()) return message.trim();
  return getApiMessage(Reflect.get(response, "data"));
}

function withApiToast<T>(request: Promise<T>, successMessage: string): Promise<T>;
function withApiToast<T, R>(
  request: Promise<T>,
  successMessage: string,
  mapResponse: (response: T) => R,
): Promise<R>;
async function withApiToast<T, R = T>(
  request: Promise<T>,
  successMessage: string,
  mapResponse?: (response: T) => R,
): Promise<T | R> {
  try {
    const response = await request;
    const result = mapResponse ? mapResponse(response) : response;
    toast.success(getApiMessage(response) ?? successMessage);
    return result;
  } catch (error) {
    toast.error(error instanceof Error ? error.message : "Request failed.");
    throw error;
  }
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
  const userId = readProperty(source, "userId", "UserId", "id", "Id");
  if (userId === undefined || userId === null || String(userId).length === 0) {
    throw new Error("Login succeeded but no user ID was returned.");
  }

  return {
    userId: String(userId),
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
      throw new Error(
        formatApiError(parsed as { message?: string; title?: string; errors?: string[] }),
      );
    }
    if (typeof parsed === "string" && parsed.trim()) {
      throw new Error(parsed);
    }
    throw new Error(fallback);
  }

  if (
    parsed &&
    typeof parsed === "object" &&
    "success" in parsed &&
    Reflect.get(parsed, "success") === false
  ) {
    throw new Error(
      formatApiError(
        parsed as {
          message?: string | null;
          title?: string | null;
          errors?: string[] | Record<string, string[]> | null;
        },
      ),
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return parsed as T;
}

export async function login(username: string, password: string): Promise<AuthSession> {
  return withApiToast(
    (async () => {
      const response = await apiRequest<unknown>("/api/Auth/login", {
        method: "POST",
        body: { username, password },
      });
      const session = normalizeAuthSession(response);
      setAuthSession(session);
      return session;
    })(),
    "Signed in successfully.",
  );
}

export async function registerUser(username: string, email: string, password: string) {
  return withApiToast(
    apiRequest("/api/Auth/register", {
      method: "POST",
      body: { username, email, password },
    }),
    "Account created successfully.",
  );
}

export async function requestPasswordOtp(email: string) {
  return withApiToast(
    apiRequest<string>("/api/Auth/forget-password", {
      method: "POST",
      body: { email },
    }),
    "A security code has been sent to your email.",
  );
}

export async function verifyPasswordOtp(email: string, otp: string) {
  return withApiToast(
    apiRequest<unknown>("/api/Auth/verify-reset-otp", {
      method: "POST",
      body: { email, otp },
    }),
    "One-time code verified.",
  );
}

export async function resetPasswordWithOtp(email: string, otp: string, newPassword: string) {
  return withApiToast(
    apiRequest<string>("/api/Auth/reset-password", {
      method: "POST",
      body: { email, otp, newPassword },
    }),
    "Password reset successfully.",
  );
}

export async function getBooksPaged(
  pageNumber = 1,
  pageSize = DEFAULT_PAGE_SIZE,
  searchTerm = "",
  inStock?: boolean,
  minPrice?: number,
  maxPrice?: number,
) {
  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(pageSize),
  });
  if (searchTerm.trim()) query.set("searchTerm", searchTerm.trim());
  if (inStock !== undefined) query.set("inStock", String(inStock));
  if (minPrice !== undefined) query.set("minPrice", String(minPrice));
  if (maxPrice !== undefined) query.set("maxPrice", String(maxPrice));
  return apiRequest<PagedResponse<BookDto>>(`/api/Book/paged?${query.toString()}`);
}

export async function getBookById(bookId: string | number) {
  const response = await apiRequest<ApiResponse<BookDto>>(`/api/Book/${bookId}`);
  return unwrapApi(response);
}

export async function getBooksByAuthor(
  author: string,
  pageNumber = 1,
  pageSize = DEFAULT_PAGE_SIZE,
) {
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
  const response = await apiRequest<ApiResponse<BookDto[]>>(
    `/api/Book/price-range?${query.toString()}`,
  );
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
  return withApiToast(
    apiRequest<ApiResponse<number>>("/api/Book/add-book", {
      method: "POST",
      body: form,
    }),
    "Book added successfully.",
    unwrapApi,
  );
}

export async function updateBookPrice(bookId: string | number, newPrice: number) {
  return withApiToast(
    apiRequest<ApiResponse<BookDto>>("/api/Book/update-price", {
      method: "PUT",
      body: { bookId, newPrice },
    }),
    "Book price updated successfully.",
    unwrapApi,
  );
}

export async function updateBookCover(bookId: string | number, coverImage: File) {
  const form = new FormData();
  form.append("coverImage", coverImage);
  return withApiToast(
    apiRequest<ApiResponse<BookDto>>(`/api/Book/${bookId}/cover`, {
      method: "PUT",
      body: form,
    }),
    "Book cover updated successfully.",
    unwrapApi,
  );
}

export async function deleteBookCover(bookId: string | number) {
  return withApiToast(
    apiRequest<ApiResponse<boolean>>(`/api/Book/${bookId}/cover`, {
      method: "DELETE",
    }),
    "Book cover removed successfully.",
    unwrapApi,
  );
}

export async function deleteBook(bookId: string | number) {
  return withApiToast(
    apiRequest<ApiResponse<boolean>>(`/api/Book/${bookId}`, {
      method: "DELETE",
    }),
    "Book deleted successfully.",
    unwrapApi,
  );
}

export async function borrowBook(userId: string, bookId: string | number) {
  return withApiToast(
    apiRequest<ApiResponse<BorrowRecordDto>>("/api/BorrowReturnBook/borrow", {
      method: "POST",
      body: { userId, bookId },
    }),
    "Book borrowed successfully.",
    unwrapApi,
  );
}

export async function getActiveBorrowRecords() {
  const response = await apiRequest<ApiResponse<ActiveBorrowRecordDto[]>>(
    "/api/BorrowReturnBook/active",
  );
  const records = unwrapApi(response);
  if (!Array.isArray(records)) {
    throw new Error("The active loans response is invalid.");
  }
  return records;
}

export async function getRecentBorrowTransactions(count = 5): Promise<BorrowTransactionDto[]> {
  const response = await apiRequest<ApiResponse<BorrowTransactionResponseDto[]>>(
    `/api/BorrowReturnBook/transactions?count=${count}`,
  );
  const transactions = unwrapApi(response);
  if (!Array.isArray(transactions)) {
    throw new Error("The recent transactions response is invalid.");
  }

  const userIds = [...new Set(transactions.map((transaction) => transaction.userId))];
  if (userIds.length === 0) return [];

  const users = await getUsersByIds(userIds);
  const usernamesById = new Map(users.map((user) => [user.id, user.username]));
  return transactions.map((transaction) => ({
    ...transaction,
    userName: usernamesById.get(transaction.userId) ?? "Unknown member",
    coverImageUrl: transaction.coverImageUrl
      ? transaction.coverImageUrl.startsWith("http")
        ? transaction.coverImageUrl
        : `${API_BASE_URL}${transaction.coverImageUrl}`
      : null,
  }));
}

export async function returnBook(borrowId: string, returnDate: string) {
  return withApiToast(
    apiRequest<ApiResponse<BorrowRecordDto>>("/api/BorrowReturnBook/return", {
      method: "POST",
      body: { borrowId, returnDate },
    }),
    "Book returned successfully.",
    unwrapApi,
  );
}

export async function getUsersPaged(
  pageNumber = 1,
  pageSize = DEFAULT_PAGE_SIZE,
  searchTerm = "",
  role = "all",
) {
  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(pageSize),
  });
  if (searchTerm.trim()) query.set("searchTerm", searchTerm.trim());
  if (role !== "all") query.set("role", role);
  return apiRequest<DataPagedResponse<UserDto>>(`/api/User?${query.toString()}`);
}

export async function getUserById(userId: string) {
  const response = await apiRequest<ApiResponse<UserDto>>(`/api/User/${userId}`);
  return unwrapApi(response);
}

async function getUsersByIds(userIds: string[]) {
  const query = new URLSearchParams();
  userIds.forEach((userId) => query.append("ids", userId));
  const response = await apiRequest<ApiResponse<Array<{ id: string; username: string }>>>(
    `/api/User/lookup?${query.toString()}`,
  );
  const users = unwrapApi(response);
  if (!Array.isArray(users)) {
    throw new Error("The member lookup response is invalid.");
  }
  return users;
}

export async function deleteUser(userId: string) {
  return withApiToast(
    apiRequest<ApiResponse<boolean>>(`/api/User?id=${userId}`, {
      method: "DELETE",
    }),
    "Member removed successfully.",
    unwrapApi,
  );
}

function normalizeUserProfileResponse(
  response: ApiResponse<UserProfileResponseDto> | UserProfileResponseDto,
): UserProfileResponseDto & { userId: string } {
  const profile = "success" in response ? unwrapApi(response) : response;
  const userId = profile.id ?? profile.userId;
  if (userId === undefined || userId === null || userId.length === 0) {
    throw new Error("User profile response is missing its ID.");
  }
  const profileImageUrl = profile.profileImageUrl;
  return {
    ...profile,
    userId: String(userId),
    ...(profileImageUrl !== undefined && {
      profileImageUrl: profileImageUrl?.startsWith("http")
        ? profileImageUrl
        : profileImageUrl
          ? `${API_BASE_URL}${profileImageUrl}`
          : profileImageUrl,
    }),
  };
}

export async function getUserProfile(userId: string) {
  const response = await apiRequest<ApiResponse<UserProfileResponseDto> | UserProfileResponseDto>(
    `/api/UserProfile/${userId}`,
  );
  return normalizeUserProfileResponse(response);
}

export async function updateUserProfile(
  userId: string,
  input: { username?: string; email?: string; profileImage?: File | null },
) {
  const form = new FormData();
  if (input.username) form.append("Username", input.username);
  if (input.email) form.append("Email", input.email);
  if (input.profileImage) form.append("ProfileImage", input.profileImage);
  return withApiToast(
    apiRequest<ApiResponse<UserProfileResponseDto> | UserProfileResponseDto>(
      `/api/UserProfile/${userId}`,
      {
        method: "PUT",
        body: form,
      },
    ),
    "Profile updated successfully.",
    normalizeUserProfileResponse,
  );
}

export async function getAvailableBooksReport(pageNumber = 1, pageSize = DEFAULT_PAGE_SIZE) {
  return apiRequest<PagedResponse<BookDto>>(
    `/api/Report/Avail-books?pageNumber=${pageNumber}&pageSize=${pageSize}`,
  );
}

export async function getOverdueBooksReport() {
  const response = await apiRequest<ApiResponse<BorrowRecordDto[]>>("/api/Report/OverDue-books");
  const records = unwrapApi(response) ?? [];
  if (!Array.isArray(records)) {
    throw new Error("The overdue books response is invalid.");
  }

  const userIds = [...new Set(records.map((record) => record.userId))];
  if (userIds.length === 0) return records;

  const users = await getUsersByIds(userIds);
  const usernamesById = new Map(users.map((user) => [user.id, user.username]));
  return records.map((record) => {
    const userName = usernamesById.get(record.userId);
    return {
      ...record,
      userName:
        userName ??
        (record.userName && record.userName !== "User" ? record.userName : `User ${record.userId}`),
    };
  });
}
