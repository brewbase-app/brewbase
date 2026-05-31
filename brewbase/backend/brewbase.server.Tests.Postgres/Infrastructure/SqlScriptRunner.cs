using Npgsql;

namespace brewbase.server.Tests.Postgres.Infrastructure;

/// <summary>
/// Executes multi-statement PostgreSQL scripts (including PL/pgSQL $$ blocks) via Npgsql.
/// </summary>
internal static class SqlScriptRunner
{
    public static async Task ExecuteFileAsync(string connectionString, string filePath)
    {
        var sql = await File.ReadAllTextAsync(filePath);
        await ExecuteScriptAsync(connectionString, sql);
    }

    public static async Task ExecuteScriptAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        foreach (var batch in SplitIntoBatches(StripLineComments(sql)))
        {
            await using var command = new NpgsqlCommand(batch, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static string StripLineComments(string sql)
    {
        var result = new System.Text.StringBuilder(sql.Length);

        foreach (var line in sql.Split('\n'))
        {
            var commentStart = line.IndexOf("--", StringComparison.Ordinal);
            var trimmed = commentStart >= 0 ? line[..commentStart] : line;
            result.AppendLine(trimmed);
        }

        return result.ToString();
    }

    internal static IEnumerable<string> SplitIntoBatches(string sql)
    {
        var batch = new System.Text.StringBuilder();
        string? dollarTag = null;
        var inSingleQuote = false;

        for (var index = 0; index < sql.Length; index++)
        {
            var character = sql[index];

            if (dollarTag is not null)
            {
                if (TryMatchAt(sql, index, dollarTag))
                {
                    batch.Append(dollarTag);
                    index += dollarTag.Length - 1;
                    dollarTag = null;
                    continue;
                }

                batch.Append(character);
                continue;
            }

            if (inSingleQuote)
            {
                batch.Append(character);

                if (character == '\'')
                {
                    if (index + 1 < sql.Length && sql[index + 1] == '\'')
                    {
                        batch.Append(sql[++index]);
                    }
                    else
                    {
                        inSingleQuote = false;
                    }
                }

                continue;
            }

            if (character == '\'')
            {
                inSingleQuote = true;
                batch.Append(character);
                continue;
            }

            if (character == '$' && TryReadDollarTag(sql, index, out var tag, out var tagLength))
            {
                dollarTag = tag;
                batch.Append(tag);
                index += tagLength - 1;
                continue;
            }

            if (character == ';')
            {
                var statement = batch.ToString().Trim();
                if (statement.Length > 0)
                {
                    yield return statement;
                }

                batch.Clear();
                continue;
            }

            batch.Append(character);
        }

        var tail = batch.ToString().Trim();
        if (tail.Length > 0)
        {
            yield return tail;
        }
    }

    private static bool TryReadDollarTag(string sql, int index, out string tag, out int length)
    {
        var end = index + 1;
        while (end < sql.Length && (char.IsLetterOrDigit(sql[end]) || sql[end] == '_'))
        {
            end++;
        }

        if (end >= sql.Length || sql[end] != '$')
        {
            tag = string.Empty;
            length = 0;
            return false;
        }

        tag = sql[index..(end + 1)];
        length = tag.Length;
        return true;
    }

    private static bool TryMatchAt(string sql, int index, string value) =>
        index + value.Length <= sql.Length
        && sql.AsSpan(index, value.Length).SequenceEqual(value.AsSpan());
}
