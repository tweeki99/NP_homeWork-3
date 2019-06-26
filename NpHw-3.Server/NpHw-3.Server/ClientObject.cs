using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NpHw_3.Server
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        protected internal string userName { get; private set; }
        TcpClient client;
        ServerObject server;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
               
                string message = GetMessage();
                userName = message;
                server.AddConnection(this);
                Thread.Sleep(10);
                message = userName + " вошел в чат";

                server.BroadcastMessage(Attribute.Message.ToString() + (char)1 + message, this.Id);
                Console.WriteLine(message);

                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        string[] substrings = message.Split(new char[] { (char)1 }, StringSplitOptions.RemoveEmptyEntries);

                        if (substrings[0] == Attribute.Message.ToString())
                        {
                            message = String.Format("{0}: {1}", userName, substrings[1]);
                            Console.WriteLine(message);
                            message = Attribute.Message.ToString() + (char)1 + message;

                            server.BroadcastMessage(message, this.Id);
                        }
                        else if (substrings[0] == Attribute.PrivateMessage.ToString())
                        {
                            Console.WriteLine(server.PrivateMessage(substrings[2], this ,substrings[1]));
                            
                        }
                        else if (substrings[0] == Attribute.Disconnect.ToString())
                        {
                            Console.WriteLine(this.userName + " " + substrings[0]);
                            server.RemoveConnection(this.Id);
                        }
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        message = Attribute.Message.ToString() + (char)1 + message;
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.Id);
                Close();
            }
        }
        
        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);
            
            return builder.ToString();
        }
        
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
