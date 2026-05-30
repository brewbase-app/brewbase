-- automatyczne odświeżanie rankingów co godzinę przy użyciu pg_cron
-- wymaga PostgreSQL z rozszerzeniem pg_cron (niedostępne w domyślnym obrazie Dockera postgres).
-- należy uruchamiać tylko po schema.sql na środowiskach wspierających pg_cron (np. VM uczelni).
-- Lokalnie użyj POST /api/Ranking/refresh lub SELECT refresh_all_rankings(); zamiast tego.

CREATE EXTENSION IF NOT EXISTS pg_cron;

SELECT cron.unschedule(jobid)
FROM cron.job
WHERE jobname = 'refresh-rankings-every-hour';

SELECT cron.schedule(
               'refresh-rankings-every-hour',
               '0 * * * *',
               $$SELECT refresh_all_rankings();$$
);
