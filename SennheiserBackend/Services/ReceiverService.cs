
using Microsoft.EntityFrameworkCore;
using SennheiserBackend.Database.Repositories;
using SennheiserBackend.Database.Repositories.Entities;
using SennheiserBackend.Extensions;
using SennheiserBackend.Models;
using System.Data;
using System.Diagnostics;

namespace SennheiserBackend.Services
{
    ///<inheritdoc cref="IReceiverService"/>
    public class ReceiverService : IReceiverService
    {
        private static readonly int MaxDbRetries = 3;
        private readonly IReceiverRepository repository;
        private readonly IReceiverConnectionService connectionService;
        private readonly ILogger<ReceiverService> logger;
        private readonly List<Receiver> receivers = [];
        private readonly List<string> lockedIds = [];
        private readonly Stopwatch lockStopwatch = new();

        public ReceiverService(IReceiverRepository repository, IReceiverConnectionService connectionService, ILogger<ReceiverService> logger)
        {
            this.repository = repository;
            this.connectionService = connectionService;
            this.logger = logger;
            this.connectionService.MessageReceived += OnReceiverDataReceived;

            var receiverEntities = repository.GetAll().Result;
            foreach (var receiverEntity in receiverEntities)
            {
                receivers.Add(ModelFromEntity(receiverEntity));
            }
        }

        private void OnReceiverDataReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (sender is not Receiver receiver)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            logger.LogInformation("Message received from receiver {receiver.Id}.", receiver.Id);

            LockReceiver(receiver.Id);

            try
            {
                if (e.Message.Properties.TryGetValue("micGain", out string? micGainString))
                {
                    var newMicGain = int.Parse(micGainString);

                    if (receiver.Microphone.MicGain == newMicGain)
                    {
                        logger.LogInformation("No changes in message from receiver {receiver.Id} - not updating", receiver.Id);

                        return;
                    }

                    logger.LogInformation("New MicGain {newMicGain} received from receiver {receiver.Id}", newMicGain, receiver.Id);

                    ((Microphone)receiver.Microphone).MicGain = newMicGain;

                    
                    var tryCount = 1;
                    var success = false;
                    
                    while (tryCount <= MaxDbRetries && !success)
                    {
                        try
                        {
                            UpdateDb(receiver).GetAwaiter().GetResult();
                            success = true;

                            break;
                        }
                        catch
                        {
                            logger.LogWarning("Writing new MicGain for receiver with id '{receiver.Id}' failed. Try# {tries} of {max}", receiver.Id, tryCount, MaxDbRetries);
                        }

                        tryCount++;

                        Task.Delay(500).GetAwaiter().GetResult();
                    }

                    if(!success)
                    {
                        throw new DbUpdateConcurrencyException($"Update from the device could not be applied after {MaxDbRetries}.");
                    }
                }
            }
            finally 
            {
                UnlockReceiver(receiver.Id);
            }
        }

        private void LockReceiver(string id)
        {
            if(!lockedIds.Contains(id))
            {
                lockedIds.Add(id);
                lockStopwatch.Restart();
            }
        }

        private void UnlockReceiver(string id)
        {
            lockedIds.Remove(id);
            lockStopwatch.Stop();
            logger.LogInformation("Receiver object with id '{id}' was locked for {ms} ms", id, lockStopwatch.ElapsedMilliseconds);
        }

        private bool CheckIsLocked(string id)
        {
            return lockedIds.Contains(id);
        }

        ///<inheritdoc/>
        public async Task Connect(IReceiver receiver)
        {
            try
            {
                await connectionService.Open(receiver);
            }
            catch (Exception ex)
            {
                logger.LogError("Connection to receiver with id {receiver.Id} could not be established. {ex.Message}", receiver.Id, ex.Message);
                throw;
            }
        }

        ///<inheritdoc/>
        public void Disconnect(string receiverId)
        {
            connectionService.Close(receiverId);
        }

        ///<inheritdoc/>
        public async Task<IReceiver> Create(string name, string host, int? port)
        {
            var receiver = new Receiver
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Host = host,
                Port = port,
                Microphone = new Microphone
                {
                    Id = Guid.NewGuid().ToString()
                }
            };

            if (receivers.Exists(r => r.Name == receiver.Name))
            {
                throw new DuplicateNameException($"Receiver with name '{name}' already exists.");
            }

