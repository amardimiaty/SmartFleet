using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Core.Domain.Customers;
using SmartFleet.Data;
using SmartFleet.Service.Customers;
using SmartFLEET.Web.Areas.Administrator.Models;
using SmartFLEET.Web.Controllers;

namespace SmartFLEET.Web.Areas.Administrator.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(SmartFleetObjectContext objectContext, IMapper mapper,ICustomerService customerService) : base(objectContext, mapper)
        {
            _customerService = customerService;
        }

        // GET
        public ActionResult Index()
        {
            return View( ObjectContext.Customers.ToList());
        }

        public ActionResult CreatePopup()
        {
            return PartialView("Create");
        }
        [HttpPost]
        public ActionResult AddCustomer(AddCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = Mapper.Map<Customer>(model);
                var result = _customerService.AddCustomer(customer);
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNewCustomer()
        {
            return Json(new AddCustomerViewModel(), JsonRequestBehavior.AllowGet);
        }
      
    }
}