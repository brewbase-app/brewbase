-- automatyczne odświeżanie rankingów co godzinę przy użyciu pg_cron
-- wymaga PostgreSQL z rozszerzeniem pg_cron (VM uczelni).
-- Render/Docker bez pg_cron: backup to RankingRefreshBackgroundService w backendzie (co 60 min).
-- Ręcznie: POST /api/Ranking/refresh (wymaga tokenu Admin).

CREATE EXTENSION IF NOT EXISTS pg_cron;

SELECT cron.unschedule(jobid)
FROM cron.job
WHERE jobname = 'refresh-rankings-every-hour';

SELECT cron.schedule(
               'refresh-rankings-every-hour',
               '0 * * * *',
               $$SELECT refresh_all_rankings();$$
);
