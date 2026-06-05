namespace brewbase.server.Tests.Postgres.Infrastructure;

internal static class PostgresDatabasePaths
{
    public static string SchemaSql =>
        ResolveDatabaseFile("schema.sql");

    public static string GlobalSearchMigrationSql =>
        ResolveDatabaseFile(Path.Combine("migrations", "013_global_search_extensions.sql"));

    public static string UserRankingFixMigrationSql =>
        ResolveDatabaseFile(Path.Combine("migrations", "2026-06-03_fix_user_ranking_activity_score.sql"));

    private static string ResolveDatabaseFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "database", relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Could not locate database/{relativePath} from {AppContext.BaseDirectory}");
    }
}
