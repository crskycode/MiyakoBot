using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace MiyakoBot.MessageHandler
{
    public class EventHandlerFeatureProvider : IApplicationFeatureProvider<EventHandlerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, EventHandlerFeature feature)
        {
            foreach (var part in parts.OfType<IApplicationPartTypeProvider>())
            {
                foreach (var type in part.Types)
                {
                    if (IsController(type) && !feature.Handlers.Contains(type))
                    {
                        feature.Handlers.Add(type);
                    }
                }
            }
        }

        protected virtual bool IsController(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
            {
                return false;
            }

            if (typeInfo.IsAbstract)
            {
                return false;
            }

            // We only consider public top-level classes as controllers. IsPublic returns false for nested
            // classes, regardless of visibility modifiers
            if (!typeInfo.IsPublic)
            {
                return false;
            }

            if (typeInfo.ContainsGenericParameters)
            {
                return false;
            }

            if (!typeInfo.IsDefined(typeof(EventHandlerAttribute)))
            {
                return false;
            }

            return true;
        }
    }
}
