# BrewBase

Projekt inżynierski: Platforma społecznościowa do odkrywania kaw, tworzenia receptur i budowania kawowej społeczności.

## Funkcje

- **Wiki kawowe** - artykuły społecznościowe o kawach, metodach i palarniach
- **Przepisy** - tworzenie i udostępnianie przepisów parzenia
- **Cupping** - sesje degustacyjne z ocenami
- **Notatki** - szybkie zapiski
- **Rankingi** - kaw, użytkowników i przepisów
- **Społeczność** - profile, obserwowanie innych użytkowników
- **?**

## Stack technologiczny

| Warstwa   | Technologie                          |
|-----------|--------------------------------------|
| Frontend  | React, Vite, React Router            |
| Backend   | ASP.NET Core 8, Entity Framework Core |
| Baza danych | PostgreSQL (database-first)        |
| Autoryzacja | JWT                                |

## Struktura projektu

```
brewbase/
├── backend/     — backend API ASP.NET Core
├── frontend/    — aplikacja React (SPA)
└── database/    — schemat SQL, migracje, dane seed
```

## Środowiska

### Primary environment (docelowe)

Głównym środowiskiem projektu jest **uczelniana VM** ze wspólną bazą PostgreSQL i wspólnym deploymentem aplikacji (backend + frontend). To środowisko służy do wspólnego developmentu i prezentacji projektu.

### Local environment (development / fallback)

**Docker PostgreSQL** (lub lokalna instalacja Postgresa) służy wyłącznie do developmentu i jako fallback, gdy VM jest niedostępna. Nie jest to środowisko produkcyjne projektu — ten sam flow bootstrapu bazy, inny host w connection stringu.

---

## Bootstrap bazy danych

Źródłem prawdy schematu jest `brewbase/database/schema.sql`. Katalog `brewbase/database/migrations/` to **historyczne patche** — stosuj je ręcznie tylko wtedy, gdy baza powstała wcześniej i nie była odtwarzana z aktualnego `schema.sql`.

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

**`seed_wiki.sql`** jest częścią pełnego zestawu demo — bez niego wiki i część katalogu (powiązania artykułów z kawami/metodami) będą wyglądały na puste. Można uruchamiać wielokrotnie: dodaje tylko brakujące artykuły (nie usuwa danych użytkowników).

**`refresh_all_rankings()`** przelicza rankingi do tabel `*_ranking`. Backend czyta stamtąd listy rankingowe; po samym seedzie bez tego kroku strona rankingów może być pusta. Alternatywa: `POST /api/Ranking/refresh` (wymaga uprawnień).

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

### Baza danych

1. Utwórz bazę PostgreSQL (np. kontener Docker — wyłącznie do dev).
2. Wykonaj bootstrap z sekcji [Bootstrap bazy danych](#bootstrap-bazy-danych) (wszystkie cztery kroki).

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

Aplikacja: `http://localhost:5173` (proxy `/api` -> backend).

### Testy

```bash
dotnet test brewbase/backend/brewbase.sln
```