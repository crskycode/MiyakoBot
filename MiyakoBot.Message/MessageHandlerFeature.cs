using System.Reflection;

namespace MiyakoBot.MessageHandler
{
    public class MessageHandlerFeature
    {
        public IList<TypeInfo> Handlers { get; } = new List<TypeInfo>();
    }
}
