using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Email;
using Serilog;
using System.Net;

namespace BrainstormSessions
{
    public static class SerilogCustomEmailExtension
    {
        const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";

        public static LoggerConfiguration CustomEmail(
            this LoggerSinkConfiguration loggerConfiguration,
            CustomEmailConnectionInfo connectionInfo,
            string outputTemplate = DefaultOutputTemplate,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum
        )
        {
            return loggerConfiguration.Email(
                connectionInfo,
                outputTemplate
            );
        }

        public class CustomEmailConnectionInfo : EmailConnectionInfo
        {
            public CustomEmailConnectionInfo()
            {
                NetworkCredentials = new NetworkCredential();
            }
        }
    }
}
