using System.Collections.Immutable;
using System.Text.Json;
using ConfigCompare.Session.Contract;
using ConfigCompare.Session.Service;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConfigCompare.Session.Service.Tests.Unit;

/// <summary>
/// Unit tests for SessionService.
/// </summary>
public class SessionServiceTests
{
    private readonly Mock<ILogger<SessionService>> MockLogger;

    public SessionServiceTests()
    {
        MockLogger = new Mock<ILogger<SessionService>>();
    }

    private async Task CleanupDatabaseAsync(string tempDb)
    {
        // Give SQLite a chance to release all handles
        GC.Collect();
        GC.WaitForPendingFinalizers();
        await Task.Delay(100);

        if (File.Exists(tempDb))
        {
            try { File.Delete(tempDb); } catch { }
        }
    }

    [Fact]
    public async Task SaveSessionAsync_NewSnapshot_SuccessfullyPersists()
    {
        // Arrange
        var tempDb = Path.GetTempFileName();
        var service = new SessionService(tempDb, MockLogger.Object);

        // Initialize the database
        var initialiser = new DatabaseInitialiser(tempDb, null);
        await initialiser.InitialiseAsync();

        var snapshot = new SessionSnapshotDto(
            SavedAt: DateTimeOffset.UtcNow,
            OpenModules: new[] { "module1", "module2" }.ToList().AsReadOnly(),
            ActiveModule: "module1",
            SelectedResourceGroupNames: new[] { "rg1" }.ToList().AsReadOnly(),
            WindowState: new Dictionary<string, string> { { "Width", "800" }, { "Height", "600" } }.ToImmutableDictionary(),
            ModuleState: new Dictionary<string, string> { { "scroll_pos", "100" } }.ToImmutableDictionary());

        try
        {
            // Act
            var response = await service.SaveSessionAsync(snapshot);

            // Assert
            response.IsSuccess.Should().BeTrue();
            response.ErrorMessage.Should().BeNull();

            // Verify the session was persisted
            var loadResponse = await service.LoadSessionAsync();
            loadResponse.IsSuccess.Should().BeTrue();
            loadResponse.Data.Should().NotBeNull();
            loadResponse.Data!.OpenModules.Should().Equal("module1", "module2");
            loadResponse.Data.ActiveModule.Should().Be("module1");
        }
        finally
        {
            await CleanupDatabaseAsync(tempDb);
        }
    }

    [Fact]
    public async Task SaveSessionAsync_OverwriteExisting_OnlyLatestReturned()
    {
        // Arrange
        var tempDb = Path.GetTempFileName();
        var service = new SessionService(tempDb, MockLogger.Object);

        var initialiser = new DatabaseInitialiser(tempDb, null);
        await initialiser.InitialiseAsync();

        var snapshot1 = new SessionSnapshotDto(
            SavedAt: DateTimeOffset.UtcNow,
            OpenModules: new[] { "module1" }.ToList().AsReadOnly(),
            ActiveModule: "module1",
            SelectedResourceGroupNames: Array.Empty<string>().ToList().AsReadOnly(),
            WindowState: ImmutableDictionary<string, string>.Empty,
            ModuleState: ImmutableDictionary<string, string>.Empty);

        var snapshot2 = new SessionSnapshotDto(
            SavedAt: DateTimeOffset.UtcNow.AddSeconds(1),
            OpenModules: new[] { "module2", "module3" }.ToList().AsReadOnly(),
            ActiveModule: "module2",
            SelectedResourceGroupNames: Array.Empty<string>().ToList().AsReadOnly(),
            WindowState: ImmutableDictionary<string, string>.Empty,
            ModuleState: ImmutableDictionary<string, string>.Empty);

        try
        {
            // Act
            await service.SaveSessionAsync(snapshot1);
            await service.SaveSessionAsync(snapshot2);

            // Assert
            var loadResponse = await service.LoadSessionAsync();
            loadResponse.IsSuccess.Should().BeTrue();
            loadResponse.Data!.OpenModules.Should().Equal("module2", "module3");
            loadResponse.Data.ActiveModule.Should().Be("module2");
        }
        finally
        {
            await CleanupDatabaseAsync(tempDb);
        }
    }

    [Fact]
    public async Task LoadSessionAsync_NoSession_ReturnsSuccessWithNullData()
    {
        // Arrange
        var tempDb = Path.GetTempFileName();
        var service = new SessionService(tempDb, MockLogger.Object);

        var initialiser = new DatabaseInitialiser(tempDb, null);
        await initialiser.InitialiseAsync();

        try
        {
            // Act
            var response = await service.LoadSessionAsync();

            // Assert
            response.IsSuccess.Should().BeTrue();
            response.ErrorMessage.Should().BeNull();
            response.Data.Should().BeNull();
        }
        finally
        {
            await CleanupDatabaseAsync(tempDb);
        }
    }

