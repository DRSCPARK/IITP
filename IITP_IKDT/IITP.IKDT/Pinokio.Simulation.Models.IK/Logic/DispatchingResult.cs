using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public class DispatchingResult
    {
        private Dictionary<string, List<string>> _reservedCoils; // Key: EQP Name / Value : Coil Names
        public Dictionary<string, List<string>> ReservedCoils { get => _reservedCoils; }
        public DispatchingResult(List<string> eqpNames)
        {
            _reservedCoils = new Dictionary<string, List<string>>();
            eqpNames.ForEach(name => _reservedCoils.Add(name, new List<string>()));
        }

        public DispatchingResult(DispatchingResult otherResult)
        {
            _reservedCoils = new Dictionary<string, List<string>>(otherResult._reservedCoils);
        }

        public List<string> this[string eqpName]
        {
            get => ReservedCoils[eqpName];
            set => ReservedCoils[eqpName] = value;
        }

        public void RemoveReservedCoilNames(Coil coil)
        {
            _reservedCoils.Remove(coil.Name);
        }

        public List<string> GetReservedCoilNames()
        {
            var names = new List<string>();
            foreach (var coilNames in _reservedCoils.Values)
            {
                names.AddRange(coilNames);
            }
            return names;
        }

        public string GetReservingEQPName(string coilName)
        {
            foreach (var eqpName in _reservedCoils.Keys)
            {
                var reservedCoils = _reservedCoils[eqpName];
                if (reservedCoils.Contains(coilName))
                {
                    return eqpName;
                }
            }
            return "";
        }
    }
}
