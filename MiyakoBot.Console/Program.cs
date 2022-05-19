using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiyakoBot.Adapter;
using MiyakoBot.Http;

namespace MiyakoBot.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create services
            var services = new ServiceCollection();

            // Configure settings file
            services.AddConfiguration("appsettings.json");

            // Configure logging
            services.AddLogging(logging => {
                logging.AddFile();
                logging.AddConsole();
            });

            // Configure Http clients
            services.AddSingleton(typeof(IDefaultHttpClient), typeof(DefaultHttpClient));
            services.AddSingleton(typeof(IProxyedHttpClient), typeof(ProxyedHttpClient));

            // Configure backend adapter
            services.AddSingleton(typeof(IAdapter), typeof(MiraiWebSocketAdapter));

            // Build container
            var container = services.BuildServiceProvider();

            // Run bot
            container.GetService<IAdapter>()
                .Run();
        }
    }
}
