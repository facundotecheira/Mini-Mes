using System.ComponentModel;

namespace GateWay.Models
{
    public class VariableViewModel : INotifyPropertyChanged
    {
        private string _value;
        private DateTime _time;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public string NodeId { get; set; }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public DateTime Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}