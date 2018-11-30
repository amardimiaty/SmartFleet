using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Data;
using SmartFleet.Service.Tracking;
using SmartFLEET.Web.DailyRports;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.Controllers
{
    public class PositionController : BaseController
    {
        private readonly IPositionService _positionService;

        // GET: Position
        public ActionResult Index()
        {
            return View();
        }
        public async Task<JsonResult> GetPositionByDate(string vehicleId, string start)
        {
            var id = Guid.Parse(vehicleId);
           // var endPeriod = DateTime.Now;
            var startPeriod = new DateTime();
            try
            {
                DateTime.TryParseExact(start, "yyyy-MM-dHH:mm", null, DateTimeStyles.AssumeLocal, out startPeriod);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                //throw;
            }
            var endPeriod =startPeriod.Date.AddDays(1).AddTicks(-1);
            var vehicle = await ObjectContext.Vehicles.FindAsync(id);
            var positions = await _positionService.GetVehiclePositionsByPeriod(id, startPeriod, endPeriod);
            if (!positions.Any()) return Json(new List<TargetViewModel>(), JsonRequestBehavior.AllowGet);
            var gpsCollection = positions.Select(x =>
                new { Latitude = x.Lat, Longitude = x.Long, GpsStatement = x.Timestamp.ToString("O") });
            var positionReport = new PositionReport();
            return Json(new {Vehiclename = vehicle?.VehicleName, Periods =  positionReport.GetTargetViewModels(positions, startPeriod, vehicle.VehicleName), GpsCollection = gpsCollection }, JsonRequestBehavior.AllowGet);

        }

        public PositionController(SmartFleetObjectContext objectContext) : base(objectContext)
        {
        }

        public PositionController(SmartFleetObjectContext objectContext, IMapper mapper, IPositionService positionService) : base(objectContext, mapper)
        {
            _positionService = positionService;
        }
    }
}