# Customer Feedback — Demo Guide (live API)

Step-by-step guide to demo **Customer Feedback** against the **real backend** (YARP gateway + microservices). MSW mocks are not used in this flow.

**Shorter formats:** [demo-one-pager.md](demo-one-pager.md) · [Docs/demo-slides.md](Docs/demo-slides.md)

Related docs: [plan.md](plan.md) | [Docs/architecture.md](Docs/architecture.md) | [Docs/hld.md](Docs/hld.md) | [Docs/lld.md](Docs/lld.md)

---

## What you will show

| Audience | Flow |
| --- | --- |
| **Customer** | Register → login → submit feedback → view “My submissions” |
| **Store manager** | Login → dashboard (KPIs) → list feedback → change category/status |

Default store (v1): **Downtown Store** — id `11111111-1111-1111-1111-111111111111` (pre-filled on the feedback form).

All HTTP calls go to **`/api/*`** on the gateway: **http://localhost:5000** (Vite dev server proxies `/api` from **http://localhost:5173**).

---

## Quick start (recommended)

From the repo root in **PowerShell**:

```powershell
.\scripts\start-backend.ps1   # opens 4 terminals: Identity, Feedback, Reporting, Gateway
.\scripts\start-frontend.ps1    # http://localhost:5173
```

Single command (backend windows, then frontend in current terminal): `.\scripts\start-all.ps1`

| Open in browser | Health check |
| --- | --- |
| **http://localhost:5173** (app) | — |
| http://localhost:5000 (gateway) | http://localhost:5000/health |

Scripts live under [scripts/](scripts/). Frontend uses [frontend/.env.development](frontend/.env.development) with `VITE_USE_MSW=false`.

---

## Prerequisites

- .NET 10 SDK
- Node.js 20+ (`npm.cmd` on Windows if PowerShell blocks `npm.ps1`)
- SQL Server on **AJAYPC** (Windows authentication)
- **RabbitMQ** on `localhost:5672` (required for dashboard updates after feedback submit)
- Databases (empty catalogs are enough before first API start):
  - `CF_Identity`
  - `CF_Feedback`
  - `CF_Reporting`

**Rename old databases in SSMS:** run [scripts/sql/rename-databases-to-cf.sql](scripts/sql/rename-databases-to-cf.sql)  
**Or create new:** [scripts/sql/create-databases-cf.sql](scripts/sql/create-databases-cf.sql)

Connection strings: each service `appsettings.Development.json` → `Data Source=AJAYPC;Initial Catalog=CF_*;...`

---

## 1. Start RabbitMQ

Install and run RabbitMQ locally, or (if Docker is available):

```bash
docker compose up rabbitmq -d
```

Management UI (optional): http://localhost:15672 (`guest` / `guest`)

---

## 2. Start backend

Prefer **`.\scripts\start-backend.ps1`** (builds once, starts four processes in separate windows).

**Manual alternative** — set `ASPNETCORE_ENVIRONMENT=Development`, build once, then one `dotnet run --no-build` per service in its **own terminal**:

```powershell
cd "D:\Customer Feedback"
dotnet build backend/CustomerFeedback.slnx
$env:ASPNETCORE_ENVIRONMENT = "Development"

dotnet run --no-build --no-launch-profile --urls http://localhost:5101 --project backend/services/identity/Identity.Api/Identity.Api.csproj
dotnet run --no-build --no-launch-profile --urls http://localhost:5102 --project backend/services/feedback/Feedback.Api/Feedback.Api.csproj
dotnet run --no-build --no-launch-profile --urls http://localhost:5103 --project backend/services/reporting/Reporting.Api/Reporting.Api.csproj
dotnet run --no-build --no-launch-profile --urls http://localhost:5000 --project backend/gateway/Gateway/Gateway.csproj
```

| Service | URL | Health |
| --- | --- | --- |
| **Gateway** (browser entry) | http://localhost:5000 | `/health` |
| Identity | http://localhost:5101 | `/health` |
| Feedback | http://localhost:5102 | `/health` |
| Reporting | http://localhost:5103 | `/health` |