            if (receivers.Exists(r => r.AddressableHost == receiver.AddressableHost))
            {
                throw new DuplicateNameException($"Receiver with host and port '{receiver.AddressableHost}' already exists.");
            }

            receivers.Add(receiver);

            await repository.Add(EntityFromModel(receiver));

            return (IReceiver)receiver.Clone();
        }

        ///<inheritdoc/>
        public async Task Delete(string id)
        {
            var receiver = receivers.Find(r => r.Id == id);
            if (receiver == null)
            {
                throw new ArgumentNullException($"Receiver with id '{id}' does not exist.");
            }

            if (connectionService.IsConnected(id))
            {
                connectionService.Close(id);
            }

            await repository.Delete(id);

            var index = receivers.FindIndex(r => r.Id == id);
            receivers.RemoveAt(index);
        }

        ///<inheritdoc/>
        public IReceiver? Get(string id)
        {
            var receiver = receivers.Find(r => r.Id == id);

            return (IReceiver?)receiver?.Clone();
        }

        ///<inheritdoc/>
        public IEnumerable<IReceiver> GetAll()
        {
            var clonedReceivers = new List<IReceiver>();
            receivers.ForEach(r => clonedReceivers.Add(((IReceiver)r.Clone())));

            return clonedReceivers;
        }

        ///<inheritdoc/>
        public async Task<IReceiver> Update(Receiver receiver)
        {
            var changes = HasChanged(receiver);
            if (changes.Count == 0)
            {
                return (IReceiver)receivers.Single(r => r.Id == receiver.Id).Clone();
            }

            if (CheckDuplicate(receiver))
            {
                throw new DuplicateNameException("Name, Host and Port must be unique.");
            }

            if (changes.Exists(c => c.Name == "AddressableHost") && connectionService.IsConnected(receiver.Id))
            {
                connectionService.Close(receiver.Id);
            }

            if (connectionService.IsConnected(receiver.Id))
            {
                await connectionService.UpdateReceiverState(receiver);
            }

            if(CheckIsLocked(receiver.Id))
            {
                throw new DbUpdateConcurrencyException($"The receiver with id '{receiver.Id}' could not be updated. It is locked due to concurrent device writes.");
            }

            return (IReceiver)(await UpdateDb(receiver)).Clone();
        }

        private List<ValueChange> HasChanged(Receiver receiver)
        {
            var originalReceiver = receivers.Single(r => r.Id == receiver.Id);
            if (receiver == originalReceiver)
            {
                return [];
            }

            var changes = originalReceiver.DetailedCompare(receiver);

            return changes;
        }

        private async Task<IReceiver> UpdateDb(Receiver receiver)
        {
            logger.LogInformation("Update database entry for receiver {receiver.Id}", receiver.Id);

            var updatedEntity = await repository.Update(EntityFromModel(receiver));

            //Update in database
            var index = receivers.FindIndex(r => r.Id == receiver.Id);
            var updatedReceiver = ModelFromEntity(updatedEntity);

            //Update in model
            receivers[index] = updatedReceiver;

            return updatedReceiver;
        }

        private bool CheckDuplicate(IReceiver receiver)
        {
            return receivers.Exists(r => (r.Name == receiver.Name || r.AddressableHost == receiver.AddressableHost) && (r.Id != receiver.Id));
        }

        private static ReceiverEntity EntityFromModel(Receiver receiver)
        {
            return new ReceiverEntity
            {
                Id = receiver.Id,
                Name = receiver.Name,
                Host = receiver.Host,
                Port = receiver.Port,
                Microphone = new MicrophoneEntity
                {
                    Id = receiver.Microphone.Id,
                    MicGain = receiver.Microphone.MicGain
                }
            };
        }

        private static Receiver ModelFromEntity(ReceiverEntity receiver)
        {
            return new Receiver
            {
                Id = receiver.Id,
                Name = receiver.Name,
                Host = receiver.Host,
                Port = receiver.Port,
                Microphone = new Microphone
                {
                    Id = receiver.Microphone.Id,
                    MicGain = receiver.Microphone.MicGain
                }
            };
        }

        ///<inheritdoc/>
        public bool IsConnected(string receiverId)
        {
            return connectionService.IsConnected(receiverId);
        }
    }
}
