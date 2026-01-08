using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat_P2P
{
    internal class ChatServer
    {
        private TcpListener listener;
        private System.Action<string> callback;

        public ChatServer(int port, System.Action<string> onMessage)
        {
            listener = new TcpListener(IPAddress.Any, port);
            callback = onMessage;
        }

        public void Start()
        {
            listener.Start();
            new Thread(ListenLoop) { IsBackground = true }.Start();
        }

        private void ListenLoop()
        {
            while (true)
            {
                var client = listener.AcceptTcpClient();
                var stream = client.GetStream();
                byte[] buffer = new byte[4096];
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read > 0)
                    callback(Encoding.UTF8.GetString(buffer, 0, read));
                client.Close();
            }
        }
    }
}
