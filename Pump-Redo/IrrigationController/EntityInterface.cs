namespace Pump.IrrigationController
{
    public interface IEntity
    {
        string Id { get; set; }
        bool DeleteAwaiting { get; set; }
    }
}