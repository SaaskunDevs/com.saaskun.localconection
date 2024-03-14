using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnDataReceived), null);
        }

        private void OnUdpReceive(IAsyncResult ar)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, discoveryPort);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);

            string receivedMessage = Encoding.UTF8.GetString(receivedBytes);
            Debug.Log("Mensaje UDP recibido: " + receivedMessage + " ip: " + endPoint.Address);

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

        private void OnDataReceived(IAsyncResult ar)
        {
            try
            {
                TcpClient client = tcpListener.EndAcceptTcpClient(ar);

                // Continúa escuchando más clientes
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnDataReceived), null);

                // Maneja la comunicación con el cliente en un hilo separado
                ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClientComm), client);
            }
            catch (Exception e)
            {
                Debug.LogError("Error en conexión TCP: " + e.Message);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream stream = tcpClient.GetStream();

            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            int bytesRead; 

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log("Mensaje recibido: " + receivedMessage);

                    string[] split = receivedMessage.Split('|');
                    if (split.Length >= 2)
                    {
                        ActionData(split[0], split[1]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error en cliente TCP: " + e.Message);
                // Manéjalo como creas conveniente
            }
            finally
            {
                tcpClient.Close();
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
