-- Seed początkowy BrewBase (katalog, użytkownicy, przepisy)
-- Hasło testowe dla wszystkich kont: Test123!
-- Po tym pliku uruchom seed_wiki.sql (idempotentny), aby uzupełnić brakujące artykuły wiki.

-- COUNTRY
INSERT INTO country (id, name) VALUES
                                   (1, 'Etiopia'),
                                   (2, 'Kolumbia'),
                                   (3, 'Kenia');

-- REGION
INSERT INTO region (id, name, country_id) VALUES
                                              (1, 'Guji', 1),
                                              (2, 'Sidamo', 1),
                                              (3, 'Huila', 2),
                                              (4, 'Nyeri', 3);

-- BREWING METHOD
INSERT INTO brewing_method (id, name, description) VALUES
                                                       (1, 'V60', 'Metoda przelewowa Hario V60'),
                                                       (2, 'AeroPress', 'Metoda ciśnieniowa AeroPress'),
                                                       (3, 'Chemex', 'Czysty, klarowny profil smaku'),
                                                       (4, 'French Press', 'Pełne ciało i intensywność');

-- PROCESSING METHOD
INSERT INTO processing_method (id, name) VALUES
                                             (1, 'Washed'),
                                             (2, 'Natural'),
                                             (3, 'Honey');

-- VARIETY
INSERT INTO variety (id, name) VALUES
                                   (1, 'Heirloom'),
                                   (2, 'Bourbon'),
                                   (3, 'Caturra'),
                                   (4, 'SL28');

-- ROASTERY
INSERT INTO roastery (id, name) VALUES
                                    (1, 'CoffeeLab'),
                                    (2, 'Hard Beans'),
                                    (3, 'Audun Coffee'),
                                    (4, 'Java Coffee'),
                                    (5, 'Coffee Collective');

