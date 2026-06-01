-- Bootstrap ranking snapshot tables after schema/seed deploy.
-- Safe to re-run: refresh functions upsert current scores.
SELECT refresh_all_rankings();
