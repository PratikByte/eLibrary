# eLibrary Microservice 10-Step Tracker

**Last updated:** 11 September 2026  
**Current step:** Step 7 - Switch frontend to Gateway  
**Rule:** Complete each step in order. Keep the monolith available until Step 10.

| Step | Phase | Status | Depends on |
|---:|---|---|---|
| 1 | Foundation: ports, solution, JWT, safe monolith setup | Complete | - |
| 2 | Complete Identity service | Complete | Step 1 |
| 3 | Route Identity through Gateway | Complete | Step 2 |
| 4 | Complete Book catalog service | Complete | Step 3 |
| 5 | Route Book catalog through Gateway | Complete | Step 4 |
| 6 | Move borrowing, returning, and reports into Book | Complete | Step 5 |
| 7 | Switch frontend to Gateway | In progress | Step 6 |
| 8 | Create Docker Compose stack | Pending | Step 7 |
| 9 | Migrate service databases to PostgreSQL | Pending | Step 8 |
| 10 | Verify production cutover and retire monolith | Pending | Step 9 |

## Completion Checklist

- [x] Step 1 - Foundation
- [x] Step 2 - Identity service
- [x] Step 3 - Identity Gateway routing
- [x] Step 4 - Book catalog
- [x] Step 5 - Book Gateway routing
- [x] Step 6 - Borrowing and reports
- [ ] Step 7 - Frontend Gateway cutover (next)
- [ ] Step 8 - Docker Compose
- [ ] Step 9 - PostgreSQL migration
- [ ] Step 10 - Production cutover

## Port Map

| Component | Port |
|---|---:|
| Frontend | 3001 |
| Gateway | 8000 |
| Identity | 8001 |
| Book | 8002 |
| Monolith fallback | 5285 |
| PostgreSQL | 5432 |
| pgAdmin | 5050 |

Update the status and checklist after each verified step.

## Step 5 Verification Summary

- Book catalog API is forwarded by Ocelot from `/api/Book/*` to `http://localhost:8002/api/Book/*`.
- Book catalog routes require the JWT bearer token: unauthenticated `GET /api/Book/paged` returns `401`.
- Static covers are forwarded from `/book-covers/*` to Book port `8002` without JWT.
- The Gateway remains on `http://localhost:8000`; the Book service remains on `http://localhost:8002`.
- Database migrations are intentionally deferred until Step 9, after the service and Docker work is complete.

## Next Window Handoff

Step 6 is complete in `Services/Book`: borrowing, returning, available-book reporting, and overdue reporting use Book-owned handlers, repositories, and controllers. The existing `EBook` database was retained; its existing `Books` and `BorrowRecords` tables were verified and no dedicated Book database was created. Legacy `UserName` and `BookName` fields are runtime-only, and overdue book names are resolved from `Books`.

Gateway routes `/api/BorrowReturnBook/*` and `/api/Report/*` now target Book on port `8002` in both local and Docker Ocelot configurations. Build verification passed for Book and Gateway. Runtime smoke verification passed: Book Swagger returned `200`, and protected Book/Gateway circulation and report endpoints returned `401` without a token.

Next: complete Step 7 by switching the frontend API base URL and all frontend calls to Gateway port `8000`, then test login, catalog, borrow, return, and admin reports through Gateway.
