using System;
using AdtSvrCmn;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Fluent;


namespace LoggingTests
{
    [TestClass]
    public class NloggingTests
    {
        Logger logger;
        [TestInitialize]
        public void initTests()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        [TestMethod]
        public void WriteErrorMessage()
        {
            NlogHelper.CreateLogEntry("testMsg", "122", LogLevel.Error, logger);
        }
    }
}
