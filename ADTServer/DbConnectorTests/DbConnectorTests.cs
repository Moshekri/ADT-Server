using System.Collections.Generic;
using System.IO;
using AdtSvrCmn.Interfaces;
using DbLayer;
using GlobalApplicationConfigurationManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdtSvrCmn.Objects;

namespace DbConnectorTests
{
    [TestClass]
    public class DbTests
    {
        private IDbConnector dbCon;
        private string FullDbFilePath;
        ApplicationConfiguration con;

        [TestInitialize]
        public void Init()
        {
            ApplicationConfigurationManager config = new ApplicationConfigurationManager(@"C:\ProgramData\KriSoftware\ADTServerTests\settings.ini");
            con = config.GetConfig();
            //var testPath = Directory.CreateDirectory(@"C:\ProgramData\tests");
            //DbPath = testPath.FullName;
            //con.DataBaseFolder = DbPath;
            //dbName = "Test.db";
            FullDbFilePath = Path.Combine(con.DataBaseFolder, con.DataBaseFileName);
            dbCon = new Db(con);
        }

        [TestMethod]
        public void ShouldReturnNewDictionaryWhenNoDbFileExistsAndCreateANewDbFile()
        {
            if (File.Exists(FullDbFilePath))
            {
                File.Delete(FullDbFilePath);
                Assert.IsFalse(File.Exists(FullDbFilePath));
            }
            var data = dbCon.GetData();
            var newDic = new Dictionary<string, string>();
            Assert.AreEqual(data.Count, 0);
            Assert.AreEqual(data.GetType(), newDic.GetType());
            Assert.IsTrue(File.Exists(FullDbFilePath));
        }

        [TestMethod]
        public void ShouldSaveDataToExistingDb()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            if (!File.Exists(FullDbFilePath))
            {
                data = dbCon.GetData();
            }

            data.Add("משה", "Moshe");
            dbCon.SaveData(data);
            var names = dbCon.GetData();

            string name;
            names.TryGetValue("משה", out name);
            Assert.AreEqual("Moshe", name);
        }

        [TestMethod]
        public void ShouldGetDataFromExistingDb()
        {
            var names = dbCon.GetData();
            string name;
            names.TryGetValue("משה", out name);
            Assert.AreEqual("Moshe", name);
        }

        [TestMethod]
        public void ShouldBeAbleTosaveDAtaWhenNoDbFileExists()
        {
            if (File.Exists(FullDbFilePath))
            {
                File.Delete(FullDbFilePath);
            }
            if (!File.Exists(FullDbFilePath))
            {
                dbCon.SaveData(new Dictionary<string, string>() { { "דרור", "Dror" } });
            }
            Assert.AreEqual(true, File.Exists(FullDbFilePath));
        }

    }
}
