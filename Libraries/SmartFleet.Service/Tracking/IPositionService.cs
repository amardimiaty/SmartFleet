using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFleet.Core.Domain.Movement;

namespace SmartFleet.Service.Tracking
{
    public interface IPositionService
    {
        Task<List<Position>> GetLastVehiclPosition(string userName);

        Task<List<Position>> GetVehiclePositionsByPeriod(Guid vehivleId, DateTime startPeriod,
            DateTime endPeriod);
    }
}