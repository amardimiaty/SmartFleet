using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFleet.Core.Domain.Vehicles
{
    public class Driver : BaseEntity
    {
        public string Name { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public DateTime? OnService { get; set; }
        public string DriverNumber{ get; set; }
        public virtual ICollection<Vehicle> Vehicles{ get; set; }

    }
}
