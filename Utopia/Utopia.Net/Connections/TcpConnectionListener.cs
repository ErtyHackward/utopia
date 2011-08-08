using System;
using System.Net;
using System.Net.Sockets;

namespace Utopia.Net.Connections
{
    public class TcpConnectionListener : IDisposable
    {
        protected Socket listenSocket;
        protected IPEndPoint ep;
        protected int maximumDependingConnections = 10;
        protected readonly AsyncCallback startTransfer;

        /// <summary>
        /// Occurs when we receive a new connection, if event is not handled socket will be closed
        /// </summary>
        public event EventHandler<IncomingConnectionEventArgs> IncomingConnection;

        protected void OnIncomingConnection(IncomingConnectionEventArgs e)
        {
            if (IncomingConnection != null)
                IncomingConnection(this, e);
        }
        
        protected TcpConnectionListener()
        {
            startTransfer = OnStartConnection;
        }
        
        /// <summary>
        /// Creates new TcpListener on port specified
        /// </summary>
        /// <param name="port"></param>
        public TcpConnectionListener(int port)
            : this()
        {
            listenSocket = new Socket(
                                        AddressFamily.InterNetwork,
                                        SocketType.Stream,
                                        ProtocolType.Tcp);
            ep = new IPEndPoint(IPAddress.Any, port);
            listenSocket.Bind(ep);
        }

        public void Start()
        {
            try
            {
                // start listening
                listenSocket.Listen(maximumDependingConnections);
                SetupConnection(listenSocket);
            }
            catch (ObjectDisposedException) { /* End has been called */ }
            catch (System.Net.Sockets.SocketException) { }
        }

        public void End()
        {
            listenSocket.Close();
        }

        
        protected void SetupConnection(Socket sc)
        {
            try
            {
                sc.BeginAccept(startTransfer, sc);
            }
            catch (ObjectDisposedException) { /* End has been called */ }
            catch (System.Net.Sockets.SocketException) { }
        }

        protected void OnStartConnection(IAsyncResult ar)
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
