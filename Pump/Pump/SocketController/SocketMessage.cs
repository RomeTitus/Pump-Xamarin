using Pump.Database;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Pump.SocketController
{
    class SocketMessage : SocketConnection
    {
        Exception exception;
        public string Message(string data)
        {
            var database = new DatabaseController();

            var connection = database.GetPumpSelection();

            if (connection == null)
                return null;
            string SocketResult = "";
            try
            {
                if(connection.InternalPort != -1)
                    SocketResult = Send(data, connection.InternalPath, connection.InternalPort);
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
                    SocketResult = Send(data, connection.ExternalPath, connection.ExternalPort);
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

    

