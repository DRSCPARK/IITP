using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Core;

namespace Pinokio.Simulation.Models.IK
{
    public class AoWorker : Core.AbstractObject
    {
        #region Variables
        const double _weekMinutes = 10080;
        private double _workingTimeLimit;
        private string _workingUnit;
        private double _breakTime;
        private string _factoryName;
        private JobType _jobType;
        #endregion Variables End

        #region Properties 
        public double WorkingTimeLimit { get => _workingTimeLimit; }
        public string WorkingUnit { get => _workingUnit; }
        public double BreakTime { get => _breakTime; }
        public string FactoryName { get => _factoryName; }
        public JobType JobType { get => _jobType; }

        #endregion Properties End
        public AoWorker(string factoryName, double workingTime, string workingUnit, JobType jobType, uint id, string name) : base(id, name)
        {
            _factoryName = factoryName;
            _workingTimeLimit = workingTime * 60;
            _workingUnit = workingUnit;
            _breakTime = (_weekMinutes - workingTime) * 60;
            _jobType = jobType;
        }
    }
}
