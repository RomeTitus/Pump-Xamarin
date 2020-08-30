using System;

namespace Pump.IrrigationController
{
    internal class Sensor
    {
        public string ID { get; set; }
        public string LastReading { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public string GPIO { get; set; }
        public string AttachedPiController { get; set; }

        public void setSensorReading(string data)
        {
            var reading = Convert.ToDouble(data);
            //if (TYPE == "Pressure Sensor")
            //{
                var voltage = reading * 5.0 / 1024.0;

                var pressure_pascal = 3.0 * (voltage - 0.47) * 1000000.0;

                var bars = pressure_pascal / 10e5;

                LastReading = bars.ToString("0.##").Replace(',', '.'); //for older support
            //}
        }
    }
}