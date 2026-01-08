using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat_P2P
{
    internal class ChatClient
    {
        public static void Send(string ip, int port, string msg)
        {
            try
            {
                using (var client = new TcpClient(ip, port))
                {
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    client.GetStream().Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка отправки сообщения: " + ex.Message);
            }
        }
    }
}
