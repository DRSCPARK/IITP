using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public class CoilBatch
    {
        private List<Coil> _coils;
        private string _coilKindName;
        private SimTime _startTime;
        private SimTime _dueDate;

        public List<Coil> Coils { get => _coils; }
        public string CoilKindName { get => _coilKindName; }
        public SimTime StartTime { get => _startTime; }
        public SimTime DueDate { get => _dueDate; }

        public CoilBatch(Coil coil)
        {
            _coilKindName = coil.CoilKind.Name;
            _startTime = coil.ReleasedSimTime;
            _dueDate = coil.DueSimTime;
            _coils = new List<Coil>();
            _coils.Add(coil);
        }
    }
}
