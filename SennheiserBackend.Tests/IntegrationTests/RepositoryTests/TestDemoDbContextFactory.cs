using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SennheiserBackend.Database;

namespace SennheiserBackend.Tests.IntegrationTests.RepositoryTests
{
    internal class TestDemoDbContextFactory : IDemoDbContextFactory
    {
        private readonly SqliteConnection sqliteConnection;
        public TestDemoDbContextFactory()
        {
            sqliteConnection = new SqliteConnection("Filename=:memory:");
            sqliteConnection.Open();
        }

        public DemoDbContext CreateDbContext()
        {
            var context = new DemoDbContext(sqliteConnection);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
