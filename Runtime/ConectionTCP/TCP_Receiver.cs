using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TCPReceiver : MonoBehaviour
{
    public int discoveryPort = 5556;
    public string discoveryResponse = "UnityServerFound";

    private UdpClient udpClient;
    public MessageAction[] actions;
    void Start()
    {
        udpClient = new UdpClient(discoveryPort);
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref endPoint);
            string receivedMessage = Encoding.UTF8.GetString(receivedBytes);

            string senderIP = endPoint.Address.ToString();
            Debug.Log("Mensaje recibido de: " + senderIP + " Mensaje: " + receivedMessage);

            string[] split = receivedMessage.Split("|");
            ActionData(split[0], split[1]);


            if (receivedMessage == "UnityDiscovery")
            {
                SendResponse(endPoint);
            }

            // Aquí puedes añadir lógica para manejar otros mensajes
        }
        catch (Exception e)
        {
            Debug.LogError("Error al recibir mensaje: " + e.Message);
        }

        udpClient.BeginReceive(ReceiveCallback, null);
    }

    private void SendResponse(IPEndPoint endPoint)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(discoveryResponse);
            udpClient.Send(data, data.Length, endPoint);
        }
        catch (Exception e)
        {
            Debug.LogError("Error al enviar respuesta: " + e.Message);
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
    }

    [System.Serializable]
    public class MessageAction
    {
        public string code;
        public UnityEvent<string> action;
    }
}
