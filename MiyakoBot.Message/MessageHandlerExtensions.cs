using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace MiyakoBot.Message
{
    public static class MessageHandlerExtensions
    {
        public static void AddMessageHandles(this IServiceCollection services, ApplicationPartManager applicationPartManager)
        {
            var feature = new MessageHandlerFeature();

            applicationPartManager.PopulateFeature(feature);

            foreach (var type in feature.Handlers)
            {
                services.AddScoped(type);
            }
        }
    }
}
