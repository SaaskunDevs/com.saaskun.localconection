using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

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

        void Start()
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            SendDiscoveryMessage();
            udpClient.BeginReceive(ReceiveCallback, null);
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

                // Intenta establecer una conexión TCP
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