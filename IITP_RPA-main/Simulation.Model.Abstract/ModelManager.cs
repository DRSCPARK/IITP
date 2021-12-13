using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Simulation.Engine;
using Simulation.Geometry;
using CSScriptLibrary;

namespace Simulation.Model.Abstract
{
    /// <summary>
    /// Simulation Model 관련 Manager
    /// </summary>
    public class ModelManager
    {
        #region Variable
        static ModelManager _modelMng;
        static AstartCal _astarFinder;
        private Dictionary<string, Fab> _fabs;
        private SIMULATION_TYPE _simType;

        /// <summary>
        /// 공정 정보를 가지고 있는 제품 정보
        /// </summary>
        private Dictionary<string, Foup> _products;
        /// <summary>
        /// 제품 정보를 가지고 만든 객체
        /// </summary>
        private Dictionary<string, Foup> _foups;
        private Dictionary<uint, Step> _steps;
        private Dictionary<uint, SimNode> _simNodes;
        private Dictionary<string, RailPointNode> _railPointNodes;
        private Dictionary<string, RailLineNode> _railLineNodes;
        private List<OHTNode> _ohtNodes;
        private List<OHTNode> _lstReadyOHT;
        private Dictionary<uint, ProcessEqpNode> _processTypes;
        private Dictionary<string, ProcessEqpNode> _processEqps;
        private List<BufferNode> _bufferNodes;
        private Dictionary<string, Bay> _bays;
        private Dictionary<string, ReticleZone> _reticleZones; // FAb마다 1개라서 fabName이 Key
        private Dictionary<uint, SimLink> _simLinks;
        private Dictionary<uint, SimLink> _evtSimLinks;
        private List<NetworkPointNode> _networkPoints;
        private List<NetworkLineNode> _networkLines;
        private Dictionary<string, RailPortNode> _portNodes;
        private Dictionary<string, RailPortNode> _normalPorts;
        private Dictionary<string, RailPortNode> _reticlePorts;
        private Dictionary<string, ZCU> _zcus;
        private Dictionary<string, HID> _hids;
        private Dictionary<string, List<RouteSelection>> _routeSelections; // fabName이 Key
        private Dictionary<string, List<RailLineNode>> _railCuts; //fabName이 Key
        private uint _entityID;
        private uint _NodeID;
        private uint _linkID;
        private uint _zoneID;
        private uint _evtLinkID;
        private uint _networkPointID;
        private uint _networkLineID;
        private CommitInterval _commit;
        private CompleteNode _complete;
        private CommitEqpNode _commitEqp;
        private CompleteEqpNode _completeEqp;
        private uint _processGenID;
        private List<SimPort> _commands;
        private Dictionary<string, FabSimNode> _eqpNodes;
        private CommanderNode _commander;
        public Stopwatch wakeUpStopwatch;
        public Stopwatch updatePosStopwatch;
        public string simLogPath;
        private double _ohtSize;
        private double _ohtMinimumDistance;
        private double _reroutingIndicator;
        private Random random;
        public dynamic ScriptScoreFunction;
        public string DynamicRoutingScript;
        public char Separator = '/';
        private double _acceleration;
        private double _deceleration;
        public Dictionary<uint, uint> _dicNWPointRouteGraph;
        private float[,] _simpleNetwork; //woong
        private Dictionary<int, List<int>> _dicShortestPath;
        private Dictionary<int, string> _dicShortestPathString;
        private int[,] _refNode;

        public SIMULATION_TYPE SimType
        {
            get { return _simType; }
            set { _simType = value; }
        }

        #region Spec

        public double OHTSize
        {
            get
            {
                return _ohtSize;
            }
            set
            {
                _ohtSize = value;
            }
        }

        public double OHTSpeed
        {
            get
            {
                return Properties.Settings.Default.OHTSpeed;
            }
            set
            {
                Properties.Settings.Default.OHTSpeed = value;
            }
        }
        public double OHTUnloadingTime
        {
            get
            {
                return Properties.Settings.Default.OHTUnloadingTime;
            }
            set
            {
                Properties.Settings.Default.OHTUnloadingTime = value;
            }
        }

        public double OHTLoadingTime
        {
            get
            {
                return Properties.Settings.Default.OHTLoadingTime;
            }
            set
            {
                Properties.Settings.Default.OHTLoadingTime = value;
            }
        }
        public double OHTMinimumDistance
        {
            get
            {
                return _ohtMinimumDistance;
            }
            set
            {
                _ohtMinimumDistance = value;
            }
        }

        public double ReroutingIndicator
        {
            get { return _reroutingIndicator; }
            set { _reroutingIndicator = value; }
        }
        public Vector2 PortSize
        {
            get
            {
                return new Vector2(Properties.Settings.Default.PortWidthLength, Properties.Settings.Default.PortWidthLength);
            }
            set
            {
                Properties.Settings.Default.PortWidthLength = value.X;
            }
        }

        #endregion

        public Dictionary<string, Fab> Fabs
        {
            get { return _fabs; }
        }

        public List<NetworkPointNode> NWPoints
        {
            get { return _networkPoints; }
        }
        public List<NetworkLineNode> lstNWLines
        {
            get { return _networkLines; }
        }
        public Dictionary<uint, SimNode> SimNodes
        {
            get { return _simNodes; }
        }
        /// <summary>
        /// 제품 종류 
        /// </summary>
        public Dictionary<string, Foup> Products
        { get { return _products; } }
        /// <summary>
        /// 라인에 돌아다니는 Foup
        /// </summary>
        public Dictionary<string, Foup> Foups
        { get { return _foups; } }
        public Dictionary<uint, Step> Steps
        {
            get { return _steps; }
        }
        public Dictionary<string, RailPointNode> DicRailPoint
        {
            get { return _railPointNodes; }
        }
        public Dictionary<string, RailLineNode> DicRailLine
        {
            get { return _railLineNodes; }
        }
        public Dictionary<string, Bay> Bays
        {
            get { return _bays; }
        }
        public List<RailPointNode> LstIntersectionRailPoint
        {
            get
            {
                List<RailPointNode> intersectionRailPoints = new List<RailPointNode>();

                foreach (RailPointNode rp in DicRailPoint.Values)
                {
                    if (rp.OutLinks.Count > 1)
                        intersectionRailPoints.Add(rp);
                }

                return intersectionRailPoints;
            }
        }
        public List<OHTNode> LstOHTNode
        {
            get { return _ohtNodes; }
        }

        public List<OHTNode> ReadyOHTs
        {
            get
            {
                return _lstReadyOHT;
            }
            set
            {
                _lstReadyOHT = value;
            }
        }

        public int TotalFabAOHTCount
        { get { return _ohtNodes.Where(oht => oht.Fab.Name == "M14A").Count(); } }

        public int TotalFabBOHTCount
        { get { return _ohtNodes.Where(oht => oht.Fab.Name == "M14B").Count(); } }

        public List<OHTNode> IdleOHTs
        {
            get
            {
                IEnumerable<OHTNode> OHTs = _ohtNodes.Where(oht => oht.NodeState is OHT_STATE.IDLE);

                return OHTs.ToList();
            }
        }

        public int IdleFabAOHTCount
        {
            get
            {
                return IdleOHTs.Where(oht =>  oht.Fab.Name == "M14A").Count();
            }
        }

        public int IdleFabBOHTCount
        {
            get
            {
                return IdleOHTs.Where(oht => oht.Fab.Name == "M14B").Count();
            }
        }

        public List<OHTNode> WaitingOHTs
        {
            get
            {
                IEnumerable<OHTNode> OHTs = _ohtNodes.Where(oht => oht.NodeState is OHT_STATE.MOVE_TO_LOAD || oht.NodeState is OHT_STATE.LOADING);

                return OHTs.ToList();
            }
        }

        public int WaitingFabAOHTCount
        {
            get
            {
                return WaitingOHTs.Where(oht => oht.Fab.Name == "M14A").Count();
            }
        }

        public int WaitingFabBOHTCount
        {
            get
            {
                return WaitingOHTs.Where(oht => oht.Fab.Name == "M14B").Count();
            }
        }


        public List<OHTNode> TransferringOHTs
        {
            get
            {
                IEnumerable<OHTNode> OHTs = _ohtNodes.Where(oht => !(oht.NodeState is OHT_STATE.MOVE_TO_LOAD)
                        && !(oht.NodeState is OHT_STATE.LOADING)
                        && !(oht.NodeState is OHT_STATE.IDLE));

                return OHTs.ToList();
            }
        }

        public int TransferringFabAOHTCount
        { get { return TransferringOHTs.Where(oht => oht.Fab.Name == "M14A").Count(); } }

        public int TransferringFabBOHTCount
        { get { return TransferringOHTs.Where(oht => oht.Fab.Name == "M14B").Count(); } }

        public int TotalEqpCount
        {
            get
            {
                int totalEqpCount = 0;

                totalEqpCount = DicProcessEqpNode.Count;

                return totalEqpCount;
            }
        }

        public int BusyEqpCount
        {
            get
            {
                int busyEqpCount = 0;

                List<ProcessEqpNode> busyEqps = new List<ProcessEqpNode>();

                busyEqps = _processEqps.Values.Where(eqp => eqp.IsProcessing == true).ToList();

                busyEqpCount = busyEqps.Count;

                return busyEqpCount;
            }
        }

        public int IdleEqpCount
        {
            get
            {
                int idleEqpCount = 0;

                List<ProcessEqpNode> idleEqps = new List<ProcessEqpNode>();

                idleEqps = _processEqps.Values.Where(eqp => eqp.IsProcessing == false).ToList();

                idleEqpCount = idleEqps.Count;

                return idleEqpCount;
            }
        }

        public int TotalProcessPortCount
        {
            get
            {
                return DicRailPort.Values.Where(port => (port is ProcessPortNode) == true).ToList().Count;
            }
        }

        public int TotalSTBCount
        {
            get
            {
                return DicRailPort.Values.Count - DicRailPort.Values.Where(port => (port is ProcessPortNode)).ToList().Count;
            }
        }

        public List<ProcessPortNode> LoadedProcessPorts
        {
            get
            {
                List<ProcessPortNode> loadedProcessPorts = new List<ProcessPortNode>();
                
                loadedProcessPorts = DicRailPort.Values.Where(port => port is ProcessPortNode).Where(processPort => (PROCESSPORT_STATE)processPort.NodeState == PROCESSPORT_STATE.FULL).Cast<ProcessPortNode>().ToList();

                return loadedProcessPorts;
            }
        }

        public List<ProcessPortNode> EmptyProcessPorts
        {
            get
            {
                List<ProcessPortNode> emptyProcessPorts = new List<ProcessPortNode>();

                emptyProcessPorts = DicRailPort.Values.Where(port => port is ProcessPortNode).Where(processPort => (PROCESSPORT_STATE)processPort.NodeState != PROCESSPORT_STATE.FULL).Cast<ProcessPortNode>().ToList();

                return emptyProcessPorts;
            }
        }