-- USERS (hasło dla wszystkich kont: Test123!)
INSERT INTO app_user (
    id, email, password_hash, login, role,
    activity_points, label, is_blocked, created_at
) VALUES
      (1, 'user@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'kawosz', 'USER', 0, NULL, false, CURRENT_TIMESTAMP),
      (2, 'maja@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'maja', 'USER', 0, NULL, false, CURRENT_TIMESTAMP),
      (3, 'admin@brewbase.pl', '$2a$11$kBKPLNa4Oin6iMjt7inZKOSmNvMInjggIlmPRz69CwPdJPq6THaSW', 'admin', 'ADMIN', 0, NULL, false, CURRENT_TIMESTAMP);

-- COFFEE
INSERT INTO coffee (
    id, name, roastery_id, region_id,
    variety_id, processing_method_id,
    created_by_user_id, is_verified
) VALUES
      (1, 'Etiopia Guji Natural', 1, 1, 1, 2, 1, true),
      (2, 'Kolumbia Huila Washed', 2, 3, 3, 1, 1, true),
      (3, 'Kenia Nyeri SL28', 4, 4, 4, 1, 2, true),
      (4, 'Etiopia Sidamo Honey', 3, 2, 1, 3, 2, true),
      (5, 'Kolumbia Bourbon Natural', 2, 3, 2, 2, 1, true);

-- RECIPE
INSERT INTO recipe (
    id, title, parameters, steps,
    is_public, user_id,
    brewing_method_id, coffee_id,
    created_at
) VALUES
      (
          1,
          'V60 – klasyczny balans',
          '{"coffee":"20g","water":"300ml","temperature":"94°C","grindSize":"średnie","brewTime":"2:30"}',
          '1. Bloom 30s z 60 ml wody\n2. Powolne zalewanie spiralne\n3. Całość ok. 2:30',
          true,
          1,
          1,
          1,
          CURRENT_TIMESTAMP - INTERVAL '12 days'
      ),
      (
          2,
          'AeroPress – szybki i słodki',
          '{"coffee":"18g","water":"220ml","temperature":"92°C","grindSize":"drobniejsze","brewTime":"1:45"}',
          '1. Zalewanie 92°C\n2. Delikatne mieszanie 10 s\n3. Przeciskanie 30 s',
          true,
          1,
          2,
          2,
          CURRENT_TIMESTAMP - INTERVAL '9 days'
      ),
      (
          3,
          'Chemex – czysta filiżanka',
          '{"coffee":"22g","water":"350ml","temperature":"96°C","grindSize":"grubsze","brewTime":"4:00"}',
          '1. Przepłukanie filtra\n2. Bloom 45 s\n3. Powolne dolewanie wody do 350 ml',
          true,
          2,
          3,
          3,
          CURRENT_TIMESTAMP - INTERVAL '6 days'
      ),
      (
          4,
          'French Press – pełne ciało',
          '{"coffee":"25g","water":"400ml","temperature":"93°C","grindSize":"grube","brewTime":"4:00"}',
          '1. Zalewanie i mieszanie\n2. 4 minuty parzenia\n3. Powolne przeciskanie tłoka',
          true,
          2,
          4,
          4,
          CURRENT_TIMESTAMP - INTERVAL '4 days'
      ),
      (
          5,
          'V60 – jasna Kenia',
          '{"coffee":"15g","water":"250ml","temperature":"95°C","grindSize":"średnie","brewTime":"2:45"}',
          '1. Bloom 40 s\n2. Dolewanie partiami\n3. Krótki czas ekstrakcji',
          true,
          2,
          1,
          3,
          CURRENT_TIMESTAMP - INTERVAL '2 days'
      ),
      (
          6,
          'Robocza – test profilu',
          '{"coffee":"","water":"","temperature":"","grindSize":"","brewTime":"0:0"}',
          '',
          false,
          1,
          NULL,
          NULL,
          CURRENT_TIMESTAMP - INTERVAL '1 day'
      );

-- COFFEE RATINGS
INSERT INTO coffee_rating (id, value, created_at, updated_at, user_id, coffee_id) VALUES
    (1, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 1),
    (2, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 1),
    (3, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 2),
    (4, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 3),
    (5, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 3),
    (6, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 4),
    (7, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 5);

-- RECIPE RATINGS
INSERT INTO recipe_rating (id, value, created_at, updated_at, user_id, recipe_id) VALUES
    (1, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 1),
    (2, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 2),
    (3, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 3),
    (4, 4, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 4),
    (5, 5, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 2, 5),
    (6, 3, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 5);

-- RECIPE FAVORITES
INSERT INTO user_recipe_favorite (user_id, recipe_id, created_at) VALUES
    (1, 3, CURRENT_TIMESTAMP),
    (1, 5, CURRENT_TIMESTAMP),
    (2, 1, CURRENT_TIMESTAMP),
    (2, 2, CURRENT_TIMESTAMP);

-- COFFEE FAVORITES
INSERT INTO user_coffee_favorite (user_id, coffee_id, created_at) VALUES
    (1, 1, CURRENT_TIMESTAMP),
    (1, 3, CURRENT_TIMESTAMP),
    (2, 2, CURRENT_TIMESTAMP),
    (2, 4, CURRENT_TIMESTAMP);

SELECT setval(pg_get_serial_sequence('country', 'id'), (SELECT MAX(id) FROM country));
SELECT setval(pg_get_serial_sequence('region', 'id'), (SELECT MAX(id) FROM region));
SELECT setval(pg_get_serial_sequence('brewing_method', 'id'), (SELECT MAX(id) FROM brewing_method));
SELECT setval(pg_get_serial_sequence('processing_method', 'id'), (SELECT MAX(id) FROM processing_method));
SELECT setval(pg_get_serial_sequence('variety', 'id'), (SELECT MAX(id) FROM variety));
SELECT setval(pg_get_serial_sequence('roastery', 'id'), (SELECT MAX(id) FROM roastery));
SELECT setval(pg_get_serial_sequence('app_user', 'id'), (SELECT MAX(id) FROM app_user));
SELECT setval(pg_get_serial_sequence('coffee', 'id'), (SELECT MAX(id) FROM coffee));
SELECT setval(pg_get_serial_sequence('recipe', 'id'), (SELECT MAX(id) FROM recipe));
SELECT setval(pg_get_serial_sequence('coffee_rating', 'id'), (SELECT MAX(id) FROM coffee_rating));
SELECT setval(pg_get_serial_sequence('recipe_rating', 'id'), (SELECT MAX(id) FROM recipe_rating));
