using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace SmartFleet.Core.Infrastructure.Automapper
{
   public static class AutomapperConfig
    {
        public static IMapper Mapper<TSource, TDest>()
        {

            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDest>();

            }).CreateMapper();
        }
    }
}
