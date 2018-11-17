using System.Collections.Generic;
using SmartFleet.Core.Contracts.Commands;

namespace SmartFleet.Core
{
    public interface IFMParserProtocol
    {
        List<CreateTeltonikaGps> DecodeAvl(List<byte> receiveBytes, string imei);
    }
}