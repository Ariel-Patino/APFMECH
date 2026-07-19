---
name: APFMech RESTful API Design Skill
description: Enforces RESTful conventions, uniform response envelopes, and correct HTTP status code usage for all ASP.NET Core 10 Web API endpoints in the APFMech project.
version: 1.0.0
---

# 03-rest-api.skill.md

## Purpose

This skill governs the behaviour, routing, and schema consistency of all **ASP.NET Core 10 Web API** endpoints in the **APFMech** solution. It establishes non‑negotiable standards for:

- Resource naming and URI structure.
- HTTP verb usage and semantic correctness.
- Query parameter conventions for filtering, sorting, and pagination.
- A uniform response envelope for all API responses.
- Standardised error handling using RFC 7807 Problem Details.
- Precise HTTP status code mapping for every scenario.

Every generated controller, action method, middleware, and response object must adhere to these rules to ensure a predictable, self‑describing, and developer‑friendly API surface.

---

## RESTful Architecture & Design Rules

### 1. Resource Naming and URI Structure

- **Use pluralised nouns** for resource collections.  
  ✅ `/api/work-orders` , `/api/mechanics` , `/api/assets`  
  ❌ `/api/workOrder` , `/api/getMechanics`
- **Use hyphens (-)** for multi‑word resources (e.g., `work-orders`, `preventive-maintenance`).
- **Base path**: All API endpoints must be prefixed with `/api/`.
- **Nested resources** – use nesting only when the relationship is **owned** and **dependent** (e.g., `/api/work-orders/{workOrderId}/tasks`). Avoid deep nesting (max 2 levels).

### 2. HTTP Verb Semantics

| Verb   | Purpose                                                   | Idempotent | Safe |
|--------|-----------------------------------------------------------|------------|------|
| `GET`  | Retrieve a resource or collection.                        | Yes        | Yes  |
| `POST` | Create a new resource.                                    | No         | No   |
| `PUT`  | **Full** update of a resource (replace entire entity).    | Yes        | No   |
| `PATCH`| **Partial** update of a resource (modify specific fields).| No         | No   |
| `DELETE`| Remove a resource.                                       | Yes        | No   |

- **Never** use `GET` for state‑changing operations.
- **Never** use `POST` for retrieval (use `GET` with query parameters).
- For `PUT`, the client must send the complete representation; missing fields are interpreted as null/default.
- For `PATCH`, use JSON Patch (`application/json-patch+json`) or a custom partial update DTO with optional fields.

### 3. Query Parameters for Filtering, Sorting, and Pagination

- **Filtering** – use query parameters for exact matches and ranges (e.g., `?status=active&createdAfter=2025-01-01`).
- **Sorting** – use `sort` parameter with field names and optional direction (e.g., `?sort=-createdAt` for descending, `+name` for ascending).
- **Pagination** – use `page` (1‑based) and `pageSize` (default 20, max 100) or `offset`/`limit`. Prefer `page`/`pageSize` for simplicity.
- **Example**:
    - GET /api/work-orders?status=open&sort=-priority&page=2&pageSize=10
- **All query parameters** must be explicitly defined in the action method signature (using `[FromQuery]` and a strongly‑typed model) – never parse raw strings manually.
---
## Response Envelope Standardization
### 1. Uniform JSON Wrapper Structure

All API responses (both success and error) must be wrapped in a consistent envelope to provide metadata alongside the payload.

**Success Response Envelope**:
```json
{
"success": true,
"data": { ... },                // Can be object, array, or null
"messages": [ "string" ],       // Optional informational messages (e.g., warnings)
"timestamp": "2025-01-01T12:00:00.000Z"
}
```
- **success – always true for 2xx responses.**
- **data – the actual response payload (or null for 204 No Content).**
- **messages – an array of non‑error messages (e.g., “Resource created successfully”).**
- **timestamp – ISO 8601 UTC timestamp of the response generation.**
- **Error Response Envelope (when not using Problem Details – see below):**
```json
    {
    "success": false,
    "error": {
        "code": "VALIDATION_ERROR",
        "message": "One or more validation errors occurred.",
        "details": { "field": "error description" }
    },
    "timestamp": "2025-01-01T12:00:00.000Z"
    }
```
- **However, for all error responses (4xx and 5xx), the API must return an RFC 7807 Problem Details object (see section 2 below). The envelope above applies only to successful responses.**
### 2. Error Responses – RFC 7807 Problem Details

- **All error responses (status codes 400–599) must conform to the RFC 7807 standard, with the application/problem+json media type.**

- **Mandatory fields:**

    - type – a URI reference that identifies the problem type (e.g., "https://api.apfmech.com/errors/validation").
    - title – a short, human‑readable summary (e.g., "Validation Error").
    - status – the HTTP status code.
    - detail – a human‑readable explanation (e.g., "The 'email' field is required").
    - instance – the URI of the request that caused the problem.

- **Extension fields – may include "errors" (for validation failures) as a dictionary of field‑to‑message arrays, or "traceId" for correlation.**

- **Implementation: Use the ProblemDetails class or the Microsoft.AspNetCore.Mvc.ProblemDetails factory, and custom middleware to capture unhandled exceptions and convert them to Problem Details.**

- **Never expose stack traces or internal exception details in production – map exceptions to appropriate status codes and generic messages, while logging the full details server‑side.**

- **HTTP Status Code Mapping Matrix**
| Scenario | Status Code | Description & Usage |
| :--- | :--- | :--- |
| Successful GET, PUT, PATCH, or DELETE (with content) | **200 OK** | Return the resource representation in the data field. |
| Successful POST (resource created) | **201 Created** | Return the newly created resource in the data field. Must include a Location header with the absolute URI of the new resource (e.g., `/api/work-orders/123`). |
| Successful request with no content to return | **204 No Content** | For DELETE operations or updates where the client doesn't need the updated entity. No response body. |
| Client input validation failure | **400 Bad Request** | Validation errors (e.g., missing required fields, invalid data format) – return a Problem Details object with `type="validation"` and errors detail. |
| Missing or invalid JWT token | **401 Unauthorized** | The request lacks authentication credentials or the token is expired/malformed. No additional info should be given beyond the standard `WWW‑Authenticate` header. |
| Authenticated user lacks required role/permission | **403 Forbidden** | The user is logged in but does not have the necessary role or claim to access the resource. Do not expose the reason to avoid information leakage. |
| Resource not found | **404 Not Found** | The requested resource (by ID or other identifier) does not exist. |
| Resource conflict (e.g., duplicate unique key) | **409 Conflict** | Use when a creation or update conflicts with existing state (e.g., duplicate email). Provide a Problem Details detail explaining the conflict. |
| Unhandled server‑side error | **500 Internal Server Error** | Only returned by the global exception handling middleware for unexpected exceptions. The Problem Details detail should be generic ("An unexpected error occurred"). |
| Request violates API rate limits | **429 Too Many Requests** | If rate limiting is implemented. |

### Additional Notes:

- For PUT and PATCH on non‑existent resources, return 404 Not Found.
- For DELETE, return 204 No Content on success, or 404 if the resource does not exist.
- Use 405 Method Not Allowed only if the HTTP verb is not supported for the resource – typically handled by the framework automatically.
- All redirects (3xx) are discouraged; use direct responses.

---

This API design skill is mandatory for all endpoint implementations. Controllers and middleware must be reviewed against these rules before merging any pull request.
