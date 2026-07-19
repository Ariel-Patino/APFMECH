---
name: APFMech IAM & Security Skill
description: Enforces enterprise‑grade authentication, authorisation, and token management using OAuth 2.0, OpenID Connect, and JWT for both .NET 10 backend and Angular frontend.
version: 1.0.0
---

# 02-authentication.skill.md

## Purpose

This skill governs all security, identity validation, authorisation rules, and secure token management across the **APFMech** solution. It establishes non‑negotiable standards for:

- **Backend (.NET 10)** – strict JWT validation, claim‑based authorisation, and resource‑level access control.
- **Frontend (Angular)** – OIDC client implementation (PKCE), secure token storage, HTTP interceptors, and route guarding.

All generated code, configuration, and infrastructure must comply with these rules to protect against common attack vectors (IDOR, XSS, token leakage, etc.) and ensure compliance with enterprise IAM best practices.

---

## Backend Authentication & Authorization Standards (.NET 10)

The backend must enforce authentication and authorisation at every layer, from the API gateway down to the data access logic.

### 1. JWT Bearer Token Validation (Strict)

- **Authentication scheme**: `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Mandatory validation parameters** (set in `Program.cs` or `appsettings.json`):
  - `ValidIssuer` – must match the trusted identity provider (IdP) URL (e.g., `https://login.microsoftonline.com/{tenant}/v2.0` or custom IdP).
  - `ValidAudience` – the API’s client ID (as registered in the IdP).
  - `ValidateLifetime = true` – tokens must have a valid `exp` claim and not be expired.
  - `ValidateIssuerSigningKey = true` – keys must be obtained from the IdP’s discovery endpoint (`/.well-known/openid-configuration`).
  - `RequireHttpsMetadata = true` – never accept tokens over HTTP in non‑development environments.
  - `ClockSkew` – set to a minimal value (e.g., 1 minute) to reduce reuse windows.

- **Token processing**:
  - Use the `OnTokenValidated` event to perform additional custom validation (e.g., check if the user is active in the local database).
  - **Never** disable default validation or accept tokens with missing required claims.

### 2. Claim‑Based Authorization

- **Use standard OpenID Connect claims**:
  - `sub` – user identifier (must map to the user’s internal GUID).
  - `role` – for role‑based access (e.g., `Admin`, `Manager`, `Mechanic`).
  - `email` – verified email address.
  - `name` – display name.
