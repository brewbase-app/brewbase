ALTER TABLE recommendations
    ALTER COLUMN feedback DROP NOT NULL;

ALTER TABLE recommendations
    ADD COLUMN IF NOT EXISTS match_score double precision NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS popularity_score double precision NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS final_score double precision NOT NULL DEFAULT 0;

CREATE OR REPLACE FUNCTION refresh_recipe_ranking()
    RETURNS void
    LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM recipe_ranking;

    INSERT INTO recipe_ranking (
        refreshed_at,
        rating_count,
        save_count,
        recipe_id,
        position,
        average_rating,
        ranking_score
    )
    SELECT
        now(),
        rating_count,
        save_count,
        recipe_id,
        ROW_NUMBER() OVER (
            ORDER BY ranking_score DESC, average_rating DESC, rating_count DESC, recipe_id ASC
            ),
        average_rating,
        ranking_score
    FROM (
             SELECT
                 r.id AS recipe_id,
                 COALESCE(AVG(rr.value), 0) AS average_rating,
                 COUNT(DISTINCT rr.id)::int AS rating_count,
                 COUNT(DISTINCT urf.user_id)::int AS save_count,
                 (
                     COALESCE(AVG(rr.value), 0) * 10
                         + COUNT(DISTINCT rr.id) * 2
                         + COUNT(DISTINCT urf.user_id)
                     ) AS ranking_score
             FROM recipe r
                      LEFT JOIN recipe_rating rr ON rr.recipe_id = r.id
                      LEFT JOIN user_recipe_favorite urf ON urf.recipe_id = r.id
             WHERE r.is_public = true
             GROUP BY r.id
         ) ranked_recipes;
END;
$$;

CREATE OR REPLACE FUNCTION refresh_user_ranking()
    RETURNS void
    LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM user_ranking;

    INSERT INTO user_ranking (
        position,
        activity_score,
        user_id,
        public_recipe_count,
        coffee_rating_count,
        recipe_rating_count,
        cupping_session_count,
        followers_count,
        received_recipe_favorite_count,
        published_article_count,
        refreshed_at
    )
    SELECT
                ROW_NUMBER() OVER (
            ORDER BY
                activity_score DESC,
                public_recipe_count DESC,
                coffee_rating_count DESC,
                recipe_rating_count DESC,
                user_id ASC
            )::int AS position,
                activity_score::int,
                user_id,
                public_recipe_count::int,
                coffee_rating_count::int,
                recipe_rating_count::int,
                cupping_session_count::int,
                followers_count::int,
                received_recipe_favorite_count::int,
                published_article_count::int,
                now()
    FROM (
             SELECT
                 u.id AS user_id,
                 COALESCE(recipes.public_recipe_count, 0) AS public_recipe_count,
                 COALESCE(coffee_ratings.coffee_rating_count, 0) AS coffee_rating_count,
                 COALESCE(recipe_ratings.recipe_rating_count, 0) AS recipe_rating_count,
                 COALESCE(cupping_sessions.cupping_session_count, 0) AS cupping_session_count,
                 COALESCE(followers.followers_count, 0) AS followers_count,
                 COALESCE(favorites.received_recipe_favorite_count, 0) AS received_recipe_favorite_count,
                 COALESCE(articles.published_article_count, 0) AS published_article_count,
                 (
                     COALESCE(recipes.public_recipe_count, 0) * 10
                         + COALESCE(coffee_ratings.coffee_rating_count, 0) * 3
                         + COALESCE(recipe_ratings.recipe_rating_count, 0) * 3
                         + COALESCE(cupping_sessions.cupping_session_count, 0) * 5
                         + COALESCE(followers.followers_count, 0) * 4
                         + COALESCE(favorites.received_recipe_favorite_count, 0) * 2
                         + COALESCE(articles.published_article_count, 0) * 6
                     ) AS activity_score
             FROM app_user u
                      LEFT JOIN (
                 SELECT user_id, COUNT(*) AS public_recipe_count
                 FROM recipe
                 WHERE is_public = true
                 GROUP BY user_id
             ) recipes ON recipes.user_id = u.id
                      LEFT JOIN (
                 SELECT user_id, COUNT(*) AS coffee_rating_count
                 FROM coffee_rating
                 GROUP BY user_id
             ) coffee_ratings ON coffee_ratings.user_id = u.id
                      LEFT JOIN (
                 SELECT user_id, COUNT(*) AS recipe_rating_count
                 FROM recipe_rating
                 GROUP BY user_id
             ) recipe_ratings ON recipe_ratings.user_id = u.id
                      LEFT JOIN (
                 SELECT user_id, COUNT(*) AS cupping_session_count
                 FROM cupping_session
                 GROUP BY user_id
             ) cupping_sessions ON cupping_sessions.user_id = u.id
                      LEFT JOIN (
                 SELECT followed_id AS user_id, COUNT(*) AS followers_count
                 FROM follow
                 GROUP BY followed_id
             ) followers ON followers.user_id = u.id
                      LEFT JOIN (
                 SELECT r.user_id, COUNT(*) AS received_recipe_favorite_count
                 FROM user_recipe_favorite urf
                          JOIN recipe r ON r.id = urf.recipe_id
                 GROUP BY r.user_id
             ) favorites ON favorites.user_id = u.id
                      LEFT JOIN (
                 SELECT user_id, COUNT(*) AS published_article_count
                 FROM article
                 WHERE status = 'Approved'
                 GROUP BY user_id
             ) articles ON articles.user_id = u.id
             WHERE COALESCE(u.is_blocked, false) = false
         ) users_activity;
