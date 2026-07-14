# Roadmap

CleanApiStarter intentionally begins with a compact foundation. Candidate additions should be driven by a derived project's requirements rather than enabled by default.

Possible future work:

- Refresh-token cleanup and per-device session management.
- Email verification, account recovery, and configurable lockout.
- Fine-grained permission policies.
- Rate limiting and additional security headers.
- API versioning and generated client examples.
- Durable audit delivery and retention jobs.
- CI workflows for build, test, package audit, and container scanning.
- Optional observability exporters.

Production orchestration is intentionally not planned here. The repository provides only a development Compose file and leaves deployment choices to each project.
