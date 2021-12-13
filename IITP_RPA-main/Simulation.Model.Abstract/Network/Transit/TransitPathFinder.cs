using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Simulation.Model.Abstract
{
    public class TransitPathFinder
    {
        #region Private Variables 
        private static TransitPathFinder _instance;
        #endregion

        public static TransitPathFinder Instance { get { return _instance; } }
        
        public TransitPathFinder()
        {
            _instance = this;
        }

        public List<RailLineNode> TransitPath(RailPortNode startPort, RailPortNode endPort)
        {
            List<RailLineNode> shortestLines = new List<RailLineNode>();

            var startBay = startPort.Line.Bay;
            var endBay = endPort.Line.Bay;

            var outerPoints = TransitControl.Instance.BayOuterPoints[startBay];
            var innerPoints = TransitControl.Instance.BayInnerPoints[endBay];

            double minLength = float.MaxValue;
                 
            var outerPathTable = TransitControl.Instance.OuterPathTable;
            var transitPairTable = TransitControl.Instance.TransitPairTable;
            var innerPathTable = TransitControl.Instance.InnerPathTable;

            foreach (var outerPoint in outerPoints)
            {
                foreach(var innerPoint in innerPoints)
                {
                    var outerPath = outerPathTable.FindPath(startPort, outerPoint);
                    var transitPair = transitPairTable.FindPath(outerPoint, innerPoint);
                    var innerPath = innerPathTable.FindPath(innerPoint, endPort);

                    var length = outerPath.Length + transitPair.Length + innerPath.Length;

                    if(length <= minLength)
                    {
                        minLength = length;

                        var lines = new List<RailLineNode>();

                        lines.AddRange(outerPath.Lines);
                        lines.AddRange(transitPair.Lines);
                        lines.AddRange(innerPath.Lines);

                        shortestLines = lines;
                    }
                }
            }

            return shortestLines;
        }
    }
}
