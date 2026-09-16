# eLibrary Microservice Migration Tracker

**Last updated:** 11 September 2026  
**Current step:** Step 10 - Initialize fresh databases and seed test data
**Rule:** Complete each step in order. Keep the monolith available until final production cutover.

| Step | Phase | Status | Depends on |
|---:|---|---|---|
| 1 | Foundation: ports, solution, JWT, safe monolith setup | Complete | - |
| 2 | Complete Identity service | Complete | Step 1 |
| 3 | Route Identity through Gateway | Complete | Step 2 |
| 4 | Complete Book catalog service | Complete | Step 3 |
| 5 | Route Book catalog through Gateway | Complete | Step 4 |
| 6 | Move borrowing, returning, and reports into Book | Complete | Step 5 |
| 7 | Switch frontend to Gateway | Complete | Step 6 |
| 8 | Standardize entity IDs and audit trails | Complete | Step 7 |
| 9 | Correct database architecture: PostgreSQL + MongoDB | Complete | Step 8 |
| 10 | Initialize fresh databases and seed test data | Pending | Step 9 |
| 11 | Correct backend persistence and file storage | Pending | Step 10 |
| 12 | Correct frontend API and media integration | Pending | Step 11 |
| 13 | Full application testing and verification | Pending | Step 12 |
| 14 | Create Docker Compose stack | Pending | Step 13 |
| 15 | Production cutover and retire monolith | Pending | Step 14 |

## Completion Checklist

- [x] Step 1 - Foundation
- [x] Step 2 - Identity service
- [x] Step 3 - Identity Gateway routing
- [x] Step 4 - Book catalog
- [x] Step 5 - Book Gateway routing
- [x] Step 6 - Borrowing and reports
- [x] Step 7 - Frontend Gateway cutover
- [x] Step 8 - Entity UUIDs, audit fields, and soft delete
- [x] Step 9 - Database architecture: PostgreSQL + MongoDB
- [ ] Step 10 - Fresh database initialization and test seed
- [ ] Step 11 - Backend persistence and file storage correction
- [ ] Step 12 - Frontend API and media correction
- [ ] Step 13 - Full application testing
- [ ] Step 14 - Docker Compose
- [ ] Step 15 - Production cutover

## Port Map

| Component | Port |
|---|---:|
| Frontend | 3001 |
| Gateway | 8000 |
| Identity | 8001 |
| Book | 8002 |
| Monolith fallback | 5285 |
| PostgreSQL | 5432 |
| MongoDB | 27017 |
| pgAdmin | 5050 |

Update the status and checklist after each verified step.

## Step 5 Verification Summary

## Step 8 Verification Summary

- Identity and Book persistent entities now inherit a service-owned `BaseEntity` with application-generated `Guid` IDs, UTC creation/update fields, audit actors, and soft-delete fields.
- `IdentityDbContext` and `BookDbContext` centrally populate audit fields from the JWT subject and apply global filters to hide soft-deleted rows.
- Book and Identity delete operations now perform soft deletes; the API contracts and service relationships use UUID values rather than integer database IDs.
- No concurrency-control fields, domain events, or external audit-trail DLL were added.
- `dotnet build` passed for both `eLibrary.Book.API` and `eLibrary.Identity.API` with zero warnings and zero errors on 11 September 2026.

- Book catalog API is forwarded by Ocelot from `/api/Book/*` to `http://localhost:8002/api/Book/*`.
- Book catalog routes require the JWT bearer token: unauthenticated `GET /api/Book/paged` returns `401`.
- Static covers are forwarded from `/book-covers/*` to Book port `8002` without JWT.
- The Gateway remains on `http://localhost:8000`; the Book service remains on `http://localhost:8002`.
- Database migration is intentionally deferred until the new Step 8 database architecture work is approved.

## Step 8 Data Model Standards

- **Primary keys:** use `Guid` in C# and PostgreSQL `uuid` columns for every persistent entity. Generate IDs in the application with `Guid.NewGuid()`; do not expose SQL Server integer IDs in the new service contracts.
- **Audit timestamps:** every persistent entity has UTC `CreatedAt` and `UpdatedAt`. `CreatedAt` is immutable; `UpdatedAt` changes on every update.
- **Audit actors:** where the operation is user-driven, record nullable `CreatedBy` and `UpdatedBy` UUID values from the JWT subject. System jobs may use a documented system actor.
- **Audit trail:** add an append-only audit record owned by the service, containing audit ID, entity type, entity UUID, action, actor UUID, timestamp, correlation ID, and safe before/after JSON snapshots. Never store passwords, tokens, or image bytes in audit data.
- **Write enforcement:** set timestamps and audit records centrally in the service persistence layer/unit of work so controllers and handlers cannot bypass auditing.
- **Data rule:** do not migrate old SQL Server records into the new environment. Start with fresh databases after the UUID and audit model is complete.
- **Seed rule:** add one repeatable development/test seed file after Step 8. It creates exactly two users and five sample books; it must not run automatically against production.

