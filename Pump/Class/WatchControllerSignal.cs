using System;

namespace Pump.Class
{
    public class ControllerSignalEvent
    {
        private bool? _signalVerified = null;
        private int _signalStrength = 1;

        public void UpdateSignalStrength(bool? signalVerified, int signalStrength)
        {
            if(signalVerified != _signalVerified || signalStrength != _signalStrength)
                StatusChanged?.Invoke(this, EventArgs.Empty);
            _signalVerified = signalVerified;
            _signalStrength = signalStrength;
        }

        public (bool? signalVerified, int signalStrength) GetSignalStrengthy()
        {
            return (_signalVerified, _signalStrength);
        }

        public event EventHandler StatusChanged;
    }
}
