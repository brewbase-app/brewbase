#!/usr/bin/env bash
# Przygotowanie kodu źródłowego i skompilowanej wersji na płytę CD.
# Uruchom z katalogu brewbase/ (tam gdzie są backend/, frontend/, database/).

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="${1:-$ROOT/../cd-prep}"

echo "Źródła repozytorium: $ROOT"
echo "Folder wyjściowy:    $OUT"
echo ""

rm -rf "$OUT"
mkdir -p "$OUT/zrodla" "$OUT/aplikacja_skompilowana/backend" "$OUT/aplikacja_skompilowana/frontend"

# --- Kod źródłowy (bez artefaktów builda) ---
echo "Kopiowanie kodu źródłowego..."
rsync -a \
  --exclude 'node_modules' \
  --exclude 'bin' \
  --exclude 'obj' \
  --exclude '.git' \
  --exclude '.vs' \
  --exclude '.idea' \
  --exclude 'dist' \
  --exclude '.DS_Store' \
  --exclude '*.user' \
  --exclude '.env' \
  "$ROOT/" "$OUT/zrodla/brewbase/"

# --- Backend skompilowany ---
echo "Publikowanie backendu (dotnet publish)..."
dotnet publish "$ROOT/backend/brewbase.server/brewbase.server.csproj" \
  -c Release \
  -o "$OUT/aplikacja_skompilowana/backend"

# --- Frontend skompilowany ---
echo "Budowanie frontendu (npm run build)..."
(cd "$ROOT/frontend" && npm ci && npm run build)
cp -R "$ROOT/frontend/dist/." "$OUT/aplikacja_skompilowana/frontend/"

# --- README ---
cat > "$OUT/zrodla/README.txt" <<'EOF'
BrewBase — kody źródłowe

Wymagania:
- .NET 8 SDK
- Node.js 18+ i npm
- PostgreSQL 15+

Uruchomienie z kodu:
1. Zastosuj database/schema.sql oraz migracje z database/migrations/
2. Backend: cd brewbase/backend/brewbase.server && dotnet run
3. Frontend: cd brewbase/frontend && npm ci && npm run dev

Konto testowe (po seedzie): kawosz / Test123!
EOF

cat > "$OUT/aplikacja_skompilowana/README.txt" <<'EOF'
BrewBase — wersja skompilowana

Backend:
- Wymaga .NET 8 Runtime i działającej bazy PostgreSQL
- Uruchomienie: cd backend && dotnet brewbase.server.dll
- Skonfiguruj connection string (appsettings lub zmienne środowiskowe)

Frontend:
- Pliki statyczne w folderze frontend/ (wynik vite build)
- Wymaga serwera HTTP (np. nginx) lub npm run preview w środowisku dev

Bez skonfigurowanej bazy aplikacja nie uruchomi się w pełni — to normalne.
EOF

echo ""
echo "Gotowe."
echo "Skopiuj jeszcze pracę pisemną (PDF, max 45 MB) do: $OUT/praca_pisemna/"
echo ""
du -sh "$OUT" "$OUT/zrodla" "$OUT/aplikacja_skompilowana"
