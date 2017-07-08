using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Laby_Interfaces;

namespace Laby_Reseau
{
    class ServerTCP
    {
        int _port;
        private Hashtable _clients = new Hashtable();
        private TcpListener _listener;
        private Thread _threadEcoute;

        public List<string> Clients { get { return _clients.Keys.OfType<string>().ToList(); } }

        public event DataReceive DataReceived;
        public event ClientConnected ClientConnected;

        public ServerTCP(int port)
        {
            _port = port;
            _threadEcoute = new Thread(new ThreadStart(Ecoute));
            _threadEcoute.Start();
        }

        bool _ecouteLoop;
        private void Ecoute()
        {
            _ecouteLoop = true;
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                do
                {
                    try
                    {
                        ConnexionClient client = new ConnexionClient(_listener.AcceptTcpClient());
                        client.DataReceived += OnDataReceived;
                        _clients.Add(client.Nom, client);       // TEST !
                        System.Diagnostics.Debug.WriteLine(string.Format("ServerTCP.Ecoute : création client {0}", client.Nom));
                        ClientConnected(client.Nom); // Event ClientConnected

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("ServerTCP.Ecoute : Exception : echec création client : {0}", ex.Message));
                        _ecouteLoop = false;
                    }
                } while (_ecouteLoop);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("ServerTCP.Ecoute : Exception : erreur Ecoute server : {0}", ex.Message));
            }
        }

        private void OnDataReceived(ConnexionClient sender, object data)
        {
            /*if (data.ToString() == sender.Nom) ConnectClient(sender, (string)data);
            else DataReceived(sender.Nom, data);*/
            DataReceived(sender.Nom, data);
        }

        /*void ConnectClient(ConnexionClient client, string clientNom)
        {
            if (_clients.Contains(clientNom))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("ServerTCP.ConnectClient : Contient déjà le client {0}", clientNom));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(string.Format("ServerTCP.ConnectClient : Ne contient pas le client {0}, création client", clientNom));
                client.Nom = clientNom;
                _clients.Add(clientNom, client);
                ClientConnected(clientNom); // Event ClientConnected
            }
        }*/

        private void SendToClients(ConnexionClient client, object data)
        {
            client.SendData(data);
        }

        private void ReplyToClient(ConnexionClient client, object data)
        {
            ConnexionClient c;
            foreach (DictionaryEntry entry in _clients)
            {
                c = (ConnexionClient)entry.Value;
                if (client.Nom != c.Nom)
                {
                    client.SendData(data);
                }
            }
        }

        void DisconnectClient(ConnexionClient client)
        {
            _clients.Remove(client.Nom);
            System.Diagnostics.Debug.WriteLine(string.Format("ServerTCP.DisconnectClient : déco du client {0}", client.Nom));
        }

        void Fermeture()
        {
            _listener.Stop();
        }

        public void SendDataClients(object data)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("ServerTCP.SendDataClients : server -> clients : nb clients : {0}", _clients.Count));
            foreach (DictionaryEntry entry in _clients)
            {
                ((ConnexionClient)entry.Value).SendData(data);
            }
        }

        public void SendDataClient(object data, string clientname)
        {
            foreach (DictionaryEntry entry in _clients)
            {
                if (entry.Key.ToString() == clientname)
                {
                    ((ConnexionClient)entry.Value).SendData(data);
                }
            }
        }

        public void Close()
        {
            foreach (DictionaryEntry entry in _clients)
            {
                ConnexionClient client = ((ConnexionClient)entry.Value);
                client.DataReceived -= OnDataReceived;
                client.Close();
            }
            _clients.Clear();

            _ecouteLoop = false;
            _listener.Stop();
            System.Diagnostics.Debug.WriteLine(string.Format("ServerTCP.Close"));
        }
    }
}