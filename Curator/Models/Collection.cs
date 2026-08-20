using SQLite;

namespace Curator.Models;

public class Collection
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public bool IsFolder { get; set; }
    public string CollectionType { get; set; } = "";

    // Null indicates a root-level collection
    public int? ParentCollectionId { get; set; }

    [Ignore]
    public int ItemCount { get; set; }
}
