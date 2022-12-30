using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public class AoCommit : AoFactoryEquipment
    {
        private Dictionary<string, CoilKind> _coilKinds;
        private Dictionary<string, List<Plan>> _plans;

        public Dictionary<string, CoilKind> CoilKinds { get => _coilKinds; }
        public Dictionary<string, List<Plan>> Plans { get => _plans; }
        public AoCommit(uint mapId, uint id, string name) : base(mapId, id, name, Map.ResourceType.Commit)
        {
            _coilKinds = new Dictionary<string, CoilKind>();
            _plans = new Dictionary<string, List<Plan>>();
        }

        public void AddCoilKind(CoilKind coilKind)
        {
            if (!_coilKinds.ContainsKey(coilKind.Name))
            {
                _coilKinds.Add(coilKind.Name, coilKind);
                _plans.Add(coilKind.Name, new List<Plan>());
            }
        }

        public void AddPlan(string coilTypeName, Plan plan)
        {
            _plans[coilTypeName].Add(plan);
        }
    }
}
