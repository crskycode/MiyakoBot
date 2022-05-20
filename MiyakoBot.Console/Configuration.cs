using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MiyakoBot.Console
{
    static class ConfigurationExtensions
    {
        internal static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(typeof(IConfiguration), configuration);
        }
    }
}
