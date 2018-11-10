using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn
{
    public class MessageRecievedEventArgs : EventArgs
    {
        public TcpClient SourceClient { get; set; }
        public string Message { get; set; }
        public NetworkStream SourceStream { get; set; }
        public string PID { get; set; }
        public MessageRecievedEventArgs()
        {
            PID = null;
        }

    }
}