        public List<RailPortNode> LoadedSTBs
        {
            get
            {
                List<RailPortNode> loadedSTBs = new List<RailPortNode>();

                loadedSTBs = DicRailPort.Values.Where(port => (port is ProcessPortNode) is false).Where(stb => (RAILPORT_STATE)stb.NodeState == RAILPORT_STATE.FULL).ToList();

                return loadedSTBs;
            }
        }

        public List<RailPortNode> EmptySTBs
        {
            get
            {
                List<RailPortNode> emptySTBs = new List<RailPortNode>();

                emptySTBs = DicRailPort.Values.Where(port => (port is ProcessPortNode) is false).Where(stb => (RAILPORT_STATE)stb.NodeState != RAILPORT_STATE.FULL).ToList();

                return emptySTBs;
            }
        }

        public Dictionary<uint, ProcessEqpNode> DicProcessType
        { get { return _processTypes; } }
        public Dictionary<string, ProcessEqpNode> DicProcessEqpNode
        {
            get { return _processEqps; }
        }
        public List<BufferNode> LstBufferNode
        {
            get { return _bufferNodes; }
        }

        public Dictionary<string, RailPortNode> DicRailPort
        {
            get { return _portNodes; }
        }

        public Dictionary<string, RailPortNode> DicNormalPort
        {
            get { return _normalPorts; }
        }

        public Dictionary<string, RailPortNode> DicReticlePort
        {
            get { return _reticlePorts; }
        }
        public Dictionary<string, FabSimNode> DicEqp
        {
            get { return _eqpNodes; }
        }

        public Dictionary<string, ZCU> Zcus
        {
            get { return _zcus; }
        }

        public Dictionary<string, HID> Hids
        {
            get { return _hids; }
        }

        public CommitInterval CommitNode
        { get { return _commit; } }
        public CompleteNode CompleteNode
        { get { return _complete; } }

        public CommitEqpNode CommitEqpNode
        {
            get { return _commitEqp; }
        }

        public CompleteEqpNode CompleteEqpNode
        {
            get { return _completeEqp; }
        }

        public double BuffersLoadingRate
        {
            get
            {
                double loadingCount = 0;

                foreach (BufferNode buffer in _bufferNodes)
                {
                    if (buffer.LstEntity.Count > 0)
                        loadingCount++;
                }

                return loadingCount / _bufferNodes.Count;
            }
        }

        public CommanderNode Commander
        {
            get { return _commander; }
            set { _commander = value; }
        }

        public List<SimPort> Commands
        {
            get { return _commands; }
        }

        public Dictionary<string, List<RouteSelection>> RouteSelections  //Key: fabName
        {
            get { return _routeSelections; }
        }

        public Dictionary<string, List<RailLineNode>> RailCuts
        {
            get { return _railCuts; }
        }

        public float[,] SimpleNetwork //woong
        {
            get { return _simpleNetwork; }
        }

        public double OHTAcceleration
        {
            get { return _acceleration; }
            set { _acceleration = value; }
        }

        public double OHTDeceleration
        {
            get { return _deceleration; }
            set { _deceleration = value; }
        }
        #endregion

        static public ModelManager Instance
        {
            get { return _modelMng; }
        }

        public ModelManager()
        {
            _modelMng = this;
            _astarFinder = new AstartCal();
            _simType = SIMULATION_TYPE.MATERIAL_HANDLING;
            _ohtSize = Properties.Settings.Default.OHTWidthLength;
            _ohtMinimumDistance = Properties.Settings.Default.OHTMinimumDistance;

            GetDynamicRoutingLogic();
        }

        public void Initialize()
        {
            _fabs = new Dictionary<string, Fab>();
            _products = new Dictionary<string, Foup>();
            _foups = new Dictionary<string, Foup>();
            _steps = new Dictionary<uint, Step>();
            _simNodes = new Dictionary<uint, SimNode>();
            _railPointNodes = new Dictionary<string, RailPointNode>();
            _railLineNodes = new Dictionary<string, RailLineNode>();
            _ohtNodes = new List<OHTNode>();
            _lstReadyOHT = new List<OHTNode>();
            _processTypes = new Dictionary<uint, ProcessEqpNode>();
            _processEqps = new Dictionary<string, ProcessEqpNode>();
            _bufferNodes = new List<BufferNode>();
            _bays = new Dictionary<string, Bay>();
            _simLinks = new Dictionary<uint, SimLink>();
            _evtSimLinks = new Dictionary<uint, SimLink>();
            _networkPoints = new List<NetworkPointNode>();
            _networkLines = new List<NetworkLineNode>();
            _portNodes = new Dictionary<string, RailPortNode>();
            _normalPorts = new Dictionary<string, RailPortNode>();
            _reticlePorts = new Dictionary<string, RailPortNode>();
            _entityID = 0;
            _NodeID = 0;
            _linkID = 0;
            _zoneID = 0;
            _evtLinkID = 0;
            _networkPointID = 0;
            _networkLineID = 0;
            _commands = new List<SimPort>();
            _eqpNodes = new Dictionary<string, FabSimNode>();
            _zcus = new Dictionary<string, ZCU>();
            _hids = new Dictionary<string, HID>();
            wakeUpStopwatch = new Stopwatch();
            updatePosStopwatch = new Stopwatch();
            simLogPath = System.IO.Directory.GetCurrentDirectory() + "/SimulationLog_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + ".txt";
            _simType = SIMULATION_TYPE.MATERIAL_HANDLING;
            _reticleZones = new Dictionary<string, ReticleZone>();
            _routeSelections = new Dictionary<string, List<RouteSelection>>();
            _railCuts = new Dictionary<string, List<RailLineNode>>();
            _reroutingIndicator = 10;
            random = new Random();

            _acceleration = 1470;
            _deceleration = -2940;
        }

