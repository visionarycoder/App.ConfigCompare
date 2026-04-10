using CommunityToolkit.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ConfigCompare.Session;

/// <summary>
/// Initializes the SQLite database schema for session storage.
/// </summary>
public class DatabaseInitialiser
{
    private readonly string DatabasePath;
    private readonly ILogger<DatabaseInitialiser>? Logger;

    /// <summary>
    /// Initializes a new instance of DatabaseInitialiser.
    /// </summary>
    /// <param name="dbPath">Path to the SQLite database file.</param>
    /// <param name="logger">Optional logger instance.</param>
    public DatabaseInitialiser(string dbPath, ILogger<DatabaseInitialiser>? logger = null)
    {
        Guard.IsNotNull(dbPath);
        DatabasePath = dbPath;
        Logger = logger;
    }

    /// <summary>
    /// Creates the database file and initializes the schema if it does not already exist.
    /// </summary>
    public async Task InitialiseAsync()
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            await connection.OpenAsync().ConfigureAwait(false);

            // Create table if it doesn't exist
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Sessions (
                    Id INTEGER PRIMARY KEY,
                    SavedAt TEXT NOT NULL,
                    Payload TEXT NOT NULL
                )";

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            Logger?.LogInformation("Database initialized at '{DatabasePath}'.", DatabasePath);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to initialize database at '{DatabasePath}'.", DatabasePath);
            throw;
        }
    }
}
