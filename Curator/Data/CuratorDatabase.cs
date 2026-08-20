using Curator.Models;
using SQLite;

namespace Curator.Data;

public class CuratorDatabase
{
    // Just a constant string. The database file will be named "curator.db3".
    private const string DatabaseFilename = "curator.db3";

    // A private member variable that will hold our connection to SQLite.
    private readonly SQLiteAsyncConnection _database;

    // Constructor
    public CuratorDatabase()
    {
        // Build the full path to the database file.
        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            DatabaseFilename);

        // Create a connection object and store it in our member variable.
        _database = new SQLiteAsyncConnection(databasePath);
    }

    // Make sure the Collections table exists.
    public async Task InitializeAsync()
    {
        await _database.CreateTableAsync<Collection>();
    }

    // Return every Collection in the database.
    public async Task<List<Collection>> GetCollectionsAsync(int? collectionId = null)
    {
        await InitializeAsync();

        var query = _database.Table<Collection>();

        if (collectionId.HasValue)
        {
            query = query.Where(c => c.ParentCollectionId == collectionId.Value);
        }
        else
        {
            query = query.Where(c => c.ParentCollectionId == null);
        }

        return await query.ToListAsync();
            //.ToListAsync();

        //return await _database
        //    .Table<Collection>()
        //    .ToListAsync();
    }

    // Save one Collection
    public async Task<int> SaveCollectionAsync(Collection collection)
    {
        await InitializeAsync();

        if (collection.Id != 0)
        {
            // Existing row -> UPDATE
            return await _database.UpdateAsync(collection);
        }

        // New row -> INSERT
        return await _database.InsertAsync(collection);
    }

    // Remove one Collection
    public async Task<int> DeleteCollectionAsync(Collection collection)
    {
        await InitializeAsync();

        // Existing row -> DELETE
        return await _database.DeleteAsync(collection);
    }

    public async Task<int> GetCollectionCountAsync(int parentCollectionId)
    {
        await InitializeAsync();

        return await _database
            .Table<Collection>()
            .Where(c => c.ParentCollectionId == parentCollectionId)
            .CountAsync(); //return the count of collections with the specified parentCollectionId
    }

    public async Task<bool> CollectionExistsAsync(string name, int? parentCollectionId, int? excludeCollectionId = null)
    {
        await InitializeAsync();

        // Start building the query to check for existing collections with the same name
        var query = _database.Table<Collection>().Where(c => c.Name == name && c.ParentCollectionId == parentCollectionId);

        // Exclude the collection with the specified CollectionId if provided
        if (excludeCollectionId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCollectionId.Value);
        }

        var count = await query.CountAsync();
        return count > 0;
    }
} 