END;
$$;

CREATE OR REPLACE FUNCTION refresh_recommendations_for_user(target_user_id integer)
    RETURNS void
    LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM recommendations
    WHERE algorithm = 'cron-recommendation-v1'
      AND user_id = target_user_id;

    INSERT INTO recommendations (
        feedback,
        score,
        match_score,
        popularity_score,
        final_score,
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
        NULL AS feedback,
        ROUND(recommendation_data.final_score)::int AS score,
        recommendation_data.match_score,
        recommendation_data.popularity_score,
        recommendation_data.final_score,
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
                 coffee_data.user_id,
                 coffee_data.user_preference_id,
                 coffee_data.coffee_id,
                 NULL::int AS recipe_id,
                 coffee_data.coffee_ranking_id,
                 coffee_data.match_score,
                 coffee_data.popularity_score,
                 coffee_data.match_score * 10 + LEAST(coffee_data.popularity_score, 100) * 0.1 AS final_score,
                 'coffee' AS source,
                 row_number() OVER (
                     PARTITION BY coffee_data.user_id
                     ORDER BY
                         coffee_data.match_score * 10 + LEAST(coffee_data.popularity_score, 100) * 0.1 DESC,
                         coffee_data.match_score DESC,
                         coffee_data.popularity_score DESC,
                         coffee_data.position ASC,
                         coffee_data.coffee_name ASC
                     ) AS recommendation_position
             FROM (
                      SELECT
                          u.id AS user_id,
                          up.id AS user_preference_id,
                          up.recommendation_style,
                          up.allow_exploration,
                          cr.coffee_id,
                          cr.id AS coffee_ranking_id,
                          cr.position,
                          c.name AS coffee_name,
                          cr.ranking_score AS popularity_score,
                          (
                              CASE
                                  WHEN EXISTS (
                                      SELECT 1
                                      FROM user_preference_region upr
                                      WHERE upr.user_preference_id = up.id
                                        AND upr.region_id = c.region_id
                                  ) THEN 20
                                  ELSE 0
                                  END
                                  +
                              CASE
                                  WHEN up.preferred_acidity IS NOT NULL
                                      AND a.name IS NOT NULL
                                      AND lower(a.name) = lower(up.preferred_acidity) THEN 15
                                  ELSE 0
                                  END
                                  +
                              CASE
                                  WHEN up.preferred_body IS NOT NULL
                                      AND b.name IS NOT NULL
                                      AND lower(b.name) = lower(up.preferred_body) THEN 15
                                  ELSE 0
                                  END
                                  +
                              (
                                  SELECT COUNT(*) * 25
                                  FROM user_preference_flavor_profile upfp
                                           JOIN coffee_flavor_profile cfp
                                                ON cfp.flavor_profile_id = upfp.flavor_profile_id
                                  WHERE upfp.user_preference_id = up.id
                                    AND cfp.coffee_id = c.id
                              )
                              )::double precision AS match_score
                      FROM app_user u
                               JOIN user_preference up
                                    ON up.user_id = u.id
                                        AND up.quiz_completed = true
                               JOIN coffee_ranking cr
                                    ON cr.position > 0
                               JOIN coffee c
                                    ON c.id = cr.coffee_id
                               LEFT JOIN acidity a
                                         ON a.id = c.acidity_id
                               LEFT JOIN body b
                                         ON b.id = c.body_id
                      WHERE u.id = target_user_id
                        AND COALESCE(u.is_blocked, false) = false
                  ) coffee_data
             WHERE coffee_data.allow_exploration = true
                OR coffee_data.match_score > 0

             UNION ALL

             SELECT
                 recipe_data.user_id,
                 recipe_data.user_preference_id,
                 NULL::int AS coffee_id,
                 recipe_data.recipe_id,
                 NULL::int AS coffee_ranking_id,
                 recipe_data.match_score,
                 recipe_data.popularity_score,
                 recipe_data.match_score * 10 + LEAST(recipe_data.popularity_score, 100) * 0.1 AS final_score,
                 'recipe' AS source,
                 row_number() OVER (
                     PARTITION BY recipe_data.user_id
                     ORDER BY
                         recipe_data.match_score * 10 + LEAST(recipe_data.popularity_score, 100) * 0.1 DESC,
                         recipe_data.match_score DESC,
                         recipe_data.popularity_score DESC,
                         recipe_data.position ASC,
                         recipe_data.recipe_title ASC
                     ) AS recommendation_position
             FROM (
                      SELECT
                          u.id AS user_id,
                          up.id AS user_preference_id,
                          up.recommendation_style,
                          up.allow_exploration,
                          rr.recipe_id,
                          rr.position,
                          r.title AS recipe_title,
                          rr.ranking_score AS popularity_score,
                          (
                              CASE
                                  WHEN r.brewing_method_id IS NOT NULL
                                      AND EXISTS (
                                          SELECT 1
                                          FROM user_preference_brewing_method upbm
                                          WHERE upbm.user_preference_id = up.id
                                            AND upbm.brewing_method_id = r.brewing_method_id
                                      ) THEN 30
                                  ELSE 0
                                  END
                                  +
                              CASE
                                  WHEN c.id IS NOT NULL
                                      AND EXISTS (
                                          SELECT 1
                                          FROM user_preference_region upr
                                          WHERE upr.user_preference_id = up.id
                                            AND upr.region_id = c.region_id
                                      ) THEN 20
                                  ELSE 0
                                  END
                                  +
                              CASE
                                  WHEN c.id IS NOT NULL
                                      AND up.preferred_acidity IS NOT NULL
                                      AND a.name IS NOT NULL
                                      AND lower(a.name) = lower(up.preferred_acidity) THEN 10
                                  ELSE 0
                                  END
                                  +
                              CASE
                                  WHEN c.id IS NOT NULL
                                      AND up.preferred_body IS NOT NULL
                                      AND b.name IS NOT NULL
                                      AND lower(b.name) = lower(up.preferred_body) THEN 10
                                  ELSE 0
                                  END
                                  +
                              (
                                  SELECT COUNT(*) * 25
                                  FROM user_preference_flavor_profile upfp
                                           JOIN coffee_flavor_profile cfp
                                                ON cfp.flavor_profile_id = upfp.flavor_profile_id
                                  WHERE upfp.user_preference_id = up.id
                                    AND cfp.coffee_id = c.id
                              )
                              )::double precision AS match_score
                      FROM app_user u
                               JOIN user_preference up
                                    ON up.user_id = u.id
                                        AND up.quiz_completed = true
                               JOIN recipe_ranking rr
                                    ON rr.position > 0
                               JOIN recipe r
                                    ON r.id = rr.recipe_id
                                        AND r.is_public = true
                               LEFT JOIN coffee c
                                         ON c.id = r.coffee_id
                               LEFT JOIN acidity a
                                         ON a.id = c.acidity_id
                               LEFT JOIN body b
                                         ON b.id = c.body_id
                      WHERE u.id = target_user_id
                        AND COALESCE(u.is_blocked, false) = false
                  ) recipe_data
             WHERE recipe_data.allow_exploration = true
                OR recipe_data.match_score > 0
         ) recommendation_data
    WHERE recommendation_data.recommendation_position <= 10;
END;
$$;

CREATE OR REPLACE FUNCTION refresh_recommendations()
    RETURNS void
    LANGUAGE plpgsql
AS $$
DECLARE
    user_record record;
BEGIN
    FOR user_record IN
        SELECT u.id
        FROM app_user u
                 JOIN user_preference up
                      ON up.user_id = u.id
                          AND up.quiz_completed = true
        WHERE COALESCE(u.is_blocked, false) = false
        LOOP
            PERFORM refresh_recommendations_for_user(user_record.id);
        END LOOP;
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

SELECT refresh_all_rankings();
SELECT refresh_recommendations();