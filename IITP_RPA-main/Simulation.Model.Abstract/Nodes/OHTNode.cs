using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;
using Simulation.Model.Abstract;

namespace Simulation.Model.Abstract
{
    [DefaultPropertyAttribute("OHT")]
    public class OHTNode : FabSimNode
    {
        #region Variables
        //-------------new---------------
        private Time _saveRestTime;
        private bool _isWorking;
        private RailPortNode _destinationRailPort;
        private RailPortNode _reservationRailPort;
        private SimNode _destination;
        private RailLineNode _curRailLine;
        private RailPortNode _curRailStation;
        private Bay _curBay;
        private Bay _bumpingBay;
        private List<RailLineNode> _lstRailLine;
        private List<RailLineNode> _lstPresentRailLine;
        private List<RailLineNode> _lstLastRailLine;
        private List<RailLineNode> _lstDispatchedRailLine;
        private List<RailLineNode> _candidateRoute;
        private double _candidateRouteDistance;
        private uint _targetEntityID;
        private string _targetEntityName;
        private SimPort _command;
        private Command _dispatchedCommand;
        private Time reservationTime;
        private string _zcuResetPointName;
        private ZCU _curZcu;
        private SimEntity _simEntity;

        private bool _isQueue;

        private Time _loadingTime;
        private Time _unloadingTime;

        private double _preLength;
        private List<NetworkPointNode> _lstNWPt;
        private List<double> _lstLength;
        private List<Vector3> _lstDirection;
        //private double _endLength;
        //private Vector3 _endDirection;

        private double _movingLength;

        private double _size;
        private double _curLength;
        private double _sumLength;
        private double _lastLength;
        private double _destLength;
        private bool _reticle;
        private bool _isInZCU;
        private List<ZCU> _zcus;

        private bool _isBumping;

        //KPI--------
        private List<Time> _lstUnloadMoveTime;
        private List<Time> _lstLoadMoveTime;
        private List<Time> _lstAssignTime;
        private int _numKPI;

        private List<Time> _lstIdleStartTime;
        private List<Time> _lstAssignStartTime;
        private List<Time> _lstLoadingCompleteTime;
        private List<Time> _lstUnloadingCompleteTime;

        private double _workLoad;
        //--------
        //FOR DEEP LEARNING=====
        private List<double> _lstDeepLength;
        private List<int> _lstDeepNumOHT;
        private List<int> _lstDeepNumRegProcess;
        private List<int> _lstDeepNumRegBuffer;
        private List<Time> _lstDeepDepartureTime;
        private List<Time> _lstDeepArriveTime;
        //======================

        private Time _checkTime;
        //private double _determinedLength;
        //private bool _isResevation;
        private SimPort _reservationPort;

        [Browsable(true)]
        [DisplayName("Name")]
        public string OHTName { get { return Name; } }

        [Browsable(true)]
        public string OHTState { get { return NodeStateName; } }

        [Browsable(false)]
        public bool _tempCrossReservation;

        [Browsable(false)]
        public Time SaveRestTime { get { return _saveRestTime; } }
        [Browsable(false)]
        public RailPortNode DestinationPort { get { return _destinationRailPort; } set { _destinationRailPort = value; } }
        [Browsable(false)]
        public RailPortNode ReservationPort { get { return _reservationRailPort; } set { _reservationRailPort = value; } }
        [Browsable(false)]
        public SimNode Destination { get { return _destination; } set { _destination = value; } }
        [Browsable(false)]
        public RailLineNode CurRailLine { get { return _curRailLine; } }
        [Browsable(false)]
        public Bay CurBay
        {
            get { return _curBay; }
            set { _curBay = value; }
        }
        [Browsable(false)]
        public RailPortNode CurRailStation
        {
            get { return _curRailStation; }
        }
        [Browsable(false)]
        public Bay BumpingBay
        {
            get { return _bumpingBay; }
            set { _bumpingBay = value; }
        }
        [Browsable(true)]
        [DisplayName("Bay")]
        public string CurBayName {
            get
            {
                if (_curBay != null)
                    return _curBay.Name;
                else
                    return string.Empty;
            }
        }
        [Browsable(true)]
        [DisplayName("Bumping Bay")]
        public string BumpingBayName { get { return _bumpingBay == null ? string.Empty : _bumpingBay.Name; } }
        [Browsable(false)]
        public List<RailLineNode> LstRailLine
        {
            get { return _lstRailLine; }
        }

        [Browsable(false)]
        public List<RailLineNode> LstPresentRailLine
        {
            get { return _lstPresentRailLine; }
        }

