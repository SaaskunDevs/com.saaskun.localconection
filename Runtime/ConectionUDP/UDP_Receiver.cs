using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;

namespace Saaskun
{
    public class UDP_Receiver : MonoBehaviour
    {
        UdpClient udpClient;
        private bool isRunning = true;
        Thread t;
        public int port = 8080;

        public MessageAction[] actions;

        private void Start()
        {
            udpClient = new UdpClient(port);
            Debug.Log("Servidor UDP iniciado en el puerto " + port);
            t = new Thread(new ThreadStart(ServerThread));
            t.Start();
        }

        void ServerThread()
        {
            while (isRunning)
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string dataString = Encoding.UTF8.GetString(data);
                string[] split = dataString.Split("|");
                CheckMessageData(split[0], split[1]);
            }
        }

        void CheckMessageData(string code, string message)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ActionData(code, message);
            });

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

        private void OnDisable()
        {
            if(t != null)
                t.Abort();
            isRunning = false;
            if(udpClient != null)
                udpClient.Close();
        }

        [System.Serializable]
        public class MessageAction
        {
            public string code;
            public UnityEvent<string> action;
        }
    }
}
