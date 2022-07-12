using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public interface IEntity
    {
        string Id { get; set; }
        bool DeleteAwaiting { get; set; }
    }

    public interface IEquipment
    {
        string AttachedSubController { get; set; }
    }

    public interface ISchedule
    {
        List<ScheduleDetail> ScheduleDetails { get; set; }
        string id_Pump { get; set; }
    }

    public interface IManualSchedule
    {
        List<ManualScheduleEquipment> ManualDetails { get; set; }
    }
}