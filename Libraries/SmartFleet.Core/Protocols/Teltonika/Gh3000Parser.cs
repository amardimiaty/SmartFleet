using System;
using System.Collections.Generic;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Domain.Movement;

namespace SmartFleet.Core.Protocols.Teltonika
{
    public class Gh3000Parser : IFMParserProtocol
    {
        public Gh3000Parser()
        {
            

        }

        public List<CreateTeltonikaGps> DecodeAvl(List<byte> receiveBytes, string imei)
        {
            throw new NotImplementedException();
        }
    }
}