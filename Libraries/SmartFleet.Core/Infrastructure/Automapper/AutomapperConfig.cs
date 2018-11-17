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
        public static MapperConfiguration Config<TSource, TDest>()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDest>();

            });

            return config;
        }
    }
}
