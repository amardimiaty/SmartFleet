using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Core.Domain.Vehicles;
using SmartFleet.Data;
using SmartFleet.Service.Vehicles;
using SmartFLEET.Web.Areas.Administrator.Models;
using SmartFLEET.Web.Controllers;

namespace SmartFLEET.Web.Areas.Administrator.Controllers
{
    public class VehicleController : BaseController
    {
        private readonly IVehicleService _vehicleService;

        public ActionResult Index()
        {
            return PartialView("Index");
        }
        [HttpGet]
        public async Task<JsonResult> GetAllVehicles()
        {
            var result = Mapper.Map<List<VehicleViewModel>>(await ObjectContext.Vehicles.Include("Brand")
                .Include("Model").Include("Customer").ToListAsync());
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public VehicleController(SmartFleetObjectContext objectContext,IMapper mapper, IVehicleService vehicleService ) : base(objectContext, mapper)
        {
            _vehicleService = vehicleService;
        }

        public ActionResult Detail()
        {
            //var id = Guid.Parse(vehicleId);
            //var vehicleviewModel = Mapper.Map<VehicleViewModel>(ObjectContext.Vehicles.Include("Customer").FirstOrDefault(x=>x.Id == id));
            return PartialView("Detail");
        }

        public async Task<JsonResult> GetVehicleDetail(string vehicleId)
        {
            var id = Guid.Parse(vehicleId);
            return Json(Mapper.Map<VehicleViewModel>(await ObjectContext.Vehicles.Include(x=>x.Brand).Include(x=>x.Customer).Include(x=>x.Model).Include(x=>x.Boxes).FirstOrDefaultAsync(v=>v.Id==id)),
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetNewVehicle()
        {
            return Json(new AddVehicleViewModel(ObjectContext), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddNewVehicle(AddVehicleViewModel model)
        {
            ValidationViewModel validationModel;

            if (ModelState.IsValid)
            {
                var vehicle = Mapper.Map<Vehicle>(model);
                vehicle.VehicleType = (VehicleType)Enum.Parse(typeof(VehicleType), model.VehicleType);
                if (vehicle.Box_Id != null)
                    vehicle.VehicleStatus = VehicleStatus.Active;
                _vehicleService.AddNewVehicle(vehicle);
                validationModel = new ValidationViewModel(new List<string>(), "Ok");
                return Json(validationModel, JsonRequestBehavior.AllowGet);
            }

            var errors = (from modelStateValue in ModelState.Values
                from error in modelStateValue.Errors
                select error.ErrorMessage).ToList();
            validationModel = new ValidationViewModel(errors, "Validation's errors");

            return Json(validationModel, JsonRequestBehavior.AllowGet);
        }
    }
}