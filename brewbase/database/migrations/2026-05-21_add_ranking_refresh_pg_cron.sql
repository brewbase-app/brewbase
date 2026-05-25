CREATE EXTENSION IF NOT EXISTS pg_cron;

ALTER TABLE coffee_ranking
    ADD COLUMN IF NOT EXISTS position integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS average_rating double precision NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS ranking_score double precision NOT NULL DEFAULT 0;

ALTER TABLE recipe_ranking
    ADD COLUMN IF NOT EXISTS position integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS average_rating double precision NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS ranking_score double precision NOT NULL DEFAULT 0;

ALTER TABLE user_ranking
    ADD COLUMN IF NOT EXISTS position integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS public_recipe_count integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS coffee_rating_count integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS recipe_rating_count integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS quick_note_count integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS cupping_session_count integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS cupping_session_coffee_count integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS followers_count integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS received_recipe_favorite_count integer NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS published_article_count integer NOT NULL DEFAULT 0;

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
                COUNT(DISTINCT r.id) DESC,
                c.name ASC
        )::integer AS position,
        c.id AS coffee_id,
        AVG(cr.value)::double precision AS average_rating,
        COUNT(cr.id)::integer AS rating_count,
        COUNT(DISTINCT r.id)::integer AS recipe_used_count,
        0::integer AS like_count,
        (
            AVG(cr.value)::double precision * 100
            + COUNT(cr.id)::double precision * 2
            + COUNT(DISTINCT r.id)::double precision
        ) AS ranking_score,
        CURRENT_TIMESTAMP::timestamp without time zone AS refreshed_at
FROM coffee c
    JOIN coffee_rating cr ON cr.coffee_id = c.id
    LEFT JOIN recipe r ON r.coffee_id = c.id
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

CREATE OR REPLACE FUNCTION refresh_recipe_ranking()
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
DROP TABLE IF EXISTS tmp_recipe_ranking;

CREATE TEMP TABLE tmp_recipe_ranking ON COMMIT DROP AS
SELECT
    row_number() OVER (
            ORDER BY
                AVG(rr.value)::double precision DESC,
                COUNT(rr.id) DESC,
                COUNT(urf.recipe_id) DESC,
                r.title ASC
        )::integer AS position,
        r.id AS recipe_id,
        AVG(rr.value)::double precision AS average_rating,
        COUNT(rr.id)::integer AS rating_count,
        COUNT(urf.recipe_id)::integer AS save_count,
        COUNT(urf.recipe_id)::integer AS like_count,
        (
            AVG(rr.value)::double precision * 100
            + COUNT(rr.id)::double precision * 2
            + COUNT(urf.recipe_id)::double precision
        ) AS ranking_score,
        CURRENT_TIMESTAMP::timestamp without time zone AS refreshed_at
FROM recipe r
    JOIN recipe_rating rr ON rr.recipe_id = r.id
    LEFT JOIN user_recipe_favorite urf ON urf.recipe_id = r.id
WHERE r.is_public = true
GROUP BY r.id, r.title;

UPDATE recipe_ranking target
SET
    position = source.position,
    average_rating = source.average_rating,
    rating_count = source.rating_count,
    like_count = source.like_count,
    save_count = source.save_count,
    ranking_score = source.ranking_score,
    refreshed_at = source.refreshed_at
    FROM tmp_recipe_ranking source
WHERE target.recipe_id = source.recipe_id;

INSERT INTO recipe_ranking (
    refreshed_at,
    rating_count,
    like_count,
    save_count,
    recipe_id,
    position,
    average_rating,
    ranking_score
)
SELECT
    source.refreshed_at,
    source.rating_count,
    source.like_count,
    source.save_count,
    source.recipe_id,
    source.position,
    source.average_rating,
    source.ranking_score
FROM tmp_recipe_ranking source
WHERE NOT EXISTS (
    SELECT 1
    FROM recipe_ranking target
    WHERE target.recipe_id = source.recipe_id
);

