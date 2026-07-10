using GateWay.Models;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaHelper;
using System.Windows;

namespace GateWay.Services
{
    public class PlcService
    {
        OpcUaClient opcUaClient = new OpcUaClient();

        public event EventHandler<PlcVariableEventArgs> OnVariableChanged;

        public PlcService()
        {
            opcUaClient.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
        }

        public async Task<bool> Connect(string url, string plcName)
        {
            bool flag = false;
            try
            {
                if (opcUaClient.Connected)
                    return true;
                
                opcUaClient.OpcUaName = plcName;
                await opcUaClient.ConnectServer(url);
                if (opcUaClient.Connected)
                {
                    MessageBox.Show($"Conectado al PLC:  {opcUaClient.OpcUaName}", "Gateway");
                    flag = true;
                }

                return flag;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hubo un error con la conexion: {ex.Message}", "Gateway");
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                string PlcName = opcUaClient.OpcUaName;
                opcUaClient.Disconnect();
                MessageBox.Show($"Se ah desconectado del PLC:  {PlcName}", "Gateway");
                return opcUaClient.Connected;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hubo un error al intentar desconectar: {ex.Message}", "Gateway");
                return opcUaClient.Connected;
            }
        }

        public ReferenceDescription[] BrowseNodeReference(string tag)
        {
            return opcUaClient.BrowseNodeReference(tag);
        }

        public void AddSubscription(List<OpcNode> nodes)
        {
            try
            {
                nodes.ForEach(n =>
                { 
                    opcUaClient.AddSubscription(n.Name, n.NodeId.ToString(), SubsCallBack);

                });
            }
            catch (Exception ex) { }
        }

        private void SubsCallBack(string key, Opc.Ua.Client.MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            MonitoredItemNotification notif = e.NotificationValue as MonitoredItemNotification;

            if (notif != null)
            {
                OnVariableChanged?.Invoke(this, new PlcVariableEventArgs
                {
                    Node = monitoredItem.DisplayName,
                    Value = notif.Value.WrappedValue.ToString(),
                    Timestamp = DateTime.Now
                });

            }
        }
    }
}