using Newtonsoft.Json;

namespace Chat.MVVM.Model
{
    [JsonObject(MemberSerialization.OptOut)]
    public class BaseBroadcast
    {
        public enum Types { Default, Message, Join, Left, Client, Delete, ChangeUserName, ChangeUserNameColor, ChangeImageSource, ChangeMessage };

        [JsonProperty]
        public Types Type = Types.Default;
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

    };
    public class MessageBroadcast : BaseBroadcast
    {


        public MessageBroadcast()
        {
            //MessageId = System.Guid.NewGuid().ToString();
            Type = Types.Message;

        }
        public string MessageId { get; set; }
        public string SourceId { get; set; }
        public string SourceUserName { get; set; }
        public string SourceUserNameColor { get; set; }
        public string SourceImageSource { get; set; }
        public string Message { get; set; }
        public string DestinationId { get; set; }
    }

    public class JoinedBroadcast : BaseBroadcast
    {
        public JoinedBroadcast()
        {
            Type = Types.Join;
        }


        public string UserName { get; set; }
        public string UserNameColor { get; set; }
        public string ImageSource { get; set; }
        public string JoinedId { get; set; }

    }


    public class DeleteMessageBroadcast : BaseBroadcast
    {
        public DeleteMessageBroadcast()
        {
            Type = Types.Delete;
        }
        public string MessageId { get; set; }
    }

    public class ChangeElementBroadcast : BaseBroadcast
    {

        public ChangeElementBroadcast() { }
        public ChangeElementBroadcast(BaseBroadcast.Types type, string receiverId, string value)
        {
            Type = type;
            ReceiverId = receiverId;
            Value = value;
        }

        public string ReceiverId { get; set; }
        public string Value { get; set; }
    }


    public class LeftBroadcast : BaseBroadcast
    {
        public LeftBroadcast()
        {
            Type = Types.Left;
        }

        public string LeftId { get; set; }


    }

    public class ChangeMessageBroadcast : BaseBroadcast
    {
        public ChangeMessageBroadcast()
        {
            Type = Types.ChangeMessage;
        }
        public string MessageId { get; set; }
        public string Value { get; set; }
    }
}
