using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Simulation.Engine;

namespace Simulation.Model.Abstract
{
    public class Step : SimObj
    {
        #region Member
        protected Time _tactTime;
        protected Time _processTime;
        protected uint _jobID;

        public Time TactTime
        {
            get { return _tactTime; }
            set { _tactTime = value; }
        }
        public Time ProcessTime
        {
            get { return _processTime; }
            set { _processTime = value; }
        }
        public uint JobID
        {
            get { return _jobID; }
            set { _jobID = value; }
        }

        #endregion

        public Step(uint ID, string name, Time tactTime)
            : base(ID, name, OBJ_TYPE.NOT_DEFINED)
        {
            _tactTime = tactTime;
            _jobID = 0;
        }

        public Step(uint ID, string name, Time tactTime, uint jobID)
            : base(ID, name, OBJ_TYPE.NOT_DEFINED)
        {
            _tactTime = tactTime;
            _jobID = jobID;
        }
    }
}