## Target Database Architecture

- **PostgreSQL:** one database named `elibrary`. Identity owns `Users` and `OtpRecords`; Book owns `Books` and `BorrowRecords`. Service ownership separates table access even though both services use the same database.
- **Service communication:** if Book needs user information, it calls the Identity Service API. Book must not query Identity tables directly or reference `IdentityDbContext`.
- **MongoDB:** one reusable `media` collection stores unstructured image bytes and metadata (`category`, filename, content type). PostgreSQL stores only the returned Mongo `ObjectId` string in `CoverImageMongoId` or `ProfileImageMongoId`; no image path or bytes are stored relationally.
- **Reusable media service:** both services expose the same `IMongoMediaStore` operations for save, read, and delete. Future media such as author photos uses the same Mongo settings and category field without a new persistence design.
- **Migration source:** no old-record migration is required for the new environment. Keep the existing SQL Server `EBook` database only as the rollback/reference system until cutover.
- **Service isolation:** use one PostgreSQL server and one `elibrary` database, with table ownership enforced by service code and API boundaries.
- **Docker timing:** add PostgreSQL and MongoDB to Docker Compose only after the application passes Step 13.

## Step 9 Verification Summary

- Identity and Book use PostgreSQL connection strings through EF Core Npgsql.
- MongoDB settings are shared by database and collection name in both services.
- Book covers and user profile images are stored as Mongo media documents; PostgreSQL stores only their Mongo IDs.
- A default profile image remains a local fallback until a user uploads a Mongo-backed image.
- `dotnet build` passed for `eLibrary.Identity.API` and `eLibrary.Book.API` with zero warnings and zero errors on 11 September 2026.
- Fresh PostgreSQL EF migrations were generated for both service DbContexts and applied to the shared `elibrary` database. Identity and Book use separate migration-history tables so both services can own tables in the same database.

## Next Window Handoff

Step 6 is complete in `Services/Book`: borrowing, returning, available-book reporting, and overdue reporting use Book-owned handlers, repositories, and controllers. The existing `EBook` database was retained; its existing `Books` and `BorrowRecords` tables were verified and no dedicated Book database was created. Legacy `UserName` and `BookName` fields are runtime-only, and overdue book names are resolved from `Books`.

Gateway routes `/api/BorrowReturnBook/*` and `/api/Report/*` now target Book on port `8002` in both local and Docker Ocelot configurations. Build verification passed for Book and Gateway. Runtime smoke verification passed: Book Swagger returned `200`, and protected Book/Gateway circulation and report endpoints returned `401` without a token.

Step 7 is complete. The frontend API client remains centralized in `FroentendLovable/src/lib/api.ts`; its default base URL is now the Gateway at `http://localhost:8000`, while `VITE_API_BASE_URL` remains available for other environments. No frontend source code references the Identity, Book, or monolith ports directly. The production frontend build passed. Lint still reports pre-existing repository-wide Prettier violations.

Step 7 is complete. The migration order has been changed so entity identity and auditing happen before database migration, and Docker is the final technical phase.

1. **Step 8:** implement UUID IDs, UTC audit timestamps, audit actors, append-only audit records, and centralized audit persistence.
2. **Step 9:** define fresh PostgreSQL schemas/databases, MongoDB image storage, connection settings, and EF migrations. Do not import old records.
3. **Step 10:** add one repeatable development/test seed file after the UUID/audit model is complete. Seed Pratik as `Admin`, Hemant as `User`, both with password `Elibrary@123`, plus five sample books. Keep this seed disabled for production.
4. **Step 11:** update Identity and Book persistence, image upload/read/delete services, and backend configuration.
5. **Step 12:** update frontend IDs, image URLs, upload flows, API models, and Gateway media routing.
6. **Step 13:** test login, profiles, book CRUD, cover upload/read/delete, borrow, return, availability, overdue reports, roles, audit records, seed data, and failure/rollback paths end to end.
7. **Step 14:** after Step 13 passes, create and verify Docker Compose for PostgreSQL, MongoDB, Identity, Book, Gateway, and frontend.
