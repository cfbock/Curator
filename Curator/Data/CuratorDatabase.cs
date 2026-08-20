using Curator.Models;
using SQLite;

namespace Curator.Data;

public class CuratorDatabase
{
    private const string DatabaseFilename = "curator.db3";

    // Shared SQLite connection for database operations.
    private readonly SQLiteAsyncConnection _database;

    // Initialize database connection
    public CuratorDatabase()
    {
        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            DatabaseFilename);

        _database = new SQLiteAsyncConnection(databasePath);
    }

    // Make sure the collections table exists.
    public async Task InitializeAsync()
    {
        await _database.CreateTableAsync<Collection>();
    }

    // Get child or root collections
    public async Task<List<Collection>> GetCollectionsAsync(int? parentCollectionId = null)
    {
        await InitializeAsync();

        var query = _database.Table<Collection>();

        if (parentCollectionId.HasValue)
        {
            query = query.Where(c => c.ParentCollectionId == parentCollectionId.Value);
        }
        else
        {
            query = query.Where(c => c.ParentCollectionId == null);
        }

        return await query.ToListAsync();
    }

    // Save collection
    public async Task<int> SaveCollectionAsync(Collection collection)
    {
        await InitializeAsync();

        if (collection.Id != 0)
        {
            return await _database.UpdateAsync(collection);
        }

        return await _database.InsertAsync(collection);
    }

    // Remove collection
    public async Task<int> DeleteCollectionAsync(Collection collection)
    {
        await InitializeAsync();

        return await _database.DeleteAsync(collection);
    }

    // Count collections
    public async Task<int> GetCollectionCountAsync(int parentCollectionId)
    {
        await InitializeAsync();

        return await _database
            .Table<Collection>()
            .Where(c => c.ParentCollectionId == parentCollectionId)
            .CountAsync();
    }

    // Check if collection exists
    public async Task<bool> CollectionExistsAsync(
        string name, 
        int? parentCollectionId, 
        int? excludeCollectionId = null)
    {
        await InitializeAsync();

        var query = _database
            .Table<Collection>()
            .Where(c => c.Name == name &&
                        c.ParentCollectionId == parentCollectionId);

        if (excludeCollectionId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCollectionId.Value);
        }

        var count = await query.CountAsync();
        return count > 0;
    }
} 
