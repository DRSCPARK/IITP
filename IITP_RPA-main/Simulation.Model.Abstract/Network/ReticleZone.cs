using Simulation.Engine;
using Simulation.Geometry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class ReticleZone : Zone
    {
        #region Variables
        protected List<Bay> bays;

        public List<Bay> Bays
        {
            get { return bays; }
            set { bays = value; }
        }

        #endregion

        public ReticleZone(uint id, Fab fab)
        :base(id, fab)
        {
            ZONE_TYPE = ZONE_TYPE.RETICLE;
            Name = RETICLE_ZONE_NAME.RETICLE_ZONE.ToString();
            bays = new List<Bay>();
        }
    }
}
