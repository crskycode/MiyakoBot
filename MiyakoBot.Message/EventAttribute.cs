namespace MiyakoBot.Message
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EventAttribute : Attribute
    {
        public EventTypes Type { get; }

        public EventAttribute(EventTypes type)
        {
            Type = type;
        }
    }
}
