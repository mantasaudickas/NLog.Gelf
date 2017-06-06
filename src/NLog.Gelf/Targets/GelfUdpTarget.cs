using NLog.Targets;
using NLog.Gelf.Senders;

namespace NLog.Gelf
{
    [Target("GelfUdp")]
    public class GelfUdpTarget : GelfBaseTarget
    {
        private GelfBaseSender _gelfSender;

        protected override GelfBaseSender Sender
        {
            get
            {
                if (_gelfSender == null)
                {
                    var debugConfig = (Debug ?? string.Empty).ToLowerInvariant();
                    var debugEnabled = debugConfig == "true" || debugConfig == "1";

                    lock (this)
                    {
                        _gelfSender = new GelfUdpSender(ServerUrl, Port, debugEnabled);
                    }
                }

                return _gelfSender;
            }
        }
    }
}