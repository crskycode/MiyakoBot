using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MiyakoBot.Adapter;
using MiyakoBot.Http;
using MiyakoBot.Message;

namespace MiyakoBot.Console
{
    public class Application
    {
        /// <summary>
        /// Holds configuration of the application.
        /// </summary>
        readonly IConfiguration _configuration;

        /// <summary>
        /// Logger for current object.
        /// </summary>
        readonly ILogger<Application> _logger;

        /// <summary>
        /// Used to lookup specific classes from assembly.
        /// </summary>
        readonly ApplicationPartManager _applicationPartManager = new();

        /// <summary>
        /// Holds all user-defined message handler type information.
        /// </summary>
        readonly List<TypeInfo> _messageHandlers = new();

        /// <summary>
        /// Holds all available services of the application.
        /// </summary>
        readonly ServiceCollection _applicationServices = new();

        /// <summary>
        /// 
        /// </summary>
        readonly IServiceProvider _applicationServiceProvider;

        /// <summary>
        /// Mirai http interface adapter object.
        /// </summary>
        readonly IAdapter _adapter;

        /// <summary>
        /// Used to exit the application.
        /// </summary>
        readonly CancellationTokenSource _cancellationTokenSource = new();

        public Application()
        {
            // Create a custom logger factory.
            // We can't get the logger from the service provider because the service provider hasn't been created yet.
            var loggerFactory = LoggerFactory.Create(logging => {
                logging.AddConfiguration(_configuration.GetSection("Logging"));
                logging.AddFile();
                logging.AddConsole();
            });

            // Add logging service, then replace the logger factory.
            _applicationServices.AddLogging();
            _applicationServices.Replace(ServiceDescriptor.Singleton(loggerFactory));

            // Now we have a logger for Application.
            _logger = loggerFactory.CreateLogger<Application>();

            // Load settings file.
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            ConfigureApplicationPartManager();
            ConfigureFeatureProvider();
            ConfigureMessageHandlers();
            ConfigureHttpClients();

            _applicationServices.AddSingleton(new MessageHandlerTypeCollection(_messageHandlers));

            _applicationServices.AddSingleton<IAdapter, MiraiWebSocketAdapter>();

            _applicationServiceProvider = _applicationServices.BuildServiceProvider();

            _adapter = _applicationServiceProvider.GetRequiredService<IAdapter>();
        }

        /// <summary>
        /// Add assembly to the manager to lookup specific classes.
        /// </summary>
        void ConfigureApplicationPartManager()
        {
            _applicationPartManager.ApplicationParts.Add(new AssemblyPart(typeof(MiyakoBot.Handler.Dummy).Assembly));
        }

        /// <summary>
        /// Add feature provider for lookup specific classes.
        /// </summary>
        void ConfigureFeatureProvider()
        {
            _applicationPartManager.FeatureProviders.Add(new MessageHandlerFeatureProvider());
        }

        /// <summary>
        /// Lookup all message handlers and add them as services to support dependency injection.
        /// </summary>
        void ConfigureMessageHandlers()
        {
            var feature = new MessageHandlerFeature();

            _applicationPartManager.PopulateFeature(feature);

            foreach (var type in feature.Handlers)
            {
                _applicationServices.AddTransient(type);
                _messageHandlers.Add(type);
            }
        }

        /// <summary>
        /// Add Http clients as services.
        /// </summary>
        void ConfigureHttpClients()
        {
            _applicationServices.AddSingleton<IDefaultHttpClient, DefaultHttpClient>();
            _applicationServices.AddSingleton<IProxyedHttpClient, ProxyedHttpClient>();
        }

        /// <summary>
        /// Run the application, this function blocks until the application exits.
        /// </summary>
        public void Run()
        {
            try
            {
                _adapter.RunAsync(_cancellationTokenSource.Token)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (AggregateException e)
            {
                foreach (var x in e.InnerExceptions)
                {
                    if (x.GetType() != typeof(TaskCanceledException))
                    {
                        _logger.LogError(x, "Fatal Error");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fatal Error");
            }
        }

        /// <summary>
        /// Stop the application. Run() function will exit as soon as possible.
        /// </summary>
        public void Stop()
        {
            _logger.LogInformation("Shutting down...");
            _cancellationTokenSource.Cancel();
        }
    }
}
