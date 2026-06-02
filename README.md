# Clinic Management SaaS

Phase 1 creates the production-ready solution foundation for a multi-tenant clinic management SaaS modular monolith. Business modules such as patients, locations, and appointments are intentionally not implemented yet.

## Structure

- `src/Clinic.Api` - ASP.NET Core 9 Web API, Swagger, JWT, Serilog, Sentry, health checks
- `src/Clinic.Application` - application layer, CQRS/MediatR registration, validation registration, shared interfaces
- `src/Clinic.Domain` - domain base entities, audit/soft-delete contracts, tenant foundation, role constants
- `src/Clinic.Infrastructure` - EF Core, PostgreSQL, Identity, Redis, migrations
- `tests/Clinic.Tests` - unit and API foundation tests
- `frontend` - Next.js, React, TypeScript, TailwindCSS, shadcn-style UI, TanStack Query, Zustand, Axios

## Run Locally

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Services:

- Frontend: `http://localhost:3000`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`
- MinIO Console: `http://localhost:9001`

## Backend Commands

```powershell
dotnet restore ClinicManagementSaaS.sln
dotnet build ClinicManagementSaaS.sln
dotnet test ClinicManagementSaaS.sln
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Clinic.Infrastructure --startup-project src/Clinic.Api
```

## Frontend Commands

```powershell
cd frontend
npm ci
npm run lint
npm run build
npm run dev
```

## Configuration

Runtime configuration is supplied through `appsettings.json`, `.env`, Docker Compose variables, or standard ASP.NET Core environment variable overrides. Replace the development JWT signing key before using shared environments.
