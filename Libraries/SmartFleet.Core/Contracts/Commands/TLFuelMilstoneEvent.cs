using System;

namespace SmartFleet.Core.Contracts.Commands
{
    public class TLFuelMilstoneEvent
    {
        public int FuelConsumption { get;  set; }
        public int Milestone { get;  set; }
        public DateTime DateTimeUtc { get;  set; }
        public int FuelLevel { get;  set; }
        public Guid VehicleId { get;  set; }
    }
}