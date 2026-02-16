using Npgsql;
using Testcontainers.PostgreSql;
using TestUtilities;

namespace PrivacyConsent.Data.Tests;

/// <summary>
/// Shared test database fixture that spins up a PostgreSQL container via Testcontainers,
/// applies all SQL migrations from the Clojure project in the correct order,
/// and seeds test data from populate-db files.
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("privacy")
        .WithUsername("privacy")
        .WithPassword("privacy")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await ClojureProjectHelper.SetupTestDatabase(conn);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

/// <summary>
/// xUnit collection definition for sharing the database fixture across test classes.
/// Apply [Collection("Database")] to test classes that need the shared fixture.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
}
