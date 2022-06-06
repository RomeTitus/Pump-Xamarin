using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pump.Database.Table;
using Xamarin.Essentials;

namespace Pump.SocketController.BT
{
    public class NetworkManager
    {
        private string ConvertForIrrigation(string dataToBeConverted)
        {
            var updated = dataToBeConverted.Replace("true", "True");
            updated = updated.Replace("false", "False");
            return updated;
        }

        private string ConvertForApplication(string dataToBeConverted)
        {
            var updated = dataToBeConverted.Replace("True", "true");
            updated = updated.Replace("False", "false");
            return updated;
        }

        public async Task<string> SendAndReceiveToNetwork(JObject dataToSend, PumpConnection connection,
            int timeout = 0)
        {
            var fullData = false;
            var networkReplyBytes = new List<byte>();
            var partNumber = 0;
            while (fullData == false)
            {
                var key = Encoding.ASCII.GetBytes(SocketCommands.GenerateKey(4)).ToList();


                if (dataToSend.ContainsKey("Task"))
                    try
                    {
                        dataToSend["Task"]["Part"] = partNumber;
                    }
                    catch
                    {
                        // ignored
                    }

                var bytes = Encoding.ASCII.GetBytes(ConvertForIrrigation(dataToSend.ToString())).ToList();

                var finalBytesReceived = new byte[0];
                //Sending Large amounts of Data :/
                for (var i = 0; i < bytes.Count; i += 1020)
                {
                    var sendingBytes = bytes.Count > i + 1020 ? bytes.GetRange(i, i + 1020) : bytes;
                    sendingBytes.InsertRange(0, key);
                    finalBytesReceived = await WriteToNetwork(sendingBytes.ToArray(), connection, timeout);
                }

                partNumber++;
                networkReplyBytes.AddRange(finalBytesReceived);
                if (finalBytesReceived.Length != 512)
                    fullData = true;
            }

            return ConvertForApplication(Encoding.ASCII.GetString(networkReplyBytes.ToArray(), 0,
                networkReplyBytes.Count));
        }

        private async Task<byte[]> WriteToNetwork(byte[] bytesToSend, PumpConnection connection, int timeout = 0)
        {
            //Return nothing if there is no network path
            if (connection.InternalPort == -1 && connection.ExternalPort == -1)
                return null;

            var sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Connect to Remote EndPoint
            try
            {
                if (connection.InternalPort != -1)
                {
                    var profiles = Connectivity.ConnectionProfiles;
                    if (profiles.Contains(ConnectionProfile.WiFi))
                        sender.Connect(connection.InternalPath, connection.InternalPort);
                    else
                        throw new Exception();
                }
            }
            catch
            {
                if (connection.ExternalPort != -1)
                    sender.Connect(connection.ExternalPath, connection.ExternalPort);
            }

            // Send the data through the socket.
            sender.Send(bytesToSend);
            await Task.Delay(timeout);
            // Receive the response from the remote device.
            var bytes = new byte[512];
            var bytesRec = sender.Receive(bytes);
            var shrinkBytes = new List<byte>();

            for (var i = 0; i < bytesRec; i++) shrinkBytes.Add(bytes[i]);

            var result = shrinkBytes.ToArray();
            // Release the socket.    
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();

            if (Encoding.ASCII.GetString(result, 0, result.Length) ==
                Encoding.ASCII.GetString(bytesToSend, 0, bytesToSend.Length))
                throw new Exception("Controller did not reply back using Network \n reboot required");
            return result;
        }
    }
}