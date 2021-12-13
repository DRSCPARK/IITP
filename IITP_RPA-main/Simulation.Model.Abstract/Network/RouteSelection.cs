using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class RouteSelection
    {
        private Fab _fab;
        private Bay _fromBay;
        private Bay _toBay;

        private RailLineNode _outLine;
        private RailLineNode _inLine;
        private double _minPriority;
        private double _maxPriority;
        private Dictionary<string, RailPointNode> _viaPoints;
        
        [Browsable(false)]
        public Fab Fab 
        { 
            get { return _fab; }
            set { _fab = value; }
        }
        [Browsable(false)]
        public Bay FromBay
        {
            get { return _fromBay; }
            set { _fromBay = value; }
        }
        [Browsable(false)]
        public Bay ToBay
        {
            get { return _toBay; }
            set { _toBay = value; }
        }

        [Browsable(false)]
        public RailLineNode OutLine 
        { 
            get { return _outLine; }
            set { _outLine = value; }
        }
        [Browsable(false)]
        public RailLineNode InLine
        {
            get { return _inLine; }
            set { _inLine = value; }
        }
        [Browsable(false)]
        public Dictionary<string, RailPointNode> ViaPoints
        {
            get { return _viaPoints; }
            set { _viaPoints = value; }
        }

        [Browsable(true)]
        public string FabName { get { return _fab.Name; } }
        [Browsable(true)]
        public string FromBayName 
        { 
            get 
            {
                if (_fromBay == null)
                    return string.Empty;
                else
                    return _fromBay.Name; 
            } 
        }
        [Browsable(true)]
        public string ToBayName 
        { 
            get 
            {
                if (_toBay == null)
                    return string.Empty;
                else                    
                    return _toBay.Name; 
            } 
        }
        [Browsable(true)]
        public string OutLineName { get { return _outLine.Name; } }
        [Browsable(true)]
        public string InLineName { get { return _inLine.Name; } }

        public double MinPriority
        {
            get { return _minPriority; }
            set { _minPriority = value; }
        }

        public double MaxPriority 
        { 
            get { return _maxPriority; }
            set { _maxPriority = value; }
        }

        public RouteSelection()
        {
            _fab = null;
            _fromBay = null;
            _toBay = null;

            _outLine = null;
            _inLine = null;

            _minPriority = 1;
            _maxPriority = 99;

            _viaPoints = new Dictionary<string, RailPointNode>();

        }

        public RouteSelection(Fab fab, Bay from, Bay to, RailLineNode outLine, RailLineNode inLine, double minPriority, double maxPriority, Dictionary<string, RailPointNode> viaPoints)
        {
            _fab = fab;
            _fromBay = from;
            _toBay = to;

            _outLine = outLine;
            _inLine = inLine;

            _minPriority = minPriority;
            _maxPriority = maxPriority;

            _viaPoints = new Dictionary<string, RailPointNode>();
            
            if(viaPoints.Count > 0)
            {
                foreach(RailPointNode point in viaPoints.Values)
                {
                    _viaPoints.Add(point.Name, point);
                }
            }
        }

        public void AddWayPoint(string pntName)
        {
            if (_viaPoints.ContainsKey(pntName) == false)
            {
                RailPointNode pntNode;
                ModelManager.Instance.DicRailPoint.TryGetValue(pntName, out pntNode);
                _viaPoints.Add(pntName, pntNode);
            }
        }

        public string GetWayPointStrings()
        {
            string viaPointString = string.Empty;

            foreach(RailPointNode rp in _viaPoints.Values)
            {
                viaPointString = viaPointString + rp.ID + ModelManager.Instance.Separator;
            }
            return viaPointString;
        }
    }
}
