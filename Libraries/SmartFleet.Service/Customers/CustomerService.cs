using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Customers;
using SmartFleet.Data;

namespace SmartFleet.Service.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customeRepository;
        private readonly SmartFleetObjectContext _objectContext;

        public CustomerService(IRepository<Customer> customeRepository, SmartFleetObjectContext objectContext)
        {
            _customeRepository = customeRepository;
            _objectContext = objectContext;
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
        public Customer GetOwnerCustomer(string name)
        {
            var cst = _objectContext.UserAccounts.Include(x => x.Customer)
                .FirstOrDefault(x => x.UserName == name)?.Customer;
            return cst;
        }
    }
}
