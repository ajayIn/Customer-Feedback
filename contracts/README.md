# Contracts

Frozen v1 contracts shared by the React app and the .NET services. Change these before changing application contracts or client types so both sides stay aligned.

## OpenAPI

| File | Gateway prefix | Callers |
| --- | --- | --- |
| [openapi/identity.v1.yaml](openapi/identity.v1.yaml) | `/api/identity` | Register, login, refresh, current user |
| [openapi/feedback.v1.yaml](openapi/feedback.v1.yaml) | `/api/feedback` | Submit, list, recategorize, change status |
| [openapi/reporting.v1.yaml](openapi/reporting.v1.yaml) | `/api/reports` | Manager summary KPIs |

## Events

Published by Feedback and consumed by Reporting (MassTransit / RabbitMQ).

| File | When |
| --- | --- |
| [events/feedback-submitted.v1.json](events/feedback-submitted.v1.json) | A customer submits feedback (`status` is `New`) |
| [events/feedback-updated.v1.json](events/feedback-updated.v1.json) | A manager changes category or status |

## Other

| File | Contents |
| --- | --- |
| [errors/problem-details.md](errors/problem-details.md) | RFC 7807 error envelope for 400, 401, 403, 404, 409, and 500 |
| [stores.v1.json](stores.v1.json) | Seeded stores (v1 has Downtown Store) |

List endpoints use `page` (1-based) and `pageSize` (default 20, max 100) and return `{ "items", "page", "pageSize", "totalCount" }`.

Roles: `Customer` (public register) and `StoreManager` (seeded, not public signup). Field rules for a submission are documented in [plan.md](../plan.md).
