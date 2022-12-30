using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Core;
using Pinokio.Map;

namespace Pinokio.Simulation.Models.IK
{
    public class AoToolGroup : Core.AbstractObject
    {
        private string _factoryName;
        private FactoryToolType _factoryToolType;

        public string FactoryName { get => _factoryName; }
        public FactoryToolType ToolType { get => _factoryToolType; }

        public AoToolGroup(string factoryName, uint id, string name, FactoryToolType toolType) : base(id, name)
        {
            _factoryName = factoryName;
            _factoryToolType = toolType;
        }
    }
}
