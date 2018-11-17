using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SmartFleet.Data;
using SmartFLEET.Web.Controllers;

namespace SmartFLEET.Web.Areas.Administrator.Controllers
{
    public class GpsDeviceController : BaseController
    {
        // GET
        public ActionResult Index()
        {
            return View(ObjectContext.Boxes.ToList());
        }

        public GpsDeviceController(SmartFleetObjectContext objectContext) : base(objectContext)
        {
        }
        
        
    }
}