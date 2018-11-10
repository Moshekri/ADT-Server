using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdtSvrCmn.Interfaces;
using LeumitPatientIdParser;

namespace PatientIdParsingTests
{
    [TestClass]
    public class PatientIdParserTests
    {
        public object IsraeliIdTools { get; private set; }

        [TestMethod]
        public void ParseWhenCardWasSwiped()
        {
            PidParser pidParser = new PidParser();

            var pid = pidParser.ParseID("1023456783");

            Assert.AreEqual(pid[0].ID, "02345678");
            Assert.AreEqual(pid[0].SugId, "1");
            Assert.AreEqual(pid[0].SifratBikuret, "3");
        }
        [TestMethod]
        public void ParseWhenGoodIdEnterdManuallyWithSifratBikoret()
        {
            PidParser pidParser = new PidParser();

            var pid = pidParser.ParseID("038664488");

            Assert.AreEqual(pid[0].ID, "03866448");
            Assert.AreEqual(pid[0].SugId, "1");
            Assert.AreEqual(pid[0].SifratBikuret, "8");
        }
        [TestMethod]
        public void ParseWhenGoodIdEnterdManuallyWithSifratBikoret1()
        {
            PidParser pidParser = new PidParser();

            var pid = pidParser.ParseID("023788581");

            Assert.AreEqual(pid[0].ID, "02378858");
            Assert.AreEqual(pid[0].SugId, "1");
            Assert.AreEqual(pid[0].SifratBikuret, "1");
        }
        [TestMethod]
        public void ParseWhenBadIdEnterdManuallyWithoutSifratBikoret()
        {
            PidParser pidParser = new PidParser();

            var pid = pidParser.ParseID("03866448");

            Assert.AreEqual(pid[0].ID, "03866448");
            Assert.AreEqual(pid[0].SugId, "1");
            Assert.AreEqual(pid[0].SifratBikuret, "8");
        }
        [TestMethod]
        public void WhenPidIsCorrectShouldGet03866448()
        {
            string pid = "1038664488";
            IPatientIdHandler patientIdHandler = new PidParser();
            var data = patientIdHandler.ParseID(pid);
            Assert.AreEqual(data[0].ID, "03866448");
            Assert.AreEqual(data[0].SugId, "1");
            Assert.AreEqual(data[0].SifratBikuret, "8");

        }
        [TestMethod]
        public void TestVariousPid()
        {



            PidParser parser = new PidParser();
            var res = parser.ParseID("1038664488");
            Assert.AreEqual("03866448", res[0].ID);
            Assert.AreEqual("8", res[0].SifratBikuret);
            Assert.AreEqual("1", res[0].SugId);
            Assert.IsTrue(res[0].IsValidIsraeliId);


            var res1 = parser.ParseID("0038664488");
            Assert.AreEqual("03866448", res1[0].ID);
            Assert.AreEqual("8", res1[0].SifratBikuret);
            Assert.AreEqual("1", res1[0].SugId);
            Assert.IsTrue(res1[0].IsValidIsraeliId);

            var res2 = parser.ParseID("9038664488");
            Assert.AreEqual("03866448", res2[0].ID);
            Assert.IsTrue(res2[0].IsValidIsraeliId);
            Assert.AreEqual("9", res2[0].SugId);
            Assert.AreEqual("8", res2[0].SifratBikuret);

            var res3 = parser.ParseID("0000122334");
            Assert.AreEqual("00122334", res3[0].ID);
            Assert.AreEqual("6", res3[0].SifratBikuret);
            Assert.IsFalse(res3[0].IsValidIsraeliId);
            Assert.AreEqual("1", res3[0].SugId);
            Assert.AreEqual("00122334", res3[1].ID);
            Assert.AreEqual("9", res3[1].SugId);
            Assert.AreEqual(string.Empty, res3[1].SifratBikuret);
            // Assert.AreEqual

            var res4 = parser.ParseID("23452345123");
            Assert.IsNull(res4[0]);
            Assert.IsNull(res4[1]);


            var res5 = parser.ParseID("0000000012");
            Assert.AreEqual("00000012", res5[0].ID);
            Assert.AreEqual("5", res5[0].SifratBikuret);
            Assert.IsFalse(res5[0].IsValidIsraeliId);
            Assert.AreEqual("1", res5[0].SugId);
            Assert.AreEqual("00000012", res5[1].ID);
            Assert.AreEqual("9", res5[1].SugId);
            Assert.AreEqual(string.Empty, res5[1].SifratBikuret);


            var res6 = parser.ParseID("12");
            Assert.AreEqual("00000012", res6[0].ID);
            Assert.AreEqual("5", res6[0].SifratBikuret);
            Assert.IsFalse(res6[0].IsValidIsraeliId);
            Assert.AreEqual("1", res6[0].SugId);
            Assert.AreEqual("00000012", res6[1].ID);
            Assert.AreEqual("9", res6[1].SugId);
            Assert.AreEqual(string.Empty, res6[1].SifratBikuret);

            var res7 = parser.ParseID("21");
            Assert.AreEqual("00000021", res7[0].ID);
            Assert.AreEqual("6", res7[0].SifratBikuret);
            Assert.IsFalse(res7[0].IsValidIsraeliId);
            Assert.AreEqual("1", res7[0].SugId);
            Assert.AreEqual("00000021", res7[1].ID);
            Assert.AreEqual("9", res7[1].SugId);
            Assert.AreEqual(string.Empty, res7[1].SifratBikuret);

            res7 = parser.ParseID("1");
            Assert.AreEqual("00000001", res7[0].ID);
            Assert.AreEqual("8", res7[0].SifratBikuret);
            Assert.IsFalse(res7[0].IsValidIsraeliId);
            Assert.AreEqual("1", res7[0].SugId);
            Assert.AreEqual("00000001", res7[1].ID);
            Assert.AreEqual("9", res7[1].SugId);
            Assert.AreEqual(string.Empty, res7[1].SifratBikuret);

            res7 = parser.ParseID("test demo");
            Assert.IsNull(res7[0]);
            Assert.IsNull(res7[1]);
                  

        }
        [TestMethod]
        public void WhenPidIsNotInCorrectFormatShouldGet03866448()
        {
            string pid = "038664488";
            IPatientIdHandler patientIdHandler = new PidParser();
            var data = patientIdHandler.ParseID(pid);
            Assert.AreEqual(data[0].ID, "03866448");
            Assert.AreEqual(data[0].SugId, "1");
            Assert.AreEqual(data[0].SifratBikuret, "8");

            pid = "38664488";
            data = patientIdHandler.ParseID(pid);
            Assert.AreEqual(data[0].ID, "03866448");
            Assert.AreEqual(data[0].SugId, "1");
            Assert.AreEqual(data[0].SifratBikuret, "8");
        }
    }
}