        [Browsable(false)]
        public List<RailLineNode> LstLastRailLine
        {
            get { return _lstLastRailLine; }
        }

        [Browsable(false)]
        public List<RailLineNode> LstDispatchedLine
        {
            get { return _lstDispatchedRailLine; }
        }

        [Browsable(false)]
        public List<RailLineNode> CandidateRoute
        {
            get { return _candidateRoute; }
            set { _candidateRoute = value; }
        }

        [Browsable(false)]
        public double CandidateRouteDistance
        {
            get { return _candidateRouteDistance; }
            set { _candidateRouteDistance = value; }
        }


        [Browsable(false)]
        public double RouteDistance
        {
            get { return _lstRailLine.Sum(x => x.Length); }
        }

        private double _ohtSpeed;
        [Browsable(false)]
        public double Speed
        {
            get { return _ohtSpeed; }
            set { _ohtSpeed = value; }
        }

        [Browsable(true)]
        [DisplayName("Max Speed(meter/min)")]
        public double MaxSpeed
        {
            get { return Math.Round(_ohtSpeed * 0.001 * 60, 3); }
        }

        [Browsable(false)]
        public uint TargetEntityID
        {
            get { return _targetEntityID; }
            set { _targetEntityID = value; }
        }
        [Browsable(false)]
        public string TargetEntityName
        {
            get { return _targetEntityName; }
            set { _targetEntityName = value; }
        }

        [Browsable(false)]
        public SimPort Command
        {
            get { return _command; }
            set { _command = value; }
        }
        [Browsable(true)]
        [DisplayName("Command")]
        public string CommandName
        {
            get 
            {
                if (_command != null)
                    return ((Command)_command).Name;
                else
                    return string.Empty;
            }
        }

        [Browsable(false)]
        public Command DispatchedCommand
        {
            get { return _dispatchedCommand; }
            set { _dispatchedCommand = value; }
        }

        [Browsable(true)]
        [DisplayName("Dispatched Command")]
        public string DispatchedCommandName
        {
            get
            {
                if (_dispatchedCommand != null)
                    return _dispatchedCommand.Name;
                else
                    return string.Empty;
            }
        }

        [Browsable(true)]
        public OHT_TYPE OhtType
        {
            get
            {
                if (_reticle)
                    return OHT_TYPE.RETICLE;
                else
                    return OHT_TYPE.NORMAL;
            }
        }


        [Browsable(false)]
        public Time LoadingTime
        {
            get { return _loadingTime; }
        }
        [Browsable(false)]
        public Time UnloadingTime
        {
            get { return _unloadingTime; }
        }

        [Browsable(false)]
        public bool IsWorking
        {
            get { return _isWorking; }
            set { _isWorking = value; }
        }

        [Browsable(true)]
        [DisplayName("In ZCU")]
        public bool IsInZCU
        {
            get { return _isInZCU; }
            set { _isInZCU = value; }
        }

        [Browsable(true)]
        [DisplayName("Is Bumping")]
        public bool IsBumping
        {
            get { return _isBumping; }
            set { _isBumping = value; }
        }

        [Browsable(false)]
        public List<ZCU> LstZCU
        {
            get { return _zcus; }
        }

        [Browsable(false)]
        public SimEntity Entity
        {
            get { return _simEntity; }
        }
        [Browsable(false)]
        public new double Size // OHT 실제 사이즈 및 최소 이격 거리를 합산한 사이즈. 이동 로직에 사용.
        {
            get { return ModelManager.Instance.OHTSize; }
        }
        [Browsable(false)]
        public List<NetworkPointNode> LstNWPt
        {
            get { return _lstNWPt; }
        }
        [Browsable(false)]
        public List<double> LstLength
        {
            get { return _lstLength; }
        }
        [Browsable(false)]
        public List<Vector3> LstDirection
        {
            get { return _lstDirection; }
        }



        [Browsable(false)]
        public uint CurRailLineID
        { get { return _curRailLine.ID; } }

        [Browsable(true)]
        [DisplayName("RailLine")]
        public string CurRailLineName
        {
            get { return _curRailLine.Name; }
        }

        [Browsable(true)]
        public double CurDistance
        {
            get { return _curRailLine.GetDistanceAtTime(this, SimEngine.Instance.TimeNow); }
            set { _curLength = value; }
        }

        [Browsable(false)]
        public double DestDistance
        {
            get
            {
                if (_destinationRailPort != null)
                    return _destinationRailPort.Distance;
                else
                    return 0;
            }
        }

