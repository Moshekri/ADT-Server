using System;
using System.IO;
using System.Text;
using AdtSvrCmn.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ADTServ;
using MuseHl7Parser;

namespace hl7ParserTests
{
    [TestClass]
    public class ParserTests
    {

        IHl7Parser parser;

        [TestInitialize]
        public void init()
        {
            parser = new Hl7Parser();

        }

        [TestMethod]
        public void TestPatientIdParsingQRYQ01()
        {
            var message = File.ReadAllText(@"TestResources\QRY_Q01.txt");
            var id = parser.GetPatientId(message);
            var messgaeCtrlI = parser.GetMessageControlId(message);
            var datetime = parser.GetMessageDateTime(message);
            Assert.AreEqual(datetime, "20171113210423");
            var msgType = parser.GetMessageType(message);
            var siteId = parser.GetSiteID(message);
            Assert.AreEqual("5555", id);


        }
        [TestMethod]
        public void TestPatientIdParsingORUR01()
        {
           var message = File.ReadAllText(@"TestResources\Oru_r01.txt");
            var id = parser.GetPatientId(message);
            var messgaeCtrlI = parser.GetMessageControlId(message);
            var datetime = parser.GetMessageDateTime(message);
            var msgType = parser.GetMessageType(message);
            var siteId = parser.GetSiteID(message);
            Assert.AreEqual("5555", id);


        }
    }

}