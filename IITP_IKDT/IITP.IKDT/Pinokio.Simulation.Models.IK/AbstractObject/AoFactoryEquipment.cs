using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Map;

namespace Pinokio.Simulation.Models.IK
{
    public class AoFactoryEquipment : AoResource
    {
        private string _toolGroupName;

        public string ToolGroupName { get => _toolGroupName; }

        public AoFactoryEquipment(uint mapId, uint id, string name, ResourceType resourceType = ResourceType.Process) : base(mapId, id, name, resourceType)
        {

        }

        public void SetToolGroupName(string toolGroupName)
        {
            _toolGroupName = toolGroupName;
        }

    }
}