        [Browsable(true)]
        public bool Reticle
        {
            get { return _reticle; }
            set { _reticle = value; }
        }
        //public bool IsReservation
        //{
        //    get { return _isResevation; }
        //}

        //KPI==============
        [Browsable(false)]
        public List<Time> LstIdleStartTime
        {
            get { return _lstIdleStartTime; }
        }
        [Browsable(false)]
        public List<Time> LstAssignStartTime
        {
            get { return _lstAssignStartTime; }
        }
        [Browsable(false)]
        public List<Time> LstLoadingCompleteTime
        {
            get { return _lstLoadingCompleteTime; }
        }
        [Browsable(false)]
        public List<Time> LstUnloadingCompleteTime
        {
            get { return _lstUnloadingCompleteTime; }
        }

        [Browsable(true)]
        public Time ReservationTime
        { 
            get 
            {
                if (reservationTime != null)
                    return reservationTime;
                else
                    return Time.Zero;
            }
            set { reservationTime = value; }
        }

        [Browsable(true)]
        public string ZcuResetPointName
        {
            get { return _zcuResetPointName; }
            set { _zcuResetPointName = value; }
        }

        public ZCU CurZcu
        {
            get { return _curZcu; }
            set { _curZcu = value; }
        }

        [Browsable(true)]
        public Time lastOHTPosEndTime
        {
            get
            {
                int ohtIdx = CurRailLine.ListOHT.IndexOf(this);
                List<OHTPosData> listOhtPosData = CurRailLine.LstOHTPosData[ohtIdx];

                if (listOhtPosData.Count == 0)
                    return CurRailLine.LstStartTime[ohtIdx];
                else
                    return listOhtPosData.Last()._endTime;
            }
        }
        [Browsable(true)]
        public Time lastOHTPosEndSpeed
        {
            get
            {
                int ohtIdx = CurRailLine.ListOHT.IndexOf(this);
                List<OHTPosData> listOhtPosData = CurRailLine.LstOHTPosData[ohtIdx];

                if (listOhtPosData.Count == 0)
                    return CurRailLine.LstStartSpeed[ohtIdx];
                else
                    return listOhtPosData.Last()._endSpeed;
            }
        }

        //=================
        #endregion
        public OHTNode(uint ID, string name, Fab fab)
            : base(ID, name, fab)
        {
            Initialize();
            _ohtSpeed = ModelManager.Instance.OHTSpeed;
        }

        public OHTNode(uint ID, string name, Fab fab, bool reticle)
    : base(ID, name, fab)
        {
            Initialize();
            _reticle = reticle;
            _ohtSpeed = ModelManager.Instance.OHTSpeed;
        }

        public OHTNode(uint ID, string name, Fab fab, bool reticle, RailLineNode railLine)
            : base(ID, name, fab)
        {
            Initialize();

            _reticle = reticle;
            _curRailLine = railLine;
            _curBay = null;
            _bumpingBay = null;
            _ohtSpeed = ModelManager.Instance.OHTSpeed;
        }

        public OHTNode(uint ID, string name, Fab fab, bool reticle, RailLineNode railLine, double speed)
    : base(ID, name, fab)
        {
            Initialize();

            _reticle = reticle;
            _curRailLine = railLine;
            _curBay = null;
            _bumpingBay = null;
            _ohtSpeed = speed;
        }

