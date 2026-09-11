# eLibrary Microservice 10-Step Migration Plan

**Status:** Locked and in execution. Phase 1 is active.  
**Source of truth:** Inspected eLibrary code (monolith + started services) and Saral as a reference architecture.  
**Locked:** 10 September 2026

## Locked execution order

1. Foundation: ports, solution registration, shared JWT, and monolith-safe setup.
2. Complete Identity service.
3. Route Identity through the Gateway.
4. Complete Book catalog service.
5. Route Book catalog through the Gateway.
6. Move borrowing, returning, and reports into Book.
7. Switch the frontend to the Gateway.
8. Create the Docker Compose stack.
9. Migrate service databases to PostgreSQL.
10. Verify production cutover, retain rollback, then retire the monolith.

The monolith remains runnable until Step 10 is explicitly approved. No additional business
services will be created unless a new bounded context is proven.

---

## 1. Goal

Migrate **eLibrary** (brand: Evidya) from a working .NET 8 CQRS monolith to a **simple, industry-standard microservice architecture**.

Constraints:

- Keep the existing monolith working during migration (strangler-fig).
- Do not delete or break current functionality.
- Keep CQRS (MediatR) where it already exists.
- Do not create one microservice per controller or table.
- Do not add Kafka, Redis, RabbitMQ, gRPC, MongoDB, or Kubernetes unless a real requirement appears. None of those are required for eLibrary today.
- Saral is a **reference**, not a template to copy.

---

## 2. What exists today

### 2.1 Running product (monolith)

The live application is a **single-project** .NET 8 Web API:

| Item | Detail |
|------|--------|
| Project | `ELibrary.csproj` (also `ELibrary.sln` — one project only) |
| Layers | Folders inside one assembly: `API/`, `Application/`, `Domain/`, `Infrastructure/`, `Shared/` |
| CQRS | MediatR 8, commands/queries in `Application/` |
| Database | SQL Server, database `EBook`, EF Core `EBookDBContext` |
| Auth | Custom `User` entity + JWT Bearer (not ASP.NET Identity) |
| Port | `http://localhost:5285` (`https://localhost:7104`) |
| Frontend | `FroentendLovable/` (React + Vite), default API `http://localhost:5285` |
| Docker | Root `Dockerfile` publishes the monolith on container port 8080 |

`ELibrary.csproj` already excludes `Services/**` and the root `eLibrary.Identity.*` folders, so `dotnet run` on the monolith does not compile the new services. That exclusion must stay until cutover.

### 2.2 Monolith capabilities (complete inventory)

| Area | Controller | Routes the frontend actually calls |
|------|------------|------------------------------------|
| Auth | `AuthController` | `/api/Auth/register`, `/login`, `/forget-password`, `/reset-password` (JWT reset variants exist but frontend uses OTP) |
| Users | `UserController` | `/api/User`, `/api/User/{id}`, `/api/User?id=` DELETE, `/api/User/CurrentUser` |
| Profile | `UserProfileController` | `/api/UserProfile/{id}` GET/PUT (multipart image) |
| Catalog | `BookController` | `/api/Book/paged`, `/author`, `/price-range`, `/{id}`, `/add-book`, `/update-price`, `/{id}/cover`, DELETE `/{id}` |
| Lending | `BorrowReturnBookController` | `/api/BorrowReturnBook/borrow`, `/return` |
| Reports | `ReportController` | `/api/Report/Avail-books`, `/OverDue-books` |

Domain tables:

- `Books` — catalog + `AvailableCount` + `CoverImagePath`
- `Users` — credentials, role (`User`/`Admin`), `ProfileImagePath`
- `OtpRecords` — password-reset OTP
- `BorrowRecords` — `UserId`, `BookId`, dates, `IsReturned`, `Fine`

Related infrastructure (not separate products):

- SMTP email (OTP + borrow/return confirmation)
- Local disk files under `wwwroot` (covers, profile images)
- JWT lifetime: **10 minutes**

There is **no** category, publisher, review, payment, reservation, or multi-tenant module.

### 2.3 Frontend contract that must not break

`FroentendLovable/src/lib/api.ts` hard-codes these paths. Phase 1 must keep them unless the frontend is changed in the same step:

- `/api/Auth/*`
- `/api/Book/*`
- `/api/BorrowReturnBook/*`
- `/api/User` and `/api/User/{id}`
- `/api/UserProfile/{id}`
- `/api/Report/*`

Base URL is `VITE_API_BASE_URL` (default `http://localhost:5285`). Switching the SPA to the Gateway is a **late Phase 1** step, not the first step.

`/my-loans` currently uses placeholder data, not an API. Do not invent a new loans microservice for it. If “my loans” is implemented later, it belongs on the Book service as a query by `userId` from the JWT.

### 2.4 New services already started

These live under `Services/` and are **not** in `ELibrary.sln`.

