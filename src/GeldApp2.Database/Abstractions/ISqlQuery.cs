using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeldApp2.Database.Abstractions
{
    /// <summary>
    /// Interface for abstracting sql queries
    /// since those are not supported by EF core in-memory driver.
    /// This feature is used in integration/unit tests.
    /// </summary>
    public interface ISqlQuery
    {
        Task<T[]> Query<T>(FormattableString sql)
            where T : class;
    }

    public class SqlQuery : ISqlQuery
    {
        private readonly GeldAppContext db;

        public SqlQuery(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<T[]> Query<T>(FormattableString sql)
            where T : class
        {
            return await this.db.Query<T>()
                .FromSql(sql)
                .AsNoTracking()
                .ToArrayAsync();
        }
    }
}
