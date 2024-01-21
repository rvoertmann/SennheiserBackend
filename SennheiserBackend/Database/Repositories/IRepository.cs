using SennheiserBackend.Database.Repositories.Entities;

namespace SennheiserBackend.Database.Repositories
{
    /// <summary>
    /// Provides basic methods for repository CRUD operations.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to process.</typeparam>
    public interface IRepository<TEntity> where TEntity : EntityBase
    {
        /// <summary>
        /// Adds an entity to the repository.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns></returns>
        public Task Add(TEntity entity);
        /// <summary>
        /// Deletes an entity from the repository.
        /// </summary>
        /// <param name="id">Id of entity to delete.</param>
        /// <returns></returns>
        public Task Delete(string id);
        /// <summary>
        /// Gets an entity from the repository by id.
        /// </summary>
        /// <param name="id">Id of entity to receive.</param>
        /// <returns>Entity for Id, null if not found.</returns>
        public Task<TEntity?> GetById(string id);
        /// <summary>
        /// Updates an existing entity on the repository.
        /// </summary>
        /// <param name="entity">Entity with information to update.</param>
        /// <returns>Updated entity.</returns>
        public Task<TEntity> Update(TEntity entity);
    }
}