| Service | Layout | Port today | Database today |
|---------|--------|------------|----------------|
| Gateway | API only (correct) | **8000** | none |
| Identity | API / Application / Domain / Infrastructure | **8001** | LocalDB `eLibraryIdentityDb` |
| Book | API / Application / Domain / Infrastructure | **missing `launchSettings.json`** | LocalDB `eLibraryBookDb` |

Gateway technology: **Ocelot** 22.0.1, matching the Saral-style route configuration.

Root-level `eLibrary.Identity.API|Application|Domain|Infrastructure` folders are **abandoned empty scaffolds**. They do not build. Canonical Identity is `Services/Identity/`.

---

## 3. Saral reference — what to take, what to skip

Saral is a larger loan-origination system (Identity, Customer, Application, Document, Workflow, Notification, Reporting) behind an **Ocelot** gateway, PostgreSQL, MongoDB GridFS, and docker-compose.

### Take from Saral

- One Clean Architecture stack **per service** (API → Application → Domain ← Infrastructure).
- Thin API Gateway as the only public HTTP entry for the SPA.
- Identity issues JWT; other services validate the **same** secret, issuer, and audience.
- MediatR CQRS inside each service’s Application layer.
- Typed `HttpClient` only where a service truly needs another service.
- `docker-compose` for local/server deploy (postgres + services + gateway).
- Health checks.
- Separate docker vs local gateway destination hostnames.

### Do not copy from Saral

| Saral pattern | Why eLibrary should not copy it |
|---------------|----------------------------------|
| 7 business services | eLibrary has 2 real bounded contexts |
| Large BFF controller | eLibrary does not need duplicated forwarding logic |
| Shared single Postgres database `saralapp` for all EF contexts | Pretends isolation; eLibrary should use **one Postgres instance, two databases** |
| MongoDB GridFS | Book covers and profile photos are small files; disk volume is enough |
| Dual gateway (Ocelot + large BFF controller) | Ocelot route files are enough |
| Rate-limit stack + session fingerprinting | Banking-grade; not needed |
| `IEventBus` with no broker | Dead abstraction |
| Redis / RabbitMQ / Kubernetes (docs only in Saral) | Not implemented there; not needed here |
| Heavy Shared.Infrastructure | A tiny shared contract is enough, or none |

---

## 4. Target architecture (keep it small)

### 4.1 Services that will exist

```
Browser (Vite / later Docker frontend)
        │
        ▼
eLibrary.Gateway.API     :8000     Ocelot + CORS + JWT edge check
        │
        ├── Identity Service :8001     Auth, users, profiles, OTP, profile images
        └── Book Service     :8002     Catalog, covers, borrow/return, reports
```

**Three deployable apps. No fourth business service.**

| Component | Responsibility | Owns data |
|-----------|----------------|-----------|
| **Gateway** | Reverse proxy, CORS for the SPA, optional edge JWT. No business logic, no database. | nothing |
| **Identity** | Register, login, JWT, forgot/reset password, admin user list/get/delete, current user, profile get/update, profile images, OTP + SMTP | `Users`, `OtpRecords` |
| **Book** | Book CRUD, covers, price, availability, borrow, return, fines, available-books report, overdue report | `Books`, `BorrowRecords` |

### 4.2 Why no extra services

| Candidate | Verdict | Reason |
|-----------|---------|--------|
| Lending / Circulation | **Do not create** | Borrow **decrements `Book.AvailableCount` in the same transaction**. Splitting this forces distributed stock updates. `BorrowRecords` already live next to books. |
| Report | **Do not create** | Two queries over books + borrow rows. Belongs on Book. |
| Notification | **Do not create** | Email templates are infrastructure. Identity sends OTP mail; Book sends borrow/return mail. |
| Document / File | **Do not create** | Two image folders. Keep local disk (Docker volume). Saral’s Mongo/GridFS is for large loan documents. |
| User (separate from Identity) | **Do not create** | Users and auth are one context. |
| One service per controller | **Do not create** | That is a distributed monolith. |

### 4.3 Keep the name “Book Service”

Do not rename to Library/Catalog. The folder and projects already exist. Document internally that Book **includes circulation**.

### 4.4 CQRS

Keep MediatR **inside each service**.

- New services already use **MediatR 12**. Stay on 12 for Gateway-adjacent services.
- Do not introduce a shared message bus.
- Controllers should `IMediator.Send(...)`. Identity’s User/Profile currently call the repository from the controller — fix that in Phase 1 so CQRS is consistent.
- Handlers may live in the same file as the command (Book style) or a sibling `*Handler.cs` (Identity style). Pick **one style per service** and keep it. Do not spend time unifying both services.

### 4.5 Authentication model

Same pattern as Saral, simpler:

1. Identity **signs** JWT (HMAC-SHA256, shared secret).
2. Gateway **validates** JWT on non-public routes and forwards `Authorization`.
3. Book and Identity **validate again** (defense in depth, role checks on actions).

