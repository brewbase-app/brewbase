-- Global search: PostgreSQL extensions, immutable text wrapper, trigram indexes.
-- Requires: base schema (schema.sql) and prior migrations applied.

CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Some hosts install unaccent outside public; indexes need stable resolution.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM pg_extension e
        JOIN pg_namespace n ON n.oid = e.extnamespace
        WHERE e.extname = 'unaccent'
          AND n.nspname <> 'public'
    ) THEN
        ALTER EXTENSION unaccent SET SCHEMA public;
    END IF;
END $$;

-- unaccent(text) is STABLE, not IMMUTABLE — wrapper is safe for expression indexes.
CREATE OR REPLACE FUNCTION brewbase_search_text(input text)
RETURNS text
LANGUAGE sql
IMMUTABLE
PARALLEL SAFE
STRICT
SET search_path TO public, extensions, pg_catalog
AS $$
    SELECT unaccent('unaccent'::regdictionary, lower(input))
$$;

CREATE INDEX IF NOT EXISTS idx_coffee_name_search
    ON coffee USING gin (brewbase_search_text(name) gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_recipe_title_search
    ON recipe USING gin (brewbase_search_text(title) gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_article_title_search
    ON article USING gin (brewbase_search_text(title) gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_article_content_search
    ON article USING gin (brewbase_search_text(content) gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_app_user_login_search
    ON app_user USING gin (brewbase_search_text(login) gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_app_user_label_search
    ON app_user USING gin (brewbase_search_text(COALESCE(label, '')) gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_quick_note_content_search
    ON quick_note USING gin (brewbase_search_text(content) gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_cupping_session_name_search
    ON cupping_session USING gin (brewbase_search_text(name) gin_trgm_ops);
