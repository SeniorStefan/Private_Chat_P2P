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
                System.Windows.MessageBox.Show("Ключи не найдены");
                Close();
                return;
            }

            myPublicKey = File.ReadAllText(publicPath);

            var pw = new PasswordWindows();
            if (pw.ShowDialog() == true)
            {
                myPrivateKey = RSAHelper.DecryptPrivateKey(
                    File.ReadAllText(privatePath),
                    pw.Password);
            }
            else
            {
                Close();
            }
        }

        private void StartServer()
        {
            server = new ChatServer(PORT, OnMessageReceived);
            server.Start();
        }

        private void RequestChat_Click(object sender, RoutedEventArgs e)
        {
            peerPublicKey = PeerPublicKeyBox.Text;
            ChatClient.Send(IpBox.Text, PORT, "CHAT_REQUEST|" + myPublicKey);
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
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

        private void OnMessageReceived(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                if (msg.StartsWith("CHAT_REQUEST|"))
                {
                    peerPublicKey = msg.Substring(13);
                    if (System.Windows.MessageBox.Show("Принять чат?", "Запрос",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        System.Windows.MessageBox.Show("Чат начат");
                    }
                }
                else if (msg.StartsWith("MESSAGE|"))
                {
                    string decrypted = RSAHelper.Decrypt(
                        msg.Substring(8), myPrivateKey);
                    ChatBox.AppendText("Собеседник: " + decrypted + "\n");
                }
            });
        }

        private void LoadPublicKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Выберите открытый ключ RSA",
                Filter = "RSA Public Key (*.xml)|*.xml",
                Multiselect = false
            };
            if (dialog.ShowDialog() != true) return;
            try
            {
                string keyXML = File.ReadAllText(dialog.FileName);
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(keyXML);
                }

                PeerPublicKeyBox.Text = keyXML;
                System.Windows.MessageBox.Show("Открытый ключ успешно загружен.", "Ключ загружен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                System.Windows.MessageBox.Show("Ошибка чтения файла:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CryptographicException ex)
            {
                System.Windows.MessageBox.Show("Файл не является корректным RSA-ключом", "Ошибка ключа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
