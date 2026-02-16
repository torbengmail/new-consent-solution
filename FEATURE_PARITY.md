# Feature Parity: Clojure (Old) vs .NET (New)

> Last updated: 2026-02-13
> Total: **~97 features | ~93 implemented (96%) | 115 test cases**

## Legend

- **Impl**: Implemented in .NET
- **Tests**: Number of test cases covering this feature (DB-level + API-level)

---

## 1. Health & Utility

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Health check | `GET /ping` | Yes | 2 |
| Swagger docs redirect | `GET /docs` | No | 0 |

---

## 2. Service API — Dictionaries (`/v1/serviceapi/dictionaries/`)

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Consent types | `GET /consent-types` | Yes | 2 |
| Consent purposes | `GET /consent-purposes` | Yes | 2 |
| Expression tags | `GET /expression-tags` | Yes | 2 |
| Expression statuses | `GET /expression-statuses` | Yes | 2 |
| Languages | `GET /languages` | Yes | 2 |
| ID types | `GET /id-types` | Yes | 2 |
| Owners | `GET /owners` | Yes | 2 |
| Products | `GET /products` | Yes | 1 |
| Owners with products | `GET /owners-products` | Yes | 1 |

---

## 3. Service API — Consents

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| List consents by owner | `GET /consents?owner_id=` | Yes | 2 |
| Filter by use case | `GET /consents?use_case_id=` | Yes | 2 |
| Get consent by ID | `GET /consents/:id` | Yes | 2 |
| Get consent info by expression | (internal query) | Yes | 1 |

---

## 4. Service API — User Consent Decisions

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Get expressions by product | `POST /user-consent-decisions` | Yes | 2 |
| Save decisions (+ Pub/Sub) | `PUT /user-consent-decisions` | Yes | 3 |
| Batch query (short) | `POST /user-consent-decisions-batch` | Yes | 2 |
| Batch query (paginated) | `GET /user-consent-decisions-batch` | Yes | 2 |
| Async batch export to S3 | `POST /user-consent-decisions-batch-new` | **No** | 0 |
| Async batch status | `GET /user-consent-decisions-batch-new/:id` | **No** | 0 |
| Upsert user consent | (internal query) | Yes | 1 |
| Update last seen date | (internal query) | Yes | 0 |

---

## 5. Service API — Decision History & Request Attempts

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Decision history | `GET /user-decision-history` | Yes | 2 |
| Decision request (1-step) | `POST /decision-request-attempts` | Yes | 1 |
| Decision request (2-step GET) | `GET /decision-request-attempts` | Yes | 2 |
| Decision request events | `POST /decision-request-attempt-events` | Yes | 1 |
| Request attempts data | (internal query) | Yes | 1 |
| Audit trail creation | (internal query) | Yes | 1 |

---

## 6. Service API — Texts & Translations

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Get translations | `GET /texts` | Yes | 3 |
| Get consent expression text | `GET /texts/consent-expression` | Yes | 1 |

---

## 7. Service API — User Consent Sources

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| List sources | `GET /user-consent-sources` | Yes | 1 |
| Create source | `POST /user-consent-sources` | Yes | 1 |
| Update source | `PUT /user-consent-sources` | Yes | 1 |
| Partial update source | `PATCH /user-consent-sources` | Yes | 1 |
| Delete source | `DELETE /user-consent-sources` | Yes | 1 |

---

## 8. Service API — Data Exports

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| JSON data dump | `GET /user-data-dump-json` | Yes | 3 |
| CSV data dump | `GET /user-data-dump-csv` | Yes | 1 |

---

## 9. Service API — Decision Updates

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Retract last decision | `PATCH /retract-last-user-consent-decision` | Yes | 1 |
| Update last decision | `PATCH /update-last-user-consent-decision` | Yes | 1 |

---

## 10. Service API — ID Management

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Master ID get/create | (internal query) | Yes | 3 |
| Create ID type | `POST /id-type` | Yes | 1 |
| Create ID mapping | `POST /id-mapping` | Yes | 1 |
| Delete test user | `DELETE /test-user` | Yes | 1 |

---

## 11. Service API — File Storage

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Upload file | `PUT /file-upload` | Yes | 0 |
| Get upload link | `GET /file-upload-link` | Yes | 1 |

---

## 12. Service API — Data Subject Rights (DSR)

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Telenor ID: get requests | `GET /telenor-id-dsr/requests` | Yes | 1 |
| Telenor ID: create request | `POST /telenor-id-dsr/requests` | Yes | 1 |
| Telenor ID: data dump links | `GET /telenor-id-dsr/data-dump-links` | Yes | 1 |
| Internal DSR: get requests | `GET /dsr/requests` | Yes | 1 |
| Internal DSR: create request | `POST /dsr/requests` | Yes | 1 |
| Internal DSR: update status | `PATCH /dsr/requests` | Yes | 1 |
| Auto-deletion request | `POST /dsr/auto-deletion-request` | Yes | 1 |

