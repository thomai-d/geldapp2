using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database
{
    public class Category
    {
        private static readonly SubcategoryComparer SubcatComparer = new SubcategoryComparer();

        protected Category()
        {
        }

        public Category(string name)
        {
            this.Name = name;
        }

        public long Id { get; set; }

        public long AccountId { get; set; }

        public string Name { get; set; }

        public ICollection<Subcategory> Subcategories { get; private set; } = new List<Subcategory>();

        public override string ToString() => $"Category '{this.Name}', id: {this.Id}";

        public void SortSubcategories()
        {
            if (this.Subcategories is List<Subcategory> list)
                list.Sort(SubcatComparer);
        }

        private class SubcategoryComparer : IComparer<Subcategory>
        {
            public int Compare(Subcategory x, Subcategory y)
            {
                return string.Compare(x.Name, y.Name);
            }
        }
    }
}
