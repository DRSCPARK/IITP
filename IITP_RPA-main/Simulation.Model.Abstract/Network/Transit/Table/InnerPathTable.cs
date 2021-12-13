using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class InnerPathTable
    {
        private int _innerPointIndex;
        private int _endPortIndex;

        private Dictionary<RailPointNode, int> _innerPointIndices;
        private Dictionary<RailPortNode, int> _endPortIndices;

        private Path[,] _table;

        public InnerPathTable(List<RailPointNode> innerPoints, List<RailPortNode> endPorts)
        {
            _innerPointIndices = new Dictionary<RailPointNode, int>();
            _endPortIndices = new Dictionary<RailPortNode, int>();

            foreach (var innerPoint in innerPoints)
            { _innerPointIndices[innerPoint] = _innerPointIndex++; }

            foreach (var endPort in endPorts)
            { _endPortIndices[endPort] = _endPortIndex++; }

            _table = new Path[_innerPointIndex, _endPortIndex];
        }

        public void AssignPath(RailPointNode innerPoint, RailPortNode endPort)
        {
            var innerPointIndex = _innerPointIndices[innerPoint];
            var endPointIndex = _endPortIndices[endPort];

            var innerPointLine = innerPoint.ToLines[0];

            var lines = PathFinder.Instance.DijkstraPath(innerPointLine, 0, endPort.Line, endPort.Distance);

            var length = lines.Sum(line => line.Length);

            length -= (endPort.Line.Length - endPort.Distance);

            var path = new Path(lines, length);

            _table[innerPointIndex, endPointIndex] = path;
        }

        public Path FindPath(RailPointNode innerPoint, RailPortNode endPort)
        {
            Path path = null;

            var innerPointIndex = _innerPointIndices[innerPoint];
            var endPointIndex = _endPortIndices[endPort];

            path = _table[innerPointIndex, endPointIndex];

            return path;
        }
    }
}
