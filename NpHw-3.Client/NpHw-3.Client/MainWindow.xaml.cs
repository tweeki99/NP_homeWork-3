using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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

namespace NpHw_3.Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        static string userName;
        private const string host =/* "10.3.3.39"*/"192.168.1.68";
        private const int port = 12345;
        static TcpClient client;
        static NetworkStream stream;
        List<Client> clientsObjects = new List<Client>();

        public MainWindow()
        {
            InitializeComponent();
            Closing += WindowClosing;

            userName = UserNameWindow();
            client = new TcpClient();
            try
            {
                client.Connect(host, port);
                stream = client.GetStream();

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
              
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        static string UserNameWindow()
        {
            NameWindow nameWindow = new NameWindow();

            if (nameWindow.ShowDialog() == true)
            {
                return nameWindow.UserName;
            }
            else
            {
                return UserNameWindow();
            }
        }
        
        public void SendMessage(string attribute)
        {
                string message = attribute + (char)1 + messageTextBox.Text;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
        }
    
        public void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    
                    string message = builder.ToString();
                    string[] substrings = message.Split(new char[] { (char)1 }, StringSplitOptions.RemoveEmptyEntries);

                    if (substrings[0] == Attribute.Message.ToString())
                    {
                        Dispatcher.Invoke(new ThreadStart(() => chatTextBox.Text += substrings[1] + "\n"));
                    }
                    else if (substrings[0] == Attribute.ListClients.ToString())
                    {
                        int index = substrings[1].LastIndexOf(']');
                        substrings[1] = substrings[1].Substring(0, index+1);
                        clientsObjects = JsonConvert.DeserializeObject<List<Client>>(substrings[1]);
                        Dispatcher.Invoke(new ThreadStart(() => onlineUsersComboBox.Items.Clear()));
                        
                        foreach (var clientsObject in clientsObjects)
                        {
                            Dispatcher.Invoke(new ThreadStart(() => onlineUsersComboBox.Items.Add(String.Format("{0} {1}", clientsObject.UserName, clientsObject.Id)))); 
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Подключение прервано!\n"+ ex.Message);
                    Disconnect();
                }
            }
        }

        private void Disconnect()
        {
            SendMessage(Attribute.Disconnect.ToString());
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }

        public void WindowClosing(object sender, CancelEventArgs e)
        {
            Disconnect();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(messageTextBox.Text.Length > 0)
            SendMessage(Attribute.Message.ToString());
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            string[] substrings = onlineUsersComboBox.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if(substrings.Count() > 1 && messageTextBox.Text.Length > 0)
            SendMessage(Attribute.PrivateMessage.ToString() + (char)1 + substrings[1]);
        }
    }
}
