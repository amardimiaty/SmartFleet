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


      
    }
}