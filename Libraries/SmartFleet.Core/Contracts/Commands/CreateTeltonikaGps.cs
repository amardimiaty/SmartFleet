using System;
using System.Collections.Generic;

namespace SmartFleet.Core.Contracts.Commands
{
    public class CreateTeltonikaGps
    {
        public CreateTeltonikaGps()
        {
            IoElements_1B = new Dictionary<byte, long>();
            IoElements_2B = new Dictionary<byte, long>();
            IoElements_4B = new Dictionary<byte, long>();
            IoElements_8B = new Dictionary<byte, long>();
        }
        public string Imei { get; set; }
        public Dictionary<byte, long> IoElements_8B;
        public byte EventIoElementId;
        public Dictionary<byte, long> IoElements_1B;
        public Dictionary<byte, long> IoElements_2B;
        public Dictionary<byte, long> IoElements_4B;
        public short Altitude { get; set; }
        public short Direction { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public byte Priority { get; set; }
        public byte Satellite { get; set; }
        public short Speed { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
        public bool StopFlag { get; set; }
        public bool IsStop { get; set; }
        public string Alarm { get; set; }
        public int Mileage { get; set; }
        public int Temprature { get; set; }
    }
}
