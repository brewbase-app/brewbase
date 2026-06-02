ALTER TABLE user_preference
    ADD COLUMN experience_level varchar(50) NULL;

ALTER TABLE user_preference
    ADD COLUMN preferred_acidity varchar(50) NULL;

ALTER TABLE user_preference
    ADD COLUMN preferred_body varchar(50) NULL;

ALTER TABLE user_preference
    ADD COLUMN recommendation_style varchar(100) NULL;

ALTER TABLE user_preference
    ADD COLUMN allow_exploration boolean NOT NULL DEFAULT false;