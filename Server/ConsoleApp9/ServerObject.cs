using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Net;
namespace ChatServer
{

    using System;
    using System.Linq;
    using System.Threading;

    public class ServerObject
    {
        static TcpListener tcpListener; 
        public List<ClientObject> Clients = new List<ClientObject>();
        public List<MessageBroadcast> Messages = new List<MessageBroadcast>();
        
        protected internal void AddConnection(ClientObject clientObject)
        {
            Clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = Clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                Clients.Remove(client);
        }
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Server Started...");

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
        
             protected internal void BroadcastMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < Clients.Count; i++)
            {
                    Clients[i].Stream.Write(data, 0, data.Length);
                
            }
        }

        protected internal void BroadcastMessage(string message, string receiverId)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].Id == receiverId)
                    {
                        Clients[i].Stream.Write(data, 0, data.Length);
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected internal void BroadcastMessage(BaseBroadcast message)
        {
            if(message is MessageBroadcast)
            {
                Console.WriteLine("dd");
                Messages.Add(message as MessageBroadcast);
            }
            else if(message is ChangeElementBroadcast)
            {
                ChangeElementBroadcast _message = message as ChangeElementBroadcast;
                BroadcastMessage(_message.ToJson(), _message.ReceiverId);
            }
            BroadcastMessage(message.ToJson());
            
        }

        protected internal void BroadcastMessage(BaseBroadcast message, string Id)
        {
            if (message is MessageBroadcast)
            {
                Messages.Add(message as MessageBroadcast);
            }
            else if (message is ChangeElementBroadcast)
            {
                ChangeElementBroadcast _message = message as ChangeElementBroadcast;
                BroadcastMessage(_message.ToJson(), _message.ReceiverId);
            }
            BroadcastMessage(message.ToJson(), Id);

        }
        protected internal void Disconnect()
        {
            tcpListener.Stop(); 

            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].Close();
            }
            Environment.Exit(0); 
        }
    }


}