---

## 13. Service API — Dashboard

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Grouped consent list | `POST /dashboard/consents-list-grouped` | Yes | 1 |
| User info | `GET /users/user-info` | Yes | 1 |

---

## 14. Admin API — Authentication & RBAC

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Auth required (401) | all `/v1/adminapi/*` | Yes | 2 |
| RBAC permission checks (403) | all `/v1/adminapi/*` | Yes | 2 |

---

## 15. Admin API — User Management

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| List users | `GET /users` | Yes | 1 (403) |
| Create user | `POST /users` | Yes | 0 |
| Update user | `PUT /users/:id` | Yes | 0 |
| Delete user | `DELETE /users/:id` | Yes | 0 |
| Users reference data | `GET /users/reference-data` | Yes | 1 (403) |
| Current user info | `GET /userinfo` | Yes | 0 |

> Note: List users and reference data return 403 for the test admin user because the ADMIN role does not include user management permissions (`SHOW_USERS`, `SHOW_USERS_REFERENCE_DATA`). This is correct behavior — only the USER_ADMIN role has those permissions.

---

## 16. Admin API — Consent Management

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| List consents | `GET /consents` | Yes | 1 |
| Create consent | `POST /consents` | Yes | 1 |
| Get consent by ID | `GET /consents/:id` | Yes | 1 |
| Update consent | `PUT /consents/:id` | Yes | 1 |
| Soft delete consent | `DELETE /consents/:id` | Yes | 1 |
| Consent reference data | `GET /consents/reference-data` | Yes | 1 |

---

## 17. Admin API — Expression Management

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| List expressions | `GET /consents/:id/expressions` | Yes | 1 |
| Create expression | `POST /consents/:id/expressions` | Yes | 1 |
| Get expression by ID | `GET /expressions/:id` | Yes | 1 |
| Update expression | `PUT /expressions/:id` | Yes | 1 |
| Get expression texts | `GET /expressions/:id/texts` | Yes | 1 |
| Create expression text | `POST /expressions/:id/texts` | Yes | 1 |
| Update expression text | `PUT /expressions/:id/texts/:lang` | Yes | 1 |

---

## 18. Admin API — Tag Management

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| List tags | `GET /tags` | Yes | 1 |
| Get tag by ID | `GET /tags/:id` | Yes | 0 |
| Create + delete tag | `POST /tags`, `DELETE /tags` | Yes | 1 |

---

## 19. Admin API — Translation Management

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Get translations | `GET /texts` | Yes | 1 |
| Create text field | `POST /texts/field` | Yes | 1 |
| Update text field | `PUT /texts/field` | Yes | 1 |

---

## 20. User API (`/v1/userapi`)

| Feature | Clojure Endpoint | Impl | Tests |
|---------|-----------------|:----:|:-----:|
| Auth required (401) | all `/v1/userapi/*` | Yes | 1 |
| Dashboard decisions grouped | `POST /dashboard/decisions-list-grouped` | Yes | 1 |
| Get DSR requests | `GET /dsr-requests` | Yes | 1 |
| Create DSR request | `POST /data-subject-rights` | Yes | 0 |
| DSR texts | `GET /dsr-texts` | Yes | 1 |
| Legal texts | `GET /legal-texts` | Yes | 1 |
| Personal data dump links | `GET /pd-dump-links-extended` | Yes | 1 |
| Get random expressions | `POST /user-consent-decisions` | Yes | 1 |
| Save decisions | `PUT /user-consent-decisions` | Yes | 2 |
| Languages | `GET /languages` | Yes | 1 |
| UI settings | `GET /ui-settings` | Yes | 1 |

---

## 21. Consent Enricher Worker

| Feature | Clojure Component | Impl | Tests |
|---------|-------------------|:----:|:-----:|
| Enrich consent event | Kafka consumer | Yes (Pub/Sub push) | 3 |
| Key set by owner | enrichment config | Yes | 2 |
| Process/map results | enrichment logic | Yes | 2 |
| Health check | N/A (new) | Yes | 2 |

---

## 22. External Integrations

