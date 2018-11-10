using System;
using System.IO;
using AdtSvrCmn.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ApplicationLogger;

namespace LoggerTests
{


    [TestClass]
    public class LoggerTests
    {
        private IApplicationLogger logger = Log.GetInstance(@"c:\programdata\tests", "Log.txt");
        private const string line = "This is a test entry";
        [TestMethod]
        public void TestWritingLineToLog()
        {
            bool foundLine = false;
            logger.MakeLogEntry(line, "Test");
            string[] lines = File.ReadAllLines(@"c:\programdata\tests\Log.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(line))
                {
                    foundLine = true;
                }
            }
            Assert.AreEqual(true, foundLine);
        }
        [TestMethod]
        public void TestWritingExeptionToLog()
        {
            bool writeOk = false;

            try
            {
                int four = int.Parse("r");
            }
            catch (Exception e)
            {
                logger.LogExecption(e, "test exeption wrti to log");
            }
            string[] lines = File.ReadAllLines(@"c:\programdata\tests\Log.txt");
            for (int i = 0; i < lines.Length; i++)
            {
               if (lines[i].Contains("test exeption wrti to log")) 
                {
                    writeOk = true;
                }
            }
            Assert.IsTrue(writeOk);
        }
    }
}
