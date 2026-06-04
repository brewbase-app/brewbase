ALTER TABLE cupping_session_coffee
DROP COLUMN IF EXISTS clean_cup;

ALTER TABLE recipe_ranking
DROP COLUMN IF EXISTS like_count;

ALTER TABLE user_ranking
DROP COLUMN IF EXISTS recipe_count,
DROP COLUMN IF EXISTS like_count,
DROP COLUMN IF EXISTS quick_note_count,
DROP COLUMN IF EXISTS cupping_session_coffee_count;