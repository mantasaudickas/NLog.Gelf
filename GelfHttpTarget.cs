using System;
using System.Net;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.Gelf
{
    [Target("GelfHttp")]
    public class GelfHttpTarget : Target
    {
        private const int ShortMessageLength = 250;

        [RequiredParameter]
        public string ServerUrl { get; set; }

        public string Facility { get; set; }

        public string Debug { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            var debugConfig = (Debug ?? string.Empty).ToLowerInvariant();
            var debugEnabled = debugConfig == "true" || debugConfig == "1";

            var sender = new GelfSender(ServerUrl, debugEnabled);
            try
            {
                sender.Send(CreateGelfJsonFromLoggingEvent(logEvent));
            }
            catch (Exception ex)
            {
                InternalLogger.Log(ex, LogLevel.Error, "Unable to send logging event to remote host " + ServerUrl);
                sender.Send(CreateFatalGelfJson(ex));
            }
        }

        private GelfMessage CreateGelfJsonFromLoggingEvent(LogEventInfo logEventInfo)
        {
            if (logEventInfo == null) throw new ArgumentNullException(nameof(logEventInfo));

            var formattedMessage = logEventInfo.FormattedMessage ?? "";

            var shortMessage = formattedMessage.Length > ShortMessageLength ? formattedMessage.Substring(0, ShortMessageLength - 1) : formattedMessage;

            var gelfMessage = new GelfMessage
            {
                Facility = Facility ?? "GELF",
                FullMessage = formattedMessage,
                Host = Dns.GetHostName(),
                Level = logEventInfo.Level.Ordinal,
                LevelName = logEventInfo.Level.ToString(),
                ShortMessage = shortMessage,
                Logger = logEventInfo.LoggerName ?? ""
            };

            if (logEventInfo.Properties != null)
            {
                object notes;
                if (logEventInfo.Properties.TryGetValue("Notes", out notes))
                {
                    gelfMessage.Notes = notes as string;
                }
            }

            if (logEventInfo.Exception != null)
            {
                var exceptioToLog = logEventInfo.Exception;

                while (exceptioToLog.InnerException != null)
                {
                    exceptioToLog = exceptioToLog.InnerException;
                }

                gelfMessage.ExceptionType = exceptioToLog.GetType().Name;
                gelfMessage.ExceptionMessage = exceptioToLog.Message;
                gelfMessage.StackTrace = exceptioToLog.StackTrace;
            }

            return gelfMessage;
        }

        private GelfMessage CreateFatalGelfJson(Exception exception)
        {
            var gelfMessage = new GelfMessage
            {
                Facility = Facility ?? "GELF",
                FullMessage = "Error sending message in NLog.GelfHttpTarget",
                Host = Dns.GetHostName(),
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
        }
    }
}
