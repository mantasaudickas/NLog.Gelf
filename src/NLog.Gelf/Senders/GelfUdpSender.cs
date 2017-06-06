using System.Net.Sockets;
using System.Text;

namespace NLog.Gelf.Senders
{
    public class GelfUdpSender : GelfBaseSender
    {
        private readonly int _port;

        public GelfUdpSender(string server, int port, bool debugEnabled = false)
            : base (server, debugEnabled)
        {
            this._port = port;
        }

        protected override bool Send(string message)
        {
            using (var udpClient = new UdpClient())
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                return udpClient.SendAsync(bytes, bytes.Length, this.ServerUrl, this._port).Result > 0;
            }
        }
    }
}