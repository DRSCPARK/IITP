using System;
using System.Text;

namespace Pinokio.Simulation.Models.IK
{
    public struct CoilObsn
    {
        public string Name { get; set; }
        public string State { get; set; }
        public double RemainingDueDate { get; set; }
        public Setup SetUp { get; set; }
        public double EnPortTime { get; set; }
    }
}
