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

        GestionUDP _gestionUDP;
        GestionTCP _gestionTCP;

        public bool IsServer { get { return _ipServer == IPAddress.Loopback.ToString(); } }
        public List<string> Clients { get { return _gestionTCP.Clients; } }

        public event DataReceive DataReceived;
        public event RechercheServer FinRechercheServer;
        private void OnDataReceived(string sender, object data) { if (DataReceived != null) DataReceived(sender, data); }
        private void OnFinRechercheServer() { if (DataReceived != null) FinRechercheServer(IsServer); }

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
                case Etat.SERVER: CreationServer(); break;
                case Etat.CLIENT: RechercheServer(); break;
            }
        }

        void Initialize()
        {
            _gestionUDP = new GestionUDP(_port);
            _gestionUDP.FinRechercheServer += UDP_FinRechercheServer; ;
            _gestionTCP = new GestionTCP(_port);
            _gestionTCP.DataReceived += TCP_DataReceived;
        }

        private void TCP_DataReceived(string sender, object data)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Reseau.TCP_DataReceived : {0} : Réception data TCP : {1}", sender, data));
            OnDataReceived(sender, data); // Faire des trucs..
        }

        private void UDP_FinRechercheServer(string ipserver)
        {
            if (ipserver != null) // Il y a déjà un server
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Reseau.UDP_FinRechercheServer : {0} : server trouvé  ! création client TCP !", ipserver));
                _ipServer = ipserver;
                _gestionTCP.CreationClient(_ipServer); // Création TCP Client
            }
            else // Pas de server, création server
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Reseau.UDP_FinRechercheServer : {0} : server introuvable  ! création server TCP !", ipserver));
                CreationServer(); // Création TCP Listener
            }
            OnFinRechercheServer();
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
        public void SendData(object data, string ipclient)
        {
            _gestionTCP.SendData(data, ipclient);
        }

        public void stopLoop(string s)
        {
            _gestionUDP.LoopSendBroadcast = false;
        }
    }
}
