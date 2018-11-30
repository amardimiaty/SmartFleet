using SmartFleet.Core.Domain.Customers;

namespace SmartFleet.Service.Customers
{
    public interface ICustomerService
    {
        bool AddCustomer(Customer customer);
        Customer GetOwnerCustomer(string name);
        
    }
}