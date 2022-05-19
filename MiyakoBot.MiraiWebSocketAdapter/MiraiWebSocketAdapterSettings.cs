using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiyakoBot.Adapter
{
    internal class MiraiWebSocketAdapterSettings
    {
        public string HttpHost { get; set; } = "localhost";
        public ushort HttpPort { get; set; }
        public string WsHost { get; set; } = "localhost";
        public ushort WsPort { get; set; }
        public string VerifyKey { get; set; } = string.Empty;
        public string QQ { get; set; } = string.Empty;
    }
}
