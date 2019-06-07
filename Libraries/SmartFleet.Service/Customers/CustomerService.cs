using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SmartFleet.Core.Data;
using SmartFleet.Core.Domain.Customers;
using SmartFleet.Core.Domain.Users;
using SmartFleet.Data;

namespace SmartFleet.Service.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _customeRepository;
        private readonly SmartFleetObjectContext _objectContext;
        private readonly UserManager<User> _userManager;
        public CustomerService(IRepository<Customer> customeRepository,SmartFleetObjectContext objectContext)
        {
            _customeRepository = customeRepository;
            _objectContext = objectContext;
           _userManager = new UserManager<User>(new UserStore<User>(_objectContext));

        }
        public bool AddCustomer(Customer customer, List<User> users)
        {
            try
            {
                customer.Id = Guid.NewGuid();
                _customeRepository.Insert(customer);
                if (!users.Any()) return true;
                var passwordHash = new PasswordHasher();
                foreach (var user in users)
                {

                    if (_objectContext.Users.Any(u => u.UserName == user.UserName)) continue;
                    user.PasswordHash =  passwordHash.HashPassword(user.Password);
                    user.CustomerId = customer.Id;
                    _userManager.Create(user);
                    _userManager.AddToRole(user.Id, user.Role);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

        }
        public Customer GetCustomerbyName(string name)
        {
            var user = _userManager.Users.Include(x=>x.Customer)
                .FirstOrDefault(x => x.UserName == name);
           
            return user?.Customer;
        }

        public async Task<Customer> GetCustomerbyName(Guid id)
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

        public async Task<Boolean> GetUserbyName(string id)
        {
            return  await _userManager.Users.AnyAsync(u => u.UserName == id);
        }

        public async Task<List<InterestArea>> GetAllAreas(string userName, int page , int size)
        {
            var customer =await _userManager.Users.Include(x=>x.Customer).Select(x=> new { x.CustomerId , x.UserName}).FirstOrDefaultAsync(x => x.UserName == userName);
            if (customer != null)
                return await _objectContext.InterestAreas
                    .OrderBy(x=>x.Name)
                    .Skip(page-1)
                    .Take(size*page)
                    .ToListAsync();
            return new List<InterestArea>();
        }

        public bool AddArea(InterestArea area)
        {
            try
            {
                _objectContext.InterestAreas.Add(area);
                _objectContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
