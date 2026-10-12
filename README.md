# EkospolTest – Telefonní seznam

Jednoduchá webová aplikace pro správu telefonních čísel. Přihlášený uživatel může přidávat
telefonní čísla a označit je jako **veřejná** nebo **soukromá**. Veřejná čísla vidí všichni
přihlášení uživatelé, soukromá čísla vidí pouze jejich vlastník.

Aplikace se skládá ze čtyř částí:

| Část | Technologie | Popis |
|------|-------------|-------|
| **Frontend** | Angular 22, `keycloak-angular` | Uživatelské rozhraní v češtině |
| **Backend** | ASP.NET Core (.NET 10), Entity Framework Core | REST API pro telefonní čísla |
| **Databáze** | PostgreSQL 17 | Ukládá telefonní čísla i data Keycloaku |
| **Autentizace** | Keycloak 26.4 | Přihlášení uživatelů (OpenID Connect, JWT) |

## Obsah

- [Architektura](#architektura)
- [Rychlé spuštění (Docker Compose)](#rychlé-spuštění-docker-compose)
- [Lokální vývoj](#lokální-vývoj)
- [Struktura repozitáře](#struktura-repozitáře)
- [Backend](#backend)
- [Frontend](#frontend)
- [Keycloak](#keycloak)
- [Databáze a migrace](#databáze-a-migrace)
- [Konfigurace](#konfigurace)

## Architektura

```
┌──────────────┐   /api/*    ┌──────────────┐   EF Core   ┌──────────────┐
│  Frontend    │ ──────────▶ │   Backend    │ ──────────▶ │  PostgreSQL  │
│ (nginx :4200)│  + Bearer   │  (:5274)     │             │   (:5432)    │
└──────┬───────┘   token     └──────┬───────┘             └──────▲───────┘
       │ přihlášení (OIDC + PKCE)   │ ověření JWT                │
       ▼                            ▼                            │
┌─────────────────────────────────────────┐                      │
│            Keycloak (:8080)             │ ─────────────────────┘
│            realm „ekospol“              │   databáze „keycloak“
└─────────────────────────────────────────┘
```

1. Uživatel otevře frontend. Ten ho okamžitě přesměruje na přihlašovací stránku Keycloaku
   (`onLoad: 'login-required'`, tok Authorization Code s PKCE).
2. Po přihlášení frontend ke každému požadavku na `/api/...` automaticky přidá hlavičku
   `Authorization: Bearer <access token>`.
3. Backend ověří JWT token (vydavatel, audience `ekospol-api`) a z tokenu převezme
   identifikátor uživatele (`sub`) a uživatelské jméno (`preferred_username`).
4. Data se ukládají do PostgreSQL. Keycloak používá stejný server PostgreSQL,
   ale vlastní databázi `keycloak`.

## Rychlé spuštění (Docker Compose)

Požadavky: [Docker](https://www.docker.com/) s pluginem Docker Compose.

```bash
docker compose up -d
```

Spustí se tyto služby:

| Služba | Kontejner | Adresa |
|--------|-----------|--------|
| `postgres` | `ekospol-postgres` | `localhost:5432` |
| `keycloak-db-init` | `ekospol-keycloak-db-init` | jednorázově vytvoří databázi `keycloak` |
| `keycloak` | `ekospol-keycloak` | <http://localhost:8080> |
| `backend` | `ekospol-backend` | <http://localhost:5274> |
| `frontend` | `ekospol-frontend` | <http://localhost:4200> |

Po spuštění otevřete <http://localhost:4200> a přihlaste se testovacím účtem:

- **uživatel:** `testuser`
- **heslo:** `testuser`

Registrace nových uživatelů je v realmu povolena, takže si můžete vytvořit i další účty
a vyzkoušet, že soukromá čísla jiných uživatelů nejsou vidět.

Administrace Keycloaku: <http://localhost:8080/admin> (`admin` / `admin`).

### Hotové obrazy vs. lokální build

Ve výchozím stavu `docker-compose.yml` používá hotové obrazy z Docker Hubu:

- [`bekallaev/phonebook-api:1.0`](https://hub.docker.com/r/bekallaev/phonebook-api) (backend)
- [`bekallaev/phonebook-web:1.0`](https://hub.docker.com/r/bekallaev/phonebook-web) (frontend)

Chcete-li sestavit obrazy ze zdrojového kódu v repozitáři, odkomentujte u služeb `backend`
a `frontend` sekci `build:` a zakomentujte řádek `image:`. Poté spusťte:

```bash
docker compose up -d --build
```

### Zastavení a smazání dat

```bash
docker compose down        # zastaví kontejnery, data zůstanou ve volume postgres-data
docker compose down -v     # zastaví kontejnery a smaže i všechna data
```

## Lokální vývoj

Pro vývoj je nejpohodlnější spustit v Dockeru jen databázi a Keycloak a backend s frontendem
spouštět lokálně.

Požadavky:

- [.NET SDK 10](https://dotnet.microsoft.com/)
- [Node.js 22](https://nodejs.org/) a npm
- Docker (pro PostgreSQL a Keycloak)

### 1. Databáze a Keycloak

```bash
docker compose up -d postgres keycloak-db-init keycloak
```

### 2. Backend

```bash
cd src/EkospolTest.Backend
dotnet run --launch-profile http
```

Backend poběží na <http://localhost:5274>. Při startu automaticky aplikuje databázové migrace.
Výchozí konfigurace v `appsettings.json` počítá s PostgreSQL i Keycloakem na `localhost`.

### 3. Frontend

```bash
cd src/EkospolTest.Frontend
npm install
npm start
```

Vývojový server Angularu poběží na <http://127.0.0.1:53350> (port je nastaven v `angular.json`).
Požadavky na `/api` přeposílá na backend `http://localhost:5274` podle `proxy.conf.json`.

Další příkazy frontendu:

```bash
npm run build   # produkční build do dist/
npm run watch   # build v režimu sledování změn
npm test        # unit testy (Vitest)
```

### Visual Studio

Soubor `EkospolTest.slnx` obsahuje oba projekty (backend `.csproj` i frontend `.esproj`),
takže je lze otevřít a spustit společně ve Visual Studiu.

## Struktura repozitáře

```
.
├── docker-compose.yml              # celý stack: postgres, keycloak, backend, frontend
├── EkospolTest.slnx                # řešení pro Visual Studio
├── keycloak/
│   └── ekospol-realm.json          # realm „ekospol“ importovaný při startu Keycloaku
└── src/
    ├── EkospolTest.Backend/        # ASP.NET Core Web API
    │   ├── Controllers/            # PhoneNumbersController (+ ukázkový WeatherForecast)
    │   ├── Data/AppDbContext.cs    # EF Core kontext a mapování tabulky
    │   ├── Dtos/                   # požadavky a odpovědi API
    │   ├── Migrations/             # EF Core migrace
    │   ├── Models/PhoneNumber.cs   # entita telefonního čísla
    │   ├── Program.cs              # konfigurace služeb, JWT, migrace
    │   └── Dockerfile
    └── EkospolTest.Frontend/       # Angular aplikace
        ├── src/app/
        │   ├── keycloak.config.ts  # napojení na Keycloak
        │   ├── app.*               # hlavička s uživatelem a odhlášením
        │   └── phone-numbers/      # komponenta, modul a služba pro čísla
        ├── nginx.conf              # konfigurace nginx v Docker obrazu
        ├── proxy.conf.json         # proxy pro vývojový server
        └── Dockerfile
```

## Backend

ASP.NET Core Web API (.NET 10) s Entity Framework Core a poskytovatelem Npgsql.

### Autentizace

Backend používá JWT Bearer autentizaci (`Program.cs`):

- `Authority` – adresa realmu Keycloaku, proti které se ověřuje vydavatel tokenu.
- `MetadataAddress` – volitelná adresa OpenID konfigurace. V Dockeru backend kontaktuje
  Keycloak přes `http://keycloak:8080`, zatímco tokeny jsou vydávány pro
  `http://localhost:8080`.
- `Audience` – `ekospol-api` (do tokenu ji přidává mapper klienta `ekospol-frontend`).
- Jméno uživatele se čte z claimu `preferred_username`, identifikátor z claimu `sub`.

### REST API

Všechny endpointy vyžadují platný access token (`[Authorize]`).

#### `GET /api/phone-numbers`

Vrátí všechna **veřejná** čísla a **soukromá čísla přihlášeného uživatele**, seřazená podle `id`.

Odpověď `200 OK`:

```json
[
  {
    "id": 1,
    "number": "+420 123 456 789",
    "isPublic": true,
    "ownerId": "5b1c0f3e-...",
    "ownerUsername": "testuser"
  }
]
```

#### `POST /api/phone-numbers`

Vytvoří nové telefonní číslo. Vlastníkem se stane přihlášený uživatel.

Tělo požadavku:

```json
{
  "number": "+420 123 456 789",
  "isPublic": false
}
```

Validace pole `number`:

- povinné, nejvýše 32 znaků,
- musí odpovídat regulárnímu výrazu `^\+?[0-9 ()-]{3,31}$`
  (volitelné `+` na začátku, dále číslice, mezery, závorky a pomlčky).

Odpovědi:

- `201 Created` – vytvořené číslo (stejný formát jako v `GET`),
- `400 Bad Request` – neplatný vstup,
- `401 Unauthorized` – chybí nebo je neplatný token.

Příklad volání přes `curl` (token lze získat díky povolenému direct access grantu):

```bash
TOKEN=$(curl -s -X POST \
  http://localhost:8080/realms/ekospol/protocol/openid-connect/token \
  -d grant_type=password -d client_id=ekospol-frontend \
  -d username=testuser -d password=testuser | jq -r .access_token)

curl -H "Authorization: Bearer $TOKEN" http://localhost:5274/api/phone-numbers

curl -X POST -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"number":"+420 777 888 999","isPublic":true}' \
  http://localhost:5274/api/phone-numbers
```

V prostředí `Development` je navíc dostupný OpenAPI dokument na `/openapi/v1.json`.

> `WeatherForecastController` (`GET /WeatherForecast`) je pozůstatek ze šablony projektu
> a s aplikací nesouvisí.

## Frontend

Angular 22 aplikace (modulová, `standalone: false`) s uživatelským rozhraním v češtině.

- **`keycloak.config.ts`** – konfigurace `keycloak-angular`: server `http://localhost:8080`,
  realm `ekospol`, klient `ekospol-frontend`, vynucené přihlášení a PKCE (`S256`).
  Interceptor přidává bearer token ke všem požadavkům, jejichž URL začíná `/api`.
- **`App`** – hlavička „Telefonní čísla“ se jménem přihlášeného uživatele a tlačítkem
  „Odhlásit se“.
- **`PhoneNumbers`** – formulář pro přidání čísla (pole čísla, zaškrtávátko „Veřejné“,
  tlačítko „Přidat“) a tabulka se sloupci *Číslo*, *Veřejné* a *Vlastník*.
- **`PhoneNumbersService`** – volání `GET` a `POST` na `/api/phone-numbers`.

V Docker obrazu frontend obsluhuje nginx, který servíruje sestavenou aplikaci a požadavky
na `/api/` přeposílá na `http://backend:8080`.

## Keycloak

Realm je definován v souboru `keycloak/ekospol-realm.json` a importuje se při startu
(`start-dev --import-realm`). Data Keycloaku (uživatelé, relace) se ukládají do PostgreSQL,
takže přežijí restart kontejneru.

Obsah realmu `ekospol`:

- **klient `ekospol-frontend`** – veřejný klient (bez client secretu), standard flow s PKCE
  a povolený direct access grant. Povolené přesměrovací adresy:
  `http://localhost:4200/*`, `http://localhost:53350/*`, `http://127.0.0.1:53350/*`.
- **mapper audience** – přidává do access tokenu audience `ekospol-api`, kterou vyžaduje backend.
- **testovací uživatel** `testuser` / `testuser`.
- **lokalizace** – výchozí jazyk čeština (`cs`).
- **registrace** – uživatelé se mohou sami registrovat.

> Import realmu se provede jen tehdy, když realm ještě neexistuje. Pokud změníte
> `ekospol-realm.json`, je potřeba realm v administraci smazat nebo smazat volume
> (`docker compose down -v`).

## Databáze a migrace

Backend při startu volá `Database.Migrate()`, takže schéma se vytvoří a aktualizuje automaticky.

Tabulka `phone_numbers`:

| Sloupec | Typ | Popis |
|---------|-----|-------|
| `id` | `integer` (PK) | identifikátor |
| `phone_number` | `varchar(32)` | telefonní číslo |
| `is_public` | `boolean` | zda je číslo veřejné |
| `owner_id` | `varchar(255)` | ID vlastníka z Keycloaku (claim `sub`) |
| `owner_username` | `varchar(255)` | uživatelské jméno vlastníka |

Migrace:

- `20261008094857_InitialCreate` – vytvoření tabulky,
- `20261011120000_AddOwnerUsername` – přidání sloupce `owner_username`.

Nová migrace se vytvoří nástrojem `dotnet-ef`:

```bash
dotnet tool install --global dotnet-ef
cd src/EkospolTest.Backend
dotnet ef migrations add NazevMigrace
```

## Konfigurace

Nastavení backendu lze přepsat proměnnými prostředí (dvojité podtržítko odděluje sekce):

| Klíč | Proměnná prostředí | Výchozí hodnota |
|------|--------------------|-----------------|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;Database=ekospol;Username=postgres;Password=postgres` |
| `Keycloak:Authority` | `Keycloak__Authority` | `http://localhost:8080/realms/ekospol` |
| `Keycloak:MetadataAddress` | `Keycloak__MetadataAddress` | *(nenastaveno)* |
| `Keycloak:Audience` | `Keycloak__Audience` | `ekospol-api` |
| `Keycloak:RequireHttpsMetadata` | `Keycloak__RequireHttpsMetadata` | `false` |

Adresa Keycloaku pro frontend je zapsána přímo v `src/app/keycloak.config.ts`.

> **Upozornění:** Všechna hesla v repozitáři (`postgres`/`postgres`, `admin`/`admin`,
> `testuser`/`testuser`) slouží pouze pro lokální vývoj. Keycloak běží v režimu `start-dev`
> bez HTTPS. Pro produkční nasazení je nutné hesla změnit, zapnout HTTPS a spustit Keycloak
> v produkčním režimu.
