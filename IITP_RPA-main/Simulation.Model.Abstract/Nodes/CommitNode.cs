using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    public class CommitNode : FabSimNode
    {
        #region Variable

        private COMMIT_TYPE _commitType;
        

        #endregion
        public CommitNode()
            : base(0, "", null)
        {
        }
        public CommitNode(uint ID, string name, Fab fab, COMMIT_TYPE type)
            : base(ID, name, fab)
        {
            _commitType = type;
        }
        public CommitNode(uint ID, string name, Vector3 pos, Fab fab, COMMIT_TYPE type)
            : base(ID, name, pos, fab)
        {
            _commitType = type;
        }       

        public override List<string> SaveNode()
        {
            List<string> saveData = base.SaveNode();
            //saveData.Add(_commitType.ToString());

            return saveData;
        }

        public override void LoadNode(List<string> loadData) { }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);           
        }

        public override void InsertKPI()
        {
            base.InsertKPI();
        }
        public override void UpdateKPI(Time simTime)
        {
            base.UpdateKPI(simTime);
        }
    }
}
