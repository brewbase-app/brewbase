-- BrewBase demo seed for VM
-- Validated against current frontend/backend validation rules.
-- WARNING: this resets demo data in BrewBase tables.

ROLLBACK;
BEGIN;

TRUNCATE TABLE
    report,
    user_coffee_favorite,
    user_recipe_favorite,
    coffee_flavor_profile,
    user_preference_flavor_profile,
    recommendations,
    notification,
    follow,
    cupping_session_coffee,
    cupping_session,
    quick_note,
    recipe_rating,
    coffee_rating,
    recipe_ranking,
    coffee_ranking,
    user_ranking,
    recipe,
    article,
    coffee,
    user_preference,
    app_user,
    roastery,
    region,
    country,
    variety,
    processing_method,
    brewing_method,
    acidity,
    body,
    flavor_profile
    RESTART IDENTITY CASCADE;

INSERT INTO country (id, name) VALUES
                                   (1, 'Etiopia'),
                                   (2, 'Kolumbia'),
                                   (3, 'Kenia'),
                                   (4, 'Brazylia'),
                                   (5, 'Kostaryka'),
                                   (6, 'Gwatemala'),
                                   (7, 'Panama'),
                                   (8, 'Rwanda'),
                                   (9, 'Peru'),
                                   (10, 'Indonezja');

INSERT INTO region (id, name, country_id) VALUES
                                              (1, 'Guji', 1),
                                              (2, 'Sidamo', 1),
                                              (3, 'Huila', 2),
                                              (4, 'Nyeri', 3),
                                              (5, 'Cerrado Mineiro', 4),
                                              (6, 'Tarrazú', 5),
                                              (7, 'Antigua', 6),
                                              (8, 'Boquete', 7),
                                              (9, 'Nyamasheke', 8),
                                              (10, 'Cajamarca', 9),
                                              (11, 'Aceh Gayo', 10),
                                              (12, 'Yirgacheffe', 1),
                                              (13, 'Kirinyaga', 3),
                                              (14, 'Nariño', 2);

INSERT INTO brewing_method (id, name, description) VALUES
                                                       (1, 'V60', 'Metoda przelewowa Hario V60, dobra do jasnych kaw i czystego profilu.'),
                                                       (2, 'AeroPress', 'Szybka metoda immersion/pressure, bardzo dobra do eksperymentów.'),
                                                       (3, 'Chemex', 'Metoda przelewowa z grubym filtrem, daje bardzo klarowny napar.'),
                                                       (4, 'French Press', 'Pełne body i proste parzenie metodą immersion.'),
                                                       (5, 'Kalita Wave', 'Stabilna metoda przelewowa z płaskim dnem.'),
                                                       (6, 'Moka Pot', 'Intensywna kawa z kawiarki, dobra pod mleko.'),
                                                       (7, 'Espresso', 'Krótka ekstrakcja pod ciśnieniem, baza do napojów mlecznych.'),
                                                       (8, 'Cold Brew', 'Dłuższa ekstrakcja w niższej temperaturze, łagodna kwasowość i wysoka słodycz.');

INSERT INTO processing_method (id, name) VALUES
                                             (1, 'Washed'),
                                             (2, 'Natural'),
                                             (3, 'Honey'),
                                             (4, 'Anaerobic'),
                                             (5, 'Pulped Natural'),
                                             (6, 'Wet Hulled');

INSERT INTO variety (id, name) VALUES
                                   (1, 'Heirloom'),
                                   (2, 'Bourbon'),
                                   (3, 'Caturra'),
                                   (4, 'SL28'),
                                   (5, 'Geisha'),
                                   (6, 'Typica'),
                                   (7, 'Catuai'),
                                   (8, 'Pacamara'),
                                   (9, 'Castillo'),
                                   (10, 'Mundo Novo');

INSERT INTO roastery (id, name) VALUES
                                    (1, 'CoffeeLab'),
                                    (2, 'Hard Beans'),
                                    (3, 'Audun Coffee'),
                                    (4, 'Java Coffee'),
                                    (5, 'HAYB'),
                                    (6, 'Story Coffee'),
                                    (7, 'Kafar'),
                                    (8, 'La Cabra'),
                                    (9, 'Coffee Collective'),
                                    (10, 'Manhattan Coffee Roasters'),
                                    (11, 'Five Elephant'),
                                    (12, 'Doubleshot');

INSERT INTO acidity (id, name, description) VALUES
                                                (1, 'Niska', 'Łagodna, delikatna kwasowość.'),
                                                (2, 'Średnia', 'Zbalansowana kwasowość owocowa.'),
                                                (3, 'Wysoka', 'Wyraźna, soczysta kwasowość.'),
                                                (4, 'Bardzo wysoka', 'Intensywna kwasowość cytrusowa lub porzeczkowa.');

INSERT INTO body (id, name, description) VALUES
                                             (1, 'Lekkie', 'Delikatne, herbaciane body.'),
                                             (2, 'Średnie', 'Zbalansowane body.'),
                                             (3, 'Pełne', 'Gęste i wyraźne body.'),
                                             (4, 'Kremowe', 'Aksamitne, kremowe odczucie w ustach.');

INSERT INTO flavor_profile (id, name) VALUES
                                          (1, 'Jagody'),
                                          (2, 'Jaśmin'),
                                          (3, 'Cytrusy'),
                                          (4, 'Czekolada'),
                                          (5, 'Karmel'),
                                          (6, 'Orzechy'),
                                          (7, 'Porzeczka'),
                                          (8, 'Miód'),
                                          (9, 'Tropikalne owoce'),
                                          (10, 'Herbata'),
                                          (11, 'Kwiaty'),
                                          (12, 'Czerwone owoce'),
                                          (13, 'Kakao'),
                                          (14, 'Wanilia'),
                                          (15, 'Brzoskwinia'),
                                          (16, 'Śliwka'),
                                          (17, 'Pomarańcza'),
                                          (18, 'Migdały');

