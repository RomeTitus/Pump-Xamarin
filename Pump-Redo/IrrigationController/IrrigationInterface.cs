using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public interface IEntity
    {
        string Id { get; set; }
        bool DeleteAwaiting { get; set; }
    }

    public interface IEquipment: ISubController
    {
        string AttachedSubController { get; set; }
    }
    
    public interface ISubController: IEntity, IStatus
    {
        string Key { get; set; }
    }

    public interface ISchedule: ISubController
    {
        List<ScheduleDetail> ScheduleDetails { get; set; }
        string id_Pump { get; set; }
    }

    public interface IManualSchedule: ISubController
    {
        List<ManualScheduleEquipment> ManualDetails { get; set; }
    }
    
    public interface IStatus
    {
        ControllerStatus ControllerStatus { get; set; }
    }
}