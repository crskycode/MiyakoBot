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

        public override string ToString()
        {
            if (_object == null)
            {
                return string.Empty;
            }

            var text = string.Empty;

            foreach (var e in _object)
            {
                if (e == null)
                {
                    continue;
                }

                if (e["type"]?.ToString() == "Plain")
                {
                    text += e["text"]?.ToString() ?? string.Empty;
                }
            }

            return text;
        }
    }
}
