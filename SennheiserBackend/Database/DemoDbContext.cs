using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SennheiserBackend.Database.Repositories.Entities;

namespace SennheiserBackend.Database
{
    public class DemoDbContext : DbContext
    {
        private const string LocalConnectionString = "Data Source=database.db";
        private readonly SqliteConnection? connection;

        public string ConnectionString { get; set; } = "";

        public DbSet<ReceiverEntity> Receivers { get; set; }


        public DemoDbContext()
        {
            ConnectionString = LocalConnectionString;
        }

        public DemoDbContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DemoDbContext(SqliteConnection connection)
        {
            this.connection = connection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (connection != null)
            {
                optionsBuilder.UseSqlite(connection);
            }
            else
            {
                optionsBuilder.UseSqlite(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReceiverEntity>()
                .Navigation(r => r.Microphone)
                .AutoInclude();
        }
    }
}
