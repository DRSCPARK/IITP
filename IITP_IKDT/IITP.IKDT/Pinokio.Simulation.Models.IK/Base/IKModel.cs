using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Core;
using Pinokio.Map;
using Pinokio.Simulation.Disc;

namespace Pinokio.Simulation.Models.IK
{
    public class IKModel : DiscSimModel
    {
        private Location _location;
        public Location Location { get => _location; }

        public IKModel(uint id, string name, Enum type) : base(id, name, type)
        {

        }
        public override void SetAbstractObj(AbstractObject aObj)
        {
            base.SetAbstractObj(aObj);

            if (aObj is AoResource resource)
            {
                this.SetPosition(resource.Position);
            }
        }

        public virtual void SetInitLocaiton(Location location)
        {
            _location = location;
        }
    }
}
