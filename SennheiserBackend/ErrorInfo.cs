namespace SennheiserBackend
{
    /// <summary>
    /// Represents information about an error returned by an API request.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Message describing the error occured.
        /// </summary>
        public string Message { get; set; } = String.Empty;
        /// <summary>
        /// Type of the related exception.
        /// </summary>
        public string ExceptionType { get; set; } = String.Empty;
    }
}
