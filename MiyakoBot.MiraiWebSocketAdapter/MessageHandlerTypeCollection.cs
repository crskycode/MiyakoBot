using System.Collections;
using System.Reflection;

namespace MiyakoBot.Adapter
{
    public class MessageHandlerTypeCollection : IReadOnlyList<TypeInfo>
    {
        readonly IReadOnlyList<TypeInfo> _list;

        public MessageHandlerTypeCollection(IReadOnlyList<TypeInfo> list) => _list = list;

        public TypeInfo this[int index] => _list[index];

        public int Count => _list.Count;

        public IEnumerator<TypeInfo> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    }
}
