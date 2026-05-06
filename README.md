# Crypto Dashboard API

A backend REST API for a cryptocurrency dashboard built with ASP.NET Core 10, PostgreSQL, and the CoinGecko public API.

## Features

- Live cryptocurrency rates fetched from CoinGecko (top 100 by market cap)
- Single coin detail view
- Authenticated user watchlist (per-user, persisted in PostgreSQL)
- Historical price data for any tracked coin
- JWT authentication
- Swagger/OpenAPI documentation
- RFC 7807 Problem Details error responses

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 Web API |
| Database | PostgreSQL 17 via Entity Framework Core 10 (Npgsql) |
| Auth | JWT Bearer tokens |
| External API | CoinGecko Demo API |
| Docs | Swashbuckle / Swagger UI |
| Tests | xUnit, Moq, FluentAssertions |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 17](https://www.postgresql.org/download/)
- A free [CoinGecko Demo API key](https://www.coingecko.com/en/api) (required)

---

## Environment Variables

All configuration is read from environment variables in production. For local development, copy these into `appsettings.Development.json` (already gitignored).

| Variable | Description | Example |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=CryptoDashboard;Username=postgres;Password=postgres` |
| `Jwt__Secret` | JWT signing secret (min 32 chars) | `your-super-secret-key-here` |
| `Jwt__Issuer` | JWT issuer | `CryptoDashboardAPI` |
| `Jwt__Audience` | JWT audience | `CryptoDashboardAPI` |
| `Jwt__ExpiryHours` | Token lifetime in hours | `24` |
| `CoinGecko__ApiKey` | CoinGecko Demo API key | `CG-xxxxxxxxxxxx` |

---

## Run Locally

### 1. Clone and configure

```bash
git clone https://github.com/<your-username>/CryptoDashboardAPI.git
cd CryptoDashboardAPI
```

Create `src/CryptoDashboardAPI/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=CryptoDashboard;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "Issuer": "CryptoDashboardAPI",
    "Audience": "CryptoDashboardAPI",
    "ExpiryHours": "24"
  },
  "CoinGecko": {
    "ApiKey": "CG-your-demo-api-key-here"
  }
}
```

### 2. Create the database

```bash
createdb -U postgres CryptoDashboard
```

### 3. Run the API

```bash
cd src/CryptoDashboardAPI
dotnet run
```

The app applies migrations automatically on startup and listens on `http://localhost:5052`.

### 4. Open Swagger UI

Navigate to `http://localhost:5052/swagger` to explore all endpoints interactively.

> **First step:** Call `POST /api/cryptocurrencies/refresh` (requires auth) to populate the coin database from CoinGecko.

---

## Run Tests

```bash
cd src/CryptoDashboardAPI.Tests
dotnet test
```

---

## API Usage

### Authentication

All watchlist and refresh endpoints require a Bearer JWT. Register → log in → copy the token → set `Authorization: Bearer <token>` in requests.

### Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | — | Register a new user |
| `POST` | `/api/auth/login` | — | Log in, receive JWT |
| `GET` | `/api/cryptocurrencies` | — | List coins (paginated: `?page=1&pageSize=20`) |
| `GET` | `/api/cryptocurrencies/:id` | — | Coin detail |
| `POST` | `/api/cryptocurrencies/refresh` | ✓ | Refresh all coins from CoinGecko (60s cooldown) |
| `GET` | `/api/cryptocurrencies/:id/history` | — | Price history (`?days=7`; allowed: 1,7,14,30,90,180,365) |
| `POST` | `/api/watchlist` | ✓ | Add coin to watchlist |
| `GET` | `/api/watchlist` | ✓ | Get your watchlist with current prices |

All error responses follow [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807).

### Example flow

```bash
# Register
curl -X POST http://localhost:5052/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"you@example.com","password":"password123"}'

# Login — copy the token from the response
curl -X POST http://localhost:5052/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"you@example.com","password":"password123"}'

TOKEN="<paste token here>"

# Populate coins from CoinGecko
curl -X POST http://localhost:5052/api/cryptocurrencies/refresh \
  -H "Authorization: Bearer $TOKEN"

# List coins
curl http://localhost:5052/api/cryptocurrencies?pageSize=5

# Get Bitcoin's 7-day price history (use the id from the list response)
curl "http://localhost:5052/api/cryptocurrencies/<bitcoin-id>/history?days=7"

# Add Bitcoin to your watchlist
curl -X POST http://localhost:5052/api/watchlist \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"cryptocurrencyId":"<bitcoin-id>"}'

# View your watchlist
curl http://localhost:5052/api/watchlist -H "Authorization: Bearer $TOKEN"
```

---

## Deploy to Railway

### 1. Create a Railway project

Install the [Railway CLI](https://docs.railway.app/develop/cli) and log in:

```bash
railway login
railway init
```

### 2. Add a PostgreSQL database

In the Railway dashboard, add a **PostgreSQL** plugin to your project. Railway will inject a `DATABASE_URL` variable.

### 3. Set environment variables

In Railway → Variables, add:

```
ConnectionStrings__DefaultConnection=<copy from Railway PostgreSQL plugin>
Jwt__Secret=<generate a random 64-char string>
Jwt__Issuer=CryptoDashboardAPI
Jwt__Audience=CryptoDashboardAPI
Jwt__ExpiryHours=24
CoinGecko__ApiKey=<your CoinGecko demo key>
ASPNETCORE_ENVIRONMENT=Production
```

### 4. Deploy

```bash
railway up
```

Railway will detect the `Dockerfile` at the repo root and build/deploy automatically. Migrations run on startup via `db.Database.Migrate()`.

After deployment, Railway provides a public URL. The Swagger UI is available at `<your-url>/swagger`.

---

## Postman Collection

Import the file `CryptoDashboard.postman_collection.json` from the repo root to get a ready-to-use collection with all endpoints and example request bodies.

---

## Key Technical Decisions

- **Monolith over microservices** — appropriate scope for a 4-day assignment; layered Controller → Service → Repository pattern provides clean separation without distributed systems complexity.
- **CoinGecko over alternatives** — no API key required for the demo tier; `/coins/markets` returns all required fields in a single call; `/coins/{id}/market_chart` covers the history endpoint directly.
- **History not persisted** — time-series storage adds significant schema and indexing complexity; history is proxied live from CoinGecko on each request, which is the right tradeoff at this scale.
- **In-memory refresh cooldown** — sufficient for a single-instance deployment; documented as a known limitation.
- **`db.Database.Migrate()` on startup** — eliminates the need for a separate migration step during deployment.
