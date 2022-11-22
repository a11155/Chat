using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.MVVM.Model
{
    public class ContactModel
    {

        public string Id { get; set; }
        public string Username { get; set; }
        public string ImageSource { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }

        public string LastMessage => (Messages == null || Messages.Count == 0) ? "No messages" : Messages.Last().Message;
    }
}
