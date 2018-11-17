using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Data;
using SmartFleet.Service.Tracking;
using SmartFLEET.Web.DailyRports;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.Controllers
{
    [Authorize(Roles = "user,customer")]
    public class HomeController : BaseController
    {
        private readonly IPositionService _positionService;

        public HomeController(SmartFleetObjectContext objectContext, IMapper mapper, IPositionService positionService) : base(objectContext, mapper)
        {
            _positionService = positionService;
        }

        public  static string CurrentGroup { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            var cst = ObjectContext.UserAccounts.Include(x => x.Customer)
                .FirstOrDefault(x => x.UserName == User.Identity.Name)?.Customer;
            CurrentGroup = cst?.Name;
            ViewBag.GroupName = CurrentGroup;
            return View();
        }

        public async Task<JsonResult> AllVehiclesWithLastPosition()
        {
            var report = new PositionReport();
            var positions = await report.PositionViewModels(await _positionService.GetLastVehiclPosition(User.Identity.Name));

            return Json(positions, JsonRequestBehavior.AllowGet);
        }

      
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> LoadNodes()
        {
            var cst = ObjectContext.UserAccounts.Include(x => x.Customer)
                .FirstOrDefault(x => x.UserName == User.Identity.Name)?.Customer;
            var nodes = new List<JsTreeModel>();
            nodes.Add(new JsTreeModel()
            {
                id = Guid.Empty.ToString(),
                parent = "#",
                text = "Vehicules",

            });
            if (cst == null) return Json(nodes, JsonRequestBehavior.AllowGet);

            var vehicles = await ObjectContext.Vehicles.Where(x => x.CustomerId == cst.Id).ToArrayAsync();
            foreach (var vehicle in vehicles)
            {
                var node  = new JsTreeModel();
                node.id = vehicle.Id.ToString();
                node.text = vehicle.VehicleName == string.Empty ? vehicle.LicensePlate :"  "+ vehicle.VehicleName;
                node.parent = Guid.Empty.ToString();
                node.icon = "la la-car ";
                nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetTargetByPeriod(string vehicleId)
        {
            var id = Guid.Parse(vehicleId);
            var start = DateTime.Now.Date.ToString("yyyy-M-dHH:mm");
            var endPeriod = DateTime.Now;
            var startPeriod = new DateTime();
            try
            {
                DateTime.TryParseExact(start, "yyyy-M-dHH:mm", null, DateTimeStyles.AssumeLocal, out  startPeriod);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                //throw;
            }

            var vehicle = await ObjectContext.Vehicles.FindAsync(id);
            var positions = await _positionService.GetVehiclePositionsByPeriod(id, startPeriod, endPeriod);
            if (!positions.Any())
                return Json(new List<TargetViewModel>(), JsonRequestBehavior.AllowGet);
            var gpsCollection = positions.Select(x =>
                new { Latitude = x.Lat, Longitude = x.Long, GpsStatement = x.Timestamp.ToString("O") });
            var positionReport = new PositionReport();
            return Json(new { Periods = await positionReport.GetTargetViewModels(positions, startPeriod, vehicle.VehicleName), GpsCollection = gpsCollection }, JsonRequestBehavior.AllowGet);

        }

       

        
      
    }
}