Public routes (no JWT):

- `/api/Auth/login`
- `/api/Auth/register`
- `/api/Auth/forget-password`
- `/api/Auth/reset-password`
- Swagger in Development

All other `/api/*` routes require Bearer.

**One JWT config everywhere** (Gateway, Identity, Book, and the monolith during strangler):

```
Issuer:   eLibrary
Audience: eLibraryUsers
Key:      one shared secret (from config/env, not three different keys)
Duration: keep 10 minutes unless product asks to change it
```

Roles stay as claim values `Admin` and `User`.

### 4.6 Cross-service communication

| Call | When | How |
|------|------|-----|
| SPA → Gateway → Identity/Book | Always | HTTP via Ocelot |
| Book → Identity | Borrow by admin on behalf of a user; optional username/email for confirmation email | Typed `HttpClient`, GET user by id, forward incoming Bearer |
| Identity → Book | Never | — |

Rules:

- No EF join across service databases.
- `BorrowRecords.UserId` is a **reference id**, not a foreign key to Identity’s database.
- Snapshot `userName` / `bookName` onto the borrow DTO/record at write time so reports do not need a live Identity join.
- For self-borrow, trust `ClaimTypes.NameIdentifier` from the JWT; do not call Identity just to prove the user exists.
- No events, no outbox, no Kafka.

### 4.7 Unstructured data (files)

| File | Owner | Storage |
|------|--------|---------|
| Book covers | Book | Disk `wwwroot/book-covers/` + path column on `Books` |
| Profile images | Identity | Disk `wwwroot/profile-images/` + path column on `Users` |

Return absolute URLs through the **Gateway host** so the browser never talks to :8001/:8002 directly. Gateway will also proxy static paths (see Phase 1 routing).

Do not put image bytes in PostgreSQL. Do not add MongoDB.

### 4.8 Target folder / solution layout

```
eLibrary/
├── ELibrary.csproj                 # monolith — keep until cutover
├── ELibrary.sln                    # add microservice projects; keep monolith in solution
├── Services/
│   ├── Gateway/eLibrary.Gateway.API/
│   ├── Identity/
│   │   ├── eLibrary.Identity.API/
│   │   ├── eLibrary.Identity.Application/
│   │   ├── eLibrary.Identity.Domain/
│   │   └── eLibrary.Identity.Infrastructure/
│   └── Book/
│       ├── eLibrary.Book.API/
│       ├── eLibrary.Book.Application/
│       ├── eLibrary.Book.Domain/
│       └── eLibrary.Book.Infrastructure/
├── FroentendLovable/
├── docker-compose.yml              # Phase 3
└── microserviceplan.md             # this file
```

Optional later: a tiny `Shared/eLibrary.Shared` for `ApiResponse<T>` / `PagedResponse<T>` only. Duplicate those two types in Phase 1 if that is faster. Do not create Saral-style Shared.Messaging.

Delete root `eLibrary.Identity.*` scaffolds in Phase 1 (they are unused).

---

## 5. Inspection of the started services — what is wrong

Fix these during Phase 1. They are blocking a working split.

### 5.1 Critical

| Issue | Evidence | Fix |
|-------|----------|-----|
| JWT secret mismatch | Gateway key `YourVeryStrongSecretKeyHere1234567890`; Identity/Book key `ThisIsMyVeryStrongJwtKeyForIdentityService1234567890`; monolith uses a third key and issuer `BookStoreApi` / audience `EbookApi` | One config block in all four places (including monolith) so Identity-issued tokens work on Gateway, Book, **and** monolith during strangler |
| Ocelot does not send traffic to Book | Book route points to the wrong destination | Point catalog routes at Book `:8002`. Match the real controller paths |
| Book has no port | No `Properties/launchSettings.json` | Set HTTP **8002** |
| Gateway paths do not match the API or frontend | Gateway uses `/api/books`, `/api/borrow`, `/api/profile`; real routes are `/api/Book`, `/api/BorrowReturnBook`, `/api/UserProfile` | Match existing routes and preserve the frontend contract |
| Identity User/Profile never routed | Only `/api/auth/{**catch-all}` goes to :8001 | Add `/api/User/{**catch-all}` and `/api/UserProfile/{**catch-all}` → Identity |
| New projects not in solution | `ELibrary.sln` contains only the monolith | Add all Gateway/Identity/Book projects |

### 5.2 High

