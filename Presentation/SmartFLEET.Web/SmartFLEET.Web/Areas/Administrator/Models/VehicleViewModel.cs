using System;

namespace SmartFLEET.Web.Areas.Administrator.Models
{
    public class VehicleViewModel
    {
        public Guid Id { get; set; }
        public string VehicleName { get; set; }
        public string LicensePlate { get; set; }
        public string Vin { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Customer { get; set; }
        public string Imei { get; set; }
        public string VehicleStatus { get; set; }
        public string VehicleType { get; set; }

    }
}