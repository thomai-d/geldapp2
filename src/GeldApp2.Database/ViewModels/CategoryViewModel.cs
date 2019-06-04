using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.ViewModels
{
    public class CategoryViewModel
    {
        public static CategoryViewModel FromDb(Category cat)
        {
            return new CategoryViewModel
            {

                Name = cat.Name,
                Subcategories = cat.Subcategories
                                    .Select(sub => sub.Name)
                                    .OrderBy(sub => sub)
                                    .ToList()
            };
        }

        // todo will be removed
        public string Account { get; set; }

        public string Name { get; set; }

        public List<string> Subcategories { get; set; } = new List<string>();
    }
}
