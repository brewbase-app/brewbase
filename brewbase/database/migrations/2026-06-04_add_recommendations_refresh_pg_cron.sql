CREATE OR REPLACE FUNCTION refresh_recommendations()
RETURNS void
LANGUAGE plpgsql
AS $$
BEGIN
DELETE FROM recommendations
WHERE algorithm = 'cron-recommendation-v1';

INSERT INTO recommendations (
    feedback,
    score,
    algorithm,
    generated_at,
    source,
    coffee_id,
    recipe_id,
    user_id,
    user_preference_id,
    coffee_ranking_id
)
SELECT
    false AS feedback,
    recommendation_data.score,
    'cron-recommendation-v1' AS algorithm,
    CURRENT_TIMESTAMP::timestamp without time zone AS generated_at,
        recommendation_data.source,
        recommendation_data.coffee_id,
        recommendation_data.recipe_id,
        recommendation_data.user_id,
        recommendation_data.user_preference_id,
        recommendation_data.coffee_ranking_id
FROM (
    SELECT
    u.id AS user_id,
    up.id AS user_preference_id,
    cr.coffee_id,
    NULL::int AS recipe_id,
    cr.id AS coffee_ranking_id,
    (
    cr.ranking_score
    + CASE
    WHEN upr.region_id IS NOT NULL THEN 30
    ELSE 0
    END
    )::int AS score,
    'coffee' AS source,
    row_number() OVER (
    PARTITION BY u.id
    ORDER BY
    (
    cr.ranking_score
    + CASE
    WHEN upr.region_id IS NOT NULL THEN 30
    ELSE 0
    END
    ) DESC,
    cr.position ASC,
    c.name ASC
    ) AS recommendation_position
    FROM app_user u
    LEFT JOIN user_preference up
    ON up.user_id = u.id
    AND up.quiz_completed = true
    JOIN coffee_ranking cr
    ON cr.position > 0
    JOIN coffee c
    ON c.id = cr.coffee_id
    LEFT JOIN user_preference_region upr
    ON upr.user_preference_id = up.id
    AND upr.region_id = c.region_id
    WHERE COALESCE(u.is_blocked, false) = false

    UNION ALL

    SELECT
    u.id AS user_id,
    up.id AS user_preference_id,
    NULL::int AS coffee_id,
    rr.recipe_id,
    NULL::int AS coffee_ranking_id,
    (
    rr.ranking_score
    + CASE
    WHEN upbm.brewing_method_id IS NOT NULL THEN 30
    ELSE 0
    END
    + CASE
    WHEN upr.region_id IS NOT NULL THEN 20
    ELSE 0
    END
    )::int AS score,
    'recipe' AS source,
    row_number() OVER (
    PARTITION BY u.id
    ORDER BY
    (
    rr.ranking_score
    + CASE
    WHEN upbm.brewing_method_id IS NOT NULL THEN 30
    ELSE 0
    END
    + CASE
    WHEN upr.region_id IS NOT NULL THEN 20
    ELSE 0
    END
    ) DESC,
    rr.position ASC,
    r.title ASC
    ) AS recommendation_position
    FROM app_user u
    LEFT JOIN user_preference up
    ON up.user_id = u.id
    AND up.quiz_completed = true
    JOIN recipe_ranking rr
    ON rr.position > 0
    JOIN recipe r
    ON r.id = rr.recipe_id
    AND r.is_public = true
    LEFT JOIN coffee c
    ON c.id = r.coffee_id
    LEFT JOIN user_preference_brewing_method upbm
    ON upbm.user_preference_id = up.id
    AND upbm.brewing_method_id = r.brewing_method_id
    LEFT JOIN user_preference_region upr
    ON upr.user_preference_id = up.id
    AND c.id IS NOT NULL
    AND upr.region_id = c.region_id
    WHERE COALESCE(u.is_blocked, false) = false
    ) recommendation_data
WHERE recommendation_data.recommendation_position <= 10;
END;
$$;

DO $do$
    DECLARE
        existing_job_id integer;
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM pg_extension
            WHERE extname = 'pg_cron'
        ) THEN
            SELECT jobid
            INTO existing_job_id
            FROM cron.job
            WHERE jobname = 'refresh-recommendations-every-48h'
            LIMIT 1;

            IF existing_job_id IS NOT NULL THEN
                PERFORM cron.unschedule(existing_job_id);
            END IF;

            PERFORM cron.schedule(
                    'refresh-recommendations-every-48h',
                    '0 4 */2 * *',
                    'SELECT refresh_recommendations();'
                    );
        END IF;
    END;
$do$;