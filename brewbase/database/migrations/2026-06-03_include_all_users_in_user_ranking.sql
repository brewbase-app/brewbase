CREATE OR REPLACE FUNCTION refresh_user_ranking()
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
DROP TABLE IF EXISTS tmp_user_ranking;

CREATE TEMP TABLE tmp_user_ranking ON COMMIT DROP AS
WITH user_activity AS (
    SELECT
        u.id AS user_id,
        u.activity_points::integer AS activity_score,
        (
            SELECT COUNT(*)
            FROM recipe r
            WHERE r.user_id = u.id
              AND r.is_public = true
        )::integer AS public_recipe_count,
        (
            SELECT COUNT(*)
            FROM coffee_rating cr
            WHERE cr.user_id = u.id
        )::integer AS coffee_rating_count,
        (
            SELECT COUNT(*)
            FROM recipe_rating rr
            WHERE rr.user_id = u.id
        )::integer AS recipe_rating_count,
        (
            SELECT COUNT(*)
            FROM cupping_session cs
            WHERE cs.user_id = u.id
        )::integer AS cupping_session_count,
        (
            SELECT COUNT(*)
            FROM follow f
            WHERE f.followed_id = u.id
        )::integer AS followers_count,
        (
            SELECT COUNT(*)
            FROM recipe r
            JOIN user_recipe_favorite urf ON urf.recipe_id = r.id
            WHERE r.user_id = u.id
        )::integer AS received_recipe_favorite_count,
        (
            SELECT COUNT(*)
            FROM article a
            WHERE a.user_id = u.id
              AND UPPER(a.status) = 'APPROVED'
        )::integer AS published_article_count
    FROM app_user u
    WHERE COALESCE(u.is_blocked, false) = false
)
SELECT
    row_number() OVER (
        ORDER BY
            ua.activity_score DESC,
            ua.public_recipe_count DESC,
            ua.followers_count DESC,
            u.login ASC
    )::integer AS position,
    ua.user_id,
    ua.activity_score,
    ua.public_recipe_count,
    ua.coffee_rating_count,
    ua.recipe_rating_count,
    ua.cupping_session_count,
    ua.followers_count,
    ua.received_recipe_favorite_count,
    ua.published_article_count,
    CURRENT_TIMESTAMP::timestamp without time zone AS refreshed_at
FROM user_activity ua
    JOIN app_user u ON u.id = ua.user_id;

UPDATE user_ranking target
SET
    position = source.position,
    activity_score = source.activity_score,
    public_recipe_count = source.public_recipe_count,
    coffee_rating_count = source.coffee_rating_count,
    recipe_rating_count = source.recipe_rating_count,
    cupping_session_count = source.cupping_session_count,
    followers_count = source.followers_count,
    received_recipe_favorite_count = source.received_recipe_favorite_count,
    published_article_count = source.published_article_count,
    refreshed_at = source.refreshed_at
    FROM tmp_user_ranking source
WHERE target.user_id = source.user_id;

INSERT INTO user_ranking (
    refreshed_at,
    activity_score,
    user_id,
    position,
    public_recipe_count,
    coffee_rating_count,
    recipe_rating_count,
    cupping_session_count,
    followers_count,
    received_recipe_favorite_count,
    published_article_count
)
SELECT
    source.refreshed_at,
    source.activity_score,
    source.user_id,
    source.position,
    source.public_recipe_count,
    source.coffee_rating_count,
    source.recipe_rating_count,
    source.cupping_session_count,
    source.followers_count,
    source.received_recipe_favorite_count,
    source.published_article_count
FROM tmp_user_ranking source
WHERE NOT EXISTS (
    SELECT 1
    FROM user_ranking target
    WHERE target.user_id = source.user_id
);

UPDATE user_ranking target
SET
    position = 0,
    activity_score = 0,
    public_recipe_count = 0,
    coffee_rating_count = 0,
    recipe_rating_count = 0,
    cupping_session_count = 0,
    followers_count = 0,
    received_recipe_favorite_count = 0,
    published_article_count = 0,
    refreshed_at = CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1
    FROM tmp_user_ranking source
    WHERE source.user_id = target.user_id
);
END;
$$;

SELECT refresh_user_ranking();