namespace GateWay.Models
{
    public class PlcVariableEventArgs : EventArgs
    {
        public string Node { get; set; } = string.Empty;
        public object Value { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
}
