BEGIN;

DELETE FROM user_preference_flavor_profile;
DELETE FROM flavor_profile;

DO $$
DECLARE
    sequence_name text;
BEGIN
    IF EXISTS (
        SELECT 1
        FROM pg_attribute attribute
        JOIN pg_class relation ON relation.oid = attribute.attrelid
        JOIN pg_namespace namespace ON namespace.oid = relation.relnamespace
        WHERE relation.relname = 'flavor_profile'
          AND namespace.nspname = current_schema()
          AND attribute.attname = 'id'
          AND attribute.attidentity IN ('a', 'd')
    ) THEN
        EXECUTE 'ALTER TABLE flavor_profile ALTER COLUMN id RESTART WITH 1';
    ELSE
        sequence_name := pg_get_serial_sequence('flavor_profile', 'id');

        IF sequence_name IS NOT NULL THEN
            EXECUTE format('ALTER SEQUENCE %s RESTART WITH 1', sequence_name);
        END IF;
    END IF;
END $$;

INSERT INTO flavor_profile (name)
SELECT seed.name
FROM (
    VALUES
        ('Jaśmin'),
        ('Cytrusy'),
        ('Bergamotka'),
        ('Miód'),
        ('Czekolada'),
        ('Karmel'),
        ('Orzechy'),
        ('Czerwone owoce'),
        ('Jagody'),
        ('Winne'),
        ('Kwiaty'),
        ('Herbaciane'),
        ('Przyprawy'),
        ('Tropikalne owoce'),
        ('Porzeczka'),
        ('Brzoskwinia'),
        ('Jabłko')
) AS seed(name)
WHERE NOT EXISTS (
    SELECT 1
    FROM flavor_profile existing
    WHERE LOWER(TRIM(existing.name)) = LOWER(TRIM(seed.name))
);

COMMIT;