using System;
using System.Collections.Generic;
using System.Net;
using NLog.Common;
using NLog.Config;
using NLog.Gelf.Senders;
using NLog.Targets;

// ReSharper disable once CheckNamespace
namespace NLog.Gelf
{
    public abstract class GelfBaseTarget : TargetWithLayout
    {
        private const int ShortMessageLength = 250;
        private static readonly string Hostname = Dns.GetHostName();

        [RequiredParameter]
        public string ServerUrl { get; set; }

        public int Port { get; set; } = 12201;

        public string Facility { get; set; }

        public string Debug { get; set; }

        protected abstract GelfBaseSender Sender { get; }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                Sender.Send(CreateGelfJsonFromLoggingEvent(logEvent));
            }
            catch (Exception ex)
            {
                InternalLogger.Log(ex, LogLevel.Error, "Unable to send logging event to remote host " + ServerUrl);
                //Sender.Send(CreateFatalGelfJson(ex));
            }
        }

        private GelfMessage CreateGelfJsonFromLoggingEvent(LogEventInfo logEventInfo)
        {
            if (logEventInfo == null) throw new ArgumentNullException(nameof(logEventInfo));

            var formattedMessage = Layout.Render(logEventInfo);

            var shortMessage = formattedMessage.Length > ShortMessageLength ? formattedMessage.Substring(0, ShortMessageLength - 1) : formattedMessage;
            var syslogLevel = MapToSyslogSeverity(logEventInfo.Level);
            var gelfMessage = new GelfMessage
            {
                Facility = Facility ?? "GELF",
                FullMessage = formattedMessage,
                Host = Hostname,
                Level = (int) syslogLevel,
                LevelName = syslogLevel.ToString(),
                ShortMessage = shortMessage,
                Logger = logEventInfo.LoggerName ?? ""
            };

            if (logEventInfo.Properties != null && logEventInfo.Properties.Count > 0)
            {
                gelfMessage.Fields = new Dictionary<string, string>();
                foreach (var kv in logEventInfo.Properties)
                {
                    if (kv.Key.ToString() == "Notes")
                    {
                        gelfMessage.Notes = kv.Value.ToString();
                    }
                    else
                    {
                        gelfMessage.Fields.Add(kv.Key.ToString(), kv.Value?.ToString());
                    }
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
            if (LogLevelMap.TryGetValue(level.Name, out var severity))
                return severity;

            return SyslogSeverity.Debug;
        }
    }
}