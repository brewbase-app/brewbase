-- COUNTRY
INSERT INTO country (id, name) VALUES
                                   (1, 'Etiopia'),
                                   (2, 'Kolumbia');

-- REGION
INSERT INTO region (id, name, country_id) VALUES
                                              (1, 'Guji', 1),
                                              (2, 'Sidamo', 1),
                                              (3, 'Huila', 2);

-- BREWING METHOD
INSERT INTO brewing_method (id, name, description) VALUES
                                                       (1, 'V60', 'Metoda przelewowa'),
                                                       (2, 'AeroPress', 'Metoda ciśnieniowa'),
                                                       (3, 'Chemex', 'Czysty profil smaku');

-- PROCESSING METHOD
INSERT INTO processing_method (id, name) VALUES
                                             (1, 'Washed'),
                                             (2, 'Natural'),
                                             (3, 'Honey');

-- VARIETY
INSERT INTO variety (id, name) VALUES
                                   (1, 'Heirloom'),
                                   (2, 'Bourbon'),
                                   (3, 'Caturra');

-- ROASTERY
INSERT INTO roastery (id, name) VALUES
                                    (1, 'CoffeeLab'),
                                    (2, 'Hard Beans'),
                                    (3, 'Audun Coffee');

-- USERS
INSERT INTO app_user (
    id, email, password_hash, login, role,
    activity_points, label, is_blocked, created_at
) VALUES
      (1, 'user@brewbase.pl', 'hashed', 'kawosz', 'USER', 0, NULL, false, CURRENT_TIMESTAMP),
      (2, 'admin@brewbase.pl', 'hashed', 'admin', 'ADMIN', 0, NULL, false, CURRENT_TIMESTAMP);

-- COFFEE (FIXED COLUMN NAME)
INSERT INTO coffee (
    id, name, roastery_id, region_id,
    variety_id, processing_method_id,
    created_by_user_id, is_verified
) VALUES
      (1, 'Etiopia Guji Natural', 1, 1, 1, 2, 1, true),
      (2, 'Kolumbia Huila Washed', 2, 3, 3, 1, 1, true);

-- RECIPE (FIXED COLUMN NAME)
INSERT INTO recipe (
    id, title, parameters, steps,
    is_public, user_id,
    brewing_method_id, coffee_id
) VALUES
      (
          1,
          'V60 – klasyczny balans',
          '{"coffee_grams": 20, "water_ml": 300, "temperature": 94}',
          '1. Bloom 30s\n2. Powolne zalewanie\n3. Całość ~2:30',
          true,
          1,
          1,
          1
      ),
      (
          2,
          'AeroPress – szybki i słodki',
          '{"coffee_grams": 18, "water_ml": 220, "temperature": 92}',
          '1. Zalewanie\n2. Mieszanie\n3. Przeciskanie',
          true,
          1,
          2,
          2
      );

SELECT setval(pg_get_serial_sequence('country', 'id'), (SELECT MAX(id) FROM country));
SELECT setval(pg_get_serial_sequence('region', 'id'), (SELECT MAX(id) FROM region));
SELECT setval(pg_get_serial_sequence('brewing_method', 'id'), (SELECT MAX(id) FROM brewing_method));
SELECT setval(pg_get_serial_sequence('processing_method', 'id'), (SELECT MAX(id) FROM processing_method));
SELECT setval(pg_get_serial_sequence('variety', 'id'), (SELECT MAX(id) FROM variety));
SELECT setval(pg_get_serial_sequence('roastery', 'id'), (SELECT MAX(id) FROM roastery));
SELECT setval(pg_get_serial_sequence('app_user', 'id'), (SELECT MAX(id) FROM app_user));
SELECT setval(pg_get_serial_sequence('coffee', 'id'), (SELECT MAX(id) FROM coffee));
SELECT setval(pg_get_serial_sequence('recipe', 'id'), (SELECT MAX(id) FROM recipe));