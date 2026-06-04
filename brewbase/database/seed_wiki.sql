-- Seed danych wiki dla BrewBase (idempotentny)
-- Uruchom po: schema.sql + seed_init.sql
-- Bezpieczny do wielokrotnego uruchomienia: nie usuwa ani nie nadpisuje istniejących rekordów.
--
-- Konta testowe (hasło: Test123!):
--   kawosz  (USER,  id=1) — autor artykułów
--   admin   (ADMIN, id=2) — moderator
--
-- Klucz biznesowy:
--   - module + LOWER(TRIM(title)) — kraje, palarnie, metody, kawy bez coffee_id
--   - module + coffee_id + status='Approved' — zatwierdzone artykuły powiązane z katalogiem kaw

BEGIN;

-- 1. Etiopia Guji Natural (kawa w katalogu, coffee_id=1)
INSERT INTO article (
    title,
    content,
    status,
    module,
    created_at,
    updated_at,
    published_at,
    moderated_by_user_id,
    moderated_at,
    moderation_comment,
    user_id,
    coffee_id
)
SELECT
    'Etiopia Guji Natural',
    $$Kraj pochodzenia ziaren: Etiopia
Odmiana: Heirloom
Obróbka ziaren: Natural
Profil smakowy: Jagody, Kwiaty, Czekolada

Naturalnie obrabiana kawa z regionu Guji. W filiżance dominują nuty dojrzałych jagód i delikatnej kwiatowości, z aksamitnym body typowym dla etiopskich naturali.$$,
    'Approved',
    'coffee',
    '2026-04-10 09:15:00',
    '2026-04-12 11:00:00',
    '2026-04-12 11:00:00',
    2,
    '2026-04-12 11:00:00',
    NULL,
    1,
    1
WHERE NOT EXISTS (
    SELECT 1
    FROM article existing
    WHERE existing.module = 'coffee'
      AND existing.status = 'Approved'
      AND existing.coffee_id = 1
);

-- 2. Kolumbia Huila Washed (coffee_id=2)
INSERT INTO article (
    title,
    content,
    status,
    module,
    created_at,
    updated_at,
    published_at,
    moderated_by_user_id,
    moderated_at,
    moderation_comment,
    user_id,
    coffee_id
)
SELECT
    'Kolumbia Huila Washed',
    $$Kraj pochodzenia ziaren: Kolumbia
Odmiana: Caturra
Obróbka ziaren: Washed
Profil smakowy: Czekolada, Karmel, Orzechy

Klasyczna washed z Huili o zbalansowanym profilu: czekolada, karmel i łagodna orzechowa słodycz. Dobrze sprawdza się jako przelew i espresso.$$,
    'Approved',
    'coffee',
    '2026-04-14 08:00:00',
    '2026-04-15 16:45:00',
    '2026-04-15 16:45:00',
    2,
    '2026-04-15 16:45:00',
    NULL,
    1,
    2
WHERE NOT EXISTS (
    SELECT 1
    FROM article existing
    WHERE existing.module = 'coffee'
      AND existing.status = 'Approved'
      AND existing.coffee_id = 2
);

-- 3. Kenia Nyeri AA (Pending, bez coffee_id)
INSERT INTO article (
    title,
    content,
    status,
    module,
    created_at,
    updated_at,
    published_at,
    moderated_by_user_id,
    moderated_at,
    moderation_comment,
    user_id,
    coffee_id
)
SELECT
    'Kenia Nyeri AA',
    $$Kraj pochodzenia ziaren: Kenia
Odmiana: SL28
Obróbka ziaren: Washed
Profil smakowy: Porzeczka, Cytrusy

Klasyczna kenijska kawa o wyrazistej kwasowości i soczystym profilu owocowym. Często spotykana w profilach competition i cuppingów specialty.$$,
    'Pending',
    'coffee',
    '2026-05-20 10:00:00',
    '2026-05-20 10:00:00',
    NULL,
    NULL,
    NULL,
    NULL,
    1,
    NULL
WHERE NOT EXISTS (
    SELECT 1
    FROM article existing
    WHERE existing.module = 'coffee'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Kenia Nyeri AA'))
);

