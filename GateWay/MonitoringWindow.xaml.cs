using GateWay.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace GateWay
{
    public partial class MonitoringWindow : Window
    {
        private readonly ObservableCollection<VariableViewModel> _variables = new();

        public MonitoringWindow()
        {
            InitializeComponent();
            dgVariables.ItemsSource = _variables;
        }

        public void AddVariables(string node, object valor)
        {
            Dispatcher.Invoke(() =>
            {
                var existente = _variables.FirstOrDefault(v => v.NodeId == node);

                if (existente != null)
                {
                    existente.Value = valor.ToString();
                    existente.Time = DateTime.Now;
                }
                else
                {
                    var nombre = node.Contains(".") ? node.Split('.').Last() : node;
                    _variables.Add(new VariableViewModel
                    {
                        Name = nombre,
                        NodeId = node,
                        Value = valor.ToString(),
                        Time = DateTime.Now
                    });
                }

                txtLog.Text += $"[{DateTime.Now:HH:mm:ss}] {node}: {valor}\n";
                txtLog.ScrollToEnd();
            });
        }
    }
}
