using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        public Customer GetCustomerbyid(string name)
        {
            var cst = _objectContext.UserAccounts.Include(x => x.Customer)
                .FirstOrDefault(x => x.UserName == name)?.Customer;
            return cst;
        }

        public async Task<Customer> GetCustomerbyid(Guid id)
        {
            var cst = _objectContext.Customers
                //.Include(x => x.Vehicles)
                //.Include(x=>x.Users)
                .FirstOrDefaultAsync(x => x.Id == id);
            return await cst;
        }

        public IQueryable<Customer> GetCustomers( )
        {
            return _objectContext.Customers;
        }
    }
}
