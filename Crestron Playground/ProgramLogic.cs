using Crestron_Playground.SocketHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crestron_Playground
{
    /// <summary>
    /// This class handles the actual program logic for the system.
    /// </summary>
    internal class ProgramLogic : IDisposable
    {
        private bool _disposed;
        private SocketServer server;

        internal ProgramLogic()
        {
            server = new SocketServer();
        }

        public void Dispose()
        {

        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;
                try
                {
                    server.Dispose();
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
    }
}
