using Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class ZCUReservation
    {
        private Time _reservationTime;
        private OHTNode _oht;
        private ZCU _zcu;
        private List<RailLineNode> _zcuRoute;
        private RailPointNode _stopPoint;
        private RailPointNode _resetPoint;

        public Time ReservationTime
        {
            get { return _reservationTime; }
            set { _reservationTime = value; }
        }

        public OHTNode OHT
        {
            get { return _oht; }
        }

        public ZCU Zcu
        {
            get { return _zcu; }
        }

        public List<RailLineNode> ZcuRoute
        {
            get { return _zcuRoute; }
        }

        public RailPointNode StopPoint
        {
            get { return _stopPoint; }
            set { _stopPoint = value; }
        }

        public RailPointNode ResetPoint
        {
            get { return _resetPoint; }
            set { _resetPoint = value; }
        }


        public ZCUReservation(Time reservationTime, OHTNode oht, RailPointNode stopPoint, RailPointNode resetPoint)
        {
            _reservationTime = reservationTime;
            _oht = oht;
            _stopPoint = stopPoint;
            _resetPoint = resetPoint;
            _zcu = _stopPoint.Zcu;

//            InitializeRoute();
        }

        private void InitializeRoute()
        {
            _zcuRoute = new List<RailLineNode>();

            if (_oht.LstRailLine.Count > 1)
            {
                for (int i = 1; i < _oht.LstRailLine.Count; i++)
                {
                    RailLineNode line = _oht.LstRailLine[i];
                    if (_zcu.Lines.Contains(line))
                    {
                        _zcuRoute.Add(line);
                    }
                    else
                    {
                        _resetPoint = line.FromNode;
                        break;
                    }
                }
            }
        }
    }
}
