using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SmartFleet.Core.Domain.Customers;

namespace SmartFleet.Core.Domain.Users
{
    public class User:IdentityUser
    {
        

        public string Tel { get; set; }
        public Nullable<System.Guid> CustomerId { get; set; }
        public  Customer Customer { get; set; }
        public string TimeZoneInfo { get; set; }
       // public IdentityRole Role { get; set; }
    }
}
