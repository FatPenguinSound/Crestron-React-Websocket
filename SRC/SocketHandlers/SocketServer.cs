using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebSocketSharp;

using WebSocketSharp.Server;

namespace Crestron_Playground.SocketHandlers
{
    /// <summary>
    /// This class wraps the WebSocketServer with some added functionallity for interactivity with the rest of the program. Received messages are queued into a buffer that can be accessed outside of the class instance. Subscribe to changes in the buffer using the propertychanged event handler.
    /// </summary>
    internal class SocketServer : IDisposable, INotifyPropertyChanged
    {
        #region FIELDS
        /// <summary>
        /// The actual instance of the server.
        /// </summary>
        private WebSocketServer server;

        /// <summary>
        /// The port for the webserver.
        /// </summary>
        private int port;

        private bool Disposed = false;

        /// <summary>
        /// The message queue. Access items in the FIFO queue using the public Buffer property.
        /// </summary>
        private Queue<string> _buffer = new Queue<string>();

        /// <summary>
        /// The public buffer property. Provides access to a FIFO queue. Access the buffer with a byte array to either add or get and item from the queue.
        /// </summary>
        /// <remarks>Getting an item from the buffer will remove it from the queue.</remarks>
        public string Buffer
        {
            get {  return _buffer.Dequeue(); }
            private set { 
                _buffer.Enqueue(value);
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the number of items in the message queue.
        /// </summary>
        public int Length
        {
            get { return _buffer.Count; }
        }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Default constructor. Instanciates a websocket server on port 5000.
        /// </summary>
        internal SocketServer()
        {
            try
            {
                server = new WebSocketServer(5000);
                server.AddWebSocketService<ServerOperations>("/app", () =>
                {
                    var ws = new ServerOperations();
                    ws.NewMessageReceived += (string msg) => { Buffer = msg; };
                    return ws;
                });

                server.Start();
            }
            catch (Exception ex)
            {
                ErrorLog.Exception("Exception thrown when creating server.", ex);
            }

        }

        #endregion

        #region METHODS

        public void Send(string data)
        {
            server.WebSocketServices["/app"].Sessions.Broadcast(data);
        }

        #endregion

        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // TODO: remove buffer and transition to events.

        /*
        public delegate void ServerMessageReceivedEventHandler(object sender, ServerMessageReceivedEventArgs e);

        public event ServerMessageReceivedEventHandler ServerMessageReceived;

        protected virtual void OnServerMessageReceived(string ip, string message)
        {
            ServerMessageReceivedEventArgs args = new ServerMessageReceivedEventArgs(ip, message);
            ServerMessageReceived?.Invoke(this, args);
        }
        */


        #endregion

        #region DISPOSE

        /// <summary>
        /// Public method for disposing of the server.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
        }

        /// <summary>
        /// Private method for internally managing resources on dispose.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (!Disposed && disposing)
            {
                Disposed = true;
                try
                {
                    //Stop the server.
                    server.Stop();
                }
                catch (Exception ex)
                {

                    throw new Exception("Error in disposing server.", ex);
                }
            }
        }

        #endregion
    }

    internal class ServerOperations : WebSocketBehavior
    {
        public ServerOperations() { }


        protected override void OnOpen()
        {
            base.OnOpen();
            ErrorLog.Notice("Client connected to web server.");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            NewMessageReceived?.Invoke(e.Data);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            //CrestronConsole.PrintLine("The server has encountered exception {0} with message: {1}", e.Exception, e.Message);
            ErrorLog.Error("Server encountered exception {0} with message: {1}", e.Exception, e.Message);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            CrestronConsole.PrintLine("Server socket closed with code: {0}", e.Code);
            ErrorLog.Notice("Client disconnected with code {0}", e.Code);
        }

        public event NewMessage NewMessageReceived;

    }

    public delegate void NewMessage(string message);

    public class ServerMessageReceivedEventArgs : EventArgs
    {
        public string ClientIP { get; private set; }
        public string Message { get; private set; }

        public ServerMessageReceivedEventArgs(string clientIP, string data)
        {
            ClientIP = clientIP;
            Message = data;
        }
    }
}
