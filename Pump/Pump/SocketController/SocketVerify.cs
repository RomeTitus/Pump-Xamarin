namespace Pump.SocketController

{
    internal class SocketVerify : SocketConnection
    {
        private readonly string host;
        private readonly int port;

        public SocketVerify(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public string verifyConnection()
        {
            var commands = new SocketCommands();
            return Send(commands.getMacAddress(), host, port);
        }
    }
}