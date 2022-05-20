namespace MiyakoBot.Message
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MessageAttribute : Attribute
    {
        public MessageTypes Type { get; }

        public MessageAttribute(MessageTypes type)
        {
            Type = type;
        }
    }
}
