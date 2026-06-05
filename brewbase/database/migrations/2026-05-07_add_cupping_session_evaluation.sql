ALTER TABLE cupping_session
    ADD COLUMN session_date timestamp NULL;

ALTER TABLE cupping_session_coffee
    ADD COLUMN aroma_score int NULL,
    ADD COLUMN sweetness_score int NULL,
    ADD COLUMN acidity_score int NULL,
    ADD COLUMN body_score int NULL,
    ADD COLUMN flavor_profile_notes text NULL,
    ADD COLUMN overall_score int NULL;

ALTER TABLE cupping_session_coffee
    ADD CONSTRAINT chk_cupping_session_coffee_aroma_score
        CHECK (aroma_score IS NULL OR aroma_score BETWEEN 1 AND 10);

ALTER TABLE cupping_session_coffee
    ADD CONSTRAINT chk_cupping_session_coffee_sweetness_score
        CHECK (sweetness_score IS NULL OR sweetness_score BETWEEN 1 AND 10);

ALTER TABLE cupping_session_coffee
    ADD CONSTRAINT chk_cupping_session_coffee_acidity_score
        CHECK (acidity_score IS NULL OR acidity_score BETWEEN 1 AND 10);

ALTER TABLE cupping_session_coffee
    ADD CONSTRAINT chk_cupping_session_coffee_body_score
        CHECK (body_score IS NULL OR body_score BETWEEN 1 AND 10);

ALTER TABLE cupping_session_coffee
    ADD CONSTRAINT chk_cupping_session_coffee_overall_score
        CHECK (overall_score IS NULL OR overall_score BETWEEN 1 AND 10);