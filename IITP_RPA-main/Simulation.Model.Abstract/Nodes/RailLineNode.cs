using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    public struct OHTPosData
    {
        public double _startSpeed; //woong1130
        public double _endSpeed; //woong1130
        public double _celerate; //woong1130 가속 or 감속
        public Time _startTime;
        public Time _endTime;
        public double _startPos;
        public double _endPos;

        public OHTPosData(double celerate, double startSpeed, Time startTime, Time endTime, double startPos, double endPos)
        {            
            _celerate = celerate;
            _startSpeed = startSpeed;
            _startTime = startTime;
            _endTime = endTime;
            _startPos = startPos;
            _endPos = endPos;

            _endSpeed = _startSpeed + _celerate * (double)(_endTime - _startTime);
        }
        public OHTPosData(double celerate, double startSpeed, Time startTime, double startPos, double endSpeed, Time endTime, double endPos)
        {
            _celerate = celerate;
            _startSpeed = startSpeed;
            _startTime = startTime;
            _startPos = startPos;
            _endTime = endTime;           
            _endPos = endPos;
            _endSpeed = endSpeed;
        }
    }
    public struct EvtData
    {
        public bool _isArrive;
        public Time _time;

        public EvtData(bool isArrive, Time time)
        {
            _isArrive = isArrive;
            _time = time;
        }
    }

    public class RailLineNode : FabSimNode
    {
        #region Variables
        //---------------------충돌 방지 위한 변수-----------------------
        private List<OHTNode> _lstOHT;
        private List<double> _lstStartPos;
        private List<Time> _lstStartTime;
        private List<double> _lstOHTStartSpeed;
        private List<List<OHTPosData>> _lstOHTPosData;

        private List<Time> _lstEvtTime;
        private List<SimPort> _lstEvtPort;
        //--------------------------------------------------------------
        private List<NetworkLineNode> _nwLines;
        private double _intervalLength;

        private List<RailPortNode> _lstRailPort;
        // private List<OHTNode> _lstOHT;
        // private List<double> _lstOHTLength;
        private Vector3 _direction;
        private double _length;
        private double _cross;
        private double _maxSpeed;
        private double _acceleration;//woong1130
        private double _deceleration; //woong1130
        private List<double> _lstFollowControlLength;//woong1130
        private List<double> _lstFollowControlSpeed;
        private bool _isFull;
        private Dictionary<string, ZONE_LINE_TYPE> _dicBay;
        private Bay _bay;
        private HID _hid;
        private ZCU _zcu;
        private bool _isCurve;
        private bool _left;
        private string _shortEdgeInfo;
        private List<RailLineNode> _shortEdges;
        private float _totalCost;
        private Dictionary<uint, uint> _dicReroutingCount; // key: ohtID, value: rerouting count

        private List<int> _lstRemove; //woong
        private int _count = 0; //woong
        private List<Event> _lstEvt; //woong


        private enum DestinationType { LINE_MID, LINE_END, BEHIND_OHT, QUEUE, ARRIVE } //라인 중간으로 가야함, 라인 끝으로 가야함, 앞의 OHT 뒤로 가야함, 막혀서 대기, 도착함

        [Browsable(false)]
        public Fab Fab { get { return base.Fab; } }

        [DisplayName("Name")]
        [Browsable(true)]
        public string RailLineName { get { return Name; } }

        [DisplayName("Fab")]
        [Browsable(true)]
        public string FabName { get { return Fab.Name; } }

        [Browsable(false)]
        public string NodeStateName { get { return base.NodeStateName; } }

        [Browsable(false)]
        public Enum NodeType { get { return base.NodeType; } }

        [DisplayName("Position")]
        [Browsable(true)]
        public Vector3 PosVec3 { get { return base.PosVec3; } set { base.PosVec3 = value; } }

        [Browsable(false)]
        public bool IsVisited { get; set; }

        [Browsable(true)]
        public bool IsFull
        {
            get { return _isFull; }
            set { _isFull = value; }
        }

        [Browsable(true)]
        public bool IsCurve
        {
            get { return _isCurve; }
            set { _isCurve = value; }
        }
        
        public bool Left
        {
            get { return _left; }
        }

        public Vector3 StartPoint
        {
            get { return InLinks[0].StartNode.PosVec3; }
        }
        public Vector3 EndPoint
        {
            get { return OutLinks[0].EndNode.PosVec3; }
        }
        [Browsable(false)]
        public double IntervalLength
        {
            get { return _intervalLength; }
        }
        [Browsable(true)]
        public double Length
        {
            get { return _length; }
            set { _length = value; }
        }

        [Browsable(false)]
        public double Cross
        {
            get { return _cross; }
            set { _cross = value; }
        }
        [Browsable(false)]
        public List<RailPortNode> DicRailPort
        {
            get { return _lstRailPort; }
        }
        [Browsable(true)]
        public Vector3 Direction
        {
            get { return _direction; }
        }
        [Browsable(false)]
        public Vector2 vec2Direction
        {
            get { return new Vector2(_direction.X, _direction.Y); }
        }
        [Browsable(false)]
        public RailPointNode FromNode
        {
            get { return InLinks[0].StartNode as RailPointNode; }
        }
        [Browsable(false)]
        public RailPointNode ToNode
        {
            get { return OutLinks[0].EndNode as RailPointNode; }
        }
        [Browsable(false)]
        public List<NetworkLineNode> NWLines
        {
            get { return _nwLines; }
            set { _nwLines = value; }
        }
        [Browsable(true)]
        public int OHTCount
        {
            get { return _lstOHT.Count; }
        }

        [Browsable(false)]
        public List<OHTNode> ListOHT
        {
            get { return _lstOHT; }
        }

        public List<List<OHTPosData>> LstOHTPosData
        {
            get { return _lstOHTPosData; }
        }

        public List<double> LstStartPos
        {
            get { return _lstStartPos; }
        }

        public List<Time> LstStartTime
        {
            get { return _lstStartTime; }
        }

        public List<double> LstStartSpeed
        {
            get { return _lstOHTStartSpeed; }
        }

        [Browsable(true)]
        [DisplayName("FromPoint Name")]
        public string FromPointName
        {
            get { return FromNode.Name; }
        }

        [Browsable(true)]
        [DisplayName("ToPoint Name")]
        public string ToPointName
        {
            get { return ToNode.Name; }
        }

        [Browsable(true)]
        [DisplayName("Max Speed")]
        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; }
        }

        [Browsable(false)]
        public Bay Bay
        {
            get { return _bay; }
            set { _bay = value; }
        }


        public string BayName
        {
            get
            {
                if (_bay == null)
                    return string.Empty;
                else
                    return _bay.Name;
            }
        }

        [Browsable(true)]
        public int BayInOutCount
        {
            get { return DicBay.Count; }
        }

        [Browsable(true)]
        public int BayStopResetCount
        {
            get { return DicBay.Count; }
        }

        [Browsable(false)]
        public Dictionary<string, ZONE_LINE_TYPE> DicBay
        {
            get { return _dicBay; }
            set { _dicBay = value; }
        }

        public HID Hid
        {
            get { return _hid; }
            set { _hid = value; }
        }

        public string HidName
        {
            get
            {
                if (_hid == null)
                    return string.Empty;
                else
                    return _hid.Name;
            }
            set
            {
                if (!ModelManager.Instance.Hids.ContainsKey(value))
                {
                    _hid = ModelManager.Instance.AddHid(Fab.Name, value);
                }
                else
                    _hid = ModelManager.Instance.Hids[value];
            }
        }

        [Browsable(false)]
        public ZCU Zcu
        {
            get { return _zcu; }
            set { _zcu = value; }
        }

        [Browsable(true)]
        public string ZcuName
        {
            get
            {
                if (_zcu != null)
                    return _zcu.Name;
                else
                    return string.Empty;
            }
        }

        [Browsable(false)]
        public List<RailLineNode> ShortEdges
        {
            get { return _shortEdges; }
            set { _shortEdges = value; }
        }

        [Browsable(true)]
        public string ShortEdgeInfo
        {
            get { return _shortEdgeInfo; }
            set { _shortEdgeInfo = value; }
        }

        public double OHtDelayTime
        {
            get
            {
                double ohtDelayTime = 0;
                foreach (List<OHTPosData> ohtPosData in LstOHTPosData)
                {
                    if (ohtPosData.Count > 1)
                    {
                        Time startTime = ohtPosData[0]._startTime;
                        Time endTime = ohtPosData.Last()._endTime;
                        ohtDelayTime = ohtDelayTime + (endTime - startTime).TotalSeconds;
                    }
                }

                return ohtDelayTime;
            }
        }

        //Fixed Parmeters(Spec)------------------------------
        public float DistancePerVelocity { get; set; }
        public int PortCount { get; set; }
        public int JoiningPointCount { get; set; }
        public int DivergingPointCount { get; set; }
        //-------------------------------------------

        //Variables(State)----------------------------------------
        public int IdleOHTCount { get; set; }
        public int WorkingOHTCount { get; set; }
        public int ReservationPortCount { get; set; }
        //---------------------------------------------

        public float TotalCost
        {
            get { return _totalCost; }
            set { _totalCost = value; }
        }

        public Dictionary<uint, uint> DicReroutingCount
        {
            get { return _dicReroutingCount; }
            set { _dicReroutingCount = value; }
        }

        public uint TotalReroutingCount
        {
            get
            {
                uint totalReroutingCount = 0;
                foreach (uint count in _dicReroutingCount.Values)
                {
                    totalReroutingCount = totalReroutingCount + count;
                }
                return totalReroutingCount;
            }
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

        public RailLineNode(uint ID, string name, Fab fab, RailPointNode startPtNode, RailPointNode endPtNode, double maxSpeed, double intervalLength = 2.3, bool isCurve = false)
            : base(ID, name, fab)
        {
            SimLink link = ModelManager.Instance.AddSimLink(startPtNode, this);

            link = ModelManager.Instance.AddSimLink(this, endPtNode);

            _maxSpeed = maxSpeed;
            _acceleration = ModelManager.Instance.OHTAcceleration;
            _deceleration = ModelManager.Instance.OHTDeceleration;
            _intervalLength = ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance;//intervalLength;

            //---------------------충돌 방지 위한 변수-----------------------
            _lstOHT = new List<OHTNode>();
            _lstStartPos = new List<double>();
            _lstStartTime = new List<Time>();
            _lstOHTStartSpeed = new List<double>();
            _lstOHTPosData = new List<List<OHTPosData>>();

            _lstEvtTime = new List<Time>();
            _lstEvtPort = new List<SimPort>();
            //--------------------------------------------------------------

            _lstRailPort = new List<RailPortNode>();

            SetDirection();
            _length = GetCross();
            _cross = _length;
            IsVisited = false;
            _isFull = false;
            _isCurve = isCurve;
            _shortEdgeInfo = string.Empty;
            _shortEdges = new List<RailLineNode>();
            _dicBay = new Dictionary<string, ZONE_LINE_TYPE>();

            _nwLines = new List<NetworkLineNode>();


            _lstRemove = new List<int>(); //woong
            _lstEvt = new List<Event>(); //woong
            _dicReroutingCount = new Dictionary<uint, uint>();

            initFollowControl();

            if (Length > 3000)
                _maxSpeed = 5000;
            else if (Length > 1000)
                _maxSpeed = 3000;
            else
                _maxSpeed = 3000;
        }

        public RailLineNode(uint ID, string name, Fab fab, RailPointNode startPtNode, RailPointNode endPtNode, double distance, double maxSpeed, double intervalLength = 2.3, bool isCurve = false)
    : base(ID, name, fab)
        {
            SimLink link = ModelManager.Instance.AddSimLink(startPtNode, this);

            link = ModelManager.Instance.AddSimLink(this, endPtNode);

            _maxSpeed = maxSpeed;
            _acceleration = ModelManager.Instance.OHTAcceleration;
            _deceleration = ModelManager.Instance.OHTDeceleration;
            _intervalLength = ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance;// intervalLength;

            //---------------------충돌 방지 위한 변수-----------------------
            _lstOHT = new List<OHTNode>();
            _lstStartPos = new List<double>();
            _lstStartTime = new List<Time>();
            _lstOHTStartSpeed = new List<double>();
            _lstOHTPosData = new List<List<OHTPosData>>();

            _lstEvtTime = new List<Time>();
            _lstEvtPort = new List<SimPort>();
            //--------------------------------------------------------------

            _lstRailPort = new List<RailPortNode>();

            SetDirection();
            _length = distance;
            _cross = GetCross();
            IsVisited = false;
            _isFull = false;
            _isCurve = isCurve;
            _shortEdgeInfo = string.Empty;
            _shortEdges = new List<RailLineNode>();
            _dicBay = new Dictionary<string, ZONE_LINE_TYPE>();

            _nwLines = new List<NetworkLineNode>();
            _lstRemove = new List<int>(); //woong
            _lstEvt = new List<Event>(); //woong
            _dicReroutingCount = new Dictionary<uint, uint>();

            initFollowControl();

            //if (Length > 3000)
            //    _maxSpeed = 3000;
            //else if (Length > 1000)
            //    _maxSpeed = 1500;
            //else
            //    _maxSpeed = 1000;
            if (Length > 3000)
                _maxSpeed = 5000;
            else if (Length > 1000)
                _maxSpeed = 3000;
            else
                _maxSpeed = 3000;
        }


        public RailLineNode(uint ID, string name, Fab fab, RailPointNode startPtNode, RailPointNode endPtNode, double maxSpeed, double intervalLength = 2.3, bool isCurve = false, bool left = true)
    : base(ID, name, fab)
        {
            SimLink link = ModelManager.Instance.AddSimLink(startPtNode, this);

            link = ModelManager.Instance.AddSimLink(this, endPtNode);

            _maxSpeed = maxSpeed;
            _acceleration = ModelManager.Instance.OHTAcceleration;
            _deceleration = ModelManager.Instance.OHTDeceleration;
            _intervalLength = ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance;// intervalLength;
            _left = left;

            //---------------------충돌 방지 위한 변수-----------------------
            _lstOHT = new List<OHTNode>();
            _lstStartPos = new List<double>();
            _lstStartTime = new List<Time>();
            _lstOHTStartSpeed = new List<double>();
            _lstOHTPosData = new List<List<OHTPosData>>();

            _lstEvtTime = new List<Time>();
            _lstEvtPort = new List<SimPort>();
            //--------------------------------------------------------------

            _lstRailPort = new List<RailPortNode>();

            SetDirection();
            _length = GetCross();
            _cross = GetCross();
            IsVisited = false;
            _isFull = false;
            _isCurve = isCurve;
            _shortEdgeInfo = string.Empty;
            _shortEdges = new List<RailLineNode>();
            _dicBay = new Dictionary<string, ZONE_LINE_TYPE>();

            _nwLines = new List<NetworkLineNode>();
            _lstRemove = new List<int>(); //woong
            _lstEvt = new List<Event>(); //woong
            _dicReroutingCount = new Dictionary<uint, uint>();

            initFollowControl();

            if (Length > 3000)
                _maxSpeed = 5000;
            else if (Length > 1000)
                _maxSpeed = 3000;
            else
                _maxSpeed = 3000;
        }
        public RailLineNode(uint ID, string name, Fab fab, RailPointNode startPtNode, RailPointNode endPtNode, double distance, double maxSpeed, double intervalLength = 2.3, bool isCurve = false, bool left = true)
    : base(ID, name, fab)
        {
            SimLink link = ModelManager.Instance.AddSimLink(startPtNode, this);

            link = ModelManager.Instance.AddSimLink(this, endPtNode);

            _maxSpeed = maxSpeed;
            _acceleration = ModelManager.Instance.OHTAcceleration;
            _deceleration = ModelManager.Instance.OHTDeceleration;
            _intervalLength = ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance;// intervalLength;
            _left = left;

            //---------------------충돌 방지 위한 변수-----------------------
            _lstOHT = new List<OHTNode>();
            _lstStartPos = new List<double>();
            _lstStartTime = new List<Time>();
            _lstOHTStartSpeed = new List<double>();
            _lstOHTPosData = new List<List<OHTPosData>>();

            _lstEvtTime = new List<Time>();
            _lstEvtPort = new List<SimPort>();
            //--------------------------------------------------------------

            _lstRailPort = new List<RailPortNode>();

            SetDirection();
            _cross = GetCross();
            _length = distance;
            IsVisited = false;
            _isFull = false;
            _isCurve = isCurve;
            _shortEdgeInfo = string.Empty;
            _shortEdges = new List<RailLineNode>();
            _dicBay = new Dictionary<string, ZONE_LINE_TYPE>();

            _nwLines = new List<NetworkLineNode>();
            _lstRemove = new List<int>(); //woong
            _lstEvt = new List<Event>(); //woong
            _dicReroutingCount = new Dictionary<uint, uint>();

            initFollowControl();

            if (Length > 3000)
                _maxSpeed = 5000;
            else if (Length > 1000)
                _maxSpeed = 3000;
            else
                _maxSpeed = 3000;
        }

        private void initFollowControl()
        {
            _lstFollowControlLength = new List<double>();
            _lstFollowControlSpeed = new List<double>();

            _lstFollowControlLength.Add(4252 + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(3193 + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(1890 + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(1365 + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(798  + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(383  + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(170  + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(43   + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(5 + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
            _lstFollowControlLength.Add(0    + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);

            _lstFollowControlSpeed.Add((300 * 1000) / 60);
            _lstFollowControlSpeed.Add((260 * 1000) / 60);
            _lstFollowControlSpeed.Add((200 * 1000) / 60);
            _lstFollowControlSpeed.Add((170 * 1000) / 60);
            _lstFollowControlSpeed.Add((130 * 1000) / 60);
            _lstFollowControlSpeed.Add((90 * 1000) / 60);
            _lstFollowControlSpeed.Add((60 * 1000) / 60);
            _lstFollowControlSpeed.Add((30 * 1000) / 60);
            _lstFollowControlSpeed.Add((10 * 1000) / 60);
            _lstFollowControlSpeed.Add(0);
        }
        private void SetDirection()
        {
            _direction = OutLinks[0].EndNode.PosVec3 - InLinks[0].StartNode.PosVec3;

            double unitValue = Math.Sqrt(_direction.X * _direction.X + _direction.Y * _direction.Y);
            _direction = new Vector3(_direction.X, _direction.Y, _direction.Z) / unitValue;
        }
        private double GetCross()
        {
            Vector3 startPos = InLinks[0].StartNode.PosVec3;
            Vector3 endPos = OutLinks[0].EndNode.PosVec3;

            return  Math.Sqrt((endPos.X - startPos.X) * (endPos.X - startPos.X) + (endPos.Y - startPos.Y) * (endPos.Y - startPos.Y) + (endPos.Z - startPos.Z) * (endPos.Z - startPos.Z));
        }
        public void AddRailPort(RailPortNode rp)
        {
            _lstRailPort.Add(rp);
        }

        public void OHTClear()
        {
            _lstOHT.Clear();
            _lstStartPos.Clear();
            _lstStartTime.Clear();
            _lstOHTStartSpeed.Clear();
            _lstOHTPosData.Clear();
        }

        public void initOHT(OHTNode ohtNode, double distance, Time initTime)
        {
            if (_lstOHT.Count() == 0)
            {
                _lstOHT.Add(ohtNode);
                _lstStartPos.Add(distance);
                _lstStartTime.Add(initTime);
                _lstOHTStartSpeed.Add(0);
                _lstOHTPosData.Add(new List<OHTPosData>());
                _dicReroutingCount.Add(ohtNode.ID, 0);
            }
            else
            {
                int idx = 0;
                bool isInsert = false;
                for (int i = 0; i < _lstStartPos.Count(); i++)
                {
                    if (_lstStartPos[i] < distance)
                    {
                        idx = i;
                        isInsert = true;
                        break;
                    }
                }
                if (isInsert)
                {
                    _lstOHT.Insert(idx, ohtNode);
                    _lstStartPos.Insert(idx, distance);
                    _lstStartTime.Insert(idx, initTime);
                    _lstOHTStartSpeed.Insert(idx, 0);
                    _lstOHTPosData.Insert(idx, new List<OHTPosData>());
                    _dicReroutingCount.Add(ohtNode.ID, 0);
                }
                else
                {

                    _lstOHT.Add(ohtNode);
                    _lstStartPos.Add(distance);
                    _lstStartTime.Add(initTime);
                    _lstOHTStartSpeed.Add(0);
                    _lstOHTPosData.Add(new List<OHTPosData>());
                    _dicReroutingCount.Add(ohtNode.ID, 0);
                }
            }
        }

            public void InitializeSpec()
            {
                TotalCost = float.MaxValue;

                //Fixed Parameters
                PortCount = 0;
                JoiningPointCount = 0;
                DivergingPointCount = 0;

                //State Parameters
                IdleOHTCount = 0;
                WorkingOHTCount = 0;
                ReservationPortCount = 0;

                DistancePerVelocity = Convert.ToSingle(Length / MaxSpeed);
                PortCount = DicRailPort.Count;
                JoiningPointCount = ToNode.FromLines.Count;
                DivergingPointCount = ToNode.ToLines.Count;
            }

        /// <summary>
        /// Line이 OHT를 받을 수 있는 시점을 내뱉는 함수
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>        
        public bool checkEnableEnter(double enterPosition, out Time t)
        {
            for (int i = 0; i < _lstOHTPosData[_lstOHT.Count() - 1].Count(); i++) //마지막 OHT의 스케줄 for문
            {
                OHTPosData ohtPosData = _lstOHTPosData[_lstOHT.Count() - 1][i];

                if(i == 0 && ohtPosData._startPos >= enterPosition)
                {
                    t = ohtPosData._startTime;
                    return true;
                }
                else if (ohtPosData._endPos >= enterPosition) //들어갈 수 있는 최소 길이보다 마지막 OHT 위치가 작으면, 못들어갈 때
                {
                    double distance = enterPosition - ohtPosData._startPos;
                    t = getTimeByLength(ohtPosData._startSpeed, ohtPosData._celerate, distance); //들어갈 수 있는 최소길이가 된 시간
                    return true;
                }
            }
            t = -1;
            return false;
        }

        public double CalDestPos(Time simTime, OHTNode oht, RailPortNode railPort) //ohtIdx 0부터 시작
        {
            int ohtIdx = _lstOHT.IndexOf(oht);
            double curPos = _lstStartPos[ohtIdx];
            if (_lstOHTPosData[ohtIdx].Count > 0)
                curPos = _lstOHTPosData[ohtIdx][_lstOHTPosData[ohtIdx].Count - 1]._endPos;

            List<RailPortNode> lstRailPort = _lstRailPort.ToList();
            //현재 라인에 목적지 있고 지나치지 않은 경우
            if (lstRailPort.Contains(railPort))
            {
                if(!(oht.NodeState is OHT_STATE.IDLE))
                {
                    RailLineNode stopLine = null;
                    double stopAvailableDistance = 0;
                    List<RailLineNode> listStoppingLine = new List<RailLineNode>();

                    GetStopAvailableDistance(simTime, oht, ref listStoppingLine, ref stopLine, ref stopAvailableDistance);

                    if (stopLine == null)
                        return Length;

                    if (Name == stopLine.Name && Math.Round(railPort.Distance, 4) >= Math.Round(stopAvailableDistance, 4))
                        return railPort.Distance;
                }
                else if(railPort.Distance > curPos)
                {
                    return railPort.Distance;
                }
            }
            //현재 라인 통과해야 하는 경우
            
            return Length;
        }
        
        //멈춰있는 앞 OHT 스케줄링 하는 함수        
        public OHTPosData cutOHTPosDataByTime(OHTPosData opd, double time)
        {
            double v0 = opd._startSpeed;
            double a = opd._celerate;
            double t = time;

            double endPos = v0 * t + 0.5 * a * t * t;
            double endSpeed = v0 + a * t;

            return new OHTPosData(a, v0, opd._startTime, opd._startPos, endSpeed, opd._startTime + t, opd._startPos + endPos);
        }
        public OHTPosData cutOHTPosDataByPos(OHTPosData opd, double pos)
        {
            double a = opd._celerate;
            double s = pos - opd._startPos;
            double v0 = opd._startSpeed;
            double v = Math.Sqrt(2 * a * s + v0 * v0);

            if (double.IsNaN(v))
                v = 0;

            double t = (2 * s) / (v + v0);

            return new OHTPosData(a, v0, opd._startTime, opd._startPos, v, opd._startTime + t, pos);
        }
        public List<OHTPosData> cutListOHTPosDataByTime(List<OHTPosData> lstOpd, double cutTime)
        {
            List < OHTPosData > returnList= new List<OHTPosData>();
            for(int i=0; i < lstOpd.Count; i++)
            {
                if (lstOpd[i]._endTime <= cutTime)
                    returnList.Add(lstOpd[i]);
                else
                {
                    if (lstOpd[i]._startTime < cutTime)
                    {
                        returnList.Add(cutOHTPosDataByTime(lstOpd[i], (double)(cutTime - lstOpd[i]._startTime)));
                        break;
                    }
                }
            }
            return returnList;
        }
        public List<OHTPosData> cutListOHTPosDataByPos(List<OHTPosData> lstOpd, double cutPos)
        {
            List<OHTPosData> returnList = new List<OHTPosData>();
            for (int i = 0; i < lstOpd.Count; i++)
            {
                if(Math.Round(lstOpd[i]._endPos, 3) <= Math.Round(cutPos, 3))
                    returnList.Add(lstOpd[i]);
                else
                {
                    if (lstOpd[i]._startPos < cutPos)
                    {
                        returnList.Add(cutOHTPosDataByPos(lstOpd[i], cutPos));
                        break;
                    }
                }
            }
            return returnList;
        }

        public double getLength_v0_a_t(double v0, double a, double t)
        {
            return v0 * t + 0.5 * a * t * t;
        }
        public double getLength_v_v0_a(double v, double v0, double a)
        {
            return (v * v - v0 * v0) / (2 * a);
        }
        public double getVelocity_v0_a_s(double v0, double a, double s)
        {
            return Math.Sqrt(2 * a * s + v0 * v0);
        }

        public double getVelocity0_v_a_s(double v, double a, double s)
        {
            return Math.Sqrt(v * v - 2 * a * s);
        }

        public double getVelocity_v0_a_t(double v0, double a, double t)
        {
            return v0 + a * t;
        }

        public double getTime_v_v0_a(double v, double v0, double a)
        {
            return (v - v0) / a;
        }
        public double getTimeAndLength_v0_a_V(double v0, double a, double v, out double length)
        {
            double t = (v - v0) / a;
            length = v0*t +0.5 * a * t * t;

            return (v - v0) / a;
        }
        public double getTime_v_s(double v, double s)
        {
            return s / v;
        }
        public double getTimeByLength(double v0, double a, double s)
        {
            return (Math.Sqrt(2*a*s + v0*v0) - v0) / a;
        }
        public double getLengthByTime(double v0, double t, double a)
        {
            return v0 * t + 0.5 * a * t * t;
        }
        public double getToNodeZCUReservationPos()
        {
            double v = 0;
            double v0 = MaxSpeed;
            double reservationPosition = Length - getLength_v_v0_a(v, v0, _deceleration);

            return reservationPosition;
        }
        public double getToToNodeZCUReservationPos(RailLineNode toLine)
        {
            double toLineStartVelocityForStopPoint = getVelocity0_v_a_s(0, _deceleration, toLine.Length);
            double toLineMaxStartVelocity = toLine.MaxSpeed > toLineStartVelocityForStopPoint ? toLineStartVelocityForStopPoint : toLine.MaxSpeed;
            double maxVelocity = toLineMaxStartVelocity > MaxSpeed ? MaxSpeed : toLineMaxStartVelocity;
            return Length - getLength_v_v0_a(maxVelocity, MaxSpeed, _deceleration);
        }
        
        public double getToToToNodeZCUReservationPos(RailLineNode toLine, RailLineNode totoLine)
        {
            double totoLineStartVelocityForStopPoint = getVelocity0_v_a_s(0, _deceleration, totoLine.Length);
            double totoLineMaxStartVelocity = totoLine.MaxSpeed > totoLineStartVelocityForStopPoint ? totoLineStartVelocityForStopPoint : totoLine.MaxSpeed;
            double toLineStartVelocityForStopPoint = getVelocity0_v_a_s(totoLineMaxStartVelocity, _deceleration, toLine.Length);
            double toLineMaxStartVelocity = toLine.MaxSpeed > toLineStartVelocityForStopPoint ? toLineStartVelocityForStopPoint : toLine.MaxSpeed;
            double maxVelocity = toLineMaxStartVelocity > MaxSpeed ? MaxSpeed : toLineMaxStartVelocity;
            return Length - getLength_v_v0_a(maxVelocity, MaxSpeed, _deceleration);
        }


        public double getNextLengthByFollowControl(double length)
        {
            int idx = 0;
            for (int i = 0; i < _lstFollowControlLength.Count; i++)
                if (length >= _lstFollowControlLength[i])
                {
                    idx = i;
                }
           return _lstFollowControlLength[idx];
        }

        //목적지 및 라인 최대속도 기준 EndSpeed 출력
        public double calEndSpeedCurAtDest(OHTNode oht, double destPos)
        {
            double endSpeed = 0;
            //목적지에서 속도 0인 경우
            if ((!(oht.NodeState is OHT_STATE.IDLE) && destPos < Length) || (destPos == Length && oht.LstRailLine.Count == 1))
                endSpeed = 0;
            //목적지가 현재 라인의 끝부분이고 다음 라인 최대속도 고려해야 하는 경우
            else if (oht.LstRailLine.Count == 2) //oht.LstRailLine.Count가 2인 경우
            {
                //다음 레일의 속도가 낮은 경우 
                endSpeed = this.MaxSpeed >= oht.LstRailLine[1].MaxSpeed ? oht.LstRailLine[1].MaxSpeed : this.MaxSpeed;
                endSpeed = endSpeed >= oht.Speed ? oht.Speed : endSpeed;
                //다음 레일의 dest를 계산

                //다음라인 최대속도에서 감속했을 때 정지할 수 있는 거리
                double toLineStoppingLength = getLength_v_v0_a(0, oht.LstRailLine[1].MaxSpeed, _deceleration);

                //다음라인 최대속도 감속 정지 거리가 다음 라인의 목적지Port 거리보다 먼 경우
                if (oht.DestinationPort.Distance < toLineStoppingLength)
                {
                    double curLineStoppingLength = toLineStoppingLength - oht.DestinationPort.Distance;
                    double tmpEndSpeed = getVelocity_v0_a_s(oht.LstRailLine[1].MaxSpeed, _deceleration, curLineStoppingLength);
                    //                    Double tmpEndSpeed = getMaxVelocityByLength(oht.DestinationPort.Distance + ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance);
                    endSpeed = endSpeed > tmpEndSpeed ? tmpEndSpeed : endSpeed;
                    return endSpeed;
                }
            }
            else if (oht.LstRailLine.Count == 3) // 다음다음라인 이상 가야할 때
            {
                endSpeed = this.MaxSpeed >= oht.LstRailLine[1].MaxSpeed ? oht.LstRailLine[1].MaxSpeed : this.MaxSpeed;
                endSpeed = endSpeed >= oht.Speed ? oht.Speed : endSpeed;

                double totoLineStartVelocityForPort = getVelocity0_v_a_s(0, _deceleration, oht.DestinationPort.Distance);
                double toLineStartVelocityForPort = getVelocity0_v_a_s(totoLineStartVelocityForPort, _deceleration, oht.LstRailLine[1].Length);

                double toLineStartVelocityForMaxSpeed = getVelocity0_v_a_s(oht.LstRailLine[2].MaxSpeed, _deceleration, oht.LstRailLine[1].Length);

                endSpeed = endSpeed > toLineStartVelocityForPort ? toLineStartVelocityForPort : endSpeed;
                endSpeed = endSpeed > toLineStartVelocityForMaxSpeed ? toLineStartVelocityForMaxSpeed : endSpeed;
                return endSpeed;
            }
            else if(oht.LstRailLine.Count > 3)
            {
                endSpeed = this.MaxSpeed >= oht.LstRailLine[1].MaxSpeed ? oht.LstRailLine[1].MaxSpeed : this.MaxSpeed;
                endSpeed = endSpeed >= oht.Speed ? oht.Speed : endSpeed;

                return endSpeed;
            }

            return endSpeed;
        }
        public double getAccelerateByVelocity(double velocity)
        {
            if (velocity <= 1333)
                return 0.3 * 9810;
            else if (velocity <= 3333)
                return 0.2 * 9810;
            else
                return 0.11 * 9810;
        }
        public double getMaxVelocityByLength(double tempMaxSpeed, double length)
        {           
            double returnVel = 0;
            for (int i=0; i < _lstFollowControlLength.Count; i++)
            {
                if (length > _lstFollowControlLength[i])
                {
                    returnVel = _lstFollowControlSpeed[i];
                    break;
                }                    
            }

            returnVel = returnVel <= MaxSpeed ? returnVel : MaxSpeed;
            returnVel = returnVel <= tempMaxSpeed ? returnVel : tempMaxSpeed;
            return returnVel;
        }        
        public double getChangeTimeUsing2OHT(double length, double v0A, double v0B, double aA, double aB)
        {            
            double infiniteNum = double.MaxValue;
            double minimumLength = 0.01;

            int idx = -1;
            for(int i=0; i < _lstFollowControlLength.Count; i++)
            {
                if(length > _lstFollowControlLength[i])
                {
                    idx = i;
                    break;
                }
            }

            //220 이하인 경우 error
            if(idx == -1)
            {
                double targetLength = _lstFollowControlLength[_lstFollowControlLength.Count - 1];
                targetLength += minimumLength;
                double A = 0.5 * (aB - aA);
                double B = v0B - v0A;
                double C = length - targetLength;

                double D = B * B - 4 * A * C;
                double t1, t2;
                if (A == 0)
                {
                    t1 = (targetLength - length) / B;
                    t1 = t1 < 0 ? infiniteNum : t1;
                    return double.IsNaN(t1) ? infiniteNum : t1;
                }
                else if (D < 0)
                    return infiniteNum;
                else
                {
                    t1 = (-B + Math.Sqrt(D)) / (2 * A);
                    t2 = (-B - Math.Sqrt(D)) / (2 * A);

                    t1 = t1 < 0 ? infiniteNum : t1;
                    t2 = t2 < 0 ? infiniteNum : t2;

                    if (t1 != infiniteNum || t2 != infiniteNum)
                    {
                        double returnTime = t1 <= t2 ? t1 : t2;

                        return double.IsNaN(returnTime) ? infiniteNum : returnTime;
                    }

                    else
                        return infiniteNum;
                }
            }                
            //최대 속력인 경우 (= 낮아지는 변경점만 고려하는 경우) 
            else if(idx == 0)
            {                
                double targetLength = _lstFollowControlLength[idx];
                targetLength -= minimumLength;
                double A = 0.5 * (aB - aA);
                double B = v0B - v0A;
                double C = length - targetLength;

                double D = B * B - 4 * A * C;
                double t1, t2;
                if (A == 0)
                {
                    t1 = (targetLength - length) / B;
                    t1 = t1 < 0 ? infiniteNum : t1;
                    return double.IsNaN(t1) ? infiniteNum : t1;
                }
                else if (D < 0)
                    return infiniteNum;
                else
                {
                    t1 = (-B + Math.Sqrt(D)) / (2 * A);
                    t2 = (-B - Math.Sqrt(D)) / (2 * A);

                    t1 = t1 < 0 ? infiniteNum : t1;
                    t2 = t2 < 0 ? infiniteNum : t2;

                    if (t1 != infiniteNum || t2 != infiniteNum)
                    {
                        double returnTime = t1 <= t2 ? t1 : t2;

                        return double.IsNaN(returnTime) ? infiniteNum : returnTime;
                    }
                        
                    else
                        return infiniteNum;
                }
            }
            //양방향 변경점 고려해야 하는 경우
            else
            {           
                double targetLength1 = _lstFollowControlLength[idx];
                targetLength1 -= minimumLength;
                //if(idx == _lstFollowControlLength.Count - 1)
                //    targetLength1 = ModelManager.Instance.OHTMinimumDistance;

                double targetLength2 = _lstFollowControlLength[idx - 1];
                //if (length == targetLength2)
                targetLength2 += minimumLength;

                double A = 0.5 * (aB - aA);
                double B = v0B - v0A;
                double C1 = length - targetLength1;
                double C2 = length - targetLength2;

                double D1 = B * B - 4 * A * C1;
                double D2 = B * B - 4 * A * C2;

                double t1, t2, t3, t4;
                
                if(A == 0)
                {
                    t1 = (targetLength1 - length) / B;
                    t1 = t1 < 0 ? infiniteNum : t1;
                    t2 = infiniteNum;
                }
                else if (D1 < 0)
                {
                    t1 = infiniteNum;
                    t2 = infiniteNum;
                }
                else
                {
                    t1 = (-B + Math.Sqrt(D1)) / (2 * A);
                    t2 = (-B - Math.Sqrt(D1)) / (2 * A);

                    t1 = t1 < 0 ? infiniteNum : t1;
                    t2 = t2 < 0 ? infiniteNum : t2;
                }

                if(A == 0)
                {
                    t3 = (targetLength2 - length) / B;
                    t3 = t3 < 0 ? infiniteNum : t3;
                    t4 = infiniteNum;
                }
                else if (D2 < 0)
                {
                    t3 = infiniteNum;
                    t4 = infiniteNum;
                }
                else
                {
                    t3 = (-B + Math.Sqrt(D2)) / (2 * A);
                    t4 = (-B - Math.Sqrt(D2)) / (2 * A);

                    t3 = t3 < 0 ? infiniteNum : t3;
                    t4 = t4 < 0 ? infiniteNum : t4;
                }
                
                double returnTime = t1 < t2 ? t1 : t2;
                returnTime = returnTime < t3 ? returnTime : t3;
                returnTime = returnTime < t4 ? returnTime : t4;



                return double.IsNaN(returnTime) ? infiniteNum : returnTime;
            }
        }
        //startTime과 opd._startTime보다 크거나 같아야 함, startTime은 opd._endTime 보다 같거나?. 적어야함.
        public void AddPosdata_simlogs_Opd(double tempMaxSpeed, Time startTime, double startSpeed, double startPos, double destPos, OHTPosData opd, double lengthForToLine, double endSpeed, ref List<OHTPosData> lstRef)
        {            
            if (startTime >= opd._endTime)
                return;
            else
            {
                Time withinTime = startTime - opd._startTime;
                double curNextOHTPos, curNextOHTVel;
                if (withinTime != 0)
                {
                    curNextOHTPos = opd._startPos + lengthForToLine + getLength_v0_a_t(opd._startSpeed, opd._celerate, (double)withinTime);
                    curNextOHTVel = getVelocity_v0_a_s(opd._startSpeed, opd._celerate, curNextOHTPos - opd._startPos - lengthForToLine);
                }
                else
                {
                    curNextOHTPos = opd._startPos + lengthForToLine;
                    curNextOHTVel = opd._startSpeed;
                }
                //oht간 거리 확인
                double length = curNextOHTPos - startPos;
                //최대 속도 확인
                double maxVelocity = getMaxVelocityByLength(tempMaxSpeed, length);

                //움직일 여지 없는 경우
                if(maxVelocity == 0)
                {
                    //움직임 시작되는 시점 으로 회귀! ==> startTime 변경, startSpeed = 0, startPos, destPos, Opd 그대로
                    //                    double changeTime = getChangeTimeUsing2OHT(length, 0, curNextOHTVel, 0, opd._celerate);

                    double changeTime = 0;
                    double changePos = 0;

                    if (startSpeed > 0)
                    {
                        changeTime = getTime_v_v0_a(0, startSpeed, _deceleration);
                        changePos = getLength_v0_a_t(startSpeed, _deceleration, changeTime);
                        lstRef.Add(new OHTPosData(_deceleration, startSpeed, startTime, startPos, 0, startTime + changeTime, startPos + changePos));
                    }
                    else
                    {
                        changeTime = getChangeTimeUsing2OHT(length, startSpeed, curNextOHTVel, 0, opd._celerate);
                        changePos = 0;

                        if (( opd._celerate == 0 && opd._startSpeed == 0) || changeTime == double.MaxValue)
                        {
                            lstRef.Add(new OHTPosData(0, 0, startTime, startPos, 0, opd._endTime, startPos));
                            return;
                        }
                        else 
                        {
                            lstRef.Add(new OHTPosData(0, 0, startTime, startPos, 0, startTime + changeTime, startPos));
                            return;
                        }
                    }

                    AddPosdata_simlogs_Opd(tempMaxSpeed, startTime + changeTime, 0, startPos + changePos, destPos, opd, lengthForToLine, endSpeed, ref lstRef);
                    return;
                }
                //움직일 여지 있는 경우
                else
                {
                    ////목적지 까지 방해 없이 갈 수 있는 경우
                    //if (destPos <= curNextOHTPos - (ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance))
                    //{
                    //    AddPosdata_simlogs(maxVelocity, startTime, startSpeed, startPos, endSpeed, destPos, ref lstRef);
                    //    return;
                    //}   
                    //목적지까지 방해 되는 경우
                    //else
                    //{
                        //현재 속도가 최대 속도인 경우
                        if (startSpeed == maxVelocity)
                        {
                            //현재 속도로 destPos 고려하여 가장 많이 갈 수 있는 거리 : dl, dv
                            // dl까지 등속도로 가는 시간 dt

                            //현재 속도로 등속운동 할때 변경 시점 확인
                            double changeTime = getChangeTimeUsing2OHT(length, startSpeed, curNextOHTVel, 0, opd._celerate);

                            //if(dt <= changeTime)
                            // dt까지 짜고.. 추가로 destPos
                            //else
                            double stopReadyLength = getLength_v_v0_a(endSpeed, startSpeed, _deceleration);
                            double stopReadyPos = destPos - stopReadyLength;
                            double stopReadyTime = getTime_v_s(startSpeed, stopReadyPos - startPos);
                            //opd._endTime 보다 같거나 낮은 경우
                            if (startTime + changeTime <= opd._endTime || startTime + stopReadyTime <= opd._endTime)
                            {
                                if (startSpeed > endSpeed && stopReadyTime <= changeTime)
                                {
                                    double inputEndPos = stopReadyPos;
                                    double inputEndTime = (double)startTime + stopReadyTime;
                                    lstRef.Add(new OHTPosData(0, startSpeed, startTime, startPos, startSpeed, inputEndTime, inputEndPos));
                                    lstRef.Add(new OHTPosData(_deceleration, startSpeed, inputEndTime, inputEndPos, endSpeed, inputEndTime + getTime_v_v0_a(endSpeed, startSpeed, _deceleration), destPos));
                                    return;
                                }
                                else
                                {
                                    //변경시점까지 스케줄링(등속도 스케줄링)
                                    double inputEndPos = startPos + startSpeed * changeTime;
                                    double inputEndTime = (double)startTime + changeTime;
                                    lstRef.Add(new OHTPosData(0, startSpeed, startTime, startPos, startSpeed, inputEndTime, inputEndPos));

                                    //dest 고려
                                    if (destPos <= inputEndPos)
                                        lstRef = cutListOHTPosDataByPos(lstRef, destPos);
                                    else //재귀                                  
                                        AddPosdata_simlogs_Opd(tempMaxSpeed, inputEndTime, startSpeed, inputEndPos, destPos, opd, lengthForToLine, endSpeed, ref lstRef);

                                    return;
                                }
                            }
                            else
                            {
                                //opd._endTime까지 스케줄링 (등속도 스케줄링)
                                double inputEndTime = (double)opd._endTime;
                                double inputEndPos = startPos + (inputEndTime - (double)(startTime)) * startSpeed;
                                lstRef.Add(new OHTPosData(0, startSpeed, startTime, startPos, startSpeed, inputEndTime, inputEndPos));

                                if (destPos <= inputEndPos)
                                    lstRef = cutListOHTPosDataByPos(lstRef, destPos);


                                //종료
                                return;
                            }
                        }
                        else//현재 속도와 최대 속도가 다른 경우
                        {
                            //가감속 결정
                            double celerate = _acceleration;
                            if (startSpeed > maxVelocity)
                                celerate = _deceleration;
                            else if (startSpeed == maxVelocity)
                                celerate = 0; //error

                            //변경 시점  & 최대 시점 구하기
                            double maxTime = getTime_v_v0_a(maxVelocity, startSpeed, celerate);
                            double changeTime = getChangeTimeUsing2OHT(length, startSpeed, curNextOHTVel, celerate, opd._celerate);
                            double maxLength = getLength_v_v0_a(maxVelocity, startSpeed, celerate);
                            double stopReadyLength = getLength_v_v0_a(endSpeed, maxVelocity, _deceleration);

                            if (double.IsNaN(changeTime))
                                ;
                            double limitTime = (double)opd._endTime;

                            if (maxTime + startTime >= limitTime && changeTime + startTime >= limitTime)
                            {
                                //limitTime 까지 스케줄링
                                double inputEndTime = (double)opd._endTime;
                                double inputEndPos = startPos + getLength_v0_a_t(startSpeed, celerate, (double)(inputEndTime - startTime));
                                double inputEndSpeed = startSpeed + celerate * (double)(opd._endTime - startTime);


                                if (endSpeed < maxVelocity && celerate != _deceleration && maxLength + stopReadyLength > destPos - startPos)
                                {
                                    double velocity = Math.Sqrt((_deceleration * (2*_acceleration*(destPos - startPos) + startSpeed*startSpeed) - _acceleration * endSpeed * endSpeed)/(_deceleration - _acceleration));
                                    double movingDistance = getLength_v_v0_a(velocity, startSpeed, _acceleration);
                                    double movingTime = getTimeByLength(startSpeed, _acceleration, movingDistance);
                                    if (startTime + movingTime < limitTime)
                                    {
                                        inputEndSpeed = velocity;
                                        inputEndPos = startPos + movingDistance;
                                        inputEndTime = (double)startTime + movingTime;
                                        lstRef.Add(new OHTPosData(_acceleration, startSpeed, startTime, startPos, inputEndSpeed, inputEndTime, inputEndPos));
                                        lstRef.Add(new OHTPosData(_deceleration, inputEndSpeed, inputEndTime, inputEndPos, endSpeed, inputEndTime + getTime_v_v0_a(endSpeed, inputEndSpeed, _deceleration), destPos));
                                        return;
                                    }
                                }

                                lstRef.Add(new OHTPosData(celerate, startSpeed, startTime, startPos, inputEndSpeed, inputEndTime, inputEndPos));

                                //dest 고려
                                if (destPos <= inputEndPos)
                                    lstRef = cutListOHTPosDataByPos(lstRef, destPos);
                                //종료
                                return;
                            }
                            else
                            {
                                double inputEndTime;
                                if (maxTime <= changeTime) //maxTime 까지 스케줄링 
                                    inputEndTime = maxTime;
                                else //changeTime 까지 스케줄링
                                    inputEndTime = changeTime;

                                double inputEndPos = startPos + getLength_v0_a_t(startSpeed, celerate, (double)(inputEndTime));
                                double inputEndSpeed = startSpeed + celerate * (double)(inputEndTime);

                                if (_maxSpeed > endSpeed  && celerate != _deceleration && maxLength + stopReadyLength > destPos - startPos)
                                {
                                    double velocity = Math.Sqrt((_deceleration * (2 * _acceleration * (destPos - startPos) + startSpeed * startSpeed) - _acceleration * endSpeed * endSpeed) / (_deceleration - _acceleration));
                                    double movingDistance = getLength_v_v0_a(velocity, startSpeed, _acceleration);
                                    double movingTime =  getTimeByLength(startSpeed, _acceleration, movingDistance);
                                    if (movingTime < inputEndTime)
                                    {
                                        inputEndSpeed = velocity;
                                        inputEndPos = startPos + movingDistance;
                                        inputEndTime = (double)startTime + movingTime;
                                        lstRef.Add(new OHTPosData(_acceleration, startSpeed, startTime, startPos, inputEndSpeed, inputEndTime, inputEndPos));
                                        lstRef.Add(new OHTPosData(_deceleration, inputEndSpeed, inputEndTime, inputEndPos, endSpeed, inputEndTime + getTime_v_v0_a(endSpeed, inputEndSpeed, _deceleration), destPos));
                                        return;
                                    }
                                }


                                lstRef.Add(new OHTPosData(celerate, startSpeed, startTime, startPos, inputEndSpeed, startTime + inputEndTime, inputEndPos));

                                //dest 고려
                                if (destPos <= inputEndPos)
                                    lstRef = cutListOHTPosDataByPos(lstRef, destPos);
                                else//회귀
                                    AddPosdata_simlogs_Opd(tempMaxSpeed, (double)(startTime + (double)inputEndTime), inputEndSpeed, inputEndPos, destPos, opd, lengthForToLine, endSpeed, ref lstRef);

                                return;
                            }
                        }
                    //}                    
                }
            }
        }
        public void AddPosdata_simlogs_limitTime(double tempMaxSpeed, Time startTime, double startSpeed, double startPos, double endPos, double limitTime, double destPos, double endSpeed, ref List<OHTPosData> lstRef)
        {
            //oht간 거리 확인
            double length = endPos - startPos;
            //최대 속도 확인
            double maxVelocity = getMaxVelocityByLength(tempMaxSpeed, length);

            if (length == IntervalLength)
                return;
            //움직일 여력 없는 경우
            else if(maxVelocity == 0)
            {
                if(startSpeed != 0)
                {
                    AddPosdata_simlogs(0, startTime, startSpeed, startPos, 0, destPos, ref lstRef);
                    lstRef = cutListOHTPosDataByTime(lstRef, limitTime);
                }
                return;
            }
            //스케줄 계획할 여지 있는 경우
            else
            {
                //목적지에 방해없이 갈 수 있는 경우
                if(destPos + getLength_v_v0_a(0, endSpeed, _deceleration) + _intervalLength <= endPos)
                {
                    AddPosdata_simlogs(maxVelocity, startTime, startSpeed, startPos, endSpeed, destPos, ref lstRef);
                    return;
                }
                //목적지까지 갈 수 없는 경우
                else
                {
                    //앞에 멈춰있는 OHT의 위치 전까지 멈춘다는 스케줄
                    AddPosdata_simlogs(maxVelocity, startTime, startSpeed, startPos, 0, endPos - (ModelManager.Instance.OHTSize + ModelManager.Instance.OHTMinimumDistance), ref lstRef);
                    //스케줄 된 lstTmpOpd는 limitTime까지 스케줄을 잘라야 함
                    lstRef = cutListOHTPosDataByTime(lstRef, limitTime);
                    return;
                }
            }
        }
        public void AddPosdata_simlogs(double maxSpeed, Time startTime, double startSpeed, double startPos, double endSpeed, double endPos, ref List<OHTPosData> lstRef)
        {
            double diffLength = endPos - startPos;
            if (diffLength == 0)
                return;
            //startSpeed가 maxSpeed 초과하는 경우
            if(startSpeed > maxSpeed)
            {
                if (endSpeed > maxSpeed)
                    endSpeed = maxSpeed;

                double deceleration_length_1 = getLength_v_v0_a(maxSpeed, startSpeed, _deceleration);
                double const_length = 0;
                double deceleration_length_2 = 0;

                if (diffLength <= deceleration_length_1)
                {
                    deceleration_length_1 = diffLength;
                    //1감속 구간 정의
                }
                else
                {
                    diffLength -= deceleration_length_1;
                    deceleration_length_2 = getLength_v_v0_a(endSpeed, maxSpeed, _deceleration);
                    if (diffLength <= deceleration_length_2)
                        deceleration_length_2 = diffLength;
                    else
                        const_length = diffLength - deceleration_length_2;
                }


                if(const_length == 0)
                {
                    //startSpeed로 diffLength를 스케줄링 끝
                    double input_endSpeed = getVelocity_v0_a_s(startSpeed, _deceleration, deceleration_length_1);
                    double movingTime = getTime_v_v0_a(input_endSpeed, startSpeed, _deceleration);

                    lstRef.Add(new OHTPosData(_deceleration, startSpeed, startTime, startPos, input_endSpeed, startTime + movingTime, startPos + deceleration_length_1));
                }
                else
                {
                    double input_startSpeed = startSpeed;
                    double input_startTime = (double)startTime;
                    double input_startPos = startPos;

                    double input_endSpeed = 0;
                    double input_endTime = 0;
                    double input_endPos = 0;

                    if (deceleration_length_1 > 0)
                    {
                        //감속 스케줄링
                        input_endSpeed = getVelocity_v0_a_s(startSpeed, _deceleration, deceleration_length_1);
                        if (input_endSpeed < 0.0001)
                            input_endSpeed = 0;

                        input_endTime  = input_startTime + getTime_v_v0_a(input_endSpeed, startSpeed, _deceleration);
                        input_endPos   = input_startPos + deceleration_length_1;

                        if (!double.IsNaN(input_endSpeed))
                        {
                            lstRef.Add(new OHTPosData(_deceleration, input_startSpeed, input_startTime, input_startPos, input_endSpeed, input_endTime, input_endPos));

                            input_startSpeed = input_endSpeed;
                            input_startTime = input_endTime;
                            input_startPos = input_endPos;
                        }
                    }
                    if(const_length > 0 &&  input_endSpeed != 0)
                    {
                        input_endSpeed = maxSpeed;
                        input_endTime = input_startTime + const_length / input_endSpeed;
                        input_endPos = input_startPos + (input_endSpeed*(input_endTime - input_startTime));

                        //등속 스케줄링
                        lstRef.Add(new OHTPosData(0, input_startSpeed, input_startTime, input_startPos, input_endSpeed, input_endTime, input_endPos));

                        input_startSpeed = input_endSpeed;
                        input_startTime = input_endTime;
                        input_startPos = input_endPos;
                    }
                    if(deceleration_length_2 > 0)
                    {
                        //감속 스케줄링
                        input_endSpeed =  double.IsNaN(getVelocity_v0_a_s(input_startSpeed, _deceleration, deceleration_length_2)) ? 0 : getVelocity_v0_a_s(input_startSpeed, _deceleration, deceleration_length_2);
                        input_endTime = input_startTime + getTime_v_v0_a(input_endSpeed, input_startSpeed, _deceleration);
                        input_endPos = endPos;// input_startPos + deceleration_length_2;

                        lstRef.Add(new OHTPosData(_deceleration, input_startSpeed, input_startTime, input_startPos, input_endSpeed, input_endTime, input_endPos));
                    }
                }
                return;
            }
            else
            {
                double arrivalVelocity = getVelocity_v0_a_s(startSpeed, _acceleration, diffLength);
                if (endSpeed > arrivalVelocity)
                    endSpeed = arrivalVelocity;

                if (endSpeed > maxSpeed)
                    endSpeed = maxSpeed;

                //startSpeed가 maxSpeed 같거나 작다., 가속 충분 거리
                double acceleration_length = getLength_v_v0_a(maxSpeed, startSpeed, _acceleration);
                //endSpeed가 maxSpeed 같거나 작다., 감속 충분 거리
                double deceleration_length = getLength_v_v0_a(endSpeed, maxSpeed, _deceleration);

                //등속 구간이 존재하는지 확인

                double const_velocity_length = 0;
                double common_max_speed = maxSpeed;
                //등속도 구간 길이가 0이상인 경우
                if (acceleration_length + deceleration_length <= diffLength)
                    const_velocity_length = diffLength - (acceleration_length + deceleration_length);
                //등속도 구간 길이가 0미만인 경우... 가속 감속 합의점 찾아야 하는 경우
                else
                {
                    double vA = Math.Sqrt((2 * diffLength * _acceleration * _deceleration + _deceleration * startSpeed * startSpeed - _acceleration * endSpeed * endSpeed) / (_deceleration - _acceleration));
                    //추후 식 다시 확인해보자.!
                    acceleration_length = getLength_v_v0_a(vA, startSpeed, _acceleration);
                    deceleration_length = diffLength - acceleration_length;

                    common_max_speed = getVelocity_v0_a_s(startSpeed, _acceleration, acceleration_length);
                }
                double const_velocity_time = const_velocity_length / common_max_speed;
                double acceleration_time = getTime_v_v0_a(common_max_speed, startSpeed, _acceleration);
                double deceleration_time = getTime_v_v0_a(endSpeed, common_max_speed, _deceleration);

                double inputEndTime = (double)startTime;
                double inputEndPos = startPos;
                //가속             
                if (acceleration_length > 0)
                {
                    inputEndTime += acceleration_time;
                    inputEndPos += acceleration_length;
                    lstRef.Add(new OHTPosData(_acceleration, startSpeed, startTime, startPos, common_max_speed, inputEndTime, inputEndPos));
                }

                //등속
                if (const_velocity_length > 0)
                {
                    startTime = inputEndTime;
                    inputEndTime += const_velocity_time;
                    startPos = inputEndPos;
                    inputEndPos += const_velocity_length;
                    lstRef.Add(new OHTPosData(0, common_max_speed, startTime, startPos, common_max_speed, inputEndTime, inputEndPos));
                }

                //감속
                if (deceleration_length > 0)
                {
                    startTime = inputEndTime;
                    inputEndTime += deceleration_time;
                    startPos = inputEndPos;
                    lstRef.Add(new OHTPosData(_deceleration, common_max_speed, startTime, startPos, endSpeed, inputEndTime, startPos + deceleration_length));
                }
                return;
            }
        }

        public EvtData finishScheduling(SimHistory simLogs, OHTNode oht, int ohtIdx, List<OHTPosData> lst, double destPos, Time lastTime, List<OHTPosData> lstOpd)
        {
            if(lst.Count == 0)
                return new EvtData(false, -1);

            bool isArrive = false;
            //도착지에 도착 한 경우
            if (Math.Round(lst.Last()._endPos,3) >= Math.Round(destPos,3))
                isArrive = true;

            List<OHTPosData> lstSum = new List<OHTPosData>();

            if (lst.Count == 1)
                lstSum = lst;
            else
            {
                for (int i = 0; i < lst.Count-1;)
                {
                    OHTPosData opdStd = lst[i];
                    if(Math.Round(opdStd._endPos, 3) >= Math.Round(destPos, 3))
                        opdStd._endPos = destPos; 

                    OHTPosData opdNew = new OHTPosData(opdStd._celerate, opdStd._startSpeed, opdStd._startTime, opdStd._startPos, opdStd._endSpeed, opdStd._endTime, opdStd._endPos);

                    for (int j = i + 1; j < lst.Count; j++)
                    {
                        OHTPosData opdComp = lst[j];
                        if (Math.Round(opdComp._endPos, 3) >= Math.Round(destPos, 3))
                            opdComp._endPos = destPos;

                        if (opdStd._celerate == opdComp._celerate)
                        {
                            opdNew._endPos = opdComp._endPos;
                            opdNew._endSpeed = opdComp._endSpeed;
                            opdNew._endTime = opdComp._endTime;
                        }
                        else
                        {
                            lstSum.Add(opdNew);
                            i = j;
                        }

                        if (j == lst.Count - 1)
                        {
                            if (opdStd._celerate == opdComp._celerate)
                            {
                                lstSum.Add(opdNew);
                                i = j;
                            }
                            else
                            {
                                lstSum.Add(opdComp);
                                i = j;
                            }
                        }

                        if (i == j)
                            break;
                    }
                }
            }

            for (int i = 0; i < lstSum.Count; i++)
            {
                _lstOHTPosData[ohtIdx].Add(lstSum[i]);
                if (SimEngine.Instance.IsAnimation)
                    simLogs.AddLog(new SimLog(lstSum[i]._startTime, lstSum[i]._startSpeed, lstSum[i]._startPos, lstSum[i]._endTime, lstSum[i]._celerate, vec2Direction, oht, this, ANIMATION.OHT_MOVE_ACC));
            }

            Time inputEndTime = lastTime;
            if (lstOpd.Count > 0)
                inputEndTime = lstOpd[lstOpd.Count - 1]._endTime;

            return new EvtData(isArrive, inputEndTime);
        }
        public EvtData GenerateOHTPosData(SimHistory simLogs, Time curTime, int ohtIdx, double destPos)
        {
            //Console.WriteLine(ohtIdx + ": " + curTime);
            OHTNode oht = _lstOHT[ohtIdx];

            List<OHTPosData> lstOpd = _lstOHTPosData[ohtIdx];
            double lastPos = _lstStartPos[ohtIdx];
            Time lastTime = _lstStartTime[ohtIdx];

            if (lastTime < curTime) //이상상황
                lastTime = curTime;

            //가장 최근의 OHT 상황을 확인.
            double lastSpeed = _lstOHTStartSpeed[ohtIdx];
            if (lstOpd.Count > 0)
            {
                lastPos = lstOpd[lstOpd.Count - 1]._endPos;
                lastSpeed = lstOpd[lstOpd.Count - 1]._endSpeed;
                if (lastTime < lstOpd[lstOpd.Count - 1]._endTime)
                    lastTime = lstOpd[lstOpd.Count - 1]._endTime;

                if (lastSpeed == 0 && lastTime < curTime)
                    lastTime = curTime;
            }

            //-----------------------------------------------------------------    

            List<List<OHTPosData>> lstlstPreOpd = new List<List<OHTPosData>>();
            List<double> lstLengthForToLine = new List<double>();
            List<double> lstPreOhtStartPosition = new List<double>();

            //라인의 가장 뒤쪽에 있는 경우 다음 라인의 OHT 상황을 파악하기 위한 정보를 수집. 내 경로가 아닌 갈라지는 라인에 대해서도 고려(충돌 방지).
            if (ohtIdx == 0)
            {
                foreach (RailLineNode toLine in ToNode.ToLines)
                {
                    //다음 라인에 oht 있는 경우
                    if (toLine.ListOHT.Count > 0)
                    {
                        //진행경로가 아닌데 intervalLength보다 더 가는 계획이 있으면 경로짜는 oht가 지나가면서 충돌에 영향을 주지않는다고 판단하고 고려하지 않음.
                        if (!oht.LstRailLine.Contains(toLine) 
                            && 
                            ((toLine.LstOHTPosData[toLine.ListOHT.Count - 1].Count > 0 && toLine.LstOHTPosData[toLine.ListOHT.Count - 1].Last()._endPos > _intervalLength)
                            || (toLine.LstOHTPosData[toLine.ListOHT.Count - 1].Count == 0 && toLine._lstStartPos[toLine.ListOHT.Count - 1] > _intervalLength))
                            )
                            continue;

                        lstlstPreOpd.Add(toLine.LstOHTPosData[toLine.ListOHT.Count - 1].ToList());
                        lstPreOhtStartPosition.Add(toLine.LstStartPos[toLine.ListOHT.Count - 1]);
                        lstLengthForToLine.Add(Length);
                    }
                    //다음 라인에 oht 없는 경우
                    else
                    {
                        foreach(RailLineNode totoLine in toLine.ToNode.ToLines)
                        {
                            //totoLine에 아무도 없으면 continue;
                            if (totoLine.OHTCount == 0)
                                continue;

                            // 진행경로가 아닌데 intervalLength보다 더 가는 계획이 있으면 경로짜는 oht가 지나가면서 충돌에 영향을 주지않는다고 판단하고 고려하지 않음.
                            if ( !oht.LstRailLine.Contains(totoLine) && 
                                (totoLine.LstOHTPosData[totoLine.ListOHT.Count - 1].Count > 0 && toLine.Length + totoLine.LstOHTPosData[totoLine.ListOHT.Count - 1].Last()._endPos > _intervalLength)
                                || (totoLine.LstOHTPosData[totoLine.ListOHT.Count - 1].Count == 0 && toLine.Length + totoLine.LstStartPos[totoLine.ListOHT.Count - 1] > _intervalLength))
                                continue;

//                            //다다음 라인에 oht 있는 경우
//                            if ( oht.LstRailLine.Count > 2 && oht.LstRailLine[2].Name == totoLine.Name)
//                            {
                                lstlstPreOpd.Add(totoLine.LstOHTPosData[totoLine.ListOHT.Count - 1].ToList());
                                lstPreOhtStartPosition.Add(totoLine.LstStartPos[totoLine.ListOHT.Count - 1]);
                                lstLengthForToLine.Add(Length + toLine.Length);
//                            }
                        }
                    }
                }
            }
            else//라인의 가장 뒤쪽이 아닌 경우
            {
                lstlstPreOpd.Add(_lstOHTPosData[ohtIdx - 1]);
                lstPreOhtStartPosition.Add(_lstStartPos[ohtIdx - 1]);
                lstLengthForToLine.Add(0);
            }

            //목적지 및 라인 최대속도 기준으로 도착 속도 결정
            double endSpeed = calEndSpeedCurAtDest(oht, destPos);

            //예약지점까지만 계획하기 위해서 CutPos 결정, 예약 지점 이후인 경우 예약 우선순위에 의해 endSpeed 결정
            double cutPos = Length;

            GetEndSpeedNCutPos(oht, lastPos, ref endSpeed, ref cutPos);

            // lstPreOpd == null : 앞에 oht 없는 경우(다음라인, 다다음라인 포함)           
            if (lstlstPreOpd.Count == 0)
            {
                //스케줄링!!!!  
                List<OHTPosData> lst = new List<OHTPosData>();

                AddPosdata_simlogs(_maxSpeed, lastTime, lastSpeed, lastPos, endSpeed, destPos, ref lst);
                //cut 추가
                lst = cutListOHTPosDataByPos(lst, cutPos);

                if (lst.Count > 0 && lst.Last()._endTime > 200000)
                    ;
                if (lst.Count > 0 && lst.Last()._startSpeed < 0.00001 && lst.Last()._startSpeed != 0 && lst.Last()._endSpeed < 0.00001 && lst.Last()._endSpeed != 0)
                    ;
                if (lst.Count > 0 && lst.Last()._startSpeed > 0.1 && lst.Last()._startSpeed != 0 && lst.Last()._endSpeed < 0.00001 && lst.Last()._endSpeed != 0)
                    ;

                return finishScheduling(simLogs, oht, ohtIdx, lst, destPos, lastTime, lstOpd);
            }
            //고려해야할 앞 oht가 존재하는 경우(고려해야할 oht가 다음라인 혹인 다다음 라인에 존재할 수 있음)
            else
            {
                List<OHTPosData> lstBetter = null;
                //라스트 시점에서 이미 끝나버린 opd 무시

                for (int i = 0; i < lstlstPreOpd.Count; i++)
                {
                    List<OHTPosData> lstPreOpd = lstlstPreOpd[i];
                    double lengthForToLine = lstLengthForToLine[i];
                    double preOhtStartPosition = lstPreOhtStartPosition[i];
                    List<OHTPosData> lst = new List<OHTPosData>();

                    double nextOHTPos = lengthForToLine + preOhtStartPosition;
                    List<OHTPosData> lstNewPreOpd = new List<OHTPosData>();
                    for (int j = 0; j < lstPreOpd.Count(); j++)
                    {
                        if (lastTime >= lstPreOpd[j]._endTime)
                            nextOHTPos = lengthForToLine + lstPreOpd[j]._endPos;
                        else
                            lstNewPreOpd.Add(lstPreOpd[j]);
                    }

                    //앞 oht가 멈춰있고 이동 예약도 없는 경우
                    if (lstNewPreOpd.Count == 0)
                    {
                        //lastTime 부터 curTime까지 && 멈춰있는 OHT 전까지 스케줄링!!!!
                        double betweenLength = lengthForToLine - lastPos;
                        double stopReadyLength = getLength_v_v0_a(0, endSpeed, _deceleration);
                        double stopReadyPos = nextOHTPos - IntervalLength - stopReadyLength;

                        if (stopReadyPos > cutPos)
                            stopReadyPos = cutPos;

                        AddPosdata_simlogs_limitTime(oht.Speed, lastTime, lastSpeed, lastPos, nextOHTPos, double.MaxValue, destPos, endSpeed, ref lst);

//                                AddPosdata_simlogs(oht.Speed, lastData._endTime, lastData._endSpeed, lastData._endPos, endSpeed, stopReadyPos, ref lst);
                        lst = cutListOHTPosDataByPos(lst, stopReadyPos);

                        //if (lst.Count == 0 && nextOHTPos > stopReadyPos && lastPos < stopReadyPos)
                        //{
                        //    AddPosdata_simlogs(oht.Speed, lastTime, lastSpeed, lastPos, endSpeed, stopReadyPos, ref lst);
                        //    lst = cutListOHTPosDataByPos(lst, stopReadyPos);
                        //}
                    }
                    else
                    {
                        OHTPosData lastPreOpd = lstNewPreOpd.Last();
                        double reservationPoint = getToNodeZCUReservationPos();

                        //앞 oht가 멈춰있고 이동 전까지 
                        if (lastTime < lstNewPreOpd[0]._startTime)
                        {
                            AddPosdata_simlogs_limitTime(oht.Speed, lastTime, lastSpeed, lastPos, nextOHTPos, (double)lstNewPreOpd[0]._startTime, destPos, endSpeed, ref lst);
                            //cut 추가
                            lst = cutListOHTPosDataByPos(lst, cutPos);

                            if (lst.Count > 0)
                            {
                                lastTime = lst[lst.Count - 1]._endTime;
                                lastSpeed = lst[lst.Count - 1]._endSpeed;
                                lastPos = lst[lst.Count - 1]._endPos;
                                nextOHTPos = lengthForToLine + lstNewPreOpd[0]._endPos;
                            }
                        }

                        //앞 oht의 스케줄을 돌며 destpos 혹은 미래 스케줄 끝날때까지 스케줄링!!
                        for (int j = 0; j < lstNewPreOpd.Count; j++)
                        {
                            if (lastTime >= lstNewPreOpd[j]._startTime)//앞OHT 스케줄의 시작시간보다 지금시간이 늦을 경우
                            {
                                AddPosdata_simlogs_Opd(oht.Speed, lastTime, lastSpeed, lastPos, destPos, lstNewPreOpd[j], lengthForToLine, endSpeed, ref lst);
                                lst = cutListOHTPosDataByPos(lst, cutPos);
                            }

                            if (j < lstNewPreOpd.Count - 1) //다음 스케줄링할 opd 남아 있는 경우
                            {
                                if (lst.Count > 0)
                                {
                                    lastTime = lst[lst.Count - 1]._endTime;
                                    lastSpeed = lst[lst.Count - 1]._endSpeed;
                                    lastPos = lst[lst.Count - 1]._endPos;
                                    nextOHTPos = lengthForToLine + lstNewPreOpd[j]._endPos;
                                }

                                if (lastTime < lstNewPreOpd[j + 1]._startTime)
                                {
                                    AddPosdata_simlogs_limitTime(oht.Speed, lastTime, lastSpeed, lastPos, nextOHTPos, (double)lstNewPreOpd[j + 1]._startTime, destPos, 0, ref lst);
                                    lst = cutListOHTPosDataByPos(lst, cutPos);

                                    if (lst.Count > 0)
                                    {
                                        lastTime = lst[lst.Count - 1]._endTime;
                                        lastSpeed = lst[lst.Count - 1]._endSpeed;
                                        lastPos = lst[lst.Count - 1]._endPos;
                                        nextOHTPos = lengthForToLine + lstNewPreOpd[j + 1]._endPos;
                                    }
                                }
                            }
                        }
                    }

                    if (lstBetter == null)
                        lstBetter = lst;
                    else if (lstBetter.Count == 0)
                        lstBetter = lst;
                    else if (lstBetter.Count > 0 && lst.Count == 0)
                        lstBetter = lst;
                    else if (lstBetter.Last()._endPos > lst.Last()._endPos)
                        lstBetter = lst;
                }

                if (lstBetter.Count > 0 && lstBetter.Last()._endTime > 200000)
                    ;
                if (lstBetter.Count > 0 && lstBetter.Last()._startSpeed < 0.00001 && lstBetter.Last()._startSpeed != 0 && lstBetter.Last()._endSpeed < 0.00001 && lstBetter.Last()._endSpeed != 0)
                    ;
                if (lstBetter.Count > 0 && lstBetter.Last()._startSpeed > 0.1 && lstBetter.Last()._startSpeed != 0 && lstBetter.Last()._endSpeed < 0.00001 && lstBetter.Last()._endSpeed != 0)
                    ;
                return finishScheduling(simLogs, oht, ohtIdx, lstBetter, destPos, lastTime, lstOpd);
            }
        }

        private void GetEndSpeedNCutPos(OHTNode oht, double lastPos, ref double endSpeed, ref double cutPos)
        {
            if (ToNode.ZcuType == ZCU_TYPE.STOP)
            {
                if (lastPos < getToNodeZCUReservationPos() && !(ToNode.Zcu.ContainsReservation(oht)))
                {
                    double toNodeZCUReservationPos = getToNodeZCUReservationPos();

                    if (cutPos > toNodeZCUReservationPos)
                        cutPos = toNodeZCUReservationPos;
                }
                else if (!ToNode.Zcu.IsFirstReservation(oht))  //통과했는데 우선순위에 의해 멈춰야할 때
                {
                    endSpeed = 0;
                }
            }

            //다음다음 Point가  ZCU Stop인데 마지막 이동한 곳이 다음다음 Point 예약지점일때              
            if (oht.LstRailLine.Count > 2 && oht.LstRailLine[1].ToNode.ZcuType == ZCU_TYPE.STOP)
            {
                if (oht.LstRailLine[1].getToNodeZCUReservationPos() < 0
                && lastPos < getToToNodeZCUReservationPos(oht.LstRailLine[1]))
                {
                    double totoNodeZCUReservationPos = getToToNodeZCUReservationPos(oht.LstRailLine[1]);

                    if (cutPos > totoNodeZCUReservationPos)
                        cutPos = totoNodeZCUReservationPos;
                }
                else if (!oht.LstRailLine[1].ToNode.Zcu.IsFirstReservation(oht)) //통과했는데 우선순위에 의해 멈춰야할 때
                {
                    double toLineStartVelocityForStopPoint = getVelocity0_v_a_s(0, _deceleration, oht.LstRailLine[1].Length);
                    endSpeed = endSpeed > toLineStartVelocityForStopPoint ? toLineStartVelocityForStopPoint : endSpeed;
                }
            }

            if (oht.LstRailLine.Count > 3 && oht.LstRailLine[2].ToNode.ZcuType == ZCU_TYPE.STOP)
            {
                if (oht.LstRailLine[2].getToNodeZCUReservationPos() < 0
                && oht.LstRailLine[1].getToToNodeZCUReservationPos(oht.LstRailLine[2]) < 0
                && lastPos < getToToToNodeZCUReservationPos(oht.LstRailLine[1], oht.LstRailLine[2])
                && !(oht.LstRailLine[2].ToNode.Zcu.ContainsReservation(oht)))
                {
                    double tototoNodeZCUReservationPos = getToToToNodeZCUReservationPos(oht.LstRailLine[1], oht.LstRailLine[2]);

                    if (cutPos > tototoNodeZCUReservationPos)
                        cutPos = tototoNodeZCUReservationPos;
                }
                else if (!oht.LstRailLine[2].ToNode.Zcu.IsFirstReservation(oht)) //통과했는데 우선순위에 의해 멈춰야할 때
                {
                    double totoLineStartVelocityForStopPoint = getVelocity0_v_a_s(0, _deceleration, oht.LstRailLine[2].Length);
                    double toLineStartVelocityForStopPoint = getVelocity0_v_a_s(totoLineStartVelocityForStopPoint, _deceleration, oht.LstRailLine[1].Length);

                    endSpeed = endSpeed > toLineStartVelocityForStopPoint ? toLineStartVelocityForStopPoint : endSpeed;
                }
            }
        }

        public bool CheckAvailableStop(OHTNode oht, List<OHTPosData> lst, double endSpeed)
        {
            if (lst.Count == 0)
                return true;

            double mv3 = getVelocity_v0_a_s(_maxSpeed, _deceleration, getLength_v_v0_a(0, _maxSpeed, _deceleration) - oht.DestinationPort.Distance);
            if (oht.LstRailLine.Count == 2 && oht.DestinationPort.Line.Name == oht.LstRailLine[1].Name && lst.Count > 0 && lst.Last()._endPos == Length && lst.Last()._endSpeed - 100 > mv3)
                return false;

            double possibleEndVelocity = getVelocity_v0_a_s(lst.Last()._endSpeed, _deceleration, Length - lst.Last()._endPos);
            if (Math.Round(lst.Last()._endSpeed, 3) > Math.Round(endSpeed, 3) && Math.Round(possibleEndVelocity, 3) > Math.Round(endSpeed, 3))
                return false;

            return true;
        }

        public void UpdateOHTPosition(SimHistory simLogs, Time curTime, int ohtIdx)
        {
            if (ohtIdx == -1 && _lstOHT.Count == 0)
                return;
            else if (ohtIdx == -1 && _lstOHT.Count > 0)
                ohtIdx = 0;

            bool isLastOHTUpdated = false;
            for(int i = ohtIdx; i < _lstOHT.Count; i++)
            {
                bool isUpdatedThisOHT = UpdateOHTPosData(simLogs, curTime, _lstOHT[i]);

                if(!isUpdatedThisOHT)
                {
                    break;
                }

                if (i == _lstOHT.Count -1 && isUpdatedThisOHT == true)
                    isLastOHTUpdated = true;
            }

            if (isLastOHTUpdated)
            {
                OHTNode oht = _lstOHT.Last();

                if (oht.Name == "M14A_V_262" && Name == "rl_M14A_A2_34")
                    ;

                foreach (ZCU zcu in oht.LstZCU.ToArray())
                {
                    foreach (OHTNode stopOHT in zcu.StopOHTs)
                    {
                        if (oht.ZcuResetPointName != stopOHT.ZcuResetPointName)
                            continue;

                        List<OHTPosData> lstStopOHTPosData = stopOHT.CurRailLine.LstOHTPosData[0];
                        double stopPos = lstStopOHTPosData[lstStopOHTPosData.Count - 1]._endPos;
                        if (lstStopOHTPosData.Last()._endTime < LstOHTPosData[ohtIdx].Last()._endTime)
                        {
                            lstStopOHTPosData.Add(new OHTPosData(0, 0, lstStopOHTPosData.Last()._endTime, LstOHTPosData[ohtIdx].Last()._endTime, stopPos, stopPos));

                            if (stopOHT.CurRailLine.ListOHT.Count > 1)
                            {
                                SimPort port = new SimPort(EXT_PORT.OHT_MOVE, stopOHT.CurRailLine, stopOHT.CurRailLine._lstOHT[1]);
                                port.Time = curTime;
                                stopOHT.CurRailLine.ExternalFunction(curTime, simLogs, port);

                            }
                            else
                                stopOHT.CurRailLine.CallBackOHT(simLogs, curTime);
                        }
                    }

                    foreach (List<ZCUReservation> zcuReservations in zcu.Reservations.Values)
                    {
                        foreach (ZCUReservation zcuReservation in zcuReservations.ToArray())
                        {
                            OHTNode reservOHT = zcuReservation.OHT;

                            if (oht.ZcuResetPointName != reservOHT.ZcuResetPointName)
                                continue;

                            List<OHTPosData> lstStopOHTPosData = reservOHT.CurRailLine.LstOHTPosData[0];
                            if (lstStopOHTPosData.Count == 0 || lstStopOHTPosData.Last()._endTime < LstOHTPosData[ohtIdx].Last()._endTime)
                            {
                                SimPort port = new SimPort(EXT_PORT.OHT_MOVE, reservOHT.CurRailLine, reservOHT);
                                port.Time = curTime;
                                reservOHT.CurRailLine.ExternalFunction(curTime, simLogs, port);
                            }
                        }
                    }
                }

                CallBackOHT(simLogs, curTime);
            }
        }

        public bool UpdateOHTPosData(SimHistory simLogs, Time curTime, OHTNode oht) //ohtIdx 0부터 시작
        {
            bool isScheduled = false;
            bool isAlreadyArrived = false;
            int ohtCount = _lstOHT.Count;
            int ohtIdx = _lstOHT.FindIndex(o => o.Name == oht.Name);

            List<OHTPosData> ohtPosData = _lstOHTPosData[ohtIdx];
            int ohtPosDatasCount = ohtPosData.Count;
            double destPos = CalDestPos(curTime, oht, oht.DestinationPort);
            double scheduledDestPos = _lstStartPos[ohtIdx];
            Time lastScheduleTime = curTime;
            if (ohtPosDatasCount > 0)
            {
                OHTPosData lastOHTPosData = ohtPosData.Last();
                scheduledDestPos = lastOHTPosData._endPos;
                lastScheduleTime = lastOHTPosData._endTime;
            }

            if (scheduledDestPos >= destPos && ohtPosDatasCount > 0)
                isAlreadyArrived = true;

            if (oht.Name == "M14A_V_859" && Name == "rl_M14A_A7_10")
                ;

            if (oht.Name == "M14A_V_278" && curTime > 115.73)
                ;

            if (oht.Name == "M14A_V_262" && Name == "rl_M14A_A2_34")
                ;

            if (!isAlreadyArrived)
            {
                EvtData evtData = GenerateOHTPosData(simLogs, curTime, ohtIdx, destPos);

                // 현재 라인에 멈추는게 아닐 경우 예약. Port Loading/Unloading 끝낸 다음에도 예약
                if (evtData._time > 0 && !(oht.DestinationPort.RailLineName == Name || oht.LstRailLine.Count == 1))
                {
                    MakeReservation(oht, ohtPosData);
                }

                if (evtData._time > 0)
                {
                    isScheduled = true;
                    if (evtData._isArrive)
                    {
                        if (destPos >= Length)
                        {
                            SimPort port = new SimPort(INT_PORT.OHT_CHECK_OUT, this, oht);
                            EvtCalendar.AddEvent(evtData._time, this, port);
                        }
                        else if (_lstOHT[ohtIdx].DestinationPort.Line == this && _lstOHT[ohtIdx].DestinationPort.Distance == destPos)  //링크 내에서 업무 이벤트 예약
                        {
                            SimPort port = null;
                            if (oht.NodeState is OHT_STATE.MOVE_TO_LOAD)
                            {
                                port = new SimPort(INT_PORT.OHT_LOADING, this, oht);
                                EvtCalendar.AddEvent(evtData._time, this, port);
                            }
                            else if (oht.NodeState is OHT_STATE.MOVE_TO_UNLOAD)
                            {
                                port = new SimPort(INT_PORT.OHT_UNLOADING, this, oht);
                                EvtCalendar.AddEvent(evtData._time, this, port);
                            }
                            else if (oht.NodeState is OHT_STATE.IDLE)
                            {
                                port = new SimPort(INT_PORT.OHT_MOVE_TO_IDLE, this, oht);
                                EvtCalendar.AddEvent(evtData._time, this, port);
                            }
                        }
                    }
                    else
                    {
                        SimPort port = new SimPort(INT_PORT.OHT_MOVE, this, oht);
                        EvtCalendar.AddEvent(ohtPosData.Last()._endTime, this, port);
                    }
                }
            }

            if (isScheduled == false
                && ToNode.ZcuType == ZCU_TYPE.STOP 
                && ToNode.Zcu != null 
                && (ohtPosData.Count > 0 && ohtPosData.Last()._endSpeed == 0)
                && ((ohtPosData.Count > 0 && curTime == ohtPosData.Last()._endTime) || (ohtPosData.Count == 0 && curTime == _lstStartTime[ohtIdx])))
            {
                isScheduled = AddStoppedPosData(ohtIdx, ref ohtPosData, oht);
            }

            //if (oht.Name == "M14A_V_103" )
            //{
            //    Console.WriteLine("======" + this.Name + "=====" + Math.Round(Length, 2) + "===========" + Math.Round(curTime.TotalSeconds, 2) + "====" + oht.Name);
            //    for (int i = 0; i < LstOHTPosData.Count; i++)
            //        for (int j = 0; j < LstOHTPosData[i].Count; j++)
            //        {
            //            Console.WriteLine(_lstOHT[i].Name + "  |  (" + Math.Round(LstOHTPosData[i][j]._startTime.TotalSeconds, 2) + " , " + Math.Round(LstOHTPosData[i][j]._startSpeed, 2) + " , " + Math.Round(LstOHTPosData[i][j]._startPos, 2) + " ) => ( " + Math.Round(LstOHTPosData[i][j]._endTime.TotalSeconds, 2) + " , " + Math.Round(LstOHTPosData[i][j]._endSpeed, 2) + " , " + Math.Round(LstOHTPosData[i][j]._endPos, 2) + ")");
            //        }
            //}

            oht.CurDistance = GetDistanceAtTime(oht, curTime);

            return isScheduled;
        }

        private void MakeReservation(OHTNode oht, List<OHTPosData> ohtPosData)
        {
            // 다음 point가 ZCU Stop인데 시작점 또는 마지막 이동한 곳이 다음 Point 예약지점이거나 지났는데 예약 안했을 때 
            if (ToNode.ZcuType == ZCU_TYPE.STOP
            && !(ToNode.Zcu.ContainsReservation(oht))
            && ohtPosData.Count > 0
            && (ohtPosData.Last()._endPos == getToNodeZCUReservationPos()
                || ohtPosData.Last()._startPos > getToNodeZCUReservationPos()) // Port에 Loading 또는 Unloading 하고 늦게 예약하거나 늦게 디스패칭 됨.
                )
            {
                ToNode.Zcu.AddReservation(ohtPosData.Last()._endTime, oht, ToNode);
            }
            //다음다음 Point가  ZCU Stop인데 마지막 이동한 곳이 다음다음 Point 예약지점이거나 지났는데 예약 안했을 때  
            else if (oht.LstRailLine.Count > 2 && oht.LstRailLine[1].ToNode.ZcuType == ZCU_TYPE.STOP
                && oht.LstRailLine[1].getToNodeZCUReservationPos() < 0
                && !(oht.LstRailLine[1].ToNode.Zcu.ContainsReservation(oht))
                && ohtPosData.Count > 0
                && (ohtPosData.Last()._endPos == getToToNodeZCUReservationPos(oht.LstRailLine[1])
                    || ohtPosData.Last()._startPos > getToToNodeZCUReservationPos(oht.LstRailLine[1]))  // Port에 Loading 또는 Unloading 하고 늦게 예약하거나 늦게 디스패칭 됨.
                    )
            {
                oht.LstRailLine[1].ToNode.Zcu.AddReservation(ohtPosData.Last()._endTime, oht, oht.LstRailLine[1].ToNode);
            }
            //다다다음 Point가 ZCU Stop인데 마지막 이동한 곳이 다다다음 Point 예약지점이거나 지났는데 예약 안했을 때
            else if (oht.LstRailLine.Count > 3 && oht.LstRailLine[2].ToNode.ZcuType == ZCU_TYPE.STOP
                && oht.LstRailLine[2].getToNodeZCUReservationPos() < 0
                && oht.LstRailLine[1].getToToNodeZCUReservationPos(oht.LstRailLine[2]) < 0
                && !(oht.LstRailLine[2].ToNode.Zcu.ContainsReservation(oht))
                && ohtPosData.Count > 0
                && (ohtPosData.Last()._endPos == getToToToNodeZCUReservationPos(oht.LstRailLine[1], oht.LstRailLine[2])
                    || ohtPosData.Last()._startPos > getToToToNodeZCUReservationPos(oht.LstRailLine[1], oht.LstRailLine[2]))  // Port에 Loading 또는 Unloading 하고 늦게 예약하거나 늦게 디스패칭 됨.
                )
            {
                oht.LstRailLine[2].ToNode.Zcu.AddReservation(ohtPosData.Last()._endTime, oht, oht.LstRailLine[2].ToNode);
            }
        }

        private bool AddStoppedPosData(int ohtIdx, ref List<OHTPosData> ohtPosData, OHTNode oht)
        {
            double lastLength = 0;
            Time lastTime = Time.MinValue;
            if(ohtPosData.Count == 0)
            {
                lastLength = _lstStartPos[ohtIdx];
                lastTime = _lstStartTime[ohtIdx];
            }
            else
            {
                lastLength = ohtPosData.Last()._endPos;
                lastTime = ohtPosData.Last()._endTime;
            }

            Time zcuFrontOHTScheduleTime = ToNode.Zcu.GetScheduleTimeOfFrontOHT(oht);
            if(zcuFrontOHTScheduleTime != -1 && zcuFrontOHTScheduleTime > lastTime)
            {
                ohtPosData.Add(new OHTPosData(0, 0, lastTime, zcuFrontOHTScheduleTime, lastLength, lastLength));
                SimPort port = new SimPort(INT_PORT.OHT_MOVE, this, oht);
                EvtCalendar.AddEvent(zcuFrontOHTScheduleTime, this, port);
                return true;
            }

            return false;
        }


        public void CallBackOHT(SimHistory simLogs, Time curTime)
        {
            Time enableTime = -1;
            List<RailLineNode> visitedLines = new List<RailLineNode>();
            List<OHTNode> meetingOHTs = new List<OHTNode>();
            List<RailLineNode> meetingLines = new List<RailLineNode>();
            foreach (RailLineNode line in FromNode.FromLines)
            {
                RailLineNode lineTemp = line;
                RailPointNode fromPoint = line.FromNode;
                int sequence = 0;
                SearchFirstMeetingOHT(line, visitedLines, sequence, ref meetingLines, ref meetingOHTs );
            }

            for (int i = 0; i < meetingLines.Count; i++)
            {
                RailLineNode line = meetingLines[i];
                OHTNode oht = meetingOHTs[i];
                int ohtIdx = line.ListOHT.FindIndex(o=>o.Name == oht.Name);

                if ((line.LstOHTPosData[ohtIdx].Count == 0 && line.LstStartTime[ohtIdx] >= curTime)
                    || (line.LstOHTPosData[ohtIdx].Count > 0 && line.LstOHTPosData[ohtIdx].Last()._endTime >= curTime))
                    continue;

                //Console.WriteLine(curTime + "==Line : " + this.Name +  "  front OHT: " + _lstOHT[ohtIdx].Name + "  back OHT: " + oht.Name);
                SimPort port = new SimPort(EXT_PORT.OHT_MOVE, line, oht);
                port.Time = curTime;
                line.ExternalFunction(curTime, simLogs, port);
            }
        }

        private void SearchFirstMeetingOHT(RailLineNode line, List<RailLineNode> visitedLines, int sequence, ref List<RailLineNode> meetingLines, ref List<OHTNode> meetingOHTs)
        {
            sequence++;
            if (sequence > 3)
                return;

            visitedLines.Add(line);

            if (line.OHTCount > 0)
            {
                if (line.Name == line._lstOHT[0].CurRailLineName)
                {
                    meetingOHTs.Add(line._lstOHT[0]);
                    meetingLines.Add(line);
                    return;
                }
                else
                    return;
            }
            else
            {
                foreach (RailLineNode fromLine in line.FromNode.FromLines)
                {
                    if (!visitedLines.Contains(fromLine))
                        SearchFirstMeetingOHT(fromLine, visitedLines, sequence, ref meetingLines, ref meetingOHTs);
                }
            }
        }

        private void ReviseOHTPosData(SimHistory simLogs, Time simTime, OHTNode oht)
        {
            if (_lstOHT.Contains(oht))
            {
                if (Name == "rl_M14A_A2_18")
                    ;

                if (oht.Name == "M14A_V_114")
                    ;

                int index = _lstOHT.IndexOf(oht);

                for (int i= index; i < _lstOHT.Count; i++)
                {
                    OHTNode revisedOht = _lstOHT[i];

                    if (Name == "rl_M14A_InterBay_M14A_2_721" && oht.Name == "M14A_V_359")
                        ;

                    if (revisedOht.CurDistance == Length && _lstOHTPosData[i].Last()._endSpeed == 0)
                        break;

                    if (revisedOht.NodeState is OHT_STATE.LOADING || revisedOht.NodeState is OHT_STATE.UNLOADING)
                        break;

                    double curPos = GetDistanceAtTime(_lstOHT[i], simTime);
                    //                    _lstStartPos[index] = curPos;
                    //                    _lstStartTime[index] = simTime;
                    revisedOht.CurDistance = curPos;
                    //                    _lstOHTPosData[index].Clear();

                    List<OHTPosData> tempListOHTPosData = new List<OHTPosData>();
                    for (int j = 0; j < _lstOHTPosData[i].Count; j++)
                    {
                        OHTPosData posData = _lstOHTPosData[i][j];
                        int idx = (int)posData._endTime / simLogs.UnitNum;
                        List<SimLog> lstLog = simLogs.DicSimLogs[idx];

                        if (posData._endTime <= simTime)
                            tempListOHTPosData.Add(posData);
                        else if (posData._startTime < simTime && !(posData._startSpeed == 0 && posData._endSpeed == 0))
                        {
                            for (int k = 0; k < lstLog.Count(); k++)
                            {
                                SimLog log = lstLog[k];
                                if (log != null && log.NodeCore.Name == revisedOht.Name && log.StartTime == posData._startTime)
                                {
                                    //                                    SimLog newLog = new SimLog(log.StartTime, simTime, log.StartPos, log.Direction, oht, this, log.NodeState);
                                    log.EndTime = simTime;
                                    simLogs.AddLog(log);
                                    lstLog[k] = null;
                                    break;
                                    //                                    simLogs.AddLog(newLog);
                                }
                            }

                            posData._endTime = simTime;
                            posData._endPos = curPos;
                            posData._endSpeed = getVelocity_v0_a_s(posData._startSpeed, posData._celerate, curPos - posData._startPos);
                            tempListOHTPosData.Add(posData);
                        }
                        else
                        {
                            foreach (SimLog log in lstLog.ToList())
                            {
                                if (log != null && log.NodeCore.Name == revisedOht.Name && log.StartTime == posData._startTime)
                                {
                                    lstLog.Remove(log);
                                }
                            }
                        }
                    }
                    _lstOHTPosData[i] = tempListOHTPosData;

                    ReviseEvts(simTime, revisedOht);
                }
                if(oht.ZcuResetPointName != string.Empty)
                {
                    string oldResetPointName = oht.ZcuResetPointName;
                    RailPointNode oldResetPoint = ModelManager.Instance.DicRailPoint[oht.ZcuResetPointName];
                    oht.CurZcu.ChangeReservationResetPoint(oht);

                    RailPointNode newResetPoint = ModelManager.Instance.DicRailPoint[oht.ZcuResetPointName];
                    oht.CurZcu.ProcessReservation(simTime, simLogs, oldResetPoint);
                    oht.CurZcu.ProcessReservation(simTime, simLogs, newResetPoint);
                }
                UpdateOHTPosition(simLogs, simTime, index);
            }

        }

        private void ReviseEvts(Time simTime, OHTNode oht)
        {
            if (oht.Name == "M14A_V_290")
                ;

            for (int i = (int)(simTime / EvtCalendar.UnitNum); i < EvtCalendar.LastAddTime + 1; i++)
            {
                _lstRemove.Clear();
                _lstEvt = EvtCalendar.DicEvt[i];
                _count = _lstEvt.Count;
                Event evt = null;
                for (int j = 0; j < _count; j++)
                {
                    evt = _lstEvt[j];
                    if (evt.Port == null || evt.Port.OHTNode == null)
                        continue;

                    if (this == evt.NodeCore && evt.Port.OHTNode == oht && evt.Time > simTime)
                    {
                        if (oht.Name == "M14A_V_290")
                            ;

                        if (evt.Port.PortType is INT_PORT.OHT_MOVE_TO_LOAD && evt.Time - oht.LoadingTime <= simTime) //이미 로딩중이니 취소하지 말자.
                            continue;
                        else if (evt.Port.PortType is INT_PORT.OHT_MOVE_TO_UNLOAD && evt.Time - oht.UnloadingTime <= simTime) //이미 언로딩중이니 취소하지 말자.
                            continue;

                        if (evt.Port.PortType is INT_PORT.OHT_MOVE_TO_LOAD || evt.Port.PortType is INT_PORT.OHT_MOVE_TO_UNLOAD)
                            ;

                        _lstRemove.Add(j);
                        //EvtCalendar.RemoveEvent(i, evt);
                    }
                }

                for (int j = _lstRemove.Count - 1; j >= 0; j--)
                {
                    EvtCalendar.RemoveEvent(i, _lstRemove[j]);
                }
            }
        }


        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);

            if (Name == "rl_M14A_A2_18")
                ;

            SimPort port = new SimPort(INT_PORT.OHT_MOVE);
            evtCal.AddEvent(0, this, port);
        }

        public double processOutOHT(SimHistory simLogs, Time simTime, OHTNode oht) //OHT가 나가는 순간 속도를 리턴함 //woong1130
        {

            double lastSpeed = _lstOHTStartSpeed[0];
            if (_lstOHTPosData[0].Count > 0)
                lastSpeed = _lstOHTPosData[0].Last()._endSpeed;
            if (_lstOHT[0].Name != oht.Name)
                ;

            _lstOHT.RemoveAt(0);
            _lstStartPos.RemoveAt(0);
            _lstOHTPosData.RemoveAt(0);
            _lstStartTime.RemoveAt(0);
            _lstOHTStartSpeed.RemoveAt(0);
            oht.LstRailLine.RemoveAt(0);
            _dicReroutingCount.Remove(oht.ID);

            return lastSpeed;

//            UpdateOHTPosData(simLogs, simTime);
        }

        public void ProcessAfterOHTArrive(OHTNode oht, Time simTime, SimHistory simLogs)
        {            
            if(oht.BumpingBay != null) // Bumping 중이었음
                Scheduler.Instance.LetOHTContinueBumping(oht, simTime, simLogs);
            else // 이제 막 Command를 완료한 상태
                Scheduler.Instance.LetIdleOHTGoToBay(oht, simTime, simLogs);
        }

        //지명 요청 함수 : input - oht, 시간 / output = length
        public double GetDistanceAtTime(OHTNode oht, Time simTime)
        {
            double curDistance = 0;

            int ohtIdx = _lstOHT.FindIndex(o => o.Name == oht.Name);
            List<OHTPosData> ohtPosDataList = _lstOHTPosData[ohtIdx];

            if (ohtPosDataList.Count == 0)
            {
                curDistance = _lstStartPos[ohtIdx];
            }
            else
            {
                foreach (OHTPosData tempOHTPosData in ohtPosDataList)
                {
                    if (tempOHTPosData._startTime <= simTime && tempOHTPosData._endTime > simTime)
                    {
                        OHTPosData ohtPosData = tempOHTPosData;
                        
                        curDistance = ohtPosData._startPos + getLength_v0_a_t(ohtPosData._startSpeed, ohtPosData._celerate, (simTime - ohtPosData._startTime).TotalSeconds); ;
                        break;
                    }
                    //oht가 정차해 있는 경우
                    else if (simTime >= tempOHTPosData._startTime)
                        curDistance = tempOHTPosData._endPos;
                }
            }
            return curDistance;
        }

        public void GetStopAvailableDistance(Time simTime, OHTNode oht, ref List<RailLineNode> listStoppingLines, ref RailLineNode stopLine, ref double stopAvailableDistance)
        {
            int ohtIdx = _lstOHT.FindIndex(o => o.Name == oht.Name);
            if (ohtIdx == -1)
                return;

            List<OHTPosData> ohtPosDataList = _lstOHTPosData[ohtIdx];

            if(ohtPosDataList.Count == 0)
            {
                stopLine = oht.CurRailLine;
                double nowVelocity = getVelocity_v0_a_t(_lstOHTStartSpeed[ohtIdx], 0, (simTime - _lstStartTime[ohtIdx]).TotalSeconds);
                double nowDistance = _lstStartPos[ohtIdx] + getLength_v0_a_t(_lstOHTStartSpeed[ohtIdx], 0, (simTime - _lstStartTime[ohtIdx]).TotalSeconds);
                stopAvailableDistance = nowDistance + getLength_v_v0_a(0, nowVelocity, _deceleration);

                int i = 0;

                while (stopAvailableDistance > stopLine.Length)
                {
                    stopAvailableDistance = stopAvailableDistance - stopLine.Length;
                    listStoppingLines.Add(stopLine);
                    stopLine = stopLine.ToNode.ToLines[0];
                    i++;
                }
            }
            else
            {
                foreach (OHTPosData tempOHTPosData in ohtPosDataList)
                {
                    if (tempOHTPosData._startTime <= simTime && tempOHTPosData._endTime >= simTime)
                    {
                        stopLine = oht.CurRailLine;
                        OHTPosData ohtPosData = tempOHTPosData;
                        double nowVelocity = getVelocity_v0_a_t(ohtPosData._startSpeed, ohtPosData._celerate, (simTime - ohtPosData._startTime).TotalSeconds);
                        double nowDistance = ohtPosData._startPos + getLength_v0_a_t(ohtPosData._startSpeed, ohtPosData._celerate, (simTime - ohtPosData._startTime).TotalSeconds);
                        stopAvailableDistance = nowDistance + getLength_v_v0_a(0, nowVelocity, _deceleration);

                        int i = 0;

                        while (stopAvailableDistance > stopLine.Length)
                        {
                            stopAvailableDistance = stopAvailableDistance - stopLine.Length;
                            listStoppingLines.Add(stopLine);
                            stopLine = stopLine.ToNode.ToLines[0];
                            i++;
                        }
                    }
                    else if (tempOHTPosData._endTime < simTime)
                    {
                        stopAvailableDistance = tempOHTPosData._endPos;
                        stopLine = this;
                    }
                }
            }
        }

        public void UpdateToNodeReservation(Time simTime, OHTNode oht)
        {
            int ohtIdx = ListOHT.IndexOf(oht);
            if (ToNode.ZcuType is ZCU_TYPE.STOP
                && (LstOHTPosData[ohtIdx].Count > 0 && oht.CurDistance > getToNodeZCUReservationPos())
                && oht.DestinationPort.RailLineName != Name)
            {
                ToNode.Zcu.RemoveReservation(oht);
                ToNode.Zcu.AddReservation(simTime, oht, ToNode);

                if (Length == oht.CurDistance)
                {
                    SimPort port = new SimPort(INT_PORT.OHT_CHECK_OUT, this, oht);
                    EvtCalendar.AddEvent(simTime, this, port);
                }
            }
            else if (oht.LstRailLine.Count > 2 && oht.LstRailLine[1].ToNode.ZcuType == ZCU_TYPE.STOP
                && oht.LstRailLine[1].getToNodeZCUReservationPos() < 0
                && oht.CurDistance > getToToNodeZCUReservationPos(oht.LstRailLine[1]))
            {
                oht.LstRailLine[1].ToNode.Zcu.AddReservation(simTime, oht, oht.LstRailLine[1].ToNode);
            }
            else if (oht.LstRailLine.Count > 3 && oht.LstRailLine[2].ToNode.ZcuType == ZCU_TYPE.STOP
                && oht.LstRailLine[2].getToNodeZCUReservationPos() < 0
                && oht.LstRailLine[1].getToToNodeZCUReservationPos(oht.LstRailLine[2]) < 0
                && oht.CurDistance > getToToToNodeZCUReservationPos(oht.LstRailLine[1], oht.LstRailLine[2]))
            {
                oht.LstRailLine[2].ToNode.Zcu.AddReservation(simTime, oht, oht.LstRailLine[2].ToNode);
            }
        }

        private void MoveToLoad(Time simTime, SimHistory simLogs, OHTNode oht)
        {
            int ohtIdx = _lstOHT.IndexOf(oht);
            Foup foup = oht.Command.Entity as Foup;

            oht.CurDistance = GetDistanceAtTime(oht, simTime);
            oht.SetRailStation(oht.DestinationPort);
            oht.SetEntity(foup);

            if (ModelManager.Instance.SimType == SIMULATION_TYPE.PRODUCTION)
            {
                foup.ReservationOHT = null;

                // From Port에서 Foup 제거 이벤트 발생시키기. 경우는 2가지, 1. 장비 Port 2. STB ↓↓↓↓↓
                SimPort removePort = new SimPort(EXT_PORT.OHT_LOADING, this, foup);
                removePort.OHTNode = oht;

                if (oht.Command.FromNode is ProcessPortNode)
                {
                    ProcessPortNode fromPort = oht.Command.FromNode as ProcessPortNode;
                    fromPort.ExternalFunction(simTime, simLogs, removePort);
                }
                else
                {
                    RailPortNode fromPort = oht.Command.FromNode as RailPortNode;
                    fromPort.ExternalFunction(simTime, simLogs, removePort);
                }
                // From Port에서 Foup 제거 이벤트 발생시키기. 경우는 2가지, 1. 장비 Port 2. STB ↑↑↑↑↑
            }

            if (oht.Command.ToNode is RailPortNode)
                oht.DestinationPort = oht.Command.ToNode as RailPortNode;
            else
                oht.DestinationPort = (RailPortNode)oht.Command.ToNode.EVTOutLink.EndNode;

            oht.Destination = oht.Command.ToNode;
            oht.NodeState = OHT_STATE.MOVE_TO_UNLOAD;
            oht.DestinationPort.IsReservation = true;

            Scheduler.Instance.ExecuteCommand((Command)oht.Command, simTime);

            if (this.ID == oht.DestinationPort.Line.ID && Math.Round(oht.CurDistance, 3) == Math.Round(oht.DestinationPort.Distance,3)) // From, To Port 같을 때 (뭣하러 들었냐..)
            {
                SimPort port = new SimPort(INT_PORT.OHT_UNLOADING, this, oht);
                EvtCalendar.AddEvent(simTime, this, port);
            }
            else // To Port로 간다.
            {
                GetDistanceAtTime(oht, simTime);
                oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht));
                ((Command)(oht.Command)).TransferingDistance = oht.RouteDistance;

                if (((RailPortNode)oht.Command.ToNode).Line.Name != oht.LstRailLine.Last().Name)
                    ;


                //예약 포인트 넘어서 경로를 찾을 경우 늦게라도 예약해준다. 
                if (oht.CurZcu == null)
                    oht.CurRailLine.UpdateToNodeReservation(simTime, oht);
                else
                {
                    if (oht.ZcuResetPointName == string.Empty)
                        oht.CurZcu = null;
                    else
                    {
                        string oldResetPointName = oht.ZcuResetPointName;
                        RailPointNode oldResetPoint = ModelManager.Instance.DicRailPoint[oht.ZcuResetPointName];
                        oht.CurZcu.ChangeReservationResetPoint(oht);

                        RailPointNode newResetPoint = ModelManager.Instance.DicRailPoint[oht.ZcuResetPointName];
                        oht.CurZcu.ProcessReservation(simTime, simLogs, oldResetPoint);
                        oht.CurZcu.ProcessReservation(simTime, simLogs, newResetPoint);
                    }
                }
                UpdateOHTPosition(simLogs, simTime, ohtIdx);
            }

        }
        private void MoveToUnload(Time simTime, SimHistory simLogs, OHTNode oht)
        {
            if (!(oht.NodeState is OHT_STATE.UNLOADING))
                return;

            if (ModelManager.Instance.SimType == SIMULATION_TYPE.PRODUCTION)
            {
                Foup foup = oht.Command.Entity as Foup;

                // To Port에서 Foup Load 이벤트 발생시키기. 경우는 2가지, 1. 장비 Port 2. STB ↓↓↓↓↓
                SimPort createPort = new SimPort(EXT_PORT.OHT_UNLOADING, this, foup);

                if (oht.Command.ToNode is ProcessPortNode)
                {
                    ProcessPortNode toPort = oht.Command.ToNode as ProcessPortNode;
                    toPort.ExternalFunction(simTime, simLogs, createPort);
                }
                else
                {
                    RailPortNode toPort = ModelManager.Instance.DicRailPort[oht.Command.ToNode.Name];
                    toPort.ExternalFunction(simTime, simLogs, createPort);
                }
                // To Port에서 Foup Load 이벤트 발생시키기. 경우는 2가지, 1. 장비 Port 2. STB ↑↑↑↑↑
            }

            oht.CurDistance = GetDistanceAtTime(oht, simTime);
            oht.SetRailStation(oht.DestinationPort);
            oht.DestinationPort.IsReservation = false;
            //Command 완료 보고
            Scheduler.Instance.FinishCommand((Command)oht.Command, simTime);

            oht.SetEntity(null);

            //완료 후에는 Bumping 상태로 가야 하기 때문에 DepositOHTs에서 삭제
            if (Bay != null && Bay.DepositOHTs.Contains(oht))
                Bay.DepositOHTs.Remove(oht);

//            if(oht.ReservationPort)

            if (oht.Name == "M14A_V_34")
                ;

            if (oht.DispatchedCommand != null) // 예약 할당이 있을 때
            {
                Scheduler.Instance.ReservatedCommandList.Remove(oht.DispatchedCommand);
                Scheduler.Instance.WaitingCommandList.Add(oht.DispatchedCommand);
                oht.DispatchedCommand.CommandState = COMMAND_STATE.WAITING;
                oht.Command = oht.DispatchedCommand;
                ((Command)oht.Command).AssignedTime = SimEngine.Instance.StartDateTime.AddSeconds(simTime.TotalSeconds);
                oht.SetLstRailLine(oht.CandidateRoute);
                if (oht.CurRailLineName != oht.LstRailLine[0].Name)
                    ;
                if (oht.LstRailLine.Count == 1 || oht.LstRailLine.Count == 0)
                    ;
                oht.DispatchedCommand = null;
                oht.LstDispatchedLine.Clear();
                Scheduler.Instance.GoToAcquiringPort(simTime, simLogs, oht, oht.Command as Command);
            }
            else // 예약 할당이 없을 때
            {
                oht.NodeState = OHT_STATE.IDLE;
                if (!ModelManager.Instance.ReadyOHTs.Contains(oht))
                    ModelManager.Instance.ReadyOHTs.Add(oht);
                oht.Command = null;
                Scheduler.Instance.LetIdleOHTGoToBay(oht, simTime, simLogs);
            }
        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            OHTNode oht = port.OHTNode as OHTNode;
            int ohtIdx = _lstOHT.IndexOf(oht);
            Foup foup;
            SimPort newPort;

            switch ((INT_PORT)port.PortType)
            {
                case INT_PORT.OHT_MOVE:
                    UpdateOHTPosition(simLogs, simTime, ohtIdx);

                    break;
                case INT_PORT.OHT_CHECK_OUT:

                    if (oht.Name == "M14A_V_510")
                        ;

                    if (_lstOHT.Contains(oht))
                    {
                        oht.CurDistance = GetDistanceAtTime(oht, simTime);
                        newPort = new SimPort(EXT_PORT.OHT_CHECK_IN, this, oht);
                        ToNode.ExternalFunction(simTime, simLogs, newPort);
                    }
                    break;
                case INT_PORT.OHT_LOADING:
                    oht.NodeState = OHT_STATE.LOADING;
                    port = new SimPort(INT_PORT.OHT_MOVE_TO_LOAD, this, oht);
                    EvtCalendar.AddEvent(simTime + oht.LoadingTime, this, port);
                    oht.SetCurDistance(oht.DestinationPort.Distance);
                    LstOHTPosData[ohtIdx].Add(new OHTPosData(0, 0, simTime, simTime + oht.LoadingTime, oht.CurDistance, oht.CurDistance));

                    if (oht.Name == "M14A_V_290")
                        ;

                    if (_lstOHT.Count -1 > ohtIdx)
                        UpdateOHTPosition(simLogs, simTime, ohtIdx+1);
                    else
                        CallBackOHT(simLogs, simTime);

                    if (SimEngine.Instance.IsAnimation)
                        {
                            SimLog log = new SimLog(simTime, simTime + oht.LoadingTime, oht, ANIMATION.OHT_LOADING);
                            simLogs.AddLog(log);
                        }
                    break;
                case INT_PORT.OHT_UNLOADING:
                    oht.NodeState = OHT_STATE.UNLOADING;
                    port = new SimPort(INT_PORT.OHT_MOVE_TO_UNLOAD, this, oht);
                    EvtCalendar.AddEvent(simTime + oht.UnloadingTime, this, port);
                    oht.SetCurDistance(oht.DestinationPort.Distance);
                    LstOHTPosData[ohtIdx].Add(new OHTPosData(0, 0, simTime, simTime + oht.UnloadingTime, oht.CurDistance, oht.CurDistance));

                    if (_lstOHT.Count - 1 > ohtIdx)
                        UpdateOHTPosition(simLogs, simTime, ohtIdx+1);
                    else
                        CallBackOHT(simLogs, simTime);

                    if (SimEngine.Instance.IsAnimation)
                        {
                            SimLog log = new SimLog(simTime, simTime + oht.UnloadingTime, oht, ANIMATION.OHT_UNLOADING);
                            simLogs.AddLog(log);
                        }
                    break;
                case INT_PORT.OHT_MOVE_TO_LOAD:
                    MoveToLoad(simTime, simLogs, oht);
                    break;
                case INT_PORT.OHT_MOVE_TO_UNLOAD:
                    MoveToUnload(simTime, simLogs, oht);
                    break;
                case INT_PORT.OHT_MOVE_TO_IDLE:
                    oht.SetCurDistance(oht.DestinationPort.Distance);
                    oht.SetRailStation(oht.DestinationPort);
                    oht.NodeState = OHT_STATE.IDLE;

                    ProcessAfterOHTArrive(oht, simTime, simLogs);

                    break;
                default:
                    break;
            }
        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            OHTNode oht = port.OHTNode as OHTNode;
            int ohtIdx = _lstOHT.IndexOf(oht);
            SimPort newPort;
            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.OHT_IN: // Line의 앞쪽 Interval Length에 도착했을 때 발생하는 이벤트
                    {
                        _lstOHT.Add(oht);
                        _lstStartPos.Add(0);
                        _lstStartTime.Add(simTime);
                        _lstOHTStartSpeed.Add(port.LastSpeed);
                        _lstOHTPosData.Add(new List<OHTPosData>());
                        _dicReroutingCount.Add(oht.ID, 0);
                        oht.setCurRailLine(this);
                        if(Bay != null && Bay.FromLines.Contains(this))
                           oht.setCurBay(Bay);
                        
                        oht.CurDistance = GetDistanceAtTime(oht, simTime);
                        //                        Hids.Ohts.Add(oht);

                        if (oht.NodeState is OHT_STATE.MOVE_TO_UNLOAD
                            && oht.DestinationPort.Line.Bay != null
                            && Bay != null
                            && oht.DestinationPort.Line.Bay == Bay
                            && Bay.FromLines.Contains(this))
                        {
                            if(!Bay.DepositOHTs.Contains(oht))
                                Bay.DepositOHTs.Add(oht);

                            if(!ModelManager.Instance.ReadyOHTs.Contains(oht))
                                ModelManager.Instance.ReadyOHTs.Add(oht);
                        }

                        if (oht.NodeState is OHT_STATE.MOVE_TO_UNLOAD)
                            ((Command)oht.Command).Route.Add(this);
                        else if(oht.NodeState is OHT_STATE.IDLE && oht.DestinationPort.Line.Name == Name)
                        {
                            ProcessAfterOHTArrive(oht, simTime, simLogs);
                        }
                        UpdateOHTPosition(simLogs, simTime, _lstOHT.Count-1);
                    }
                    break;
                case EXT_PORT.REVISE:
                    {
                        ReviseOHTPosData(simLogs, simTime, oht);

                        if (oht.DestinationPort.Line.ID == this.ID && Math.Round(oht.DestinationPort.Distance,3) == Math.Round(oht.CurDistance,3)) //사실상 위치가 같을 때.
                        {
                            if (oht.DestinationPort.Distance != oht.CurDistance)
                                ;

                            port = new SimPort(INT_PORT.OHT_MOVE_TO_LOAD, this, oht);
                            EvtCalendar.AddEvent(simTime + oht.LoadingTime, this, port);
                        }
                    }
                    break;
                case EXT_PORT.OHT_MOVE:
                    {
                        if(simTime < port.Time)
                        {
                            port.setType(INT_PORT.OHT_MOVE);
                            EvtCalendar.AddEvent(port.Time, this, port);
                        }
                        else
                        {
                            UpdateOHTPosition(simLogs, simTime, ohtIdx);
                        }
                    }
                    break;
                case EXT_PORT.OHT_LOADING:
                    MoveToLoad(simTime, simLogs, oht);
                    break;
                case EXT_PORT.OHT_UNLOADING:
                    MoveToUnload(simTime, simLogs, oht);
                    break;
                case EXT_PORT.OHT_CHECK_OUT:
                    if (_lstOHT.Contains(oht))
                    {
                        oht.CurDistance = GetDistanceAtTime(oht, simTime);
                        newPort = new SimPort(EXT_PORT.OHT_CHECK_IN, this, oht);
                        ToNode.ExternalFunction(simTime, simLogs, newPort);
                    }
                    break;
                case EXT_PORT.OHT_OUT:
                    oht.CurDistance = GetDistanceAtTime(oht, simTime);

                    if (oht.LstRailLine.Count == 1)
                    {
                        oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);
                    }

                    double lastSpeed = 0;

                    if (_lstOHT[0].Name != oht.Name)
                    {
                        OHTNode frontOHT = _lstOHT[0];
                        lastSpeed = processOutOHT(simLogs, simTime, frontOHT);

                        if (Bay != null && Bay.ToLines.Contains(this))
                            oht.setCurBay(null);

                        newPort = new SimPort(EXT_PORT.OHT_IN, this, oht, lastSpeed);
                        oht.LstRailLine[0].ExternalFunction(simTime, simLogs, newPort);
                        simTime = simTime + 1;
                    }
                    if (_lstOHT.Count == 0)
                        return;
                    lastSpeed = processOutOHT(simLogs, simTime, oht);
                    if (Bay != null && Bay.ToLines.Contains(this))
                        oht.setCurBay(null);

                    newPort = new SimPort(EXT_PORT.OHT_IN, this, oht, lastSpeed);
                    oht.LstRailLine[0].ExternalFunction(simTime, simLogs, newPort);
                    break;
                default:
                    break;
            }
        }
    }
}



