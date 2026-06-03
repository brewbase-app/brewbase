CREATE UNIQUE INDEX IF NOT EXISTS uq_region_country_name_normalized
    ON region (country_id, LOWER(TRIM(name)));
