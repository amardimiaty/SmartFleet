using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Gpsdevices;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Core.Domain.Vehicles;
using SmartFleet.Core.ReverseGeoCoding;
using SmartFleet.Data;

namespace SmartFleet.Service.Tracking
{
    public class PositionService : IPositionService
    {
        private readonly IRepository<Position> _positionRepository;
        private readonly SmartFleetObjectContext _objectContext;
        // private readonly IRepository<User> _userAccountRepository;
        private readonly IRepository<Box> _gpsDeviceRepository;
        private readonly ReverseGeoCodingService _geoCodingService;
        private readonly IRepository<Vehicle> _vehicleRepository;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="positionRepository"></param>
        /// <param name="geoCodingService"></param>
        /// <param name="objectContext"></param>
        /// <param name="gpsDeviceRepository"></param>
        /// <param name="vehicleRepository"></param>
        public PositionService(IRepository<Position> positionRepository, 
            ReverseGeoCodingService geoCodingService,
            SmartFleetObjectContext objectContext,
            IRepository<Box> gpsDeviceRepository, 
            IRepository<Vehicle> vehicleRepository)
        {
            _positionRepository = positionRepository;
            _geoCodingService = geoCodingService;
            _objectContext = objectContext;
            _gpsDeviceRepository = gpsDeviceRepository;
             _vehicleRepository = vehicleRepository;
        }
        public async Task<List<Position>> GetLastVehiclPosition(string userName)
        {
            var positions = new List<Position>();
            var cst = await _objectContext.UserAccounts.Include(x => x.Customer).Include(x => x.Customer.Vehicles)
                .FirstOrDefaultAsync(x => x.UserName == userName);
            if (cst?.Customer.Vehicles != null)
            {
                var vehicles = cst.Customer.Vehicles;
                var vehicleids = vehicles.Select(v => v.Id);
                var gpsDevices = _gpsDeviceRepository.Table.Where(x => vehicleids.Any(v => v == x.VehicleId));
                foreach (var geDevice in gpsDevices)
                {
                    var position = await _positionRepository.Table.OrderByDescending(x => x.Timestamp)
                        .FirstOrDefaultAsync(p => p.Box_Id == geDevice.Id);
                    if (position == null) continue;
                    position.Vehicle = vehicles.FirstOrDefault(v => v.Id == geDevice.VehicleId);
                    await _geoCodingService.ReverseGeoCoding(position);
                    positions.Add(position);
                }
            }

            return positions;

        }

        public async Task<List<Position>> GetVehiclePositionsByPeriod(Guid vehivleId, DateTime startPeriod,
            DateTime endPeriod)
        {
            var vehicle = await _vehicleRepository.Table.Include(v => v.Boxes).FirstOrDefaultAsync(v => v.Id == vehivleId).ConfigureAwait(false);
            if (vehicle == null) return new List<Position>();
            var box = vehicle.Boxes.FirstOrDefault();
            if (box == null) return new List<Position>();
            var positions = await _positionRepository.Table.OrderBy(x => x.Timestamp)
                .Where(p => p.Box_Id == box.Id && p.Timestamp >= startPeriod && p.Timestamp <= endPeriod).ToListAsync().ConfigureAwait(false);
            return positions;
        }
    }
}
