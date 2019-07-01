using System;
using System.Collections.Generic;
using System.Text;

namespace GeldApp2.Database.ViewModels
{
    /// <summary>
    /// Simple key value item for custom sql queries.
    /// </summary>
    public class KeyValueItem
    {
        public string Key { get; set; }

        public long Value { get; set; }

        public override string ToString()
        {
            return $"{this.Key} => {this.Value}";
        }
    }
}
