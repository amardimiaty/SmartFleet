using System;
using System.Linq;
using System.Threading.Tasks;
using SmartFleet.Core.Domain.Customers;

namespace SmartFleet.Service.Customers
{
    public interface ICustomerService
    {
        bool AddCustomer(Customer customer);
        Customer GetCustomerbyid(string name);
        Task<Customer> GetCustomerbyid(Guid id);
        IQueryable<Customer> GetCustomers();
    }
}