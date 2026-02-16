using System.Text.RegularExpressions;
using Npgsql;

namespace TestUtilities;

/// <summary>
/// Shared helpers for resolving the Clojure project root and applying
/// SQL migrations/seed data. Used by all test factories/fixtures.
/// </summary>
public static class ClojureProjectHelper
{
    /// <summary>
    /// Resolves the path to the sibling Clojure project (dk-s11008-privacy-service-main)
    /// relative to the test assembly output directory.
    /// </summary>
    public static string ResolveClojureProjectRoot()
    {
        var assemblyDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", ".."));
        var privacyServiceDir = Path.GetDirectoryName(repoRoot)!;
        var clojureRoot = Path.Combine(privacyServiceDir, "dk-s11008-privacy-service-main");

        if (!Directory.Exists(clojureRoot))
        {
            var sourceDir = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
            clojureRoot = Path.GetFullPath(Path.Combine(sourceDir, "..", "..", "..", "dk-s11008-privacy-service-main"));
        }

        if (!Directory.Exists(clojureRoot))
        {
            throw new DirectoryNotFoundException(
                $"Could not find Clojure project directory. Searched at: {clojureRoot}. " +
                $"Assembly base: {assemblyDir}");
        }

        return clojureRoot;
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
        var clojureRoot = ResolveClojureProjectRoot();

        await ExecuteSqlFile(conn, Path.Combine(clojureRoot, "docker", "postgres", "3-create-schemas.sql"));
        await ApplyMigrations(conn, Path.Combine(clojureRoot, "resources", "data-inventory", "migrations"));
        await ApplyMigrations(conn, Path.Combine(clojureRoot, "resources", "consent", "migrations"));
        await ExecuteSqlFile(conn, Path.Combine(clojureRoot, "test-resources", "data-inventory", "populate-db.sql"));
        await ExecuteSqlFile(conn, Path.Combine(clojureRoot, "test-resources", "consent", "populate-db.sql"));

        if (supplementPath != null && File.Exists(supplementPath))
            await ExecuteSqlFile(conn, supplementPath);
    }
}
