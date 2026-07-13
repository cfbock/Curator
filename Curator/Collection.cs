using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Curator.Models
{
    public class Collection
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public int ItemCount { get; set; }
        public bool IsFolder { get; set; }
    }
}
