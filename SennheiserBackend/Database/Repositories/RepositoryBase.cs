using SennheiserBackend.Database.Repositories.Entities;

namespace SennheiserBackend.Database.Repositories
{
    ///<inheritdoc cref="IRepository{T}"/>
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : EntityBase
    {
        protected readonly IDemoDbContextFactory dbContextFactory;

        protected RepositoryBase(IDemoDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        ///<inheritdoc/>
        public async Task Add(TEntity entity)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            await dbContext.AddAsync(entity);
            await dbContext.SaveChangesAsync();
        }

        ///<inheritdoc/>
        public async Task Delete(string id)
        {
            using var dbContext = dbContextFactory.CreateDbContext();
            var entity = await dbContext.FindAsync<TEntity>(id);

            if (entity == null)
            {
                throw new ArgumentNullException($"Deletion of entity '{id}' failed. Entity does not exist in database.");
            }

            dbContext.Remove(entity);
            await dbContext.SaveChangesAsync();
        }

        ///<inheritdoc/>
        public async Task<TEntity?> GetById(string id)
        {
            using var dbContext = dbContextFactory.CreateDbContext();

            return await dbContext.FindAsync<TEntity>(id);
        }

        ///<inheritdoc/>
        public async Task<TEntity> Update(TEntity entity)
        {
            using var dbContext = dbContextFactory.CreateDbContext();

            dbContext.Update(entity);
            await dbContext.SaveChangesAsync();
            var updatedEntity = await dbContext.FindAsync<TEntity>(entity.Id);

            return updatedEntity!;
        }
    }
}
