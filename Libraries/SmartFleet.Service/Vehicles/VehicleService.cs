using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Gpsdevices;
using SmartFleet.Core.Domain.Vehicles;
using SmartFleet.Data;

namespace SmartFleet.Service.Vehicles
{
    public class VehicleService : IVehicleService
    {
        private readonly IDbContextScopeFactory _dbContextScopeFactory;
        private SmartFleetObjectContext _db;

        public VehicleService(IDbContextScopeFactory dbContextScopeFactory)
        {
             _dbContextScopeFactory = dbContextScopeFactory;
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

        public async Task<Vehicle[]> GetVehiclesFromCustomer(Guid customerId)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return await _db.Vehicles.Where(x => x.CustomerId == customerId).ToArrayAsync().ConfigureAwait(false);
            }
        }
        public async Task<Vehicle> GetVehicleById(Guid id)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return await _db.Vehicles.FindAsync(id).ConfigureAwait(false);
            }
        }
    }
}
