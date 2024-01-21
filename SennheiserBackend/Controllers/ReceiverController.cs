using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SennheiserBackend.Models;
using SennheiserBackend.Services;

namespace SennheiserBackend.Controllers
{
    /// <summary>
    /// Manages thes receivers endpoint.
    /// </summary>
    /// <param name="receiverService">Receiver service managing receivers.</param>
    /// <param name="logger">Logging servive.</param>
    [ApiController]
    [Route("receivers")]
    public class ReceiverController(IReceiverService receiverService, ILogger<ReceiverController> logger) : Controller()
    {
        private readonly IReceiverService receiverService = receiverService;
        private readonly ILogger<ReceiverController> logger = logger;

        /// <summary>
        /// Register a new receiver in the backend.
        /// </summary>
        /// <param name="request">Defines the parameters for receiver creation.</param>
        /// <returns>The newly created receiver.</returns>
        [HttpPost]
        public async Task<IActionResult> Create(ReceiverCreateRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                var errorInfo = new ErrorInfo
                {
                    Message = "A name muste be specified.",
                    ExceptionType = typeof(ArgumentException).ToString()
                };

                logger.LogError("No name sepcified for creation.");

                return new BadRequestObjectResult(errorInfo);
            }

            if (string.IsNullOrEmpty(request.Host))
            {
                var errorInfo = new ErrorInfo
                {
                    Message = "A name muste be specified.",
                    ExceptionType = typeof(ArgumentException).ToString()
                };

                logger.LogError("No host sepcified for creation.");

                return new BadRequestObjectResult(errorInfo);
            }

            var receiver = await receiverService.Create(request.Name, request.Host, request.Port);

            return new CreatedResult(Request.Path, receiver);
        }

        /// <summary>
        /// Delete a receiver in the backend. This will also close any associated connection.
        /// </summary>
        /// <param name="id">Id of the receiver to delete.</param>
        [HttpDelete]
        [Route("{id?}")]
        public async Task<IActionResult> Delete(string id)
        {
            await receiverService.Delete(id);

            return new NoContentResult();
        }

        /// <summary>
        /// Lists all registered receivers.
        /// </summary>
        /// <returns>List of receiver objects.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            return new ObjectResult(receiverService.GetAll());
        }

        /// <summary>
        /// Get a single receiver.
        /// </summary>
        /// <param name="id">Id of the receiver to get.</param>
        /// <returns>The requested receiver object.</returns>
        [HttpGet]
        [Route("{id?}")]
        public IActionResult Get(string id)
        {
            var receiver = receiverService.Get(id);

            if (receiver == null)
            {
                var errorInfo = new ErrorInfo
                {
                    Message = $"Receiver with id '{id}' does not exist.",
                    ExceptionType = typeof(ArgumentNullException).ToString()
                };

                logger.LogError("Receiver with id '{id}' does not exist.", id);

                return new NotFoundObjectResult(errorInfo);
            }

            return new ObjectResult(receiver);
        }

        /// <summary>
        /// Update a receiver using a JsonPatch document.
        /// </summary>
        /// <param name="patchDocument">JsonDocument describing the changes.</param>
        /// <param name="id">Id of the receiver to be updated.</param>
        /// <returns>The updated receiver object.</returns>
        [HttpPatch]
        [Route("{id?}")]
        public async Task<IActionResult> Patch(JsonPatchDocument<Receiver> patchDocument, string id)
        {
            if (receiverService.Get(id) is not Receiver receiver)
            {
                var errorInfo = new ErrorInfo
                {
                    Message = $"Receiver with id '{id}' not found. Use POST method to create new objects.",
                    ExceptionType = typeof(ArgumentNullException).ToString()
                };

                logger.LogError("Receiver with id '{id}' does not exist.", id);

                return new NotFoundObjectResult(errorInfo);
            }

            patchDocument.ApplyTo(receiver);

            var updatedReceiver = await receiverService.Update(receiver);

            return new OkObjectResult(updatedReceiver);
        }

        /// <summary>
        /// Create new connection to receiver.
        /// </summary>
        /// <param name="receiverId">Id of receiver to connect to.</param>
        [HttpPost]
        [Route("{receiverId}/connection")]
        public async Task<IActionResult> OpenConnection(string receiverId)
        {
            var receiver = receiverService.Get(receiverId);

            if (receiver == null)
            {
                var errorInfo = new ErrorInfo
                {
                    Message = $"Receiver with id '{receiverId}' does not exist.",
                    ExceptionType = typeof(ArgumentNullException).ToString()
                };

                logger.LogError("Receiver with id '{receiverId}' does not exist.", receiverId);

                return new NotFoundObjectResult(errorInfo);
            }

            await receiverService.Connect(receiver);

            return new CreatedResult();
        }

        /// <summary>
        /// Disconnect from a receiver.
        /// </summary>
        /// <param name="receiverId">Id of the receiver to disconnect from.</param>
        [HttpDelete]
        [Route("{receiverId}/connection")]
        public IActionResult CloseConnection(string receiverId)
        {
            receiverService.Disconnect(receiverId);

            return new NoContentResult();
        }

        /// <summary>
        /// Get the connection state of the connection to the receiver.
        /// </summary>
        /// <param name="receiverId">Id of the receiver to fetch the connection state for.</param>
        [HttpGet]
        [Route("{receiverId}/connection")]
        public IActionResult GetConnectionInfo(string receiverId)
        {
            var connectionInfo = new ReceiverConnectionInfo
            {
                IsConnected = receiverService.IsConnected(receiverId)
            };

            return new OkObjectResult(connectionInfo);
        }
    }
}
