using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    public class RailPortNode : FabSimNode
    {
        #region Variables
        private SimNode _connectSimNode;
        private RailLineNode _railLineNode;
        private double _length;
        //private bool _isEmpty;
        //private int _reservationOHTID;

        public SimNode ConnectSimNode
        {
            get { return _connectSimNode; }
            set { _connectSimNode = value; }
        }
        
        public double Length
        {
            get { return _length; }
        }

        public RailLineNode Line
        {
            get { return _railLineNode; }
        }
        #endregion

        public RailPortNode(uint ID, string name, Fab fab, RailLineNode rl, double length)
            : base(ID, name, fab)
        {
            _length = length;
            _railLineNode = rl;
            PosVec3 = CalPos();
            rl.AddStationPoint(this);
        }
        public Vector3 CalPos()
        {
            return _railLineNode.StartPoint + _railLineNode.Direction * _length;
        }
    }
}