UPDATE recipe_ranking target
SET
    position = 0,
    average_rating = 0,
    rating_count = 0,
    like_count = 0,
    save_count = 0,
    ranking_score = 0,
    refreshed_at = CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1
    FROM tmp_recipe_ranking source
    WHERE source.recipe_id = target.recipe_id
);
END;
$$;

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
                FROM quick_note qn
                WHERE qn.user_id = u.id
            )::integer AS quick_note_count,
            (
                SELECT COUNT(*)
                FROM cupping_session cs
                WHERE cs.user_id = u.id
            )::integer AS cupping_session_count,
            (
                SELECT COUNT(*)
                FROM cupping_session cs
                JOIN cupping_session_coffee csc ON csc.cupping_session_id = cs.id
                WHERE cs.user_id = u.id
            )::integer AS cupping_session_coffee_count,
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
    ),
    scored_users AS (
        SELECT
            user_id,
            public_recipe_count,
            coffee_rating_count,
            recipe_rating_count,
            quick_note_count,
            cupping_session_count,
            cupping_session_coffee_count,
            followers_count,
            received_recipe_favorite_count,
            published_article_count,
            (
                public_recipe_count * 10
                + coffee_rating_count * 3
                + recipe_rating_count * 3
                + quick_note_count * 2
                + cupping_session_count * 8
                + cupping_session_coffee_count * 2
                + followers_count * 5
                + received_recipe_favorite_count * 4
                + published_article_count * 12
            )::integer AS activity_score
        FROM user_activity
    )
SELECT
    row_number() OVER (
            ORDER BY
                su.activity_score DESC,
                su.public_recipe_count DESC,
                su.followers_count DESC,
                u.login ASC
        )::integer AS position,
        su.user_id,
        su.activity_score,
        su.public_recipe_count,
        su.coffee_rating_count,
        su.recipe_rating_count,
        su.quick_note_count,
        su.cupping_session_count,
        su.cupping_session_coffee_count,
        su.followers_count,
        su.received_recipe_favorite_count,
        su.published_article_count,
        CURRENT_TIMESTAMP::timestamp without time zone AS refreshed_at
FROM scored_users su
    JOIN app_user u ON u.id = su.user_id
WHERE su.activity_score > 0;

UPDATE user_ranking target
SET
    position = source.position,
    activity_score = source.activity_score,
    recipe_count = source.public_recipe_count,
    like_count = source.received_recipe_favorite_count,
    public_recipe_count = source.public_recipe_count,
    coffee_rating_count = source.coffee_rating_count,
    recipe_rating_count = source.recipe_rating_count,
    quick_note_count = source.quick_note_count,
    cupping_session_count = source.cupping_session_count,
    cupping_session_coffee_count = source.cupping_session_coffee_count,
    followers_count = source.followers_count,
    received_recipe_favorite_count = source.received_recipe_favorite_count,
    published_article_count = source.published_article_count,
    refreshed_at = source.refreshed_at
    FROM tmp_user_ranking source
WHERE target.user_id = source.user_id;

INSERT INTO user_ranking (
    refreshed_at,
    activity_score,
    recipe_count,
    like_count,
    user_id,
    position,
    public_recipe_count,
    coffee_rating_count,
    recipe_rating_count,
    quick_note_count,
    cupping_session_count,
    cupping_session_coffee_count,
    followers_count,
    received_recipe_favorite_count,
    published_article_count
)
SELECT
    source.refreshed_at,
    source.activity_score,
    source.public_recipe_count,
    source.received_recipe_favorite_count,
    source.user_id,
    source.position,
    source.public_recipe_count,
    source.coffee_rating_count,
    source.recipe_rating_count,
    source.quick_note_count,
    source.cupping_session_count,
    source.cupping_session_coffee_count,
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
    recipe_count = 0,
    like_count = 0,
    public_recipe_count = 0,
    coffee_rating_count = 0,
    recipe_rating_count = 0,
    quick_note_count = 0,
    cupping_session_count = 0,
    cupping_session_coffee_count = 0,
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

CREATE OR REPLACE FUNCTION refresh_all_rankings()
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
    PERFORM refresh_coffee_ranking();
    PERFORM refresh_recipe_ranking();
    PERFORM refresh_user_ranking();
END;
$$;

SELECT cron.unschedule(jobid)
FROM cron.job
WHERE jobname = 'refresh-rankings-every-hour';

SELECT cron.schedule(
               'refresh-rankings-every-hour',
               '0 * * * *',
               $$SELECT refresh_all_rankings();$$
);

SELECT refresh_all_rankings();