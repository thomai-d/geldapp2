using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database
{
    public class Subcategory
    {
        public Subcategory()
        {
        }

        public Subcategory(string name)
        {
            this.Name = name;
        }

        public long Id { get; set; }

        public long CategoryId { get; set; }

        public string Name { get; set; }

        public override string ToString() => $"Subcategory '{this.Name}', id: {this.Id}";
    }
}
