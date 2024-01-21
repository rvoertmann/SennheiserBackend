namespace SennheiserBackend.Database
{
    public class DemoDbContextFactory : IDemoDbContextFactory
    {
        public DemoDbContext CreateDbContext()
        {
            var context = new DemoDbContext();
            context.Database.EnsureCreated();

            return context;
        }
    }
}
