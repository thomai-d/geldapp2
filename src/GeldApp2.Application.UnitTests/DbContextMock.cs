using GeldApp2.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GeldApp2.Application.UnitTests
{
    public static class DbContextMock
    {
        public static async Task<GeldAppContext> Create()
        {
            var options = new DbContextOptionsBuilder<GeldAppContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new GeldAppContext(options);
            databaseContext.Database.EnsureCreated();
            databaseContext.Users.Add(new User()
            {
                Id = 1,
                Name = "hans",
            });

            await databaseContext.SaveChangesAsync();
            return databaseContext;
        }
    }
}
