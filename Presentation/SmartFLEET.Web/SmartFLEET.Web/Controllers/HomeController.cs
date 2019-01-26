using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Core.Domain.Vehicles;
using SmartFleet.Service.Customers;
using SmartFleet.Service.Tracking;
using SmartFleet.Service.Vehicles;
using SmartFLEET.Web.DailyRports;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            var user = User.Identity;
            var cst = _customerService.GetOwnerCustomer(user.Name);
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
            var user = User.Identity;

            var positions =  report.PositionViewModels(await _positionService.GetLastVehiclPosition(user.Name));
            return Json(positions, JsonRequestBehavior.AllowGet);
        }
      
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> LoadNodes()
        {
            var user = User.Identity;
            var cst = _customerService.GetOwnerCustomer(user.Name);
            var nodes = JsTreeModels();
            if (cst == null)
                return Json(nodes, JsonRequestBehavior.AllowGet);
            var vehicles = await _vehicleService.GetVehiclesFromCustomer(cst.Id);
            nodes.AddRange(vehicles.Select(JsTreeModel));

            return Json(nodes, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <returns></returns>
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
            return Json(new { Periods =  positionReport.BuidDailyReport(positions, startPeriod, vehicle.VehicleName), GpsCollection = gpsCollection }, JsonRequestBehavior.AllowGet);

        }
        [NonAction]
        private static JsTreeModel JsTreeModel(Vehicle vehicle)
        {
            var node = new JsTreeModel();
            node.id = vehicle.Id.ToString();
            // ReSharper disable once ComplexConditionExpression
            node.text = vehicle.VehicleName == string.Empty ?
                vehicle.LicensePlate :
                " " + vehicle.VehicleName;
            node.parent = "vehicles-" + Guid.Empty;
            node.icon = "la la-car ";
            return node;
        }
        [NonAction]
        private static List<JsTreeModel> JsTreeModels()
        {
            var nodes = new List<JsTreeModel>();
            nodes.Add(new JsTreeModel
            {
                id = "vehicles-" + Guid.Empty,
                parent = "#",
                text = "Vehicules",
            });
            nodes.Add(new JsTreeModel
            {
                id = "drivers-" + Guid.Empty,
                parent = "#",
                text = "Condcuteurs",
            });
            return nodes;
        }


    }
}