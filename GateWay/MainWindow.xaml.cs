using GateWay.Models;
using GateWay.Services;
using Opc.Ua;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GateWay
{
    public partial class MainWindow : Window
    {
        private readonly PlcService _plcService = new();
        private ObservableCollection<OpcNode> nodosRoot = new();
        private MonitoringWindow _monitoringWindow;

        public MainWindow()
        {
            InitializeComponent();
            _plcService.OnVariableChanged += CurrencyChangeHandler;
            tvNodos.ItemsSource = nodosRoot;
            tvNodos.ItemContainerGenerator.ItemsChanged += (s, e) =>
            {
                AddExpandedHandler();
            };
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
              btnConnect.IsEnabled = false;
              bool conectado = await _plcService.Connect(txtUrl.Text, txtPlcName.Text);
              if (conectado)
              {
                 btnDisconnect.IsEnabled = true;
                 await LoadRootNodesAsync();
              }   
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
           bool desconectado = _plcService.Disconnect();
           if (!desconectado)
            {
                btnConnect.IsEnabled = true;
                btnDisconnect.IsEnabled = false;
                ClearFields();
            }
        }

        private void ClearFields()
        {
            txtUrl.Text = string.Empty;
            txtPlcName.Text = string.Empty;
            txtFilterNode.Text = string.Empty;
            checkAllTree.IsChecked = false;
            nodosRoot.Clear();
        }

        private void AddExpandedHandler()
        {
            foreach (var item in tvNodos.Items)
            {
                var container = tvNodos.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container != null)
                {
                    container.Expanded -= TreeViewItem_Expanded;
                    container.Expanded += TreeViewItem_Expanded;
                    AddExpandedHandlerRecursive(container);
                }
            }
        }

        private void AddExpandedHandlerRecursive(TreeViewItem item)
        {
            foreach (var childItem in item.Items)
            {
                var container = item.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
                if (container != null)
                {
                    container.Expanded -= TreeViewItem_Expanded;
                    container.Expanded += TreeViewItem_Expanded;
                    AddExpandedHandlerRecursive(container);
                }
            }
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem item &&
                item.DataContext is OpcNode node)
            {
                LoadChildren(node);
            }
        }

        private async Task LoadRootNodesAsync()
        {
            try
            {
                var references = _plcService.BrowseNodeReference("ns=0;i=85"); 

                nodosRoot.Clear();

                foreach (var refDesc in references)
                {
                    var node = new OpcNode
                    {
                        Name = refDesc.DisplayName.Text,
                        NodeId = refDesc.NodeId.ToString(),
                        NodeClass = refDesc.NodeClass.ToString(),
                        IsExpandable = refDesc.NodeClass == NodeClass.Object ||
                                       refDesc.NodeClass == NodeClass.View,
                        HasChildren = true
                    };

                    nodosRoot.Add(node);
                }

                await Dispatcher.InvokeAsync(() => AddExpandedHandler());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando nodos: {ex.Message}");
            }
        }

        private void LoadChildren(OpcNode parentNode)
        {
            try
            {
                if (parentNode.Children.Count > 0) return;

                var references = _plcService.BrowseNodeReference(parentNode.NodeId);

                foreach (var refDesc in references)
                {
                    var childNode = new OpcNode
                    {
                        Name = refDesc.DisplayName.Text,
                        NodeId = refDesc.NodeId.ToString(),
                        NodeClass = refDesc.NodeClass.ToString(),
                        IsExpandable = refDesc.NodeClass == NodeClass.Object ||
                                       refDesc.NodeClass == NodeClass.View,
                        HasChildren = true
                    };

                    parentNode.Children.Add(childNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando hijos de {parentNode.Name}: {ex.Message}");
            }
        }

        private List<OpcNode> GetSelectedNodes(List<OpcNode> nodos)
        {
            var selected = new List<OpcNode>();

            foreach (var node in nodos)
            {
                if (node.IsSelected)
                    selected.Add(node);

                if (node.Children.Count > 0)
                    selected.AddRange(GetSelectedNodes(node.Children.ToList()));
            }

            return selected;
        }

        private void FilterNode_Click(object sender, RoutedEventArgs e)
        {
            var references = _plcService.BrowseNodeReference("ns=0;i=85");
            var nodeToFilter = txtFilterNode.Text;
            
            foreach (var refDesc in references)
            {
                var result = new List<ReferenceDescription>();

                if (nodeToFilter != string.Empty)
                {
                   var childrens = _plcService.BrowseNodeReference(refDesc.NodeId.ToString());
                   result = GetNodeFilter(childrens, [], nodeToFilter);
                   if (result.Count > 0) references = result.ToArray();
                }

               nodosRoot.Clear();

               foreach (var child in references)
               {
                    var node = new OpcNode
                    {
                        Name = child.DisplayName.Text,
                        NodeId = child.NodeId.ToString(),
                        NodeClass = child.NodeClass.ToString(),
                        IsExpandable = child.NodeClass == NodeClass.Object ||
                                       child.NodeClass == NodeClass.View,
                        HasChildren = true
                    };

                    nodosRoot.Add(node);
               }

                if (result.Count > 0) break;

            }
        }

        private List<ReferenceDescription> GetNodeFilter(ReferenceDescription[] nodes,  List<ReferenceDescription> result, string variableToFilter)
        {
            bool getOnlyNode = checkAllTree.IsChecked.Value; 

            if (result.Count > 0) return result;

            foreach (var node in nodes)
            {
                if (node.DisplayName.Text.ToLower().Contains(variableToFilter.ToLower()))
                {
                    result.Add(node);
                    if(!getOnlyNode) result = nodes.ToList();
                    break;
                }
                
                var childrens = _plcService.BrowseNodeReference(node.NodeId.ToString());
                result = GetNodeFilter(childrens, result, variableToFilter);
            }

            return result;
        }

        private void btnSuscribe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nodosSeleccionados = GetSelectedNodes(nodosRoot.ToList());
                var variablesSeleccionadas = nodosSeleccionados
                    .Where(n => !n.IsExpandable && n.IsSelected)
                    .ToList();

                if(variablesSeleccionadas.Count > 0)
                {
                    _plcService.AddSubscription(variablesSeleccionadas);
                    MessageBox.Show("Suscrpcion realizada correctamente");
                }
                else
                {
                    MessageBox.Show("Debe Seleccionar algun nodo antes de suscribir");
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Hubo un error al intentar suscribirce al Plc {ex.Message}");
            }
        }

        private void btnShowMonitoringWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_monitoringWindow == null || !_monitoringWindow.IsVisible)
            {
                _monitoringWindow = new MonitoringWindow();
                _monitoringWindow.Closed += (s, e) => _monitoringWindow = null;
                _monitoringWindow.Show();
            }
            else
            {
                _monitoringWindow.Activate();
            }
        }

        private void CurrencyChangeHandler(object? sender, PlcVariableEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_monitoringWindow != null && _monitoringWindow.IsVisible)
                {
                    _monitoringWindow.AddVariables(e.Node, e.Value);
                }
            });
        }
    }
}