| Issue | Fix |
|-------|-----|
| No EF migrations under `Services/` | Add migrations per DbContext before any cutover |
| Gateway custom JWT middleware **duplicates** `UseAuthentication` | Remove the custom `ValidateToken` middleware. Use JWT Bearer + a public-path exclusion (or YARP route metadata). Keep `UseAuthentication` / `UseAuthorization` |
| Identity `EmailService` is a console stub | Port monolith SMTP (`SmtpSettings` + real send) |
| Identity missing vs monolith: paged user list, delete user, profile image file pipeline | Copy behavior from monolith User/UserProfile commands |
| Identity `CurrentUser` is `[Authorize(Roles = "Admin")]` | Monolith allows any authenticated user. Restore that |
| Book missing `UpdateBookPrice` | Frontend calls `/api/Book/update-price`. Port the command |
| Book missing borrow/return/reports | These stay on the monolith until extracted **into Book**, not a new service |
| Book/Identity have no CORS | Correct **if** the browser only talks to Gateway. Do not add CORS on inner services unless someone will call them from the browser during debug |
| Cover/profile URLs use process `wwwroot` CWD | Keep `UseStaticFiles`; in Docker mount a volume |

### 5.3 Medium / cleanup

| Issue | Fix |
|-------|-----|
| Root `eLibrary.Identity.*` duplicates | Delete |
| Empty `Class1.cs` in Identity Application/Domain/Infrastructure | Delete |
| Gateway `.http` still says port 5181 | Update to 8000 |
| Swagger package versions differ (Gateway 6.6.2 vs 8.0.0) | Align on 8.x |
| Cluster name `application-cluster` | Rename to `book-cluster` when it points at Book |
| Gateway does not route `/api/Report` | Add it to Book cluster |
| Identity JWT password-reset endpoints not extracted | Frontend uses OTP. Port OTP first; JWT reset is optional parity with monolith |
| Book Application has `FrameworkReference` to ASP.NET for `IFormFile` | Acceptable; alternatively put upload DTOs in the API layer |

### 5.4 What is already correct (keep)

- Gateway is API-only (no Domain/DbContext).
- Identity and Book use 4-layer Clean Architecture.
- Book CQRS catalog commands/queries have handlers.
- Identity auth commands have handlers (`Register`, `Authenticate`, `ForgotPassword`, `ResetPassword`).
- Ocelot is a valid .NET gateway choice and matches the Saral reference.
- Ports 8000 / 8001 follow a clear scheme.
- Monolith exclusion of `Services/` keeps the old app buildable.

---

## 6. Target ports and URLs

Keep 8000/8001 that already exist. Assign Book 8002. Leave the monolith on 5285 until cutover.

| Process | Local HTTP | Docker host:container | Notes |
|---------|------------|------------------------|-------|
| Frontend (Vite) | 3001 | 3001:3001 (Phase 3, optional) | Talks only to Gateway after cutover |
| Gateway | **8000** | 8000:8000 | Public API |
| Identity | **8001** | 8001:8001 | Internal + Gateway |
| Book | **8002** | 8002:8002 | Internal + Gateway |
| Monolith | **5285** | not containerized in the new compose | Strangler only |
| PostgreSQL | 5432 | 5432:5432 | Phase 2/3 |
| pgAdmin | 5050 | 5050:80 | Phase 2/3 |

Ocelot destinations:

- Local: `http://localhost:8001`, `http://localhost:8002`
- Docker: `http://identity:8001`, `http://book:8002`

Use two gateway config files or env-based addresses (`appsettings.json` vs `appsettings.Docker.json`), same idea as Saral’s `ocelot.json` / `ocelot.docker.json`.

---

## 7. Gateway routing (target)

Keep controller names so `api.ts` does not need a rewrite.

| Route id | Path | Cluster | Destination |
|----------|------|---------|-------------|
| identity-auth | `/api/Auth/{**catch-all}` | identity | Identity :8001 |
| identity-user | `/api/User/{**catch-all}` | identity | Identity :8001 |
| identity-profile | `/api/UserProfile/{**catch-all}` | identity | Identity :8001 |
| book-catalog | `/api/Book/{**catch-all}` | book | Book :8002 |
| book-borrow | `/api/BorrowReturnBook/{**catch-all}` | book | Book :8002 |
| book-report | `/api/Report/{**catch-all}` | book | Book :8002 |
| identity-files | `/profile-images/{**catch-all}` | identity | Identity :8001 |
| book-files | `/book-covers/{**catch-all}` | book | Book :8002 |

Ocelot route templates should mirror the existing controller paths. Keep the spelling above for clarity.

Do not send leftover traffic to the monolith once a route is extracted. During strangler, **unextracted** routes may temporarily use a `monolith-cluster` → `:5285`. Remove that cluster when Phase 1 is done.

---

## 8. Phase 1 — Monolith to microservices (strangler)

**Objective:** Extract Identity and Book behind the Gateway without turning off the monolith. Frontend keeps working on `:5285` until each slice is proven.

**Database in this phase:** stay on SQL Server / LocalDB. Do not switch to PostgreSQL yet.

### 8.1 Ground rules

