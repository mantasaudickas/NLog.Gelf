using System;
using System.Net;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.Gelf.Net
{
    [Target("GelfHttp")]
    public class GelfHttpTarget : Target
    {
        private const int ShortMessageLength = 250;

        [RequiredParameter]
        public string ServerUrl { get; set; }

        public string Facility { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            var sender = new GelfSender(ServerUrl);
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
            var shortMessage = logEventInfo.FormattedMessage.Length > ShortMessageLength ? logEventInfo.FormattedMessage.Substring(0, ShortMessageLength - 1) : logEventInfo.FormattedMessage;

            var gelfMessage = new GelfMessage
            {
                Facility = Facility ?? "GELF",
                FullMessage = logEventInfo.FormattedMessage,
                Host = Dns.GetHostName(),
                Level = logEventInfo.Level.GelfSeverity(),
                ShortMessage = shortMessage,
                Logger = logEventInfo.LoggerName ?? ""
            };

            if (logEventInfo.Properties != null)
            {
                object notes;
                if (logEventInfo.Properties.TryGetValue("Notes", out notes))
                {
                    gelfMessage.Notes = (string)notes;
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
                Level = LogLevel.Fatal.GelfSeverity(),
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
