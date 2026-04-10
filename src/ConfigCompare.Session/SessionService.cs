using System.Text.Json;
using CommunityToolkit.Diagnostics;
using ConfigCompare.Session.Resources;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ConfigCompare.Session;

/// <summary>
/// SQLite-backed implementation of ISessionService.
/// Persists application session state to a local SQLite database.
/// </summary>
public class SessionService : ISessionService
{
    private readonly string DatabasePath;
    private readonly ILogger<SessionService> Logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the SessionService.
    /// </summary>
    /// <param name="dbPath">Path to the SQLite database file.</param>
    /// <param name="logger">Logger instance.</param>
    public SessionService(string dbPath, ILogger<SessionService> logger)
    {
        Guard.IsNotNull(dbPath);
        Guard.IsNotNull(logger);
        DatabasePath = dbPath;
        Logger = logger;
    }

    /// <summary>
    /// Saves a session snapshot to persistent storage.
    /// Only the most recent session snapshot is retained; previous snapshots are discarded.
    /// </summary>
    public async Task<SaveSessionResponse> SaveSessionAsync(SessionSnapshotDto snapshot, CancellationToken cancellationToken = default)
    {
        try
        {
            if (snapshot is null)
            {
                const string errorMessage = "Snapshot cannot be null.";
                Logger.LogWarning(errorMessage);
                return new SaveSessionResponse(false, errorMessage);
            }

            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            // Delete all existing sessions
            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM Sessions";
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            // Insert new session
            var json = JsonSerializer.Serialize(snapshot, JsonOptions);
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO Sessions (SavedAt, Payload) VALUES (@SavedAt, @Payload)";
            insertCommand.Parameters.AddWithValue("@SavedAt", DateTimeOffset.UtcNow.ToString("O"));
            insertCommand.Parameters.AddWithValue("@Payload", json);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            Logger.LogInformation("Session saved successfully.");

            return new SaveSessionResponse(true, null);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to save session: {ex.Message}";
            Logger.LogError(ex, errorMessage);
            return new SaveSessionResponse(false, errorMessage);
        }
    }

    /// <summary>
    /// Loads the most recent session snapshot from persistent storage.
    /// Returns success with Data=null if no prior session exists (not an error condition).
    /// </summary>
    public async Task<LoadSessionResponse> LoadSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Payload FROM Sessions LIMIT 1";
            var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                Logger.LogInformation("No prior session found; returning null data.");
                return new LoadSessionResponse(true, null, null);
            }

            var payload = reader.GetString(0);
            var snapshot = JsonSerializer.Deserialize<SessionSnapshotDto>(payload, JsonOptions);

            if (snapshot is null)
            {
                Logger.LogWarning("Deserialized snapshot was null; returning null data.");
                return new LoadSessionResponse(true, null, null);
            }

            Logger.LogInformation("Session loaded successfully.");
            return new LoadSessionResponse(true, null, snapshot);
        }
        catch (JsonException ex)
        {
            const string errorMessage = "Session payload contains invalid JSON.";
            Logger.LogWarning(ex, errorMessage);
            return new LoadSessionResponse(false, errorMessage, null);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to load session: {ex.Message}";
            Logger.LogError(ex, errorMessage);
            return new LoadSessionResponse(false, errorMessage, null);
        }
    }

    /// <summary>
    /// Clears all persisted session data.
    /// </summary>
    public async Task<ClearSessionResponse> ClearSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={DatabasePath}");
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Sessions";
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            Logger.LogInformation("Session cleared successfully.");
            return new ClearSessionResponse(true, null);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to clear session: {ex.Message}";
            Logger.LogError(ex, errorMessage);
            return new ClearSessionResponse(false, errorMessage);
        }
    }
}
