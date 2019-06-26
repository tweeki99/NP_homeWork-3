using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NpHw_3.Server
{
    public class ServerObject
    {
        static TcpListener tcpListener;
        List<ClientObject> clients = new List<ClientObject>();
        List<Client> clientsObj = new List<Client>();

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
            clientsObj.Add(new Client { Id = clientObject.Id, UserName = clientObject.userName });
            BroadcastMessage(Attribute.ListClients.ToString()+ (char)1 + JsonConvert.SerializeObject(clientsObj), clientObject.Id);
            Debug.Write(clients);
        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);

            Client clientsObject = clientsObj.FirstOrDefault(c => c.Id == id);
            if (clientsObject != null)
                clientsObj.Remove(clientsObject);

            BroadcastMessage(Attribute.ListClients.ToString() + (char)1 + JsonConvert.SerializeObject(clientsObj), id);
        }
        
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 12345);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }
        
        protected internal void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Stream.Write(data, 0, data.Length);
            }
        }

        protected internal string PrivateMessage(string message, ClientObject clientObject, string idTo)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == idTo);
            string messageToClient = "";
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == idTo || clients[i].Id == clientObject.Id)
                {
                    messageToClient = "Личное сообщение от " + clientObject.userName + " для " + client.userName + ": " + message;
                    byte[] data = Encoding.Unicode.GetBytes(Attribute.Message.ToString() + (char)1 + messageToClient);

                    clients[i].Stream.Write(data, 0, data.Length);
                }
            }
            return messageToClient;
        }
        
        protected internal void Disconnect()
        {
            tcpListener.Stop();

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            Environment.Exit(0);
        }
    }
}
