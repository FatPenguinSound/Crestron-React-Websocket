using Crestron_Playground.SocketHandlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.UI.DVPHD;

namespace Crestron_Playground
{
    /// <summary>
    /// This class handles the actual program logic for the system.
    /// </summary>
    internal class ProgramLogic : IDisposable
    {
        private bool _disposed;
        private SocketServer server;

        private int CurrentColour;
        private int Saturation;

        internal ProgramLogic()
        {
            server = new SocketServer();
            Saturation = 50;
            CurrentColour = 1;
            server.PropertyChanged += OnMessageReceive;
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
                    CrestronConsole.PrintLine("Message parsed into control {0} with value {1}.", parse.Control, parse.Value);
                }
                catch (Exception)
                {
                    CrestronConsole.PrintLine("Error in parsing data. Skipping message.");
                    continue;
                }
                

                switch (parse.Control)
                {
                    case "Button":
                        CrestronConsole.PrintLine("A button was pressed!");
                        CurrentColour = parse.Value;
                        break;
                    case "Slider":
                        CrestronConsole.PrintLine("The slider was moved!");
                        Saturation = parse.Value;
                        break;
                    default: 
                        CrestronConsole.PrintLine("Unexpected control {0} with value {1}", parse.Control, parse.Value);
                            break;
                }

                CrestronConsole.PrintLine("Updating colours...");
                ResponseObject response = CalculateNewColours();
                CrestronConsole.PrintLine("Sending new colours: {0}, {1}, {2}", response.Value[0], response.Value[1], response.Value[2]);
                server.Send(JsonConvert.SerializeObject(response));
            }
        }

        private ResponseObject CalculateNewColours()
        {
            CrestronConsole.PrintLine("Calculating new colours: {0}, {1}", CurrentColour, Saturation);
            float colourVal = 255f * ((float)Saturation / 100f);
            CrestronConsole.PrintLine(colourVal.ToString());
            colourVal = (int)Math.Round(colourVal);
            int[] vals = new int[3] {0, 0, 0};
            vals[CurrentColour - 1] = (int)colourVal;
            CrestronConsole.PrintLine(vals[CurrentColour - 1].ToString());
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
