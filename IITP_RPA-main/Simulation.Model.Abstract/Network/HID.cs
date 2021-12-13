using Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class HID : Zone
    {
        private int _maxCount;

        public bool IsFull
        {
            get
            {
                if (TotalOHTs.Count == _maxCount)
                    return true;
                else
                    return false;
            }
        }


        public bool IsBusy
        {
            get
            {
                if (TotalOHTs.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        public HID(uint id, string name, Fab fab, int maxCount)
        :base(id, fab)
        {
            ZONE_TYPE = ZONE_TYPE.HID;
            Name = name;
            _maxCount = maxCount;
        }

        //public SimPort SelectReservationPort(Time simTime, out RailPointNode earestPoint)
        //{
        //    SimPort earestPort = null; earestPoint = null;
        //    foreach (RailLineNode line in InLines.Values)
        //    {
        //        line.re
        //        OHTNode oht = port.OHTNode as OHTNode;
        //        if (line.FromNode.CheckEnter(simTime, oht))
        //        {
        //            if (earestPort == null)
        //            {
        //                earestPort = port;
        //                earestPoint = line.FromNode;
        //            }
        //            else if (earestPort.Time > port.Time)
        //            {
        //                earestPort = port;
        //                earestPoint = line.FromNode;
        //            }
        //        }
        //    }
        //}

        //    if(earestPort != null)
        //    {
        //    }
        //    Return earestPort;
        //}
    }
}
