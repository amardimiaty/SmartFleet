using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Threading.Tasks;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Gpsdevices;
using SmartFleet.Core.Domain.Vehicles;
using SmartFleet.Data;
using SmartFleet.Data.Dbcontextccope.Implementations;

namespace SmartFleet.Service.Vehicles
{
    public class VehicleService : IVehicleService
    {
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<Box> _boxRepository;
        private readonly IDbContextScopeFactory _dbContextScopeFactory;
        private SmartFleetObjectContext _db;

        public VehicleService(IRepository<Vehicle> vehicleRepository, IRepository<Box> boxRepository)
        {
            _vehicleRepository = vehicleRepository;
            _boxRepository = boxRepository;
            _dbContextScopeFactory = new DbContextScopeFactory();
        }

        public async Task<bool> AddNewVehicle(Vehicle vehicle)
        {
            try
            {
                using (var contextFScope = _dbContextScopeFactory.Create())
                {
                    _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                    var boxId = vehicle.Box_Id;
                    var box = await _db.Boxes.FirstOrDefaultAsync(b => b.Id == boxId).ConfigureAwait(false);

                    //  vehicle.Id = Guid.NewGuid();
                    if (box != null)
                    {
                        box.VehicleId = vehicle.Id;
                        box.BoxStatus = BoxStatus.Valid;
                        _db.Entry(box).State = EntityState.Modified; 
                      //  _db.Boxes.AddOrUpdate(box);
                        _db.Vehicles.Add(vehicle);
                        await contextFScope.SaveChangesAsync().ConfigureAwait(false);


                    }
                    return true;
                }

               
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
    }
}
