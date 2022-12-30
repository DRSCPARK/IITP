using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Core;

namespace Pinokio.Simulation.Models.IK
{
    public class Coil : SimEntity
    {
        #region Variables
        private SimTime _dueSimTime;
        private SimTime _releasedSimTime;
        private SimModel _currentPort;
        private double _remainingProcessingTime;
        private SimTime _enqueueTime;
        private double _valueAddedTime;
        #endregion Variables

        #region Properties
        public CoilKind CoilKind { get => (CoilKind)this.AbstractObj; }
        public SimTime DueSimTime { get => _dueSimTime; }
        public SimTime ReleasedSimTime { get => _releasedSimTime; }
        public SimModel CurrentPort { get => _currentPort; }
        public double RemainingProcessingTime { get => _remainingProcessingTime; set => _remainingProcessingTime = value; }
        public double ValueAddedTime { get => _valueAddedTime; set => _valueAddedTime = value; }
        public SimTime EnPortTime { get => _enqueueTime; set => _enqueueTime = value; }
        #endregion Properties

        #region Constructor
        public Coil(uint id, string name, SimTime releasedSimTime, SimTime dueSimTime) : base(id, name)
        {
            _dueSimTime = dueSimTime;
            _releasedSimTime = releasedSimTime;
            _valueAddedTime = 0;
        }
        #endregion Constructor

        #region Methods
        public override void SetAbstractObj(AbstractObject aObj)
        {
            base.SetAbstractObj(aObj);
        }

        public void SetCurrentPort(SimModel currentPort)
        {
            _currentPort = currentPort;
        }
        public double GetProcessingTime()
        {
            return this.CoilKind.ProcessingTime.GetNumber();
        }

        public double GetRemainingDueDate(SimTime timeNow)
        {
            return (_dueSimTime - timeNow).TotalSeconds;
        }
        #endregion Methods

        #region Observation
        public CoilObsn GetObservation(SimTime timenow)
        {
            return new CoilObsn()
            {
                Name = this.Name,
                State = this.State.ToString(),
                RemainingDueDate = this.GetRemainingDueDate(timenow),
                SetUp = this.CoilKind.Setup,
                EnPortTime = this.EnPortTime.TotalSeconds,
            };
        }
        #endregion Observation
    }
}
