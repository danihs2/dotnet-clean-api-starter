# Auditing

The audit module records security and operational events without coupling use cases to EF Core.

## Stored fields

`AuditLog` stores the event and entity identifiers, actor ID/email/role, IP address, user agent, message, JSON metadata, UTC timestamp, success flag, and optional error code/message.

Current request details are provided through `IRequestContext`. Events created outside an HTTP request still work; request-specific fields remain empty.

## Service usage

`IAuditService.LogAsync` records an explicit success or failure event. `ExecuteAsync` wraps an important asynchronous operation, records success, or captures the exception code/message and rethrows the original exception.

Audit persistence failures are written to structured logs and do not replace the original business exception. For requirements that demand guaranteed audit delivery, replace this best-effort synchronous implementation with a transactional or durable design.

## Events included

- Successful and failed login attempts.
- Successful registration and duplicate registration attempts.
- Successful and rejected role changes.
- Refresh-token rotation, revocation, failed refreshes, and reuse detection.
- Authenticated and SuperAdmin demo actions.
- Explicitly wrapped process failures.

`GET /api/audit` requires SuperAdmin and supports `page`, `pageSize`, `eventType`, and `success`. Page size is limited to 100.

## Sensitive data

Never put passwords, tokens, signing keys, connection strings, full authorization headers, or confidential payloads in `Message` or `Metadata`. Stack traces remain in internal structured logs and are not returned to clients.

Before production, define audit access, retention, export, redaction, tamper resistance, and deletion rules appropriate to the project.
