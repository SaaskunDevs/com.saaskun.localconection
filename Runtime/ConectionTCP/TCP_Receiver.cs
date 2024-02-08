using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Saaskun
{
    public class TCPReceiver : MonoBehaviour
    {
        public int discoveryPort = 5555; // Puerto para descubrimiento UDP
        public int listenPort = 5556; // Puerto para comunicación TCP

        private UdpClient udpClient; // Cliente UDP para descubrimiento
        private TcpListener tcpListener; // Listener TCP para comunicación de datos
        private TcpClient connectedClient;

        public MessageAction[] actions;

        void Start()
        {
            // Iniciar listener UDP para descubrimiento
            udpClient = new UdpClient(discoveryPort);
            udpClient.BeginReceive(OnUdpReceive, null);

            // Iniciar listener TCP para comunicación de datos
            tcpListener = new TcpListener(IPAddress.Any, listenPort);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
        }

        private void OnUdpReceive(IAsyncResult ar)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, discoveryPort);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);

            string receivedMessage = Encoding.UTF8.GetString(receivedBytes);
            Debug.Log("Mensaje UDP recibido: " + receivedMessage);

            if (receivedMessage == "UnityDiscovery")
            {
                SendUdpResponse(endPoint);
            }

            udpClient.BeginReceive(OnUdpReceive, null);
        }

        private void SendUdpResponse(IPEndPoint endPoint)
        {
            string response = "TCP:" + listenPort; // Respuesta con el puerto TCP
            byte[] data = Encoding.UTF8.GetBytes(response);
            udpClient.Send(data, data.Length, endPoint);
        }

        private void OnClientConnect(IAsyncResult ar)
        {
            try
            {
                connectedClient = tcpListener.EndAcceptTcpClient(ar);
                Debug.Log("Cliente conectado.");

                NetworkStream stream = connectedClient.GetStream();
                byte[] buffer = new byte[connectedClient.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log("Mensaje recibido: " + receivedMessage);

                    string[] split = receivedMessage.Split("|");
                    ActionData(split[0], split[1]);

                    // Continúa escuchando más clientes
                    tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error en conexión TCP: " + e.Message);
            }
        }

        void ActionData(string code, string message)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (code == actions[i].code)
                {
                    actions[i].action.Invoke(message);
                }
            }
        }

        private void OnDestroy()
        {
            if (udpClient != null)
                udpClient.Close();
            if (tcpListener != null)
                tcpListener.Stop();
            if (connectedClient != null)
                connectedClient.Close();
        }

        [System.Serializable]
        public class MessageAction
        {
            public string code;
            public UnityEvent<string> action;
        }
    }
}
