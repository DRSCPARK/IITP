using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public struct ToolGroupObsn
    {
        public string Name;
        public FactoryToolType EQPType;
        public Dictionary<string, EQPObsn> EQPObservations;
        public Dictionary<string, CoilObsn> CoilObservations;

        public ToolGroupObsn(string name, FactoryToolType eqpType)
        {
            Name = name;
            EQPType = eqpType;
            EQPObservations = new Dictionary<string, EQPObsn>();
            CoilObservations = new Dictionary<string, CoilObsn>();
        }
    }
}
