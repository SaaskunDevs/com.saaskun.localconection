using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;
using System;

namespace Saaskun
{
    public class UDP_Receiver : MonoBehaviour
    {
        private UdpClient udpClient;
        private bool isRunning = true;
        private Thread receiveThread;
        public int port = 8080;

        public MessageAction[] actions;

        private ManualResetEvent quitEvent = new ManualResetEvent(false);

        private void Start()
        {
            try
            {
                udpClient = new UdpClient(port);
                Debug.Log($"Servidor UDP iniciado en el puerto {port}");
                receiveThread = new Thread(new ThreadStart(ServerThread));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error al iniciar el servidor UDP: {ex.Message}");
            }
        }

        private void ServerThread()
        {
            try
            {
                while (isRunning)
                {
                    if (quitEvent.WaitOne(0))
                        break;

                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    string dataString = Encoding.UTF8.GetString(data);
                    string[] split = dataString.Split('|');

                    if (split.Length >= 2)
                    {
                        CheckMessageData(split[0], split[1]);
                    }
                    else
                    {
                        Debug.LogWarning("Mensaje recibido con formato incorrecto.");
                    }
                }
            }
            catch (SocketException ex)
            {
                if (isRunning)
                {
                    Debug.LogError($"Error en el servidor UDP: {ex.Message}");
                }
            }
            catch (ObjectDisposedException)
            {
                // El UdpClient fue cerrado, lo cual es esperado al deshabilitar
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error inesperado en el servidor UDP: {ex.Message}");
            }
        }

        private void CheckMessageData(string code, string message)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ActionData(code, message);
            });
        }

        private void ActionData(string code, string message)
        {
            foreach (var action in actions)
            {
                if (code == action.code)
                {
                    action.action.Invoke(message);
                }
            }
        }

        private void OnDisable()
        {
            isRunning = false;
            quitEvent.Set();

            if (udpClient != null)
            {
                udpClient.Close(); // Cierra el UdpClient primero para desbloquear Receive
            }

            if (receiveThread != null && receiveThread.IsAlive)
            {
                // Intenta unir el hilo con un tiempo de espera para evitar congelamientos
                if (!receiveThread.Join(2000)) // Espera hasta 2 segundos
                {
                    Debug.LogWarning("El hilo de recepción no terminó en el tiempo esperado.");
                    // Opcional: Puedes abortar el hilo si es necesario, aunque no es recomendado
                    // receiveThread.Abort();
                }
            }
        }

        [System.Serializable]
        public class MessageAction
        {
            public string code;
            public UnityEvent<string> action;
        }
    }
}
