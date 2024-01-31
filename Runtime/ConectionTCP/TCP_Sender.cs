using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class TCPSender : MonoBehaviour
{
    public int discoveryPort = 5556;
    public string discoveryMessage = "UnityDiscovery";

    private UdpClient udpClient;
    private List<IPEndPoint> clientEndpoints = new List<IPEndPoint>();

    void Start()
    {
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;

        SendDiscoveryMessage();

        // Comienza a escuchar respuestas
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            SendMessageToClients("adaw");
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

        if (!clientEndpoints.Contains(endPoint))
        {
            clientEndpoints.Add(endPoint);
            Debug.Log("Nuevo cliente descubierto: " + endPoint.Address.ToString());
        }

        // Volver a escuchar para más mensajes
        udpClient.BeginReceive(ReceiveCallback, null);
    }

    public void SendMessageToClients(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        foreach (IPEndPoint client in clientEndpoints)
        {
            try
            {
                udpClient.Send(data, data.Length, client);
                Debug.Log($"Mensaje enviado a {client.Address.ToString()}");
            }
            catch (Exception e)
            {
                Debug.LogError("Error al enviar mensaje al cliente: " + e.Message);
            }
        }
    }

    private void OnDestroy()
    {
        if (udpClient != null)
            udpClient.Close();
    }
}
