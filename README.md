# Customer Feedback

Store-manager product for collecting customer feedback, categorizing it, and showing satisfaction trends. The frontend (React 19) and backend (.NET 10) share versioned contracts so they can be built and demoed independently.

## Stack

| Area | Choice |
| --- | --- |
| Frontend | React 19, Vite, TypeScript, React Router, Redux Toolkit + RTK Query, MSW |
| Backend | .NET 10, YARP gateway, EF Core, FluentValidation, Serilog, xUnit |
| Data | SQL Server, one database per service (`CF_Identity`, `CF_Feedback`, `CF_Reporting`) |
| Messaging | MassTransit + RabbitMQ (`FeedbackSubmitted`, `FeedbackUpdated`) |
| Auth | JWT + roles. Customers self-register. Store managers are seeded. |

## Layout

```
contracts/     OpenAPI, event schemas, Problem Details
frontend/      React SPA
backend/
  gateway/     YARP API gateway
  services/    Identity, Feedback, Reporting
               each service: Domain, Application, Infrastructure, Api
  shared/      CustomerFeedback.BuildingBlocks
scripts/       Local start scripts and SQL helpers
Docs/          Architecture, HLD, LLD
```

## Run locally

Prerequisites: .NET 10 SDK, Node.js 20+, SQL Server (Windows auth on `AJAYPC`), RabbitMQ on `localhost:5672`.

Create the databases with [scripts/sql/create-databases-cf.sql](scripts/sql/create-databases-cf.sql), then from the repo root in PowerShell:

```powershell
.\scripts\start-all.ps1
```

That starts Identity, Feedback, Reporting, and the gateway in separate windows, then the Vite app in the current terminal.

| Surface | URL |
| --- | --- |
| App | http://localhost:5173 |
| Gateway | http://localhost:5000 |
| Gateway health | http://localhost:5000/health |

Seeded store manager: `manager@store.local` / `ChangeMe!123`. Default store is Downtown Store (`11111111-1111-1111-1111-111111111111`).

Frontend-only demo (no backend): set `VITE_USE_MSW=true` in `frontend/.env.development`, then `npm run dev` inside `frontend/`.

Full stack in Docker: copy [.env.example](.env.example) if you need to override secrets, then `docker compose up --build`.

## Tests

```powershell
dotnet test backend/CustomerFeedback.slnx
```

From `frontend/`: `npm run lint` and `npm run build`.

## Docs

| Document | Contents |
| --- | --- |
| [demo.md](demo.md) | Live API demo walkthrough |
| [plan.md](plan.md) | Domain, API table, parallel-work plan |
| [Docs/architecture.md](Docs/architecture.md) | As-built architecture |
| [contracts/README.md](contracts/README.md) | Contract index |
| [backend/README.md](backend/README.md) | Services, ports, databases |
| [frontend/README.md](frontend/README.md) | UI routes and env flags |
