using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database
{
    public class Account
    {
        protected Account()
        {
        }

        public Account(string name, params Category[] categories)
        {
            this.Name = name;
            this.Categories = categories.ToList();
        }

        public Account(string name)
        {
            this.Name = name;
        }

        public Account(long id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public ICollection<Category> Categories { get; private set; } = new List<Category>();

        public ICollection<UserAccount> UserAccounts { get; private set; }

        public override string ToString() => $"Account '{this.Name}', id: {this.Id}";
    }
}
