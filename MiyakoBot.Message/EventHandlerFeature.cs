using System.Reflection;

namespace MiyakoBot.MessageHandler
{
    public class EventHandlerFeature
    {
        public IList<TypeInfo> Handlers { get; } = new List<TypeInfo>();
    }
}
