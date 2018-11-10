using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ADTServ;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Net;
using AdtSvrCmn.Objects;
using GlobalApplicationConfigurationManager;
using MuseTcpServer;

namespace Hl7ListenerTests
{
    [TestClass]
    public class ServerTests
    {

        ServiceObject so;
        [TestInitialize]
        public void initTest()
        {
            so = new ServiceObject();
            so.Port = 9550;
            so.MessageToSend = "HelloFromTest";
            so.MessageRecived = "";
            so.ServerStarted = false;
            so.TokenSource = new CancellationTokenSource();
            ApplicationConfigurationManager configurationManager =
                new ApplicationConfigurationManager(@"C:\ProgramData\KriSoftware\ADTServerTests\settings.ini");
            so.Server = new Server(configurationManager.GetConfig(), so.TokenSource.Token);
            so.Server.ServerStarted += Server_ServerStarted;
            so.Server.MessageRecieved += Server_MessageRecieved;
            so.Server.ServerStopped += Server_ServerStopped;
        }

        [TestCleanup]
        public void Clean()
        {
                so.Server.StopServer();
        }



        //[TestMethod]
        //public void CheckServerIsStarting()
        //{
        //    so.Server.StartServerThread();
        //    Thread.Sleep(100);
        //    Assert.IsTrue(so.ServerStarted);
        //}

        //[TestMethod]
        //public void CheckServerIsRecievingAMessage()
        //{
        //    ApplicationConfigurationManager configurationManager = new ApplicationConfigurationManager(@"C:\ProgramData\KriSoftware\ADTServerTests\settings.ini");
        //    var config = configurationManager.GetConfig();
        //    so.Server.StartServerThread();
        //    Thread.Sleep(200);
        //    TcpClient client = new TcpClient();
        //    IPAddress localIp = IPAddress.Parse(config.ServerIp);
        //    client.Connect(localIp, int.Parse(config.ServerListeningPort));
        //    var stream = client.GetStream();

        //    var buffer = Encoding.UTF8.GetBytes("HelloFromTest");
        //    stream.Write(buffer, 0, buffer.Length);
        //    Thread.Sleep(550);
        //    Assert.IsTrue(so.ServerStarted);
       
        //    client.Close();
        //    so.Server.StopServer();
        //    Thread.Sleep(550);
        //    Assert.AreEqual(so.MessageToSend, so.MessageRecived); 
        //}

        //[TestMethod]
        //public void CheckServerIsStoppingWithStopCommand()
        //{

        //    so.Server.StartServerThread();
        //    Thread.Sleep(550);
        //    Assert.IsTrue(so.ServerStarted);
        //    so.Server.StopServer();
        //    Thread.Sleep(550);
        //    Assert.IsFalse(so.ServerStarted);

        //}

        //[TestMethod]
        //public void CheckServerIsStoppingWithStopCancellationToken()
        //{

        //    so.Server.StartServerThread();
        //     Thread.Sleep(550);
        //    Assert.IsTrue(so.ServerStarted);
        //    so.TokenSource.Cancel();
        //     Thread.Sleep(550);
        //    Assert.IsFalse(so.ServerStarted);

        //}


        private void Server_MessageRecieved(object sender, AdtSvrCmn.MessageRecievedEventArgs e)
        {
            this.so.MessageRecived = e.Message;
        }

        private void Server_ServerStarted(object sender, EventArgs e)
        {
            so.ServerStarted = true;
        }

        private void Server_ServerStopped(object sender, EventArgs e)
        {
            so.ServerStarted = false;
        }


    }

    public class ServiceObject
    {
        public string MessageToSend { get; set; }
        public string MessageRecived { get; set; }
        public bool ServerStarted { get; set; }
        public Server Server { get; set; }
        public int Port { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
    }
}
