---
name: APFMech GenAI Critique Log Template
description: A structured markdown schema for logging Generative AI design critiques, architectural deviations, code modifications, and security reviews across the APFMech project.
version: 1.0.0
---

# 02-genai-critique-log-template.md

## Template Metadata

- **Purpose**: This file serves as the canonical log for tracking all AI‑driven code reviews, architectural critiques, security audits, and subsequent code modifications. It must be populated for every significant feature generation, refactoring, or security review session.
- **Retention**: Keep all logs for audit and historical reference. Do not delete old entries – append new entries at the top or bottom as designated.
- **Usage**: Fill out a new section (or a new copy of the template) for each distinct review session, feature module, or PR discussion.

---

## Log Entry Schema

### Entry Header

| Field | Description |
|-------|-------------|
| **Timestamp** | `YYYY-MM-DDTHH:MM:SSZ` (UTC) |
| **Author** | AI Agent persona (e.g., `Master System Architect`, `Elite .NET Developer`, `Defensive Security Auditor`) or developer name |
| **Session ID** | Unique identifier (e.g., GitHub PR number, or a generated GUID) |
| **Target Feature/Layer** | Specify the layer(s) affected: <br> - `.NET Domain` <br> - `.NET Application (CQRS)` <br> - `.NET Infrastructure (Persistence)` <br> - `.NET WebAPI (Controllers/Middleware)` <br> - `Angular UI (Components/Services/Guards)` <br> - `Shared/Cross‑cutting` |
| **Scope** | Brief description of the code under review (e.g., `CreateWorkOrderCommand`, `InventoryFacade`, `AuthGuard`). |

### Example Header

```yaml
Timestamp: 2025-07-18T14:30:00Z
Author: Defensive Security Auditor
Session ID: PR #42
Target Feature/Layer: .NET WebAPI + Angular UI
Scope: Implementation of Work Order archive endpoint and associated frontend route.
```
--- 

## Critique and Review Criteria Matrix
For each review session, the reviewer must evaluate the code against the following criteria and record the outcome (Pass / Fail / Warning) with a brief comment.

| Criteria | Status | Comments / Evidence |
|----------|--------|---------------------|
| **Architectural Alignment** (Clean Architecture/DDD) | `PASS` / `FAIL` / `WARN` | Example: "Controller directly called DbContext – violates layer dependency. **Fail.**" |
| **Coding Conventions** (C# 14 idioms, Angular Signals/Standalone) | `PASS` / `FAIL` / `WARN` | Example: "Command uses record with primary constructor – **Pass.**" |
| **Security Checkpoints** (JWT validation, IDOR prevention, XSS) | `PASS` / `FAIL` / `WARN` | Example: "Missing `[Authorize]` on POST endpoint – **Fail.**" |
| **Test Automation** (xUnit v3 TDD coverage and AAA) | `PASS` / `FAIL` / `WARN` | Example: "Handler has no unit test – **Fail.**" |
| **Observability** (Structured logging, Correlation ID) | `PASS` / `FAIL` / `WARN` | Example: "Logs missing correlation ID – **Warn.**" |
| **Performance / Scalability** | `PASS` / `FAIL` / `WARN` | Example: "N+1 query detected in repository – **Warn.**" |
| **Frontend Responsiveness / Accessibility** | `PASS` / `FAIL` / `WARN` | Example: "No ARIA labels on interactive elements – **Warn.**" |
| **Additional custom criteria (if applicable):** | | |
| `[Add custom criterion]` | `PASS` / `FAIL` / `WARN` | Comment |

## Action Item Log
Divide findings into two categories. Each item must include a clear description, the exact location (file path + line number if possible), and a proposed fix or workaround.

## 🔴 Refusal / Blockers (must be fixed before proceeding)

| # | Issue Description | Location | Proposed Fix | Severity |
|---|-------------------|----------|--------------|----------|
| 1 | Example: Missing `[Authorize]` on endpoint | `WorkOrdersController.cs:45` | Add `[Authorize(Policy = "CanManageWorkOrders")]` | **Critical** |
| 2 | IDOR – no ownership check in handler | `GetWorkOrderByIdQueryHandler.cs:28` | Add tenant/owner filter in repository call | **High** |
| 3 | Token stored in localStorage | `auth.service.ts:62` | Migrate to HTTP‑only cookie or memory storage | **High** |

## 🟡 Refactoring Recommendations (improvements for hygiene, maintainability, or performance)

| # | Improvement Suggestion | Location | Proposed Refactor | Priority |
|---|------------------------|----------|-------------------|----------|
| 1 | Use computed signal instead of manual subscription | `work-order-list.component.ts:34` | Replace `subscribe` with `computed` and `toSignal` | **Medium** |
| 2 | Extract magic string to constant | `work-order.service.ts:12` | Define `API_BASE = '/api/work-orders'` in constants file | **Low** |
| 3 | Add retry logic for transient failures | `work-order.facade.ts:45` | Wrap HTTP call with `retry(2)` operator | **Medium** |

## Approval Status
After all reviews and action items have been addressed, the final decision must be recorded here.

- **Approved for Merge** – All blockers resolved, recommendations noted.
- **Requires Re‑indexing** – Major refactoring or migration needed; a follow‑up session is required before merge.
- **Rejected due to Drift** – The changes violate fundamental architectural principles and must be redesigned from scratch.

**Sign‑off:** [Name/Agent] – [Date]

**Additional Notes:**
(Optional space for any extra context, trade‑offs, or decisions made during the review.)

---

This log shall be stored in the project documentation folder (e.g., /docs/critique-logs/) and referenced in pull requests to provide transparency and traceability of all AI‑assisted changes.