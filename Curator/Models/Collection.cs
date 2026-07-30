using SQLite;

namespace Curator.Models
{
    public class Collection
    {
        // SQLite.NET ORM attributes.
        // Tell SQLite this property is the table's primary key
        // and to automatically generate the next ID when a new row is added.
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Other properties describing a collection.
        public string Name { get; set; } = "";
        public int ItemCount { get; set; }
        public bool IsFolder { get; set; }
    }
}
