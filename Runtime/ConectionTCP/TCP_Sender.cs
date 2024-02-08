using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Saaskun
{
    public class TCPSender : MonoBehaviour
    {
        public int discoveryPort = 5555;
        public int tcpPort = 5556;
        public string discoveryMessage = "UnityDiscovery";

        private UdpClient udpClient;
        private List<IPEndPoint> discoveredEndpoints = new List<IPEndPoint>();
        private List<TcpClient> tcpClients = new List<TcpClient>();

        private bool searching = true;

        #region Delegates
        public delegate void ConnectionEventHandler();
        public event ConnectionEventHandler OnConnected;

        public delegate void MessageErrorEventHandler(string exeption);
        public event MessageErrorEventHandler OnSendError;
        #endregion

        void Start()
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            StartCoroutine(Searching());
            udpClient.BeginReceive(ReceiveCallback, null);
        }

        IEnumerator Searching()
        {
            while(searching)
            {
                SendDiscoveryMessage();
                yield return new WaitForSeconds(0.5f);

            }
        }

        private void SendDiscoveryMessage()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
                byte[] data = Encoding.UTF8.GetBytes(discoveryMessage);
                udpClient.Send(data, data.Length, endPoint);
            }
            catch (Exception e)
            {
                Debug.LogError("Error al enviar mensaje de descubrimiento: " + e.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);

            if (!discoveredEndpoints.Contains(endPoint))
            {
                discoveredEndpoints.Add(endPoint);
                Debug.Log("Nuevo servidor descubierto: " + endPoint.Address.ToString());

                searching = false;
                ConnectTCP(endPoint.Address);
            }

            udpClient.BeginReceive(ReceiveCallback, null);
        }

        private void ConnectTCP(IPAddress serverIP)
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(serverIP, tcpPort);
                tcpClients.Add(tcpClient);
                Debug.Log("Conexión TCP establecida con " + serverIP.ToString());
                OnConnected?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError("Error al conectar con servidor TCP: " + e.Message);
            }
        }

        public void SendMessageToServers(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (TcpClient tcpClient in tcpClients)
            {
                try
                {
                    NetworkStream stream = tcpClient.GetStream();
                    if (stream.CanWrite)
                    {
                        stream.Write(data, 0, data.Length);
                        Debug.Log("Mensaje TCP enviado a " + tcpClient.Client.RemoteEndPoint.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error al enviar mensaje TCP: " + e.Message);
                    OnSendError?.Invoke(e.Message);
                }
            }
        }

        private void OnDestroy()
        {
            if (udpClient != null)
                udpClient.Close();

            foreach (TcpClient tcpClient in tcpClients)
            {
                if (tcpClient != null)
                    tcpClient.Close();
            }
        }
    }
}