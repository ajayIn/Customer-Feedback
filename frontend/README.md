# Frontend

React 19 SPA for customers and store managers. API calls go through RTK Query. Shapes match [`contracts/openapi`](../contracts/openapi).

## Scripts

From this folder:

```powershell
npm install
npm run dev       # http://localhost:5173
npm run build
npm run lint
npm run preview
```

From the repo root, `.\scripts\start-frontend.ps1` installs dependencies if needed and starts the dev server. The Vite proxy sends `/api` to the gateway at `http://localhost:5000`.

## Environment

[`/.env.development`](.env.development):

| Variable | Meaning |
| --- | --- |
| `VITE_USE_MSW` | `true` serves MSW mocks and needs no backend. `false` uses the live gateway. |
| `VITE_API_BASE_URL` | Leave empty in dev so requests stay on the Vite origin and use the proxy. |

## Routes

| Path | Role | Page |
| --- | --- | --- |
| `/login` | Anonymous | Login |
| `/register` | Anonymous | Customer registration |
| `/feedback/new` | Customer | Submit feedback |
| `/feedback/mine` | Customer | Own submissions |
| `/` | Store manager | Dashboard summary |
| `/manager/feedback` | Store manager | List, recategorize, change status |

Unauthenticated users are sent to `/login`. The access token stays in memory. The refresh token is stored in `localStorage` for v1.

## Mocks

Handlers are in `src/mocks/handlers.ts` and follow the same paths and error envelope as the OpenAPI files. Turn them on with `VITE_USE_MSW=true` when the gateway is not running.