        public void GetDynamicRoutingLogic()
        {
            try
            {
                DynamicRoutingScript = @"
                using System;
                using System.Collections.Generic;
                using System.Diagnostics;
                using System.Linq;
                using System.Text;
                using System.Threading;
                using System.Threading.Tasks;
                using Simulation.Engine;
                using Simulation.Geometry;
                using Simulation.Model.Abstract;
                public void UpdateRailLine(RailLineNode line)
                {
                ////고정 값의 weight
                float distanceWeight = 1;

                //상태 변화 값의 weight
                float idleOHTCountWeight = 0;
                float workingOHTCountWeight = 0;

                line.WorkingOHTCount = line.ListOHT.FindAll(oht => !(oht.NodeState is OHT_STATE.IDLE)).Count;
                line.IdleOHTCount = line.ListOHT.FindAll(oht => oht.NodeState is OHT_STATE.IDLE).Count;

                line.TotalCost = (float)line.Length * distanceWeight
                                + line.IdleOHTCount * idleOHTCountWeight
                                + line.WorkingOHTCount * workingOHTCountWeight;

                    ////고정 값의 weight
                    //float distancePerVelocityWeight = 0;
                    //float portCountWeight = 0;
                    //float divergingPointCountWeight = 0;
                    //float joiningPointCountWeight = 0;

                    ////상태 변화 값의 weight
                    //float idleOHTCountWeight = 0;
                    //float workingOHTCountWeight = 0;
                    //float reservationPortCountWeight = 0;

                    //line.WorkingOHTCount = line.ListOHT.FindAll(oht => !(oht.NodeState is OHT_STATE.IDLE)).Count;
                    //line.IdleOHTCount = line.ListOHT.FindAll(oht => oht.NodeState is OHT_STATE.IDLE).Count;
                    //line.ReservationPortCount = line.DicRailPort.FindAll(port => port.IsReservation).Count;

                    //line.TotalCost = line.DistancePerVelocity * distancePerVelocityWeight
                    //                + line.PortCount * portCountWeight
                    //                + line.DivergingPointCount * divergingPointCountWeight
                    //                + line.JoiningPointCount * joiningPointCountWeight
                    //                + line.IdleOHTCount * idleOHTCountWeight
                    //                + line.WorkingOHTCount * workingOHTCountWeight
                    //                + line.ReservationPortCount * reservationPortCountWeight;
                }";

                ScriptScoreFunction = CSScript.Evaluator.CreateDelegate(DynamicRoutingScript);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void IdentifyFromToLines()
        {
            List<RailLineNode> lines = DicRailLine.Values.ToList();

            foreach(RailLineNode line in lines)
            {
                Bay lineBay = line.Bay;
                BAY_TYPE lineBayType = lineBay.BayType;

                RailPointNode fromNode = line.FromNode;
                RailPointNode toNode = line.ToNode;

                RailLineNode fromLine;
                RailLineNode toLine;

                switch (lineBayType)
                {
                    case BAY_TYPE.INTERBAY:

                        // InterBay의 FromLine인지 검사
                        // line의 FromNode의 FromLine의 Bay가 IntraBay일 경우 InterBay의 FromLine
                        fromLine = fromNode.FromLines.First();

                        if(fromLine.Bay.BayType == BAY_TYPE.INTRABAY)
                        { lineBay.FromLines.Add(line); }

                        // InterBay의 ToLine인지 검사
                        // line의 ToNode의 ToLine의 Bay가 IntraBay일 경우 InterBay의 ToLine
                        toLine = toNode.ToLines.First();

                        if (toLine.Bay.BayType == BAY_TYPE.INTRABAY)
                        { lineBay.ToLines.Add(line); }

                        break;
                    case BAY_TYPE.INTRABAY:

                        // IntraBay의 FromLine인지 검사
                        // 경우는 2가지. InterBay에서 들어오는 line이거나 옆 IntraBay에서 들어오는 line이거나

                        // Case 1. InterBay에서 들어오는 line
                        fromLine = fromNode.FromLines.First();

                        if (fromLine.Bay.BayType == BAY_TYPE.INTERBAY)
                        { lineBay.FromLines.Add(line); }

                        // Case 2. 옆 IntraBay에서 들어오는 line
                        if (line.Name.Contains("leftOut_3") || line.Name.Contains("rightOut_3"))
                        {
                            Bay bay = toNode.ToLines.First().Bay;

                            bay.FromLines.Add(line);
                        }

                        // IntraBay의 ToLine인지 검사
                        // 경우는 2가지. InterBay에서 나가는 line이거나 옆 IntraBay에서 나가는 line이거나

                        // Case 1. InterBay에서 나가는 line
                        toLine = toNode.ToLines.First();

                        if (toLine.Bay.BayType == BAY_TYPE.INTERBAY)
                        { lineBay.ToLines.Add(line); }

                        // Case 2. 옆 IntraBay에서 들어오는 line
                        if (line.Name.Contains("leftOut_1") || line.Name.Contains("rightOut_1"))
                        {
                            Bay bay = fromNode.FromLines.First().Bay;

                            bay.ToLines.Add(line);
                        }

                        break;
                }
            }
        }

        public void InitializeZcu()
        {
            foreach (ZCU zcu in Zcus.Values)
            {
                zcu.SetLines();
            }
        }

        public SimNode GetSimNodebyID(uint ID)
        {
            if (_simNodes.ContainsKey(ID))
                return _simNodes[ID];
            else
                return null;
        }

        #region Model Control
        #region Fab
        public Fab AddFab(string fabName)
        {
            Fab fab = new Fab(fabName);
            _fabs.Add(fab.Name, fab);
            List<RouteSelection> routeSelections = new List<RouteSelection>();
            RouteSelections.Add(fabName, routeSelections);
            List<RailLineNode> railCuts = new List<RailLineNode>();
            RailCuts.Add(fabName, railCuts);

            return fab;
        }

        public void GetDataToFab()
        {
            foreach(Fab fab in Fabs.Values)
            {
                fab.GetFabModels();
            }
        }

        public void GetOHtDataOfFab()
        {
            foreach(Fab fab in Fabs.Values)
            {
                fab.GetLstOHT();
            }
        }
        #endregion
        #region Entity
        public Foup AddFoup(string entityName)
        {
            string foupName = entityName + "_" + _entityID;
            Foup entity = new Foup(_entityID, foupName);

            Foup product = _products[entityName];
            entity.LstProcess = product.LstProcess.ToList();
            entity.LstProcess.RemoveAt(0);

            _foups.Add(entity.Name, entity);
            _entityID++;

            return entity;
        }

        public Foup AddFoup(string fabName, string entityName, string currentStepId, string currentProcessId, string currentProductId, string currentStepType, FabSimNode currentEqp, FOUP_STATE currentState, int lotQty)
        {
            Foup entity = new Foup(entityName);
            entity.Fab = Fabs[fabName];
            entity.ProductID = currentProductId;
            entity.CurrentStepID = currentStepId;
            entity.CurrentProcessID = currentProcessId;
            entity.CurrentStepType = currentStepType;
            entity.CurrentEqp = currentEqp;
            if(currentEqp != null) { entity.CurrentNode = currentEqp; }
            entity.CurrentState = currentState;
            entity.LotQty = lotQty;

            _foups.Add(entity.Name, entity);
            _entityID++;

            return entity;
        }

        public Foup AddProduct(string entityName)
        {
            uint id = Convert.ToUInt32(_products.Count);
            Foup product = new Foup(id, entityName);
            _products.Add(entityName, product);
            return product;
        }

        public Foup GetFoupbyID(string name)
        {
            return _foups[name];
        }
        public void RemoveFoupbyID(string entityName)
        {
            _foups.Remove(entityName);
        }

        public uint GetUsableNodeID()
        {
            if (_simNodes.Count == 0)
            {
                return _NodeID;
            }
            else
            {
                IEnumerable<KeyValuePair<uint, SimNode>> sort =
                    from node in _simNodes
                    orderby node.Key descending
                    select node;

                _NodeID = sort.ElementAt(0).Key + 1;
            }
            return _NodeID;
        }
        #endregion
        #region Commit
        public CommitInterval AddCommitInterval(Vector3 pos, string name, string fabName, string entityName, Time startTime, Time intervalTime, int entityCount, RailPortNode rs)
        {
            Fab fab = Fabs[fabName];
            CommitInterval commitInterval = new CommitInterval(_NodeID, pos, name, fab, entityName, startTime, intervalTime, entityCount);
            _simNodes.Add(_NodeID, commitInterval);
            _NodeID++;
            _commit = commitInterval;
            _eqpNodes.Add(name, commitInterval);
            SimLink evtLink_1 = AddEvtSimLink(commitInterval, rs);
            commitInterval.EVTOutLink = evtLink_1;
            rs.EVTInLinks.Add(evtLink_1);
            SimLink evtLink_2 = AddEvtSimLink(rs, commitInterval);
            rs.EVTOutLink = evtLink_2;
            commitInterval.EVTInLinks.Add(evtLink_2);

            return commitInterval;
        }

        public CommitInterval AddCommitInterval(string name, string fabName, string entityName, Time startTime, Time intervalTime, int entityCount)
        {
            Fab fab = Fabs[fabName];
            CommitInterval commitInterval = new CommitInterval(_NodeID, name, fab);
            _simNodes.Add(_NodeID, commitInterval);
            _eqpNodes.Add(name, commitInterval);
            _NodeID++;
            _commit = commitInterval;

            return commitInterval;
        }

        #endregion
        #region Buffer

        public TBNode AddTB(uint id, string name, string fabName, string type, uint capacity)
        {
            Fab fab = Fabs[fabName];
            BUFFER_TYPE bufferType = (BUFFER_TYPE)Enum.Parse(typeof(BUFFER_TYPE), type);
            TBNode buffer = new TBNode(id, name, fab, capacity, bufferType);
            _simNodes.Add(id, buffer);
            _bufferNodes.Add(buffer);
            _eqpNodes.Add(name, buffer);
            _NodeID++;

            return buffer;
        }

        public TBNode AddTB(Vector3 pos, string name, string fabName, BUFFER_TYPE type)
        {
            Fab fab = Fabs[fabName];
            TBNode tb = new TBNode(_NodeID, pos, name, fab, type);
            _simNodes.Add(_NodeID, tb);
            _bufferNodes.Add(tb);
            _eqpNodes.Add(name, tb);
            _NodeID++;

            return tb;
        }

        public TBNode AddTB(Vector3 pos, string name, string fabName, BUFFER_TYPE type, RailPortNode rs)
        {
            Fab fab = Fabs[fabName];
            TBNode tb = new TBNode(_NodeID, pos, name, fab, type);
            _simNodes.Add(_NodeID, tb);
            _bufferNodes.Add(tb);
            _eqpNodes.Add(name, tb);
            _NodeID++;

            SimLink evtLink_1 = AddEvtSimLink(tb, rs);
            tb.EVTOutLink = evtLink_1;
            rs.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(rs, tb);
            rs.EVTOutLink = evtLink_2;
            tb.EVTInLinks.Add(evtLink_2);

            return tb;
        }

        public TBNode AddTB(string name, string fabName, BUFFER_TYPE type, RailLineNode line, double distance)
        {
            Fab fab = Fabs[fabName];

            Vector3 pos = line.StartPoint + line.Direction * distance;

            TBNode tb = new TBNode(_NodeID, pos, name, fab, type);
            _simNodes.Add(_NodeID, tb);
            _bufferNodes.Add(tb);
            _eqpNodes.Add(name, tb);
            _NodeID++;

            return tb;
        }

        public ProcessPortNode AddProcessPort(RailLineNode line, double distance, ProcessEqpNode processEqp)
        {
            ProcessPortNode port = new ProcessPortNode(_NodeID, line, distance, processEqp, processEqp.Fab);
            _simNodes.Add(_NodeID, port);
            _portNodes.Add(port.Name, port);
            _NodeID++;

            _normalPorts.Add(port.Name, port);

            SimLink evtLink_1 = AddEvtSimLink(port, processEqp);
            port.EVTOutLink = evtLink_1;
            processEqp.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(processEqp, port);
            processEqp.EVTOutLink = evtLink_2;
            port.EVTInLinks.Add(evtLink_2);

            return port;
        }

        public ProcessPortNode AddProcessPort(uint id, string name, RailLineNode line, double distance, CommitEqpNode eqp)
        {
            ProcessPortNode port = new ProcessPortNode(id, name, line, distance, eqp, eqp.Fab);
            _simNodes.Add(id, port);
            _portNodes.Add(port.Name, port);
            _NodeID++;

            if (name.Contains("AR") || name.Contains("PS"))
                _reticlePorts.Add(port.Name, port);
            else
                _normalPorts.Add(port.Name, port);

            SimLink evtLink_1 = AddEvtSimLink(port, eqp);
            port.EVTOutLink = evtLink_1;
            eqp.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(eqp, port);
            eqp.EVTOutLink = evtLink_2;
            port.EVTInLinks.Add(evtLink_2);

            return port;
        }

        public ProcessPortNode AddProcessPort(uint id, string name, RailLineNode line, double distance, CompleteEqpNode eqp)
        {
            ProcessPortNode port = new ProcessPortNode(id, name, line, distance, eqp, eqp.Fab);
            _simNodes.Add(id, port);
            _portNodes.Add(port.Name, port);
            _NodeID++;

            if (name.Contains("AR") || name.Contains("PS"))
                _reticlePorts.Add(port.Name, port);
            else
                _normalPorts.Add(port.Name, port);

            SimLink evtLink_1 = AddEvtSimLink(port, eqp);
            port.EVTOutLink = evtLink_1;
            eqp.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(eqp, port);
            eqp.EVTOutLink = evtLink_2;
            port.EVTInLinks.Add(evtLink_2);

            return port;
        }

        public ProcessPortNode AddProcessPort(uint id, string name, RailLineNode line, double distance, ProcessEqpNode processEqp)
        {
            ProcessPortNode port = new ProcessPortNode(id, name, line, distance, processEqp, processEqp.Fab);
            _simNodes.Add(id, port);
            _portNodes.Add(port.Name, port);
            _NodeID++;

            if (name.Contains("AR") || name.Contains("PS") || processEqp.StepGroup is STEP_GROUP.PHOTO)
                _reticlePorts.Add(port.Name, port);
            else
                _normalPorts.Add(port.Name, port);

            SimLink evtLink_1 = AddEvtSimLink(port, processEqp);
            port.EVTOutLink = evtLink_1;
            processEqp.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(processEqp, port);
            processEqp.EVTOutLink = evtLink_2;
            port.EVTInLinks.Add(evtLink_2);

            return port;
        }

        public ProcessPortNode AddProcessPort(ProcessEqpNode processEqp)
        {
            ProcessPortNode port = new ProcessPortNode(_NodeID, processEqp, processEqp.Fab);
            _simNodes.Add(_NodeID, port);
            _portNodes.Add(port.Name, port);
            _NodeID++;

            if (processEqp.StepGroup == STEP_GROUP.PHOTO)
                _reticlePorts.Add(port.Name, port);
            else
                _normalPorts.Add(port.Name, port);

            SimLink evtLink_1 = AddEvtSimLink(port, processEqp);
            port.EVTOutLink = evtLink_1;
            processEqp.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(processEqp, port);
            processEqp.EVTOutLink = evtLink_2;
            port.EVTInLinks.Add(evtLink_2);

            return port;
        }
        #endregion
        #region ProcessEqp
        public ProcessEqpNode AddProcessEqp(string name, string fabName)
        {
            Fab fab = Fabs[fabName];

            ProcessEqpNode processEqp = new ProcessEqpNode(_NodeID, name, PROCESS_TYPE.INLINE, fab);

            _simNodes.Add(_NodeID, processEqp);
            _eqpNodes.Add(name, processEqp);
            _processEqps.Add(name, processEqp);
            _NodeID++;

            return processEqp;
        }

        public ProcessEqpNode AddProcessEqp(uint id, string name, string fabName, string stepGroup, string type, int capacity)
        {
            Fab fab = Fabs[fabName];
            STEP_GROUP stepGroupEnum = (STEP_GROUP)Enum.Parse(typeof(STEP_GROUP), stepGroup);
            PROCESS_TYPE processTypeEnum = (PROCESS_TYPE)Enum.Parse(typeof(PROCESS_TYPE), type);

            ProcessEqpNode processEqp = new ProcessEqpNode(id, name, fab, capacity, processTypeEnum);

            _simNodes.Add(id, processEqp);
            _eqpNodes.Add(name, processEqp);
            _processEqps.Add(name, processEqp);

            return processEqp;
        }

        public ProcessEqpNode AddProcessEqp(uint id, string fabName, string eqpId, string processGroup, string processType, string stepGroup, string bayName, int minBatchSize = 0, int maxBatchSize = 0)
        {
            Fab fab = Fabs[fabName];

            ProcessEqpNode processEqp = new ProcessEqpNode(id, fab, eqpId, processGroup, processType, stepGroup, bayName, minBatchSize, maxBatchSize);

            _simNodes.Add(id, processEqp);
            _eqpNodes.Add(eqpId, processEqp);
            _processEqps.Add(eqpId, processEqp);
            _NodeID++;

            return processEqp;
        }

        public CommitEqpNode AddCommitEqp(string fabName, string eqpId, string processGroup, string processType, string stepGroup, string bayName)
        {
            Fab fab = Fabs[fabName];

            uint id = GetUsableNodeID();

            CommitEqpNode commitEqp = new CommitEqpNode(id, fab, eqpId, processGroup, processType, stepGroup, bayName);

            _simNodes.Add(id, commitEqp);
            _eqpNodes.Add(eqpId, commitEqp);
            _commitEqp = commitEqp;
            _processEqps.Add(eqpId, commitEqp);
            _NodeID++;

            return commitEqp;
        }

        public CommitEqpNode AddCommitEqp(uint id, string fabName, string eqpId, string processGroup, string processType, string stepGroup, string bayName)
        {
            Fab fab = Fabs[fabName];

            CommitEqpNode commitEqp = new CommitEqpNode(id, fab, eqpId, processGroup, processType, stepGroup, bayName);

            _simNodes.Add(id, commitEqp);
            _eqpNodes.Add(eqpId, commitEqp);
            _commitEqp = commitEqp;
            _processEqps.Add(eqpId, commitEqp);
            _NodeID++;

            return commitEqp;
        }

        public CompleteEqpNode AddCompleteEqp(string fabName, string eqpId, string processGroup, string processType, string stepGroup, string bayName)
        {
            Fab fab = Fabs[fabName];

            uint id = GetUsableNodeID();

            CompleteEqpNode completeEqp = new CompleteEqpNode(id, fab, eqpId, processGroup, processType, stepGroup, bayName);

            _simNodes.Add(id, completeEqp);
            _eqpNodes.Add(eqpId, completeEqp);
            _completeEqp = completeEqp;
            _processEqps.Add(eqpId, completeEqp);
            _NodeID++;

            return completeEqp;
        }

        public CompleteEqpNode AddCompleteEqp(uint id, string fabName, string eqpId, string processGroup, string processType, string stepGroup, string bayName)
        {
            Fab fab = Fabs[fabName];

            CompleteEqpNode completeEqp = new CompleteEqpNode(id, fab, eqpId, processGroup, processType, stepGroup, bayName);

            _simNodes.Add(id, completeEqp);
            _eqpNodes.Add(eqpId, completeEqp);
            _completeEqp = completeEqp;
            _processEqps.Add(eqpId, completeEqp);
            _NodeID++;

            return completeEqp;
        }

        public ProcessEqpNode AddProcessEqp(uint id, string name, string fabName, string type, int capacity)
        {
            Fab fab = Fabs[fabName];
            PROCESS_TYPE processTypeEnum = (PROCESS_TYPE)Enum.Parse(typeof(PROCESS_TYPE), type);

            ProcessEqpNode process = new ProcessEqpNode(id, name, fab, capacity, processTypeEnum);
            _processTypes.Add(process.ID, process);
            _eqpNodes.Add(name, process);
            _processEqps.Add(name, process);

            return process;
        }
        #endregion
        #region ProcessStep
        public void AddStep(Step step)
        {
            try
            {
                _steps.Add(step.ID, step);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
        #region Complete
        public CompleteNode AddComplete(Vector3 pos, string name, string fabName, RailPortNode rs)
        {
            Fab fab = Fabs[fabName];
            CompleteNode complete = new CompleteNode(_NodeID, pos, name, fab, COMPLETE_TYPE.NORMAL);
            _simNodes.Add(_NodeID, complete);
            _eqpNodes.Add(name, complete);
            _NodeID++;
            _complete = complete;

            SimLink evtLink_1 = AddEvtSimLink(complete, rs);
            complete.EVTOutLink = evtLink_1;
            rs.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(rs, complete);
            rs.EVTOutLink = evtLink_2;
            complete.EVTInLinks.Add(evtLink_2);

            return complete;
        }

        public CompleteNode AddComplete(string name, string fabName)
        {
            Fab fab = Fabs[fabName];
            CompleteNode complete = new CompleteNode(_NodeID, name, fab, COMPLETE_TYPE.NORMAL);
            _simNodes.Add(_NodeID, complete);
            _eqpNodes.Add(name, complete);
            _NodeID++;
            _complete = complete;

            return complete;
        }
        #endregion
        #region RailPoint
        public RailPointNode AddRailPoint(Vector3 pos, string name, string fabName, string zcuName, ZCU_TYPE zcuType)
        {
            ZCU zcu = null;
            Fab fab = Fabs[fabName];
            if (zcuName != null && zcuName != string.Empty)
                zcu = Zcus[zcuName];


            RailPointNode rp = new RailPointNode(_NodeID, pos, name, fab, zcu, zcuType);
            _simNodes.Add(_NodeID, rp);
            _railPointNodes.Add(name, rp);
            _NodeID++;

            if (zcu != null)
            {
                if (zcuType == ZCU_TYPE.STOP)
                    zcu.FromPoints.Add(rp.Name, rp);
                else if (zcuType == ZCU_TYPE.RESET)
                {
                    zcu.ToPoints.Add(rp.Name, rp);
                }
            }
            return rp;
        }



        public RailPointNode AddRailPoint(uint ID, Vector3 pos, string name, string fabName, string zcuName, string zcuTypeString)
        {
            ZCU zcu = null;
            Fab fab = Fabs[fabName];
            if (zcuName != "")
                zcu = Zcus[zcuName];

            ZCU_TYPE zcuType = (ZCU_TYPE)Enum.Parse(typeof(ZCU_TYPE), zcuTypeString);

            RailPointNode rp = new RailPointNode(ID, pos, name, fab, zcu, zcuType);
            _simNodes.Add(ID, rp);
            _railPointNodes.Add(name, rp);
            _NodeID++;

            if (zcuType == ZCU_TYPE.STOP)
                zcu.FromPoints.Add(rp.Name, rp);
            else if (zcuType == ZCU_TYPE.RESET)
            {
                zcu.ToPoints.Add(rp.Name, rp);
            }

            return rp;
        }

        public RailPointNode AddRailPoint(uint ID, Vector3 pos, string name, string fabName, string zcuName, ZCU_TYPE zcuType)
        {
            ZCU zcu = null;
            Fab fab = Fabs[fabName];
            if (zcuName != "")
                zcu = Zcus[zcuName];

            RailPointNode rp = new RailPointNode(ID, pos, name, fab, zcu, zcuType);
            _simNodes.Add(ID, rp);
            _railPointNodes.Add(name, rp);
            _NodeID++;

            return rp;
        }

        public void SetStopNResetLine()
        {
            foreach (RailPointNode point in DicRailPoint.Values)
            {
                if (point.ZcuType == ZCU_TYPE.STOP)
                    point.SaveStopNResetLine();
            }
        }
        #endregion
        #region RailLine
        public RailLineNode AddRailLine(string name, string fabName, uint startPointID, uint endPointID, bool isCurve = false, double maxSpeed = 1)
        {
            Fab fab = Fabs[fabName];
            RailPointNode startRoadNode = (RailPointNode)GetSimNodebyID(startPointID);
            RailPointNode endRoadNode = (RailPointNode)GetSimNodebyID(endPointID);

            RailLineNode rl = new RailLineNode(_NodeID, name, fab, startRoadNode, endRoadNode, maxSpeed, OHTSize + OHTMinimumDistance, isCurve);
            _simNodes.Add(_NodeID, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(string name, string fabName, uint startPointID, uint endPointID, double distance, bool isCurve = false, double maxSpeed = 1)
        {
            Fab fab = Fabs[fabName];
            RailPointNode startRoadNode = (RailPointNode)GetSimNodebyID(startPointID);
            RailPointNode endRoadNode = (RailPointNode)GetSimNodebyID(endPointID);

            RailLineNode rl = new RailLineNode(_NodeID, name, fab, startRoadNode, endRoadNode, maxSpeed, distance, OHTSize + OHTMinimumDistance, isCurve);
            _simNodes.Add(_NodeID, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(uint id, string name, string fabName, uint startPointID, uint endPointID, bool isCurve = false, double maxSpeed = 1)
        {
            Fab fab = Fabs[fabName];
            RailPointNode startRP = (RailPointNode)GetSimNodebyID(startPointID);
            RailPointNode endRP = (RailPointNode)GetSimNodebyID(endPointID);

            RailLineNode rl = new RailLineNode(id, name, fab, startRP, endRP, maxSpeed, OHTSize + OHTMinimumDistance, isCurve);

            _simNodes.Add(id, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(uint id, string name, string fabName, uint startPointID, uint endPointID, double distance, bool isCurve = false, double maxSpeed = 1)
        {
            Fab fab = Fabs[fabName];

            RailPointNode startRP = (RailPointNode)GetSimNodebyID(startPointID);
            RailPointNode endRP = (RailPointNode)GetSimNodebyID(endPointID);

            RailLineNode rl = new RailLineNode(id, name, fab, startRP, endRP, distance, maxSpeed, OHTSize + OHTMinimumDistance, isCurve);

            _simNodes.Add(id, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(uint id, string name, string fabName, uint startPointID, uint endPointID, double distance, bool isCurve = false, double maxSpeed = 1, bool left = true)
        {
            Fab fab = Fabs[fabName];

            RailPointNode startRP = (RailPointNode)GetSimNodebyID(startPointID);
            RailPointNode endRP = (RailPointNode)GetSimNodebyID(endPointID);

            RailLineNode rl = new RailLineNode(id, name, fab, startRP, endRP, distance, maxSpeed, OHTSize + OHTMinimumDistance, isCurve, left);

            _simNodes.Add(id, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(string name, string fabName, RailPointNode startRP, RailPointNode endRP, bool isCurve = false, double maxSpeed = 1)
        {
            Fab fab = Fabs[fabName];
            RailLineNode rl = new RailLineNode(_NodeID, name, fab, startRP, endRP, maxSpeed, OHTSize + OHTMinimumDistance, isCurve);
            _simNodes.Add(_NodeID, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(string name, string fabName, Bay bay, RailPointNode startRP, RailPointNode endRP,  bool isCurve = false, double maxSpeed = 1)
        {
            Fab fab = Fabs[fabName];
            RailLineNode rl = new RailLineNode(_NodeID, name, fab, startRP, endRP, maxSpeed, OHTSize + OHTMinimumDistance, isCurve);
            rl.Bay = bay;
            _simNodes.Add(_NodeID, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(string name, string fabName, RailPointNode startRP, RailPointNode endRP, bool isCurve = false, double maxSpeed = 1, bool left = true)
        {
            Fab fab = Fabs[fabName];
            RailLineNode rl = new RailLineNode(_NodeID, name, fab, startRP, endRP, maxSpeed, OHTSize + OHTMinimumDistance, isCurve, left);
            _simNodes.Add(_NodeID, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(string name, string fabName, RailPointNode startRP, RailPointNode endRP, double distance, bool isCurve = false, double maxSpeed = 1)
        {
            Fab fab = Fabs[fabName];
            RailLineNode rl = new RailLineNode(_NodeID, name, fab, startRP, endRP, distance, maxSpeed, OHTSize + OHTMinimumDistance, isCurve);
            _simNodes.Add(_NodeID, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public RailLineNode AddRailLine(string name, string fabName, RailPointNode startRP, RailPointNode endRP, double distance, bool isCurve = false, double maxSpeed = 1, bool left = true)
        {
            Fab fab = Fabs[fabName];
            RailLineNode rl = new RailLineNode(_NodeID, name, fab, startRP, endRP, distance, maxSpeed, OHTSize + OHTMinimumDistance, isCurve, left);
            _simNodes.Add(_NodeID, rl);
            _railLineNodes.Add(name, rl);
            _NodeID++;

            return rl;
        }

        public void ChangeLineToLongEdge(string fabName)
        {
            try
            {
                IEnumerable<KeyValuePair<string, RailLineNode>> lines = ModelManager.Instance.DicRailLine.Where(p => p.Value.Fab.Name == fabName);

                Dictionary<string, RailLineNode> dicRailLine = lines.ToDictionary(p => p.Key, p => p.Value);
                List<string> listTailRailLineName = new List<string>();
                foreach (RailLineNode line in dicRailLine.Values)
                {
                    if (listTailRailLineName.Contains(line.Name))
                        continue;

                    string shortEdgeInfo = string.Empty;
                    RailPointNode fromPoint = line.FromNode;
                    RailPointNode toPoint = line.ToNode;

                    List<RailLineNode> removedLines = new List<RailLineNode>();
                    List<RailPointNode> removedPoints = new List<RailPointNode>();

                    bool isLongLine = false;
                    bool isFirst = true;

                    double angle = 0;
                    if (fromPoint.FromLines.Count != 0)
                        angle = Vector2.AngleDegree(new Vector2(fromPoint.FromLines[0].Direction.X, fromPoint.FromLines[0].Direction.Y), new Vector2(line.Direction.X, line.Direction.Y));

                    double minimumAngle = 3;

                    if (line.DicBay.Count > 0)
                        continue;

                    if (fromPoint.FromLines.Count > 1
                        || (fromPoint.ToLines.Count > 1 && angle < minimumAngle)
                        || fromPoint.ZcuType == ZCU_TYPE.RESET
                        || (fromPoint.ToLines.Count == 1 && angle > minimumAngle && fromPoint.Zcu == null)
                        || fromPoint.FromLines.Count == 0
                        || fromPoint.ZcuType == ZCU_TYPE.STOP 
                        || (fromPoint.FromLines.Count > 0 && fromPoint.FromLines[0].MaxSpeed != line.MaxSpeed) 
                        && toPoint.ZcuType == ZCU_TYPE.NON 
                        && angle < minimumAngle)
                    {
                        angle = Vector2.AngleDegree(new Vector2(toPoint.FromLines[0].Direction.X, toPoint.FromLines[0].Direction.Y), new Vector2(toPoint.ToLines[0].Direction.X, toPoint.ToLines[0].Direction.Y));
                        while (toPoint.FromLines.Count == 1 && toPoint.ToLines.Count == 1
                            && toPoint.FromLines[0].MaxSpeed == toPoint.ToLines[0].MaxSpeed
                            && angle < minimumAngle
//                            && toPoint.ToLines[0].DicBay.Count == 0
                            && toPoint.ZcuType == ZCU_TYPE.NON)
                        {
                            isLongLine = true;

                            if (isFirst)
                            {
                                shortEdgeInfo = toPoint.FromLines[0].Name;
                                _railLineNodes.Remove(toPoint.FromLines[0].Name);
                                fromPoint.OutLinks.Remove(fromPoint.GetOutLink(line)); //수정 필요
                                isFirst = false;
                            }

                            shortEdgeInfo = shortEdgeInfo + "," + toPoint.ToLines[0].Name;
                            _railLineNodes.Remove(toPoint.ToLines[0].Name);
                            removedLines.Add(toPoint.ToLines[0]);
                            _railPointNodes.Remove(toPoint.Name);
                            removedPoints.Add(toPoint);
                            listTailRailLineName.Add(toPoint.ToLines[0].Name);
                            if (toPoint.ToLines.Count > 0)
                                toPoint = toPoint.ToLines[0].ToNode;
                            else
                                break;

                            angle = Vector2.AngleDegree(new Vector2(toPoint.FromLines[0].Direction.X, toPoint.FromLines[0].Direction.Y), new Vector2(toPoint.ToLines[0].Direction.X, toPoint.ToLines[0].Direction.Y));
                        }

                        if (isLongLine)
                        {
                            RailLineNode newLine = ModelManager.Instance.AddRailLine(shortEdgeInfo, line.Fab.Name, line.FromNode, toPoint, line.IsCurve, line.MaxSpeed, line.Left);
                            removedLines.Insert(0, line);
                            newLine.ShortEdges = GetShortLines(removedLines);
                            newLine.ShortEdgeInfo = shortEdgeInfo;
                            newLine.OutLinks[0].EndNode = toPoint;
                            toPoint.InLinks[0].StartNode = newLine;

                            newLine.Length = newLine.ShortEdges.Sum(x => x.Length);

                            foreach (RailLineNode removedLine in removedLines)
                            {
                                foreach (KeyValuePair<string, ZONE_LINE_TYPE> kvp in removedLine.DicBay)
                                    newLine.DicBay.Add(kvp.Key, kvp.Value);
                                ModelManager.Instance.SimNodes.Remove(removedLine.ID);
                                ModelManager.Instance.DicRailLine.Remove(removedLine.Name);
                            }

                            foreach (RailPointNode removedPoint in removedPoints)
                            {
                                ModelManager.Instance.SimNodes.Remove(removedPoint.ID);
                                ModelManager.Instance.DicRailPoint.Remove(removedPoint.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LongEdge Error");
            }
        }


        public List<RailLineNode> GetShortLines(List<RailLineNode> lines)
        {
            List<RailLineNode> removedShortLines = new List<RailLineNode>();
            foreach(RailLineNode shortLine in lines)
            {
                if (shortLine.ShortEdges.Count == 0)
                    removedShortLines.Add(shortLine);
                else
                    removedShortLines.AddRange(shortLine.ShortEdges);
            }

            return removedShortLines;
        }

        public RailLineNode GetLine(string fromPointName, string toPointName)
        {
            RailLineNode line = null;

            foreach(RailLineNode lineTemp in _railLineNodes.Values)
            {
                if(lineTemp.FromPointName == fromPointName && lineTemp.ToPointName == toPointName)
                {
                    line = lineTemp;
                    break;
                }
            }

            return line;
        }

        public void DeleteUnnessaryZcuReset(string fabName)
        {
            foreach(RailPointNode point in ModelManager.Instance.DicRailPoint.Values)
            {
                if(point.ZcuType == ZCU_TYPE.RESET)
                {
                    if(point.FromLines.Count == 1 && point.ToLines.Count == 1 
                        && point.FromLines[0].FromNode.ZcuType == ZCU_TYPE.STOP
                        && point.ToLines[0].ToNode.ZcuType == ZCU_TYPE.RESET)
                    {
                        point.Zcu = null;
                        point.ZcuType = ZCU_TYPE.NON;
                    }
                }
            }
        }

        private double getLength_v_v0_a(double v, double v0, double a)
        {
            return (v * v - v0 * v0) / (2 * a);
        }

        public double GetLongDistance(string pointName, out RailLineNode longLine)
        {
            foreach(RailLineNode line in _railLineNodes.Values)
            {
                double distance = 0;
                foreach(RailLineNode shortLine in line.ShortEdges)
                {
                    distance = distance + shortLine.Length;
                    if(shortLine.ToNode.Name == pointName)
                    {
                        longLine = line;
                        return distance;
                    }
                }
            }
            longLine = null;
            return 0;
        }
        public RailLineNode GetRailLine(string fromPointID, string toPointID)
        {
            foreach(RailLineNode line in DicRailLine.Values)
            {
                string fromPointName = line.FabName + "_point_" + fromPointID;
                string toPointName = line.FabName + "_point_" + toPointID;

                if (line.FromNode.Name == fromPointName && line.ToNode.Name == toPointName)
                    return line;
                else
                {
                    foreach(RailLineNode shortLine in line.ShortEdges)
                    {
                        if (shortLine.FromNode.Name == fromPointName && shortLine.ToNode.Name == toPointName)
                            return line;
                    }
                }                      
            }

            return null;
        }

        public void InitializeRailLineSpec()
        {
            foreach(RailLineNode line in _railLineNodes.Values)
                line.InitializeSpec();
        }
        #endregion
        #region RailStation
        public RailPortNode AddRailPort(string fabName, string name, RailLineNode rl, double length)
        {
            Fab fab = Fabs[fabName];
            RailPortNode rp = new RailPortNode(_NodeID, name, fab, rl, length);
            _simNodes.Add(_NodeID, rp);
            _portNodes.Add(rp.Name, rp);
            _NodeID++;

            if (name.Contains("AR") || name.Contains("PS"))
                _reticlePorts.Add(rp.Name, rp);
            else
                _normalPorts.Add(rp.Name, rp);

            return rp;
        }

        public RailPortNode AddRailPort(string fabName, RailLineNode rl, double length, FabSimNode eqp)
        {
            string name = fabName + "_" + eqp.Name + "_port_" + (eqp.DicRailPort.Count + 1);
            Fab fab = Fabs[fabName];
            RailPortNode rp = new RailPortNode(_NodeID, name, fab, rl, length);
            _simNodes.Add(_NodeID, rp);
            _portNodes.Add(rp.Name, rp);
            _NodeID++;

            rp.ConnectedEqp = eqp;
            eqp.DicRailPort.Add(rp);

            if (name.Contains("AR") || name.Contains("PS"))
                _reticlePorts.Add(rp.Name, rp);
            else
                _normalPorts.Add(rp.Name, rp);

            SimLink evtLink_1 = AddEvtSimLink(eqp, rp);
            eqp.EVTOutLink = evtLink_1;
            rp.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(rp, eqp);
            rp.EVTOutLink = evtLink_2;
            eqp.EVTInLinks.Add(evtLink_2);

            return rp;
        }

        public RailPortNode AddRailPort(string fabName, string name, RailLineNode rl, double length, FabSimNode eqp)
        {
            Fab fab = Fabs[fabName];
            RailPortNode rp = new RailPortNode(_NodeID, name, fab, rl, length);
            _simNodes.Add(_NodeID, rp);
            _portNodes.Add(rp.Name, rp);
            _NodeID++;

            if (name.Contains("AR") || name.Contains("PS"))
                _reticlePorts.Add(rp.Name, rp);
            else
                _normalPorts.Add(rp.Name, rp);

            rp.ConnectedEqp = eqp;
            eqp.DicRailPort.Add(rp);

            SimLink evtLink_1 = AddEvtSimLink(eqp, rp);
            eqp.EVTOutLink = evtLink_1;
            rp.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(rp, eqp);
            rp.EVTOutLink = evtLink_2;
            eqp.EVTInLinks.Add(evtLink_2);

            return rp;
        }

        public RailPortNode AddRailPort(uint id, string fabName, string name, string rlName, double length, string eqpName, string type, bool waitAllowed = true, bool bumpAllowed = true)
        {
            RailLineNode rl = _railLineNodes[rlName];
            FabSimNode eqp = _eqpNodes[eqpName];
            Fab fab = Fabs[fabName];
            PORT_TYPE typeEnum = (PORT_TYPE)Enum.Parse(typeof(PORT_TYPE), type);
            RailPortNode rp = new RailPortNode(id, name, fab, rl, length, typeEnum, waitAllowed, bumpAllowed);
            _simNodes.Add(id, rp);
            _portNodes.Add(rp.Name, rp);
            _NodeID++;

            if (name.Contains("AR") || name.Contains("PS"))
                _reticlePorts.Add(rp.Name, rp);
            else
                _normalPorts.Add(rp.Name, rp);

            rp.ConnectedEqp = eqp;
            eqp.DicRailPort.Add(rp);

            SimLink evtLink_1 = AddEvtSimLink(eqp, rp);
            eqp.EVTOutLink = evtLink_1;
            rp.EVTInLinks.Add(evtLink_1);

            SimLink evtLink_2 = AddEvtSimLink(rp, eqp);
            rp.EVTOutLink = evtLink_2;
            eqp.EVTInLinks.Add(evtLink_2);

            return rp;
        }


        public SimNode GetNowRoadNodeOrLinkOfOHT(Vector3 position)
        {
            foreach (SimNode simNode in SimNodes.Values)
            {
                if (simNode is RailPointNode)
                {
                    if (((RailPointNode)simNode).PosVec3 == position)
                        return simNode;
                }
                else if (simNode is RailLineNode)
                {
                    foreach (OHTNode oht in ((RailLineNode)simNode).ListOHT)
                    {
                        if (oht.PosVec3 == position)
                            return simNode;
                    }
                }
            }

            return null;
        }

        public RailPortNode GetNowRailPortOfOHT(Vector3 position)
        {
            foreach (RailPortNode railPort in DicRailPort.Values)
            {
                if (railPort.CalPos() == position)
                    return railPort;
            }

            return null;
        }
        #endregion
        #region OHT
        public OHTNode AddOHT(string name, string fabName)
        {

            Fab fab = Fabs[fabName];
            OHTNode oht = new OHTNode(_NodeID, name, fab);
            _simNodes.Add(_NodeID, oht);
            _ohtNodes.Add(oht);
            _NodeID++;

            return oht;
        }

        public OHTNode AddOHT(string name, string fabName, bool reticle)
        {
            Fab fab = Fabs[fabName];
            OHTNode oht = new OHTNode(_NodeID, name, fab, reticle);

            _simNodes.Add(_NodeID, oht);
            _ohtNodes.Add(oht);
            _NodeID++;

            return oht;
        }

        public OHTNode AddOHT(uint id, string name, string fabName, bool reticle, string railLineName, double distance)
        {
            Fab fab = Fabs[fabName];
            RailLineNode railLine = DicRailLine[railLineName];
            OHTNode oht = new OHTNode(id, name, fab, reticle, railLine);
            oht.PosVec3 = railLine.StartPoint + railLine.Direction * distance * railLine.Cross / railLine.Length;

            _simNodes.Add(id, oht);
            _ohtNodes.Add(oht);
            _NodeID++;

            railLine.initOHT(oht, distance, 0);
            oht.setCurRailLine(railLine);
            oht.SetCurDistance(distance);
            if (railLine.Bay != null)
            {
                oht.setCurBay(railLine.Bay);

                if (railLine.Bay.BumpingPorts.Count > 0)
                {
                    oht.BumpingBay = railLine.Bay;
                    oht.IsBumping = true;
                }
            }
            return oht;
        }

        public OHTNode AddOHT(uint id, string name, string fabName, bool reticle, string railLineName, double distance, double speed)
        {
            Fab fab = Fabs[fabName];
            RailLineNode railLine = DicRailLine[railLineName];
            OHTNode oht = new OHTNode(id, name, fab, reticle, railLine, speed);
            oht.PosVec3 = railLine.StartPoint + railLine.Direction * distance * railLine.Cross / railLine.Length;

            _simNodes.Add(id, oht);
            _ohtNodes.Add(oht);
            _NodeID++;

            railLine.initOHT(oht, distance, 0);
            oht.setCurRailLine(railLine);
            oht.SetCurDistance(distance);
            if (railLine.Bay != null)
            {
                oht.setCurBay(railLine.Bay);

                if(railLine.Bay.BumpingPorts.Count > 0)
                {
                    oht.BumpingBay = railLine.Bay;
                    oht.IsBumping = true;
                }
            }

            return oht;
        }

        public OHTNode AddOHT(string name, string fabName, bool reticle, string railLineName, double distance, double speed)
        {
            Fab fab = Fabs[fabName];
            RailLineNode railLine = DicRailLine[railLineName];
            OHTNode oht = new OHTNode(_NodeID, name, fab, reticle, railLine, speed);
            oht.PosVec3 = railLine.StartPoint + railLine.Direction * distance * railLine.Cross / railLine.Length;

            _simNodes.Add(_NodeID, oht);
            _ohtNodes.Add(oht);
            _NodeID++;

            railLine.initOHT(oht, distance, 0);
            oht.setCurRailLine(railLine);
            oht.SetCurDistance(distance);
            if (railLine.Bay != null)
            {
                oht.setCurBay(railLine.Bay);

                if (railLine.Bay.BumpingPorts.Count > 0)
                {
                    oht.BumpingBay = railLine.Bay;
                    oht.IsBumping = true;
                }
            }

            return oht;
        }

        public void MakeOHTs(string fabName, int normalCount, double normalMaxSpeed, int reticleCount, double reticleMaxSpeed)
        {
            _NodeID = GetUsableNodeID();
            Fab fab = ModelManager.Instance.Fabs[fabName];

            double totalNormalLength = 0;
            double totalReticleLength = 0;

            List<RailLineNode> normalLines = new List<RailLineNode>();
            List<RailLineNode> reticleLines = new List<RailLineNode>();

            foreach(RailLineNode line in fab.DicRailLine.Values)
            {
                if (line.Name == "rl_M14A_InterBay_M14A_3_341")
                    ;
                if (fab.RailCuts.Contains(line) || line.Zcu != null)
                    continue;

                if (line.ToNode.ZcuType == ZCU_TYPE.STOP && line.getToNodeZCUReservationPos() < 0)
                    continue;

                if (line.Bay != null && line.Bay.Reticle)
                {
                    totalReticleLength = totalReticleLength + line.Length;
                    reticleLines.Add(line);
                }
                else
                {
                    totalNormalLength = totalNormalLength + line.Length;
                    normalLines.Add(line);
                }
            }

            double intervalNormalDistance = totalNormalLength / (normalCount + 1);
            double intervalReticleDistance = totalReticleLength / (reticleCount + 1);

            int lineNo = 0;
            double lineDistance = 0;
            for (int i = 1; i <= normalCount; i++)
            {
                double remainDistance = intervalNormalDistance;
                OHTNode oht;

                while(remainDistance > 0)
                {
                    if (remainDistance > normalLines[lineNo].Length - lineDistance)
                    {
                        remainDistance = remainDistance - (normalLines[lineNo].Length - lineDistance);
                        lineNo++;
                        lineDistance = 0;
                    }
                    else
                    {
                        lineDistance = lineDistance + remainDistance;
                        remainDistance = 0;
                    }
                }

                double inputLength = lineDistance;
                if (normalLines[lineNo].ToNode.ZcuType == ZCU_TYPE.STOP && normalLines[lineNo].getToNodeZCUReservationPos() < lineDistance)
                    inputLength = normalLines[lineNo].getToNodeZCUReservationPos() - 50; // 여유 50mm 추가

                //OHT 생성
                ModelManager.Instance.AddOHT(fabName + "_V_" + i.ToString(), fabName, false, normalLines[lineNo].Name, inputLength, normalMaxSpeed);
            }

            lineNo = 0;
            for (int i = 1; i <= reticleCount; i++)
            {
                double remainDistance = intervalReticleDistance;
                OHTNode oht;

                while (remainDistance > 0)
                {
                    if (remainDistance > reticleLines[lineNo].Length - lineDistance)
                    {
                        remainDistance = remainDistance - (reticleLines[lineNo].Length - lineDistance);
                        lineNo++;
                        lineDistance = 0;
                    }
                    else
                    {
                        lineDistance = lineDistance + remainDistance;
                        remainDistance = 0;
                    }
                }

                double inputLength = lineDistance;
                if (reticleLines[lineNo].ToNode.ZcuType == ZCU_TYPE.STOP && reticleLines[lineNo].getToNodeZCUReservationPos() < lineDistance)
                    inputLength = reticleLines[lineNo].getToNodeZCUReservationPos() - 50; // 여유 50mm 추가

                //OHT 생성
                ModelManager.Instance.AddOHT(fabName + "_R_" + (i).ToString(), fabName, true, reticleLines[lineNo].Name, inputLength, reticleMaxSpeed);
            }
        }


        public void SetInitialPositionOfOHT(string fabName)
        {
            Fab fab = ModelManager.Instance.Fabs[fabName];

            double totalNormalLength = 0;
            double totalReticleLength = 0;

            List<RailLineNode> normalLines = new List<RailLineNode>();
            List<RailLineNode> reticleLines = new List<RailLineNode>();

            foreach (RailLineNode line in fab.DicRailLine.Values)
            {
                if (fab.RailCuts.Contains(line) || line.Zcu != null)
                    continue;

                if (line.ToNode.ZcuType == ZCU_TYPE.STOP && line.getToNodeZCUReservationPos() < 0)
                    continue;

                if (line.Bay != null && line.Bay.Reticle)
                {
                    totalReticleLength = totalReticleLength + line.Length;
                    reticleLines.Add(line);
                }
                else
                {
                    totalNormalLength = totalNormalLength + line.Length;
                    normalLines.Add(line);
                }
            }

            List<OHTNode> normalOHTs = (fab.LstOHT.Where(x => x.Reticle == false)).ToList();
            List<OHTNode> reticleOHTs = (fab.LstOHT.Where(x => x.Reticle == true)).ToList();

            double intervalNormalDistance = totalNormalLength / (normalOHTs.Count + 1);
            double intervalReticleDistance = totalReticleLength / (reticleOHTs.Count + 1);

            int lineNo = 0;
            double lineDistance = 0;
            foreach (OHTNode oht in normalOHTs)
            {
                double remainDistance = intervalNormalDistance;

                while (remainDistance > 0)
                {
                    if (remainDistance > normalLines[lineNo].Length - lineDistance)
                    {
                        remainDistance = remainDistance - (normalLines[lineNo].Length - lineDistance);
                        lineNo++;
                        lineDistance = 0;
                    }
                    else
                    {
                        lineDistance = lineDistance + remainDistance;
                        remainDistance = 0;
                    }
                }

                double inputLength = lineDistance;
                if (normalLines[lineNo].ToNode.ZcuType == ZCU_TYPE.STOP && normalLines[lineNo].getToNodeZCUReservationPos() < lineDistance)
                    inputLength = normalLines[lineNo].getToNodeZCUReservationPos() - 50;

                normalLines[lineNo].initOHT(oht, inputLength, 0);
                oht.setCurRailLine(normalLines[lineNo]);
                oht.SetCurDistance(inputLength);
                oht.PosVec3 = normalLines[lineNo].StartPoint + normalLines[lineNo].Direction * inputLength;

            }

            lineNo = 0;
            foreach (OHTNode oht in reticleOHTs)
            {
                double remainDistance = intervalReticleDistance;

                while (remainDistance > 0)
                {
                    if (remainDistance > reticleLines[lineNo].Length - lineDistance)
                    {
                        remainDistance = remainDistance - (reticleLines[lineNo].Length - lineDistance);
                        lineNo++;
                        lineDistance = 0;
                    }
                    else
                    {
                        lineDistance = lineDistance + remainDistance;
                        remainDistance = 0;
                    }
                }

                double inputLength = lineDistance;
                if (reticleLines[lineNo].ToNode.ZcuType == ZCU_TYPE.STOP && reticleLines[lineNo].getToNodeZCUReservationPos() < lineDistance)
                    inputLength = reticleLines[lineNo].getToNodeZCUReservationPos() -  50;

                reticleLines[lineNo].initOHT(oht, inputLength, 0);
                oht.setCurRailLine(reticleLines[lineNo]);
                oht.SetCurDistance(inputLength);
                oht.PosVec3 = reticleLines[lineNo].StartPoint + reticleLines[lineNo].Direction * inputLength;
            }
        }

        public void SetRandomDestination()
        {
            int threadCount = 1;

            int parallelCalculationCount = _ohtNodes.Count / threadCount;
            OhtThread[] oths = new OhtThread[threadCount];

            for ( int i = 0; i < threadCount; i++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(SetRandomDestinationByThread));

                OhtThread oth;
                if (i < threadCount - 1)
                    oth = new OhtThread(parallelCalculationCount * i, parallelCalculationCount * (i + 1) - 1, thread);
                else
                    oth = new OhtThread(parallelCalculationCount * i, _ohtNodes.Count - 1, thread);

                oths[i] = oth;
                thread.Start(oth);
            }

            foreach (OhtThread oth in oths)
                oth.thread.Join();
        }

        private void SetRandomDestinationByThread(object threadParameter)
        {
            int startOht = ((OhtThread)threadParameter).startOht;
            int endOht = ((OhtThread)threadParameter).endOht;
            bool isDone = ((OhtThread)threadParameter).isDone;

            for(; startOht <= endOht; startOht++)
            {
                OHTNode oht = _ohtNodes[startOht];

                RailPortNode railPort = null;
                Random random = new Random();

                if (oht.BumpingBay != null)
                {
                    //int randomNum = random.Next(0, oht.BumpingBay.BumpingPorts.Count);
                    //railPort = oht.BumpingBay.BumpingPorts[randomNum];
                    railPort = oht.BumpingBay.BumpingPorts[0];
                    //                    railPort = GetOtherLineBumpingPort(oht);
                }
                else if (oht.Reticle)
                {
                    //int randomNum = random.Next(0, oht.Fab.DicReticlePort.Count);
                    //railPort = oht.Fab.DicReticlePort.Values.ToList()[randomNum];
                    railPort = oht.Fab.DicReticlePort.Values.ToList()[0];
                    //railPort = GetOtherLinePort(oht.Fab.DicReticlePort.Values.ToList(), oht.CurRailLineName);
                }
                else
                {
                    //int randomNum = random.Next(0, oht.Fab.DicNormalPort.Count);
                    //railPort = oht.Fab.DicNormalPort.Values.ToList()[randomNum];
                    railPort = oht.Fab.DicNormalPort.Values.ToList()[0];
                    //railPort = GetOtherLinePort(oht.Fab.DicNormalPort.Values.ToList(), oht.CurRailLineName);
                }
                SimNode destNode = railPort.ConnectedEqp;

                oht.Destination = destNode;
                oht.DestinationPort = railPort;

                oht.SetLstRailLine(PathFinder.Instance.GetPath(SimEngine.Instance.TimeNow, oht)); ;
            }
        }

        public void SetBumpingBayAndPort()
        {
            Time simTime = SimEngine.Instance.TimeNow;
            SimHistory simHistory = SimEngine.Instance.SimHistory;
            foreach (OHTNode oht in _ohtNodes)
            {
                Scheduler.Instance.LetIdleOHTGoToBay(oht, simTime, simHistory);
            }
        }

        private RailPortNode GetOtherLineBumpingPort(OHTNode oht)
        {
            if (oht.BumpingBay != null)
            {
                //                    int randomNum = random.Next(0, oht.BumpingBay.BumpingPorts.Count);
                //                    railPort = oht.BumpingBay.BumpingPorts[randomNum];
                RailPortNode railPort = null;
                foreach (RailPortNode port in oht.BumpingBay.BumpingPorts)
                {
                    if (port.RailLineName != oht.CurRailLineName)
                        return port;
                }
            }
            return null;
        }


        private RailPortNode GetOtherLinePort(List<RailPortNode> list, string lineName)
        {
            RailPortNode railPort = null;
            foreach (RailPortNode port in list)
            {
                if (port.RailLineName != lineName)
                    return port;
            }
            return null;
        }

        public void SetRandomPortToOHT()
        {
            int i = 0;
            foreach (OHTNode oht in _ohtNodes)
            {
                RailPortNode toPort = GetRandomPort(oht);
                FabSimNode toNode = toPort.ConnectedEqp;

                oht.Destination = toPort;
                oht.DestinationPort = toPort;

                Time timeNow = SimEngine.Instance.TimeNow;
                if (PathFinder.Instance.IsUsingCandidatePath && SimModelDBManager.Instance.IsCandidatePath())
                    oht.SetLstRailLine(PathFinder.Instance.GetPath(timeNow, oht), false);
                else
                    oht.SetLstRailLine(PathFinder.Instance.GetPath(timeNow, oht), false);

                i++;
            }
        }

        public RailPortNode GetRandomPort(OHTNode oht)
        {
            RailPortNode railPort = null;
            Random random = new Random();

            if (oht.Reticle)
            {
                int randomNum = random.Next(0, oht.Fab.DicReticlePort.Count);

                railPort = oht.Fab.DicReticlePort.Values.ToList()[randomNum];
            }
            else
            {
                int randomNum = random.Next(0, oht.Fab.DicNormalPort.Count);

                railPort = oht.Fab.DicNormalPort.Values.ToList()[randomNum];
            }
            return railPort;
        }

        public RailPortNode GetRandomBumpingPort(OHTNode oht)
        {
            RailPortNode railPort = null;

            if (oht.Reticle)
            {
                while (railPort == null || railPort.Line.Bay == null || railPort.Line.Bay.BumpingPorts.Count == 0 || railPort.Line.Bay.BumpingPorts.Contains(railPort))
                {
                    int randomNum = random.Next(0, oht.Fab.DicReticlePort.Count);

                    railPort = oht.Fab.DicReticlePort.Values.ToList()[randomNum];
                }
            }
            else
            {
                while (railPort == null || railPort.Line.Bay == null || railPort.Line.Bay.BumpingPorts.Count == 0 || railPort.Line.Bay.BumpingPorts.Contains(railPort))
                {
                    int randomNum = random.Next(0, oht.Fab.DicNormalPort.Count);

                    railPort = oht.Fab.DicNormalPort.Values.ToList()[randomNum];
                }
            }
            return railPort;
        }

        public OHTNode GetOHT(string name)
        {
            foreach (OHTNode oht in LstOHTNode)
            {
                if (oht.Name == name)
                    return oht;
            }

            return null;
        }
        #endregion
        #region Bay

        public Bay AddBay(string name, string fabName, string type, bool reticle)
        {
            Bay bay;
            Fab fab = Fabs[fabName];
            if (type == BAY_TYPE.INTRABAY.ToString())
                bay = new IntraBay(_zoneID, name, fab, reticle);
            else
                bay = new InterBay(_zoneID, name, fab, reticle);

            _bays.Add(name, bay);
            _zoneID++;

            return bay;
        }
        public Bay AddBay(string name, string fabName, bool reticle = false)
        {
            Bay bay;
            Fab fab = Fabs[fabName];
            bay = new Bay(_zoneID, name, fab, reticle);

            _bays.Add(name, bay);
            _zoneID++;

            return bay;
        }
        public Bay AddBay(Vector3 pos, Vector3 size, string name, string fabName, BAY_TYPE type, bool reticle)
        {
            Bay bay;
            Fab fab = Fabs[fabName];

            if (type is BAY_TYPE.INTRABAY)
                bay = new IntraBay(_zoneID, pos, size, name, fab, reticle);
            else
                bay = new InterBay(_zoneID, pos, size, name, fab, reticle);

            _bays.Add(name, bay);
            _zoneID++;

            return bay;
        }
        public InterBay AddInterBay(Vector3 pos, Vector3 size, string name, string fabName, BAY_TYPE type, bool reticle, List<IntraBay> topIntraBays, List<IntraBay> bottomIntraBays)
        {
            Fab fab = Fabs[fabName];

            InterBay bay = new InterBay(_zoneID, pos, size, name, fab, reticle, topIntraBays, bottomIntraBays);

            _bays.Add(name, bay);
            _zoneID++;

            return bay;
        }

        public Bay GetNearestBay(string fabName, Vector3 pos, bool isReticle = false)
        {
            double nearestDistance = double.MaxValue;
            Bay nearestBay = null;

            foreach(Bay bay in _bays.Values)
            {
                if (isReticle && bay.Reticle == false)
                    continue;

                double distance = Vector3.Distance(bay.Position, pos);
                if(fabName == bay.Fab.Name && nearestDistance > distance && bay.NeighborBay.Count > 0 && bay.BumpingPorts.Count > 0 )
                {
                    nearestDistance = distance;
                    nearestBay = bay;
                }
            }

            return nearestBay;
        }

        #endregion
        #region ZCU

        public ZCU AddZcu(string fabName, string name)
        {
            Fab fab = Fabs[fabName];
            ZCU zcu = new ZCU(name, fab);

            Zcus.Add(zcu.Name, zcu);

            return zcu;
        }

        #endregion
        #region HID

        public HID AddHid(string fabName, string name)
        {
            Fab fab = Fabs[fabName];
            HID hid = new HID(_zoneID, name, fab, int.MaxValue);
            _zoneID++;
            Hids.Add(hid.Name, hid);

            return hid;
        }

        public HID AddHid(string fabName, string name, int maxCount)
        {
            Fab fab = Fabs[fabName];
            HID hid = new HID(_zoneID, name, fab, maxCount);
            _zoneID++;
            Hids.Add(hid.Name, hid);

            return hid;
        }

        #endregion
        #region Reticle Zone

        public ReticleZone AddReticleZone(string fabName)
        {
            Fab fab = _fabs[fabName];

            ReticleZone reticleZone = new ReticleZone(_zoneID, fab);
            fab.ReticleZone = reticleZone;
            _reticleZones.Add(fab.Name, reticleZone);

            return reticleZone;
        }

        #endregion

        #region ConveyorPoint
        //public ConveyorPointNode AddConveyorPoint(Vector3 pos, string name)
        //{
        //    ConveyorPointNode cvPt = new ConveyorPointNode(_NodeID, pos, name);
        //    _simNodes.Add(_NodeID, cvPt);
        //    _NodeID++;

        //    return cvPt;
        //}
        #endregion

        #region ConveyorLine
        //public ConveyorLineNode AddConveyorLine(Vector3 startPos, Vector3 endPos, string name)
        //{
        //    ConveyorLineNode cvLine = new ConveyorLineNode(_NodeID, name, startPos, endPos);
        //    _simNodes.Add(_NodeID, cvLine);
        //    _NodeID++;

        //    return cvLine;
        //}
        #endregion

        #region Command
        public CommanderNode AddCommander(List<Command> inactiveCommands)
        {
            CommanderNode commander = new CommanderNode(GetUsableNodeID(), inactiveCommands);

            _commander = commander;

            _simNodes.Add(commander.ID, commander);

            return commander;
        }
        #endregion
        #region Route Selection

        public List<RouteSelection> RouteByFabName(string fabName)
        {
            List<RouteSelection> lstRoute;
            _routeSelections.TryGetValue(fabName, out lstRoute);

            return lstRoute;
        }

        public void AddRouteSelection(string fabName, RouteSelection routeSelection)
        {
            if (_routeSelections.ContainsKey(fabName))
                _routeSelections[fabName].Add(routeSelection);
            else
            {
                _routeSelections.Add(fabName, new List<RouteSelection>());
                var lstRoute = RouteByFabName(fabName);
                lstRoute.Add(routeSelection);
            }
        }

        public void RemoveRouteSelection(string fabName, string fromBayName, string toBayName)
        {
            if (_routeSelections.ContainsKey(fabName))
            {
                foreach (RouteSelection rs in ModelManager.Instance.RouteSelections[fabName].ToList())
                {
                    if (rs.FromBay.Name == fromBayName && rs.ToBay.Name == toBayName)
                        ModelManager.Instance.RouteSelections[fabName].Remove(rs);
                }
            }
        }

        public bool IsSameRouteSelection(string fabName, string fromBayName, string toBayName, out RouteSelection routeSelection, out int no)
        {
            if (_routeSelections.ContainsKey(fabName))
            {
                for (int i = 0; i < _routeSelections[fabName].Count; i++)
                {
                    RouteSelection rs = _routeSelections[fabName][i];
                    if (rs.FromBayName == fromBayName && rs.ToBayName == toBayName)
                    {
                        routeSelection = rs;
                        no = i;
                        return true;
                    }
                }
            }

            routeSelection = null;
            no = -1;
            return false;
        }


        #region EvtSimLink
        public SimLink AddEvtSimLink(SimNode startNode, SimNode endNode)
        {
            SimLink evtSimLink = new SimLink(_evtLinkID, startNode, endNode);
            _evtSimLinks.Add(_evtLinkID, evtSimLink);
            _evtLinkID++;

            return evtSimLink;
        }
        #endregion

        #region SimLink
        public SimLink AddSimLink(SimNode startNode, SimNode endNode)
        {
            SimLink simLink = new SimLink(_linkID, startNode, endNode);
            _simLinks.Add(_linkID, simLink);
            _linkID++;

            startNode.AddOutLink(simLink);
            endNode.AddInLink(simLink);

            return simLink;
        }
        #endregion


        #endregion

        #endregion

        #region NetworkPoint & NetworkLine
        public NetworkPointNode CreateNetworkPoint(SimNode coreNode)
        {
            NetworkPointNode np = new NetworkPointNode(_networkPointID, coreNode);
            _networkPoints.Add(np);
            _networkPointID++;
            return np;
        }
        public void CreateNetworkLineStructure()
        {
            // Network에서는 분기가 2개 이상인 Node만 살아남는다. Network은 분기가 없으면 무조건 라인을 다 합치고 Node들은 계산에 쓰이지 않음.
            //NetworkPoint 생성
            foreach (RailPointNode fromPoint in DicRailPoint.Values)
            {
                if (fromPoint.ToLines.Count > 1)
                {
                    NetworkPointNode nwPoint = CreateNetworkPoint(fromPoint);
                    fromPoint.NWPoint = nwPoint;
//                    _networkPoints.Add(nwPoint);
                }
            }

            //NetworkPoint를 기준으로 NetworkLine 생성
            foreach (NetworkPointNode fromNetworkPoint in _networkPoints)
            {
                RailPointNode fromPoint = (RailPointNode)fromNetworkPoint.CoreNode;
                foreach (RailLineNode line in fromPoint.ToLines)
                {
                    double length = 0;
                    List<RailLineNode> railLines = new List<RailLineNode>();
                    RailLineNode lineTemp = line;
                    RailPointNode outPoint = line.ToNode;
                    railLines.Add(lineTemp);
                    length = lineTemp.Length;
                    outPoint = lineTemp.ToNode;

                    if (outPoint.ToLines.Count == 0)
                        continue;

                    lineTemp = outPoint.ToLines[0];

                    bool isAvailableNetworkLine = true;
                    // 분기되는 포인트가 나올 때까지 line을 합친다.
                    while (outPoint.ToLines.Count == 1)
                    {
                        if (ModelManager.Instance.RailCuts[line.FabName].Contains(lineTemp))
                            isAvailableNetworkLine = false;

                        railLines.Add(lineTemp);
                        length = length + lineTemp.Length;
                        outPoint = lineTemp.ToNode;
                        lineTemp = outPoint.ToLines[0];

                    }

                    //NetworkLine은 합친 RailLine들의 객체와 길이를 가진다.
                    NetworkLineNode nl = new NetworkLineNode(_networkLineID, length, line.FromNode.NWPoint, outPoint.NWPoint, railLines);

                    nl.IsAvailable = isAvailableNetworkLine;
                    _networkLines.Add(nl);
                    _networkLineID++;
                }
            }

            _simpleNetwork = new float[NWPoints.Count(), NWPoints.Count()];
        }

        public void UpdateNetworkState()
        {
            foreach(RailLineNode line in _railLineNodes.Values)
            {
                if (ModelManager.Instance.RailCuts[line.FabName].Contains(line))
                    line.TotalCost = float.MaxValue;
                //else
                //    ScriptScoreFunction(line);
            //    //고정 값의 weight
            //    float distanceWeight = 1;

            //    //상태 변화 값의 weight
            //    float idleOHTCountWeight = 15000;
            //    float workingOHTCountWeight = 15000;

            //    line.WorkingOHTCount = line.ListOHT.FindAll(oht => !(oht.NodeState is OHT_STATE.IDLE)).Count; // 라인 위의 OHT 중 상태가 Idle하지 않은 OHT만 수집
            //    line.IdleOHTCount = line.ListOHT.FindAll(oht => oht.NodeState is OHT_STATE.IDLE).Count; // 라인 위의 OHT 중 상태가 Idle한 OHT만 수집

            //    //3가지 파라미터와 Weight로 Weight-Sum Cost 산출
            //    line.TotalCost = (float)line.Length * distanceWeight
            //+ line.IdleOHTCount * idleOHTCountWeight
            //+ line.WorkingOHTCount * workingOHTCountWeight;
            }

            foreach(NetworkLineNode NWLine in lstNWLines)
            {
                _simpleNetwork[NWLine.FromNodeID, NWLine.ToNodeID] = NWLine.TotalCost;
            }

            SimpleDijkstra di = new SimpleDijkstra(ModelManager.Instance.SimpleNetwork, ModelManager.Instance.NWPoints.Count);
            _refNode = di.Dijkstra();
        }

        #endregion
    }
}
