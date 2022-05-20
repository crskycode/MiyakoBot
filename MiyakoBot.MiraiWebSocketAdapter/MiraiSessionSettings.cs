using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiyakoBot.Adapter
{
    internal class MiraiSessionSettings
    {
        public string Host { get; set; } = "localhost";
        public ushort Port { get; set; } = 8080;
        public string VerifyKey { get; set; } = string.Empty;
        public string QQ { get; set; } = string.Empty;
    }
}
