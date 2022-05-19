using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiyakoBot.Adapter;
using MiyakoBot.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

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

            // Configure application part manager
            services.AddSingleton(typeof(ApplicationPartManager));

            // Configure Http clients
            services.AddSingleton(typeof(IDefaultHttpClient), typeof(DefaultHttpClient));
            services.AddSingleton(typeof(IProxyedHttpClient), typeof(ProxyedHttpClient));

            // Configure backend adapter
            services.AddSingleton(typeof(IAdapter), typeof(MiraiWebSocketAdapter));

            // Build container
            var container = services.BuildServiceProvider();

            // Get logger for Main
            var logger = container.GetService<ILogger<Program>>();

            // For quit
            var cts = new CancellationTokenSource();

            // Add Ctrl+C handler
            System.Console.CancelKeyPress += (s, e) => {
                logger.LogInformation("Shutting down");
                cts.Cancel();
                e.Cancel = true;
            };

            try
            {
                // Run bot
                container.GetService<IAdapter>()
                    .RunAsync(cts.Token)
                    .Wait();
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                {
                    if (ie.GetType() != typeof(TaskCanceledException))
                    {
                        logger.LogError(e, "Fatal Error");
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Fatal Error");
            }
        }
    }
}
