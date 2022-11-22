using System.Net.Sockets;
using System.Text;
namespace ChatServer
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class ClientObject
    {
        
        public string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        public string? userName;
        public string? userNameColor;
        public string? imageLink;
        bool left = false;
        static List<string> DefaultImageLinks = new List<string>()
        {
            "http://www.lecriconsult.co.tz/wp-content/uploads/2016/11/person-outline-.png",
            "https://external-preview.redd.it/4PE-nlL_PdMD5PrFNLnjurHQ1QKPnCvg368LTDnfM-M.png?auto=webp&s=ff4c3fbc1cce1a1856cff36b5d2a40a6d02cc1c3",
            "https://support.discord.com/hc/user_images/H1EPcDoqyiUYgOTB-4NwzQ.png",
            "https://loaris.app/wp-content/uploads/2021/03/discord-logo.png"
        };
        static Random random;


        static ClientObject()
        {
            random = new Random();
        }
        TcpClient client;
        ServerObject server;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);

            Stream = client.GetStream();

            getCurrentUsers();
        }

        public void Process()
        {
            try
            {
                
                string message;
                

                while (true)
                {
                    try

                    {
                        message = GetMessage();
                        Console.WriteLine(message);
                        BaseBroadcast baseBroadcast = JsonConvert.DeserializeObject<BaseBroadcast>(message);
                        Console.WriteLine(baseBroadcast.Type);
                        if (baseBroadcast.Type == BaseBroadcast.Types.Join)
                        {
                            JoinedBroadcast joinedBroadcast = JsonConvert.DeserializeObject<JoinedBroadcast>(message);

                            joinBroadcast(joinedBroadcast);

                        }
                        else if(baseBroadcast.Type == BaseBroadcast.Types.Message)
                        {
                            Console.WriteLine("Message " + message);
                            MessageBroadcast messageBroadcast = JsonConvert.DeserializeObject<MessageBroadcast>(message);
                            messageBroadcast.MessageId = Guid.NewGuid().ToString();
                            if (messageBroadcast.DestinationId == "0")
                                server.BroadcastMessage(messageBroadcast);
                            else
                                server.BroadcastMessage(messageBroadcast, messageBroadcast.DestinationId);
                          }
                        else if(baseBroadcast.Type == BaseBroadcast.Types.Delete)
                        {
                            DeleteMessageBroadcast deleteMessageBroadcast = JsonConvert.DeserializeObject<DeleteMessageBroadcast>(message);
                            deleteMessage(deleteMessageBroadcast);
                           
                        }
                        else if(baseBroadcast.Type == BaseBroadcast.Types.Left)
                        {
                            leave();
                        }
                        else if(baseBroadcast.Type == BaseBroadcast.Types.ChangeMessage)
                        {
                            ChangeMessageBroadcast changeMessageBroadcast = JsonConvert.DeserializeObject<ChangeMessageBroadcast>(message);
                            changeMessage(changeMessageBroadcast);
                        }
                    }
                    catch (Exception ex)
                    {

                        leave();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {

                leave();
            }
        }

        private void changeMessage(ChangeMessageBroadcast changeMessageBroadcast)
        {
            foreach (MessageBroadcast broadcast in server.Messages)
            {
                if (broadcast.MessageId == changeMessageBroadcast.MessageId)
                {
                    broadcast.Message = changeMessageBroadcast.Value;
                    break;
                }
            }
            server.BroadcastMessage(changeMessageBroadcast);
        }

        private void leave()
        {
            if (left) return;
            left = true;
            
            Console.WriteLine("left");
            LeftBroadcast leftBroadcast = new LeftBroadcast()
            {
                LeftId = Id
            };

            server.Clients.Remove(this);
            server.BroadcastMessage(leftBroadcast);
            server.RemoveConnection(this.Id);
            Close();

        }
        private void deleteMessage(DeleteMessageBroadcast deleteMessageBroadcast)
        {
            foreach (MessageBroadcast broadcast in server.Messages)
            {
                if (broadcast.MessageId == deleteMessageBroadcast.MessageId)
                {
                    Console.WriteLine("Deleted " + broadcast.Message);
                    server.Messages.Remove(broadcast);
                    break;
                }
            }
            server.BroadcastMessage(deleteMessageBroadcast);
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

        private ClientObject getClientById(string _Id)
        {
            foreach(ClientObject clientObject in server.Clients)
            {
                if (clientObject.Id == _Id)
                    return clientObject;
            }
            return null;

        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }

        private void serverBroadcast(string message, string destination = "0")
        {
            /*
            MessageBroadcast messageBroadcast = new MessageBroadcast()
            {
                DestinationId = destination,
                SourceId = "0",
                Message = $"{userName} joined the server"
            };
            server.BroadcastMessage(messageBroadcast);
            */
        }

        private void joinBroadcast(JoinedBroadcast joinedBroadcast)
        { 
            userName = joinedBroadcast.UserName;
            userNameColor = joinedBroadcast.UserNameColor;
            imageLink = (joinedBroadcast.ImageSource != null && joinedBroadcast.ImageSource != "") ? joinedBroadcast.ImageSource : DefaultImageLinks[random.Next(DefaultImageLinks.Count)];

            changeImageSource(imageLink);

            Thread.Sleep(100);


            JoinedBroadcast _joinedBroadcast = new JoinedBroadcast(this);
            Console.WriteLine(JsonConvert.SerializeObject(_joinedBroadcast).ToString());
            server.BroadcastMessage(_joinedBroadcast);
        }

        private void joinBroadcast(string id)
        {




            JoinedBroadcast joinedBroadcast = new JoinedBroadcast(this);
            Console.WriteLine(JsonConvert.SerializeObject(joinedBroadcast).ToString());
            server.BroadcastMessage(joinedBroadcast, id);
        }
        private void changeImageSource(string newValue)
        {
            ChangeElementBroadcast changeElementBroadcast = new ChangeElementBroadcast(
                BaseBroadcast.Types.ChangeImageSource, Id, newValue);
            server.BroadcastMessage(changeElementBroadcast);
        }

        private void getCurrentUsers()
        {
            foreach(ClientObject client in server.Clients)
            {
                
                if(client.Id != Id)
                {
                    client.joinBroadcast(Id);
                }
            }
        }
    }


}