1. Do not remove or empty monolith controllers until the Gateway slice is verified.
2. Align JWT on the **monolith** with Identity **before** any route is served from both.
3. Extract in slices. After each slice: Swagger + frontend flow for that slice.
4. File uploads (`FromForm`) must be forwarded by Ocelot without buffering away the request body.
5. Keep `ApiResponse<T>` / `PagedResponse<T>` JSON shape identical to what `api.ts` expects.

### 8.2 Phase 1 workstreams

#### Step 0 — Housekeeping (does not change runtime)

- Add Gateway, Identity, Book projects to `ELibrary.sln`.
- Delete root `eLibrary.Identity.*` scaffolds.
- Delete empty `Class1.cs`.
- Give Book `launchSettings.json` port **8002**.
- Unify JWT settings in Gateway, Identity, Book, **and monolith** `appsettings`.
- Remove Gateway duplicate token middleware; keep standard JWT Bearer.
- Fix Ocelot paths to the table in section 7, but keep unextracted routes on the monolith `:5285`.

#### Step 1 — Finish Identity (still SQL Server)

Port remaining monolith behavior into `Services/Identity`:

- Auth: register, login, OTP forgot/reset (real SMTP). Optional: JWT reset endpoints for parity.
- Users: paged list, get by id, delete, CurrentUser (any authenticated user).
- Profile: get/update + `IFormFile` + static files.
- CQRS queries/commands for User/Profile (stop calling repository from controllers).
- `IdentityDbContext` EF migrations; create `eLibraryIdentityDb`.
- Seed or copy a known admin user so login can be tested independently of the monolith.

Identity must not reference Book entities.

#### Step 2 — Strangle Auth + Users through Gateway

Ocelot:

- `/api/Auth/*` → Identity :8001
- `/api/User/*`, `/api/UserProfile/*` → Identity :8001
- Everything else → monolith :5285

Prove:

- Login via Gateway `:8000` returns a token.
- That token still works on monolith Book routes (JWT alignment from Step 0).
- Register, forgot/reset, admin user list, profile image.

Frontend: still on `:5285` until you choose to point `VITE_API_BASE_URL` at `:8000` for a dedicated test. Prefer a **second** frontend env or a temporary `.env.gateway` rather than breaking default `start-all.ps1`.

#### Step 3 — Finish Book catalog (still SQL Server)

Complete `Services/Book` catalog to match monolith `BookController`:

- Existing paged/author/price-range/id/add/cover/delete handlers.
- Add `UpdateBookPrice`.
- File storage + `UseStaticFiles` for covers.
- FluentValidation for add-book (monolith has `AddBookDtoValidator`).
- `BookDbContext` migrations; create `eLibraryBookDb`.
- Copy catalog data from `EBook.Books` for testing (script or one-off SQL).

Do **not** extract borrow yet in this step.

#### Step 4 — Strangle catalog through Gateway

Ocelot: `/api/Book/*` → Book :8002. Borrow and Report still on monolith.

**Stock warning:** until borrow moves, `AvailableCount` can diverge (Book DB vs monolith `EBook`). Accept this only for a short window, or keep catalog on the monolith until Step 5. **Recommended:** keep `/api/Book` on the monolith until borrow is ready, then flip Book + Borrow + Report together.

Safer sequence for data consistency:

1. Finish Book **including** borrow/return/reports against `eLibraryBookDb`.
2. Copy `Books` + `BorrowRecords` from `EBook` in one script.
3. Flip `/api/Book`, `/api/BorrowReturnBook`, `/api/Report` in **one** Gateway change.

#### Step 5 — Move lending and reports into Book (not a new service)

Port into Book service:

- Entities: `BorrowRecords` (UserId as int, **no** navigation to Identity `User`).
- Commands: `BorrowBook`, `ReturnBook` (same rules: non-admin forced to own userId; admin may specify userId).
- Stock change in the **same** `SaveChanges` as the borrow row.
- Due date = +7 days; fine logic as in monolith `ReturnBookCommand`.
- Email: port `NotificationService` templates into Book infrastructure; send via SMTP.
- Queries: `GetAvailableBooksQuery`, `GetBorrowReportQuery`.
- Controllers: keep route names `BorrowReturnBook` and `Report`.

Username on DTOs: snapshot from JWT name claim or from Identity HTTP GET when admin borrows for someone else.

#### Step 6 — Frontend cutover to Gateway

1. All Ocelot routes point at Identity/Book. Remove the monolith routes.
2. Set `VITE_API_BASE_URL=http://localhost:8000`.
3. CORS on Gateway includes `http://localhost:3001`.
4. Run full UI: login, register, catalog, add book, cover, borrow, return, users, profile, reports.
5. Update `start-all.ps1` to start Gateway + Identity + Book + frontend (monolith optional).

#### Step 7 — Freeze monolith (do not delete yet)

