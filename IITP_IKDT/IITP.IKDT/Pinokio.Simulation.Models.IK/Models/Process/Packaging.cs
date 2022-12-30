using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public struct WorkerJob
    {
        public JobType Type { get; set; }
        public SimTime EnqueueTime { get; set; }
        public double ProcessingTime { get; set; }
        public Equipment EQP { get; set; }
        public Coil Coil { get; set; }
        public List<Coil> BatchCoil { get; set; }

    }
}
