using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFleet.Core.Domain.Customers;
using SmartFleet.Core.Domain.Users;

namespace SmartFleet.Service.Customers
{
    public interface ICustomerService
    {
        bool AddCustomer(Customer customer, List<User> users );
        Customer GetCustomerbyName(string name);
        Task<Customer> GetCustomerbyName(Guid id);
        IQueryable<Customer> GetCustomers();
        Task<bool> GetUserbyName(string id);
        Task<List<InterestArea>> GetAllAreas(string userName, int page, int size );
        bool AddArea(InterestArea area);
    }
}