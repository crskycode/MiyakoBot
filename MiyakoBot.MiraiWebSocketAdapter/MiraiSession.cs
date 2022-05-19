using MiyakoBot.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiyakoBot.Adapter
{
    internal class MiraiSession
    {
        readonly IHttpClient _httpClient;
        readonly string _verifyKey;

        public MiraiSession(IHttpClient httpClient, string verifyKey)
        {
            _httpClient = httpClient;
            _verifyKey = verifyKey;
        }

        public string Verify()
        {

        }

        public bool Bind(string sessionKey, string qq)
        {

        }

        public void Release()
        {

        }
    }
}
