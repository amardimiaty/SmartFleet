using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Data;
using SmartFleet.Service.Common;
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
    public class VehicleReportController : BaseController
    {
        private readonly IPositionService _positionService;
        private readonly ICustomerService _customerService;
        private readonly IVehicleService _vehicleService;
        private readonly IPdfService _pdfService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectContext"></param>
        /// <param name="mapper"></param>
        /// <param name="positionService"></param>
        /// <param name="customerService"></param>
        /// <param name="vehicleService"></param>
        /// <param name="pdfService"></param>
        public VehicleReportController(SmartFleetObjectContext objectContext,
            IMapper mapper,
            IPositionService positionService,
            ICustomerService customerService,
            IVehicleService vehicleService, 
            IPdfService pdfService ) : base(objectContext, mapper)
        {
            _positionService = positionService;
            _customerService = customerService;
            _vehicleService = vehicleService;
            _pdfService = pdfService;
        }
        // GET: VehicleReport
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return PartialView();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetVehicles()
        {
            var cst = _customerService.GetOwnerCustomer(User.Identity.Name);
            var vehicles = await ObjectContext.Vehicles.Where(x => x.CustomerId == cst.Id).Select(x=>new {x.VehicleName, x.Id}).ToArrayAsync();
            return Json(vehicles, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <param name="startPeriod"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetDailyVehicleReport(string vehicleId, string startPeriod)
        {
            var id = Guid.Parse(vehicleId);
            DateTime start;
            try
            {
                DateTime.TryParseExact(startPeriod, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal, out start);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            var endPeriod = start.AddHours(24).AddTicks(-1);
            var vehicle = await _vehicleService.GetVehicleById(id);
            var positions = await _positionService.GetVehiclePositionsByPeriod(id, start, endPeriod);
            if (positions.Any())
                return Json(new CompleteDailyReport(positions, vehicle), JsonRequestBehavior.AllowGet);
            return Json(
                new CompleteDailyReport
                {
                    VehicleName = vehicle?.VehicleName,
                    ReportDate = start.ToShortDateString(),
                    Positions = new List<TargetViewModel>()
                }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// export daily report to pdf
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <param name="startPeriod"></param>
        /// <returns></returns>
        public async Task<FileResult> ExportReportPdf(string vehicleId, string startPeriod)
        {
            var id = Guid.Parse(vehicleId);
            DateTime start;
            try
            {
                DateTime.TryParseExact(startPeriod, "yyyy-MM-dd", null, DateTimeStyles.AssumeLocal, out start);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            var endPeriod = start.AddHours(24).AddTicks(-1);
            var vehicle = await _vehicleService.GetVehicleById(id);
            var positions = await _positionService.GetVehiclePositionsByPeriod(id, start, endPeriod);
            MemoryStream stream = new MemoryStream();
            if (positions.Any())
            {
                var report = new CompleteDailyReport(positions, vehicle);
                _pdfService.CreatePdfReport(positions, vehicle, report,stream);
            }
            stream.Flush(); //Always catches me out
            stream.Position = 0; //Not sure if this is required
            return File(stream, "application/pdf", $"reeport_{vehicle?.VehicleName}_{DateTime.Now.Date:yyyy-MM-dd}.pdf");

        }

        // ReSharper disable once MethodTooLong
     
       
    }
}