using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SmartFleet.Data;

namespace SmartFLEET.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly SmartFleetObjectContext ObjectContext;
        protected readonly IMapper Mapper;

        public BaseController(SmartFleetObjectContext objectContext)
        {
            ObjectContext = objectContext;
        }
        public BaseController(SmartFleetObjectContext objectContext, IMapper mapper)
        {
            ObjectContext = objectContext;
            Mapper = mapper;
        }
        public BaseController( IMapper mapper)
        {
            Mapper = mapper;
        }


        #region All queries

        [HttpGet]
        public async Task<JsonResult> GetAllBoxes()
        {
            return Json(await ObjectContext.Boxes.ToListAsync());
        }
        [HttpGet]
        public async Task<JsonResult> GetAllBrands()
        {
            return Json(await ObjectContext.Brands.ToListAsync());
        }
       
        public async Task<JsonResult> GetAllCustomers()
        {
            return Json(await ObjectContext.Customers.ToListAsync());
        }

        #endregion
        [HttpGet]
        public async Task<JsonResult> GetBox(string id)
        {
            var boxId = Guid.Parse(id);
            return Json(await ObjectContext.Boxes.FindAsync(boxId));
        }
    }
}