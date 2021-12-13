using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;
using Simulation.Model.Abstract;

namespace Simulation.Model.Abstract
{
    public class NetworkPointNode
    {
        #region Varialbes        
        public uint ID;
        private SimNode _coreNode;
        public SimNode CoreNode
        {
            get { return _coreNode; }
        }
        public float TotalCost { get; set; }
        public bool IsVisited { get; set; }
        public List<NetworkLineNode> inLines { get; set; } //들어오고 나가는 line 리스트
        public List<NetworkLineNode> outLines { get; set; }
        #endregion

        public NetworkPointNode(uint id, SimNode core)
        {
            ID = id;
            _coreNode = core;
            TotalCost = -1;
            IsVisited = false;
            inLines = new List<NetworkLineNode>();
            outLines = new List<NetworkLineNode>();
        }
    }
}
