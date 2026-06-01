ALTER TABLE recipe
    ADD COLUMN IF NOT EXISTS moderation_comment text NULL;
