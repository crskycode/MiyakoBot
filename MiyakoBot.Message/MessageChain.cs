using System;
using System.Text.Json.Nodes;

namespace MiyakoBot.Message
{
    public class MessageChain
    {
        readonly JsonArray _object;

        public MessageChain(JsonArray jsonArray)
        {
            _object = jsonArray;
        }

        public JsonArray ToJson()
        {
            return _object;
        }
    }
}
