using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class Path
    {
        public List<RailLineNode> Lines;
        public double Length;

        public Path(List<RailLineNode> lines, double length)
        {
            Lines = lines;
            Length = length;
        }
    }
}
