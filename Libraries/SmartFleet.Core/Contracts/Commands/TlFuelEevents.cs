using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFleet.Core.Contracts.Commands
{
    public class TlFuelEevents
    {
        public Guid Id { get; set; }
        public List<TLFuelMilstoneEvent> Events { get; set; }
    }
}
