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
    public class Bay : Zone
    {
        #region Variables

        protected Vector3 pos;
        protected Vector3 size;
        protected bool _reticle;

        protected BAY_TYPE bayType;
        protected List<OHTNode> bumpingOHTs;
        protected List<OHTNode> depositOHTs;
        protected List<RailPortNode> bumpingPorts;
        protected List<Bay> neighborBay;

        public Vector3 Position
        { 
            get { return pos; }
            set { pos = value; }
        }

        public Vector3 Size
        { 
            get { return size; }
            set { size = value; }
        }

        public bool Reticle
        { 
            get { return _reticle; }
            set { _reticle = value; }
        }

        public BAY_TYPE BayType
        {
            get { return bayType; }
            set { bayType = value; }
        }

        public List<OHTNode> BumpingOHTs
        {
            get { return bumpingOHTs; }
            set { bumpingOHTs = value; }
        }

        public List<OHTNode> DepositOHTs
        {
            get { return depositOHTs; }
            set { depositOHTs = value; }
        }

        public List<OHTNode> DispatchableOHTs
        {
            get 
            {
                List<OHTNode> result = new List<OHTNode>();

                //합치기
                result.AddRange(depositOHTs);
                result.AddRange(bumpingOHTs);

                //중복 제거
                result = result.Distinct().ToList();

                return result;
            }
        }

        public List<RailPortNode> BumpingPorts
        {
            get { return bumpingPorts; }
            set { bumpingPorts = value; }
        }

        /// <summary>
        /// Bay 내 BumpingOHT 대수가 Max 상태인가?
        /// InterBay의 경우 기본 Max값은 20개.
        /// IntraBay의 경우 기본 Max값은 100개.
        /// ZoneHelper에서 Max값 변경 가능
        /// </summary>
        public bool IsMaxBumping
        {
            get
            {
                bool isMaxBumping = true;

                int bumpingOHTCount = bumpingOHTs.Count;

                switch (bayType)
                {
                    case BAY_TYPE.INTERBAY:
                        if (bumpingOHTCount <= ZoneHelper.Instance.InterBayBumpingOHTMaxValue)
                        { isMaxBumping = false; }
                        break;
                    case BAY_TYPE.INTRABAY:
                        if (bumpingOHTCount <= ZoneHelper.Instance.IntraBayBumpingOHTMaxValue)
                        { isMaxBumping = false; }
                        break;
                }

                return isMaxBumping;
            }
        }

        public List<RailPortNode> LeftSideRailPorts
        {
            get 
            {
                List<RailPortNode> leftSideRailPorts = new List<RailPortNode>();

                foreach(RailPortNode port in ports)
                {
                    if (port.PosVec3.X == pos.X)
                        leftSideRailPorts.Add(port);
                }

                IEnumerable<RailPortNode> sort =
                from node in leftSideRailPorts
                orderby node.PosVec3.Y
                select node;

                leftSideRailPorts = sort.ToList();


                return leftSideRailPorts;
            }
        }

        public List<RailPortNode> RightSideRailPorts
        {
            get
            {
                List<RailPortNode> rightSideRailPorts = new List<RailPortNode>();

                foreach (RailPortNode port in ports)
                {
                    if (port.PosVec3.X == pos.X + size.X)
                        rightSideRailPorts.Add(port);
                }

                IEnumerable<RailPortNode> sort =
                from node in rightSideRailPorts
                orderby node.PosVec3.Y
                select node;

                rightSideRailPorts = sort.ToList();

                return rightSideRailPorts;
            }
        }

        public List<RailLineNode> LeftOutSideLines
        {
            get
            {
                List<RailLineNode> leftOutSideLines = new List<RailLineNode>();

                foreach (RailLineNode line in lines)
                {
                    if (line.StartPoint.X == pos.X && line.EndPoint.X == pos.X)
                        leftOutSideLines.Add(line);
                }

                IEnumerable<RailLineNode> sort =
                from node in leftOutSideLines
                orderby node.PosVec3.Y
                select node;

                leftOutSideLines = sort.ToList();

                return leftOutSideLines;
            }
        }

        public List<RailLineNode> RightOutSideLines
        {
            get
            {
                List<RailLineNode> outSideLines = new List<RailLineNode>();

                foreach (RailLineNode line in lines)
                {
                    if (line.StartPoint.X == pos.X + size.X && line.EndPoint.X == pos.X + size.X)
                        outSideLines.Add(line);
                }

                IEnumerable<RailLineNode> sort =
                from node in outSideLines
                orderby node.PosVec3.Y
                select node;

                outSideLines = sort.ToList();

                return outSideLines;
            }
        }

        public List<Bay> NeighborBay
        {
            get { return neighborBay; }
            set { neighborBay = value; }
        }
        #endregion

        public Bay(uint id, string name, Fab fab, bool reticle)
        :base(id, fab)
        {
            ZONE_TYPE = ZONE_TYPE.BAY;
            Name = name;
            _reticle = reticle;
            bumpingOHTs = new List<OHTNode>();
            depositOHTs = new List<OHTNode>();
            bumpingPorts = new List<RailPortNode>();
            neighborBay = new List<Bay>();
        }

        public Bay(uint id, Vector3 pos, Vector3 size, string name, Fab fab, bool reticle)
        : base(id, fab)
        {
            ZONE_TYPE = ZONE_TYPE.BAY;
            Name = name;
            this.pos = pos;
            this.size = size;
            _reticle = reticle;
            bumpingOHTs = new List<OHTNode>();
            depositOHTs = new List<OHTNode>();
            bumpingPorts = new List<RailPortNode>();
            neighborBay = new List<Bay>();
        }

        public RailPointNode GetTopLeftPoint()
        {
            RailPointNode topLeftPoint = null;

            foreach(RailPointNode point in points)
            {
                if (topLeftPoint == null)
                    topLeftPoint = point;

                if (point.PosVec3.X == Position.X && point.PosVec3.Y >= topLeftPoint.PosVec3.Y)
                    topLeftPoint = point;
            }

            return topLeftPoint;
        }

        public RailPointNode GetTopRightPoint()
        {
            RailPointNode topRightPoint = null;

            foreach (RailPointNode point in points)
            {
                if (topRightPoint == null)
                    topRightPoint = point;

                if (point.PosVec3.X >= Position.X + size.X && point.PosVec3.Y >= topRightPoint.PosVec3.Y)
                    topRightPoint = point;
            }

            return topRightPoint;
        }

        public RailPointNode GetLeftBottomPoint()
        {
            RailPointNode bottomLeftPoint = null;

            foreach (RailPointNode point in points)
            {
                if (bottomLeftPoint == null)
                    bottomLeftPoint = point;

                if (point.PosVec3.X == Position.X && point.PosVec3.Y <= bottomLeftPoint.PosVec3.Y)
                    bottomLeftPoint = point;
            }

            return bottomLeftPoint;

        }

        public RailPointNode GetRightBottomPoint()
        {
            RailPointNode bottomRightPoint = null;

            foreach (RailPointNode point in points)
            {
                if (bottomRightPoint == null)
                    bottomRightPoint = point;

                if (point.PosVec3.X == Position.X + size.X && point.PosVec3.Y <= bottomRightPoint.PosVec3.Y)
                    bottomRightPoint = point;
            }

            return bottomRightPoint;

        }

        public RailPointNode GetTopLeftPointByZCU()
        {
            RailPointNode topLeftPoint = null;

            foreach (RailPointNode point in points)
            {
                if(point.Zcu != null)
                {
                    if (topLeftPoint == null)
                        topLeftPoint = point;

                    if (point.PosVec3.X == Position.X && point.PosVec3.Y >= topLeftPoint.PosVec3.Y)
                        topLeftPoint = point;
                }
            }

            return topLeftPoint;
        }

        public RailPointNode GetTopRightPointByZCU()
        {
            RailPointNode topRightPoint = null;

            foreach (RailPointNode point in points)
            {
                if (point.Zcu != null)
                {
                    if (topRightPoint == null)
                        topRightPoint = point;

                    if (point.PosVec3.X >= Position.X + size.X && point.PosVec3.Y >= topRightPoint.PosVec3.Y)
                        topRightPoint = point;
                }
            }

            return topRightPoint;
        }

        public RailPointNode GetLeftBottomPointByZCU()
        {
            RailPointNode bottomLeftPoint = null;

            foreach (RailPointNode point in points)
            {
                if (point.Zcu != null)
                {
                    if (bottomLeftPoint == null)
                        bottomLeftPoint = point;

                    if (point.PosVec3.X == Position.X && point.PosVec3.Y <= bottomLeftPoint.PosVec3.Y)
                        bottomLeftPoint = point;
                }
            }

            return bottomLeftPoint;

        }

        public RailPointNode GetRightBottomPointByZCU()
        {
            RailPointNode bottomRightPoint = null;

            foreach (RailPointNode point in points)
            {
                if (point.Zcu != null)
                {
                    if (bottomRightPoint == null)
                        bottomRightPoint = point;

                    if (point.PosVec3.X == Position.X + size.X && point.PosVec3.Y <= bottomRightPoint.PosVec3.Y)
                        bottomRightPoint = point;
                }
            }

            return bottomRightPoint;

        }

        public RailLineNode GetNearestLineByPoint(RailPointNode point)
        {
            RailLineNode nearestLIne = null;
            double shortestDistance = 0;

            foreach(RailLineNode line in lines)
            {
                if(line.vec2Direction.Y == 0)
                {
                    if (nearestLIne == null)
                        nearestLIne = line;

                    double distance = Math.Abs(point.PosVec3.Y - line.StartPoint.Y);

                    if (distance < shortestDistance)
                        nearestLIne = line;
                }
            }

            return nearestLIne;            
        }
        public void SetPosition(Vector3 position)
        {
            pos = position;
        }
    }
}
