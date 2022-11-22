using Chat.MVVM.ViewModel;
using Chat.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.MVVM.Model
{
    public class MessageModel
    {

        public MessageModel()
        {
            DeleteMessageCommand = new RelayCommand(o =>
            {
                Client.DeleteMessage(MainViewModel.GetInstance().SelectedMessage.Id);
            });

            EditMessageCommand = new RelayCommand(o =>
            {
                MainViewModel.GetInstance().StartChanging(this);
            });

        }

        public MessageModel(MessageModel messageModel, string message) : this()
        {
            this.Message = message;
            Id = messageModel.Id;
            SenderId = messageModel.SenderId;
            Username = messageModel.Username;
            UsernameColor = messageModel.UsernameColor;
            ImageSource = messageModel.ImageSource;
            Time = DateTime.Now;
            CurrentUserIsSender = messageModel.CurrentUserIsSender;
            Console.WriteLine(Username);
        }

        public string Id { get; set; }
        public string SenderId { get; set; }

        public string Username { get; set; }
        public string UsernameColor { get; set; }
        public string ImageSource { get; set; }

        public string Message { get; set; }

        public DateTime Time { get; set; }
        public bool IsNativeOrigin { get; set; }
        public bool? FirstMessage { get; set; }
        public RelayCommand DeleteMessageCommand { get; set; }
        public RelayCommand EditMessageCommand { get; set; }

        public bool CurrentUserIsSender { get; set; }
    }
}
