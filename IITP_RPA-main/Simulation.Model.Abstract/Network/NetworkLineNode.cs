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
    public class NetworkLineNode
    {
        #region Variables        
        public uint ID;
        public double Length { get; set; }
        public bool IsVisited { get; set; }
        public bool IsAvailable { get; set; }
        public NetworkPointNode ToNode { get; set; }
        public NetworkPointNode FromNode { get; set; }

        public uint ToNodeID { get { return ToNode.ID; } }
        public uint FromNodeID { get { return FromNode.ID; } }

        public float TotalCost
        {
            get
            {
                return RailLines.Sum(x => x.TotalCost);
            }
        }
        public RailLineNode[] RailLines { get; set; }

        #endregion

        public NetworkLineNode(uint id, double length, NetworkPointNode fromNode, NetworkPointNode toNode, List<RailLineNode> railLines)
        {
            ID = id;
            Length = length;
            ToNode = toNode;
            FromNode = fromNode;
            IsVisited = false;

            fromNode.outLines.Add(this);
            toNode.inLines.Add(this);

            RailLines = railLines.ToArray();
            foreach (RailLineNode line in railLines)
                line.NWLines.Add(this);
        }
    }
}