- Leave `ELibrary.csproj` runnable for rollback.
- Do not delete monolith code in Phase 1.
- Document: “monolith is fallback only.”
- Physical delete of monolith folders is **out of scope** until after Phase 3 is stable.

### 8.3 Phase 1 definition of done

- SPA works through Gateway `:8000` with no calls to `:5285`.
- Identity and Book run as separate processes.
- JWT login from Identity authorizes Book admin/user actions.
- Monolith still starts on `:5285` if needed.
- No Kafka/new services added.

---

## 9. Phase 2 — PostgreSQL + pgAdmin

**Objective:** Replace SQL Server / LocalDB with PostgreSQL. Each service owns its own database on **one** Postgres server. Use pgAdmin for inspection.

Saral uses one shared database for all services. eLibrary will **not** do that. Two databases on one instance is enough isolation without extra Postgres containers.

### 9.1 Target databases

| Database | Owner | Tables |
|----------|--------|--------|
| `elibrary_identity` | Identity | `Users`, `OtpRecords` |
| `elibrary_book` | Book | `Books`, `BorrowRecords` |

Postgres instance:

- Image: `postgres:16-alpine` (current stable; Saral uses 15 — either is fine)
- User: from env (`POSTGRES_USER` / `POSTGRES_PASSWORD`)
- Port: 5432
- Volume: `postgres_data`

pgAdmin:

- Image: `dpage/pgadmin4`
- Host port **5050**
- Pre-configure server connection to `postgres:5432` in Docker; locally `localhost:5432`

No MongoDB. No SQL Server in the target topology.

### 9.2 Schema (PostgreSQL)

Keep columns aligned with current EF models so the app behavior stays the same.

**elibrary_identity.Users**

| Column | Type | Notes |
|--------|------|--------|
| UserId | integer PK identity | |
| Email | varchar(100) unique | |
| Username | varchar(50) unique | |
| PasswordHash | text | ASP.NET `PasswordHasher` |
| Role | varchar(20) | `User` / `Admin` |
| CreatedAt | timestamptz | |
| ProfileImagePath | varchar | relative path, nullable |

**elibrary_identity.OtpRecords**

| Column | Type |
|--------|------|
| Id | integer PK |
| Email | varchar |
| Code | varchar |
| ExpiryTime | timestamptz |

**elibrary_book.Books**

| Column | Type | Notes |
|--------|------|--------|
| BookId | integer PK | map entity `Id` → column `BookId` as today |
| Title | varchar nullable | |
| Author | varchar nullable | |
| Price | real | current `float` |
| PublishedYear | integer | |
| AvailableCount | integer | |
| CoverImagePath | varchar nullable | |

**elibrary_book.BorrowRecords**

| Column | Type | Notes |
|--------|------|--------|
| Id | integer PK | |
| UserId | integer | **no FK** to Identity DB |
| BookId | integer | **FK to Books.BookId** (same database) |
| BorrowDate | timestamptz | |
| DueDate | timestamptz | |
| ReturnDate | timestamptz nullable | |
| IsReturned | boolean | |
| Fine | numeric(10,2) nullable | |
| UserNameSnapshot | varchar nullable | optional denormalized display |
| BookNameSnapshot | varchar nullable | optional; else join Books |

Indexes: `BorrowRecords(UserId)`, `BorrowRecords(BookId)`, `BorrowRecords(IsReturned, DueDate)` for overdue reports.

### 9.3 EF Core configuration

Per service:

- Package: `Npgsql.EntityFrameworkCore.PostgreSQL` (8.x to match net8).
- Remove `UseSqlServer` from Identity and Book `Program.cs`.
- Connection string pattern:

```
Host=localhost;Port=5432;Database=elibrary_identity;Username=elibrary;Password=***
```

Docker:

```
Host=postgres;Port=5432;Database=elibrary_book;Username=elibrary;Password=***
```

- `EnableRetryOnFailure` for transient Docker starts.
- Naming: Npgsql default **snake_case** would rename `UserId` → `user_id` and break a naive data copy. **Decision:** keep EF default Pascal/exact column names (explicit `[Column]` / `HasColumnName`) so they match the current schema and the data-migration script stays simple. Do not switch to snake_case unless you want a mapping layer.
- Timestamps: use UTC (`timestamptz`). Audit existing `DateTime.UtcNow` usage (already UTC in borrow handler).

Migrations:

- Store under each Infrastructure project:  
  `Services/Identity/eLibrary.Identity.Infrastructure/Migrations/`  
  `Services/Book/eLibrary.Book.Infrastructure/Migrations/`
- Commands (run after provider switch, **new** initial migration; do not try to replay SQL Server migrations on Postgres):

```text
dotnet ef migrations add InitialPostgres --project ...Infrastructure --startup-project ...API --context IdentityDbContext
dotnet ef database update --project ...Infrastructure --startup-project ...API --context IdentityDbContext
```

Same for `BookDbContext`.

