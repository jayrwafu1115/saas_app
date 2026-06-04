# Final Architecture

## System Overview

Clinic Management SaaS is a modular monolith with a Next.js frontend, ASP.NET Core Minimal API backend, PostgreSQL persistence, Redis-backed caching, MinIO document storage, background AI processing, and Philippines-focused subscription billing integrations.

## Backend

- API host: `Clinic.Api`
- Application layer: CQRS commands, queries, validators, service contracts, and DTOs in `Clinic.Application`
- Domain layer: tenant, location, identity, patient, appointment, encounter, AI, reporting, billing, and security entities in `Clinic.Domain`
- Infrastructure layer: EF Core persistence, repositories, Identity, JWT, Redis caches, MinIO storage, AI providers, billing providers, and background workers in `Clinic.Infrastructure`

## Modules

- Multi-tenancy: tenants, locations, tenant resolver, tenant context, and tenant-scoped entities.
- Authentication and authorization: ASP.NET Identity, JWT, refresh tokens, roles, permissions, email verification, password reset, and MFA.
- Patient management: CRUD, search, pagination, soft delete, document upload, and patient timeline.
- Appointment management: appointment lifecycle, availability service, conflict detection, and calendar views.
- Clinical encounters: SOAP notes, vitals, diagnoses, prescriptions, timeline, print/PDF export support, and clinical audit logs.
- AI: OpenAI/Ollama providers behind `IAIProvider`, queue processing, caching, usage/cost/latency tracking, and persisted generated outputs.
- Reporting: dashboard KPIs, analytics charts, optimized reporting queries, Redis caching, Excel/PDF exports.
- Billing: Starter, Professional, and Enterprise subscriptions with trial support, usage tracking, feature limits, tenant restrictions, and GCash/Maya checkout abstractions.
- Production hardening: MFA, rate limiting, IP restrictions, security headers, security audit logs, health checks, Sentry, and structured logging.

## Data Stores

- PostgreSQL stores durable business data, Identity records, refresh/action tokens, generated AI outputs, billing state, audit logs, and soft-delete metadata.
- Redis stores reporting and AI response cache entries when configured.
- MinIO stores patient documents by object key with metadata persisted in PostgreSQL.

## Request Flow

1. The frontend sends API requests with tenant headers and JWT bearer tokens.
2. API middleware applies HTTPS redirection, security headers, IP checks, CORS, rate limiting, tenant resolution, authentication, and authorization.
3. Endpoints dispatch CQRS commands/queries or call focused application services.
4. Repositories persist data through EF Core.
5. Background workers process queued AI generations and update persisted outputs.
6. Logs, Sentry traces, health checks, and audit records support operations and compliance review.

## Deployment Topology

- Frontend container or static Next.js deployment.
- API container running ASP.NET Core.
- PostgreSQL primary database with backups and WAL archiving.
- Redis cache.
- MinIO object storage.
- Ingress/load balancer terminating HTTPS and forwarding trusted headers.
- Observability stack for logs, traces, errors, and metrics.

## Security Model

- Super Admin manages global platform settings, roles, subscriptions, and audit review.
- Clinic Owner and Clinic Admin manage tenant operations.
- Doctor and Nurse access clinical workspace features.
- Receptionist manages appointment and front-desk workflows.
- Patient role is reserved for future patient-portal implementation.
- Permissions are emitted as JWT claims and enforced through authorization policies.

## Operational Baseline

- Use secrets management for credentials and provider keys.
- Keep migrations versioned with releases.
- Run daily backups and monthly restore drills.
- Monitor health endpoints, request latency, background queue latency, Sentry events, PostgreSQL slow queries, Redis availability, and MinIO storage health.
