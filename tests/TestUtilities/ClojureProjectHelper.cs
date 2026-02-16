using System.Text.RegularExpressions;
using Npgsql;

namespace TestUtilities;

/// <summary>
/// Shared helpers for resolving the database SQL root and applying
/// SQL migrations/seed data. Used by all test factories/fixtures.
/// </summary>
public static class ClojureProjectHelper
{
    /// <summary>
    /// Resolves the path to the db/ directory at the repository root.
    /// Works from any test assembly output directory (bin/Debug/net9.0/).
    /// </summary>
    public static string ResolveClojureProjectRoot()
    {
        // Walk up from bin/Debug/net9.0/ to the repo root
        var assemblyDir = AppContext.BaseDirectory;
        var candidate = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", ".."));

        if (Directory.Exists(Path.Combine(candidate, "db", "schemas")))
            return candidate;

        // Fallback: try from source directory
        candidate = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", ".."));
        if (Directory.Exists(Path.Combine(candidate, "db", "schemas")))
            return candidate;

        throw new DirectoryNotFoundException(
            $"Could not find db/ directory at repository root. " +
            $"Assembly base: {assemblyDir}");
    }

    /// <summary>
    /// Applies all *.up.sql migration files from a directory in sorted order.
    /// </summary>
    public static async Task ApplyMigrations(NpgsqlConnection conn, string migrationsDir)
    {
        if (!Directory.Exists(migrationsDir))
            throw new DirectoryNotFoundException($"Migrations directory not found: {migrationsDir}");

        var upFiles = Directory.GetFiles(migrationsDir, "*.up.sql")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        foreach (var file in upFiles)
            await ExecuteSqlFile(conn, file);
    }

    /// <summary>
    /// Executes a single SQL file against the given connection.
    /// Strips psql-specific meta-commands that aren't valid in plain SQL.
    /// </summary>
    public static async Task ExecuteSqlFile(NpgsqlConnection conn, string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"SQL file not found: {filePath}");

        var sql = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(sql))
            return;

        sql = Regex.Replace(sql, @"^\\connect\s+\w+;?\s*$", "", RegexOptions.Multiline);
        sql = Regex.Replace(sql, @"INTO\s+\w+_?ignored\s*;", ";", RegexOptions.IgnoreCase);

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.CommandTimeout = 60;
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Full database setup: schemas, migrations, seed data, and optional supplement SQL.
    /// </summary>
    public static async Task SetupTestDatabase(NpgsqlConnection conn, string? supplementPath = null)
    {
        var repoRoot = ResolveClojureProjectRoot();

        await ExecuteSqlFile(conn, Path.Combine(repoRoot, "db", "schemas", "3-create-schemas.sql"));
        await ApplyMigrations(conn, Path.Combine(repoRoot, "db", "migrations", "data-inventory"));
        await ApplyMigrations(conn, Path.Combine(repoRoot, "db", "migrations", "consent"));
        await ExecuteSqlFile(conn, Path.Combine(repoRoot, "db", "seed", "data-inventory", "populate-db.sql"));
        await ExecuteSqlFile(conn, Path.Combine(repoRoot, "db", "seed", "consent", "populate-db.sql"));

        if (supplementPath != null && File.Exists(supplementPath))
            await ExecuteSqlFile(conn, supplementPath);
    }
}