-- 4. Brazylia Santos (Rejected)
INSERT INTO article (
    title,
    content,
    status,
    module,
    created_at,
    updated_at,
    published_at,
    moderated_by_user_id,
    moderated_at,
    moderation_comment,
    user_id,
    coffee_id
)
SELECT
    'Brazylia Santos',
    $$Kraj pochodzenia ziaren: Brazylia
Odmiana: Catuai
Obróbka ziaren: Natural

Popularna brazylijska kawa o łagodnym, orzechowym profilu. Dobrze sprawdza się jako espresso i baza do mlecznych napojów.$$,
    'Rejected',
    'coffee',
    '2026-05-05 12:00:00',
    '2026-05-06 09:00:00',
    NULL,
    2,
    '2026-05-06 09:00:00',
    'Artykuł wymaga uzupełnienia o profil smakowy oraz bardziej szczegółowy opis regionu uprawy.',
    1,
    NULL
WHERE NOT EXISTS (
    SELECT 1
    FROM article existing
    WHERE existing.module = 'coffee'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Brazylia Santos'))
);

-- 5–7. Kraje
INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'Etiopia',
    $$Region: Yirgacheffe
Profil smakowy: Jaśmin, Cytrusy, Herbaciane

Etiopia to kolebka kawy arabica. W kraju działa wiele mikroregionów, w których tradycyjne obróbki natural i washed tworzą różnorodne profile smakowe — od kwiatowych i herbacianych po jagodowe i winne.

Najważniejsze regiony specialty: Yirgacheffe, Sidamo, Guji, Harrar.$$,
    'Approved', 'country',
    '2026-04-08 10:00:00', '2026-04-09 14:00:00', '2026-04-09 14:00:00',
    2, '2026-04-09 14:00:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'country'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Etiopia'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'Kolumbia',
    $$Region: Huila
Profil smakowy: Czekolada, Karmel, Czerwone owoce

Kolumbia jest jednym z największych eksporterów kawy specialty. Dzięki różnorodności wysokości i mikroklimatów można tu znaleźć zarówno klasyki typu washed z nutami czekolady i orzechów, jak i nowoczesne procesy honey czy anaerobic.

Popularne regiony: Huila, Nariño, Cauca, Antioquia.$$,
    'Approved', 'country',
    '2026-04-09 11:30:00', '2026-04-10 08:15:00', '2026-04-10 08:15:00',
    2, '2026-04-10 08:15:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'country'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Kolumbia'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'Kenia',
    $$Region: Nyeri
Profil smakowy: Porzeczka, Cytrusy, Winne

Kenia słynie z kaw o intensywnej kwasowości i wyrazistych nutach czarnej porzeczki. Wysoko położone farmy i staranna obróbka washed tworzą bardzo czysty profil filiżanki.

Ważne regiony: Nyeri, Kirinyaga, Kiambu.$$,
    'Approved', 'country',
    '2026-04-16 09:00:00', '2026-04-17 11:30:00', '2026-04-17 11:30:00',
    2, '2026-04-17 11:30:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'country'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Kenia'))
);

-- 8–10. Metody parzenia
INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'V60',
    $$Metoda przelewowa Hario V60 pozwala uzyskać czysty, transparentny profil smaku. Kluczowe są: świeżo zmielona kawa, kontrola tempa zalewania oraz odpowiednia temperatura wody (ok. 92–96°C).

Typowy przepis: 15 g kawy na 250 ml wody, całkowity czas ekstrakcji 2:30–3:00.$$,
    'Approved', 'brewing_method',
    '2026-04-07 09:00:00', '2026-04-08 10:30:00', '2026-04-08 10:30:00',
    2, '2026-04-08 10:30:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'brewing_method'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('V60'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'AeroPress',
    $$AeroPress to wszechstronna metoda łącząca immersion i ciśnienie. Można parzyć w stylu klasycznym (normal) lub odwróconym (inverted). Daje pełniejsze body niż V60, zachowując przy tym dobrą klarowność.

Sprawdza się zarówno w domu, jak i w podróży.$$,
    'Approved', 'brewing_method',
    '2026-04-12 16:00:00', '2026-04-13 12:00:00', '2026-04-13 12:00:00',
    2, '2026-04-13 12:00:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'brewing_method'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('AeroPress'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'Chemex',
    $$Metoda filtracyjna o charakterystycznym, eleganckim kształcie. Grubsze filtry Chemex dają wyjątkowo czysty napar z delikatnym body.

