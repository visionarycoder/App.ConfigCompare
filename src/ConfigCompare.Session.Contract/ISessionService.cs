namespace ConfigCompare.Session.Contract;

/// <summary>
/// Provides operations for saving, loading, and clearing application session state.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Saves a session snapshot to persistent storage.
    /// Only the most recent session snapshot is retained; previous snapshots are discarded.
    /// </summary>
    /// <param name="snapshot">The session snapshot to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Save response with success status and error details if applicable.</returns>
    Task<SaveSessionResponse> SaveSessionAsync(SessionSnapshotDto snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the most recent session snapshot from persistent storage.
    /// Returns success with Data=null if no prior session exists (not an error condition).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Load response with success status and session data or error details.</returns>
    Task<LoadSessionResponse> LoadSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all persisted session data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Clear response with success status and error details if applicable.</returns>
    Task<ClearSessionResponse> ClearSessionAsync(CancellationToken cancellationToken = default);
}
