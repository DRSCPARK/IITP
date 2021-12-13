using Simulation.Engine;
using Simulation.Geometry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public abstract class Zone : SimObj
    {
        #region Variables

        protected ZONE_TYPE zoneType;
        protected Fab fab;

        protected List<RailPointNode> points;
        protected List<RailLineNode> lines;
        protected List<ProcessEqpNode> processes;
        protected List<BufferNode> buffers;
        protected List<RailPortNode> ports;

        protected List<RailLineNode> fromLines;
        protected List<RailLineNode> toLines;

        protected List<OHTNode> totalOHTs;

        public ZONE_TYPE ZONE_TYPE
        {
            get { return zoneType; }
            set { zoneType = value; }
        }

        public Fab Fab
        { get { return fab; } }

        public List<RailPointNode> Points
        {
            get { return points; }
            set { points = value; }
        }

        public List<RailLineNode> Lines
        {
            get { return lines; }
            set { lines = value; }
        }

        public List<ProcessEqpNode> ProcessEqps
        {
            get { return processes; }
            set { processes = value; }
        }

        public List<RailPortNode> Ports
        {
            get { return ports; }
            set { ports = value; }
        }

        public List<OHTNode> TotalOHTs
        {
            get { return totalOHTs; }
            set { totalOHTs = value; }
        }

        public List<RailLineNode> FromLines
        {
            get { return fromLines; }
            set { fromLines = value; }
        }

        public List<RailLineNode> ToLines
        {
            get { return toLines; }
            set { toLines = value; }
        }

        #endregion

        public Zone(uint id, Fab fab )
        {
            Name = RETICLE_ZONE_NAME.RETICLE_ZONE.ToString();
            ID = id;
            this.fab = fab;
            points = new List<RailPointNode>();
            lines = new List<RailLineNode>();
            ports = new List<RailPortNode>();
            processes = new List<ProcessEqpNode>();
            fromLines = new List<RailLineNode>();
            toLines = new List<RailLineNode>();
            totalOHTs = new List<OHTNode>();
        }
    }
}
