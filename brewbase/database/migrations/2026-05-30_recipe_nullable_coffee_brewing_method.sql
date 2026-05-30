-- Allow draft recipes without coffee or brewing method selected yet.
ALTER TABLE recipe ALTER COLUMN coffee_id DROP NOT NULL;
ALTER TABLE recipe ALTER COLUMN brewing_method_id DROP NOT NULL;

SELECT refresh_all_rankings();