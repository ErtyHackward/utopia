using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;

namespace Utopia.Shared.Net.Connections
{
    public sealed class TcpConnectionListener : IDisposable
    {
        private readonly int _maxDependingConnections = 10;
        private readonly Socket _listenSocket;
        private readonly IPEndPoint _ep;
        private readonly AsyncCallback startTransfer;

        /// <summary>
        /// Occurs when we receive a new connection, if event is not handled socket will be closed
        /// </summary>
        public event EventHandler<IncomingConnectionEventArgs> IncomingConnection;

        private void OnIncomingConnection(IncomingConnectionEventArgs e)
        {
            if (IncomingConnection != null)
                IncomingConnection(this, e);
        }

        /// <summary>
        /// Local port to open to listen incoming connections
        /// </summary>
        public int Port { get; set; }

        private TcpConnectionListener()
        {
            startTransfer = OnStartConnection;
        }
        
        /// <summary>
        /// Creates new TcpListener on port specified
        /// </summary>
        /// <param name="port"></param>
        public TcpConnectionListener(int port, int maxDependingConnections = 10)
            : this()
        {
            _maxDependingConnections = maxDependingConnections;
            _listenSocket = new Socket(
                                        AddressFamily.InterNetwork,
                                        SocketType.Stream,
                                        ProtocolType.Tcp);
            _ep = new IPEndPoint(IPAddress.Any, port);
            _listenSocket.Bind(_ep);
            Port = port;
        }

        public void Start()
        {
            try
            {
                // start listening
                _listenSocket.Listen(_maxDependingConnections);
                SetupConnection(_listenSocket);
            }
            catch (ObjectDisposedException) { /* End has been called */ }
            catch (System.Net.Sockets.SocketException) { }
        }

        public void End()
        {
            _listenSocket.Close();
        }


        private void SetupConnection(Socket sc)
        {
            try
            {
                sc.BeginAccept(startTransfer, sc);
            }
            catch (ObjectDisposedException) { /* End has been called */ }
            catch (System.Net.Sockets.SocketException) { }
        }

        [DebuggerStepThrough]
        private void OnStartConnection(IAsyncResult ar)
        {
            try
            {
                var s = (Socket)ar.AsyncState;
                var incomingSocket = s.EndAccept(ar);
                var ea = new IncomingConnectionEventArgs { Socket = incomingSocket };
                OnIncomingConnection(ea);

                if (!ea.Handled)
                {
                    incomingSocket.Disconnect(false); // DisconnectAsync(new SocketAsyncEventArgs());
                }
                
                // continue accepting
                SetupConnection(s);
            }
            catch (ObjectDisposedException) { /* End has been called */ }
            catch (System.Net.Sockets.SocketException) {
                try
                {
                    var s = (Socket)ar.AsyncState;
                    SetupConnection(s);
                }
                catch { }
            }
        }

        #region Members of IDisposable

        public void Dispose()
        {
            End();
        }

        #endregion

        public static bool IsPortFree(int port)
        {
            Socket socket = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);
                var ep = new IPEndPoint(IPAddress.Any, port);
                socket.Bind(ep);

                var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                foreach (var tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == port)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (socket != null)
                    socket.Dispose();
            }
        }
        
    }

    public class IncomingConnectionEventArgs : BaseEventArgs
    {
        public Socket Socket { get; set; }

    }

    /// <summary>
    /// Event that can be handled
    /// </summary>
    public class BaseEventArgs : EventArgs
    {
        public bool Handled { get; set; }
    }
}
