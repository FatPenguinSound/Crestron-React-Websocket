using Crestron_Websocket_Server.SocketHandlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.UI.DVPHD;
using Crestron.SimplSharpPro.UI;

namespace Crestron_Websocket_Server
{
    /// <summary>
    /// This class handles the actual program logic for the system.
    /// </summary>
    internal class ProgramLogic : IDisposable
    {
        private bool _disposed;
        private SocketServer server;
        private XpanelForHtml5 panel;

        private int CurrentColour;
        private int Saturation;

        internal ProgramLogic(ControlSystem system)
        {
            server = new SocketServer();
            Saturation = 50;
            CurrentColour = 1;
            server.PropertyChanged += OnMessageReceive;
            panel = new XpanelForHtml5(0x0f, system);
            CrestronConsole.PrintLine("DEBUG: Registering xpanel...");
            if(panel.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error($"Error registering xpanel {panel.RegistrationFailureReason}");
            }
            else
            {
                ErrorLog.Notice("Xpanel registered.");
            }
        }

        private void OnMessageReceive(object sender, EventArgs e)
        {
            while (server.Length > 0)
            {
                string raw = server.Buffer;
                CrestronConsole.PrintLine("Raw data: {0}", raw);
                MessageObject parse;

                try
                {
                    parse = JsonConvert.DeserializeObject<MessageObject>(raw);
                }
                catch (Exception)
                {
                    CrestronConsole.PrintLine("Error in parsing data. Skipping message. {0}", raw);
                    continue;
                }
                

                switch (parse.Control)
                {
                    case "Button":
                        CurrentColour = parse.Value;
                        break;
                    case "Slider":
                        Saturation = parse.Value;
                        break;
                    default: 
                        CrestronConsole.PrintLine("Unexpected control {0} with value {1}", parse.Control, parse.Value);
                        break;
                }

                ResponseObject response = CalculateNewColours();
                server.Send(JsonConvert.SerializeObject(response));
            }
        }

        private ResponseObject CalculateNewColours()
        {
            float colourVal = 255f * ((float)Saturation / 100f);
            colourVal = (int)Math.Round(colourVal);
            int[] vals = new int[3] {0, 0, 0};
            vals[CurrentColour - 1] = (int)colourVal;
            return new ResponseObject("Color", vals);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
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

    public class MessageObject
    {
        public string Control;
        public int Value;
        public MessageObject()
        {

        }
    }

    public class ResponseObject
    {
        public string Control;
        public int[] Value;

        public ResponseObject(string ctl, int[] clrArray)
        {
            Control = ctl;
            Value = clrArray;
        }
    }
}
