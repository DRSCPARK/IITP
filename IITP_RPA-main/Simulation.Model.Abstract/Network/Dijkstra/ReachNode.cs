using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class ReachNode : IComparable<ReachNode>
    {
        #region Member Variable

        public NetworkPointNode _networkPoint { get; set; }

        public NetworkLineNode _networkLine { get; set; }

        public double TotalCost
        { get; set; }

        #endregion

        public int CompareTo(ReachNode compareNode)
        {
            // A null value means that this object is greater.
            if (compareNode == null)
                return 1;

            else
                return this.TotalCost.CompareTo(compareNode.TotalCost);
        }

        public ReachNode(NetworkPointNode point, NetworkLineNode line)
        {
            _networkPoint = point;
            _networkLine = line;
        }
    }
}
