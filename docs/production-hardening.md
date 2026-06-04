# Production Hardening Guide

## Security Controls

- MFA is available through ASP.NET Identity authenticator tokens at `/api/auth/mfa/setup`, `/api/auth/mfa/enable`, and `/api/auth/mfa/disable`.
- JWT access tokens remain short lived and refresh tokens are stored as hashes. Login requires confirmed email and, when enabled, a valid MFA code.
- Rate limiting is enabled globally at 300 requests per minute per remote IP, with a stricter `/api/auth` policy of 10 requests per minute.
- IP restrictions are configured by `IpRestrictions` in API configuration. Keep `Enabled=false` for local development and enable an allowlist in production ingress environments.
- Security headers are applied to all API responses: content type sniffing protection, frame denial, referrer policy, permissions policy, and a restrictive CSP.
- Security audit logs are stored in `security_audit_logs` and can be reviewed through `GET /api/security/audit-logs` by Super Admin users.

## Performance

- Redis is used for AI response caching and reporting cache when `ConnectionStrings:Redis` is configured.
- Background AI generation is handled by `AIGenerationWorker` and the in-memory generation queue. Production workers should run in the API process count sized for queue latency targets.
- Database indexes support patient search, appointment calendar/status views, encounter timeline queries, AI generation queue/result lookups, billing status reporting, and audit-log review.
- Keep PostgreSQL autovacuum enabled and monitor slow queries on patient, appointment, reporting, and billing endpoints.

## Monitoring

- Sentry is enabled through `Sentry` configuration. Set `Sentry:Dsn` in production secrets and tune `TracesSampleRate` per environment.
- Serilog provides structured request and application logging. Configure a production sink such as Seq, Elasticsearch, Azure Monitor, CloudWatch, or OpenTelemetry Collector.
- Health endpoints:
  - `/health` for default health status.
  - `/health/live` for container liveness probes.
  - `/health/ready` for load balancer readiness probes.

## Backup Strategy

- PostgreSQL: run daily full backups and point-in-time recovery with WAL archiving. Retain at least 30 days online and 90 days cold storage.
- MinIO: enable bucket versioning for patient documents and replicate the bucket to a separate storage target.
- Redis: treat cache as disposable for AI/reporting cache, but keep configuration ready for persistence if future queue state moves into Redis.
- Secrets: keep JWT signing keys, billing provider credentials, Sentry DSN, MinIO credentials, and AI provider keys in a managed secret store.
- Test restores monthly by restoring PostgreSQL and MinIO into an isolated environment and running smoke tests.

## Disaster Recovery

- Target RPO: 15 minutes for PostgreSQL with WAL archiving; 24 hours for non-critical cache state.
- Target RTO: 4 hours for a regional rebuild when infrastructure templates and images are available.
- Recovery order:
  1. Provision network, PostgreSQL, Redis, MinIO, and secret store.
  2. Restore PostgreSQL backup and replay WAL to the desired recovery point.
  3. Restore or fail over MinIO patient-document storage.
  4. Deploy API and frontend images.
  5. Run EF migrations if the restored schema is behind the deployed version.
  6. Validate `/health/ready`, login, tenant lookup, patient search, appointments, and document download.

## Deployment Guide

- Build and publish API and frontend images from a clean commit.
- Apply database migrations before routing traffic to the new API version.
- Configure production settings:
  - `ConnectionStrings:DefaultConnection`
  - `ConnectionStrings:Redis`
  - `Jwt:SigningKey`
  - `Cors:AllowedOrigins`
  - `IpRestrictions`
  - `Minio`
  - `AI`
  - `PhilippinesBilling`
  - `Sentry`
- Use HTTPS at the edge and forward headers from the ingress/load balancer.
- Configure probes:
  - Liveness: `GET /health/live`
  - Readiness: `GET /health/ready`
- Run smoke tests after deployment:
  - Register/confirm/login flow.
  - Tenant and location list.
  - Patient search and profile.
  - Appointment calendar.
  - Encounter timeline.
  - Reporting dashboard.
  - Billing plan list.
  - Security audit-log visibility for Super Admin.
