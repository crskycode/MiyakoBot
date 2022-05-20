using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiyakoBot.Adapter;
using MiyakoBot.Http;
using MiyakoBot.Message;
using System;
using System.IO;
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

            // Load settings file
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Configure settings file
            services.AddConfiguration(configuration);

            // Configure logging
            services.AddLogging(logging => {
                logging.AddConfiguration(configuration.GetSection("Logging"));
                logging.AddFile();
                logging.AddConsole();
            });

            // Create application part manager for message handlers
            var apm = new ApplicationPartManager();

            // Add handler dlls
            apm.ApplicationParts.Add(new AssemblyPart(typeof(MiyakoBot.Handler.Dummy).Assembly));
            apm.FeatureProviders.Add(new MessageHandlerFeatureProvider());

            // Add message handlers
            services.AddMessageHandles(apm);

            // Configure application part manager
            services.AddSingleton(typeof(ApplicationPartManager), apm);

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
                logger.LogInformation("Shutting down...");
                cts.Cancel();
                e.Cancel = true;
            };

            try
            {
                // Run bot
                container.GetService<IAdapter>()
                    .RunAsync(cts.Token)
                    .GetAwaiter()
                    .GetResult();
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