On first start, EF **creates tables** in the three `CF_*` databases. Identity **seeds** the store manager (see below).

---

## 3. Start frontend (live API)

Run **`.\scripts\start-frontend.ps1`** from the repo root, or:

```powershell
cd frontend
npm.cmd install
npm.cmd run dev
```

`frontend/.env.development` must keep `VITE_USE_MSW=false` and empty `VITE_API_BASE_URL`. Open **http://localhost:5173** — `/api` is proxied to **http://localhost:5000** ([vite.config.ts](frontend/vite.config.ts)).

---

## Demo accounts

| Role | Email | Password |
| --- | --- | --- |
| **Store manager** (seeded) | `manager@store.local` | `ChangeMe!123` |
| **Customer** | Register in the app | min. 8 characters |

Override manager seed: `Seed:ManagerEmail` / `Seed:ManagerPassword` in [Identity appsettings.Development.json](backend/services/identity/Identity.Api/appsettings.Development.json) or [.env.example](.env.example).

---

## Customer demo path

1. **Register** → new customer account.
2. **Login** → **Submit feedback** (ratings, NPS, category, comments).
3. **My feedback** — list of that user’s submissions.

**Backend:** `POST /api/feedback` → `CF_Feedback`; **FeedbackSubmitted** event → RabbitMQ → Reporting updates `CF_Reporting` (allow a few seconds before the manager dashboard changes).

---

## Store manager demo path

1. **Login** as `manager@store.local` / `ChangeMe!123`.
2. **Dashboard** (`/`) — `GET /api/reports/summary` from the read model.
3. **Manage feedback** — change **category** or **status**; **FeedbackUpdated** events refresh reporting.
4. Refresh dashboard to see updated KPIs.

---

## Optional: verify APIs with curl

Replace `TOKEN` with `accessToken` from login.

```powershell
curl -s -X POST http://localhost:5000/api/identity/login `
  -H "Content-Type: application/json" `
  -d "{\"email\":\"manager@store.local\",\"password\":\"ChangeMe!123\"}"

curl -s http://localhost:5000/api/reports/summary `
  -H "Authorization: Bearer TOKEN"
```

---

## Docker Compose (live API, container SQL)

Uses **sa** + `MSSQL_SA_PASSWORD`, not AJAYPC Windows auth.

```powershell
copy .env.example .env
docker compose up --build
```

| URL |
| --- |
| Frontend http://localhost:5173 |
| Gateway http://localhost:5000 |
| RabbitMQ UI http://localhost:15672 |

Set `VITE_USE_MSW=false` in the frontend image/env if you customize the compose frontend service.

---

## 10-minute demo script

1. **Intro (1 min)** — Structured feedback + manager visibility.
2. **Customer (4 min)** — Register → submit feedback (include a low score + comment) → **My feedback**.
3. **Manager (4 min)** — Login → dashboard → triage one item → refresh dashboard.
4. **Architecture (1 min)** — Gateway, three services, RabbitMQ projection ([Docs/architecture.md](Docs/architecture.md)).

---

## Troubleshooting

| Issue | What to check |
| --- | --- |
| Network / failed fetch | Gateway on **5000**; all four backends up; `VITE_USE_MSW=false`; restart `npm.cmd run dev` after env change |
| 401 on feedback/report | Log in again; customer vs manager routes |
| Dashboard empty after submit | RabbitMQ running; Reporting on **5103**; wait 2–5 s for consumer |
| SQL errors | `CF_*` databases exist on AJAYPC; run rename/create SQL scripts; restart APIs |
| Manager login fails | Identity running; Development config; password `ChangeMe!123` |
| CORS | Use UI on **5173** or call **5000** gateway only |
| Build “file in use” | Use `.\scripts\start-backend.ps1` or build once, then `dotnet run --no-build` per service |

---

## Tests before demo (optional)

```powershell
dotnet test backend/CustomerFeedback.slnx
```

---

## Contract reference

- OpenAPI: [contracts/openapi/](contracts/openapi/)
- Events: [contracts/events/](contracts/events/)
- Errors: [contracts/errors/problem-details.md](contracts/errors/problem-details.md)
