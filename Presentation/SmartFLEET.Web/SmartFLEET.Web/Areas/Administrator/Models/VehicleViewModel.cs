using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SmartFleet.Core.Domain.Vehicles;

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
        [JsonConverter(typeof(StringEnumConverter))]
        public VehicleStatus VehicleStatus { get; set; }
        public string VehicleType { get; set; }
        public string CreationDate { get; set; }
        public string InitServiceDate { get; set; }

    }
}