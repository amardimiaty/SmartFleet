using System.Collections.Generic;
using SmartFleet.Core.Domain.Users;
using SmartFleet.Core.Domain.Vehicles;

namespace SmartFleet.Core.Domain.Customers
{
    public class Customer:BaseEntity
    {
        public Customer()
        {
          //  this.Boxes = new List<Box>();
            this.Users = new List<User>();
            this.Vehicles = new List<Vehicle>();
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public System.DateTime? CreationDate { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public int CustomerStatus { get; set; }
       // public virtual ICollection<Box> Boxes { get; set; }
        public  ICollection<User> Users { get; set; }
        public  ICollection<Vehicle> Vehicles { get; set; }
    }

    public enum CustomerStatus
    {
        Active,
        Suspended,
        Deleted
    }
}
