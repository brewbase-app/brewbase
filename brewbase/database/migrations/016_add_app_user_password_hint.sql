ALTER TABLE app_user
    ADD COLUMN IF NOT EXISTS password_hint varchar(255) NULL;
