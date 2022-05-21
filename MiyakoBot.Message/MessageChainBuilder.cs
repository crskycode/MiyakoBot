using System;
using System.Text.Json.Nodes;

namespace MiyakoBot.Message
{
    public class MessageChainBuilder
    {
        readonly JsonArray _array = new();

        public MessageChainBuilder()
        {
        }

        public MessageChainBuilder AddAt(string target, string display)
        {
            _array.Add(new JsonObject {
                ["type"] = "At",
                ["target"] = uint.Parse(target),
                ["display"] = display
            });
            return this;
        }

        public MessageChainBuilder AddAtAll()
        {
            _array.Add(new JsonObject {
                ["type"] = "AtAll"
            });
            return this;
        }

        public MessageChainBuilder AddPlain(string text)
        {
            _array.Add(new JsonObject {
                ["type"] = "Plain",
                ["text"] = text
            });
            return this;
        }

        public MessageChainBuilder AddLocalImage(string path)
        {
            _array.Add(new JsonObject {
                ["type"] = "Image",
                ["path"] = path
            });
            return this;
        }

        public MessageChainBuilder AddRemoteImage(string url)
        {
            _array.Add(new JsonObject {
                ["type"] = "Image",
                ["url"] = url
            });
            return this;
        }

        public MessageChain Build()
        {
            return new MessageChain(_array);
        }
    }
}
