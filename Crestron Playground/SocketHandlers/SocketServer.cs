using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        private Queue<byte[]> _buffer = new Queue<byte[]>();

        /// <summary>
        /// The public buffer property. Provides access to a FIFO queue. Access the buffer with a byte array to either add or get and item from the queue.
        /// </summary>
        /// <remarks>Getting an item from the buffer will remove it from the queue.</remarks>
        public byte[] Buffer
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
                    return ws;
                });

                CrestronConsole.PrintLine("Starting the webserver...");
                server.Start();
                CrestronConsole.PrintLine("Webserver started.");
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("Exception when creating server", ex.Message);
                ErrorLog.Exception("Exception thrown when creating server.", ex);
            }

        }

        #endregion

        #region METHODS


        #endregion

        #region EVENTS

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            CrestronConsole.PrintLine("Client connected");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            CrestronConsole.PrintLine("Message recieved from client: {0}", e.Data);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            CrestronConsole.PrintLine("Error in the server: {0}", e.Message);
            ErrorLog.Error("Server encountered and error: {0}", e.Message);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            CrestronConsole.PrintLine("Server socket closed with code: {0}", e.Code.ToString());
        }


    }
}
