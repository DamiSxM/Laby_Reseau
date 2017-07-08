using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Laby_Reseau
{
    class GestionUDP
    {
        int _port;
        string _ipServer;
        bool _loopSendBroadcast = true;
        int _rechercheServerTimeout;
        int _sleepBetweenBroadcast;

        public int RechercheServerTimeout { get { return _rechercheServerTimeout; } set { _rechercheServerTimeout = value; } }
        public int SleepBetweenBroadcast { get { return _sleepBetweenBroadcast; } set { _sleepBetweenBroadcast = value; } }
        public bool LoopSendBroadcast { get { return _loopSendBroadcast; } set { _loopSendBroadcast = value; } }

        #region Events
        public delegate void ReturnUDP(string ipserver);
        public event ReturnUDP FinRechercheServer;
        public event ReturnUDP FinRechercheClients;
        void OnFinRechercheServer(string s) { if (FinRechercheServer != null) FinRechercheServer(s); }
        void OnFinRechercheClients(string s) { if (FinRechercheClients != null) FinRechercheClients(s); }
        #endregion

        public GestionUDP(int port)
        {
            _port = port;
            _rechercheServerTimeout = 5000;
            _sleepBetweenBroadcast = 500;
        }

        #region Recherche Server UDP
        public void RechercheServer() { new Thread(ThreadRechercheServer).Start(); }

        void ThreadRechercheServer()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("GestionUDP.ThreadRechercheServer : Recherche de serveur UDP..."));
            UdpClient client = new UdpClient(_port);
            client.Client.ReceiveTimeout = _rechercheServerTimeout;
            try
            {
                IPEndPoint toutLeMonde = new IPEndPoint(IPAddress.Any, _port);
                client.Receive(ref toutLeMonde);
                _ipServer = toutLeMonde.Address.ToString().Split(':')[0];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("GestionUDP.ThreadRechercheServer : Exception : {0}", ex));
            }
            finally
            {
                OnFinRechercheServer(_ipServer); // On renvoie l'ip du server
                client.Close();
            }
        }
        #endregion

        #region Création Server UDP
        Thread _thBroadcast;
        public void CreationServer(){
            _thBroadcast = new Thread(LoopEnvoiBroadcast);
            _thBroadcast.Start();
        }

        void LoopEnvoiBroadcast()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("GestionUDP.CreationServer : Création du serveur UDP..."));
            UdpClient server = new UdpClient();
            System.Diagnostics.Debug.WriteLine(string.Format("GestionUDP.CreationServer : début envoie broadcast UDP"));
            do
            {
                IPEndPoint broadcast = new IPEndPoint(IPAddress.Broadcast, _port);
                byte[] data = { byte.MinValue };
                server.Send(data, data.Length, broadcast);
                Thread.Sleep(_sleepBetweenBroadcast);
            } while (_loopSendBroadcast);

            System.Diagnostics.Debug.WriteLine(string.Format("GestionUDP.CreationServer : fin envoie broadcast UDP"));
            OnFinRechercheClients(null);
            server.Close();
        }
        #endregion

        public void Close()
        {
            _loopSendBroadcast = false;
            if(_thBroadcast != null) _thBroadcast.Join();
            System.Diagnostics.Debug.WriteLine(string.Format("GestionUDP.Close"));
        }
    }
}