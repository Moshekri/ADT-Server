using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdtSvrCmn.Interfaces;

namespace MuseHl7Parser 
{
    public class Hl7Parser :IHl7Parser
    {
       static char[] splitters = new char[] { '\r', '\n' };

        public  string GetPatientId(string message)
        {
            
            string PID = "";
            var msgtype = GetMessageType(message);
            if (msgtype.ToLower() == "qry^q01" )
            {
               
                var lines = message.Split(splitters);
                foreach (var line in lines)
                {
                    var fields = line.Split('|');
                    if (fields[0].ToLower() == "qrd")
                    {
                        var pidField = fields[8];
                        PID = pidField.Split('^')[0];
                        break;
                    }
                }
               
            }
            else if (msgtype.ToLower() == "adt^a19")
            {
                
                var lines = message.Split(splitters);
                foreach (var line in lines)
                {
                    var fields = line.Split('|');
                    if (fields[0].ToLower() == "pid")
                    {
                         PID = fields[3];
                        
                        break;
                    }
                }
             
            }
            else if (msgtype.ToLower() == "oru^r01")
            {

                var lines = message.Split(splitters);
                foreach (var line in lines)
                {
                    var fields = line.Split('|');
                    if (fields[0].ToLower() == "pid")
                    {
                        PID = fields[2];

                        break;
                    }
                }

            }
            return PID;
        }
        public  string GetSiteID(string message)
        {
            string SiteID = "";
            var lines = message.Split(splitters);
            foreach (var line in lines)
            {
                var fields = line.Split('|');
                if (fields[0].ToLower() == "\vmsh" || fields[0].ToLower() == "msh")
                {
                    SiteID= fields[3];
                    break;
                }
            }
            return SiteID;
        }
        public  string GetMessageDateTime(string message)
        {
            string date = "";
            var lines = message.Split(splitters);
            foreach (var line in lines)
            {
                var fields = line.Split('|');
                if (fields[0].ToLower() == "\vmsh" || fields[0].ToLower() == "msh")
                {
                    date = fields[6];

                }
            }
            return date;
        }
        public  string GetMessageControlId(string message)
        {
            string msgCtrlId = "";
            var lines = message.Split(splitters);
            foreach (var line in lines)
            {
                var fields = line.Split('|');
                if (fields[0].ToLower() == "\vmsh" || fields[0].ToLower() == "msh")
                {
                    msgCtrlId = fields[9];
                    break;

                }
            }
            return msgCtrlId;
        }
        public  string GetMessageType(string message)
        {
            string msgType = "";
            var lines = message.Split(splitters);
            foreach (var line in lines)
            {
                var fields = line.Split('|');
                if (fields[0].ToLower() == "\vmsh" || fields[0].ToLower() == "msh")
                {
                    msgType = fields[8];
                    break;

                }
            }
            return msgType;
        }

        
    }
}
