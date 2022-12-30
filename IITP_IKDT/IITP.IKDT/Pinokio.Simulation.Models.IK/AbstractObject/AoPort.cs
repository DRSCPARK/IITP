using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Map;
using Pinokio.Core;

namespace Pinokio.Simulation.Models.IK
{
    public enum PortType
    {
        EQPPort,
        Commit,
        Complete,
    }
    public class AoPort : AoResource
    {
        private PortType _type;
        public PortType PortType { get => _type; }
        public AoPort(uint mapId, string name, PortType portType) : base(mapId, 0, name, ResourceType.Port)
        {
            _type = portType;
        }
    }
}
