CREATE OR REPLACE FUNCTION refresh_coffee_ranking()
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
DROP TABLE IF EXISTS tmp_coffee_ranking;

CREATE TEMP TABLE tmp_coffee_ranking ON COMMIT DROP AS
SELECT
    row_number() OVER (
            ORDER BY
                AVG(cr.value)::double precision DESC,
                COUNT(cr.id) DESC,
                COUNT(DISTINCT ucf.user_id) DESC,
                COUNT(DISTINCT r.id) DESC,
                c.name ASC
        )::integer AS position,
        c.id AS coffee_id,
        AVG(cr.value)::double precision AS average_rating,
        COUNT(cr.id)::integer AS rating_count,
        COUNT(DISTINCT r.id)::integer AS recipe_used_count,
        COUNT(DISTINCT ucf.user_id)::integer AS like_count,
        (
            AVG(cr.value)::double precision * 100
            + COUNT(cr.id)::double precision * 2
            + COUNT(DISTINCT r.id)::double precision
            + COUNT(DISTINCT ucf.user_id)::double precision
        ) AS ranking_score,
        CURRENT_TIMESTAMP::timestamp without time zone AS refreshed_at
FROM coffee c
    JOIN coffee_rating cr ON cr.coffee_id = c.id
    LEFT JOIN recipe r ON r.coffee_id = c.id
    LEFT JOIN user_coffee_favorite ucf ON ucf.coffee_id = c.id
GROUP BY c.id, c.name;

UPDATE coffee_ranking target
SET
    position = source.position,
    average_rating = source.average_rating,
    rating_count = source.rating_count,
    recipe_used_count = source.recipe_used_count,
    like_count = source.like_count,
    ranking_score = source.ranking_score,
    refreshed_at = source.refreshed_at
    FROM tmp_coffee_ranking source
WHERE target.coffee_id = source.coffee_id;

INSERT INTO coffee_ranking (
    refreshed_at,
    rating_count,
    recipe_used_count,
    like_count,
    coffee_id,
    position,
    average_rating,
    ranking_score
)
SELECT
    source.refreshed_at,
    source.rating_count,
    source.recipe_used_count,
    source.like_count,
    source.coffee_id,
    source.position,
    source.average_rating,
    source.ranking_score
FROM tmp_coffee_ranking source
WHERE NOT EXISTS (
    SELECT 1
    FROM coffee_ranking target
    WHERE target.coffee_id = source.coffee_id
);

UPDATE coffee_ranking target
SET
    position = 0,
    average_rating = 0,
    rating_count = 0,
    recipe_used_count = 0,
    like_count = 0,
    ranking_score = 0,
    refreshed_at = CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1
    FROM tmp_coffee_ranking source
    WHERE source.coffee_id = target.coffee_id
);
END;
$$;
