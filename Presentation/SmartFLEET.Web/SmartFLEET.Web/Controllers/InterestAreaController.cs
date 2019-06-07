using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Core.Domain.Customers;
using SmartFleet.Data;
using SmartFleet.Service.Customers;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.Controllers
{
    public class InterestAreaController : BaseController
    {
        private readonly ICustomerService _customerService;
        public InterestAreaController(ICustomerService customerService,  SmartFleetObjectContext objectContext,
            IMapper mapper) : base(objectContext, mapper)
        {
            _customerService = customerService;
        }
        // GET: InterestArea
        public ActionResult Index()
        {
            return PartialView("_List");
        }
        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult AddNewZone(InterestAreaVm model)
        {
            if (ModelState.IsValid)
            {
                var custome = _customerService.GetCustomerbyName(User.Identity.Name);
                if (custome != null)
                {
                    var area = Mapper.Map<InterestArea>(model);
                    area.Id = Guid.NewGuid();
                    area.CustomerId = custome.Id;
                   var r=  _customerService.AddArea(area);
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
            }

            var validation = ValidationViewModel();
            return Json(validation, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetZones()
        {
            var r = Request.Form.ToString().Split('&');
            var page =Convert.ToInt32(Regex.Match(r[0], @"\d+").Value);
            var rows = Convert.ToInt32(Regex.Match(r[1], @"\d+").Value);
            return Json(await _customerService.GetAllAreas(User.Identity.Name, page, rows), JsonRequestBehavior.AllowGet);
        }
    }
}