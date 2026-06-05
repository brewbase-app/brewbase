namespace brewbase.server.Tests.Postgres.Infrastructure;

/// <summary>
/// Brakujące elementy schematu względem schema.sql (obecne w current_schema.sql / produkcji).
/// Idempotentne — bezpieczne przy każdym uruchomieniu fixture.
/// </summary>
internal static class PostgresTestSchemaSupplement
{
    internal const string Sql = """
        ALTER TABLE user_preference
            ADD COLUMN IF NOT EXISTS experience_level varchar(50);

        ALTER TABLE user_preference
            ADD COLUMN IF NOT EXISTS preferred_acidity varchar(50);

        ALTER TABLE user_preference
            ADD COLUMN IF NOT EXISTS preferred_body varchar(50);

        ALTER TABLE user_preference
            ADD COLUMN IF NOT EXISTS recommendation_style varchar(100);

        ALTER TABLE user_preference
            ADD COLUMN IF NOT EXISTS allow_exploration boolean NOT NULL DEFAULT false;

        CREATE TABLE IF NOT EXISTS user_preference_brewing_method (
            user_preference_id integer NOT NULL,
            brewing_method_id integer NOT NULL,
            CONSTRAINT user_preference_brewing_method_pk
                PRIMARY KEY (user_preference_id, brewing_method_id),
            CONSTRAINT fk_upbm_preference
                FOREIGN KEY (user_preference_id)
                REFERENCES user_preference (id)
                ON DELETE CASCADE,
            CONSTRAINT fk_upbm_method
                FOREIGN KEY (brewing_method_id)
                REFERENCES brewing_method (id)
                ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS user_preference_region (
            user_preference_id integer NOT NULL,
            region_id integer NOT NULL,
            CONSTRAINT user_preference_region_pk
                PRIMARY KEY (user_preference_id, region_id),
            CONSTRAINT fk_upr_preference
                FOREIGN KEY (user_preference_id)
                REFERENCES user_preference (id)
                ON DELETE CASCADE,
            CONSTRAINT fk_upr_region
                FOREIGN KEY (region_id)
                REFERENCES region (id)
                ON DELETE CASCADE
        );
        """;

    public static Task ApplyAsync(string connectionString) =>
        SqlScriptRunner.ExecuteScriptAsync(connectionString, Sql);
}
