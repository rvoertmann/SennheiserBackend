using Microsoft.EntityFrameworkCore;
using SennheiserBackend.Database.Repositories.Entities;

namespace SennheiserBackend.Database.Repositories
{
    ///<inheritdoc cref="IReceiverRepository"/>
    public class ReceiverRepository(IDemoDbContextFactory dbContextFactory) : RepositoryBase<ReceiverEntity>(dbContextFactory), IReceiverRepository
    {
        ///<inheritdoc/>
        public async Task<IEnumerable<ReceiverEntity>> GetAll()
        {
            using var dbContext = dbContextFactory.CreateDbContext();

            return await dbContext.Receivers.ToListAsync();
        }
    }
}