Do not use `EnsureCreated()` in production. `Database.Migrate()` on startup is acceptable for this app size if gated by config; prefer explicit `dotnet ef database update` in compose (Saral `ef-migrations` profile pattern, simplified to two contexts).

### 9.4 Structured vs unstructured

| Kind | Store |
|------|--------|
| Users, OTP, books, borrow rows | PostgreSQL |
| Cover and profile **files** | Filesystem volumes `identity_files`, `book_files` |
| Paths | VARCHAR columns only |

Backup: Postgres dump + volume backup. Images are not in pgAdmin; they are on disk.

### 9.5 Data migration from SQL Server `EBook`

Order:

1. Deploy empty Postgres schemas via EF migrations.
2. Export from SQL Server (`EBook`): Users, OtpRecords, Books, BorrowRecords.
3. Import Users/OTP → `elibrary_identity`.
4. Import Books then BorrowRecords → `elibrary_book` (respect BookId identity; use `OVERRIDING SYSTEM VALUE` or set sequence).
5. Reset Postgres sequences (`setval`) to `MAX(id)`.
6. Copy `wwwroot` files into the new service wwwroot folders.
7. Validate counts and a sample login + borrow.

Approach: a one-off console script or SQL with `postgres_fdw` is unnecessary complexity. A small C# “migrator” that reads SQL Server with the old context and writes Npgsql is enough. Run once; do not leave it in the production compose.

Password hashes: `PasswordHasher<User>` output is provider-agnostic. Copy hashes as-is; users must be able to log in without reset.

### 9.6 Validation checklist (Phase 2)

- pgAdmin shows both databases and tables.
- Identity register/login against Postgres.
- Book list/add/borrow/return/report against Postgres.
- Image upload still served via Gateway.
- SQL Server can be stopped; app still works.
- Row counts: `Users`, `Books`, `BorrowRecords` match source (or documented deltas).
- Overdue report still returns the same borrow ids as a pre-cutover snapshot.

### 9.7 Monolith during Phase 2

Leave the monolith on SQL Server. Do not dual-write. Phase 2 applies to **Identity and Book only**. If the monolith is still used as rollback, it keeps `EBook` on SQL Server.

---

## 10. Phase 3 — Docker / Docker Compose

**Objective:** Run Gateway, Identity, Book, PostgreSQL, pgAdmin (and optionally frontend) with docker-compose, using Saral as the **shape** reference (compose file, Dockerfiles, network, volumes, healthchecks) — not Saral’s service count or Mongo.

### 10.1 What to create

| File | Role |
|------|------|
| `Services/Gateway/eLibrary.Gateway.API/Dockerfile` | Multi-stage .NET 8 |
| `Services/Identity/Dockerfile` | Multi-stage; context = repo root |
| `Services/Book/Dockerfile` | Same |
| `FroentendLovable/Dockerfile` | Optional; Vite preview or nginx static |
| `docker-compose.yml` | Local/dev |
| `.env.example` | JWT, postgres password, SMTP — no real secrets committed |

Reuse the **idea** of Saral’s Identity Dockerfile: copy csproj files first, `dotnet restore`, then copy source, publish, `ASPNETCORE_URLS=http://+:{port}`.

Root monolith `Dockerfile` stays for emergency rollback; it is **not** in the new compose.

### 10.2 Compose services (proposed)

```
services:
  postgres:     postgres:16-alpine, healthcheck pg_isready, volume postgres_data
  pgadmin:      dpage/pgadmin4, port 5050, depends_on postgres
  identity:     build Identity Dockerfile, port 8001, wait for postgres healthy
  book:         build Book Dockerfile, port 8002, wait for postgres healthy
  gateway:      build Gateway Dockerfile, port 8000, depends_on identity, book
  frontend:     optional, port 3001, VITE/API URL http://localhost:8000
```

Network: bridge `elibrary-network`.  
Volumes: `postgres_data`, `identity_files` → `/app/wwwroot/profile-images`, `book_files` → `/app/wwwroot/book-covers`.

Environment (compose, not hardcoded passwords in git):

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__Key`, `JwtSettings__Issuer`, `JwtSettings__Audience`
- `SmtpSettings__*`
- Gateway Ocelot routes use Docker service names such as `identity:8001` and `book:8002`

Apply EF migrations: either

- `dotnet ef database update` in an opt-in compose profile `migration` (Saral-style, only two projects), or
- Identity/Book `Program.cs` call `Database.Migrate()` when `APPLY_MIGRATIONS=true`.

Prefer the explicit migration profile so containers do not race.

### 10.3 Gateway in Docker

- Bind `http://0.0.0.0:8000` (Saral hardcodes URLs; eLibrary can use `ASPNETCORE_URLS`).
- Destinations use **Docker DNS names**, not localhost.
- CORS origins: localhost:3001 (dev) and the frontend container origin if used.

### 10.4 What Phase 3 will not include

