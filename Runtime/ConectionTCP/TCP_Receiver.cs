using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class TCPReceiver : MonoBehaviour
{
    public int listenPort = 5556; // Puerto en el que escuchará
    private TcpListener tcpListener;
    private TcpClient connectedClient;

    public MessageAction[] actions;

    void Start()
    {
        tcpListener = new TcpListener(IPAddress.Any, listenPort);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
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
