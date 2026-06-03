CREATE UNIQUE INDEX IF NOT EXISTS uq_country_name_normalized
    ON country (LOWER(TRIM(name)));
