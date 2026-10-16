# High Level Design (HLD) — Customer Feedback

## 1. Purpose and scope

### 1.1 Purpose

Enable **store managers** to collect **customer feedback**, **categorize** submissions, and view **satisfaction trends** on a landing-page style dashboard. **Customers** can self-register, sign in, and submit structured feedback about store visits.

### 1.2 In scope (v1)

- Customer registration and authentication
- Feedback submission form (ratings, NPS, category, comments, optional anonymity)
- Manager feedback list with filters, recategorization, and status workflow
- Reporting summary (counts, averages, category breakdown, simple trend)
- Microservices backend (.NET 10), React 19 frontend, API gateway, RabbitMQ async integration
- Contract-first development (OpenAPI + event schemas)

### 1.3 Out of scope (v1)

- Multi-level store/region organization
- Email/SMS notifications
- AI-based categorization
- Real-time updates (SignalR)
- Production cloud landing zone (beyond docker-compose)

## 2. Stakeholders and actors

| Actor | Description |
| --- | --- |
| **Customer** | Registers via public API; submits and views own feedback |
| **Store manager** | Seeded user; views dashboard and manages all feedback for stores |
| **Developer (FE/BE)** | Implements against frozen contracts in parallel |

## 3. User stories (from requirements)

| ID | Story |
| --- | --- |
| US-1 | As a store manager, I want to collect and analyze customer feedback to improve operations and satisfaction |
| US-2 | As a customer, I want to submit feedback after a store visit |
| US-3 | As a store manager, I want a landing-page summary of satisfaction trends |
| US-4 | As a customer, I want to register and log in securely |

## 4. Functional decomposition

```mermaid
flowchart TB
  subgraph authMod [Authentication]
    REG[Register customer]
    LOGIN[Login / refresh]
    ME[Current profile]
  end

  subgraph fbMod [Feedback lifecycle]
    SUB[Submit feedback]
    LIST_C[My submissions]
    LIST_M[Manager list and filters]
    CAT[Recategorize]
    STAT[Update status]
  end

  subgraph rptMod [Reporting]
    SUM[Summary KPIs]
    PROJ[Event projection]
  end

  SUB --> EVT[FeedbackSubmitted]
  CAT --> EVT2[FeedbackUpdated]
  STAT --> EVT2
  EVT --> PROJ
  EVT2 --> PROJ
  PROJ --> SUM
```

### 4.1 Capability matrix

| Capability | Customer | Store manager |
| --- | --- | --- |
| Register | Yes | No (seeded) |
| Login | Yes | Yes |
| Submit feedback | Yes | No |
| View own feedback | Yes | No |
| View all feedback | No | Yes |
| Change category/status | No | Yes |
| View report summary | No | Yes |

## 5. System context

```mermaid
flowchart LR
  Customer[Customer browser]
  Manager[Manager browser]
  System[Customer Feedback platform]
  SQL[(SQL Server)]
  RMQ[RabbitMQ]

  Customer --> System
  Manager --> System
  System --> SQL
  System --> RMQ
```

External systems in v1: **none** (no payment, CRM, or email providers).

## 6. High-level component design

### 6.1 Frontend (presentation)

- **React 19** SPA with **React Router** for routes
- **Redux Toolkit + RTK Query** for server state and caching
- **MSW** for offline-first UI development against OpenAPI-shaped handlers
- Role-based route guards (`Customer` vs `StoreManager`)

### 6.2 API gateway (edge)

- **YARP** reverse proxy
- Routes: `/api/identity/*`, `/api/feedback/*`, `/api/reports/*`
- CORS for local frontend origin
- Does not own business data

### 6.3 Microservices

| Service | Bounded context | Persistence |
| --- | --- | --- |
| Identity | Users, credentials, JWT issuance, refresh rotation | `CF_Identity` |
| Feedback | Feedback aggregate, validation, publishing domain events | `CF_Feedback` |
| Reporting | Read model for analytics | `CF_Reporting` |

### 6.4 Shared kernel

`CustomerFeedback.BuildingBlocks`: JWT setup, Serilog, Problem Details middleware, pagination helpers, shared event DTOs, comment sanitization.

## 7. Data flow — submit feedback (happy path)

```mermaid
sequenceDiagram
  participant C as Customer
  participant FE as Frontend
  participant GW as Gateway
  participant FB as Feedback
  participant RMQ as RabbitMQ
  participant RP as Reporting

  C->>FE: Fill form and submit
  FE->>GW: POST /api/feedback + JWT
  GW->>FB: Forward
  FB->>FB: Validate and persist
  FB->>RMQ: FeedbackSubmitted
  FB-->>FE: 201 Created
  RMQ->>RP: Consume event
  RP->>RP: Upsert FeedbackFact
```

Manager dashboard reads **only** from Reporting (`GET /api/reports/summary`), not from Feedback.

## 8. Data flow — manager updates feedback

```mermaid
sequenceDiagram
  participant M as Manager
  participant FE as Frontend
  participant FB as Feedback
  participant RMQ as RabbitMQ
  participant RP as Reporting

  M->>FE: Change category or status
  FE->>FB: PATCH /api/feedback/{id}/...
  FB->>FB: Update row
  FB->>RMQ: FeedbackUpdated
  FB-->>FE: 200 OK
  RMQ->>RP: Update projection
```

## 9. Non-functional requirements (mapping)

| NFR (from spec) | HLD decision |
| --- | --- |
| Microservices + REST | Gateway + three APIs |
| Async where needed | RabbitMQ between Feedback and Reporting |
| Logging | Serilog structured JSON per service |
| Validation | Client + FluentValidation; sanitized comments |
| Exception handling | Global handler → Problem Details |
| Unit testing | xUnit per service; WebApplicationFactory + in-memory DB / bus |
| Security | JWT + RBAC; manager not self-register |
| Configuration | JSON + env; encrypt secrets in deployment |
| Performance | Indexes on filter columns; reporting pre-aggregated via projection |

## 10. Parallel development model

```mermaid
flowchart LR
  contracts[contracts freeze]
  feTrack[Frontend MSW + RTK]
  beTrack[Backend APIs + tests]
  integration[Gateway integration]

  contracts --> feTrack
  contracts --> beTrack
  feTrack --> integration
  beTrack --> integration
```

**Exit criteria:** Full UX demo on MSW; backend tests pass without UI; switching off MSW and pointing at gateway completes integration.

## 11. Technology stack summary

| Layer | Technology |
| --- | --- |
| UI | React 19, TypeScript, Vite |
| Client state | Redux Toolkit, RTK Query |
| API contracts | OpenAPI 3.0 YAML |
| API host | ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| DB | SQL Server (database per service) |
| Messaging | MassTransit + RabbitMQ |
| Gateway | YARP |
| Tests | xUnit, Microsoft.AspNetCore.Mvc.Testing |

## 12. Assumptions and constraints

- Small number of stores; `storeId` is a UUID (seeded store in [`contracts/stores.v1.json`](../contracts/stores.v1.json)).
- Clock skew for JWT validation: 30 seconds.
- Eventual consistency: dashboard may lag briefly after submit until consumer processes message.
- Same `Jwt:SigningKey` across Identity, Feedback, Reporting for token validation.

## 13. Document traceability

| HLD section | Detailed design |
| --- | --- |
| APIs and payloads | [LLD — REST API](lld.md#3-rest-api-design) |
| Tables and entities | [LLD — Data model](lld.md#4-data-model) |
| Deployment ports | [Architecture — Deployment](architecture.md#deployment-views) |
