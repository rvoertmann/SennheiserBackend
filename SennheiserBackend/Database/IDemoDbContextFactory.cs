namespace SennheiserBackend.Database
{
    public interface IDemoDbContextFactory
    {
        public DemoDbContext CreateDbContext();
    }
}
