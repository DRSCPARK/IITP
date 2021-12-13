using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    public class CompleteNode : FabSimNode
    {
        #region Variables
        private COMPLETE_TYPE _completeType;
        private Dictionary<uint, SimEntity> _dicEntities;
        
        #endregion

        public CompleteNode()
            : base(0, "", null)
        {
        }
        public CompleteNode(uint ID, Vector3 pos, string name, Fab fab, COMPLETE_TYPE type)
            : base(ID, name, pos, fab)
        {
            _completeType = type;
        }

        public CompleteNode(uint ID, string name, Fab fab, COMPLETE_TYPE type)
            : base(ID, name, fab)
        {
            _completeType = type;
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);
            _dicEntities = new Dictionary<uint, SimEntity>();
            Receivable = true;
        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            Console.WriteLine("[" + simTime + "] : complete Part Ext ID : " + port.Entity.ID);
            _dicEntities.Add(port.Entity.ID, port.Entity);
           // ModelManager.Instance.RemoveEntitybyID(port.Entity.ID);
        }

        public override void OutputFunction(Time simTime, SimHistory simLogs, SimPort port)
        {            
        }
    }
}
