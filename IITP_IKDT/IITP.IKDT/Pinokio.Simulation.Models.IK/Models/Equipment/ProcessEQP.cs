using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public class ProcessEQP : Equipment
    {
        public ProcessEQP(uint id, string name) : base(id, name, ModelType.Equipment)
        {

        }
        #region Simulation
        public override void InitializeModel(EventCalendar eventCalendar)
        {
            base.InitializeModel(eventCalendar);
        }

        public override void InternalTransition(SimTime timeNow, SimPort port)
        {
            base.InternalTransition(timeNow, port);
        }

        public override void ExternalTransition(SimTime timeNow, SimPort port)
        {
            base.ExternalTransition(timeNow, port);
        }
        #endregion
    }
}
