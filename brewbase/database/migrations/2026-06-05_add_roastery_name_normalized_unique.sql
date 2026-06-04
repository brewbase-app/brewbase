CREATE UNIQUE INDEX IF NOT EXISTS uq_roastery_name_normalized
    ON roastery (LOWER(TRIM(name)));
