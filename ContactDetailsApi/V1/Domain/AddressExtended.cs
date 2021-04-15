using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Domain
{
    public class AddressExtended
    {
        public int UPRN { get; set; }
        public bool IsOverseasAddress { get; set; }
        public string OverseasAddress { get; set; }
    }
}
