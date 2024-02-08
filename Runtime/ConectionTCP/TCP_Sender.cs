using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TCPSender : MonoBehaviour
{
    public string serverIP; // IP del servidor
    public int serverPort = 5556; // Puerto del servidor

    private TcpClient tcpClient;

    void Start()
    {
        string localIP = GetLocalIPAddress();
        Debug.Log("La dirección IP local del sender es: " + localIP);

        try
        {
            // Establecer conexión TCP
            tcpClient = new TcpClient(serverIP, serverPort);
            Debug.Log("Conexión TCP establecida con el servidor");
        }
        catch (Exception e)
        {
            Debug.LogError("Error al conectar con el servidor: " + e.Message);
        }
    }

    public void SendMessageToServer(string message)
    {
        if (tcpClient == null)
        {
            Debug.LogError("Cliente TCP no está conectado al servidor");
            return;
        }

        try
        {
            NetworkStream stream = tcpClient.GetStream();
            if (stream.CanWrite)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Debug.Log("Mensaje enviado al servidor: " + message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error al enviar mensaje al servidor: " + e.Message);
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void OnDestroy()
    {
        if (tcpClient != null)
        {
            tcpClient.Close();
        }
    }
}
