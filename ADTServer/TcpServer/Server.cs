// using nlog logging

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using AdtSvrCmn;
using AdtSvrCmn.Objects;
using NLog;


namespace MuseTcpServer
{
    public class Server : IDisposable
    {
        #region Private Fields
        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        public event EventHandler ServerStarted;
        public event EventHandler MessageSent;
        public event EventHandler ServerStopped;

        TcpListener server;
        BinaryFormatter bf = new BinaryFormatter();
        CancellationToken cToken;
        List<Thread> serverThreads = new List<Thread>();
        System.Timers.Timer timer = new System.Timers.Timer();
        private bool IsServerStarted;
        private ApplicationConfiguration _configuration;
        Logger logger;
        private object locker;
        #endregion

        public Server(ApplicationConfiguration configuration, CancellationToken cancellationToken)
        {
            logger = LogManager.GetCurrentClassLogger();
            _configuration = configuration;
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 500;
            locker = new object();
            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, int.Parse(_configuration.ServerListeningPort));
                server = new TcpListener(localEndPoint);
                logger.Debug($"created Server at port {_configuration.ServerListeningPort}");
                cToken = cancellationToken;
            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message, ex, null);
            }

        }
        public bool StartServerThread()
        {
            try
            {
                //Thread serverThread = new Thread(StartServer);
                //logger.Debug($"Started server thread on thread {Thread.CurrentThread.ManagedThreadId} ");
                //serverThreads.Add(serverThread);
                //serverThread.Start();
                StartServer();
                return true;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.Message, ex, null);
                return false;
            }

        }
        public void SendResponse(string message, NetworkStream stream, TcpClient sourceClient)
        {

            lock (this)
            {
                try
                {
                    if (stream.CanTimeout)
                    {
                        stream.WriteTimeout = 5000;
                    }

                    TcpState stateOfConnection = GetConnectionState(sourceClient);
                    LogConnectionStatusForDebug(stateOfConnection, sourceClient);
                    if (stateOfConnection == TcpState.Established || stateOfConnection == TcpState.CloseWait)
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(message);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        //stream.Flush();
                        logger.Trace($"Sent Message To muse {Environment.NewLine} {message}");
                        MessageSent?.Invoke(this, new MessageSentEventArgs()
                        {
                            Message = message,
                        });
                    }
                    else
                    {
                        logger.Debug($"Error while sending response to {sourceClient.Client.RemoteEndPoint}");
                        sourceClient.Client.Dispose();

                        sourceClient.Close();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message, ex, null);
                }
                finally
                {

                    TcpState currentState = GetConnectionState(sourceClient);
                    var remote = sourceClient.Client.RemoteEndPoint.ToString();
                    logger.Debug($"Current connection state to {remote} is {currentState}");
                    logger.Debug("Closing Connection");
                    sourceClient.Client.Dispose();
                    stream.Dispose();
                    stream.Close();
                    sourceClient.Close();
                    Thread.Sleep(100);
                }
            }

        }
        private TcpState GetConnectionState(TcpClient client)
        {
            TcpState stateOfConnection = new TcpState();
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(client.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(client.Client.RemoteEndPoint)).ToArray();

            if (tcpConnections != null && tcpConnections.Length > 0)
            {
                return stateOfConnection = tcpConnections.First().State;
            }
            else
            {
                TcpState noState = TcpState.Closed;
                return noState;
            }
        }
        private void LogConnectionStatusForDebug(TcpState stateOfConnection, TcpClient sourceClient)
        {
            logger.Debug($"Current connection state to {sourceClient.Client.RemoteEndPoint} is {stateOfConnection}");
        }
        public void StopServer()
        {
            timer.Enabled = false;
            ServerStopped?.Invoke(this, EventArgs.Empty);
            logger.Debug("Tcp Server Stopped");
            server.Stop();
            IsServerStarted = false;
            Thread.Sleep(1000);
            if (serverThreads.Count > 0)
            {
                foreach (var thread in serverThreads)
                {

                    if (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.Suspended || thread.ThreadState == ThreadState.Background)
                    {
                        thread.Abort();
                    }

                }
            }

        }
        private void StartServer()
        {
            server.Start();
            timer.Enabled = true;
            ServerStarted?.Invoke(this, EventArgs.Empty);

        }

        private void HandleClient(object client)
        {

            var connectedClient = (TcpClient)client;
            logger.Debug($"Client Connected {connectedClient.Client.RemoteEndPoint}");
            logger.Debug($"Inside handle Client - connection status = {GetConnectionState(connectedClient)}");
            var stream = connectedClient.GetStream();
            logger.Debug($"Inside handle Client - connection status :  getting stream= {GetConnectionState(connectedClient)}");
            try
            {
                var cp = int.Parse(ConfigurationManager.AppSettings["CodePage"]);
                var message = GetMessageFromStream(stream,cp);
                logger.Debug($"Inside handle Client - connection status : got message from stream = {GetConnectionState(connectedClient)}");
                // raise event with the message in the eventArgs this will be handeled elsewhere
                // in the hl7Manager Class
                MessageRecievedEventArgs e = new MessageRecievedEventArgs()
                {
                    Message = message,
                    SourceClient = connectedClient,
                    SourceStream = stream
                };
                MessageRecieved?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                logger.Debug($"error getting message from stream {ex.Message}");
                logger.Error(ex, ex.Message);
            }




        }

        private string GetMessageFromStream(NetworkStream stream, int codePage)
        {
            logger.Debug($"CodePage = {codePage}");
            StringBuilder sb = new StringBuilder();
            byte[] buffer = new byte[1024];
            do
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                //sb.Append(Encoding.UTF8.GetString(buffer));
                sb.Append(Encoding.GetEncoding(codePage).GetString(buffer));

            } while (stream.DataAvailable);


            //byte[] buffer = new byte[500000];
            //stream.Read(buffer, 0, buffer.Length);
            //var message = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
            // return message;
            return sb.ToString().TrimEnd('\0');
        }

        /// <summary>
        /// Used to monitor for cancellation tokens
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //if no cancellation token issued continue listening
            if (!cToken.IsCancellationRequested)
            {
                if (!IsServerStarted)
                {
                    server.Start();
                }
                this.IsServerStarted = true; ;
                if (server.Pending())
                {
                    var client = server.AcceptTcpClient();
                    //Thread newClientThread = new Thread(HandleClient);
                    // newClientThread.Start(client);
                    HandleClient(client);
                }


            }
            else if (cToken.IsCancellationRequested == true)
            {
                StopServer();
            }


        }

        public void Dispose()
        {
            timer.Dispose();
        }

    }

}
