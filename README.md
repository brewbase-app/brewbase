# BrewBase

Projekt inżynierski: platforma społecznościowa do odkrywania kaw, tworzenia receptur parzenia i wymiany wiedzy w społeczności kawowej.

Architektura backendu to **modularny monolit** (jedna aplikacja ASP.NET Core, logiczny podział na moduły i serwisy). 
Baza danych jest projektowana **database-first** - schemat PostgreSQL jest źródłem prawdy, encje EF pochodzą ze schematu.

## Funkcje

- **Katalog kaw** — przeglądanie, filtrowanie, oceny i ulubione
- **Wiki kawowe** — artykuły o kawach, krajach, metodach parzenia i palarniach (moderacja przez admina)
- **Przepisy** — tworzenie, publikacja, oceny i ulubione (publiczne / prywatne)
- **Cupping** — sesje degustacyjne z wieloma kawami i ocenami
- **Notatki** — szybkie zapiski użytkownika
- **Wyszukiwarka globalna** — kawa, przepisy, wiki, użytkownicy (PostgreSQL: `pg_trgm`, normalizacja tekstu)
- **Rankingi** — snapshoty kaw, przepisów i aktywności użytkowników (`refresh_*` w bazie)
- **Rekomendacje** — dopasowanie kaw i przepisów do preferencji użytkownika
- **Preferencje / onboarding** — quiz smakowy, regiony, metody parzenia
- **Społeczność** — profile, obserwowanie użytkowników
- **Moderacja** — zgłoszenia treści, approve/reject artykułów wiki
- **Powiadomienia** - m.in. dot. warstwy społecznościowej

## Stack technologiczny

| Warstwa | Technologie |
|---------|-------------|
| Frontend | React, Vite, React Router |
| Backend | ASP.NET Core 8, Entity Framework Core |
| Baza danych | PostgreSQL (database-first) |
| Autoryzacja | JWT |
| Testy backendu | xUnit, SQLite (integracja), Testcontainers + PostgreSQL 18 (integracja PG) |

## Struktura projektu

```
brewbase/
├── backend/
│   ├── brewbase.server/              — API ASP.NET Core
│   ├── brewbase.server.Tests/        — testy integracyjne na SQLite (~286)
│   └── brewbase.server.Tests.Postgres/ — testy integracyjne na PostgreSQL (26)
├── frontend/                         — aplikacja React (SPA)
└── database/
    ├── schema.sql                    — schemat bazy (źródło prawdy)
    ├── migrations/                   — historyczne patche SQL
    └── seed_*.sql                    — dane startowe / demo
```

## Testy backendu

| Projekt | Baza | Liczba testów | Co weryfikuje |
|---------|------|---------------|---------------|
| `brewbase.server.Tests` | SQLite in-memory | **286** | Endpointy HTTP, logika aplikacji, CRUD, auth, cupping, przepisy, raporty itd. |
| `brewbase.server.Tests.Postgres` | PostgreSQL 18 (Docker / Testcontainers) | **26** | SQL specyficzny dla PG: global search, ranking refresh, rekomendacje, constrainty wiki, catalog approve, normalized unique, `EF.Functions.ILike` |

**Razem: ~300 testów.** CI (`.github/workflows/dotnet.yml`) uruchamia `dotnet test brewbase.sln`.
Testy PostgreSQL wymagają **działającego Dockera**.

### Uruchomienie testów

```bash
cd brewbase/backend

# wszystkie testy (SQLite + PostgreSQL)
dotnet test brewbase.sln

# tylko SQLite (bez Dockera)
dotnet test brewbase.server.Tests/brewbase.server.Tests.csproj

# tylko PostgreSQL (wymagany Docker)
dotnet test brewbase.server.Tests.Postgres/brewbase.server.Tests.Postgres.csproj
```

Scenariusze zależne od PostgreSQL (np. wyszukiwanie `ILike`, funkcje `refresh_*`, partial unique index na wiki) **nie są duplikowane na SQLite**.

## Środowiska

### Primary environment (docelowe)

Głównym środowiskiem projektu jest **uczelniana VM** ze wspólną bazą PostgreSQL i wspólnym deploymentem (backend + frontend).

### Local environment (development / fallback)

**Docker PostgreSQL** (lub lokalna instalacja) służy do developmentu i jako fallback, gdy VM jest niedostępna. Ten sam flow bootstrapu bazy, inny host w connection stringu.

---

## Bootstrap bazy danych

Źródłem prawdy schematu jest `brewbase/database/schema.sql`. Katalog `brewbase/database/migrations/` to **historyczne patche**.

### Kolejność (pełne demo danych)

Wykonaj skrypty **w tej kolejności** na świeżej bazie:

```
schema.sql → seed_init.sql → seed_wiki.sql → refresh_all_rankings()
```

| Krok | Plik / polecenie | Po co |
|------|------------------|--------|
| 1 | `brewbase/database/schema.sql` | Tabele, constrainty, funkcje `refresh_*` |
| 2 | `brewbase/database/seed_init.sql` | Katalog, użytkownicy testowi, przykładowe receptury |
| 3 | `brewbase/database/seed_wiki.sql` | Artykuły wiki (kawy, kraje, metody parzenia, palarnie) |
| 4 | `SELECT refresh_all_rankings();` | Wypełnienie tabel snapshot rankingów |

**`seed_wiki.sql`** jest częścią pełnego zestawu demo — bez niego wiki i część katalogu będą wyglądały na puste. Skrypt można uruchamiać wielokrotnie (dopisuje brakujące artykuły, nie kasuje danych użytkowników).

**`refresh_all_rankings()`** przelicza rankingi do tabel `*_ranking`. Backend czyta stamtąd listy rankingowe; po samym seedzie bez tego kroku strona rankingów może być pusta. Alternatywa: `POST /api/ranking/refresh` (wymaga uprawnień admina).

### pg_cron (opcjonalnie, infrastruktura VM)

Plik `brewbase/database/cron/pg_cron.sql` rejestruje **godzinny** refresh rankingów przez rozszerzenie pg_cron. Uruchamiaj go **tylko** na PostgreSQL z pg_cron (typowo VM uczelni), **po** `schema.sql`.

- Domyślny obraz Docker `postgres` **nie** zawiera pg_cron.
- Lokalnie zamiast cronu: `SELECT refresh_all_rankings();` lub endpoint refresh powyżej.

---

## Uruchomienie lokalne (fallback)

### Wymagania

- .NET 8 SDK
- Node.js (npm)
- PostgreSQL 16+ (Docker lub lokalna instalacja)
- Docker

### Baza danych

1. Utwórz bazę PostgreSQL
2. Wykonaj bootstrap z sekcji [Bootstrap bazy danych](#bootstrap-bazy-danych).

Konta testowe po seedzie (hasło: `Test123!`): `kawosz`, `maja`, `admin`.

### Backend

```bash
cd brewbase/backend/brewbase.server
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=brewbase;Username=...;Password=..."
dotnet user-secrets set "Jwt:Key" "twoj-tajny-klucz-min-32-znaki"
dotnet run
```

API: `http://localhost:5025` · Swagger w trybie Development.

### Frontend

```bash
cd brewbase/frontend
npm install
npm run dev
```

Aplikacja: `http://localhost:5173` (proxy `/api` → backend).

Testy frontendu (Vitest):

```bash
cd brewbase/frontend
npm test
```
