using System;
using System.Net.Sockets;
using System.Text;

namespace Pump.SocketController
{
    internal class SocketConnection
    {
        protected string Send(string message, string host, int port)
        {
            try
            {
                var sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    return sendSocket(sender, message, host, port);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane);
                    throw new Exception(ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se);
                    throw new Exception(se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e);
                    throw new Exception(e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception(e.ToString());
            }
        }

        protected string Send(string message, string host, int port, int timeout)
        {
            try
            {
                var sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                sender.SendTimeout = timeout;
                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    return sendSocket(sender, message, host, port);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane);
                    throw new Exception(ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se);
                    throw new Exception(se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e);
                    throw new Exception(e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception(e.ToString());
            }
        }

        private string sendSocket(Socket sender, string message, string host, int port)
        {
            var bytes = new byte[1024];
            // Connect to Remote EndPoint  
            sender.Connect(host, port);

            //Console.WriteLine("Socket connected to {0}",
            //    sender.RemoteEndPoint.ToString());

            // Encode the data string into a byte array.
            //byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
            var msg = Encoding.ASCII.GetBytes(message);
            // Send the data through the socket.
            var bytesSent = sender.Send(msg);

            // Receive the response from the remote device.
            var bytesRec = sender.Receive(bytes);
            //Console.WriteLine("Echoed test = {0}",
            //    Encoding.ASCII.GetString(bytes, 0, bytesRec));

            // Release the socket.    
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();

            return Encoding.ASCII.GetString(bytes, 0, bytesRec);
        }
    }
}