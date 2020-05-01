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

            var connection = database.getPumpSelection();

            if (connection == null)
                return null;

            try
            {
                if(connection.InternalPort != -1)
                    return Send(data, connection.InternalPath, connection.InternalPort);
            }
            catch (Exception e)
            {
                exception = e;
            }

            try
            {
                if (connection.ExternalPort != -1)
                    return Send(data, connection.ExternalPath, connection.ExternalPort);
            }
            catch (Exception e)
            {
                exception = e;
            }

            throw new Exception(exception.ToString());
        }

    }
}

    

