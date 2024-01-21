using FluentAssertions;
using SennheiserBackend.Database.Repositories;
using SennheiserBackend.Tests.TestData;

namespace SennheiserBackend.Tests.IntegrationTests.RepositoryTests
{
    [TestClass]
    public class ReceiverRepositoryTest
    {
        [TestMethod]
        public async Task GetAll_Always_ShouldReturnListOfEntities()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var contextFactory = new TestDemoDbContextFactory();

            var arrangeDbContext = contextFactory.CreateDbContext();
            await arrangeDbContext.AddRangeAsync(testData.ReceiverEntities);
            await arrangeDbContext.SaveChangesAsync();

            var repository = new ReceiverRepository(contextFactory);

            //ACT
            var entityList = await repository.GetAll();

            //ASSERT
            entityList.Should().HaveCount(testData.ReceiverEntities.Count);
            entityList.First().Microphone.Id.Should().BeOneOf(testData.ReceiverEntities.Select(r => r.Microphone.Id));
        }
    }
}
