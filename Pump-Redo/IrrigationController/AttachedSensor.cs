namespace Pump.IrrigationController
{
    public class AttachedSensor
    {
        public string id_Equipment { get; set; }
        public double ThresholdLow { get; set; }
        public double ThresholdHigh { get; set; }
        public double ThresholdTimer { get; set; }
    }
}