| Feature | Clojure | Impl | Tests |
|---------|---------|:----:|:-----:|
| Denmark API (user info, DSR) | Yes | Yes | 0 (mocked) |
| Zendesk (tickets, files) | Yes | Yes | 0 (mocked) |
| Email (SES -> SendGrid) | Yes | Yes | 0 (mocked) |
| File storage (S3 -> GCS) | Yes | Yes | 0 (mocked) |
| Event publishing (Kafka -> Pub/Sub) | Yes | Yes | 1 |

---

## 23. Cross-Cutting Concerns

| Feature | Clojure | Impl | Tests |
|---------|---------|:----:|:-----:|
| Basic Auth (Service API) | Yes | Yes | 1 |
| Bearer/OAuth Auth (Admin/User) | Yes | Yes | 3 |
| RBAC permissions | Yes | Yes | 2 |
| Owner-based data isolation | Yes | Yes | 0 |
| User data caching | Yes (Redis-like) | Yes (DB cache) | 0 |
| Audit trail (AOP) | Yes | Partial (decisions only) | 1 |
| Prometheus metrics | Yes | **No** | 0 |
| Request logging middleware | Yes | Partial (ASP.NET default) | 0 |

---

## Summary

| Category | Old Features | Implemented | Not Implemented | Tests |
|----------|:-----------:|:-----------:|:---------------:|:-----:|
| Service API endpoints | 43 | 41 | 2 | 56 |
| Admin API endpoints | 24 | 24 | 0 | 24 |
| User API endpoints | 10 | 10 | 0 | 11 |
| Consent Enricher | 1 worker | 1 worker | 0 | 9 |
| External integrations | 4 | 4 | 0 | 1 |
| DB query coverage | 15 classes | 15 classes | 0 | 25 |
| Cross-cutting | 4 | 2 | 2 | 7 |
| **Totals** | **~97** | **~93 (96%)** | **~4** | **115** |

---

## Not Implemented

1. **Async batch export** — `POST /user-consent-decisions-batch-new` + `GET /:batch_id` (S3 multipart streaming export with status tracking)
2. **Prometheus metrics** — request rate, latency, Kafka/PubSub producer metrics
3. **Swagger docs redirect** — `GET /docs`
4. **Full AOP audit trail** — only decision-level audit exists; admin-action-level audit (who created/updated/deleted consents, users, etc.) is not implemented

---

## Remaining Test Gaps

Implemented features with 0 test coverage:

| Area | Feature | Notes |
|------|---------|-------|
| User API | `POST /data-subject-rights` | User-facing DSR creation (service-level DSR create is tested) |
| Admin API | User CRUD (create, update, delete) | Test admin lacks `SHOW_USERS` permission; needs USER_ADMIN role |
| Admin API | `GET /userinfo` | Current user endpoint |
| Admin API | `GET /tags/:id` | Single tag lookup |
| Service API | `PUT /file-upload` | File upload (get-link is tested) |
| Service API | Update last seen date | Internal query, low risk |
| Cross-cutting | Owner-based data isolation | Middleware exists but no dedicated cross-owner denial test |
| Cross-cutting | User data caching | `UserDataCacheService` + `CacheQueries` — no coverage |

---

## Bugs Fixed During .NET Rewrite

1. **Permission name format mismatch** — DB stored `UPPER_SNAKE_CASE` (e.g., `READ_CONSENT`), controller checked `lower-kebab-case` (e.g., `read-consent`). All Admin API endpoints returned 403. Fixed by normalizing in `BasicAuthHandler`.

2. **Missing `user_id`/`id_type_id` columns on entity inserts** — `user_consent`, `request_attempt`, and `request_attempt_audit_trail` tables have `user_id TEXT NOT NULL` and `id_type_id INT NOT NULL` columns not mapped in EF entities. Caused insert failures. Fixed by adding properties to entities and passing values through the call chain.

3. **Translation JSON deserialization** — `augmented_translations` JSONB contains nested objects (`{"home":{"home_title":"..."}}`), but code deserialized as flat `Dictionary<string, string>`. Fixed with `FlattenTranslationJson` helper that handles both flat and nested structures.

4. **PostgreSQL enum type mismatch** — `dsr_tracking` table uses custom enum types (`type_dsr_processing`, `status_dsr_processing`). EF Core sent `text` parameters which PostgreSQL can't compare against enum columns. Fixed by rewriting DSR queries with raw SQL and explicit `::text`/`::enum` casts.

5. **`DsrTracking` column name mismatch** — Entity mapped `created_at`/`updated_at` but DB columns are `created_date`/`updated_date`.

6. **`IdMap` entity missing primary key** — Entity had a fake `id` column that doesn't exist in DB; actual PK is `id_type_id`.

7. **`UserConsent.ExtDate` nullability** — Entity had `bool?` but DB column is `boolean NOT NULL DEFAULT false`.
