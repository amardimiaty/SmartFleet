using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Core.ReverseGeoCoding;
using SmartFleet.Data;

namespace SmartFleet.Service.Tracking
{
    public class PositionService : IPositionService
    {
        private  SmartFleetObjectContext _objectContext;
        private readonly ReverseGeoCodingService _geoCodingService;
        private readonly IDbContextScopeFactory _dbContextScopeFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="positionRepository"></param>
        /// <param name="geoCodingService"></param>
        /// <param name="objectContext"></param>
        /// <param name="gpsDeviceRepository"></param>
        /// <param name="vehicleRepository"></param>
        /// <param name="dbContextScopeFactory"></param>
        public PositionService(  ReverseGeoCodingService geoCodingService, IDbContextScopeFactory dbContextScopeFactory)
        {
            _geoCodingService = geoCodingService;
            _dbContextScopeFactory = dbContextScopeFactory;
        }
        public async Task<List<Position>> GetLastVehiclPosition(string userName)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _objectContext = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                var positions = new List<Position>();
                var cst = await _objectContext.UserAccounts.Include(x => x.Customer).Include(x => x.Customer.Vehicles)
                    .FirstOrDefaultAsync(x => x.UserName == userName);
                if (cst?.Customer.Vehicles != null)
                {
                    var vehicles = cst.Customer.Vehicles;
                    var vehicleids = vehicles.Select(v => v.Id);
                    var gpsDevices = _objectContext.Boxes.Where(x => vehicleids.Any(v => v == x.VehicleId));
                    foreach (var geDevice in gpsDevices)
                    {
                        var position = await _objectContext.Positions.OrderByDescending(x => x.Timestamp)
                            .FirstOrDefaultAsync(p => p.Box_Id == geDevice.Id);
                        if (position == null) continue;
                        position.Vehicle = vehicles.FirstOrDefault(v => v.Id == geDevice.VehicleId);
                        await _geoCodingService.ReverseGeoCoding(position);
                        positions.Add(position);
                    }
                }

                return positions;
            }
            

        }

        public async Task<List<Position>> GetVehiclePositionsByPeriod(Guid vehivleId, DateTime startPeriod,DateTime endPeriod)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _objectContext = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                var vehicle = await _objectContext.Vehicles.Include(v => v.Boxes).FirstOrDefaultAsync(v => v.Id == vehivleId).ConfigureAwait(false);
                if (vehicle == null) return new List<Position>();
                var box = vehicle.Boxes.FirstOrDefault();
                if (box == null) return new List<Position>();
                var positions =  _objectContext.Positions .Where(p => p.Box_Id == box.Id && p.Timestamp >= startPeriod && p.Timestamp <= endPeriod).ToList();
                return positions;
            }

          
        }
    }
}
