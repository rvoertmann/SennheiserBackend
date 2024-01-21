using Microsoft.AspNetCore.Diagnostics;
using System.Data;

namespace SennheiserBackend
{
    /// <summary>
    /// Global handler for handling exceptions that occur during API calls.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Tries to handle an exception that occured during an API call.
        /// </summary>
        /// <param name="httpContext">Context of the request.</param>
        /// <param name="exception">Exception thrown.</param>
        /// <param name="cancellationToken">Cancellation token to allow canceliing the async operation.</param>
        /// <returns>True if exception could be handled, false otherwise.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = exception switch
            {
                NullReferenceException => 404,
                DuplicateNameException or ArgumentException => 400,
                _ => 500,
            };

            httpContext.Response.ContentType = "application/json";

            var errorInfo = new ErrorInfo
            {
                Message = exception.Message,
                ExceptionType = exception.GetType().Name
            };

            await httpContext.Response.WriteAsJsonAsync(errorInfo, CancellationToken.None);

            return true;
        }
    }
}
