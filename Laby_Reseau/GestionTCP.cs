using System.Collections.Generic;
using Laby_Interfaces;

namespace Laby_Reseau
{
    public enum MessageTCP
    {
        CONNECT = 0,
        DISCONNECT = 1,
        DATA = 2,
        ECHEC = 3
    };
    public class DataTCP
    {
        MessageTCP _message;
        object _data;
        public MessageTCP Message { get { return _message; } set { _message = value; } }
        public object Data { get { return _data; } set { _data = value; } }
        public DataTCP(MessageTCP message, object data)
        {
            _message = message;
            _data = data;
        }
    }

    class GestionTCP
    {
        int _port;
        ServerTCP _server;
        ClientTCP _client;

        public event DataReceive DataReceived;
        public event ClientConnected ClientConnected;

        public List<string> Clients { get { if (_server != null)  return _server.Clients; else return null; } }

        public GestionTCP(int port)
        {
            _port = port;
        }

        #region Partie Server
        public void CreationServer()
        {
            _server = new ServerTCP(_port);
            _server.DataReceived += GestionTCPDataReceived;
            _server.ClientConnected += GestionTCPClientConnected;
        }
        private void GestionTCPClientConnected(string ip)
        {
            ClientConnected(ip);
        }
        #endregion

        #region Partie Client
        public void CreationClient(string ipserver)
        {
            _client = new ClientTCP(_port);
            _client.DataReceived += GestionTCPDataReceived;
            if (_client.Connect(ipserver))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("GestionTCP.CreationClient : {0} : Connexion réussie...", ipserver));
                _client.SendData(_client.Nom);
            }
            else System.Diagnostics.Debug.WriteLine(string.Format("GestionTCP.CreationClient : {0} : Echec connexion...", ipserver));
        }
        #endregion

        private void GestionTCPDataReceived(string sender, object data)
        {
            DataReceived(sender, data);
        }

        public void SendData(object data)
        {
            if (_server != null) _server.SendDataClients(data);
            else
            {
                if (_client != null)_client.SendData(data);
                else System.Diagnostics.Debug.WriteLine("GestionTCP.SendData : Problème... ni client, ni server...");
            }
        }

        public void SendData(object data, string ipclient)
        {
            if (_server != null) _server.SendDataClient(data, ipclient);
        }
    }
}
