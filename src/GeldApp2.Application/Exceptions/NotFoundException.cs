using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GeldApp2.Application.Exceptions
{
    /// <summary>
    /// Exception which is thrown if an entity could not be found.
    /// </summary>
    public class NotFoundException : UserException
    {
        public NotFoundException(string entity) : base($"'{entity}' could not be found")
        {
            this.Entity = entity;
        }

        public string Entity { get; private set; }
    }
}
