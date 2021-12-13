using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class Fab
    {
        private string _name;
        private Dictionary<string, RailPointNode> _dicRailPoint;
        private Dictionary<string, RailLineNode> _dicRailLine;
        private List<OHTNode> _lstOHT;
        private Dictionary<string, RailPortNode> _dicRailPort;
        private Dictionary<string, RailPortNode> _dicNormalPort;
        private Dictionary<string, RailPortNode> _dicReticlePort;
        private Dictionary<string, Bay> _lstBay;
        private Dictionary<string, ProcessEqpNode> _dicProcessEqp;
        private List<BufferNode> _lstBuffer;
        private Dictionary<string, FabSimNode> _dicEqp;
        private ReticleZone _reticleZone;

        public string Name
        { 
            get { return _name; }  
            set { _name = value; }
        }

        public Dictionary<string, RailPointNode> DicRailPoint
        {
            get 
            {
                return _dicRailPoint;
            }
        }

        public Dictionary<string, RailLineNode> DicRailLine
        {
            get
            {
                return _dicRailLine;
            }
        }

        public void GetDicRailLine()
        {
            IEnumerable<KeyValuePair<string, RailLineNode>> lines = ModelManager.Instance.DicRailLine.Where(p => p.Value.Fab.Name == _name);

            _dicRailLine = lines.ToDictionary(p => p.Key, p => p.Value);
        }
        public List<OHTNode> LstOHT
        {
            get
            {
                return _lstOHT;
            }
        }

        public void GetLstOHT()
        {
            IEnumerable<OHTNode> ohts = ModelManager.Instance.LstOHTNode.Where(oht => oht.Fab.Name == _name);

            _lstOHT = ohts.ToList();
        }

        public List<OHTNode> IdleOHTs
        {
            get
            {
                IEnumerable<OHTNode> OHTs = _lstOHT.Where(oht => oht.NodeState is OHT_STATE.IDLE && oht.Fab.Name == _name);

                return OHTs.ToList();
            }
        }

        public List<OHTNode> ReadyOHTs
        {
            get
            {
                IEnumerable<OHTNode> OHTs = ModelManager.Instance.ReadyOHTs.Where(oht=>oht.Fab.Name == _name);

                return OHTs.ToList();
            }
        }

        public List<OHTNode> WaitingOHTs
        {
            get
            {
                IEnumerable<OHTNode> OHTs = _lstOHT.Where(oht => (oht.NodeState is OHT_STATE.MOVE_TO_LOAD || oht.NodeState is OHT_STATE.LOADING) && oht.Fab.Name == _name);

                return OHTs.ToList();
            }
        }

        public List<OHTNode> TransferringOHTs
        {
            get
            {
                IEnumerable<OHTNode> OHTs = _lstOHT.Where(oht => !(oht.NodeState is OHT_STATE.MOVE_TO_LOAD && oht.Fab.Name == _name)
                        && !(oht.NodeState is OHT_STATE.LOADING)
                        && !(oht.NodeState is OHT_STATE.IDLE));

                return OHTs.ToList();
            }
        }

        public Dictionary<string, RailPortNode> DicRailPort
        {
            get 
            {
                return _dicRailPort;
            }
        }

        public void GetDicRailPort()
        {
            IEnumerable<KeyValuePair<string, RailPortNode>> ports = ModelManager.Instance.DicRailPort.Where(p => p.Value.Fab.Name == _name);

            _dicRailPort = ports.ToDictionary(p => p.Key, p => p.Value);
        }

        public Dictionary<string, RailPortNode> DicNormalPort
        {
            get
            {
                return _dicNormalPort;
            }
        }

        public void GetDicNormalPort()
        {
            IEnumerable<KeyValuePair<string, RailPortNode>> ports = ModelManager.Instance.DicNormalPort.Where(p => p.Value.Fab.Name == _name);

            _dicNormalPort = ports.ToDictionary(p => p.Key, p => p.Value);
        }

        public Dictionary<string, RailPortNode> DicReticlePort
        {
            get
            {
                return _dicReticlePort;
            }
        }

        public void GetDicReticlePort()
        {
            IEnumerable<KeyValuePair<string, RailPortNode>> ports = ModelManager.Instance.DicReticlePort.Where(p => p.Value.Fab.Name == _name);

            _dicReticlePort = ports.ToDictionary(p => p.Key, p => p.Value);
        }

        public Dictionary<string, Bay> LstBay
        {
            get
            {
                return _lstBay;
            }
        }

        public void GetDicBay()
        {
            IEnumerable<KeyValuePair<string, Bay>> bays = ModelManager.Instance.Bays.Where(pair => pair.Value.Fab.Name == _name);

            _lstBay = bays.ToDictionary(p => p.Key, p => p.Value);
        }

        public Dictionary<string, ProcessEqpNode> DicProcessEqp
        {
            get
            {
                return _dicProcessEqp;
            }
        }

        public void GetDicProcessEqp()
        {
            IEnumerable<KeyValuePair<string, ProcessEqpNode>> eqps = ModelManager.Instance.DicProcessEqpNode.Where(p => p.Value.Fab.Name == _name);

            _dicProcessEqp = eqps.ToDictionary(p => p.Key, p => p.Value);
        }


        public List<BufferNode> LstBuffer
        {
            get
            {
                return _lstBuffer;
            }
        }

        public void GetLstBuffer()
        {
            IEnumerable<BufferNode> buffers = ModelManager.Instance.LstBufferNode.Where(bf => bf.Fab.Name == _name);

            _lstBuffer = buffers.ToList();
        }

        public Dictionary<string, FabSimNode> DicEqp
        {
            get 
            {
                return _dicEqp;
            }
        }

        public void GetDicEqp()
        {
            IEnumerable<KeyValuePair<string, FabSimNode>> foupPorts = ModelManager.Instance.DicEqp.Where(p => p.Value.Fab.Name == _name);

            _dicEqp =  foupPorts.ToDictionary(p => p.Key, p => p.Value);
        }

        public ReticleZone ReticleZone
        {
            get { return _reticleZone; }
            set { _reticleZone = value; }
        }

        public List<RouteSelection> RouteSelections
        {
            get { return ModelManager.Instance.RouteSelections[Name]; }
        }

        public List<RailLineNode> RailCuts
        {
            get { return ModelManager.Instance.RailCuts[Name]; }
        }


        public Fab(string name)
        {
            _name = name;
            _dicEqp = new Dictionary<string, FabSimNode>();
            _dicProcessEqp = new Dictionary<string, ProcessEqpNode>();
            _dicRailLine = new Dictionary<string, RailLineNode>();
            _dicRailPoint = new Dictionary<string, RailPointNode>();
            _dicRailPort = new Dictionary<string, RailPortNode>();
            _dicNormalPort = new Dictionary<string, RailPortNode>();
            _dicReticlePort = new Dictionary<string, RailPortNode>();
        }

        public void GetFabModels()
        {
            GetDicRailLine();
            GetDicRailPoint();
            GetDicRailPort();
            GetDicNormalPort();
            GetDicReticlePort();
            GetLstOHT();
            GetDicBay();
            GetDicProcessEqp();
            GetLstBuffer();
            GetDicProcessEqp();
        }

        public void GetListBay()
        {
            IEnumerable<KeyValuePair<string, Bay>> bays = ModelManager.Instance.Bays.Where(pair => pair.Value.Fab.Name == _name);
            _lstBay = bays.ToDictionary(p => p.Key, p => p.Value);
        }

        public void GetListProcessEqp()
        {
            IEnumerable<KeyValuePair<string, ProcessEqpNode>> eqps = ModelManager.Instance.DicProcessEqpNode.Where(pair => pair.Value.Fab.Name == _name);
            _dicProcessEqp = eqps.ToDictionary(p => p.Key, p => p.Value);
        }

        public void GetDicRailPoint()
        {
            IEnumerable<KeyValuePair<string, RailPointNode>> points = ModelManager.Instance.DicRailPoint.Where(pair => pair.Value.Fab.Name == _name);

            _dicRailPoint = points.ToDictionary(p => p.Key, p => p.Value);

        }

        public RailLineNode GetRailLine(string fromPointID, string toPointID)
        {
            string lineName = Name + "_line_" + fromPointID + "_" + toPointID;
            string reverseLineName = Name + "_line_" + toPointID + "_" + fromPointID;
            foreach (RailLineNode line in DicRailLine.Values)
            {
                if (line.Name == lineName)
                    return line;
                else if(line.Name == reverseLineName)
                    return line;
            }

            foreach (RailLineNode line in DicRailLine.Values)
            {
                if (line.Name.Contains(lineName))
                    return line;
            }

            return null;
        }
    }
}
