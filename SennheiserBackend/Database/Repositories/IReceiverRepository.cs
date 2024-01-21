using SennheiserBackend.Database.Repositories.Entities;

namespace SennheiserBackend.Database.Repositories
{
    /// <summary>
    /// Provides methods sepcifically for receiver entities.
    /// </summary>
    public interface IReceiverRepository : IRepository<ReceiverEntity>
    {
        /// <summary>
        /// Gets all receiver entities from the repository.
        /// </summary>
        /// <returns>Enumerable list of receiver entities.</returns>
        public Task<IEnumerable<ReceiverEntity>> GetAll();
    }
}
