using System;
using System.Text;

namespace ADTServ
{
    public class MessageComposer
    {
       

        public static string GetA019Message(string pid, string siteId, string fName, string lName, string gender, string DOB, string messageControlId,string height,string weight)
        {

            StringBuilder message = new StringBuilder();
            message.Append( '\v');

            message.Append( @"MSH|^~\\&|||MUSE|" + siteId + "|" + DateTime.Now.ToFileTimeUtc() + "||ADT^A19|" + messageControlId + "|P|2.4|");
            message.Append( '\r');

            message.Append( @"MSA|AA|" + messageControlId + "|||");
            message.Append( '\r');

            message.Append( "QRD|||||||||||||");
            message.Append( '\r');

            message.Append( @"EVN|A19|" + DateTime.Now.ToFileTimeUtc() + "|");
            message.Append( '\r');
            

            message.Append( @"PID|||" + pid + "||" + lName + "^" + fName + "||" + DOB.ToString() + "|" + gender + "||||||||||" + messageControlId + "|");
            message.Append( '\r');

            
            
            message.Append( @"PV1||||||||||||||||||1|1|1|||||||||||||||||||||||||||||||||");
            message.Append('\r');

            message.Append($"OBX|1||||^{height}| ");
            message.Append('\r');

            message.Append($"OBX|2||||^{weight}|");
            message.Append('\r');


            
            message.Append( '\u001c');
            message.Append( '\r');


            var msg = message.ToString();
            return message.ToString();
        }
        public static string GetApplicationErrorMessage(string siteId, string messageControlId)
        {
            StringBuilder message = new StringBuilder();
            message.Append( '\v');

            message.Append( @"MSH|^~\\&|||MUSE|" + siteId + "|" + DateTime.Now.ToFileTimeUtc() + "||ADT^A19|" + messageControlId + "|P|2.4|");
            message.Append( '\r');

            message.Append( @"MSA|AE|" + messageControlId + "|||");
            message.Append( '\r');

            message.Append( '\r');
            message.Append( '\u001c');
            message.Append( '\r');

           
            return message.ToString();
        }

    }
}
