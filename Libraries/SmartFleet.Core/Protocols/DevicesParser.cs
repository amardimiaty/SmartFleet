using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Helpers;
using SmartFleet.Core.Protocols.Teltonika;

namespace SmartFleet.Core.Protocols
{
    public class DevicesParser
    {
        public string GetIMEI(byte[] buffer, int recievedBytes)
        {
            var imei = string.Empty;
            var dataReceived = Encoding.ASCII.GetString(buffer, 2, recievedBytes);
            if (imei != string.Empty || !Commonhelper.IsValidImei(dataReceived)) return imei;
            imei = dataReceived;
            return imei;
        }

        public List<CreateTeltonikaGps> Decode(List<byte> receiveBytes, string imei)
        {
            string myString = System.Text.Encoding.ASCII.GetString(receiveBytes.ToArray()).Trim();
            Trace.WriteLine(myString);
            IFMParserProtocol parser = null;
            //Get codec ID and initialize appropriate parser
            var head = receiveBytes.Skip(8).Take(1).ToList();
            var codecId = Convert.ToInt32(head[0]);
            switch (codecId)
            {
                case 8:
                    parser = new FmxxxxParser();
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
