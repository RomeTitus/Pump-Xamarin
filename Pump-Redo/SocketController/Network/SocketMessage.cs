using System;
using System.Linq;
using Pump.Database;
using Xamarin.Essentials;

namespace Pump.SocketController
{
    internal class SocketMessage : SocketConnection
    {
        private Exception exception;

        public string Message(string data)
        {
            var database = new DatabaseController();

            var connection = database.GetControllerConnectionSelection();

            if (connection == null)
                return null;
            var SocketResult = "";
            try
            {
                if (connection.InternalPort != -1)
                {
                    var profiles = Connectivity.ConnectionProfiles;
                    if (profiles.Contains(ConnectionProfile.WiFi))
                        SocketResult = Send(data, connection.InternalPath, connection.InternalPort.Value);
                }
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (SocketResult != "")
                return SocketResult;

            try
            {
                if (connection.ExternalPort != -1)
                    SocketResult = Send(data, connection.ExternalPath, connection.ExternalPort.Value);
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (SocketResult != "")
                return SocketResult;

            throw new Exception("No Connection");
        }
    }
}