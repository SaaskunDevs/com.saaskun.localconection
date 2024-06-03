using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Saaskun
{
    public class UDP_Sender : MonoBehaviour
    {
        UdpClient udpClient;

        private void Start()
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;  // Habilitar el envío de mensajes de difusión
        }

        public void SendData(string code, string message, int port)
        {
            // Puedes enviar mensajes a la dirección de difusión (255.255.255.255) en el puerto deseado
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, port);

            byte[] data = Encoding.UTF8.GetBytes(code + "|" + message + "|");
            udpClient.Send(data, data.Length, remoteEndPoint);
        }

        public void SendDataCustomIP(string code, string message, string ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);

            byte[] data = Encoding.UTF8.GetBytes(code + "|" + message + "|");
            udpClient.Send(data, data.Length, remoteEndPoint);
        }

        private void OnDisable()
        {
            udpClient.Close();
        }
    }
}