Artykuł oczekuje na moderację.$$,
    'Pending', 'brewing_method',
    '2026-05-21 08:30:00', '2026-05-21 08:30:00', NULL,
    NULL, NULL, NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'brewing_method'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Chemex'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'French Press',
    $$French Press to metoda immersion dająca pełne body i intensywny profil smaku. Prosta w obsłudze, dobrze sprawdza się w domowym parzeniu kawy.$$,
    'Approved', 'brewing_method',
    '2026-04-18 10:00:00', '2026-04-19 11:00:00', '2026-04-19 11:00:00',
    2, '2026-04-19 11:00:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'brewing_method'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('French Press'))
);

-- 11–15. Palarnie (tytuł = nazwa z katalogu roastery w seed_init)
INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'CoffeeLab',
    $$Styl palenia: Light Roast, Omni Roast

Polska palarnia specialty z Krakowa, znana z selekcji ziaren pod przelew i espresso. W ofercie dominują jasne profile palenia z wyraźną kwasowością i czystością filiżanki.$$,
    'Approved', 'roastery',
    '2026-04-05 10:00:00', '2026-04-06 11:00:00', '2026-04-06 11:00:00',
    2, '2026-04-06 11:00:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'roastery'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('CoffeeLab'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'Hard Beans',
    $$Styl palenia: Light Roast, Nordic Roast

Palarnia z Poznania koncentrująca się na kawach pod filtr i metody przelewowe. Profil palenia podkreśla przejrzystość, słodycz i złożoność aromatyczną ziaren specialty.$$,
    'Approved', 'roastery',
    '2026-04-07 12:00:00', '2026-04-08 09:30:00', '2026-04-08 09:30:00',
    2, '2026-04-08 09:30:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'roastery'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Hard Beans'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'Audun Coffee',
    $$Styl palenia: Omni Roast, Light Roast

Norweska palarnia specjalizująca się w kawach konkursowych i lotowych. Profil często podkreśla słodycz, kwasowość owocową i czystość filiżanki.$$,
    'Approved', 'roastery',
    '2026-04-13 10:00:00', '2026-04-14 15:20:00', '2026-04-14 15:20:00',
    2, '2026-04-14 15:20:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'roastery'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Audun Coffee'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'Java Coffee',
    $$Styl palenia: Light Roast, Espresso Roast

Palarnia z Krakowa łącząca ofertę pod przelew i espresso. Znana z rotacji sezonowych mikrolotów i profili balansujących słodycz z kwasowością.$$,
    'Approved', 'roastery',
    '2026-04-15 09:00:00', '2026-04-16 10:00:00', '2026-04-16 10:00:00',
    2, '2026-04-16 10:00:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'roastery'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Java Coffee'))
);

INSERT INTO article (
    title, content, status, module,
    created_at, updated_at, published_at,
    moderated_by_user_id, moderated_at, moderation_comment,
    user_id, coffee_id
)
SELECT
    'Coffee Collective',
    $$Styl palenia: Light Roast, Nordic Roast

Duńska palarnia specialty znana z transparentności sourcingu i jasnego profilu palenia. Coffee Collective współpracuje bezpośrednio z producentami i promuje zrównoważony rozwój łańcucha dostaw.$$,
    'Approved', 'roastery',
    '2026-04-17 13:00:00', '2026-04-18 09:00:00', '2026-04-18 09:00:00',
    2, '2026-04-18 09:00:00', NULL, 1, NULL
WHERE NOT EXISTS (
    SELECT 1 FROM article existing
    WHERE existing.module = 'roastery'
      AND LOWER(TRIM(existing.title)) = LOWER(TRIM('Coffee Collective'))
);

-- Ustaw sekwencję ID powyżej maksymalnego istniejącego (bezpieczne przy wielokrotnym uruchomieniu)
SELECT setval(
    pg_get_serial_sequence('article', 'id'),
    GREATEST(COALESCE((SELECT MAX(id) FROM article), 0), 1),
    true
);

COMMIT;
