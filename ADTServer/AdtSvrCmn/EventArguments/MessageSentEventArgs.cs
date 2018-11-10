using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AdtSvrCmn
{
   public class MessageSentEventArgs :EventArgs
    {
        public string Message { get; set; }
        public string PID { get; set; }
        public TcpClient TargetClient { get; set; }
        public MessageSentEventArgs()
        {
            PID = null;
        }

    }
}
