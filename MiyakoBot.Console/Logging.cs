using Microsoft.Extensions.Logging;
using Serilog;

namespace MiyakoBot.Console
{
    static class LoggingExtensions
    {
        internal static void AddFile(this ILoggingBuilder builder)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.File("log-.txt")
                .CreateLogger();
            builder.AddSerilog(logger);
        }
    }
}
