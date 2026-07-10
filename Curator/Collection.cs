using System;
using System.Collections.Generic;
using System.Text;

namespace Curator.Models
{
    public class Collection
    {
        public string Name { get; set; } = "";
        public int ItemCount { get; set; }
        public bool IsFolder { get; set; }
    }
}