    [Fact]
    public async Task LoadSessionAsync_ExistingSession_ReturnsSnapshot()
    {
        // Arrange
        var tempDb = Path.GetTempFileName();
        var service = new SessionService(tempDb, MockLogger.Object);

        var initialiser = new DatabaseInitialiser(tempDb, null);
        await initialiser.InitialiseAsync();

        var originalSnapshot = new SessionSnapshotDto(
            SavedAt: DateTimeOffset.UtcNow,
            OpenModules: new[] { "module1" }.ToList().AsReadOnly(),
            ActiveModule: "module1",
            SelectedResourceGroupNames: new[] { "rg1", "rg2" }.ToList().AsReadOnly(),
            WindowState: new Dictionary<string, string> { { "Width", "1024" } }.ToImmutableDictionary(),
            ModuleState: new Dictionary<string, string> { { "column_width", "200" } }.ToImmutableDictionary());

        try
        {
            // Act
            await service.SaveSessionAsync(originalSnapshot);
            var loadResponse = await service.LoadSessionAsync();

            // Assert
            loadResponse.IsSuccess.Should().BeTrue();
            loadResponse.Data.Should().NotBeNull();
            loadResponse.Data!.OpenModules.Should().Equal("module1");
            loadResponse.Data.ActiveModule.Should().Be("module1");
            loadResponse.Data.SelectedResourceGroupNames.Should().Equal("rg1", "rg2");
            loadResponse.Data.WindowState.Should().Contain(new KeyValuePair<string, string>("Width", "1024"));
            loadResponse.Data.ModuleState.Should().Contain(new KeyValuePair<string, string>("column_width", "200"));
        }
        finally
        {
            await CleanupDatabaseAsync(tempDb);
        }
    }

    [Fact]
    public async Task ClearSessionAsync_ClearsData()
    {
        // Arrange
        var tempDb = Path.GetTempFileName();
        var service = new SessionService(tempDb, MockLogger.Object);

        var initialiser = new DatabaseInitialiser(tempDb, null);
        await initialiser.InitialiseAsync();

        var snapshot = new SessionSnapshotDto(
            SavedAt: DateTimeOffset.UtcNow,
            OpenModules: new[] { "module1" }.ToList().AsReadOnly(),
            ActiveModule: "module1",
            SelectedResourceGroupNames: Array.Empty<string>().ToList().AsReadOnly(),
            WindowState: ImmutableDictionary<string, string>.Empty,
            ModuleState: ImmutableDictionary<string, string>.Empty);

        try
        {
            // Act
            await service.SaveSessionAsync(snapshot);
            var clearResponse = await service.ClearSessionAsync();

            // Assert
            clearResponse.IsSuccess.Should().BeTrue();

            // Verify subsequent load returns null data
            var loadResponse = await service.LoadSessionAsync();
            loadResponse.IsSuccess.Should().BeTrue();
            loadResponse.Data.Should().BeNull();
        }
        finally
        {
            await CleanupDatabaseAsync(tempDb);
        }
    }

    [Fact]
    public async Task SaveSessionAsync_NullSnapshot_ReturnsError()
    {
        // Arrange
        var tempDb = Path.GetTempFileName();
        var service = new SessionService(tempDb, MockLogger.Object);

        var initialiser = new DatabaseInitialiser(tempDb, null);
        await initialiser.InitialiseAsync();

        try
        {
            // Act
            var response = await service.SaveSessionAsync(null!);

            // Assert
            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().NotBeNullOrEmpty();
        }
        finally
        {
            await CleanupDatabaseAsync(tempDb);
        }
    }

    [Fact]
    public async Task LoadSessionAsync_CorruptJson_ReturnsError()
    {
        // Arrange
        var tempDb = Path.GetTempFileName();
        var service = new SessionService(tempDb, MockLogger.Object);

        var initialiser = new DatabaseInitialiser(tempDb, null);
        await initialiser.InitialiseAsync();

        // Manually insert corrupted JSON into the database
        using (var connection = new SqliteConnection($"Data Source={tempDb}"))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Sessions (SavedAt, Payload) VALUES (@SavedAt, @Payload)";
            command.Parameters.AddWithValue("@SavedAt", DateTimeOffset.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("@Payload", "{ invalid json ]");
            await command.ExecuteNonQueryAsync();
        }

        try
        {
            // Act
            var response = await service.LoadSessionAsync();

            // Assert
            response.IsSuccess.Should().BeFalse();
            response.ErrorMessage.Should().NotBeNullOrEmpty();
            response.Data.Should().BeNull();
        }
        finally
        {
            await CleanupDatabaseAsync(tempDb);
        }
    }
}
