using SQLite;

namespace Curator.Models;
public class Item
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public string Name { get; set; } = "";

}
