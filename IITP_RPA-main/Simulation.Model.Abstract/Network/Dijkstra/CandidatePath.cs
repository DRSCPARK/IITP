using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class CandidatePath
    {
        Fab _fab;
        RailPointNode _fromPoint;
        RailPointNode _toPoint;
        bool _reticle = false;
        private double _elapsedTime; // 초단위

        public double ElapsedTime
        {
            get { return _elapsedTime; }
            set { _elapsedTime = value; }
        }

        public bool Reticle
        {
            get { return _reticle; }
        }
        public List<RailPointNode> RailPoints { get; set; }
        public List<RailLineNode> RailLines { get; set; }
        public List<NetworkLineNode> NWLines { get; set; }
        public List<NetworkPointNode> NWPoints { get; set; }
        public RailPointNode FromNode
        {
            get { return _fromPoint; }
        }

        public RailPointNode ToNode
        {
            get { return _toPoint; }
        }

        public double Length
        {
            get
            {
                double totalLength = 0;

                foreach (RailLineNode line in RailLines)
                {
                    totalLength = totalLength + line.Length;
                }

                return totalLength;
            }
        }

        public int OhtCount
        {
            get
            {
                int ohtCount = 0;

                foreach(RailLineNode line in RailLines)
                {
                    ohtCount = ohtCount + line.OHTCount;
                }

                return ohtCount;
            }
        }


        public double WeightSumScore
        {
            get; set;
        }


        public CandidatePath(Fab fab, RailPointNode fromPoint, RailPointNode toPoint, bool reticle)
        {
            RailPoints = new List<RailPointNode>();
            RailLines = new List<RailLineNode>();
            NWLines = new List<NetworkLineNode>();

            _fab = fab;
            _fromPoint = fromPoint;
            _toPoint = toPoint;
            _reticle = reticle;
        }

        public CandidatePath(Fab fab, RailPointNode fromPoint, RailPointNode toPoint, bool reticle, string candidatePath)
        {
            RailPoints = new List<RailPointNode>();
            RailLines = new List<RailLineNode>();
            NWLines = new List<NetworkLineNode>();

            _fab = fab;
            _fromPoint = fromPoint;
            _toPoint = toPoint;
            _reticle = reticle;

            AddRailLines(candidatePath);
        }

        public string GetRoadNodeNamesWithArrow()
        {
            string names = "";


            for (int i = 0; i < RailPoints.Count; i++)
            {
                RailPointNode node = RailPoints[i];

                if (i == RailPoints.Count - 1)
                    names = names + i + "th: " + node.Name;
                else
                    names = names + i + "th: " + node.Name + " -> ";
            }

            return names;
        }

        public string GetRoadNodeNames()
        {
            string names = "";


            for (int i = 0; i < RailPoints.Count; i++)
            {
                RailPointNode node = RailPoints[i];

                if (i == RailPoints.Count - 1)
                    names = names + node.Name;
                else
                    names = names + node.Name + ",";
            }

            return names;
        }

        public string GetRailLineIDs()
        {
            string ids = "";

            for (int i = 0; i < RailLines.Count; i++)
            {
                RailLineNode line = RailLines[i];

                if (i == RailLines.Count - 1)
                    ids = ids + line.ID;
                else
                    ids = ids + line.ID + ",";
            }

            return ids;
        }

        public void AddRailLines(string parsingData)
        {
            uint id1 = 0;
            uint id2 = 0;

            string[] strArr = parsingData.Split(',');

            for (int j = 0; j < strArr.Count(); j++)
            {
                RailLineNode rl = ModelManager.Instance.SimNodes[Convert.ToUInt32(strArr[j])] as RailLineNode;
                RailLines.Add(rl);
            }
        }
    }
}
