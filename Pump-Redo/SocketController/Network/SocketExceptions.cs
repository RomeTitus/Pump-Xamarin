namespace Pump.SocketController
{
    public static class SocketExceptions
    {
        public static string SocketTimedOut = "timed out";


        public static bool CheckException(string exception)
        {
            if (exception == SocketTimedOut)
                return true;
            return false;
        }
    }
}