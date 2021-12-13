using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Simulation.Engine;
using System.ComponentModel;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    public class TBNode : BufferNode
    {
        #region Member Variable

        public string TBName
        {
            get { return Name; }
        }

        #endregion

        public TBNode()
            : base(0, "", BUFFER_TYPE.RSTB, null)
        {

        }

        public TBNode(uint ID, string name, Fab fab, uint capacity, BUFFER_TYPE type)
            : base(ID, name, fab, capacity, type)
        {
           
        }

        public TBNode(uint ID, Vector3 pos, string name, Fab fab, BUFFER_TYPE type)
            : base(ID, pos, name, fab, 1, type)
        {
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);
            NodeState = RAILPORT_STATE.EMPTY;
        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch ((RAILPORT_STATE)NodeState)
            {

            }
        }

        public override void OutputFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch ((RAILPORT_STATE)NodeState)
            {

            }

        }


        public double GetDistance(Vector2 position)
        {
            return (position.X - PosVec3.X) * (position.X - PosVec3.X) + (position.Y - PosVec3.Y) * (position.Y - PosVec3.Y);
        }
    }
}
