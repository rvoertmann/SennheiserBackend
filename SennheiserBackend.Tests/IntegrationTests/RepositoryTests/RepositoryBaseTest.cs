using FluentAssertions;
using Moq;
using SennheiserBackend.Database.Repositories;
using SennheiserBackend.Database.Repositories.Entities;
using SennheiserBackend.Tests.TestData;

namespace SennheiserBackend.Tests.IntegrationTests.RepositoryTests
{
    [TestClass]
    public class RepositoryBaseTest
    {
        [TestMethod]
        public async Task Add_AnyEntity_ShouldAddAndSave()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiverEntity = testData.ReceiverEntities[0];

            var contextFactory = new TestDemoDbContextFactory();

            var arrangeDbContext = contextFactory.CreateDbContext();

            var repositoryBaseMock = new Mock<RepositoryBase<ReceiverEntity>>(contextFactory)
            {
                CallBase = true,
            };

            //ACT
            await repositoryBaseMock.Object.Add(testReceiverEntity);

            //ASSERT
            var addedEntity = await arrangeDbContext.FindAsync<ReceiverEntity>(testReceiverEntity.Id);
            addedEntity.Should().NotBeNull();
            addedEntity?.Id.Should().Be(testReceiverEntity.Id);
            addedEntity?.Microphone.Id.Should().Be(testReceiverEntity.Microphone.Id);
        }

        [TestMethod]
        public async Task Delete_EntityNotExistent_ShouldThrowException()
        {
            //ARRANGE
            var contextFactory = new TestDemoDbContextFactory();

            var repositoryBaseMock = new Mock<RepositoryBase<ReceiverEntity>>(contextFactory)
            {
                CallBase = true,
            };

            Func<Task> action = async () => await repositoryBaseMock.Object.Delete(Guid.NewGuid().ToString());

            //ACT / ASSERT
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task Delete_ExistingEntity_ShouldDelete()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiverEntity = testData.ReceiverEntities[0];

            var contextFactory = new TestDemoDbContextFactory();

            var arrangeDbContext = contextFactory.CreateDbContext();
            await arrangeDbContext.AddAsync(testReceiverEntity);
            await arrangeDbContext.SaveChangesAsync();

            var repositoryBaseMock = new Mock<RepositoryBase<ReceiverEntity>>(contextFactory)
            {
                CallBase = true,
            };

            var addedReceiver = await arrangeDbContext.FindAsync<ReceiverEntity>(testReceiverEntity.Id);

            //ACT
            await repositoryBaseMock.Object.Delete(testReceiverEntity.Id);

            //ASSERT
            addedReceiver.Should().NotBeNull();
            addedReceiver?.Id.Should().Be(testReceiverEntity.Id);
            var assertDbContext = contextFactory.CreateDbContext();
            var deletedReceiver = await assertDbContext.FindAsync<ReceiverEntity>(testReceiverEntity.Id);
            deletedReceiver.Should().BeNull();
        }

        [TestMethod]
        public async Task GetById_ExistingEntity_ShouldReturnEntity()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiverEntity = testData.ReceiverEntities[0];

            var contextFactory = new TestDemoDbContextFactory();

            var arrangeDbContext = contextFactory.CreateDbContext();
            await arrangeDbContext.AddAsync(testReceiverEntity);
            await arrangeDbContext.SaveChangesAsync();

            var repositoryBaseMock = new Mock<RepositoryBase<ReceiverEntity>>(contextFactory)
            {
                CallBase = true,
            };

            //ACT
            var resultEntity = await repositoryBaseMock.Object.GetById(testReceiverEntity.Id);

            //ASSERT
            resultEntity.Should().NotBeNull();
            resultEntity?.Id.Should().Be(testReceiverEntity.Id);
        }

        [TestMethod]
        public async Task GetById_NonExistentEntity_ShouldReturnNull()
        {
            //ARRANGE
            var contextFactory = new TestDemoDbContextFactory();

            var repositoryBaseMock = new Mock<RepositoryBase<ReceiverEntity>>(contextFactory)
            {
                CallBase = true,
            };

            //ACT
            var resultEntity = await repositoryBaseMock.Object.GetById(It.IsAny<string>());

            //ASSERT
            resultEntity.Should().BeNull();
        }

        [TestMethod]
        public async Task Update_ExistingEntity_ShouldUpdateAndReturnEntity()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiverEntity = testData.ReceiverEntities[0];

            var contextFactory = new TestDemoDbContextFactory();

            var arrangeDbContext = contextFactory.CreateDbContext();
            await arrangeDbContext.AddAsync(testReceiverEntity);
            await arrangeDbContext.SaveChangesAsync();

            var repositoryBaseMock = new Mock<RepositoryBase<ReceiverEntity>>(contextFactory)
            {
                CallBase = true,
            };

            //Create new instance to prevent unintended entity change on instance directly which could invalidate the test
            var updatedEntity = new ReceiverEntity
            {
                Id = testReceiverEntity.Id,
                Name = testReceiverEntity.Name,
                Host = "newHost",
                Port = testReceiverEntity.Port,
                Microphone = testReceiverEntity.Microphone
            };

            //ACT
            var resultEntity = await repositoryBaseMock.Object.Update(updatedEntity);

            //ASSERT
            var assertDbContext = contextFactory.CreateDbContext();
            var fetchedEntity = await assertDbContext.FindAsync<ReceiverEntity>(testReceiverEntity.Id);
            fetchedEntity?.Host.Should().Be(updatedEntity.Host);
            resultEntity.Should().NotBeNull();
            resultEntity.Host.Should().Be(updatedEntity.Host);
        }
    }
}