- **Authorization policies**:
  - Define policies in `Program.cs` using `AuthorizationPolicyBuilder` based on claims and roles.
  - Example:
    ```csharp
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("CanManageAssets", policy => policy.RequireClaim("permissions", "assets.manage"));
    });

    All endpoints that mutate state (POST, PUT, PATCH, DELETE) must have [Authorize] with at least one policy.

Read‑only endpoints (GET) may allow unauthenticated access only if explicitly documented for public data; by default, all endpoints require authentication.

### 3. Resource‑Level Authorization (Prevent IDOR)
- **Principle: Users must only access resources they own or have explicit permission for.**

- **Implementation: In every handler/service method that fetches or modifies an entity, the user’s identifier (sub claim) must be compared against the target resource’s owner ID.**

- **Techniques:**

    - Pass the sub claim to the Application layer via IHttpContextAccessor or a custom user context service.
    - In the handler, query the data store using both the resource ID and the user ID – if the resource does not exist for that user, return a 404 Not Found or 403 Forbidden.
    - Forbidden: Relying solely on front‑end UI hiding; all checks must be performed server‑side.
    - Example:
    ```csharp
    public async Task<MechanicDto> Handle(GetMechanicQuery query, CancellationToken cancellationToken)
    {
        var mechanic = await _repository.GetByIdAsync(query.MechanicId, cancellationToken);
        if (mechanic == null || mechanic.TenantId != _currentUser.TenantId)
            throw new UnauthorizedAccessException("You do not have access to this resource.");
        return mapper.Map<MechanicDto>(mechanic);
    }
    ```
### 4. Endpoint Security Policies
- **[Authorize] must be applied to all controllers/actions that are not explicitly marked [AllowAnonymous].**

- **[AllowAnonymous] is permitted only for:**

    - Public health/status endpoints (e.g., /health).
    - Authentication endpoints (login, logout, token refresh) – if they are part of the API.
    - Swagger documentation (if accessible only over HTTPS and with proper network restrictions).
    - Global fallback – if a controller has no attribute, it should be treated as requiring authentication (configure via FallbackPolicy).

## Frontend Security Standards (Angular)
    The Angular client must implement the OAuth 2.0/OIDC authorization code flow with PKCE (Proof Key for Code Exchange) for maximum security.

### 1. OIDC Client Protocol (PKCE)
- **Library: Use a well‑maintained OIDC client library such as angular-oauth2-oidc or oidc-client-ts.**

- **Configuration:**

    - Use the authorization code flow with PKCE – never use implicit flow (deprecated).
    - response_type = 'code'
    - scope must include openid profile email roles to retrieve claims.
    - Silent refresh must be enabled using an iframe or RefreshToken endpoint (if supported).
    - Redirect URIs: Must be exact match with those registered in the IdP (e.g., https://app.apfmech.com/auth-callback).
    - Logout: Clear local session and redirect to IdP logout endpoint to terminate SSO session.
### 2. Secure Token Storage
- **For browser storage:**
    - Never store tokens in localStorage or sessionStorage – these are vulnerable to XSS.

    - Preferred approach: store tokens in session‑only memory (e.g., a service variable) and rely on HTTP‑only cookies for storage if the backend sets them.

    - If cookies are used, they must be Secure, HttpOnly, SameSite=Strict, and have a short expiration.

    - For SPA without cookie support, use in‑memory storage with a refresh token rotation mechanism, ensuring that tokens are never persisted across page reloads (use silent refresh to obtain a new token on app restart).

- **Refresh tokens: If stored client‑side, they must be encrypted and stored in a session‑only storage. Prefer to delegate refresh to the backend using a secure endpoint.**

### 3. HTTP Interceptor for Token Attachment
- **Implement an Angular HttpInterceptor that:**

    - Automatically adds the Authorization: Bearer <access_token> header to all outgoing HTTP requests (except to the IdP token endpoint).
    - Handles token expiration: if a 401 is received, attempt a silent refresh (or redirect to login if refresh fails).
    - Avoid adding tokens to requests that are exempt (e.g., health checks)
- **Example:**
    ```csharp
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getAccessToken();
    if (token && !req.url.includes('/api/public/')) {
        req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
        });
    }
    return next.handle(req).pipe(
        catchError(error => {
        if (error.status === 401) {
            return this.authService.refreshToken().pipe(
            switchMap(newToken => {
                const newReq = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
                return next.handle(newReq);
            }),
            catchError(() => this.authService.logoutAndRedirect())
            );
        }
        return throwError(error);
        })
    );
    }
    ```
### 4. Route Guards (Functional)
- **Use Angular functional route guards (canActivate, canLoad, canMatch) to protect routes.**
- **Authentication guard:**
    - Checks if the user is authenticated (i.e., has a valid token and not expired).
    - If not, redirect to the login page.
- **Role‑based guard:**
    - Reads the role claim from the token and compares against required roles for that route.
    - Example:
    ```csharp
    export const adminGuard: CanActivateFn = (route, state) => {
        const auth = inject(AuthService);
        const roles = auth.getUserRoles();
        return roles.includes('Admin') ? true : router.createUrlTree(['/unauthorized']);
    };
    ```
- **Lazy‑loaded modules should also be protected with canLoad or canMatch to prevent unauthorized bundle loading**

### 5. XSS Prevention
- **Since tokens are not stored in localStorage, XSS attacks that attempt to read localStorage will not steal tokens.**

- **However, ensure that any user‑supplied data rendered in templates is sanitised – Angular’s default DOM sanitisation is sufficient, but avoid bypassSecurityTrust unless absolutely necessary.**

- **Validate all incoming user inputs and encode outputs to prevent injection.**

### 6. CORS and HTTPS
- **The backend must have CORS configured to allow only the Angular frontend origin(s).**
- **All communication must occur over HTTPS in production – no token transmission over plain HTTP.**
- **Enforce strict Content-Security-Policy headers to mitigate script injection.**