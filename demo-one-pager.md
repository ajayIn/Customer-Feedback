# Customer Feedback — Demo One-Pager

**Purpose:** Collect customer visit feedback; managers see KPIs and triage submissions.  
**Stack:** React 19 · .NET 10 microservices · YARP · SQL Server (`CF_*`) · RabbitMQ  
**API:** Live only — gateway **http://localhost:5000**, UI **http://localhost:5173**

---

## Quick start (live API)

**Prerequisites:** RabbitMQ on `localhost:5672`; SQL databases `CF_Identity`, `CF_Feedback`, `CF_Reporting` on **AJAYPC** ([SQL scripts](scripts/sql/)).

From repo root (PowerShell):

```powershell
.\scripts\start-backend.ps1    # four dotnet windows
.\scripts\start-frontend.ps1   # Vite on :5173
```

Or one step: `.\scripts\start-all.ps1` (backend first, then frontend).

Full guide: [demo.md](demo.md)

---

## URLs

| What | URL |
| --- | --- |
| **App (open this)** | http://localhost:5173 |
| **API gateway** | http://localhost:5000 |
| Identity / Feedback / Reporting (debug) | :5101 / :5102 / :5103 |

---

## Demo accounts

| Role | Email | Password |
| --- | --- | --- |
| **Manager** | `manager@store.local` | `ChangeMe!123` |
| **Customer** | Register in UI | min. 8 chars |

**Store:** `11111111-1111-1111-1111-111111111111` (Downtown)

---

## 5-minute script

1. **Customer:** Register → login → submit feedback → **My feedback**.
2. **Manager:** Login → **Dashboard** → **Manage feedback** → status **Reviewed** / change category → refresh dashboard.
3. **Architecture:** Feedback writes + RabbitMQ events; Reporting serves the dashboard.

---

## If something breaks

- **Fetch failed** → Run `start-backend.ps1`; gateway :5000 health = 200; `VITE_USE_MSW=false` in `frontend/.env.development`.
- **401** → Log in again (customer vs manager).
- **Empty dashboard** → RabbitMQ + Reporting (:5103); wait a few seconds after submit.
- **SQL** → Run [rename-databases-to-cf.sql](scripts/sql/rename-databases-to-cf.sql) or [create-databases-cf.sql](scripts/sql/create-databases-cf.sql).

Tests: `dotnet test backend/CustomerFeedback.slnx`
