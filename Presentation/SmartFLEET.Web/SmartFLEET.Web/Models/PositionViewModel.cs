using System;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Domain.Movement;
using SmartFleet.Core.Domain.Vehicles;

namespace SmartFLEET.Web.Models
{
    public class PositionViewModel
    {
        
        

        public PositionViewModel(Position position, Vehicle vehicle)
        {
            //Task.Run(async () => { await GeoReverse(position); });

            Latitude = position.Lat;
            Longitude = position.Long;
            Address = position.Address;
            //IMEI = position.IMEI;
            //SerialNumber = tk103Gps.SerialNumber;
            //Direction = tk103Gps.Address
            Speed = position.Speed;
            VehicleName = vehicle.VehicleName;
            VehicleId = vehicle.Id.ToString();
            CustomerName = vehicle.Customer?.Name;
            TimeStampUtc = position.Timestamp;
            SetVehicleImage(vehicle);
        }

      
        public PositionViewModel(CreateTk103Gps tk103Gps , Vehicle vehicle)
        {
            Latitude = tk103Gps.Latitude;
            Longitude = tk103Gps.Longitude;
            Address = tk103Gps.Address;
            IMEI = tk103Gps.IMEI;
            SerialNumber = tk103Gps.SerialNumber;
            //Direction = tk103Gps.Address
            Speed = tk103Gps.Speed;
            VehicleName = vehicle.VehicleName;
            VehicleId = vehicle.Id.ToString();
            CustomerName = vehicle.Customer?.Name;
            TimeStampUtc = tk103Gps.TimeStampUtc;
            SetVehicleImage(vehicle);
        }

        public PositionViewModel(CreateTeltonikaGps tk103Gps, Vehicle vehicle)
        {
            Latitude = tk103Gps.Lat;
            Longitude = tk103Gps.Long;
            Address = tk103Gps.Address;
            IMEI = tk103Gps.Imei;
           // SerialNumber = tk103Gps.s;
            //Direction = tk103Gps.Address
            Speed = tk103Gps.Speed;
            VehicleName = vehicle.VehicleName;
            VehicleId = vehicle.Id.ToString();
            CustomerName = vehicle.Customer?.Name;
            TimeStampUtc = tk103Gps.Timestamp;
            SetVehicleImage(vehicle);
        }


        public PositionViewModel(CreateNewBoxGps tk103Gps, Vehicle vehicle)
        {
            Latitude = tk103Gps.Latitude;
            Longitude = tk103Gps.Longitude;
            Address = tk103Gps.Address;
            IMEI = tk103Gps.IMEI;
            SerialNumber = tk103Gps.SerialNumber;
            //Direction = tk103Gps.Address
            Speed = tk103Gps.Speed;
            VehicleName = vehicle.VehicleName;
            VehicleId = vehicle.Id.ToString();
            CustomerName = vehicle.Customer?.Name;
            TimeStampUtc = tk103Gps.TimeStampUtc;
            SetVehicleImage(vehicle);
        }


        private void SetVehicleImage(Vehicle vehicle)
         {
            switch (vehicle.VehicleType)
            {
                case VehicleType.Track:
                {
                    ImageUri = Speed < 1.0 ? "../assets/vehicles/TractorTruckStopped.png" : "../assets/vehicles/TractorTruckMovingLeft.png";
                }
                    break;
                case VehicleType.Car:
                {
                    ImageUri = Speed > 1.0 ? "../assets/vehicles/LightVehicleMovingLeft.png" : "../assets/vehicles/LightVehicleStopped.png";
                }
                    break;
            }
        }

        public double AvgSpeed { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public DateTime TimeStampUtc { get; set; }
        public string IMEI { get; set; }
        public string SerialNumber { get; set; }
        public int Angle { get; set; }
        public double Heading { get; set; }
        public string Address { get; set; }
        public string Region { get; set; }
        public string State { get; set; }
        public string Direction { get; set; }
        public string VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string CustomerName { get; set; }
        public string ImageUri { get; set; }
    }
}