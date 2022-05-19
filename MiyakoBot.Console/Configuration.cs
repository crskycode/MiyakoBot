using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace MiyakoBot.Console
{
    static class ConfigurationExtensions
    {
        internal static void AddConfiguration(this ServiceCollection services, string path)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path, false, true)
                .Build();
            services.AddSingleton(typeof(IConfiguration), configuration);
        }
    }
}
