using System;
using System.Collections.Generic;
using System.Net;
using Laby_Interfaces;

namespace Laby_Reseau
{
    public enum Etat
    {
        SERVER, CLIENT
    };
    public class Reseau : ILiaison
    {
        string _ipServer;
        int _port;
        int _maxPlayer;
        bool _isFinRechercheServer;

        GestionUDP _gestionUDP;
        GestionTCP _gestionTCP;

        /*public bool IsServer { get { return _ipServer == IPAddress.Loopback.ToString(); } }
        public List<string> Clients { get { return _gestionTCP.Clients; } }*/

        public List<string> GetClientsIP() { return _gestionTCP.Clients; }
        public int GetClientsCount() { return _gestionTCP.Clients.Count; }
        public bool IsServer() { return _ipServer == IPAddress.Loopback.ToString(); }

        public event DataReceive DataReceived;
        public event ClientConnected ClientConnected;
        public event RechercheServer FinRechercheServer;
        void OnDataReceived(string sender, object data) { if (DataReceived != null) DataReceived(sender, data); }
        void OnClientConnected(string ip) { if (ClientConnected != null) ClientConnected(ip); }
        void OnFinRechercheServer(bool isServer) { if (FinRechercheServer != null) FinRechercheServer(isServer); }

        public Reseau()
        {
            _port = 1234;
            _maxPlayer = 4;
            Initialize();
            RechercheServer();
        }

        public Reseau(Etat init)
        {
            _port = 1234;
            _maxPlayer = 4;
            Initialize();
            switch (init)
            {
                case Etat.CLIENT: RechercheServer(); break;
                case Etat.SERVER: UDP_FinRechercheServer(null); break;
            }
        }

        void Initialize()
        {
            _gestionUDP = new GestionUDP(_port);
            _gestionUDP.FinRechercheServer += UDP_FinRechercheServer; ;
            _gestionTCP = new GestionTCP(_port);
            _gestionTCP.ClientConnected += TCP_ClientConnected;
            _gestionTCP.DataReceived += TCP_DataReceived;
        }

        private void TCP_ClientConnected(string ip)
        {
            OnClientConnected(ip);
        }

        private void TCP_DataReceived(string sender, object data)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Reseau.TCP_DataReceived : {0} : Réception data TCP : {1}", sender, data));
            OnDataReceived(sender, data); // Faire des trucs..
        }

        public bool IsFinRechercheServer() { return _isFinRechercheServer; }
        private void UDP_FinRechercheServer(string ipserver)
        {
            _isFinRechercheServer = true;
            if (ipserver != null) // Il y a déjà un server
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Reseau.UDP_FinRechercheServer : {0} : server trouvé  ! création client TCP !", ipserver));
                _ipServer = ipserver;
                _gestionTCP.CreationClient(_ipServer); // Création TCP Client
            }
            else // Pas de server, création server
            {
                CreationServer(); // Création TCP Listener
                System.Diagnostics.Debug.WriteLine(string.Format("Reseau.UDP_FinRechercheServer : {0} : server introuvable  ! création server TCP !", ipserver));
            }
            OnFinRechercheServer(IsServer());
        }

        public void RechercheServer() { _gestionUDP.RechercheServer(); }

        public void CreationServer()
        {
            _ipServer = IPAddress.Loopback.ToString();
            _gestionTCP.CreationServer();
            _gestionUDP.CreationServer();
        }

        public void SendData(object data)
        {
            _gestionTCP.SendData(data);
        }
        public void SendDataTo(object data, string ipclient)
        {
            _gestionTCP.SendData(data, ipclient);
        }

        public void stopLoop(string s)
        {
            _gestionUDP.LoopSendBroadcast = false;
        }

        public void Close()
        {
            _gestionUDP.Close();
            _gestionTCP.Close();
            System.Diagnostics.Debug.WriteLine(string.Format("Reseau.Close"));
        }
    }
}