using System.Collections.Generic;

namespace Pump.Class
{
    public static class GpioPins
    {
        public static List<long> GetAllGpioList()
        {
            return new List<long>
            {
                1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36
            };
        }

        public static List<long> GetDigitalGpioList()
        {
            return new List<long>
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
            };
        }

        public static List<long> GetAnalogGpioList()
        {
            return new List<long> { 1,2,3,4,5,6};
        }
        
        public static List<long> GetLegacyGpioList()
        {
            return new List<long> { 33,34,35,36};
        }
    }
}