namespace SennheiserBackend.Extensions
{
    /// <summary>
    /// Defines a change in a comparison result.
    /// </summary>
    public class ValueChange
    {
        public string Name { get; set; } = "";
        public object? ValueA { get; set; }
        public object? ValueB { get; set; }

    }
}
