using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SmartFleet.Core.Domain.Gpsdevices;
using SmartFleet.Core.Domain.Vehicles;
using SmartFleet.Data;

namespace SmartFLEET.Web.Areas.Administrator.Models
{
    //[Validator(typeof(AddVehicleValidator))]
    public  class AddVehicleViewModel
    {
        private readonly SmartFleetObjectContext _context;

        public AddVehicleViewModel(SmartFleetObjectContext context)
        {
            _context = context;
            _context.Configuration.ProxyCreationEnabled = false;
               //  Brand_Id = null;
               VehicleTypes = new List<KeyValuePair<int, string>>();
            foreach (var vehicleType in Enum.GetValues(typeof(VehicleType))
                .Cast<VehicleType>())
            {
                VehicleTypes.Add(new KeyValuePair<int, string>((int)vehicleType, vehicleType.ToString()));
            }
        }

        public AddVehicleViewModel()
        {
            
        }
       
       // public Guid Id { get; set; }
        [Required]
        public string VehicleName { get; set; }
        public string LicensePlate { get; set; }
        public string Vin { get; set; }

        public Guid? Brand_Id { get; set; }
        [Required]
        public Guid? Model_Id { get; set; }
        [Required]
        public Guid? CustomerId { get; set; }
        public VehicleStatus VehicleStatus { get; set; }
        [Required]
        public string VehicleType { get; set; }
        [Required]
        public Guid? Box_Id { get; set; }
        public List<Brand> Brands =>_context?.Brands.ToList();
        public List<Model> Models =>_context?.Models?.ToList();
        public  List<CustomerItemViewModel> Customers => _context?.Customers?.Select(c=>new CustomerItemViewModel(){Id = c.Id, Name = c.Name}).ToList();
        public List<BoxItemModelView> Boxes => _context?.Boxes?.Where(b=>b.BoxStatus!= BoxStatus.Valid).Select(b=>new BoxItemModelView() {Id =b.Id, Imei = b.Imei}).ToList();
        public List<KeyValuePair<int, string>> VehicleTypes { get; set; }
    }
}