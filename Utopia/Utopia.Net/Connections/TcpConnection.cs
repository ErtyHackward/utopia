using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Utopia.Net.Connections
{
    /// <summary>
    /// Class represents a Tcp/Ip connection supporting different protocols
    /// </summary>
    public abstract class TcpConnection : IDisposable
    {
        #region Events

        /// <summary>
        /// Is triggered when connection status has been changed.
        /// </summary>
        public event EventHandler<ConnectionStatusEventArgs> ConnectionStatusChanged;
        
        /// <summary>
        /// Update connection status, fire event if status is changed
        /// </summary>
        /// <param name="e"></param>
        protected void SetConnectionStatus(ConnectionStatusEventArgs e)
        {
            if (e.Status != ConnectionStatus)
            {
                _connectionStatus = e.Status;
                if (ConnectionStatusChanged != null)
                    ConnectionStatusChanged(this, e);
            }
        }

        #endregion
        #region Variables
        protected bool importedSocket = false;
        protected IPEndPoint remoteAddress = null;
        protected IPEndPoint localAddress = null;
        protected Socket socket;
        protected long bufferSize = 128 * 1024;
        protected byte[] Buffer = null;
        protected volatile bool disposed = false;
        protected readonly object _sendSynObject = new object();
        protected readonly object _receiveSynObject = new object();

        // bandwidth limit
        protected static long blBytesSent;
        protected static long blBytesReceived;
        protected static DateTime blTimeMarker;
        protected static long blMaxUpload;
        protected static long blMaxDownload;

        private static long _totalReceived;
        /// <summary>
        /// Gets total bytes received from all connections
        /// </summary>
        public static long TotalBytesReceived
        {
            get { return _totalReceived; }
        }

        public int ConnectionTimeOut { get; set; }

        private static long _totalSent;
        /// <summary>
        /// Gets total bytes sent from all connections
        /// </summary>
        public static long TotalBytesSent
        {
            get { return _totalSent; }
        }
        
        /// <summary>
        /// Gets or sets upload speed limit in bytes per second. Use 0 to disable.
        /// </summary>
        public static long BandwidthUploadLimit
        {
            get { return blMaxUpload; }
            set { blMaxUpload = value; }
        }

        /// <summary>
        /// Gets or sets download speed limit in bytes per second. Use 0 to disable.
        /// </summary>
        public static long BandwidthDownloadLimit
        {
            get { return blMaxDownload; }
            set { blMaxDownload = value; }
        }

        // Thread signal, It is needed so we know when we have a connection.
        protected ManualResetEvent connectionDone = new ManualResetEvent(false);
        protected ManualResetEvent sendDone       = new ManualResetEvent(false);
        protected ManualResetEvent receiveDone    = new ManualResetEvent(false);

        // Callbacks
        protected readonly AsyncCallback receiveDataCallback;
        protected readonly AsyncCallback sendDataCallback;
        protected readonly AsyncCallback connectCallback;

        #endregion
        #region Properties

        private ConnectionStatus _connectionStatus;

        /// <summary>
        /// Gets status of the connection
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get { return _connectionStatus; }
        }

        /// <summary>
        /// Receive/Send buffer size in bytes
        /// </summary>
        public long BufferSize
        {
            get
            {
                return bufferSize;
            }
            set
            {
                if (bufferSize != value)
                {
                    bufferSize = value;
                    Buffer = new byte[bufferSize];
                }
            }
        }

        /// <summary>
        /// Local ports to be used for this connection
        /// </summary>
        public IEnumerable<int> LocalPorts { get; set; }

        /// <summary>
        /// Indicates if object has released its resources
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Remote IPEndPoint for the other party
        /// </summary>
        public IPEndPoint RemoteAddress
        {
            get { return remoteAddress; }
        }

        /// <summary>
        /// Local IPEndPoint for our party
        /// </summary>
        public IPEndPoint LocalAddress
        {
            get {
                if (localAddress == null && socket != null)
                    return localAddress = (IPEndPoint)socket.LocalEndPoint;
                return localAddress;
            }
            set { localAddress = value; }
        }

        /// <summary>
        /// Gets socket used for connection
        /// </summary>
        public Socket Socket
        {
            get { return socket; }
            internal set
            { 
                socket = value;
                importedSocket = value != null;
                if (socket != null)
                {
                    SetConnectionStatus(new ConnectionStatusEventArgs { Status = socket.Connected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected });
                }
                else SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected });
            }
        }

        public object SendSyncRoot
        {
            get { return _sendSynObject; }
        }

        public object ReceiveSyncRoot
        {
            get { return _receiveSynObject; }
        }

        #region speed calculation
        protected long lastReceivedBytes = 0;
        protected long lastCalcReceivedSpeed = DateTime.Now.Ticks;
        protected long lastCalcSendSpeed = DateTime.Now.Ticks;
        protected long lastAverageReceived = 0;
        protected long lastAverageCalc = DateTime.Now.Ticks;
        protected long lastAverageReceived1min = 0;
        protected long lastAverageCalc1min = DateTime.Now.Ticks;

        protected void UpdateAverage()
        {
            if (DateTime.Now.AddSeconds(-10).Ticks > lastAverageCalc1min)
            {
                lastAverageReceived = lastAverageReceived1min;
                lastAverageReceived1min = TotalBytesReceived;
                lastAverageCalc = lastAverageCalc1min;
                lastAverageCalc1min = DateTime.Now.Ticks;
            }
        }

        public double AverageReceiveSpeed
        {
            get
            {
                UpdateAverage();
                long tmpBytes = TotalBytesReceived - lastAverageReceived;

                long now = DateTime.Now.Ticks;
                // Get how long time has passed since last time
                var tmpTime = new TimeSpan(now - lastAverageCalc);
                if (tmpBytes <= 0)
                    return 0;   // No new data
                double value = tmpBytes / tmpTime.TotalSeconds;
                if (double.IsInfinity(value))
                {
                    return -1;
                }
                return value;
            }
        }
        #endregion

        #endregion
        #region Constructor(s)/Deconstructor/Dispose
        /// <summary>
        /// Creating TcpConnection
        /// </summary>
        public TcpConnection()
        {
            receiveDataCallback = new AsyncCallback(OnRecievedData);
            sendDataCallback = new AsyncCallback(OnSendData);
            connectCallback = new AsyncCallback(OnConnect);
            Buffer = new byte[bufferSize];
            _connectionStatus = ConnectionStatus.Disconnected;
        }

        /// <summary>
        /// Creating TcpConnection with s as the underlaying socket
        /// </summary>
        /// <param name="socket">Socket you want to have as underlaying connection</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TcpConnection(Socket socket)
            : this()
        {
            if (socket == null) 
                throw new ArgumentNullException("socket");

            this.socket = socket;

            // no need to fire event in constructor
            _connectionStatus = socket.Connected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
            
            importedSocket = true;
        }

        /// <summary>
        /// Creating TcpConnection with addy as the remote address.
        /// </summary>
        /// <param name="addy">IPEndPoint that we want as our remote address</param>
        public TcpConnection(System.Net.IPEndPoint addy)
            : this()
        {
            remoteAddress = addy;
        }

        /// <summary>
        /// Creating TcpConnection with address and port.
        /// </summary>
        /// <param name="address">String representation of a IP/DNS address</param>
        /// <param name="prt">Int port representation</param>
        [DebuggerStepThrough()]
        public TcpConnection(string address, int prt)
            : this()
        {
            System.Net.IPAddress addy = null;
            try
            {
                if (!System.Net.IPAddress.TryParse(address, out addy))
                {
                    addy = System.Net.Dns.GetHostEntry(address).AddressList[0];
                }
            }
            catch (System.Exception)
            {
                return;
            }
            remoteAddress = new IPEndPoint(addy, prt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [DebuggerStepThrough()]
        protected IPEndPoint ParseAddress(string address)
        {
            int port = 4815;
            int i = address.IndexOf(':');
            if (i != -1)
            {
                port = int.Parse(address.Substring(i + 1));
                address = address.Remove(i, address.Length - i);
            }
            IPAddress addy = null;
            try
            {
                if (!System.Net.IPAddress.TryParse(address, out addy))
                {
                    addy = System.Net.Dns.GetHostEntry(address).AddressList[0];
                }
            }
            catch (System.Exception)
            {
                return null;
            }
            return new IPEndPoint(addy, port);
        }

        /// <summary>
        /// Creating TcpConnection with address, port and buffer size
        /// </summary>
        /// <param name="address">String representation of a IP/DNS address</param>
        /// <param name="prt">Int port representation</param>
        /// <param name="bufferSize">Bytes in receive/send buffer</param>
        public TcpConnection(string address, int prt, long bufferSize)
            :this(address, prt)
        {
            this.bufferSize = bufferSize;
        }

        ~TcpConnection()
        {
            Dispose(false);
        }


        /// <summary>
        /// Releases all resources used by this connection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if (disposing)
                {
                    lock (_sendSynObject)
                    {
                        lock (_receiveSynObject)
                        {
                            Disconnect(DisconnectReason.Dispose);

                            Buffer = null;
                            localAddress = null;
                            remoteAddress = null;

                            connectionDone.Dispose();
                            receiveDone.Dispose();
                            sendDone.Dispose();


                            socket = null;

                            disposed = true;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Prepares object for another use
        /// </summary>
        public virtual void Recycle()
        {
            if (socket != null && socket.Connected)
            {
                Disconnect(DisconnectReason.Dispose);
            }
            Socket = null;
        }

        /// <summary>
        /// Begins dispose operation in another thread
        /// </summary>
        public virtual void BeginDispose()
        {
            new ThreadStart(Dispose).BeginInvoke(null, null);
        }

        #endregion
        #region Functions
        #region Connect

        /// <summary>
        /// Starts connection process in asyncronus mode
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="cbData"></param>
        public virtual void ConnectAsync()
        {
            var ts = new ThreadStart(Connect);
            ts.BeginInvoke(null, null);
        }

        /// <summary>
        /// Starts connection process in asyncronus mode
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="cbData"></param>
        public virtual void ConnectAsync(AsyncCallback callback, object cbData)
        {
            var ts = new ThreadStart(Connect);
            ts.BeginInvoke(callback, cbData);
        }
        
        /// <summary>
        /// Creates connection to server.
        /// </summary>
        public virtual void Connect()
        {
            if (disposed)
                throw new ObjectDisposedException("TcpConnection");

            // Change Connection status.
            SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Connecting });

            if (remoteAddress == null)
            {
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = new Exception("Invalid IP address !!") });
                return;
            }

            try
            {
                // Establish Connection
                socket = new Socket(remoteAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
                importedSocket = false;
                #region LocalPortBinding
                if (LocalPorts != null)
                {
                    bool binded = false;
                    foreach (var localPort in LocalPorts)
                    {
                        try
                        {
                            socket.Bind(new IPEndPoint(IPAddress.Any, localPort));
                            Debug.WriteLine(string.Format("Port {0} binded", localPort));
                            binded = true;
                            break;
                        }
                        catch (SocketException) //
                        {

                        }
                    }
                    if (!binded)
                    {
                        //error
                        Debug.WriteLine("Unable to open local ports");
                    }
                }
                #endregion

                // It is needed so we know when we have a connection.
                connectionDone.Reset();
                
                socket.BeginConnect(remoteAddress, connectCallback, socket);
                
                // Waits until we have a connection...
                //Connect with time out
                if (ConnectionTimeOut > 0)
                {
                    if (!connectionDone.WaitOne(ConnectionTimeOut, false))
                    {
                        // Time Out !
                        SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = new Exception("Server connection timeout") });
                        return;
                    }
                }
                else
                {
                    connectionDone.WaitOne();
                }

                // tell everyone that we done
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = socket.Connected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected });

                if (socket.Connected)
                {
                    // sending inital commands
                    SentFirstCommands();
                    
                    // start receiving
                    receiveDone.Reset();
                    SetupReceiveCallback(socket);
                    // this needs to work properly in Windows XP, it cancels receive operation if thread exits
                    receiveDone.WaitOne();
                }

            }
            catch (System.Exception e2)
            {
                // Change Connection Status
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = e2 });
            }
        }

        protected virtual void SentFirstCommands()
        {

        }

        /// <summary>
        /// Disconnects connection
        /// </summary>
        public void Disconnect()
        {
            Disconnect(DisconnectReason.Unknown);
        }

        /// <summary>
        /// Disconnects connection
        /// </summary>
        /// <param name="reason">Message that will be sent out in the ConnectionStatusChange event</param>
        public virtual void Disconnect(DisconnectReason reason)
        {
            if (ConnectionStatus == ConnectionStatus.Disconnected || ConnectionStatus == ConnectionStatus.Disconnecting)
            {
                return;
            }
            SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnecting, Reason = reason });

            bool locked = false;
            try
            {
                // we want to immediately disconnect, lets set linger 0 seconds
                var myOpts = new LingerOption(true, 0);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, myOpts);
                socket.Close();

                // possible there is active Protocol processing in another thread, we need to wait before it finishes
                Monitor.Enter(_sendSynObject);
                Monitor.Enter(_receiveSynObject);
                locked = true;
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Reason = reason });
            }
            catch (Exception x)
            {
                SetConnectionStatus(new ConnectionStatusEventArgs{ Status = ConnectionStatus.Disconnected, Exception = x});
            }
            finally
            {
                if (locked)
                {
                    Monitor.Exit(_receiveSynObject);
                    Monitor.Exit(_sendSynObject);
                }
            }
        }

        protected virtual void OnConnect(System.IAsyncResult ar)
        {
            try
            {
                Socket sock = (Socket)ar.AsyncState;
                sock.EndConnect(ar);
            }
            catch (Exception se)
            {
                // Change Connection Status
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = se });
            }
            finally
            {
                // unblocking caller thread
                if(connectionDone != null)
                    connectionDone.Set();
            }
        }
        #endregion
        #region Receive


        /// <summary>
        /// Setup the callback for recieved data and loss of conneciton
        /// </summary>
        protected virtual void SetupReceiveCallback(Socket sock)
        {
            try
            {
                if (sock.Connected)
                {
                    sock.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, receiveDataCallback, sock);
                }
                else
                {
                    SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Reason = DisconnectReason.Unknown});
                }
            }
            catch (Exception e)
            {
                // releasing connect thread
                receiveDone.Set();
                // Change Connection Status
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = e });
            }
        }

        public abstract void ParseRaw(byte[] buffer, int length);
        

        /// <summary>
        /// Get the new data and send it out to all other connections. 
        /// Note: If no data was recieved the connection has probably 
        /// died.
        /// </summary>
        /// <param name="ar"></param>
        protected virtual void OnRecievedData(IAsyncResult ar)
        {
            if (disposed) return;
            lock (_receiveSynObject)
            {
                if (disposed) return;
                // Socket was the passed in object
                Socket sock = ar.AsyncState as Socket;
                int nBytesRec = 0;
                // Check if we got any data
                try
                {
                    if (!sock.Connected)
                    {
                        SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected });
                        return;
                    }
                    nBytesRec = sock.EndReceive(ar);
                    if (nBytesRec > 0)
                    {
                        blBytesReceived += nBytesRec;
                        _totalReceived += nBytesRec;
                        // Send data to protocol.

                        ParseRaw(Buffer, nBytesRec);

                        while (blMaxDownload > 0 && blBytesReceived >= blMaxDownload)
                        {
                            if (blTimeMarker.AddSeconds(1) < DateTime.Now)
                            {
                                blTimeMarker = DateTime.Now;
                                blBytesSent = 0;
                                blBytesReceived = 0;
                            }
                            var waitFactor = (float)blBytesReceived / blMaxDownload;
                            if (waitFactor > 1)
                            {
                                var sleep = (blTimeMarker.AddSeconds(waitFactor) - DateTime.Now).Milliseconds;
                                Thread.Sleep(sleep);
                            }
                        }
                        // If the connection is still usable restablish the callback
                        SetupReceiveCallback(sock);
                    }
                    else
                    {
                        // If no data was recieved then the connection is probably dead
                        // Change Connection Status
                        SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected });
                    }
                }
                catch (System.ObjectDisposedException ex)
                {
                    SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = ex });
                }
                catch (SocketException ex)
                {
                    SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = ex });
                }
                finally
                {
                    receiveDone.Set();
                }
            }
        }
        #endregion

        /// <summary>
        /// If connection exist it will be closed.
        /// Then a new connection attempt will be made
        /// </summary>
        public virtual void Reconnect()
        {
            if (socket != null && socket.Connected)
            {
                Disconnect(DisconnectReason.Reconnecting);
            }
            Connect();
        }

        /// <summary>
        /// Sets TcpConnection in a listening mode waiting for a message.
        /// NOTE: This function can't be used if you havnt used the TcpConnection(Socket s) constructor.
        /// </summary>
        public virtual void Listen()
        {
            lock (_receiveSynObject)
            {

                if (!importedSocket)
                    throw new InvalidOperationException("To call this function you need to have created this object with the TcpConnection(Socket s) constructor");
                if (socket.Connected)
                {
                    localAddress = (IPEndPoint)socket.LocalEndPoint;
                    remoteAddress = (IPEndPoint)socket.RemoteEndPoint;

                    SetupReceiveCallback(socket);
                    // Change Connection Status
                    SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Connected });
                }
            }
        }

        #region Send
        /// <summary>
        /// Sends Raw from msg to server.
        /// </summary>
        /// <param name="msg">Message where </param>
        public virtual bool Send(byte[] bytes, int length)
        {
            lock (_sendSynObject)
            {
                return InternalSend(bytes, length, false);
            }
        }

        /// <summary>
        /// Sends byte[] to server.
        /// </summary>
        /// <param name="raw">byte[] that will be sent to server</param>
        public virtual bool Send(byte[] raw)
        {
            return InternalSend(raw,raw.Length, true);
        }

        protected virtual bool InternalSend(byte[] raw, int length, bool lockResource)
        {
            if (socket == null || !socket.Connected)
                return false;

            if (lockResource)
                Monitor.Enter(_sendSynObject);

            try
            {
                if (raw == null || length <= 0)
                    return false;

                #region Speed Limiter
                while (blMaxUpload > 0 && blBytesSent >= blMaxUpload)
                {
                    if (blTimeMarker.AddSeconds(1) < DateTime.Now)
                    {
                        blTimeMarker = DateTime.Now;
                        blBytesSent = 0;
                        blBytesReceived = 0;
                    }
                    var waitFactor = (float)blBytesSent / blMaxUpload;
                    if (waitFactor > 1)
                    {
                        var sleep = (blTimeMarker.AddSeconds(waitFactor) - DateTime.Now).Milliseconds;
                        Thread.Sleep(sleep);
                    }
                }
                #endregion

                sendDone.Reset();
                socket.BeginSend(raw, 0, length, SocketFlags.None, sendDataCallback, socket);
                sendDone.WaitOne();
                return true;
            }
            catch (ObjectDisposedException se)
            {
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = se });
            }
            catch (SocketException se)
            {
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = se });
            }
            finally
            {
                if (lockResource)
                    Monitor.Exit(_sendSynObject);
                    //System.Threading.Monitor.Exit(synObject);
            }
            return false;
        }

        protected virtual void OnSendData(System.IAsyncResult async)
        {
            Socket handler = async.AsyncState as Socket;
            try
            {
                var bytes = handler.EndSend(async);
                blBytesSent += bytes;
                _totalSent += bytes;
            }
            catch (ObjectDisposedException ex)
            {
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = ex });
            }
            catch (SocketException se)
            {
                SetConnectionStatus(new ConnectionStatusEventArgs { Status = ConnectionStatus.Disconnected, Exception = se });
            }
            finally
            {
                sendDone.Set();
            }
        }
        #endregion

        #endregion

    }

    public enum ConnectionStatus
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected
    }

    public enum DisconnectReason
    {
        Unknown,
        UserRequest,
        TooManyConnections,
        Dispose,
        Reconnecting,
        Inactivity,
        RequestDeny
    }

    public class ConnectionStatusEventArgs : EventArgs
    {
        public ConnectionStatus Status { get; set; }
        public DisconnectReason Reason { get; set; }
        public Exception Exception { get; set; }
    }
}
