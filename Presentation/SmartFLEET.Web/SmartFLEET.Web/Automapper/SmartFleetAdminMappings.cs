using System.Linq;
using AutoMapper;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Domain.Customers;
using SmartFleet.Core.Domain.Vehicles;
using SmartFLEET.Web.Areas.Administrator.Models;
using SmartFLEET.Web.Models;

namespace SmartFLEET.Web.Automapper
{
    /// <summary>
    /// 
    /// </summary>
    public class SmartFleetAdminMappings : Profile
    {
        /// <summary>
        /// 
        /// </summary>
        public SmartFleetAdminMappings()
        {
            CreateMap<AddVehicleViewModel, Vehicle>()
                .ForMember(x => x.Boxes, o => o.Ignore())
                .ForMember(x => x.VehicleType, o => o.Ignore())
            //   .ForMember(x => x.VehicleType, o => o.MapFrom(x => (VehicleType)Enum.Parse(typeof(VehicleType), x.VehicleType)))
                .ReverseMap();
            CreateMap<AddCustomerViewModel, Customer>()
                //.ForMember(x => x., o => o.Ignore())
                //.ForMember(x => x.VehicleType, o => o.Ignore())
                //   .ForMember(x => x.VehicleType, o => o.MapFrom(x => (VehicleType)Enum.Parse(typeof(VehicleType), x.VehicleType)))
                .ReverseMap();

            CreateMap<PositionViewModel, CreateTk103Gps>();
            CreateMap<Vehicle, VehicleViewModel>()
                .ForMember(x=>x.Customer, o=>o.MapFrom(v=>v.Customer.Name))
                .ForMember(x => x.Brand, o => o.MapFrom(v => v.Brand.Name))
                .ForMember(x => x.Model, o => o.MapFrom(v => v.Model.Name))
                .ForMember(x => x.Imei, y => y.MapFrom(s => s.Boxes.Any() ? s.Boxes.FirstOrDefault().Imei:""));
        }
       
    }
}