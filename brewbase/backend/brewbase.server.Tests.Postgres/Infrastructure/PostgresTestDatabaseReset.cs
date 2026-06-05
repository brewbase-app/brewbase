namespace brewbase.server.Tests.Postgres.Infrastructure;

/// <summary>
/// Resets mutable test data while keeping schema, extensions and functions intact.
/// </summary>
internal static class PostgresTestDatabaseReset
{
    private const string TruncateSql = """
        TRUNCATE TABLE
            report,
            notification,
            recommendations,
            user_recipe_favorite,
            user_coffee_favorite,
            recipe_rating,
            coffee_rating,
            recipe_ranking,
            coffee_ranking,
            user_ranking,
            cupping_session_coffee,
            cupping_session,
            quick_note,
            recipe,
            article,
            user_preference_flavor_profile,
            user_preference_brewing_method,
            user_preference_region,
            user_preference,
            coffee_flavor_profile,
            coffee,
            follow,
            app_user,
            brewing_method,
            roastery,
            region,
            country,
            flavor_profile,
            processing_method,
            variety,
            acidity,
            body
        RESTART IDENTITY CASCADE;
        """;

    public static async Task ResetToMinimalSeedAsync(string connectionString)
    {
        await SqlScriptRunner.ExecuteScriptAsync(connectionString, TruncateSql);
        await PostgresTestSeed.SeedMinimalDataAsync(connectionString);
    }
}
