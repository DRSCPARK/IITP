using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Core;

namespace Pinokio.Simulation.Models.IK
{
    public class AoComplete : AoFactoryEquipment
    {
        private Dictionary<string, CoilKind> _coilKinds;
        public Dictionary<string, CoilKind> CoilKinds { get => _coilKinds; }
        public AoComplete(uint mapid, uint id, string name) : base(mapid, id, name, Map.ResourceType.Complete)
        {
            _coilKinds = new Dictionary<string, CoilKind>();
        }

        public void AddCoilKind(CoilKind coilType)
        {
            if (!_coilKinds.ContainsKey(coilType.Name))
            {
                _coilKinds.Add(coilType.Name, coilType);
            }
        }
    }
}
