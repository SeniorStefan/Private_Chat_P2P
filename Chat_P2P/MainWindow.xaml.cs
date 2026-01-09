using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace Chat_P2P
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string appFolder =
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MySecureChat");

        private string myPrivateKey;
        private string myPublicKey;
        private string peerPublicKey;

        private ChatServer server;
        private const int PORT = 5000;

        public MainWindow()
        {
            InitializeComponent();
            LoadKeys();

            StartServer();
        }

        private void LoadKeys()
        {
            string publicPath = System.IO.Path.Combine(appFolder, "publicKey.xml");
            string privatePath = System.IO.Path.Combine(appFolder, "privateKey.enc");

            if (!File.Exists(publicPath) || !File.Exists(privatePath))
            {
                System.Windows.MessageBox.Show("Ключи не найдены, сгенерируйте ключи с помощью ПО Generat Key");
                Close();
                return;
            }

            myPublicKey = File.ReadAllText(publicPath);
            bool resultTest = false;
            do
            {
                var pw = new PasswordWindows();
                if (pw.ShowDialog() == true)
                {
                    myPrivateKey = RSAHelper.DecryptPrivateKey(
                        File.ReadAllText(privatePath),
                        pw.Password);
                    if (myPrivateKey != "false")
                    {
                        resultTest = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Ошибка при вводе пароля, попробуйте снова.");
                    }
                }
                else
                {
                    System.Environment.Exit(0);
                }
            } while (!resultTest);
        }

        private void StartServer()
        {
            server = new ChatServer(PORT, OnMessageReceived);
            server.Start();
        }

        private void RequestChat_Click(object sender, RoutedEventArgs e)
        {
            RequestChat();
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMesssage();
        }
        private void RequestChat()
        {
            peerPublicKey = PeerPublicKeyBox.Text;
            ChatClient.Send(IpBox.Text, PORT, "CHAT_REQUEST|" + myPublicKey);
        }

        private void SendMesssage()
        {
            if (MessageBox.Text == "") return;
            if (string.IsNullOrEmpty(peerPublicKey))
            {
                System.Windows.MessageBox.Show("Нет открытого ключа собеседника");
                return;
            }

            string encrypted = RSAHelper.Encrypt(MessageBox.Text, peerPublicKey);
            ChatClient.Send(IpBox.Text, PORT, "MESSAGE|" + encrypted);

            ChatBox.AppendText("Я: " + MessageBox.Text + "\n");
            MessageBox.Clear();
        }

        private void OnMessageReceived(string msg, string senderIp)
        {
            Dispatcher.Invoke(() =>
            {
                if (msg.StartsWith("CHAT_REQUEST|"))
                {
                    if (PeerPublicKeyBox.Text != "") return;
                    peerPublicKey = msg.Substring(13);
                    if (System.Windows.MessageBox.Show("Принять чат?", "Запрос",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        System.Windows.MessageBox.Show("Чат начат");
                        PeerPublicKeyBox.Text = peerPublicKey;
                        IpBox.Text = senderIp;
                    }
                }
                else if (msg.StartsWith("MESSAGE|"))
                {
                    string decrypted = RSAHelper.Decrypt(
                        msg.Substring(8), myPrivateKey);
                    ChatBox.AppendText("Собеседник: " + decrypted + "\n");
                    ChatBox.ScrollToEnd();
                }
            });
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter: SendMesssage();
                    break;
            }
        }
    }
}
