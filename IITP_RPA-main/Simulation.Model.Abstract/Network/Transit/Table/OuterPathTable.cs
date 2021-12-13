using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class OuterPathTable
    {
        private int _startPortIndex;
        private int _outerPointIndex;

        private Dictionary<RailPortNode, int> _startPortIndices;
        private Dictionary<RailPointNode, int> _outerPointIndices;

        private Path[,] _table;

        public OuterPathTable(List<RailPortNode> startPorts, List<RailPointNode> outerPoints)
        {
            _startPortIndices = new Dictionary<RailPortNode, int>();
            _outerPointIndices = new Dictionary<RailPointNode, int>();

            foreach (var startPort in startPorts)
            { _startPortIndices[startPort] = _startPortIndex++; }

            foreach (var outerPoint in outerPoints)
            { _outerPointIndices[outerPoint] = _outerPointIndex++; }

            _table = new Path[_startPortIndex, _outerPointIndex];
        }

        public void AssignPath(RailPortNode startPort, RailPointNode outerPoint)
        {
            var startPortIndex = _startPortIndices[startPort];
            var outerPointIndex = _outerPointIndices[outerPoint];

            var outerPointLine = outerPoint.FromLines[0];

            var lines = PathFinder.Instance.DijkstraPath(startPort.Line, startPort.Distance,
                outerPointLine, outerPointLine.Length);

            var length = lines.Sum(line => line.Length);

            length -= startPort.Distance;

            var path = new Path(lines, length);

            _table[startPortIndex, outerPointIndex] = path;
        }

        public Path FindPath(RailPortNode startPort, RailPointNode outerPoint)
        {
            Path path = null;

            var startPortIndex = _startPortIndices[startPort];
            var outerPointIndex = _outerPointIndices[outerPoint];

            path = _table[startPortIndex, outerPointIndex];

            return path;
        }
    }
}
