using Simulation.Engine;
using Simulation.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class FabSimNode: SimNode
    {
        #region Variables
        private Fab fab;
        protected List<RailPortNode> _connectPort; //port ID, port

        [Browsable(false)]
        public Fab Fab
        {
            get { return fab; }
        }

        [Browsable(false)]
        public List<RailPortNode> DicRailPort
        {
            get { return _connectPort; }
            set { _connectPort = value; }
        }

        #endregion

        public FabSimNode()
        {
            _connectPort = new List<RailPortNode>();
        }

        public FabSimNode(uint ID, string objName, Fab fab)
    : base(ID, objName)
        {
            this.fab = fab;
            _connectPort = new List<RailPortNode>();
        }
        public FabSimNode(uint ID, string objName, Vector3 pos, Fab fab)
            : base(ID, objName, pos)
        {
            this.fab = fab;
            _connectPort = new List<RailPortNode>();
        }

        public FabSimNode(uint ID, string objName, Vector3 pos, Vector3 size, Fab fab)
    : base(ID, objName, pos, size)
        {
            this.fab = fab;
            _connectPort = new List<RailPortNode>();
        }

        public FabSimNode(uint ID, string objName, Enum nodeType, Fab fab)
    : base(ID, objName, OBJ_TYPE.NODE)
        {
            this.fab = fab;
            _connectPort = new List<RailPortNode>();
        }

        public SimLink GetOutLink(SimNode node)
        {
            foreach (SimLink link in OutLinks)
            {
                if (link.EndNode == node)
                    return link;
            }
 
            return null;
        }

        public SimLink GetInLink(SimNode node)
        {
            foreach(SimLink link in InLinks)
            {
                if (link.StartNode == node)
                    return link;
            }

            return null;
        }
    }
}
