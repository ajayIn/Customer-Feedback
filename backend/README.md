# Backend

.NET 10 microservices behind a YARP gateway. Each service owns its database and exposes the paths in [`contracts/openapi`](../contracts/openapi).

| Process | Project | Port | Database |
| --- | --- | --- | --- |
| Gateway | `gateway/Gateway` | 5000 | — |
| Identity | `services/identity/Identity.Api` | 5101 | `CF_Identity` |
| Feedback | `services/feedback/Feedback.Api` | 5102 | `CF_Feedback` |
| Reporting | `services/reporting/Reporting.Api` | 5103 | `CF_Reporting` |

Shared code (JWT, Problem Details, pagination, event types) lives in `shared/CustomerFeedback.BuildingBlocks`.

Each service is four projects, named `{Service}.Domain`, `{Service}.Application`, `{Service}.Infrastructure`, and `{Service}.Api`:

| Project | Contents |
| --- | --- |
| Domain | Entities and rules. No EF Core, ASP.NET, or MassTransit. |
| Application | Use-case handlers, contracts, FluentValidation, and ports such as `IFeedbackRepository` and `ITokenIssuer`. |
| Infrastructure | EF Core, MassTransit, and ASP.NET Identity. `ApplicationUser` stays here because it extends `IdentityUser`. |
| Api | Composition root and minimal API routes. This is the project the start script runs. |

## Local development

From the repo root:

```powershell
.\scripts\start-backend.ps1
```

The script builds `backend/CustomerFeedback.slnx` and starts the four processes. Development connection strings in each service’s `appsettings.Development.json` use SQL Server on `AJAYPC` with Windows authentication. Create the catalogs first with [scripts/sql/create-databases-cf.sql](../scripts/sql/create-databases-cf.sql).

RabbitMQ must be reachable at `localhost:5672`. Feedback publishes `FeedbackSubmitted` and `FeedbackUpdated`; Reporting projects those events into its own tables. The manager dashboard reads only that projection.

Identity seeds a store manager on startup (`manager@store.local` / `ChangeMe!123` in Development). Customers register through `POST /api/identity/register`.

Public traffic goes through the gateway (`http://localhost:5000`). Direct service ports are for debugging.

## Tests

```powershell
dotnet test backend/CustomerFeedback.slnx
```

Test projects sit next to each API: `Identity.Api.Tests`, `Feedback.Api.Tests`, `Reporting.Api.Tests`.

## Logging

All hosts call `AddSerilogLogging()` and `UseCustomerFeedbackPipeline()` from `CustomerFeedback.BuildingBlocks`. Logs are written as **compact JSON** (one object per line) with `Service`, `Environment`, and `TraceId` on each request:

| Sink | Location |
| --- | --- |
| Console | stdout (PowerShell window, `docker compose logs`, etc.) |
| File | `{ContentRoot}/logs/{ApplicationName}-YYYYMMDD.json` (daily rolling) |

Example when you run Identity locally: `backend/services/identity/Identity.Api/logs/Identity.Api-20261018.json`.

Configure the folder with `Logging:File:Directory` (default `logs`) and retention with `Logging:File:RetainedFileCountLimit` (default `14` days). Use an absolute path for `Logging:File:Directory` to write outside the project folder. Log files are gitignored.

Development uses `Serilog:MinimumLevel:Default` = `Debug` in `appsettings.Development.json`. Override levels with the `Serilog` section in `appsettings` or environment variables (for example `Serilog__MinimumLevel__Default=Warning`).

Shutdown calls `Log.CloseAndFlush()` via `RunWithSerilog()`.

## Configuration

`appsettings.json` plus `appsettings.Development.json`, environment variables, and user-secrets. Do not commit connection strings, JWT signing keys, or seed passwords beyond the local Development defaults. Docker overrides are listed in [docker-compose.yml](../docker-compose.yml) and [.env.example](../.env.example).
