using System;

namespace Pump.IrrigationController
{
    public class Equipment
    {
        public string ID { get; set; }
        public string AttachedPiController { get; set; }
        public string NAME { get; set; }
        public string GPIO { get; set; }
        public bool isPump { get; set; }
        public string DirectOnlineGPIO { get; set; }
        public Equipment Clone()
        {
            return (Equipment) this.MemberwiseClone();
        }
    }
}