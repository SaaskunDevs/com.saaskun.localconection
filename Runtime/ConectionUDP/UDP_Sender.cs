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
            udpClient.EnableBroadcast = true;  // Habilitar el env�o de mensajes de difusi�n
        }

        public void SendData(string code, string message, int port)
        {
            // Puedes enviar mensajes a la direcci�n de difusi�n (255.255.255.255) en el puerto deseado
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, port);

            byte[] data = Encoding.UTF8.GetBytes(code + "|" + message + "|");
            udpClient.Send(data, data.Length, remoteEndPoint);
        }

        private void OnDisable()
        {
            udpClient.Close();
        }
    }
}