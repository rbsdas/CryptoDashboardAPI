# CryptoDashboardAPI

A cryptocurrency dashboard REST API built with C# + ASP.NET Core (.NET 10), PostgreSQL, and JWT auth. Backend home assignment.

## Behavior Rules

- Do NOT introduce new architectural patterns beyond what is defined here
- Do NOT refactor structure unless explicitly asked
- Do NOT generate large multi-file code outputs
- Implement features in small, incremental steps
- For non-trivial tasks, propose a short plan before coding
- Keep solutions simple and aligned with this architecture

## Stack

- **Framework:** ASP.NET Core Web API (.NET 10)
- **Database:** PostgreSQL via EF Core (Npgsql)
- **Auth:** JWT Bearer (24h expiry, claims: `sub`, `email`)
- **Password hashing:** BCrypt.Net-Next
- **External data:** CoinGecko public API (no key required)
- **Docs:** Swagger/OpenAPI (Swashbuckle)
- **Tests:** xUnit + Moq + FluentAssertions
- **CI/CD:** GitHub Actions → Railway/Render

## Architecture

Controller → Service → DbContext → PostgreSQL. All external CoinGecko calls are isolated in `CoinGeckoClient` (typed `HttpClient`).

```
HTTP Request
     |
 [Controllers]     — parse/validate input, call service, return DTO
     |
 [Services]        — business logic, orchestration
     |         \
 [AppDbContext] [CoinGeckoClient]
     |              — typed HttpClient, all external calls here
 [PostgreSQL]
```

## Constraints

- This is a home assignment — do NOT overengineer
- Avoid unnecessary abstractions, layers, or patterns
- Prefer direct and readable solutions over flexible ones
- Do not introduce caching, messaging systems, or complex patterns unless explicitly required

## EF Core Guidelines

- Use `AppDbContext` directly — no repository pattern
- Keep entity configuration simple
- Use `AsNoTracking()` for read queries
- Avoid premature optimization

## Project Structure

```
src/CryptoDashboardAPI/
├── Controllers/
├── Services/
├── Models/
├── DTOs/
│   ├── Auth/
│   ├── Crypto/
│   ├── Watchlist/
│   └── External/
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Exceptions/
├── Middleware/
├── Program.cs
└── appsettings.json
```

## API Endpoints

| Method | Endpoint | Auth | Notes |
|---|---|---|---|
| POST | `/api/auth/register` | None | 201 or 409 |
| POST | `/api/auth/login` | None | returns `{ token, expiresAt }` |
| GET | `/api/cryptocurrencies` | None | paginated (`page`, `pageSize`) |
| GET | `/api/cryptocurrencies/{id}` | None | 404 if not found |
| POST | `/api/cryptocurrencies/refresh` | Required | 429 within 60s cooldown, 502 if CoinGecko fails |
| GET | `/api/cryptocurrencies/{id}/history` | None | `?days=` (1,7,14,30,90,180,365) |
| POST | `/api/watchlist` | Required | 409 duplicate, 400 coin not found |
| GET | `/api/watchlist` | Required | returns items with coin data |

## External API

All CoinGecko calls must go through `CoinGeckoClient`. Do NOT call external APIs directly from controllers or services.

- **Refresh:** `GET /coins/markets?vs_currency=usd&order=market_cap_desc&per_page=100&page=1`
- **History:** `GET /coins/{id}/market_chart?vs_currency=usd&days={days}`
- Refresh cooldown: 60 seconds in-memory (returns 429 with `Retry-After` if called too soon)
- CoinGecko failures throw `ExternalApiException` → 502

## Error Handling

`GlobalExceptionMiddleware` maps typed exceptions to RFC 7807 Problem Details:

| Exception | HTTP Status |
|---|---|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `ValidationException` | 400 |
| `CooldownException` | 429 + `Retry-After` header |
| `ExternalApiException` | 502 |
| `Exception` (catch-all) | 500 |

## Critical Files

- `src/CryptoDashboardAPI/Data/AppDbContext.cs` — entity config, relationships, constraints
- `src/CryptoDashboardAPI/Services/CryptoService.cs` — refresh orchestration, cooldown, history proxy
- `src/CryptoDashboardAPI/Services/CoinGeckoClient.cs` — all external API calls
- `src/CryptoDashboardAPI/Middleware/GlobalExceptionMiddleware.cs` — unified error response shapes
- `src/CryptoDashboardAPI/Program.cs` — DI root, JWT config, middleware pipeline, Swagger

## Important Notes

- The database starts empty. **Call `POST /api/cryptocurrencies/refresh` (authenticated) before listing coins.**
- Refresh fetches the top 100 coins from CoinGecko by market cap (USD only).
- Historical price data is a live CoinGecko proxy — not persisted.
- The 60-second refresh cooldown is in-memory and does not survive restarts.
- All `DateTime` values are stored and returned as UTC.
- `decimal` columns use `HasPrecision(18, 8)`.
