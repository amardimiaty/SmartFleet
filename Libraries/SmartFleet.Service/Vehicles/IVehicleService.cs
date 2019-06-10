using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFleet.Core.Domain.Vehicles;

namespace SmartFleet.Service.Vehicles
{
    public interface IVehicleService
    {

        /// <summary>
        /// adds new vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        Task<bool> AddNewVehicle(Vehicle vehicle);
        /// <summary>
        /// gets vehicule for specific customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<Vehicle[]> GetVehiclesFromCustomer(Guid customerId);
        /// <summary>
        /// gets single vehicle by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Vehicle> GetVehicleByIdAsync(Guid id);
        /// <summary>
        /// gets a single vehicle with all entities
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Vehicle> GetVehicleByIdWithDetailAsync(Guid id);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<Vehicle>> GetAllvehiclesQuery();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<List<Vehicle>> GetAllvehiclesOfCustomer(Guid  customerId);
        /// <summary>
        /// get all vehicles for a specific  customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        IQueryable<Vehicle> GetvehiclesOfCustomer(Guid customerId);
        /// <summary>
        /// gets all vehicles
        /// </summary>
        /// <returns></returns>
        IQueryable<Vehicle> GetAllvehicles();
    }
}