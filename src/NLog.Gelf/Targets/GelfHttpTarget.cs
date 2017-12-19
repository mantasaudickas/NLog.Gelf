using NLog.Targets;
using NLog.Gelf.Senders;

// ReSharper disable once CheckNamespace
namespace NLog.Gelf
{
    [Target("GelfHttp")]
    public class GelfHttpTarget : GelfBaseTarget
    {
        private GelfBaseSender _gelfSender;

        protected override GelfBaseSender Sender
        {
            get
            {
                if (_gelfSender == null)
                {
                    var debugConfig = (Debug ?? string.Empty).ToLower();
                    var debugEnabled = debugConfig == "true" || debugConfig == "1";

                    lock (this)
                    {
                        _gelfSender = new GelfHttpSender(ServerUrl, debugEnabled);
                    }
                }

                return _gelfSender;
            }
        }

        /*private GelfMessage CreateFatalGelfJson(Exception exception)
        {
            var gelfMessage = new GelfMessage
            {
                Facility = Facility ?? "GELF",
                FullMessage = "Error sending message in NLog.GelfHttpTarget",
                Host = HostName,
                Level = LogLevel.Fatal.Ordinal,
                LevelName = LogLevel.Fatal.ToString(),
                ShortMessage = "Error sending message in NLog.GelfHttpTarget"
            };

            if (exception != null)
            {
                var exceptioToLog = exception;

                while (exceptioToLog.InnerException != null)
                {
                    exceptioToLog = exceptioToLog.InnerException;
                }

                gelfMessage.ExceptionType = exceptioToLog.GetType().Name;
                gelfMessage.ExceptionMessage = exceptioToLog.Message;
                gelfMessage.StackTrace = exceptioToLog.StackTrace;
            }

            return gelfMessage;
        }*/
    }
}