- Kubernetes / Helm
- Nginx (optional later for TLS on a VM; Saral puts nginx on the host). For local compose, Gateway is enough.
- MongoDB, Redis, RabbitMQ, Kafka
- Pre-built private registry images (Saral uses `diyagent/saralapp:*`). eLibrary will `build:` from Dockerfiles until a registry is actually needed.

### 10.5 Local developer workflow after Phase 3

**Option A — full stack in Docker**

```text
docker compose up --build
```

SPA: `http://localhost:3001` pointed at `http://localhost:8000`.  
pgAdmin: `http://localhost:5050`.

**Option B — DB in Docker, APIs on host** (good for debugging)

```text
docker compose up postgres pgadmin
```

Then `dotnet run` Gateway / Identity / Book with localhost connection strings.

### 10.6 Phase 3 definition of done

- `docker compose up` starts postgres, pgadmin, identity, book, gateway.
- Login + borrow + report work through `http://localhost:8000`.
- Data survives compose restart (volumes).
- Stopping SQL Server does not affect the stack.
- README (when you write it later) documents ports and `.env`.

---

## 11. Frontend changes (minimal)

| When | Change |
|------|--------|
| Until Gateway cutover | Keep `VITE_API_BASE_URL` default `http://localhost:5285` |
| End of Phase 1 | Default to `http://localhost:8000` |
| Phase 3 | Docker frontend port `3001`; env `http://localhost:8000` (browser → host-mapped Gateway) |

Do not change path strings in `api.ts` if Gateway preserves `/api/Auth`, `/api/Book`, etc.

CORS: Gateway only. Origin: `http://localhost:3001`.

---

## 12. Risks and rollback

| Risk | Mitigation |
|------|------------|
| Split-brain stock if catalog moves before borrow | Flip Book + Borrow + Report in one Gateway change |
| JWT mismatch | Unify secrets before strangling any route |
| File URLs point at :8002 | Generate URLs with Gateway host; proxy static paths |
| Identity user deleted while borrow rows remain | Keep `UserId` as historical reference; do not cascade across DBs |
| SMTP secrets in git (already in monolith `appsettings.json`) | Move to user secrets / env in Phase 1; never put production passwords in compose committed files |
| Ocelot and multipart uploads | Test add-book and profile image through Gateway early |
| EF SQL Server migrations applied to Postgres | New InitialPostgres migrations only |

Rollback: point `VITE_API_BASE_URL` back to `:5285` and start the monolith. Do not drop `EBook` until Phase 3 has been stable.

---

## 13. Suggested implementation order (when implementation starts)

This is the execution checklist for a later coding session. Not work to do now.

1. Solution + ports + unified JWT + Ocelot route fix + delete duplicate Identity scaffolds.
2. Complete Identity (users, profile, SMTP, migrations).
3. Gateway slice: Auth/User/Profile → Identity; rest → monolith.
4. Complete Book including borrow/return/reports (SQL Server).
5. One-shot data copy; Gateway slice: Book/Borrow/Report → Book.
6. Frontend → Gateway; update `start-all.ps1`.
7. Switch EF to Npgsql; new migrations; pgAdmin; copy data from SQL Server.
8. Dockerfiles + compose + volumes + Docker gateway addresses.
9. End-to-end test; keep monolith in repo as rollback.

---

## 14. Success criteria

**Architecture**

- Exactly: Gateway + Identity + Book.
- CQRS retained inside Identity and Book.
- Monolith still present and runnable until an explicit later removal.

**Behavior**

- All current frontend API calls succeed via Gateway with the same JSON shapes.
- Admin and User roles unchanged.
- Borrow still changes availability atomically inside Book.

**Data**

- Two PostgreSQL databases, browsable in pgAdmin.
- Files on volumes, not in Mongo.

**Ops**

- docker-compose brings the system up without SQL Server.
- No Kubernetes, Kafka, or extra microservices.

---

## 15. Decisions log

| Decision | Choice | Why |
|----------|--------|-----|
| Gateway product | Use **Ocelot** | Matches the Saral reference and keeps local/Docker routes in simple JSON files. |
| Extra services | **None** | Lending/reports/files/notifications are not independent contexts at this size. |
| Lending location | **Book service** | Same transaction as `AvailableCount`. |
| Database isolation | **Two databases, one Postgres** | Real ownership without two servers. Avoid Saral’s shared-DB “microservices”. |
| Files | **Disk volumes** | Small images. |
| Messaging | **HTTP only** | One Book→Identity GET at most. |
| Migration style | **Strangler** | Monolith stays up; route-by-route cutover. |
| Path contract | **Keep existing `/api/Book` names** | Avoid a frontend rewrite. |
| JWT | **Shared secret, validate at Gateway and services** | Same as Saral, without session middleware. |

---

*End of plan. Implementation starts only after this document is accepted.*
