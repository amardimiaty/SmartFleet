using System;
using System.Diagnostics;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Customers;

namespace SmartFleet.Service.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customeRepository;

        public CustomerService(IRepository<Customer> customeRepository)
        {
            _customeRepository = customeRepository;
        }
        public bool AddCustomer(Customer customer)
        {
            try
            {
                _customeRepository.Insert(customer);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

        }
    }
}
