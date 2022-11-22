using Chat.MVVM.Model;
using Chat.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;

namespace Chat.MVVM.ViewModel
{

    public class MainViewModel : ObservableObject
    {
        static  MainViewModel _instance;
        
        private ObservableCollection<MessageModel> _messages;

        public ObservableCollection<MessageModel> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged();
            }

        }

        private bool _isChanging;

        public bool IsChanging
        {
            get { return _isChanging; }
            set { _isChanging = value;
                OnPropertyChanged();
            }
        }

        private MessageModel _changedMessage;

        public MessageModel ChangedMessage
        {
            get { return _changedMessage; }
            set { _changedMessage = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<ContactModel> _contacts;

        public ObservableCollection<ContactModel> Contacts
        {
            get { return _contacts; }
            set { _contacts = value;
                OnPropertyChanged();
            }
        }



        private MessageModel _selectedMessage;
        public MessageModel SelectedMessage
        {
            get { return _selectedMessage; }
            set { _selectedMessage = value;
                OnPropertyChanged();
            }
        }
        public RelayCommand SendCommand { get; set; }
        public RelayCommand DeleteMessageCommand { get; set; }
        public RelayCommand CheckCommand { get; set; }
        public RelayCommand LoginButton_Command { get; set; }
        public RelayCommand LoginSubmitButton_Command { get; set; }

        private RelayCommand _currentSendCommand;

        public RelayCommand CurrentSendCommand {
            get { return _currentSendCommand; }
            set { _currentSendCommand = value;
                OnPropertyChanged();
            } }
        public RelayCommand ChangeCommand { get; set; }
       
        public RelayCommand CloseChangingCommand { get; set; }

        public RelayCommand ShutDownCommand { get; set; }

        private ContactModel _selectedContact;

         private string _userName;


        public string UserNameColor { get; set; } = "#3e4147";

        private string _imageLink;

        public string ImageLink
        {
            get { return _imageLink; }
            set { _imageLink = value;
                OnPropertyChanged();
            }
        }


        public  string UserName
        {
            get { return _userName; }
            set { _userName = value;
                OnPropertyChanged();
            }
        }



        static private bool _loggedIn;

        public  bool LoggedIn
        {
            get { return _loggedIn; }
            set { _loggedIn = value;
                InversedLoggedIn = !_loggedIn;
                OnPropertyChanged();
            }
        }

        static private bool _inversedLoggedIn;

        public bool InversedLoggedIn
        {
            get { return _inversedLoggedIn; }
            set
            {
                _inversedLoggedIn = value;
                OnPropertyChanged();
            }
        }


        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set { 
                _selectedContact = value;
                Messages = _selectedContact.Messages;
                OnPropertyChanged();

                
            }
        }

        static private string _message;

        public string Message
        {
            get { return _message; }
            set { 
                _message = value;
                OnPropertyChanged();
            }
        }
        void init()
        {
            LoggedIn = false;
            Messages = new ObservableCollection<MessageModel>();
            Contacts = new ObservableCollection<ContactModel>();

            SendCommand = new RelayCommand(o =>
            {
                if (LoggedIn)
                {
                    Client.MessageMessage(Message);
                    Message = "";
                }
            });

            CheckCommand = new RelayCommand(o =>
            {
                //TODO
            });

            ShutDownCommand = new RelayCommand(o =>
            {
                Client.LeftMessage();
                Client.Disconnect();
            });
            LoginButton_Command = new RelayCommand(o =>
            {
                new LoginWindow().Show();
            });
            LoginSubmitButton_Command = new RelayCommand(o =>
            {
                LoggedIn = true;
                Client.JoinMessage();

            });

            DeleteMessageCommand = new RelayCommand(o =>
            {
                Client.DeleteMessage(SelectedMessage.Id);
            });

            ChangeCommand = new RelayCommand(o =>
            {
                Client.ChangeMessage(ChangedMessage.Id, Message);
                IsChanging = false;
                CurrentSendCommand = SendCommand;
                Message = "";
            });

            CloseChangingCommand = new RelayCommand(o =>
            {
                IsChanging = false;
                CurrentSendCommand = SendCommand;
                Message = "";
            });

            CurrentSendCommand = SendCommand;
        }
        public void addMessage(MessageModel message)
        {
            Messages.Add(message);

        }

        public void StartChanging(MessageModel changedMessage)
        {
            IsChanging = true;
            CurrentSendCommand = ChangeCommand;
            ChangedMessage = changedMessage;


        }


        public static MainViewModel GetInstance()
        {
            if(_instance == null)
            {
                _instance = new MainViewModel();
            }
            return _instance;
        }
        private MainViewModel()
        {
            init();
            Client.initServer();

            initPublic();
        }


        void initPublic()
        {
            Contacts.Add(new ContactModel()
            {
                Id = "0",
                Messages = Messages,
                ImageSource = "https://i.pinimg.com/originals/60/46/50/6046506410b530315f49bde35d5cccb6.jpg",
                Username = "Public"
            });
        }

  
 
    }

 
}
