using System;
using System.Collections.Generic;
using System.Text;

namespace Pinokio.Simulation.Models.IK
{
    public struct EQPObsn
    {
        public string Name;
        public string State;
        public double LastProcessEndTime;
        public double ExpectedProcessEndTime;
        public double SetupFinishTime;
        public string CurrentSetUpName;
        public PortObsn PortObsns;
    }
}
