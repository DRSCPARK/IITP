using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class TransitPairTable
    {
        private int _outerPointIndex;
        private int _innerPointIndex;

        private Dictionary<RailPointNode, int> _outerPointIndices;
        private Dictionary<RailPointNode, int> _innerPointIndices;

        private Path[,] _table; 

        public TransitPairTable(List<RailPointNode> outerPoints, List<RailPointNode> innerPoints)
        {
            _outerPointIndices = new Dictionary<RailPointNode, int>();
            _innerPointIndices = new Dictionary<RailPointNode, int>();

            foreach(var outerPoint in outerPoints)
            { _outerPointIndices[outerPoint] = _outerPointIndex++; }

            foreach (var innerPoint in innerPoints)
            { _innerPointIndices[innerPoint] = _innerPointIndex++; }

            _table = new Path[_outerPointIndex, _innerPointIndex];
        }

        public void AssignPath(RailPointNode outerPoint, RailPointNode innerPoint)
        {
            var outerPointIndex = _outerPointIndices[outerPoint];
            var innerPointIndex = _innerPointIndices[innerPoint];

            var outerPointLine = outerPoint.ToLines[1];
            var innerPointLine = innerPoint.FromLines[1];

            var lines = PathFinder.Instance.DijkstraPath(outerPointLine, 0, innerPointLine, innerPointLine.Length);

            var length = lines.Sum(line => line.Length);

            var path = new Path(lines, length);

            _table[outerPointIndex, innerPointIndex] = path;
        }

        public Path FindPath(RailPointNode outerPoint, RailPointNode innerPoint)
        {
            Path path = null;

            var outerPointIndex = _outerPointIndices[outerPoint];
            var innerPointIndex = _innerPointIndices[innerPoint];

            path = _table[outerPointIndex, innerPointIndex];

            return path;
        }
    }
}
