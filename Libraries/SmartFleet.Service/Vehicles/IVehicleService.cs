using System.Threading.Tasks;
using SmartFleet.Core.Domain.Vehicles;

namespace SmartFleet.Service.Vehicles
{
    public interface IVehicleService
    {
         Task<bool> AddNewVehicle(Vehicle vehicle);
    }
}