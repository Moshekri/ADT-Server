using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeumitADTTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string message = File.ReadAllText(@"C:\Users\MuseAdmin\Documents\Service\Applications\Test Client\Queries\QRY_Q01.txt");
            var buffer = Encoding.UTF8.GetBytes(message);

            TcpClient c = new TcpClient("localhost", 9001);
            c.Connect("localhost", 9005);
            c.GetStream().Write(buffer, 0, buffer.Length);
            c.Close();
        }
    }
}
