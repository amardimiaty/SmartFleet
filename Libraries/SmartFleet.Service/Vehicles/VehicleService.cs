using System;
using System.Collections.Generic;
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
        public async Task<Vehicle> GetVehicleByIdAsync(Guid id)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return await _db.Vehicles.FindAsync(id).ConfigureAwait(false);
            }
        }
        public async Task<Vehicle> GetVehicleByIdWithDetailAsync(Guid id)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return await _db.Vehicles.Include(x => x.Brand).Include(x => x.Customer).Include(x => x.Model)
                    .Include(x => x.Boxes).FirstOrDefaultAsync(v => v.Id == id);
            }
        }
        public async Task<List<Vehicle>> GetAllvehiclesQuery()
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return await _db.Vehicles
                    .Include("Brand")
                    .Include("Model")
                    .Include("Customer")
                    .ToListAsync();
            }
        }

        public async Task<List<Vehicle>> GetAllvehiclesOfCustomer(Guid customerId)
        {
            using (var contextFScope = _dbContextScopeFactory.Create())
            {
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return await _db.Vehicles
                    .Where(v=>v.CustomerId == customerId)
                    //.Include("Brand")
                    //.Include("Model")
                    //.Include("Customer")
                    .ToListAsync();
            }
        }

        public IQueryable<Vehicle> GetvehiclesOfCustomer(Guid customerId)
        {
            var contextFScope = _dbContextScopeFactory.Create();
            
                _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
                return _db.Vehicles.Where(v => v.CustomerId == customerId)
                    .Include("Brand")
                    .Include("Model");
        }
        public IQueryable<Vehicle> GetAllvehicles()
        {
            var contextFScope = _dbContextScopeFactory.Create();

            _db = contextFScope.DbContexts.Get<SmartFleetObjectContext>();
            return _db.Vehicles
                .Include("Brand")
                .Include("Model");
        }
    }
}
