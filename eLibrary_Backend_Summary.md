# eLibrary Backend Summary

## 1. Backend Overview

- **Purpose:** Library management for users, books, borrowing/returning, profiles, and admin reports.
- **Technology:** ASP.NET Core Web API on .NET 8, C#, MediatR/CQRS, FluentValidation, Swagger.
- **Architecture:** Controllers -> MediatR commands/queries -> repositories/services -> Entity Framework Core.
- **Database:** SQL Server via EF Core (`EBook` database; local `DefaultConnection`).
- **External services:** Gmail SMTP sends password-reset, borrow-confirmation, and return-confirmation emails. CORS is configured for local frontend ports 5173, 3000, and 8080.

## 2. Modules

- Authentication and password reset
- Books and book covers
- Borrowing and returning
- Users and profiles
- Admin reports

There are no active reservation, fine-payment, settings, notifications inbox, borrowing-history, or digital e-book/content modules.

## 3. API Summary

All routes are prefixed with `/api`. Send the JWT as `Authorization: Bearer <token>` where authentication is required.

### Authentication

- `POST /api/Auth/register` -> register a user; new users receive role `User`.
- `POST /api/Auth/login` -> login with username/password; returns JWT and user summary.
- `POST /api/Auth/forget-password` -> send a six-digit OTP by email.
- `POST /api/Auth/reset-password` -> reset password using email + OTP + new password.
- `POST /api/Auth/forget-password-jwt` -> generate a password-reset JWT (returned directly for demo use).
- `POST /api/Auth/reset-password-jwt` -> reset password using the reset JWT.

### Books

- `GET /api/Book/paged?pageNumber=1&pageSize=5` -> paginated books. `User`, `Admin`.
- `GET /api/Book/{Bookid}` -> book details. `User`, `Admin`.
- `GET /api/Book/author?author=...&pageNumber=1&pageSize=5` -> exact, case-insensitive author filter with pagination. `User`, `Admin`.
- `GET /api/Book/price-range?minPrice=...&maxPrice=...` -> inclusive price filter, not paginated. `User`, `Admin`.
- `POST /api/Book/add-book` -> create a book using multipart form data; admin only.
- `PUT /api/Book/update-price` -> update price; admin only.
- `PUT /api/Book/{id}/cover` -> replace cover image; admin only.
- `DELETE /api/Book/{id}/cover` -> delete cover image; admin only.
- `DELETE /api/Book/{id}` -> delete a book; admin only.

### Borrowing and Returning

- `POST /api/BorrowReturnBook/borrow` -> borrow a book for a user; authenticated users.
- `POST /api/BorrowReturnBook/return` -> return a borrow record; authenticated users.

### Users

- `GET /api/User?pageNumber=1&pageSize=5` -> paginated user list; admin only.
- `GET /api/User/{id}` -> user details; admin only.
- `DELETE /api/User?id=...` -> delete a user; admin only.
- `GET /api/User/CurrentUser` -> current JWT user ID, email, and role; authenticated users.

### Profiles

- `GET /api/UserProfile/{id}` -> get a profile; user can access self, admin can access any user.
- `PUT /api/UserProfile/{id}` -> update email/username and optional profile image using multipart form data; same access rule.

### Reports

- `GET /api/Report/Avail-books?pageNumber=1&pageSize=5` -> paginated books with `AvailableCount > 0`; admin only.
- `GET /api/Report/OverDue-books` -> active overdue borrow records with user/book information and stored fine; admin only.

## 4. Main Functionality

- **Authentication:** Registration, username/password login, JWT issuing, and two password-reset flows (OTP or JWT).
- **Books:** Admin creates, updates price, manages cover images, and deletes books. Users and admins view books, author results, price results, availability count, and details.
- **Circulation:** An authenticated user borrows a book; stock decreases. Returning a book marks it returned and restores stock.
- **Users/profiles:** Admin manages users. Users manage their own profile; admins can manage any profile.
- **Reports:** Admin views available books and overdue loans.

## 5. Important Business Rules

- A book cannot be borrowed when `AvailableCount <= 0`.
- A normal user can borrow only for their own JWT user ID; an admin may provide another `UserId`.
- A normal user can return only their own borrow record; an admin can return any record.
- A loan is due **7 days** after borrowing.
- Returning after the due date applies a fine of **10 per overdue day**. The backend does not provide payment or fine-management APIs.
- A returned record cannot be returned again.
- Adding a duplicate title + author (case-insensitive and trimmed) is rejected.
- Registration/profile updates reject duplicate email or username.
- The add-book handler currently accesses the cover as if required even though the DTO marks it optional. **Needs verification** before the frontend treats cover upload as optional.
- The registered FluentValidation validator targets `BookDto`, not `CreateBookDto`; add-book validation coverage is **Needs verification**.
- Borrow and return emails are best effort: the operation succeeds even if SMTP delivery fails, with a warning.

## 6. Authentication & Roles

- Login returns a signed HMAC-SHA256 JWT with user ID, email, and role claims.
- Normal access tokens expire after **10 minutes** and are validated with zero clock skew.
- Roles are `User` and `Admin`. Registration creates `User`; admin creation/assignment is not exposed by an API.
- Public: registration, login, and both password-reset flows.
- `User` or `Admin`: book reads.
- `Admin`: book writes, user administration, and reports.
- Authenticated users: borrow/return, current-user endpoint, and their own profile. Admins can access profiles for any user.

## 7. Search / Filter / Pagination

- **Search/filter:** books can be filtered by exact author and inclusive `minPrice`/`maxPrice`.
- **Pagination:** books, author results, users, available-books report use `pageNumber` and `pageSize`; default is page 1 and size 5.
- **Sorting:** no sorting parameter or sorting endpoint exists.
- No general keyword/title search, category filter, member search, or borrow-history filter exists.

## 8. Dashboard / Reports

- There is no aggregate dashboard or chart/analytics endpoint.
- Admin reports available:
  - Available books, paginated.
  - Overdue active loans, including due date and fine values.

## 9. Files / E-Books

- Book covers can be uploaded/replaced/deleted by admins.
- Profile images can be uploaded/replaced by the profile owner or an admin.
- Files are stored locally under `wwwroot/book-covers` and `wwwroot/profile-pictures` and served through ASP.NET static files.
- Active APIs return image URLs where applicable. Dedicated cover/profile download endpoints are commented out and inactive.
- No e-book file upload, download, preview, streaming, or document model exists. File type/size validation is not visible. **Needs verification** if stricter upload rules are required.

## 10. Frontend-Relevant Summary

Frontend needs to support:

1. Login, registration, logout/token expiry handling
2. OTP or JWT password reset
3. User and admin role-based navigation
4. Book list, details, author/price filters, and pagination
5. Admin book creation, price updates, deletion, and cover management
6. Borrow and return workflows with availability, due date, and fine display
7. User management for admins
8. Self profile editing and profile images
9. Admin available-books and overdue-loans reports
10. Image URL display from the backend

Backend supports:

- **5 modules**
- **25 active APIs**
- **Authentication:** JWT Bearer, 10-minute access tokens
- **Roles:** `User`, `Admin`
- **Database:** SQL Server with EF Core
- **Search:** exact author and price range only
- **Pagination:** books, users, and available-books report; default `pageNumber=1`, `pageSize=5`
- **File handling:** local book-cover and profile-image storage only; no e-book content APIs
