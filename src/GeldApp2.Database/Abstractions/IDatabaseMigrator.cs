using GeldApp2.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DatabaseMigrator> log;

        public DatabaseMigrator(GeldAppContext db, ILogger<DatabaseMigrator> log)
        {
            this.db = db;
            this.log = log;
        }

        public void Migrate()
        {
            var migrations = this.db.Database.GetPendingMigrations().ToArray();
            if (migrations.Any())
            {
                this.log.LogInformation($"Migrating {migrations.Length} steps...");
            }

            this.db.Database.Migrate();
        }
    }
}
