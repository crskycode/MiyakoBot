using System.Reflection;

namespace MiyakoBot.Message
{
    public class MessageHandlerFeature
    {
        public IList<TypeInfo> Handlers { get; } = new List<TypeInfo>();
    }
}
