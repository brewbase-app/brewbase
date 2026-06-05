namespace brewbase.server.Tests.Postgres.Infrastructure;

internal static class PostgresTestSeed
{
    public const int DefaultUserId = 1;
    public const int AdminUserId = 2;
    public const int SecondUserId = 3;

    public const string DefaultUserLogin = "postgres.tester";
    public const string AdminUserLogin = "admin.postgres";
    public const string SecondUserLogin = "postgres.tester.two";

    public static Task SeedMinimalDataAsync(string connectionString) =>
        SqlScriptRunner.ExecuteScriptAsync(connectionString, MinimalSeedSql);

    internal const string MinimalSeedSql = """
        INSERT INTO country (name) VALUES ('Ethiopia');

        INSERT INTO region (name, country_id) VALUES ('Yirgacheffe', 1);

        INSERT INTO roastery (name) VALUES ('Postgres Test Roastery');

        INSERT INTO brewing_method (name, description)
        VALUES ('V60', 'Pour-over brewing method for tests.');

        INSERT INTO app_user (email, password_hash, login, role, created_at, is_blocked)
        VALUES
            (
                'postgres.tester@brewbase.local',
                'test-hash',
                'postgres.tester',
                'User',
                CURRENT_TIMESTAMP,
                FALSE),
            (
                'admin.postgres@brewbase.local',
                'test-hash',
                'admin.postgres',
                'Admin',
                CURRENT_TIMESTAMP,
                FALSE),
            (
                'postgres.tester.two@brewbase.local',
                'test-hash',
                'postgres.tester.two',
                'User',
                CURRENT_TIMESTAMP,
                FALSE);

        INSERT INTO coffee (name, roastery_id, region_id, created_by_user_id, is_verified)
        VALUES ('Część Etiopii', 1, 1, 1, TRUE);
        """;
}
