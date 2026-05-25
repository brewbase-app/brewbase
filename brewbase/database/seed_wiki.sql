-- Seed danych wiki dla BrewBase
-- Uruchom po: schema.sql + seed_init.sql
--
-- Konta testowe (hasło: Test123!):
--   kawosz  (USER)  - autor artykułów
--   admin   (ADMIN) - moderator
--
-- Ponowne uruchomienie czyści i nadpisuje artykuły wiki.

DELETE FROM report;
DELETE FROM article;

INSERT INTO article (
    id,
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
    user_id
) VALUES
(
    1,
    'Etiopia Yirgacheffe Kochere',
    $$Kraj pochodzenia ziaren: Etiopia
Odmiana: Heirloom
Obróbka ziaren: Washed
Profil smakowy: Jaśmin, Cytrusy, Bergamotka

Jasno palona kawa o wyraźnym profilu kwiatowym i cytrusowym. W filiżance często pojawiają się nuty jaśminu, bergamotki i lemoniady. Doskonała do metod przelewowych, szczególnie V60.$$,
    'Approved',
    'coffee',
    '2026-04-10 09:15:00',
    '2026-04-12 11:00:00',
    '2026-04-12 11:00:00',
    2,
    '2026-04-12 11:00:00',
    NULL,
    1
),
(
    2,
    'Panama Geisha Esmeralda',
    $$Kraj pochodzenia ziaren: Panama
Odmiana: Geisha
Obróbka ziaren: Washed
Profil smakowy: Tropikalne owoce, Miód, Kwiaty

Jedna z najbardziej rozpoznawalnych kaw specialty na świecie. Charakteryzuje się elegancką słodyczą, aromatem tropikalnych owoców i herbacianą strukturą. Wymaga delikatnego parzenia, aby nie zdominować subtelnych nut.$$,
    'Approved',
    'coffee',
    '2026-04-11 14:20:00',
    '2026-04-13 09:30:00',
    '2026-04-13 09:30:00',
    2,
    '2026-04-13 09:30:00',
    NULL,
    1
),
(
    3,
    'Kolumbia Huila Pink Bourbon',
    $$Kraj pochodzenia ziaren: Kolumbia
Odmiana: Bourbon
Obróbka ziaren: Honey
Profil smakowy: Czerwone owoce, Karmel, Czekolada

Kawa o zbalansowanym profilu: czerwone owoce, karmel i delikatna kwasowość. Obróbka honey podkreśla słodyczę i daje pełniejsze body niż klasyczna washed.$$,
    'Approved',
    'coffee',
    '2026-04-14 08:00:00',
    '2026-04-15 16:45:00',
    '2026-04-15 16:45:00',
    2,
    '2026-04-15 16:45:00',
    NULL,
    1
),
(
    4,
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
    1
),
(
    5,
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
    1
),
(
    6,
    'Etiopia',
    $$Region: Yirgacheffe
Profil smakowy: Jaśmin, Cytrusy, Herbaciane

Etiopia to kolebka kawy arabica. W kraju działa wiele mikroregionów, w których tradycyjne obróbki natural i washed tworzą różnorodne profile smakowe — od kwiatowych i herbacianych po jagodowe i winne.

Najważniejsze regiony specialty: Yirgacheffe, Sidamo, Guji, Harrar.$$,
    'Approved',
    'country',
    '2026-04-08 10:00:00',
    '2026-04-09 14:00:00',
    '2026-04-09 14:00:00',
    2,
    '2026-04-09 14:00:00',
    NULL,
    1
),
(
    7,
    'Kolumbia',
    $$Region: Huila
Profil smakowy: Czekolada, Karmel, Czerwone owoce

Kolumbia jest jednym z największych eksporterów kawy specialty. Dzięki różnorodności wysokości i mikroklimatów można tu znaleźć zarówno klasyki typu washed z nutami czekolady i orzechów, jak i nowoczesne procesy honey czy anaerobic.

Popularne regiony: Huila, Nariño, Cauca, Antioquia.$$,
    'Approved',
    'country',
    '2026-04-09 11:30:00',
    '2026-04-10 08:15:00',
    '2026-04-10 08:15:00',
    2,
    '2026-04-10 08:15:00',
    NULL,
    1
),
(
    8,
    'Kenia',
    $$Region: Nyeri
Profil smakowy: Porzeczka, Cytrusy, Winne

Kenia słynie z kaw o intensywnej kwasowości i wyrazistych nutach czarnej porzeczki. Wysoko położone farmy i staranna obróbka washed tworzą bardzo czysty profil filiżanki.

Ważne regiony: Nyeri, Kirinyaga, Kiambu.$$,
    'Approved',
    'country',
    '2026-04-16 09:00:00',
    '2026-04-17 11:30:00',
    '2026-04-17 11:30:00',
    2,
    '2026-04-17 11:30:00',
    NULL,
    1
),
(
    9,
    'V60',
    $$Metoda przelewowa Hario V60 pozwala uzyskać czysty, transparentny profil smaku. Kluczowe są: świeżo zmielona kawa, kontrola tempa zalewania oraz odpowiednia temperatura wody (ok. 92–96°C).

Typowy przepis: 15 g kawy na 250 ml wody, całkowity czas ekstrakcji 2:30–3:00.$$,
    'Approved',
    'brewing_method',
    '2026-04-07 09:00:00',
    '2026-04-08 10:30:00',
    '2026-04-08 10:30:00',
    2,
    '2026-04-08 10:30:00',
    NULL,
    1
),
(
    10,
    'AeroPress',
    $$AeroPress to wszechstronna metoda łącząca immersion i ciśnienie. Można parzyć w stylu klasycznym (normal) lub odwróconym (inverted). Daje pełniejsze body niż V60, zachowując przy tym dobrą klarowność.

Sprawdza się zarówno w domu, jak i w podróży.$$,
    'Approved',
    'brewing_method',
    '2026-04-12 16:00:00',
    '2026-04-13 12:00:00',
    '2026-04-13 12:00:00',
    2,
    '2026-04-13 12:00:00',
    NULL,
    1
),
(
    11,
    'Chemex',
    $$Metoda filtracyjna o charakterystycznym, eleganckim kształcie. Grubsze filtry Chemex dają wyjątkowo czysty napar z delikatnym body.

Artykuł oczekuje na moderację.$$,
    'Pending',
    'brewing_method',
    '2026-05-21 08:30:00',
    '2026-05-21 08:30:00',
    NULL,
    NULL,
    NULL,
    NULL,
    1
),
(
    12,
    'Coffee Collective',
    $$Styl palenia: Light Roast, Nordic Roast

Duńska palarnia specialty znana z transparentności sourcingu i jasnego profilu palenia. Coffee Collective współpracuje bezpośrednio z producentami i promuje zrównoważony rozwój łańcucha dostaw.

Siedziba: Kopenhaga, Dania. Założona w 2007 roku.$$,
    'Approved',
    'roastery',
    '2026-04-06 13:00:00',
    '2026-04-07 09:00:00',
    '2026-04-07 09:00:00',
    2,
    '2026-04-07 09:00:00',
    NULL,
    1
),
(
    13,
    'Audun Coffee',
    $$Styl palenia: Omni Roast, Light Roast

Norweska palarnia specjalizująca się w kawach konkursowych i lotowych. Profil często podkreśla słodycz, kwasowość owocową i czystość filiżanki.

Siedziba: Oslo, Norwegia.$$,
    'Approved',
    'roastery',
    '2026-04-13 10:00:00',
    '2026-04-14 15:20:00',
    '2026-04-14 15:20:00',
    2,
    '2026-04-14 15:20:00',
    NULL,
    1
);

SELECT setval(
    pg_get_serial_sequence('article', 'id'),
    (SELECT MAX(id) FROM article)
);
