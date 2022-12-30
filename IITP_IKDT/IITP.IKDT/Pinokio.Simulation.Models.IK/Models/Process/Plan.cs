using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public class Plan
    {
        public string Name { get; set; }
        public string CoilKindName { get; set; }
        public uint Standard { get; set; }
        public string StandardUnit { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime DueDateTime { get; set; }
        public string FactoryName { get; set; } // To Be Processing Factory Name
    }
}
