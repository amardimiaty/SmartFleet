using System;
using System.Collections.Generic;
using System.Linq;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Protocols.Teltonika;

namespace SmartFleet.Core.Protocols
{
    public class DevicesParser
    {
       
        public List<CreateTeltonikaGps> Decode(List<byte> receiveBytes, string imei)
        {
            IFMParserProtocol parser = null;
            //Get codec ID and initialize appropriate parser
            var codecId = Convert.ToInt32(receiveBytes.Skip(8).Take(1).ToList()[0]);
            switch (codecId)
            {
                case 8:
                    parser = new FMXXXXParserV2();
                    break;
                case 7:
                    parser = new Gh3000Parser();
                    break;
                default:
                    throw new Exception("Unsupported device type code: " + codecId);
            }
           return parser.DecodeAvl(receiveBytes, imei);
          
        }
    }
}
