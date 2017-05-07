using System;
using System.Collections.Generic;
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
        private GelfSender _gelfSender;
        private string _hostname;

        [RequiredParameter]
        public string ServerUrl { get; set; }

        public string Facility { get; set; }

        public string Debug { get; set; }

        private GelfSender Sender
        {
            get
            {
                if (_gelfSender == null)
                {
                    var debugConfig = (Debug ?? string.Empty).ToLowerInvariant();
                    var debugEnabled = debugConfig == "true" || debugConfig == "1";

                    lock (this)
                    {
                        _gelfSender = new GelfSender(ServerUrl, debugEnabled);
                    }
                }

                return _gelfSender;
            }
        }

        private string HostName
        {
            get
            {
                if (string.IsNullOrEmpty(_hostname))
                    _hostname = Dns.GetHostName();
                return _hostname;
            }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                Sender.Send(CreateGelfJsonFromLoggingEvent(logEvent));
            }
            catch (Exception ex)
            {
                InternalLogger.Log(ex, LogLevel.Error, "Unable to send logging event to remote host " + ServerUrl);
                Sender.Send(CreateFatalGelfJson(ex));
            }
        }

        private GelfMessage CreateGelfJsonFromLoggingEvent(LogEventInfo logEventInfo)
        {
            if (logEventInfo == null) throw new ArgumentNullException(nameof(logEventInfo));

            var formattedMessage = logEventInfo.FormattedMessage ?? "";

            var shortMessage = formattedMessage.Length > ShortMessageLength ? formattedMessage.Substring(0, ShortMessageLength - 1) : formattedMessage;
            var syslogLevel = MapToSyslogSeverity(logEventInfo.Level);
            var gelfMessage = new GelfMessage
            {
                Facility = Facility ?? "GELF",
                FullMessage = formattedMessage,
                Host = Dns.GetHostName(),
                Level = (int) syslogLevel,
                LevelName = syslogLevel.ToString(),
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
        }

        private static readonly Dictionary<string, SyslogSeverity> LogLevelMap = new Dictionary<string, SyslogSeverity>
        {
            {LogLevel.Trace.Name, SyslogSeverity.Debug},
            {LogLevel.Debug.Name, SyslogSeverity.Debug},
            {LogLevel.Info.Name, SyslogSeverity.Informational},
            {LogLevel.Warn.Name, SyslogSeverity.Warning},
            {LogLevel.Error.Name, SyslogSeverity.Error},
            {LogLevel.Fatal.Name, SyslogSeverity.Alert}
        };

        private SyslogSeverity MapToSyslogSeverity(LogLevel level)
        {
            SyslogSeverity severity;

            if (LogLevelMap.TryGetValue(level.Name, out severity))
                return severity;

            return SyslogSeverity.Debug;
        }
    }
}
