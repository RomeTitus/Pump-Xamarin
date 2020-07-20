using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using Xamarin.Forms;

namespace Pump.SocketController

{
    class SocketVerify : SocketConnection
    {
        readonly string host;
        readonly int port;
        public SocketVerify(string host, int port)
        {
            this.host = host;
            this.port = port;
        }
       
        public string verifyConnection()
        {
            var commands = new SocketCommands();
            return Send(commands.getMacAddress(), this.host, this.port);
        }
    }
}