INSERT INTO app_user (id, email, password_hash, login, role, activity_points, label, is_blocked, created_at, password_hint) VALUES
                                                                                                                                (1, 'user@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'kawosz', 'User', 120, 'Home barista', false, '2026-04-01 09:00:00', 'Test123!'),
                                                                                                                                (2, 'maja@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'maja', 'User', 95, 'V60 lover', false, '2026-04-02 10:00:00', 'Test123!'),
                                                                                                                                (3, 'admin@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'admin', 'Admin', 30, 'Administrator', false, '2026-04-01 08:00:00', 'Test123!'),
                                                                                                                                (4, 'lidu@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'Lidu', 'User', 72, 'AeroPress tester', false, '2026-04-04 12:30:00', 'Test123!'),
                                                                                                                                (5, 'bartek@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'bartek', 'User', 64, 'Espresso fan', false, '2026-04-05 13:00:00', 'Test123!'),
                                                                                                                                (6, 'ania@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'ania', 'User', 83, 'Cupping notes', false, '2026-04-07 15:00:00', 'Test123!'),
                                                                                                                                (7, 'tomek@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'tomek', 'User', 38, 'Cold brew', false, '2026-04-10 16:00:00', 'Test123!'),
                                                                                                                                (8, 'ola@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'ola', 'User', 51, 'Sweet profiles', false, '2026-04-12 18:00:00', 'Test123!'),
                                                                                                                                (9, 'michal@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'michal', 'User', 44, 'Chemex user', false, '2026-04-13 19:00:00', 'Test123!'),
                                                                                                                                (10, 'kasia@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'kasia', 'User', 57, 'Natural coffees', false, '2026-04-14 20:00:00', 'Test123!'),
                                                                                                                                (11, 'piotr@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'piotr', 'User', 0, 'New user', false, '2026-05-01 09:00:00', 'Test123!'),
                                                                                                                                (12, 'blocked@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'zablokowany', 'User', 10, 'Blocked demo', true, '2026-05-02 09:00:00', 'Test123!');

INSERT INTO user_preference (id, preferred_roast_level, favorite_notes, quiz_completed, user_id) VALUES
                                                                                                     (1, 'Light', 'jaśmin, cytrusy, herbata', true, 1),
                                                                                                     (2, 'Light', 'jagody, kwiaty, tropikalne owoce', true, 2),
                                                                                                     (3, 'Medium', 'czekolada, orzechy', true, 3),
                                                                                                     (4, 'Medium', 'karmel, czerwone owoce', true, 4),
                                                                                                     (5, 'Dark', 'kakao, espresso', true, 5),
                                                                                                     (6, 'Light', 'porzeczka, cytrusy', true, 6),
                                                                                                     (7, 'Medium', 'miód, migdały', true, 7),
                                                                                                     (8, 'Light', 'brzoskwinia, jaśmin', true, 8),
                                                                                                     (9, 'Medium', 'czekolada, śliwka', true, 9),
                                                                                                     (10, 'Light', 'natural, jagody', true, 10),
                                                                                                     (11, 'Medium', 'kawa do mleka', true, 11),
                                                                                                     (12, 'Dark', 'klasyczne espresso', true, 12);

INSERT INTO user_preference_flavor_profile (user_preference_id, flavor_profile_id) VALUES
                                                                                       (1, 2),
                                                                                       (1, 3),
                                                                                       (1, 4),
                                                                                       (2, 4),
                                                                                       (2, 5),
                                                                                       (2, 7),
                                                                                       (3, 6),
                                                                                       (3, 7),
                                                                                       (3, 10),
                                                                                       (4, 8),
                                                                                       (4, 9),
                                                                                       (4, 13),
                                                                                       (5, 10),
                                                                                       (5, 11),
                                                                                       (5, 16),
                                                                                       (6, 12),
                                                                                       (6, 13),
                                                                                       (6, 1),
                                                                                       (7, 14),
                                                                                       (7, 15),
                                                                                       (7, 4),
                                                                                       (8, 16),
                                                                                       (8, 17),
                                                                                       (8, 7),
                                                                                       (9, 18),
                                                                                       (9, 1),
                                                                                       (9, 10),
                                                                                       (10, 2),
                                                                                       (10, 3),
                                                                                       (10, 13),
                                                                                       (11, 4),
                                                                                       (11, 5),
                                                                                       (11, 16),
                                                                                       (12, 6),
                                                                                       (12, 7),
                                                                                       (12, 1);

INSERT INTO coffee (id, name, roastery_id, region_id, variety_id, acidity_id, body_id, processing_method_id, created_by_user_id, is_verified) VALUES
                                                                                                                                                  (1, 'Etiopia Guji Natural', 1, 1, 1, 3, 2, 2, 1, true),
                                                                                                                                                  (2, 'Kolumbia Huila Washed', 2, 3, 3, 2, 2, 1, 1, true),
                                                                                                                                                  (3, 'Kenia Nyeri SL28', 4, 4, 4, 4, 1, 1, 2, true),
                                                                                                                                                  (4, 'Etiopia Sidamo Honey', 3, 2, 1, 3, 3, 3, 2, true),
                                                                                                                                                  (5, 'Kolumbia Bourbon Natural', 2, 3, 2, 2, 3, 2, 1, true),
                                                                                                                                                  (6, 'Brazylia Cerrado Espresso', 5, 5, 7, 1, 4, 5, 5, true),
                                                                                                                                                  (7, 'Kostaryka Tarrazú Honey', 6, 6, 3, 2, 2, 3, 6, true),
                                                                                                                                                  (8, 'Gwatemala Antigua Bourbon', 7, 7, 2, 2, 3, 1, 6, true),
                                                                                                                                                  (9, 'Panama Boquete Geisha', 8, 8, 5, 4, 1, 1, 8, true),
                                                                                                                                                  (10, 'Rwanda Nyamasheke Washed', 9, 9, 2, 3, 2, 1, 6, true),
                                                                                                                                                  (11, 'Peru Cajamarca Caturra', 10, 10, 3, 2, 2, 1, 7, true),
                                                                                                                                                  (12, 'Indonezja Aceh Gayo', 11, 11, 6, 1, 4, 6, 5, true),
                                                                                                                                                  (13, 'Etiopia Yirgacheffe Floral', 9, 12, 1, 4, 1, 1, 8, true),
                                                                                                                                                  (14, 'Kenia Kirinyaga AA', 12, 13, 4, 4, 1, 1, 9, true),
                                                                                                                                                  (15, 'Kolumbia Nariño Castillo', 10, 14, 9, 2, 2, 1, 10, true),
                                                                                                                                                  (16, 'Brazylia Natural Sweet Espresso', 1, 5, 10, 1, 4, 2, 5, true),
                                                                                                                                                  (17, 'Gwatemala Pacamara Washed', 11, 7, 8, 2, 3, 1, 9, true),
                                                                                                                                                  (18, 'Kostaryka Anaerobic Red', 6, 6, 7, 3, 3, 4, 10, true),
                                                                                                                                                  (19, 'Etiopia Guji Anaerobic', 8, 1, 1, 4, 2, 4, 4, true),
                                                                                                                                                  (20, 'Peru Organic Washed', 7, 10, 3, 2, 2, 1, 8, true),
                                                                                                                                                  (21, 'Kolumbia Decaf Sugarcane', 5, 3, 9, 1, 2, 1, 7, true),
                                                                                                                                                  (22, 'Rwanda Natural Fruity', 12, 9, 2, 3, 2, 2, 6, true),
                                                                                                                                                  (23, 'Panama Geisha Competition Lot', 10, 8, 5, 4, 1, 4, 2, true),
                                                                                                                                                  (24, 'Brazylia Pulped Natural', 3, 5, 7, 1, 4, 5, 1, true);

INSERT INTO coffee_flavor_profile (coffee_id, flavor_profile_id) VALUES
                                                                     (1, 1),
                                                                     (1, 2),
                                                                     (1, 11),
                                                                     (1, 12),
                                                                     (2, 4),
                                                                     (2, 5),
                                                                     (2, 6),
                                                                     (2, 12),
                                                                     (3, 3),
                                                                     (3, 7),
                                                                     (3, 17),
                                                                     (4, 8),
                                                                     (4, 9),
                                                                     (4, 11),
                                                                     (5, 5),
                                                                     (5, 12),
                                                                     (5, 16),
                                                                     (6, 4),
                                                                     (6, 6),
                                                                     (6, 13),
                                                                     (7, 5),
                                                                     (7, 8),
                                                                     (7, 15),
                                                                     (8, 4),
                                                                     (8, 6),
                                                                     (8, 18),
                                                                     (9, 2),
                                                                     (9, 3),
                                                                     (9, 11),
                                                                     (9, 15),
                                                                     (10, 3),
                                                                     (10, 7),
                                                                     (10, 12),
                                                                     (11, 5),
                                                                     (11, 13),
                                                                     (11, 16),
                                                                     (12, 4),
                                                                     (12, 6),
                                                                     (12, 14),
                                                                     (13, 2),
                                                                     (13, 10),
                                                                     (13, 11),
                                                                     (14, 3),
                                                                     (14, 7),
                                                                     (14, 17),
                                                                     (15, 4),
                                                                     (15, 5),
                                                                     (15, 12),
                                                                     (16, 4),
                                                                     (16, 5),
                                                                     (16, 13),
                                                                     (16, 18),
                                                                     (17, 6),
                                                                     (17, 12),
                                                                     (17, 15),
                                                                     (18, 9),
                                                                     (18, 12),
                                                                     (18, 16),
                                                                     (19, 1),
                                                                     (19, 9),
                                                                     (19, 11),
                                                                     (19, 15),
                                                                     (20, 5),
                                                                     (20, 10),
                                                                     (20, 14),
                                                                     (21, 4),
                                                                     (21, 5),
                                                                     (21, 14),
                                                                     (22, 1),
                                                                     (22, 7),
                                                                     (22, 12),
                                                                     (23, 2),
                                                                     (23, 3),
                                                                     (23, 11),
                                                                     (23, 15),
                                                                     (24, 4),
                                                                     (24, 6),
                                                                     (24, 8);

INSERT INTO recipe (id, title, parameters, steps, is_public, user_id, brewing_method_id, coffee_id, created_at, moderation_comment) VALUES
                                                                                                                                        (1, 'V60 jasna Etiopia', '{"coffee":"15g","water":"250ml","temperature":"94°C","grindSize":"średnie","brewTime":"2:45"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 1, 1, 1, CURRENT_TIMESTAMP - INTERVAL '44 days', NULL),
                                                                                                                                        (2, 'AeroPress słodka Kolumbia', '{"coffee":"18g","water":"230ml","temperature":"92°C","grindSize":"średnio drobne","brewTime":"1:50"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 2, 2, 2, CURRENT_TIMESTAMP - INTERVAL '43 days', NULL),
                                                                                                                                        (3, 'Chemex Kenia porzeczka', '{"coffee":"24g","water":"380ml","temperature":"95°C","grindSize":"grubsze","brewTime":"4:15"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 3, 3, 3, CURRENT_TIMESTAMP - INTERVAL '42 days', NULL),
                                                                                                                                        (4, 'French Press Brazylia', '{"coffee":"28g","water":"420ml","temperature":"93°C","grindSize":"grube","brewTime":"4:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 4, 4, CURRENT_TIMESTAMP - INTERVAL '41 days', NULL),
                                                                                                                                        (5, 'Kalita balans Huila', '{"coffee":"20g","water":"320ml","temperature":"94°C","grindSize":"średnie","brewTime":"3:10"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 6, 5, 2, CURRENT_TIMESTAMP - INTERVAL '40 days', NULL),
                                                                                                                                        (6, 'Espresso Brazylia klasyk', '{"coffee":"18g","water":"40ml","temperature":"93°C","grindSize":"drobne","brewTime":"1:30"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 7, 6, CURRENT_TIMESTAMP - INTERVAL '39 days', NULL),
                                                                                                                                        (7, 'Cold Brew łagodne Rwanda', '{"coffee":"70g","water":"1000ml","temperature":"80°C","grindSize":"grube","brewTime":"9:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 7, 8, 10, CURRENT_TIMESTAMP - INTERVAL '38 days', NULL),
                                                                                                                                        (8, 'Moka Pot Indonezja', '{"coffee":"20g","water":"180ml","temperature":"95°C","grindSize":"drobne","brewTime":"3:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 6, 12, CURRENT_TIMESTAMP - INTERVAL '37 days', NULL),
                                                                                                                                        (9, 'V60 Panama Geisha', '{"coffee":"16g","water":"260ml","temperature":"94°C","grindSize":"średnie","brewTime":"3:05"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 8, 1, 9, CURRENT_TIMESTAMP - INTERVAL '36 days', NULL),
                                                                                                                                        (10, 'AeroPress Etiopia Anaerobic', '{"coffee":"17g","water":"220ml","temperature":"90°C","grindSize":"średnie","brewTime":"2:10"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 4, 2, 19, CURRENT_TIMESTAMP - INTERVAL '35 days', NULL),
                                                                                                                                        (11, 'V60 jasna Etiopia #11', '{"coffee":"15g","water":"250ml","temperature":"94°C","grindSize":"średnie","brewTime":"2:45"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 1, 1, 1, CURRENT_TIMESTAMP - INTERVAL '34 days', NULL),
                                                                                                                                        (12, 'AeroPress słodka Kolumbia #12', '{"coffee":"18g","water":"230ml","temperature":"92°C","grindSize":"średnio drobne","brewTime":"1:50"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, false, 2, 2, 2, CURRENT_TIMESTAMP - INTERVAL '33 days', NULL),
                                                                                                                                        (13, 'Chemex Kenia porzeczka #13', '{"coffee":"24g","water":"380ml","temperature":"95°C","grindSize":"grubsze","brewTime":"4:15"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 3, 3, 3, CURRENT_TIMESTAMP - INTERVAL '32 days', NULL),
                                                                                                                                        (14, 'French Press Brazylia #14', '{"coffee":"28g","water":"420ml","temperature":"93°C","grindSize":"grube","brewTime":"4:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 4, 4, CURRENT_TIMESTAMP - INTERVAL '31 days', NULL),
                                                                                                                                        (15, 'Kalita balans Huila #15', '{"coffee":"20g","water":"320ml","temperature":"94°C","grindSize":"średnie","brewTime":"3:10"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 6, 5, 2, CURRENT_TIMESTAMP - INTERVAL '30 days', NULL),
                                                                                                                                        (16, 'Espresso Brazylia klasyk #16', '{"coffee":"18g","water":"40ml","temperature":"93°C","grindSize":"drobne","brewTime":"1:30"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 7, 6, CURRENT_TIMESTAMP - INTERVAL '29 days', NULL),
                                                                                                                                        (17, 'Cold Brew łagodne Rwanda #17', '{"coffee":"70g","water":"1000ml","temperature":"80°C","grindSize":"grube","brewTime":"9:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 7, 8, 10, CURRENT_TIMESTAMP - INTERVAL '28 days', NULL),
                                                                                                                                        (18, 'Moka Pot Indonezja #18', '{"coffee":"20g","water":"180ml","temperature":"95°C","grindSize":"drobne","brewTime":"3:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 6, 12, CURRENT_TIMESTAMP - INTERVAL '27 days', NULL),
                                                                                                                                        (19, 'V60 Panama Geisha #19', '{"coffee":"16g","water":"260ml","temperature":"94°C","grindSize":"średnie","brewTime":"3:05"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 8, 1, 9, CURRENT_TIMESTAMP - INTERVAL '26 days', NULL),
                                                                                                                                        (20, 'AeroPress Etiopia Anaerobic #20', '{"coffee":"17g","water":"220ml","temperature":"90°C","grindSize":"średnie","brewTime":"2:10"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 4, 2, 19, CURRENT_TIMESTAMP - INTERVAL '25 days', NULL),
                                                                                                                                        (21, 'V60 jasna Etiopia #21', '{"coffee":"15g","water":"250ml","temperature":"94°C","grindSize":"średnie","brewTime":"2:45"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 1, 1, 1, CURRENT_TIMESTAMP - INTERVAL '24 days', NULL),
                                                                                                                                        (22, 'AeroPress słodka Kolumbia #22', '{"coffee":"18g","water":"230ml","temperature":"92°C","grindSize":"średnio drobne","brewTime":"1:50"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 2, 2, 2, CURRENT_TIMESTAMP - INTERVAL '23 days', NULL),
                                                                                                                                        (23, 'Chemex Kenia porzeczka #23', '{"coffee":"24g","water":"380ml","temperature":"95°C","grindSize":"grubsze","brewTime":"4:15"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 3, 3, 3, CURRENT_TIMESTAMP - INTERVAL '22 days', NULL),
                                                                                                                                        (24, 'French Press Brazylia #24', '{"coffee":"28g","water":"420ml","temperature":"93°C","grindSize":"grube","brewTime":"4:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, false, 5, 4, 4, CURRENT_TIMESTAMP - INTERVAL '21 days', NULL),
                                                                                                                                        (25, 'Kalita balans Huila #25', '{"coffee":"20g","water":"320ml","temperature":"94°C","grindSize":"średnie","brewTime":"3:10"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 6, 5, 2, CURRENT_TIMESTAMP - INTERVAL '20 days', NULL),
                                                                                                                                        (26, 'Espresso Brazylia klasyk #26', '{"coffee":"18g","water":"40ml","temperature":"93°C","grindSize":"drobne","brewTime":"1:30"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 7, 6, CURRENT_TIMESTAMP - INTERVAL '19 days', NULL),
                                                                                                                                        (27, 'Cold Brew łagodne Rwanda #27', '{"coffee":"70g","water":"1000ml","temperature":"80°C","grindSize":"grube","brewTime":"9:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 7, 8, 10, CURRENT_TIMESTAMP - INTERVAL '18 days', NULL),
                                                                                                                                        (28, 'Moka Pot Indonezja #28', '{"coffee":"20g","water":"180ml","temperature":"95°C","grindSize":"drobne","brewTime":"3:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 6, 12, CURRENT_TIMESTAMP - INTERVAL '17 days', NULL),
                                                                                                                                        (29, 'V60 Panama Geisha #29', '{"coffee":"16g","water":"260ml","temperature":"94°C","grindSize":"średnie","brewTime":"3:05"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 8, 1, 9, CURRENT_TIMESTAMP - INTERVAL '16 days', NULL),
                                                                                                                                        (30, 'AeroPress Etiopia Anaerobic #30', '{"coffee":"17g","water":"220ml","temperature":"90°C","grindSize":"średnie","brewTime":"2:10"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 4, 2, 19, CURRENT_TIMESTAMP - INTERVAL '15 days', NULL),
                                                                                                                                        (31, 'V60 jasna Etiopia #31', '{"coffee":"15g","water":"250ml","temperature":"94°C","grindSize":"średnie","brewTime":"2:45"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 1, 1, 1, CURRENT_TIMESTAMP - INTERVAL '14 days', NULL),
                                                                                                                                        (32, 'AeroPress słodka Kolumbia #32', '{"coffee":"18g","water":"230ml","temperature":"92°C","grindSize":"średnio drobne","brewTime":"1:50"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 2, 2, 2, CURRENT_TIMESTAMP - INTERVAL '13 days', NULL),
                                                                                                                                        (33, 'Chemex Kenia porzeczka #33', '{"coffee":"24g","water":"380ml","temperature":"95°C","grindSize":"grubsze","brewTime":"4:15"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 3, 3, 3, CURRENT_TIMESTAMP - INTERVAL '12 days', NULL),
                                                                                                                                        (34, 'French Press Brazylia #34', '{"coffee":"28g","water":"420ml","temperature":"93°C","grindSize":"grube","brewTime":"4:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 4, 4, CURRENT_TIMESTAMP - INTERVAL '11 days', NULL),
                                                                                                                                        (35, 'Kalita balans Huila #35', '{"coffee":"20g","water":"320ml","temperature":"94°C","grindSize":"średnie","brewTime":"3:10"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, false, 6, 5, 2, CURRENT_TIMESTAMP - INTERVAL '10 days', NULL),
                                                                                                                                        (36, 'Espresso Brazylia klasyk #36', '{"coffee":"18g","water":"40ml","temperature":"93°C","grindSize":"drobne","brewTime":"1:30"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 7, 6, CURRENT_TIMESTAMP - INTERVAL '9 days', NULL),
                                                                                                                                        (37, 'Cold Brew łagodne Rwanda #37', '{"coffee":"70g","water":"1000ml","temperature":"80°C","grindSize":"grube","brewTime":"9:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 7, 8, 10, CURRENT_TIMESTAMP - INTERVAL '8 days', NULL),
                                                                                                                                        (38, 'Moka Pot Indonezja #38', '{"coffee":"20g","water":"180ml","temperature":"95°C","grindSize":"drobne","brewTime":"3:00"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 5, 6, 12, CURRENT_TIMESTAMP - INTERVAL '7 days', NULL),
                                                                                                                                        (39, 'V60 Panama Geisha #39', '{"coffee":"16g","water":"260ml","temperature":"94°C","grindSize":"średnie","brewTime":"3:05"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 8, 1, 9, CURRENT_TIMESTAMP - INTERVAL '6 days', NULL),
                                                                                                                                        (40, 'AeroPress Etiopia Anaerobic #40', '{"coffee":"17g","water":"220ml","temperature":"90°C","grindSize":"średnie","brewTime":"2:10"}'::jsonb, $seed$1. Przepłucz filtr i ogrzej naczynie.
2. Wsyp świeżo zmieloną kawę.
3. Zalej wodą zgodnie z parametrami.
4. Zamieszaj delikatnie i kontroluj czas.
5. Zapisz smak oraz ewentualne korekty.$seed$, true, 4, 2, 19, CURRENT_TIMESTAMP - INTERVAL '5 days', NULL);

INSERT INTO coffee_rating (id, value, created_at, updated_at, user_id, coffee_id) VALUES
                                                                                      (1, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 1),
                                                                                      (2, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 3),
                                                                                      (3, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 4),
                                                                                      (4, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 6),
                                                                                      (5, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 7),
                                                                                      (6, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 9),
                                                                                      (7, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 10),
                                                                                      (8, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 12),
                                                                                      (9, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 13),
                                                                                      (10, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 15),
                                                                                      (11, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 16),
                                                                                      (12, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 18),
                                                                                      (13, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 19),
                                                                                      (14, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 21),
                                                                                      (15, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 22),
                                                                                      (16, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 24),
                                                                                      (17, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 2),
                                                                                      (18, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 3),
                                                                                      (19, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 5),
                                                                                      (20, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 6),
                                                                                      (21, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 8),
                                                                                      (22, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 9),
                                                                                      (23, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 11),
                                                                                      (24, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 12),
                                                                                      (25, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 14),
                                                                                      (26, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 15),
                                                                                      (27, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 17),
                                                                                      (28, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 18),
                                                                                      (29, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 20),
                                                                                      (30, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 21),
                                                                                      (31, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 23),
                                                                                      (32, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 24),
                                                                                      (33, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 1),
                                                                                      (34, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 2),
                                                                                      (35, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 4),
                                                                                      (36, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 5),
                                                                                      (37, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 7),
                                                                                      (38, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 8),
                                                                                      (39, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 10),
                                                                                      (40, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 11),
                                                                                      (41, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 13),
                                                                                      (42, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 14),
                                                                                      (43, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 16),
                                                                                      (44, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 17),
                                                                                      (45, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 19),
                                                                                      (46, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 20),
                                                                                      (47, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 22),
                                                                                      (48, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 23),
                                                                                      (49, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 1),
                                                                                      (50, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 3),
                                                                                      (51, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 4),
                                                                                      (52, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 6),
                                                                                      (53, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 7),
                                                                                      (54, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 9),
                                                                                      (55, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 10),
                                                                                      (56, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 12),
                                                                                      (57, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 13),
                                                                                      (58, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 15),
                                                                                      (59, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 16),
                                                                                      (60, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 18),
                                                                                      (61, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 19),
                                                                                      (62, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 21),
                                                                                      (63, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 22),
                                                                                      (64, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 24),
                                                                                      (65, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 2),
                                                                                      (66, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 3),
                                                                                      (67, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 5),
                                                                                      (68, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 6),
                                                                                      (69, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 8),
                                                                                      (70, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 9),
                                                                                      (71, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 11),
                                                                                      (72, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 12),
                                                                                      (73, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 14),
                                                                                      (74, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 15),
                                                                                      (75, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 17),
                                                                                      (76, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 18),
                                                                                      (77, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 20),
                                                                                      (78, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 21),
                                                                                      (79, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 23),
                                                                                      (80, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 24),
                                                                                      (81, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 1),
                                                                                      (82, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 2),
                                                                                      (83, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 4),
                                                                                      (84, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 5),
                                                                                      (85, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 7),
                                                                                      (86, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 8),
                                                                                      (87, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 10),
                                                                                      (88, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 11),
                                                                                      (89, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 13),
                                                                                      (90, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 14),
                                                                                      (91, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 16),
                                                                                      (92, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 17),
                                                                                      (93, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 19),
                                                                                      (94, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 20),
                                                                                      (95, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 22),
                                                                                      (96, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 23),
                                                                                      (97, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 1),
                                                                                      (98, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 3),
                                                                                      (99, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 4),
                                                                                      (100, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 6),
                                                                                      (101, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 7),
                                                                                      (102, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 9),
                                                                                      (103, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 10),
                                                                                      (104, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 12),
                                                                                      (105, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 13),
                                                                                      (106, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 15),
                                                                                      (107, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 16),
                                                                                      (108, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 18),
                                                                                      (109, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 19),
                                                                                      (110, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 21),
                                                                                      (111, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 22),
                                                                                      (112, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 24),
                                                                                      (113, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 2),
                                                                                      (114, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 3),
                                                                                      (115, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 5),
                                                                                      (116, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 6),
                                                                                      (117, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 8),
                                                                                      (118, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 9),
                                                                                      (119, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 11),
                                                                                      (120, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 12),
                                                                                      (121, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 14),
                                                                                      (122, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 15),
                                                                                      (123, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 17),
                                                                                      (124, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 18),
                                                                                      (125, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 20),
                                                                                      (126, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 21),
                                                                                      (127, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 23),
                                                                                      (128, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 24),
                                                                                      (129, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 1),
                                                                                      (130, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 2),
                                                                                      (131, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 4),
                                                                                      (132, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 5),
                                                                                      (133, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 7),
                                                                                      (134, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 8),
                                                                                      (135, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 10),
                                                                                      (136, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 11),
                                                                                      (137, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 13),
                                                                                      (138, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 14),
                                                                                      (139, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 16),
                                                                                      (140, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 17),
                                                                                      (141, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 19),
                                                                                      (142, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 20),
                                                                                      (143, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 22),
                                                                                      (144, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 23),
                                                                                      (145, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 1),
                                                                                      (146, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 3),
                                                                                      (147, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 4),
                                                                                      (148, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 6),
                                                                                      (149, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 7),
                                                                                      (150, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 9),
                                                                                      (151, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 10),
                                                                                      (152, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 12),
                                                                                      (153, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 13),
                                                                                      (154, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 15),
                                                                                      (155, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 16),
                                                                                      (156, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 18),
                                                                                      (157, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 19),
                                                                                      (158, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 21),
                                                                                      (159, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 22),
                                                                                      (160, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 24),
                                                                                      (161, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 2),
                                                                                      (162, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 3),
                                                                                      (163, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 5),
                                                                                      (164, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 6),
                                                                                      (165, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 8),
                                                                                      (166, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 9),
                                                                                      (167, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 11),
                                                                                      (168, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 12),
                                                                                      (169, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 14),
                                                                                      (170, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 15),
                                                                                      (171, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 17),
                                                                                      (172, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 18),
                                                                                      (173, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 20),
                                                                                      (174, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 21),
                                                                                      (175, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 23),
                                                                                      (176, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 24);

INSERT INTO recipe_rating (id, value, created_at, updated_at, user_id, recipe_id) VALUES
                                                                                      (1, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 1),
                                                                                      (2, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 2),
                                                                                      (3, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 4),
                                                                                      (4, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 5),
                                                                                      (5, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 6),
                                                                                      (6, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 8),
                                                                                      (7, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 9),
                                                                                      (8, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 10),
                                                                                      (9, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 13),
                                                                                      (10, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 14),
                                                                                      (11, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 16),
                                                                                      (12, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 17),
                                                                                      (13, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 18),
                                                                                      (14, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 20),
                                                                                      (15, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 21),
                                                                                      (16, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 22),
                                                                                      (17, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 25),
                                                                                      (18, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 26),
                                                                                      (19, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 28),
                                                                                      (20, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 29),
                                                                                      (21, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 30),
                                                                                      (22, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 32),
                                                                                      (23, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 33),
                                                                                      (24, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 34),
                                                                                      (25, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 36),
                                                                                      (26, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 37),
                                                                                      (27, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 38),
                                                                                      (28, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 40),
                                                                                      (29, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 1),
                                                                                      (30, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 3),
                                                                                      (31, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 4),
                                                                                      (32, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 5),
                                                                                      (33, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 7),
                                                                                      (34, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 8),
                                                                                      (35, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 9),
                                                                                      (36, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 11),
                                                                                      (37, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 13),
                                                                                      (38, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 15),
                                                                                      (39, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 16),
                                                                                      (40, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 17),
                                                                                      (41, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 19),
                                                                                      (42, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 20),
                                                                                      (43, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 21),
                                                                                      (44, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 23),
                                                                                      (45, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 25),
                                                                                      (46, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 27),
                                                                                      (47, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 28),
                                                                                      (48, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 29),
                                                                                      (49, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 31),
                                                                                      (50, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 32),
                                                                                      (51, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 33),
                                                                                      (52, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 36),
                                                                                      (53, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 37),
                                                                                      (54, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 39),
                                                                                      (55, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 40),
                                                                                      (56, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 2),
                                                                                      (57, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 3),
                                                                                      (58, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 4),
                                                                                      (59, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 6),
                                                                                      (60, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 7),
                                                                                      (61, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 8),
                                                                                      (62, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 10),
                                                                                      (63, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 11),
                                                                                      (64, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 14),
                                                                                      (65, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 15),
                                                                                      (66, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 16),
                                                                                      (67, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 18),
                                                                                      (68, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 19),
                                                                                      (69, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 20),
                                                                                      (70, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 22),
                                                                                      (71, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 23),
                                                                                      (72, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 26),
                                                                                      (73, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 27),
                                                                                      (74, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 28),
                                                                                      (75, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 30),
                                                                                      (76, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 31),
                                                                                      (77, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 32),
                                                                                      (78, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 34),
                                                                                      (79, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 36),
                                                                                      (80, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 38),
                                                                                      (81, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 39),
                                                                                      (82, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, 40),
                                                                                      (83, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 1),
                                                                                      (84, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 2),
                                                                                      (85, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 3),
                                                                                      (86, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 5),
                                                                                      (87, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 6),
                                                                                      (88, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 7),
                                                                                      (89, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 9),
                                                                                      (90, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 10),
                                                                                      (91, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 11),
                                                                                      (92, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 13),
                                                                                      (93, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 14),
                                                                                      (94, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 15),
                                                                                      (95, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 17),
                                                                                      (96, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 18),
                                                                                      (97, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 19),
                                                                                      (98, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 21),
                                                                                      (99, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 22),
                                                                                      (100, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 23),
                                                                                      (101, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 25),
                                                                                      (102, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 26),
                                                                                      (103, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 27),
                                                                                      (104, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 29),
                                                                                      (105, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 30),
                                                                                      (106, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 31),
                                                                                      (107, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 33),
                                                                                      (108, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 34),
                                                                                      (109, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 37),
                                                                                      (110, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 38),
                                                                                      (111, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 4, 39),
                                                                                      (112, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 1),
                                                                                      (113, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 2),
                                                                                      (114, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 4),
                                                                                      (115, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 5),
                                                                                      (116, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 6),
                                                                                      (117, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 8),
                                                                                      (118, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 9),
                                                                                      (119, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 10),
                                                                                      (120, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 13),
                                                                                      (121, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 14),
                                                                                      (122, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 16),
                                                                                      (123, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 17),
                                                                                      (124, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 18),
                                                                                      (125, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 20),
                                                                                      (126, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 21),
                                                                                      (127, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 22),
                                                                                      (128, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 25),
                                                                                      (129, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 26),
                                                                                      (130, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 28),
                                                                                      (131, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 29),
                                                                                      (132, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 30),
                                                                                      (133, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 32),
                                                                                      (134, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 33),
                                                                                      (135, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 34),
                                                                                      (136, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 36),
                                                                                      (137, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 37),
                                                                                      (138, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 38),
                                                                                      (139, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 5, 40),
                                                                                      (140, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 1),
                                                                                      (141, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 3),
                                                                                      (142, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 4),
                                                                                      (143, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 5),
                                                                                      (144, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 7),
                                                                                      (145, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 8),
                                                                                      (146, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 9),
                                                                                      (147, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 11),
                                                                                      (148, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 13),
                                                                                      (149, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 15),
                                                                                      (150, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 16),
                                                                                      (151, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 17),
                                                                                      (152, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 19),
                                                                                      (153, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 20),
                                                                                      (154, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 21),
                                                                                      (155, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 23),
                                                                                      (156, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 25),
                                                                                      (157, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 27),
                                                                                      (158, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 28),
                                                                                      (159, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 29),
                                                                                      (160, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 31),
                                                                                      (161, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 32),
                                                                                      (162, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 33),
                                                                                      (163, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 36),
                                                                                      (164, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 37),
                                                                                      (165, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 39),
                                                                                      (166, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 6, 40),
                                                                                      (167, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 2),
                                                                                      (168, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 3),
                                                                                      (169, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 4),
                                                                                      (170, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 6),
                                                                                      (171, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 7),
                                                                                      (172, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 8),
                                                                                      (173, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 10),
                                                                                      (174, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 11),
                                                                                      (175, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 14),
                                                                                      (176, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 15),
                                                                                      (177, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 16),
                                                                                      (178, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 18),
                                                                                      (179, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 19),
                                                                                      (180, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 20),
                                                                                      (181, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 22),
                                                                                      (182, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 23),
                                                                                      (183, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 26),
                                                                                      (184, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 27),
                                                                                      (185, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 28),
                                                                                      (186, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 30),
                                                                                      (187, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 31),
                                                                                      (188, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 32),
                                                                                      (189, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 34),
                                                                                      (190, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 36),
                                                                                      (191, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 38),
                                                                                      (192, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 39),
                                                                                      (193, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 7, 40),
                                                                                      (194, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 1),
                                                                                      (195, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 2),
                                                                                      (196, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 3),
                                                                                      (197, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 5),
                                                                                      (198, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 6),
                                                                                      (199, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 7),
                                                                                      (200, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 9),
                                                                                      (201, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 10),
                                                                                      (202, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 11),
                                                                                      (203, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 13),
                                                                                      (204, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 14),
                                                                                      (205, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 15),
                                                                                      (206, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 17),
                                                                                      (207, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 18),
                                                                                      (208, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 19),
                                                                                      (209, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 21),
                                                                                      (210, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 22),
                                                                                      (211, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 23),
                                                                                      (212, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 25),
                                                                                      (213, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 26),
                                                                                      (214, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 27),
                                                                                      (215, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 29),
                                                                                      (216, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 30),
                                                                                      (217, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 31),
                                                                                      (218, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 33),
                                                                                      (219, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 34),
                                                                                      (220, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 37),
                                                                                      (221, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 38),
                                                                                      (222, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 8, 39),
                                                                                      (223, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 1),
                                                                                      (224, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 2),
                                                                                      (225, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 4),
                                                                                      (226, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 5),
                                                                                      (227, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 6),
                                                                                      (228, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 8),
                                                                                      (229, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 9),
                                                                                      (230, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 10),
                                                                                      (231, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 13),
                                                                                      (232, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 14),
                                                                                      (233, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 16),
                                                                                      (234, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 17),
                                                                                      (235, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 18),
                                                                                      (236, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 20),
                                                                                      (237, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 21),
                                                                                      (238, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 22),
                                                                                      (239, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 25),
                                                                                      (240, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 26),
                                                                                      (241, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 28),
                                                                                      (242, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 29),
                                                                                      (243, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 30),
                                                                                      (244, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 32),
                                                                                      (245, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 33),
                                                                                      (246, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 34),
                                                                                      (247, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 36),
                                                                                      (248, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 37),
                                                                                      (249, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 38),
                                                                                      (250, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 9, 40),
                                                                                      (251, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 1),
                                                                                      (252, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 3),
                                                                                      (253, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 4),
                                                                                      (254, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 5),
                                                                                      (255, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 7),
                                                                                      (256, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 8),
                                                                                      (257, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 9),
                                                                                      (258, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 11),
                                                                                      (259, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 13),
                                                                                      (260, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 15),
                                                                                      (261, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 16),
                                                                                      (262, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 17),
                                                                                      (263, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 19),
                                                                                      (264, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 20),
                                                                                      (265, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 21),
                                                                                      (266, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 23),
                                                                                      (267, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 25),
                                                                                      (268, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 27),
                                                                                      (269, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 28),
                                                                                      (270, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 29),
                                                                                      (271, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 31),
                                                                                      (272, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 32),
                                                                                      (273, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 33),
                                                                                      (274, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 36),
                                                                                      (275, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 37),
                                                                                      (276, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 39),
                                                                                      (277, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 10, 40),
                                                                                      (278, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 2),
                                                                                      (279, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 3),
                                                                                      (280, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 4),
                                                                                      (281, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 6),
                                                                                      (282, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 7),
                                                                                      (283, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 8),
                                                                                      (284, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 10),
                                                                                      (285, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 11),
                                                                                      (286, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 14),
                                                                                      (287, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 15),
                                                                                      (288, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 16),
                                                                                      (289, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 18),
                                                                                      (290, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 19),
                                                                                      (291, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 20),
                                                                                      (292, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 22),
                                                                                      (293, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 23),
                                                                                      (294, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 26),
                                                                                      (295, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 27),
                                                                                      (296, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 28),
                                                                                      (297, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 30),
                                                                                      (298, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 31),
                                                                                      (299, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 32),
                                                                                      (300, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 34),
                                                                                      (301, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 36),
                                                                                      (302, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 38),
                                                                                      (303, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 39),
                                                                                      (304, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 11, 40);

INSERT INTO user_recipe_favorite (user_id, recipe_id, created_at) VALUES
                                                                      (1, 1, CURRENT_TIMESTAMP),
                                                                      (1, 2, CURRENT_TIMESTAMP),
                                                                      (1, 6, CURRENT_TIMESTAMP),
                                                                      (1, 7, CURRENT_TIMESTAMP),
                                                                      (1, 11, CURRENT_TIMESTAMP),
                                                                      (1, 16, CURRENT_TIMESTAMP),
                                                                      (1, 17, CURRENT_TIMESTAMP),
                                                                      (1, 21, CURRENT_TIMESTAMP),
                                                                      (1, 22, CURRENT_TIMESTAMP),
                                                                      (1, 26, CURRENT_TIMESTAMP),
                                                                      (1, 27, CURRENT_TIMESTAMP),
                                                                      (1, 31, CURRENT_TIMESTAMP),
                                                                      (1, 32, CURRENT_TIMESTAMP),
                                                                      (1, 36, CURRENT_TIMESTAMP),
                                                                      (1, 37, CURRENT_TIMESTAMP),
                                                                      (2, 1, CURRENT_TIMESTAMP),
                                                                      (2, 3, CURRENT_TIMESTAMP),
                                                                      (2, 6, CURRENT_TIMESTAMP),
                                                                      (2, 8, CURRENT_TIMESTAMP),
                                                                      (2, 11, CURRENT_TIMESTAMP),
                                                                      (2, 13, CURRENT_TIMESTAMP),
                                                                      (2, 16, CURRENT_TIMESTAMP),
                                                                      (2, 18, CURRENT_TIMESTAMP),
                                                                      (2, 21, CURRENT_TIMESTAMP),
                                                                      (2, 23, CURRENT_TIMESTAMP),
                                                                      (2, 26, CURRENT_TIMESTAMP),
                                                                      (2, 28, CURRENT_TIMESTAMP),
                                                                      (2, 31, CURRENT_TIMESTAMP),
                                                                      (2, 33, CURRENT_TIMESTAMP),
                                                                      (2, 36, CURRENT_TIMESTAMP),
                                                                      (2, 38, CURRENT_TIMESTAMP),
                                                                      (3, 2, CURRENT_TIMESTAMP),
                                                                      (3, 4, CURRENT_TIMESTAMP),
                                                                      (3, 7, CURRENT_TIMESTAMP),
                                                                      (3, 9, CURRENT_TIMESTAMP),
                                                                      (3, 14, CURRENT_TIMESTAMP),
                                                                      (3, 17, CURRENT_TIMESTAMP),
                                                                      (3, 19, CURRENT_TIMESTAMP),
                                                                      (3, 22, CURRENT_TIMESTAMP),
                                                                      (3, 27, CURRENT_TIMESTAMP),
                                                                      (3, 29, CURRENT_TIMESTAMP),
                                                                      (3, 32, CURRENT_TIMESTAMP),
                                                                      (3, 34, CURRENT_TIMESTAMP),
                                                                      (3, 37, CURRENT_TIMESTAMP),
                                                                      (3, 39, CURRENT_TIMESTAMP),
                                                                      (4, 3, CURRENT_TIMESTAMP),
                                                                      (4, 4, CURRENT_TIMESTAMP),
                                                                      (4, 8, CURRENT_TIMESTAMP),
                                                                      (4, 9, CURRENT_TIMESTAMP),
                                                                      (4, 13, CURRENT_TIMESTAMP),
                                                                      (4, 14, CURRENT_TIMESTAMP),
                                                                      (4, 18, CURRENT_TIMESTAMP),
                                                                      (4, 19, CURRENT_TIMESTAMP),
                                                                      (4, 23, CURRENT_TIMESTAMP),
                                                                      (4, 28, CURRENT_TIMESTAMP),
                                                                      (4, 29, CURRENT_TIMESTAMP),
                                                                      (4, 33, CURRENT_TIMESTAMP),
                                                                      (4, 34, CURRENT_TIMESTAMP),
                                                                      (4, 38, CURRENT_TIMESTAMP),
                                                                      (4, 39, CURRENT_TIMESTAMP),
                                                                      (6, 1, CURRENT_TIMESTAMP),
                                                                      (6, 2, CURRENT_TIMESTAMP),
                                                                      (6, 6, CURRENT_TIMESTAMP),
                                                                      (6, 7, CURRENT_TIMESTAMP),
                                                                      (6, 11, CURRENT_TIMESTAMP),
                                                                      (6, 16, CURRENT_TIMESTAMP),
                                                                      (6, 17, CURRENT_TIMESTAMP),
                                                                      (6, 21, CURRENT_TIMESTAMP),
                                                                      (6, 22, CURRENT_TIMESTAMP),
                                                                      (6, 26, CURRENT_TIMESTAMP),
                                                                      (6, 27, CURRENT_TIMESTAMP),
                                                                      (6, 31, CURRENT_TIMESTAMP),
                                                                      (6, 32, CURRENT_TIMESTAMP),
                                                                      (6, 36, CURRENT_TIMESTAMP),
                                                                      (6, 37, CURRENT_TIMESTAMP),
                                                                      (7, 1, CURRENT_TIMESTAMP),
                                                                      (7, 3, CURRENT_TIMESTAMP),
                                                                      (7, 6, CURRENT_TIMESTAMP),
                                                                      (7, 8, CURRENT_TIMESTAMP),
                                                                      (7, 11, CURRENT_TIMESTAMP),
                                                                      (7, 13, CURRENT_TIMESTAMP),
                                                                      (7, 16, CURRENT_TIMESTAMP),
                                                                      (7, 18, CURRENT_TIMESTAMP),
                                                                      (7, 21, CURRENT_TIMESTAMP),
                                                                      (7, 23, CURRENT_TIMESTAMP),
                                                                      (7, 26, CURRENT_TIMESTAMP),
                                                                      (7, 28, CURRENT_TIMESTAMP),
                                                                      (7, 31, CURRENT_TIMESTAMP),
                                                                      (7, 33, CURRENT_TIMESTAMP),
                                                                      (7, 36, CURRENT_TIMESTAMP),
                                                                      (7, 38, CURRENT_TIMESTAMP),
                                                                      (8, 2, CURRENT_TIMESTAMP),
                                                                      (8, 4, CURRENT_TIMESTAMP),
                                                                      (8, 7, CURRENT_TIMESTAMP),
                                                                      (8, 9, CURRENT_TIMESTAMP),
                                                                      (8, 14, CURRENT_TIMESTAMP),
                                                                      (8, 17, CURRENT_TIMESTAMP),
                                                                      (8, 19, CURRENT_TIMESTAMP),
                                                                      (8, 22, CURRENT_TIMESTAMP),
                                                                      (8, 27, CURRENT_TIMESTAMP),
                                                                      (8, 29, CURRENT_TIMESTAMP),
                                                                      (8, 32, CURRENT_TIMESTAMP),
                                                                      (8, 34, CURRENT_TIMESTAMP),
                                                                      (8, 37, CURRENT_TIMESTAMP),
                                                                      (8, 39, CURRENT_TIMESTAMP),
                                                                      (9, 3, CURRENT_TIMESTAMP),
                                                                      (9, 4, CURRENT_TIMESTAMP),
                                                                      (9, 8, CURRENT_TIMESTAMP),
                                                                      (9, 9, CURRENT_TIMESTAMP),
                                                                      (9, 13, CURRENT_TIMESTAMP),
                                                                      (9, 14, CURRENT_TIMESTAMP),
                                                                      (9, 18, CURRENT_TIMESTAMP),
                                                                      (9, 19, CURRENT_TIMESTAMP),
                                                                      (9, 23, CURRENT_TIMESTAMP),
                                                                      (9, 28, CURRENT_TIMESTAMP),
                                                                      (9, 29, CURRENT_TIMESTAMP),
                                                                      (9, 33, CURRENT_TIMESTAMP),
                                                                      (9, 34, CURRENT_TIMESTAMP),
                                                                      (9, 38, CURRENT_TIMESTAMP),
                                                                      (9, 39, CURRENT_TIMESTAMP),
                                                                      (11, 1, CURRENT_TIMESTAMP),
                                                                      (11, 2, CURRENT_TIMESTAMP),
                                                                      (11, 6, CURRENT_TIMESTAMP),
                                                                      (11, 7, CURRENT_TIMESTAMP),
                                                                      (11, 11, CURRENT_TIMESTAMP),
                                                                      (11, 16, CURRENT_TIMESTAMP),
                                                                      (11, 17, CURRENT_TIMESTAMP),
                                                                      (11, 21, CURRENT_TIMESTAMP),
                                                                      (11, 22, CURRENT_TIMESTAMP),
                                                                      (11, 26, CURRENT_TIMESTAMP),
                                                                      (11, 27, CURRENT_TIMESTAMP),
                                                                      (11, 31, CURRENT_TIMESTAMP),
                                                                      (11, 32, CURRENT_TIMESTAMP),
                                                                      (11, 36, CURRENT_TIMESTAMP),
                                                                      (11, 37, CURRENT_TIMESTAMP);

INSERT INTO user_coffee_favorite (user_id, coffee_id, created_at) VALUES
                                                                      (1, 1, CURRENT_TIMESTAMP),
                                                                      (1, 4, CURRENT_TIMESTAMP),
                                                                      (1, 5, CURRENT_TIMESTAMP),
                                                                      (1, 8, CURRENT_TIMESTAMP),
                                                                      (1, 9, CURRENT_TIMESTAMP),
                                                                      (1, 12, CURRENT_TIMESTAMP),
                                                                      (1, 13, CURRENT_TIMESTAMP),
                                                                      (1, 16, CURRENT_TIMESTAMP),
                                                                      (1, 17, CURRENT_TIMESTAMP),
                                                                      (1, 20, CURRENT_TIMESTAMP),
                                                                      (1, 21, CURRENT_TIMESTAMP),
                                                                      (1, 24, CURRENT_TIMESTAMP),
                                                                      (2, 3, CURRENT_TIMESTAMP),
                                                                      (2, 4, CURRENT_TIMESTAMP),
                                                                      (2, 7, CURRENT_TIMESTAMP),
                                                                      (2, 8, CURRENT_TIMESTAMP),
                                                                      (2, 11, CURRENT_TIMESTAMP),
                                                                      (2, 12, CURRENT_TIMESTAMP),
                                                                      (2, 15, CURRENT_TIMESTAMP),
                                                                      (2, 16, CURRENT_TIMESTAMP),
                                                                      (2, 19, CURRENT_TIMESTAMP),
                                                                      (2, 20, CURRENT_TIMESTAMP),
                                                                      (2, 23, CURRENT_TIMESTAMP),
                                                                      (2, 24, CURRENT_TIMESTAMP),
                                                                      (3, 2, CURRENT_TIMESTAMP),
                                                                      (3, 3, CURRENT_TIMESTAMP),
                                                                      (3, 6, CURRENT_TIMESTAMP),
                                                                      (3, 7, CURRENT_TIMESTAMP),
                                                                      (3, 10, CURRENT_TIMESTAMP),
                                                                      (3, 11, CURRENT_TIMESTAMP),
                                                                      (3, 14, CURRENT_TIMESTAMP),
                                                                      (3, 15, CURRENT_TIMESTAMP),
                                                                      (3, 18, CURRENT_TIMESTAMP),
                                                                      (3, 19, CURRENT_TIMESTAMP),
                                                                      (3, 22, CURRENT_TIMESTAMP),
                                                                      (3, 23, CURRENT_TIMESTAMP),
                                                                      (4, 1, CURRENT_TIMESTAMP),
                                                                      (4, 2, CURRENT_TIMESTAMP),
                                                                      (4, 5, CURRENT_TIMESTAMP),
                                                                      (4, 6, CURRENT_TIMESTAMP),
                                                                      (4, 9, CURRENT_TIMESTAMP),
                                                                      (4, 10, CURRENT_TIMESTAMP),
                                                                      (4, 13, CURRENT_TIMESTAMP),
                                                                      (4, 14, CURRENT_TIMESTAMP),
                                                                      (4, 17, CURRENT_TIMESTAMP),
                                                                      (4, 18, CURRENT_TIMESTAMP),
                                                                      (4, 21, CURRENT_TIMESTAMP),
                                                                      (4, 22, CURRENT_TIMESTAMP),
                                                                      (5, 1, CURRENT_TIMESTAMP),
                                                                      (5, 4, CURRENT_TIMESTAMP),
                                                                      (5, 5, CURRENT_TIMESTAMP),
                                                                      (5, 8, CURRENT_TIMESTAMP),
                                                                      (5, 9, CURRENT_TIMESTAMP),
                                                                      (5, 12, CURRENT_TIMESTAMP),
                                                                      (5, 13, CURRENT_TIMESTAMP),
                                                                      (5, 16, CURRENT_TIMESTAMP),
                                                                      (5, 17, CURRENT_TIMESTAMP),
                                                                      (5, 20, CURRENT_TIMESTAMP),
                                                                      (5, 21, CURRENT_TIMESTAMP),
                                                                      (5, 24, CURRENT_TIMESTAMP),
                                                                      (6, 3, CURRENT_TIMESTAMP),
                                                                      (6, 4, CURRENT_TIMESTAMP),
                                                                      (6, 7, CURRENT_TIMESTAMP),
                                                                      (6, 8, CURRENT_TIMESTAMP),
                                                                      (6, 11, CURRENT_TIMESTAMP),
                                                                      (6, 12, CURRENT_TIMESTAMP),
                                                                      (6, 15, CURRENT_TIMESTAMP),
                                                                      (6, 16, CURRENT_TIMESTAMP),
                                                                      (6, 19, CURRENT_TIMESTAMP),
                                                                      (6, 20, CURRENT_TIMESTAMP),
                                                                      (6, 23, CURRENT_TIMESTAMP),
                                                                      (6, 24, CURRENT_TIMESTAMP),
                                                                      (7, 2, CURRENT_TIMESTAMP),
                                                                      (7, 3, CURRENT_TIMESTAMP),
                                                                      (7, 6, CURRENT_TIMESTAMP),
                                                                      (7, 7, CURRENT_TIMESTAMP),
                                                                      (7, 10, CURRENT_TIMESTAMP),
                                                                      (7, 11, CURRENT_TIMESTAMP),
                                                                      (7, 14, CURRENT_TIMESTAMP),
                                                                      (7, 15, CURRENT_TIMESTAMP),
                                                                      (7, 18, CURRENT_TIMESTAMP),
                                                                      (7, 19, CURRENT_TIMESTAMP),
                                                                      (7, 22, CURRENT_TIMESTAMP),
                                                                      (7, 23, CURRENT_TIMESTAMP),
                                                                      (8, 1, CURRENT_TIMESTAMP),
                                                                      (8, 2, CURRENT_TIMESTAMP),
                                                                      (8, 5, CURRENT_TIMESTAMP),
                                                                      (8, 6, CURRENT_TIMESTAMP),
                                                                      (8, 9, CURRENT_TIMESTAMP),
                                                                      (8, 10, CURRENT_TIMESTAMP),
                                                                      (8, 13, CURRENT_TIMESTAMP),
                                                                      (8, 14, CURRENT_TIMESTAMP),
                                                                      (8, 17, CURRENT_TIMESTAMP),
                                                                      (8, 18, CURRENT_TIMESTAMP),
                                                                      (8, 21, CURRENT_TIMESTAMP),
                                                                      (8, 22, CURRENT_TIMESTAMP),
                                                                      (9, 1, CURRENT_TIMESTAMP),
                                                                      (9, 4, CURRENT_TIMESTAMP),
                                                                      (9, 5, CURRENT_TIMESTAMP),
                                                                      (9, 8, CURRENT_TIMESTAMP),
                                                                      (9, 9, CURRENT_TIMESTAMP),
                                                                      (9, 12, CURRENT_TIMESTAMP),
                                                                      (9, 13, CURRENT_TIMESTAMP),
                                                                      (9, 16, CURRENT_TIMESTAMP),
                                                                      (9, 17, CURRENT_TIMESTAMP),
                                                                      (9, 20, CURRENT_TIMESTAMP),
                                                                      (9, 21, CURRENT_TIMESTAMP),
                                                                      (9, 24, CURRENT_TIMESTAMP),
                                                                      (10, 3, CURRENT_TIMESTAMP),
                                                                      (10, 4, CURRENT_TIMESTAMP),
                                                                      (10, 7, CURRENT_TIMESTAMP),
                                                                      (10, 8, CURRENT_TIMESTAMP),
                                                                      (10, 11, CURRENT_TIMESTAMP),
                                                                      (10, 12, CURRENT_TIMESTAMP),
                                                                      (10, 15, CURRENT_TIMESTAMP),
                                                                      (10, 16, CURRENT_TIMESTAMP),
                                                                      (10, 19, CURRENT_TIMESTAMP),
                                                                      (10, 20, CURRENT_TIMESTAMP),
                                                                      (10, 23, CURRENT_TIMESTAMP),
                                                                      (10, 24, CURRENT_TIMESTAMP),
                                                                      (11, 2, CURRENT_TIMESTAMP),
                                                                      (11, 3, CURRENT_TIMESTAMP),
                                                                      (11, 6, CURRENT_TIMESTAMP),
                                                                      (11, 7, CURRENT_TIMESTAMP),
                                                                      (11, 10, CURRENT_TIMESTAMP),
                                                                      (11, 11, CURRENT_TIMESTAMP),
                                                                      (11, 14, CURRENT_TIMESTAMP),
                                                                      (11, 15, CURRENT_TIMESTAMP),
                                                                      (11, 18, CURRENT_TIMESTAMP),
                                                                      (11, 19, CURRENT_TIMESTAMP),
                                                                      (11, 22, CURRENT_TIMESTAMP),
                                                                      (11, 23, CURRENT_TIMESTAMP);

INSERT INTO follow (follower_id, followed_id, created_at) VALUES
                                                              (1, 5, CURRENT_TIMESTAMP),
                                                              (1, 9, CURRENT_TIMESTAMP),
                                                              (2, 6, CURRENT_TIMESTAMP),
                                                              (2, 10, CURRENT_TIMESTAMP),
                                                              (3, 7, CURRENT_TIMESTAMP),
                                                              (3, 11, CURRENT_TIMESTAMP),
                                                              (4, 8, CURRENT_TIMESTAMP),
                                                              (5, 1, CURRENT_TIMESTAMP),
                                                              (5, 9, CURRENT_TIMESTAMP),
                                                              (6, 2, CURRENT_TIMESTAMP),
                                                              (6, 10, CURRENT_TIMESTAMP),
                                                              (7, 3, CURRENT_TIMESTAMP),
                                                              (7, 11, CURRENT_TIMESTAMP),
                                                              (8, 4, CURRENT_TIMESTAMP),
                                                              (9, 1, CURRENT_TIMESTAMP),
                                                              (9, 5, CURRENT_TIMESTAMP),
                                                              (10, 2, CURRENT_TIMESTAMP),
                                                              (10, 6, CURRENT_TIMESTAMP),
                                                              (11, 3, CURRENT_TIMESTAMP),
                                                              (11, 7, CURRENT_TIMESTAMP);

INSERT INTO quick_note (id, content, created_at, updated_at, user_id) VALUES
                                                                          (1, $seed$Notatka degustacyjna #1: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP - INTERVAL '30 days', 1),
                                                                          (2, $seed$Notatka degustacyjna #2: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '29 days', CURRENT_TIMESTAMP - INTERVAL '29 days', 2),
                                                                          (3, $seed$Notatka degustacyjna #3: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '28 days', CURRENT_TIMESTAMP - INTERVAL '28 days', 3),
                                                                          (4, $seed$Notatka degustacyjna #4: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '27 days', CURRENT_TIMESTAMP - INTERVAL '27 days', 4),
                                                                          (5, $seed$Notatka degustacyjna #5: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '26 days', CURRENT_TIMESTAMP - INTERVAL '26 days', 5),
                                                                          (6, $seed$Notatka degustacyjna #6: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '25 days', CURRENT_TIMESTAMP - INTERVAL '25 days', 6),
                                                                          (7, $seed$Notatka degustacyjna #7: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '24 days', CURRENT_TIMESTAMP - INTERVAL '24 days', 7),
                                                                          (8, $seed$Notatka degustacyjna #8: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '23 days', CURRENT_TIMESTAMP - INTERVAL '23 days', 8),
                                                                          (9, $seed$Notatka degustacyjna #9: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '22 days', CURRENT_TIMESTAMP - INTERVAL '22 days', 9),
                                                                          (10, $seed$Notatka degustacyjna #10: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '21 days', CURRENT_TIMESTAMP - INTERVAL '21 days', 10),
                                                                          (11, $seed$Notatka degustacyjna #11: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '20 days', CURRENT_TIMESTAMP - INTERVAL '20 days', 1),
                                                                          (12, $seed$Notatka degustacyjna #12: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '19 days', CURRENT_TIMESTAMP - INTERVAL '19 days', 2),
                                                                          (13, $seed$Notatka degustacyjna #13: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '18 days', CURRENT_TIMESTAMP - INTERVAL '18 days', 3),
                                                                          (14, $seed$Notatka degustacyjna #14: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '17 days', CURRENT_TIMESTAMP - INTERVAL '17 days', 4),
                                                                          (15, $seed$Notatka degustacyjna #15: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '16 days', CURRENT_TIMESTAMP - INTERVAL '16 days', 5),
                                                                          (16, $seed$Notatka degustacyjna #16: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '15 days', CURRENT_TIMESTAMP - INTERVAL '15 days', 6),
                                                                          (17, $seed$Notatka degustacyjna #17: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '14 days', CURRENT_TIMESTAMP - INTERVAL '14 days', 7),
                                                                          (18, $seed$Notatka degustacyjna #18: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '13 days', CURRENT_TIMESTAMP - INTERVAL '13 days', 8),
                                                                          (19, $seed$Notatka degustacyjna #19: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '12 days', CURRENT_TIMESTAMP - INTERVAL '12 days', 9),
                                                                          (20, $seed$Notatka degustacyjna #20: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '11 days', CURRENT_TIMESTAMP - INTERVAL '11 days', 10),
                                                                          (21, $seed$Notatka degustacyjna #21: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '10 days', CURRENT_TIMESTAMP - INTERVAL '10 days', 1),
                                                                          (22, $seed$Notatka degustacyjna #22: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '9 days', CURRENT_TIMESTAMP - INTERVAL '9 days', 2),
                                                                          (23, $seed$Notatka degustacyjna #23: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '8 days', CURRENT_TIMESTAMP - INTERVAL '8 days', 3),
                                                                          (24, $seed$Notatka degustacyjna #24: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '7 days', CURRENT_TIMESTAMP - INTERVAL '7 days', 4),
                                                                          (25, $seed$Notatka degustacyjna #25: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '6 days', CURRENT_TIMESTAMP - INTERVAL '6 days', 5),
                                                                          (26, $seed$Notatka degustacyjna #26: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '5 days', CURRENT_TIMESTAMP - INTERVAL '5 days', 6),
                                                                          (27, $seed$Notatka degustacyjna #27: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '4 days', CURRENT_TIMESTAMP - INTERVAL '4 days', 7),
                                                                          (28, $seed$Notatka degustacyjna #28: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '3 days', CURRENT_TIMESTAMP - INTERVAL '3 days', 8),
                                                                          (29, $seed$Notatka degustacyjna #29: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '2 days', CURRENT_TIMESTAMP - INTERVAL '2 days', 9),
                                                                          (30, $seed$Notatka degustacyjna #30: czysty profil, dobra słodycz i krótki finisz. Do powtórki przy drobniejszym mieleniu.$seed$, CURRENT_TIMESTAMP - INTERVAL '1 days', CURRENT_TIMESTAMP - INTERVAL '1 days', 10);

INSERT INTO cupping_session (id, name, description, session_date, created_at, user_id) VALUES
                                                                                           (1, 'Cupping sesja #1', 'Porównanie trzech kaw specialty, sesja demo numer 1.', CURRENT_TIMESTAMP - INTERVAL '19 days', CURRENT_TIMESTAMP - INTERVAL '19 days', 1),
                                                                                           (2, 'Cupping sesja #2', 'Porównanie trzech kaw specialty, sesja demo numer 2.', CURRENT_TIMESTAMP - INTERVAL '18 days', CURRENT_TIMESTAMP - INTERVAL '18 days', 2),
                                                                                           (3, 'Cupping sesja #3', 'Porównanie trzech kaw specialty, sesja demo numer 3.', CURRENT_TIMESTAMP - INTERVAL '17 days', CURRENT_TIMESTAMP - INTERVAL '17 days', 3),
                                                                                           (4, 'Cupping sesja #4', 'Porównanie trzech kaw specialty, sesja demo numer 4.', CURRENT_TIMESTAMP - INTERVAL '16 days', CURRENT_TIMESTAMP - INTERVAL '16 days', 4),
                                                                                           (5, 'Cupping sesja #5', 'Porównanie trzech kaw specialty, sesja demo numer 5.', CURRENT_TIMESTAMP - INTERVAL '15 days', CURRENT_TIMESTAMP - INTERVAL '15 days', 5),
                                                                                           (6, 'Cupping sesja #6', 'Porównanie trzech kaw specialty, sesja demo numer 6.', CURRENT_TIMESTAMP - INTERVAL '14 days', CURRENT_TIMESTAMP - INTERVAL '14 days', 6),
                                                                                           (7, 'Cupping sesja #7', 'Porównanie trzech kaw specialty, sesja demo numer 7.', CURRENT_TIMESTAMP - INTERVAL '13 days', CURRENT_TIMESTAMP - INTERVAL '13 days', 7),
                                                                                           (8, 'Cupping sesja #8', 'Porównanie trzech kaw specialty, sesja demo numer 8.', CURRENT_TIMESTAMP - INTERVAL '12 days', CURRENT_TIMESTAMP - INTERVAL '12 days', 8),
                                                                                           (9, 'Cupping sesja #9', 'Porównanie trzech kaw specialty, sesja demo numer 9.', CURRENT_TIMESTAMP - INTERVAL '11 days', CURRENT_TIMESTAMP - INTERVAL '11 days', 1),
                                                                                           (10, 'Cupping sesja #10', 'Porównanie trzech kaw specialty, sesja demo numer 10.', CURRENT_TIMESTAMP - INTERVAL '10 days', CURRENT_TIMESTAMP - INTERVAL '10 days', 2),
                                                                                           (11, 'Cupping sesja #11', 'Porównanie trzech kaw specialty, sesja demo numer 11.', CURRENT_TIMESTAMP - INTERVAL '9 days', CURRENT_TIMESTAMP - INTERVAL '9 days', 3),
                                                                                           (12, 'Cupping sesja #12', 'Porównanie trzech kaw specialty, sesja demo numer 12.', CURRENT_TIMESTAMP - INTERVAL '8 days', CURRENT_TIMESTAMP - INTERVAL '8 days', 4);

INSERT INTO cupping_session_coffee (id, notes, aroma_score, sweetness_score, acidity_score, body_score, flavor_profile_notes, clean_cup, overall_score, created_at, coffee_id, cupping_session_id) VALUES
                                                                                                                                                                                                       (1, $seed$Kawa 3: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 8, 7, 7, 'cytrusy, słodycz, czysty posmak', true, 8, CURRENT_TIMESTAMP, 3, 1),
                                                                                                                                                                                                       (2, $seed$Kawa 8: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 9, 9, 6, 'cytrusy, słodycz, czysty posmak', true, 9, CURRENT_TIMESTAMP, 8, 1),
                                                                                                                                                                                                       (3, $seed$Kawa 13: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 6, 7, 9, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 13, 1),
                                                                                                                                                                                                       (4, $seed$Kawa 5: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 6, 8, 8, 'cytrusy, słodycz, czysty posmak', true, 9, CURRENT_TIMESTAMP, 5, 2),
                                                                                                                                                                                                       (5, $seed$Kawa 10: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 7, 6, 7, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 10, 2),
                                                                                                                                                                                                       (6, $seed$Kawa 15: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 8, 8, 6, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 15, 2),
                                                                                                                                                                                                       (7, $seed$Kawa 7: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 8, 9, 9, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 7, 3),
                                                                                                                                                                                                       (8, $seed$Kawa 12: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 9, 7, 8, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 12, 3),
                                                                                                                                                                                                       (9, $seed$Kawa 17: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 6, 9, 7, 'cytrusy, słodycz, czysty posmak', false, 8, CURRENT_TIMESTAMP, 17, 3),
                                                                                                                                                                                                       (10, $seed$Kawa 9: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 6, 6, 6, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 9, 4),
                                                                                                                                                                                                       (11, $seed$Kawa 14: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 7, 8, 9, 'cytrusy, słodycz, czysty posmak', false, 8, CURRENT_TIMESTAMP, 14, 4),
                                                                                                                                                                                                       (12, $seed$Kawa 19: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 8, 6, 8, 'cytrusy, słodycz, czysty posmak', true, 9, CURRENT_TIMESTAMP, 19, 4),
                                                                                                                                                                                                       (13, $seed$Kawa 11: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 8, 7, 7, 'cytrusy, słodycz, czysty posmak', false, 8, CURRENT_TIMESTAMP, 11, 5),
                                                                                                                                                                                                       (14, $seed$Kawa 16: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 9, 9, 6, 'cytrusy, słodycz, czysty posmak', true, 9, CURRENT_TIMESTAMP, 16, 5),
                                                                                                                                                                                                       (15, $seed$Kawa 21: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 6, 7, 9, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 21, 5),
                                                                                                                                                                                                       (16, $seed$Kawa 13: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 6, 8, 8, 'cytrusy, słodycz, czysty posmak', true, 9, CURRENT_TIMESTAMP, 13, 6),
                                                                                                                                                                                                       (17, $seed$Kawa 18: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 7, 6, 7, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 18, 6),
                                                                                                                                                                                                       (18, $seed$Kawa 23: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 8, 8, 6, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 23, 6),
                                                                                                                                                                                                       (19, $seed$Kawa 15: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 8, 9, 9, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 15, 7),
                                                                                                                                                                                                       (20, $seed$Kawa 20: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 9, 7, 8, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 20, 7),
                                                                                                                                                                                                       (21, $seed$Kawa 1: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 6, 9, 7, 'cytrusy, słodycz, czysty posmak', true, 8, CURRENT_TIMESTAMP, 1, 7),
                                                                                                                                                                                                       (22, $seed$Kawa 17: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 6, 6, 6, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 17, 8),
                                                                                                                                                                                                       (23, $seed$Kawa 22: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 7, 8, 9, 'cytrusy, słodycz, czysty posmak', true, 8, CURRENT_TIMESTAMP, 22, 8),
                                                                                                                                                                                                       (24, $seed$Kawa 3: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 8, 6, 8, 'cytrusy, słodycz, czysty posmak', false, 9, CURRENT_TIMESTAMP, 3, 8),
                                                                                                                                                                                                       (25, $seed$Kawa 19: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 8, 7, 7, 'cytrusy, słodycz, czysty posmak', true, 8, CURRENT_TIMESTAMP, 19, 9),
                                                                                                                                                                                                       (26, $seed$Kawa 24: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 9, 9, 6, 'cytrusy, słodycz, czysty posmak', false, 9, CURRENT_TIMESTAMP, 24, 9),
                                                                                                                                                                                                       (27, $seed$Kawa 5: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 6, 7, 9, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 5, 9),
                                                                                                                                                                                                       (28, $seed$Kawa 21: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 6, 8, 8, 'cytrusy, słodycz, czysty posmak', false, 9, CURRENT_TIMESTAMP, 21, 10),
                                                                                                                                                                                                       (29, $seed$Kawa 2: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 7, 6, 7, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 2, 10),
                                                                                                                                                                                                       (30, $seed$Kawa 7: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 8, 8, 6, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 7, 10),
                                                                                                                                                                                                       (31, $seed$Kawa 23: aromat owocowy, czysty profil i dobra słodycz.$seed$, 9, 8, 9, 9, 'cytrusy, słodycz, czysty posmak', true, 10, CURRENT_TIMESTAMP, 23, 11),
                                                                                                                                                                                                       (32, $seed$Kawa 4: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 9, 7, 8, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 4, 11),
                                                                                                                                                                                                       (33, $seed$Kawa 9: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 6, 9, 7, 'cytrusy, słodycz, czysty posmak', true, 8, CURRENT_TIMESTAMP, 9, 11),
                                                                                                                                                                                                       (34, $seed$Kawa 1: aromat owocowy, czysty profil i dobra słodycz.$seed$, 6, 6, 6, 6, 'cytrusy, słodycz, czysty posmak', true, 7, CURRENT_TIMESTAMP, 1, 12),
                                                                                                                                                                                                       (35, $seed$Kawa 6: aromat owocowy, czysty profil i dobra słodycz.$seed$, 7, 7, 8, 9, 'cytrusy, słodycz, czysty posmak', true, 8, CURRENT_TIMESTAMP, 6, 12),
                                                                                                                                                                                                       (36, $seed$Kawa 11: aromat owocowy, czysty profil i dobra słodycz.$seed$, 8, 8, 6, 8, 'cytrusy, słodycz, czysty posmak', true, 9, CURRENT_TIMESTAMP, 11, 12);

INSERT INTO article (id, title, content, status, module, created_at, updated_at, published_at, moderated_by_user_id, moderated_at, moderation_comment, user_id, coffee_id) VALUES
                                                                                                                                                                               (1, 'Etiopia Guji Natural', $seed$Kawa: Etiopia Guji Natural
Profil sensoryczny: Jagody, Jaśmin, Kwiaty, Czerwone owoce

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '29 days', CURRENT_TIMESTAMP - INTERVAL '28 days', CURRENT_TIMESTAMP - INTERVAL '28 days', 3, CURRENT_TIMESTAMP - INTERVAL '28 days', NULL, 2, 1),
                                                                                                                                                                               (2, 'Kolumbia Huila Washed', $seed$Kawa: Kolumbia Huila Washed
Profil sensoryczny: Czekolada, Karmel, Orzechy, Czerwone owoce

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '28 days', CURRENT_TIMESTAMP - INTERVAL '27 days', CURRENT_TIMESTAMP - INTERVAL '27 days', 3, CURRENT_TIMESTAMP - INTERVAL '27 days', NULL, 3, 2),
                                                                                                                                                                               (3, 'Kenia Nyeri SL28', $seed$Kawa: Kenia Nyeri SL28
Profil sensoryczny: Cytrusy, Porzeczka, Pomarańcza

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '27 days', CURRENT_TIMESTAMP - INTERVAL '26 days', CURRENT_TIMESTAMP - INTERVAL '26 days', 3, CURRENT_TIMESTAMP - INTERVAL '26 days', NULL, 4, 3),
                                                                                                                                                                               (4, 'Etiopia Sidamo Honey', $seed$Kawa: Etiopia Sidamo Honey
Profil sensoryczny: Miód, Tropikalne owoce, Kwiaty

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '26 days', CURRENT_TIMESTAMP - INTERVAL '25 days', CURRENT_TIMESTAMP - INTERVAL '25 days', 3, CURRENT_TIMESTAMP - INTERVAL '25 days', NULL, 5, 4),
                                                                                                                                                                               (5, 'Kolumbia Bourbon Natural', $seed$Kawa: Kolumbia Bourbon Natural
Profil sensoryczny: Karmel, Czerwone owoce, Śliwka

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '25 days', CURRENT_TIMESTAMP - INTERVAL '24 days', CURRENT_TIMESTAMP - INTERVAL '24 days', 3, CURRENT_TIMESTAMP - INTERVAL '24 days', NULL, 6, 5),
                                                                                                                                                                               (6, 'Brazylia Cerrado Espresso', $seed$Kawa: Brazylia Cerrado Espresso
Profil sensoryczny: Czekolada, Orzechy, Kakao

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '24 days', CURRENT_TIMESTAMP - INTERVAL '23 days', CURRENT_TIMESTAMP - INTERVAL '23 days', 3, CURRENT_TIMESTAMP - INTERVAL '23 days', NULL, 7, 6),
                                                                                                                                                                               (7, 'Kostaryka Tarrazú Honey', $seed$Kawa: Kostaryka Tarrazú Honey
Profil sensoryczny: Karmel, Miód, Brzoskwinia

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '23 days', CURRENT_TIMESTAMP - INTERVAL '22 days', CURRENT_TIMESTAMP - INTERVAL '22 days', 3, CURRENT_TIMESTAMP - INTERVAL '22 days', NULL, 8, 7),
                                                                                                                                                                               (8, 'Gwatemala Antigua Bourbon', $seed$Kawa: Gwatemala Antigua Bourbon
Profil sensoryczny: Czekolada, Orzechy, Migdały

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '22 days', CURRENT_TIMESTAMP - INTERVAL '21 days', CURRENT_TIMESTAMP - INTERVAL '21 days', 3, CURRENT_TIMESTAMP - INTERVAL '21 days', NULL, 9, 8),
                                                                                                                                                                               (9, 'Panama Boquete Geisha', $seed$Kawa: Panama Boquete Geisha
Profil sensoryczny: Jaśmin, Cytrusy, Kwiaty, Brzoskwinia

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '21 days', CURRENT_TIMESTAMP - INTERVAL '20 days', CURRENT_TIMESTAMP - INTERVAL '20 days', 3, CURRENT_TIMESTAMP - INTERVAL '20 days', NULL, 10, 9),
                                                                                                                                                                               (10, 'Rwanda Nyamasheke Washed', $seed$Kawa: Rwanda Nyamasheke Washed
Profil sensoryczny: Cytrusy, Porzeczka, Czerwone owoce

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '20 days', CURRENT_TIMESTAMP - INTERVAL '19 days', CURRENT_TIMESTAMP - INTERVAL '19 days', 3, CURRENT_TIMESTAMP - INTERVAL '19 days', NULL, 1, 10),
                                                                                                                                                                               (11, 'Peru Cajamarca Caturra', $seed$Kawa: Peru Cajamarca Caturra
Profil sensoryczny: Karmel, Kakao, Śliwka

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '19 days', CURRENT_TIMESTAMP - INTERVAL '18 days', CURRENT_TIMESTAMP - INTERVAL '18 days', 3, CURRENT_TIMESTAMP - INTERVAL '18 days', NULL, 2, 11),
                                                                                                                                                                               (12, 'Indonezja Aceh Gayo', $seed$Kawa: Indonezja Aceh Gayo
Profil sensoryczny: Czekolada, Orzechy, Wanilia

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '18 days', CURRENT_TIMESTAMP - INTERVAL '17 days', CURRENT_TIMESTAMP - INTERVAL '17 days', 3, CURRENT_TIMESTAMP - INTERVAL '17 days', NULL, 3, 12),
                                                                                                                                                                               (13, 'Etiopia Yirgacheffe Floral', $seed$Kawa: Etiopia Yirgacheffe Floral
Profil sensoryczny: Jaśmin, Herbata, Kwiaty

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '17 days', CURRENT_TIMESTAMP - INTERVAL '16 days', CURRENT_TIMESTAMP - INTERVAL '16 days', 3, CURRENT_TIMESTAMP - INTERVAL '16 days', NULL, 4, 13),
                                                                                                                                                                               (14, 'Kenia Kirinyaga AA', $seed$Kawa: Kenia Kirinyaga AA
Profil sensoryczny: Cytrusy, Porzeczka, Pomarańcza

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '16 days', CURRENT_TIMESTAMP - INTERVAL '15 days', CURRENT_TIMESTAMP - INTERVAL '15 days', 3, CURRENT_TIMESTAMP - INTERVAL '15 days', NULL, 5, 14),
                                                                                                                                                                               (15, 'Kolumbia Nariño Castillo', $seed$Kawa: Kolumbia Nariño Castillo
Profil sensoryczny: Czekolada, Karmel, Czerwone owoce

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '15 days', CURRENT_TIMESTAMP - INTERVAL '14 days', CURRENT_TIMESTAMP - INTERVAL '14 days', 3, CURRENT_TIMESTAMP - INTERVAL '14 days', NULL, 6, 15),
                                                                                                                                                                               (16, 'Brazylia Natural Sweet Espresso', $seed$Kawa: Brazylia Natural Sweet Espresso
Profil sensoryczny: Czekolada, Karmel, Kakao, Migdały

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '14 days', CURRENT_TIMESTAMP - INTERVAL '13 days', CURRENT_TIMESTAMP - INTERVAL '13 days', 3, CURRENT_TIMESTAMP - INTERVAL '13 days', NULL, 7, 16),
                                                                                                                                                                               (17, 'Gwatemala Pacamara Washed', $seed$Kawa: Gwatemala Pacamara Washed
Profil sensoryczny: Orzechy, Czerwone owoce, Brzoskwinia

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '13 days', CURRENT_TIMESTAMP - INTERVAL '12 days', CURRENT_TIMESTAMP - INTERVAL '12 days', 3, CURRENT_TIMESTAMP - INTERVAL '12 days', NULL, 8, 17),
                                                                                                                                                                               (18, 'Kostaryka Anaerobic Red', $seed$Kawa: Kostaryka Anaerobic Red
Profil sensoryczny: Tropikalne owoce, Czerwone owoce, Śliwka

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '12 days', CURRENT_TIMESTAMP - INTERVAL '11 days', CURRENT_TIMESTAMP - INTERVAL '11 days', 3, CURRENT_TIMESTAMP - INTERVAL '11 days', NULL, 9, 18),
                                                                                                                                                                               (19, 'Etiopia Guji Anaerobic', $seed$Kawa: Etiopia Guji Anaerobic
Profil sensoryczny: Jagody, Tropikalne owoce, Kwiaty, Brzoskwinia

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '11 days', CURRENT_TIMESTAMP - INTERVAL '10 days', CURRENT_TIMESTAMP - INTERVAL '10 days', 3, CURRENT_TIMESTAMP - INTERVAL '10 days', NULL, 10, 19),
                                                                                                                                                                               (20, 'Peru Organic Washed', $seed$Kawa: Peru Organic Washed
Profil sensoryczny: Karmel, Herbata, Wanilia

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP - INTERVAL '29 days', CURRENT_TIMESTAMP - INTERVAL '29 days', 3, CURRENT_TIMESTAMP - INTERVAL '29 days', NULL, 1, 20),
                                                                                                                                                                               (21, 'Kolumbia Decaf Sugarcane', $seed$Kawa: Kolumbia Decaf Sugarcane
Profil sensoryczny: Czekolada, Karmel, Wanilia

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '29 days', CURRENT_TIMESTAMP - INTERVAL '28 days', CURRENT_TIMESTAMP - INTERVAL '28 days', 3, CURRENT_TIMESTAMP - INTERVAL '28 days', NULL, 2, 21),
                                                                                                                                                                               (22, 'Rwanda Natural Fruity', $seed$Kawa: Rwanda Natural Fruity
Profil sensoryczny: Jagody, Porzeczka, Czerwone owoce

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '28 days', CURRENT_TIMESTAMP - INTERVAL '27 days', CURRENT_TIMESTAMP - INTERVAL '27 days', 3, CURRENT_TIMESTAMP - INTERVAL '27 days', NULL, 3, 22),
                                                                                                                                                                               (23, 'Panama Geisha Competition Lot', $seed$Kawa: Panama Geisha Competition Lot
Profil sensoryczny: Jaśmin, Cytrusy, Kwiaty, Brzoskwinia

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '27 days', CURRENT_TIMESTAMP - INTERVAL '26 days', CURRENT_TIMESTAMP - INTERVAL '26 days', 3, CURRENT_TIMESTAMP - INTERVAL '26 days', NULL, 4, 23),
                                                                                                                                                                               (24, 'Brazylia Pulped Natural', $seed$Kawa: Brazylia Pulped Natural
Profil sensoryczny: Czekolada, Orzechy, Miód

Opis demo do katalogu BrewBase. Kawa ma dane referencyjne, oceny, receptury oraz powiązanie z rankingiem. Nadaje się do testowania wyszukiwania, filtrowania, szczegółów kawy i rankingu.$seed$, 'Approved', 'coffee', CURRENT_TIMESTAMP - INTERVAL '26 days', CURRENT_TIMESTAMP - INTERVAL '25 days', CURRENT_TIMESTAMP - INTERVAL '25 days', 3, CURRENT_TIMESTAMP - INTERVAL '25 days', NULL, 5, 24),
                                                                                                                                                                               (101, 'Jak dobrać mielenie do V60', $seed$Poradnik opisujący korektę mielenia, czasu ekstrakcji oraz temperatury wody w metodzie V60.$seed$, 'Approved', 'brewing_method', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, CURRENT_TIMESTAMP, NULL, 3, NULL),
                                                                                                                                                                               (102, 'AeroPress inverted czy klasyczny', $seed$Porównanie dwóch sposobów parzenia AeroPressa oraz wpływu czasu kontaktu na słodycz i body.$seed$, 'Approved', 'brewing_method', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, CURRENT_TIMESTAMP, NULL, 4, NULL),
                                                                                                                                                                               (103, 'Kawy z Etiopii', $seed$Etiopia oferuje profile kwiatowe, herbaciane i jagodowe. Artykuł opisuje Guji, Sidamo oraz Yirgacheffe.$seed$, 'Approved', 'country', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, CURRENT_TIMESTAMP, NULL, 6, NULL),
                                                                                                                                                                               (104, 'Obróbka natural', $seed$Natural podbija słodycz, body i nuty owocowe, ale wymaga starannej kontroli fermentacji.$seed$, 'Approved', 'processing_method', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, CURRENT_TIMESTAMP, NULL, 8, NULL),
                                                                                                                                                                               (105, 'Jak czytać ranking kaw', $seed$Ranking kaw uwzględnia średnią ocen, liczbę ocen, użycia w recepturach i ulubione.$seed$, 'Approved', 'general', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 3, CURRENT_TIMESTAMP, NULL, 9, NULL),
                                                                                                                                                                               (106, 'Nowy artykuł do moderacji', $seed$Treść oczekująca na decyzję administratora. Zawiera wystarczający opis do testów panelu moderacji.$seed$, 'Pending', 'general', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, NULL, NULL, NULL, NULL, 10, NULL),
                                                                                                                                                                               (107, 'Niepełny wpis o palarni', $seed$Zbyt krótki opis używany do testowania odrzucenia i komentarza moderacyjnego.$seed$, 'Rejected', 'roastery', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, NULL, 3, CURRENT_TIMESTAMP, 'Za krótki opis.', 1, NULL);

INSERT INTO report (id, reason, created_at, article_id, reported_by_user_id) VALUES
                                                                                 (1, 'Zgłoszenie demo: opis wymaga sprawdzenia przez administratora.', CURRENT_TIMESTAMP, 106, 2),
                                                                                 (2, 'Zgłoszenie demo: podejrzenie niepełnych informacji w artykule.', CURRENT_TIMESTAMP, 107, 4);

INSERT INTO notification (id, user_id, content, created_at, is_read) VALUES
                                                                         (1, 1, 'Powiadomienie demo #1: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '1 hours', false),
                                                                         (2, 2, 'Powiadomienie demo #2: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '2 hours', false),
                                                                         (3, 3, 'Powiadomienie demo #3: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '3 hours', true),
                                                                         (4, 4, 'Powiadomienie demo #4: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '4 hours', false),
                                                                         (5, 5, 'Powiadomienie demo #5: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '5 hours', false),
                                                                         (6, 6, 'Powiadomienie demo #6: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '6 hours', true),
                                                                         (7, 7, 'Powiadomienie demo #7: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '7 hours', false),
                                                                         (8, 8, 'Powiadomienie demo #8: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '8 hours', false),
                                                                         (9, 9, 'Powiadomienie demo #9: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '9 hours', true),
                                                                         (10, 10, 'Powiadomienie demo #10: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '10 hours', false),
                                                                         (11, 1, 'Powiadomienie demo #11: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '11 hours', false),
                                                                         (12, 2, 'Powiadomienie demo #12: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '12 hours', true),
                                                                         (13, 3, 'Powiadomienie demo #13: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '13 hours', false),
                                                                         (14, 4, 'Powiadomienie demo #14: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '14 hours', false),
                                                                         (15, 5, 'Powiadomienie demo #15: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '15 hours', true),
                                                                         (16, 6, 'Powiadomienie demo #16: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '16 hours', false),
                                                                         (17, 7, 'Powiadomienie demo #17: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '17 hours', false),
                                                                         (18, 8, 'Powiadomienie demo #18: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '18 hours', true),
                                                                         (19, 9, 'Powiadomienie demo #19: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '19 hours', false),
                                                                         (20, 10, 'Powiadomienie demo #20: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '20 hours', false),
                                                                         (21, 1, 'Powiadomienie demo #21: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '21 hours', true),
                                                                         (22, 2, 'Powiadomienie demo #22: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '22 hours', false),
                                                                         (23, 3, 'Powiadomienie demo #23: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '23 hours', false),
                                                                         (24, 4, 'Powiadomienie demo #24: nowa aktywność w BrewBase.', CURRENT_TIMESTAMP - INTERVAL '24 hours', true);


DO $$
    BEGIN
        IF to_regprocedure('public.refresh_all_rankings()') IS NOT NULL THEN
            PERFORM refresh_all_rankings();
        ELSE
            IF to_regprocedure('public.refresh_coffee_ranking()') IS NOT NULL THEN
                PERFORM refresh_coffee_ranking();
            END IF;
            IF to_regprocedure('public.refresh_recipe_ranking()') IS NOT NULL THEN
                PERFORM refresh_recipe_ranking();
            END IF;
            IF to_regprocedure('public.refresh_user_ranking()') IS NOT NULL THEN
                PERFORM refresh_user_ranking();
            END IF;
        END IF;
    END $$;

-- Hard validation of seed consistency.
DO $$
    DECLARE
        invalid_count integer;
    BEGIN
        SELECT COUNT(*) INTO invalid_count
        FROM app_user
        WHERE email !~* '^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$'
           OR login IS NULL OR length(trim(login)) = 0 OR length(login) > 50
           OR role NOT IN ('User', 'Admin')
           OR password_hash IS NULL OR length(password_hash) = 0
           OR password_hint IS NULL OR length(password_hint) < 3 OR length(password_hint) > 255;
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid app_user rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count
        FROM recipe
        WHERE length(trim(title)) < 3 OR length(title) > 120
           OR length(trim(steps)) < 5 OR length(steps) > 5000
           OR parameters IS NULL OR jsonb_typeof(parameters) <> 'object'
           OR (is_public = true AND (coffee_id IS NULL OR coffee_id <= 0 OR brewing_method_id IS NULL OR brewing_method_id <= 0));
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid recipe base rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count
        FROM recipe
        WHERE is_public = true AND (
            NOT (parameters ? 'coffee')
                OR NOT (parameters ? 'water')
                OR NOT (parameters ? 'temperature')
                OR NOT (parameters ? 'brewTime')
                OR COALESCE(NULLIF(regexp_replace(parameters->>'coffee', '[^0-9.,]', '', 'g'), ''), '0')::numeric <= 0
                OR COALESCE(NULLIF(regexp_replace(parameters->>'coffee', '[^0-9.,]', '', 'g'), ''), '0')::numeric > 1000
                OR COALESCE(NULLIF(regexp_replace(parameters->>'water', '[^0-9.,]', '', 'g'), ''), '0')::numeric <= 0
                OR COALESCE(NULLIF(regexp_replace(parameters->>'water', '[^0-9.,]', '', 'g'), ''), '0')::numeric > 5000
                OR COALESCE(NULLIF(regexp_replace(parameters->>'temperature', '[^0-9.,]', '', 'g'), ''), '0')::numeric < 70
                OR COALESCE(NULLIF(regexp_replace(parameters->>'temperature', '[^0-9.,]', '', 'g'), ''), '0')::numeric > 100
                OR (parameters->>'brewTime') !~ '^\d+:[0-5][0-9]$'
                OR ((split_part(parameters->>'brewTime', ':', 1)::int * 60) + split_part(parameters->>'brewTime', ':', 2)::int) < 60
                OR ((split_part(parameters->>'brewTime', ':', 1)::int * 60) + split_part(parameters->>'brewTime', ':', 2)::int) > 540
            );
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid recipe parameter rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count FROM coffee_rating WHERE value NOT BETWEEN 1 AND 5;
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid coffee_rating rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count FROM recipe_rating WHERE value NOT BETWEEN 1 AND 5;
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid recipe_rating rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count
        FROM cupping_session
        WHERE name IS NULL OR length(trim(name)) = 0 OR length(name) > 255;
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid cupping_session rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count
        FROM cupping_session_coffee
        WHERE (notes IS NOT NULL AND length(notes) > 1000)
           OR (flavor_profile_notes IS NOT NULL AND length(flavor_profile_notes) > 1000)
           OR (aroma_score IS NOT NULL AND aroma_score NOT BETWEEN 1 AND 10)
           OR (sweetness_score IS NOT NULL AND sweetness_score NOT BETWEEN 1 AND 10)
           OR (acidity_score IS NOT NULL AND acidity_score NOT BETWEEN 1 AND 10)
           OR (body_score IS NOT NULL AND body_score NOT BETWEEN 1 AND 10)
           OR (overall_score IS NOT NULL AND overall_score NOT BETWEEN 1 AND 10);
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid cupping_session_coffee rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count
        FROM quick_note
        WHERE content IS NULL OR length(trim(content)) = 0 OR length(content) > 5000;
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid quick_note rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count
        FROM article
        WHERE title IS NULL OR length(trim(title)) = 0 OR length(title) > 255
           OR content IS NULL OR length(trim(content)) = 0
           OR module IS NULL OR length(trim(module)) = 0 OR length(module) > 50
           OR status NOT IN ('Pending', 'Approved', 'Rejected');
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Invalid article rows: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count
        FROM (
                 SELECT lower(trim(name)) normalized_name
                 FROM flavor_profile
                 GROUP BY lower(trim(name))
                 HAVING COUNT(*) > 1
             ) duplicates;
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Duplicate flavor_profile names: %', invalid_count; END IF;

        SELECT COUNT(*) INTO invalid_count
        FROM (
                 SELECT coffee_id
                 FROM article
                 WHERE module = 'coffee' AND status = 'Approved' AND coffee_id IS NOT NULL
                 GROUP BY coffee_id
                 HAVING COUNT(*) > 1
             ) duplicates;
        IF invalid_count > 0 THEN RAISE EXCEPTION 'Duplicate approved coffee wiki rows: %', invalid_count; END IF;
    END $$;

-- Reset identity sequences after explicit IDs.
SELECT setval(pg_get_serial_sequence('country', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM country), 1), true);
SELECT setval(pg_get_serial_sequence('region', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM region), 1), true);
SELECT setval(pg_get_serial_sequence('brewing_method', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM brewing_method), 1), true);
SELECT setval(pg_get_serial_sequence('processing_method', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM processing_method), 1), true);
SELECT setval(pg_get_serial_sequence('variety', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM variety), 1), true);
SELECT setval(pg_get_serial_sequence('roastery', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM roastery), 1), true);
SELECT setval(pg_get_serial_sequence('acidity', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM acidity), 1), true);
SELECT setval(pg_get_serial_sequence('body', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM body), 1), true);
SELECT setval(pg_get_serial_sequence('flavor_profile', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM flavor_profile), 1), true);
SELECT setval(pg_get_serial_sequence('app_user', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM app_user), 1), true);
SELECT setval(pg_get_serial_sequence('user_preference', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM user_preference), 1), true);
SELECT setval(pg_get_serial_sequence('coffee', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM coffee), 1), true);
SELECT setval(pg_get_serial_sequence('recipe', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM recipe), 1), true);
SELECT setval(pg_get_serial_sequence('coffee_rating', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM coffee_rating), 1), true);
SELECT setval(pg_get_serial_sequence('recipe_rating', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM recipe_rating), 1), true);
SELECT setval(pg_get_serial_sequence('quick_note', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM quick_note), 1), true);
SELECT setval(pg_get_serial_sequence('cupping_session', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM cupping_session), 1), true);
SELECT setval(pg_get_serial_sequence('cupping_session_coffee', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM cupping_session_coffee), 1), true);
SELECT setval(pg_get_serial_sequence('article', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM article), 1), true);
SELECT setval(pg_get_serial_sequence('report', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM report), 1), true);
SELECT setval(pg_get_serial_sequence('notification', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM notification), 1), true);
SELECT setval(pg_get_serial_sequence('coffee_ranking', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM coffee_ranking), 1), true);
SELECT setval(pg_get_serial_sequence('recipe_ranking', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM recipe_ranking), 1), true);
SELECT setval(pg_get_serial_sequence('user_ranking', 'id'), GREATEST((SELECT COALESCE(MAX(id), 1) FROM user_ranking), 1), true);

COMMIT;

SELECT
    (SELECT COUNT(*) FROM app_user) AS users,
    (SELECT COUNT(*) FROM coffee) AS coffees,
    (SELECT COUNT(*) FROM recipe) AS recipes,
    (SELECT COUNT(*) FROM coffee_rating) AS coffee_ratings,
    (SELECT COUNT(*) FROM recipe_rating) AS recipe_ratings,
    (SELECT COUNT(*) FROM article) AS articles,
    (SELECT COUNT(*) FROM cupping_session) AS cupping_sessions,
    (SELECT COUNT(*) FROM user_ranking) AS user_ranking_rows,
    (SELECT COUNT(*) FROM recipe_ranking) AS recipe_ranking_rows,
    (SELECT COUNT(*) FROM coffee_ranking) AS coffee_ranking_rows;