        private void Initialize()
        {
            _size = ModelManager.Instance.OHTSize;
            _curLength = 0;
            _lastLength = 0;
            _checkTime = 0;
            _preLength = 0;
            NodeState = OHT_STATE.IDLE;
            if (!ModelManager.Instance.ReadyOHTs.Contains(this))
                ModelManager.Instance.ReadyOHTs.Add(this);
            _loadingTime = ModelManager.Instance.OHTLoadingTime;
            _unloadingTime = ModelManager.Instance.OHTUnloadingTime;
            //_isResevation = false;
            _zcus = new List<ZCU>();

            _tempCrossReservation = false;
            _lstNWPt = new List<NetworkPointNode>();
            _lstLength = new List<double>();
            _lstRailLine = new List<RailLineNode>();
            _lstPresentRailLine = new List<RailLineNode>();
            _lstLastRailLine = new List<RailLineNode>();
            _lstUnloadMoveTime = new List<Time>();
            _lstLoadMoveTime = new List<Time>();
            _lstAssignTime = new List<Time>();
            _numKPI = 0;

            _lstIdleStartTime = new List<Time>();
            _lstAssignStartTime = new List<Time>();
            _lstLoadingCompleteTime = new List<Time>();
            _lstUnloadingCompleteTime = new List<Time>();

            _lstDeepLength = new List<double>();
            _lstDeepNumOHT = new List<int>();
            _lstDeepNumRegProcess = new List<int>();
            _lstDeepNumRegBuffer = new List<int>();
            _lstDeepArriveTime = new List<Time>();
            _lstDeepDepartureTime = new List<Time>();
            _zcuResetPointName = string.Empty;
        }


        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);
        }

        public void setCurRailLine(RailLineNode line) 
        {
            _curRailLine = line;
            Direction = line.Direction;
        }
        public void setCurBay(Bay bay)
        {
            if(_curBay != bay)
            {
                if(bay != null)
                    bay.TotalOHTs.Add(this);
                
                if(_curBay != null)
                    _curBay.TotalOHTs.Remove(this);
            }

            _curBay = bay;
        }
        public void SetCurDistance(double length) { _curLength = length; }
        public void SetRailStation(RailPortNode port) { _curRailStation = port; }
        public void SetLstRailLine(List<RailLineNode> lst, bool saveLastRoute = true) 
        {
            if (saveLastRoute)
                _lstLastRailLine = _lstRailLine;
            else
                _lstLastRailLine.Clear();

            _lstPresentRailLine = lst.ToList();
            _lstRailLine = lst.ToList(); 
        }

        public void AddBumpingBay()
        {
            Bay bumpingBay = CurBay; // oht의 BumpingBay는 현재 머무르고 있는 Bay

            if (bumpingBay == null || bumpingBay.NeighborBay.Count == 0 || bumpingBay.BumpingPorts.Count == 0)
                bumpingBay = ModelManager.Instance.GetNearestBay(Fab.Name, CurRailLine.ToNode.PosVec3);

            if (!bumpingBay.BumpingOHTs.Contains(this))
                bumpingBay.BumpingOHTs.Add(this); // BumpingBay의 BumpingOHT에 해당 oht 등록

            BumpingBay = bumpingBay;
            IsBumping = true;
        }

        public void SetLstDispatchedRailLine(List<RailLineNode> lst) { _lstDispatchedRailLine = lst; }

        public void InitializeLstRailLine()
        {
            List<RailLineNode> lst = new List<RailLineNode>();
            _lstRailLine = lst;
        }
        //-------------------------------

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {

        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch((EXT_PORT)port.PortType)
            {              
                case EXT_PORT.OHT_ARRIVE:
                    {
                        if(NodeState is OHT_STATE.MOVE_TO_UNLOAD) // unloading 할 차례
                        {
                            NodeState = OHT_STATE.UNLOADING;
                            SimPort sp = new SimPort(INT_PORT.OHT_UNLOADING);
                            EvtCalendar.AddEvent(simTime + _unloadingTime, this, sp);

//                            SimLog log =  new SimLog(simTime, simTime + _unloadingTime, this, ANIMATION.OHT_UNLOADING);
//                            simLogs.AddLog(log);
                        }
                        else if(NodeState is OHT_STATE.MOVE_TO_LOAD)// loading 할 차례
                        {
                            NodeState = OHT_STATE.LOADING;

                            //entity 요청
                            //                            SimEntity entity = Destination.RequestEntity(this);
                            SimEntity entity = Command.Entity;

                            SimPort sp = new SimPort(INT_PORT.OHT_LOADING, entity);
                            EvtCalendar.AddEvent(simTime + _loadingTime, this, sp);

//                            SimLog log = new SimLog(simTime, simTime + _loadingTime, this, ANIMATION.OHT_LOADING);
//                            simLogs.AddLog(log);
                        }
                        else if(NodeState is OHT_STATE.IDLE)
                        {
                            ProcessAfterOHTArrive(simTime, simLogs);
                        }
                    }
                    break;

                default:
                    break;
            }            
        }

        public void SetPathforIdleOHT(NetworkPointNode startNWPt)
        {
            //Random r = new Random();

            //int nw_end_idx = r.Next(0, ModelManager.Instance.NWPoints.Count() - 1);
            
            //while(true)
            //{
            //    RailPortNode rsn = ModelManager.Instance.NWPoints[(uint)nw_end_idx].CoreNode as RailPortNode;
            //    if (startNWPt.ID == ModelManager.Instance.NWPoints[(uint)nw_end_idx].ID || //시작과 끝 위치가 같은 경우
            //        !(ModelManager.Instance.NWPoints[(uint)nw_end_idx].CoreNode is RailPortNode) || //끝 위치가 railStation이 아닌 경우
            //        !(rsn.Line.Length - rsn.Line.IntervalLength > rsn.Length &&
            //        rsn.Line.IntervalLength < rsn.Length))//interval 안에 있는 경우
            //        nw_end_idx = r.Next(0, ModelManager.Instance.NWPoints.Count() - 1);
            //    else
            //        break;
            //}
            
            //List<NetworkPointNode> lst = Scheduler.Instance.GetShortestPathList((RailPortNode)startNWPt.CoreNode,
            //                                                                    (RailPortNode)ModelManager.Instance.NWPoints[(uint)nw_end_idx].CoreNode);

            //SetLstNWPt(lst);
            //_curLength = ((RailPortNode)startNWPt.CoreNode).Length;
            //_destLength = _curLength;//처음 세팅만..

            //((RailPortNode)startNWPt.CoreNode).initOHT(this);
        }
        public void ProcessAfterOHTArrive(Time simTime, SimHistory simLogs)
        {
            switch(ModelManager.Instance.SimType)
            {
                case SIMULATION_TYPE.MATERIAL_HANDLING:
                    Scheduler.Instance.LetIdleOHTGoToBay(this, simTime, simLogs);
                    Scheduler.Instance.AssignCommands(simTime, simLogs);
                    break;
                case SIMULATION_TYPE.PRODUCTION:
                    Scheduler.Instance.LetIdleOHTGoToBay(this, simTime, simLogs);
                    Scheduler.Instance.AssignCommands(simTime, simLogs);
                    break;
            }
        }

        public double GetDistanceToDestination()
        {
            double distance = 0;
            foreach (RailLineNode line in _lstRailLine)
            {
                distance = distance + line.Length;
            }

            return distance;
        }

        public void SetEntity(SimEntity entity, RailPortNode startNode = null)
        {
            _simEntity = entity;
        }


        public void ReservationNextDestination(SimPort port)
        {
            //다음 이동 예약
            //_isResevation = true;
            _reservationPort = port;
        }

        public void SetLstNWPt(List<NetworkPointNode> lst)
        {
            _lstNWPt = lst;
            _lstDirection = new List<Vector3>();
            _lstLength = new List<double>();

            for (int i = 1; i < _lstNWPt.Count(); i++)
            {
                Vector3 startVec = _lstNWPt[i - 1].CoreNode.PosVec3;
                Vector3 endVec = _lstNWPt[i].CoreNode.PosVec3;

                Vector3 vec3Direction = endVec - startVec;

                double len = Math.Sqrt(vec3Direction.X * vec3Direction.X + vec3Direction.Y * vec3Direction.Y);
                _lstDirection.Add(vec3Direction / len);
                _lstLength.Add(len);
            }
            SetRailStation(_lstNWPt[0].CoreNode as RailPortNode);

            _lstRailLine = new List<RailLineNode>();
            for(int i=0; i < _lstNWPt.Count(); i++) //RailLine 리스트 세팅
            {
                if(_lstNWPt[i].CoreNode is RailPortNode)
                {
                    if (_lstRailLine.Count() == 0)
                        _lstRailLine.Add(((RailPortNode)_lstNWPt[i].CoreNode).Line);
                    else
                        if(((RailPortNode)_lstNWPt[i].CoreNode).Line.ID != _lstRailLine[_lstRailLine.Count()-1].ID)
                        _lstRailLine.Add(((RailPortNode)_lstNWPt[i].CoreNode).Line);
                }
                else if (_lstNWPt[i].CoreNode is RailPointNode && i > 0)
                {
                    if(_lstNWPt[i].CoreNode is RailPointNode && _lstNWPt[i-1].CoreNode is RailPointNode)
                    {
                        for (int m = 0; m < ((RailPointNode)_lstNWPt[i].CoreNode).InLinks.Count(); m++)
                            for (int n = 0; n < ((RailPointNode)_lstNWPt[i - 1].CoreNode).OutLinks.Count(); n++)
                                if (((RailPointNode)_lstNWPt[i].CoreNode).InLinks[m].StartNode.ID == ((RailPointNode)_lstNWPt[i - 1].CoreNode).OutLinks[n].EndNode.ID)
                                    _lstRailLine.Add(((RailPointNode)_lstNWPt[i].CoreNode).InLinks[m].StartNode as RailLineNode);

                    }
                }
            }
        }

        public List<ZCU> GetFirstReservationZcus()
        {
            List<ZCU> firstReservationZcus = new List<ZCU>();
            foreach(ZCU zcu in _zcus)
            {
                if (zcu.IsFirstReservation(this))
                    firstReservationZcus.Add(zcu);
            }

            return firstReservationZcus;
        }
    }
}
    