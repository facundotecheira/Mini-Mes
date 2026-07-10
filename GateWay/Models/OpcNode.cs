using System.Collections.ObjectModel;

namespace GateWay.Models
{
    public class OpcNode
    {
        public string Name { get; set; }
        public string NodeId { get; set; }
        public string NodeClass { get; set; }
        public bool IsExpandable { get; set; }
        public bool HasChildren { get; set; }
        public ObservableCollection<OpcNode> Children { get; set; } = new();
        public bool IsSelected { get; set; }
    }
}
