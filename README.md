# BrewBase

Projekt inżynierski: Platforma dla miłośników kawy.

## Funkcje

- **Wikipedia** - 
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
├── backend/     — API (modularny monolit)
├── frontend/    — aplikacja React (SPA)
└── database/    — schemat SQL, migracje, dane seed
```

## Uruchomienie lokalne
### Wymagania

- .NET 8 SDK
- Node.js (npm)
- PostgreSQL 15+

### Baza danych

1. Utwórz bazę PostgreSQL.
2. Wykonaj `brewbase/database/schema.sql`.
3. +`brewbase/database/seed_init.sql` (przykładowe dane).

Migracje z katalogu `brewbase/database/migrations/` stosuj ręcznie, jeśli baza powstała wcześniej.

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