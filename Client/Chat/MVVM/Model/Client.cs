using Chat.MVVM.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;


namespace Chat.MVVM.Model
{
    class Client
    {
        private const string host = "127.0.0.1";
        static int port = 8888;
        static TcpClient client;
        static NetworkStream stream;
        public static string Id;


        public static void initServer()
        {
            client = new TcpClient();
            try
            {
                client.Connect(host, port);
                stream = client.GetStream();



                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Something went wrong");
            }
        }

        public static void SendMessage(string message)
        {

            if (message != "")
            {
                Console.WriteLine(message);
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        public static void SendMessage(BaseBroadcast baseBroadcast)
        {
            SendMessage(JsonConvert.SerializeObject(baseBroadcast).ToString());
        }

        public static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    string message = "";
                    Console.WriteLine("Trying");
                    if (DecodeMessage(ref message))
                    {
                        Console.WriteLine(message);
                        BaseBroadcast broadcast = JsonConvert.DeserializeObject<BaseBroadcast>(message);
                        Console.WriteLine(broadcast.Type);
                        if(broadcast.Type == BaseBroadcast.Types.Message)
                        {
                            MessageBroadcast messageBroadcast = JsonConvert.DeserializeObject<MessageBroadcast>(message);
                            

                            Console.WriteLine(messageBroadcast.Message);
                            Console.WriteLine(messageBroadcast.SourceImageSource);

                            MessageModel messageModel = new MessageModel()
                            {
                                Id = messageBroadcast.MessageId,
                                SenderId = messageBroadcast.SourceId,
                                Username = messageBroadcast.SourceUserName,
                                UsernameColor = messageBroadcast.SourceUserNameColor,
                                    ImageSource = messageBroadcast.SourceImageSource,
                                Message = messageBroadcast.Message,
                                Time = DateTime.Now,
                                CurrentUserIsSender = (bool)(Client.Id == messageBroadcast.SourceId)
                            };

                            App.Current.Dispatcher.Invoke((Action)delegate ()
                            {

                                MainViewModel.GetInstance().Messages.Add(messageModel);
                            });

                        }
                        else if(broadcast.Type == BaseBroadcast.Types.Join)
                        {
                            JoinedBroadcast joinedBroadcast = JsonConvert.DeserializeObject<JoinedBroadcast>(message);
                            ReadJoinBroadcast(joinedBroadcast);

                        }
                        else if(broadcast.Type == BaseBroadcast.Types.Delete)
                        {
                 
                            DeleteMessageBroadcast deleteMessageBroadcast = JsonConvert.DeserializeObject<DeleteMessageBroadcast>(message);
                            deleteMessage(deleteMessageBroadcast);
                            
                        }
                        else if(broadcast.Type == BaseBroadcast.Types.ChangeImageSource)
                        {
                            Console.WriteLine(message);
                            ChangeElementBroadcast changeElementBroadcast = JsonConvert.DeserializeObject<ChangeElementBroadcast>(message);
                            MainViewModel.GetInstance().ImageLink = changeElementBroadcast.Value;
                        }
                        else if(broadcast.Type == BaseBroadcast.Types.Left)
                        {
                            LeftBroadcast leftBroadcast = JsonConvert.DeserializeObject<LeftBroadcast>(message);
                            deleteContact(leftBroadcast);
                        }
                        else if(broadcast.Type == BaseBroadcast.Types.ChangeMessage)
                        {
                            ChangeMessageBroadcast changeMessageBroadcast = JsonConvert.DeserializeObject<ChangeMessageBroadcast>(message);
                            ReadChangeBroadcast(changeMessageBroadcast);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Disconnect();
                }
            }

        }

        private static void ReadChangeBroadcast(ChangeMessageBroadcast changeMessageBroadcast)
        {
            ObservableCollection<MessageModel> messages = MainViewModel.GetInstance().Messages;
            for(int i = 0; i < messages.Count; i++)
            {
                if(messages[i].Id == changeMessageBroadcast.MessageId)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate ()
                    {
                        messages[i] = new MessageModel(messages[i], changeMessageBroadcast.Value);
                    });
                    break;
                }
            }
        }

        internal static void LeftMessage()
        {
            LeftBroadcast leftBroadcast = new LeftBroadcast();
            SendMessage(leftBroadcast);
        }

        internal static void ChangeMessage(string id, string new_value)
        {
            ChangeMessageBroadcast changeMessageBroadcast = new ChangeMessageBroadcast() {
                MessageId = id,
                Value = new_value
            };
            SendMessage(changeMessageBroadcast);
        }

        static void deleteMessage(DeleteMessageBroadcast deleteMessageBroadcast)
        {
            foreach (MessageModel messageModel in MainViewModel.GetInstance().Messages)
            {
                if (messageModel.Id == deleteMessageBroadcast.MessageId)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate ()
                    {
                        MainViewModel.GetInstance().Messages.Remove(messageModel);
                    });
                    break;
                }
            }
        }

        static void deleteContact(LeftBroadcast leftBroadcast)
        {
            foreach (ContactModel contact in MainViewModel.GetInstance().Contacts)
            {
                if (contact.Id == leftBroadcast.LeftId)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate ()
                    {
                        MainViewModel.GetInstance().Contacts.Remove(contact);
                    });
                    break;
                }
            }
        }
        static void ReadJoinBroadcast(JoinedBroadcast joinedBroadcast)
        {
            App.Current.Dispatcher.Invoke((Action)delegate ()
            {
                MainViewModel.GetInstance().Contacts.Add(new ContactModel()
                {
                    Username = joinedBroadcast.UserName,
                    Id = joinedBroadcast.JoinedId,
                    ImageSource = joinedBroadcast.ImageSource,
                    Messages = new ObservableCollection<MessageModel>()
                }); 
            });
        }

        internal static void DeleteMessage(string id)
        {
            DeleteMessageBroadcast deleteMessageBroadcast = new DeleteMessageBroadcast()
            {
                MessageId = id
            };
            SendMessage(deleteMessageBroadcast);
        }

        static bool DecodeMessage(ref string message)
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            message = builder.ToString();
            return message != "";
        }
        public static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }

        public static void JoinMessage()
        {
            MainViewModel mv = MainViewModel.GetInstance();
            JoinedBroadcast joinedBroadcast = new JoinedBroadcast()
            {
                UserName = mv.UserName,
                UserNameColor = mv.UserNameColor,
                ImageSource = mv.ImageLink,
                JoinedId = Id
            };
            Console.WriteLine(JsonConvert.SerializeObject(joinedBroadcast).ToString());
            SendMessage(joinedBroadcast);
            
        }

        public static void MessageMessage(string message)

        {
            MainViewModel mv = MainViewModel.GetInstance();
            string destinationId = mv.SelectedContact.Id;
            MessageBroadcast messageBroadcast = new MessageBroadcast() {
                DestinationId = destinationId,
                SourceUserName = mv.UserName,
                SourceId = Id,
                SourceImageSource = mv.ImageLink,
                SourceUserNameColor = mv.UserNameColor,
                Message = message

            };
            Console.WriteLine(JsonConvert.SerializeObject(messageBroadcast).ToString());
            SendMessage(messageBroadcast);


        }
    }
}


