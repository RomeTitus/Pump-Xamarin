using System.Collections.Generic;
namespace Pump.IrrigationController
{
    public class ControllerStatus
    {
        public bool Complete { get; set;}
        public long? LastUpdated { get; set; }
        public List<string> Steps { get; set;}
        public StatusTypeEnum? StatusType { get; set;}
    }
    
    public enum StatusTypeEnum
    {
        Success,
        ReachFail,
        TimeAdjusted,
        UnknownFailure
    }
}