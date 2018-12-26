using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Service.Customers;
using SmartFleet.Service.Tracking;
using SmartFleet.Service.Vehicles;
using SmartFLEET.Web.DailyRports;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.Controllers
{
    [Authorize(Roles = "user,customer")]
    public class HomeController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        public static string CurrentGroup { get; set; }

        private readonly IPositionService _positionService;
        private readonly ICustomerService _customerService;
        private readonly IVehicleService _vehicleService;

        /// <inheritdoc />
        // ReSharper disable once TooManyDependencies
        public HomeController(IMapper mapper,
            IPositionService positionService, 
            ICustomerService customerService , 
            IVehicleService vehicleService) : base( mapper)
        {
            _positionService = positionService;
            _customerService = customerService;
            _vehicleService = vehicleService;
        }

     
        [HttpGet]
        public ActionResult Index()
        {
            var cst = _customerService.GetOwnerCustomer(User.Identity.Name);
            CurrentGroup = cst?.Name;
            ViewBag.GroupName = CurrentGroup;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> AllVehiclesWithLastPosition()
        {
            var report = new PositionReport();
            var positions =  report.PositionViewModels(await _positionService.GetLastVehiclPosition(User.Identity.Name));
            return Json(positions, JsonRequestBehavior.AllowGet);
        }
      
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> LoadNodes()
        {
            var cst = _customerService.GetOwnerCustomer(User.Identity.Name);
            var nodes = new List<JsTreeModel>();
            nodes.Add(new JsTreeModel
            {
                id = "vehicles-"+Guid.Empty,
                parent = "#",
                text = "Vehicules",

            });
            nodes.Add(new JsTreeModel
            {
                id ="drivers-"+ Guid.Empty,
                parent = "#",
                text = "Condcuteurs",

            });
            if (cst == null)
                return Json(nodes, JsonRequestBehavior.AllowGet);

            var vehicles = await _vehicleService.GetVehiclesFromCustomer(cst.Id);
            foreach (var vehicle in vehicles)
            {
                var node  = new JsTreeModel();
                node.id = vehicle.Id.ToString();
                node.text = vehicle.VehicleName == string.Empty ? vehicle.LicensePlate :" "+ vehicle.VehicleName;
                node.parent = "vehicles-" + Guid.Empty;
                node.icon = "la la-car ";
                nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
        }

       

        public async Task<JsonResult> GetTargetByPeriod(string vehicleId)
        {
            var id = Guid.Parse(vehicleId);
            //var start = 
            var endPeriod = DateTime.Now;
            var startPeriod = DateTime.Now.Date;
            var vehicle = await _vehicleService.GetVehicleById(id);
            var positions = await _positionService.GetVehiclePositionsByPeriod(id, startPeriod, endPeriod);
            if (!positions.Any())
                return Json(new List<TargetViewModel>(), JsonRequestBehavior.AllowGet);
            var gpsCollection = positions.Select(x =>
                new { Latitude = x.Lat, Longitude = x.Long, GpsStatement = x.Timestamp.ToString("O") });
            var positionReport = new PositionReport();
            return Json(new { Periods =  positionReport.GetTargetViewModels(positions, startPeriod, vehicle.VehicleName), GpsCollection = gpsCollection }, JsonRequestBehavior.AllowGet);

        }

        
    }
}