using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Core.Domain.Customers;
using SmartFleet.Data;
using SmartFleet.Service.Customers;
using SmartFleet.Web.Framework.DataTables;
using SmartFLEET.Web.Areas.Administrator.Models;
using SmartFLEET.Web.Controllers;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.Areas.Administrator.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;
        private readonly DataTablesLinqQueryBulider _queryBuilder;
        public CustomerController(SmartFleetObjectContext objectContext, IMapper mapper,ICustomerService customerService, DataTablesLinqQueryBulider queryBuilder) : base(objectContext, mapper)
        {
            _customerService = customerService;
            _queryBuilder = queryBuilder;
        }

        // GET
        public ActionResult Index()
        {
            return View(new List<Customer>());
        }

        public async Task<ActionResult> GetCustomers()
        {
            var query = _queryBuilder.BuildQuery(Request, _customerService.GetCustomers());
            var jsResult = new
            {
                recordsTotal =  query.recordsTotal,
                draw = query.draw,
                recordsFiltered = query.recordsFiltered,
                data = Mapper.Map<List<CustomerVm>>( query.data),
                lenght = query.length
            };
            return Json(jsResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
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

        public ActionResult Detail()
        {
            return PartialView("Detail" , new Customer());
        }
       // [HttpPost]
        public async Task<JsonResult> GetCustomer(Guid id)
        {
            var customer = await _customerService.GetCustomerbyid(id);
            return Json(Mapper.Map <CustomerVm>(customer), JsonRequestBehavior.AllowGet);
        }
      
    }

    
}