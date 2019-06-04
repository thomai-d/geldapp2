using GeldApp2.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database.Abstractions
{
    /// <summary>
    /// Interface for abstracting db migration
    /// since this is not supported by EF core in-memory driver.
    /// This feature is used in integration/unit tests.
    /// </summary>
    public interface IDatabaseMigrator
    {
        void Migrate();
    }

    public class DatabaseMigrator : IDatabaseMigrator
    {
        private readonly GeldAppContext db;

        public DatabaseMigrator(GeldAppContext db)
        {
            this.db = db;
        }

        public void Migrate()
        {
            this.db.Database.Migrate();
        }
    }
}
