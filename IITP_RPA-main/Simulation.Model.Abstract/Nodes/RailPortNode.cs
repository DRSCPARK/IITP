using Simulation.Engine;
using Simulation.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class RailPortNode : FabSimNode
    {
        private bool _isReservation;
        private Foup _reserveEntity;
        private Foup _loadedEntity;
        private PORT_TYPE _portType;
        private FabSimNode _connectedEqpNode;
        private RailLineNode _line;
        private double _distance;
        private bool _waitAllowed;
        private bool _bumpAllowed;


        [Browsable(true)]
        [DisplayName("Name")]
        public string RailPortName
        { get { return Name; } }

        [Browsable(true)]
        public bool IsReservation
        {
            get { return _isReservation; }
            set { _isReservation = value; }
        }

        [Browsable(true)]
        public PORT_TYPE PortType
        {
            get { return _portType; }
            set { _portType = value; }
        }

        [Browsable(true)]
        public FabSimNode ConnectedEqp
        {
            get { return _connectedEqpNode; }
            set { _connectedEqpNode = value; }
        }

        [Browsable(true)]
        public Foup ReserveEntity
        {
            get { return _reserveEntity; }
            set { _reserveEntity = value; }
        }

        [Browsable(true)]
        public Foup LoadedEntity
        {
            get { return _loadedEntity; }
            set { _loadedEntity = value; }
        }

        [Browsable(false)]
        public RailLineNode Line
        {
            get { return _line; }
            set { _line = value; }
        }

        [Browsable(true)]
        public string RailLineName
        {
            get { return _line.Name; }
        }

        [Browsable(true)]
        public double Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }

        [Browsable(true)]
        public bool WaitAllowed
        {
            get { return _waitAllowed; }
            set { _waitAllowed = value; }
        }

        [Browsable(true)]
        public bool BumpAllowed
        {
            get { return _bumpAllowed; }
            set { _bumpAllowed = value; }
        }

        public RailPortNode()
            : base(0, "", null)
        {
            NodeType = NODE_TYPE.PORT;
            double widthLength = Properties.Settings.Default.PortWidthLength;
            Size = new Vector3(widthLength, widthLength, widthLength);
        }

        public RailPortNode(uint ID, string name, Fab fab, PORT_TYPE type)
            : base(ID, name, fab)
        {
            NodeType = NODE_TYPE.PORT;
            _portType = type;
            _loadedEntity = null;
            double widthLength = Properties.Settings.Default.PortWidthLength;
            Size = new Vector3(widthLength, widthLength, widthLength);
        }

        public RailPortNode(uint ID, string name, RailLineNode line, double distance, Fab fab, PORT_TYPE type)
    : base(ID, name, fab)
        {
            NodeType = NODE_TYPE.PORT;
            _portType = type;
            _line = line;
            _distance = distance;
            _loadedEntity = null;
            PosVec3 = line.StartPoint + line.Direction * distance;

            double widthLength = Properties.Settings.Default.PortWidthLength;
            Size = new Vector3(widthLength, widthLength, widthLength);
        }

        public RailPortNode(uint ID, Vector3 pos, string name, Fab fab, PORT_TYPE type)
    : base(ID, name, pos, fab)
        {
            NodeType = NODE_TYPE.PORT;
            _portType = type;
            _loadedEntity = null;
            NodeState = RAILPORT_STATE.EMPTY;
            double widthLength = Properties.Settings.Default.PortWidthLength;
            Size = new Vector3(widthLength, widthLength, widthLength);
        }

        public RailPortNode(uint ID, string name, Fab fab, RailLineNode rl, double distance)
    : base(ID, name, fab)
        {
            NodeType = NODE_TYPE.PORT;
            PortType = PORT_TYPE.BOTH;
            _distance = distance;
            _line = rl;
            double widthLength = Properties.Settings.Default.PortWidthLength;
            Size = new Vector3(widthLength, widthLength, widthLength);
            NodeState = RAILPORT_STATE.EMPTY;
            PosVec3 = CalPos();
            rl.AddRailPort(this);
        }

        public RailPortNode(uint ID, string name, Fab fab, RailLineNode rl, double distance, PORT_TYPE potyType, bool waitAllowed, bool bumpAllowed)
: base(ID, name, fab)
        {
            NodeType = NODE_TYPE.PORT;
            PortType = potyType;
            _distance = distance;
            _line = rl;
            double widthLength = Properties.Settings.Default.PortWidthLength;
            Size = new Vector3(widthLength, widthLength, widthLength);
            _waitAllowed = waitAllowed;
            _bumpAllowed = bumpAllowed;
            NodeState = RAILPORT_STATE.EMPTY;
            PosVec3 = CalPos();
            rl.AddRailPort(this);
        }

        public Vector3 CalPos()
        {
            return _line.StartPoint + _line.Direction * _distance;
        }

        public uint GetRemainCapacity()
        {
            if (LoadedEntity != null)
                return 0;

            return 1;
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);

            if (LoadedEntity == null)
                NodeState = RAILPORT_STATE.EMPTY;
            else
                NodeState = RAILPORT_STATE.FULL;
        }

        public double GetDistance(Vector2 position)
        {
            return (position.X - PosVec3.X) * (position.X - PosVec3.X) + (position.Y - PosVec3.Y) * (position.Y - PosVec3.Y);
        }

        public virtual bool CanReserve()
        {
            bool canReserve = false;

            if ((RAILPORT_STATE)NodeState == RAILPORT_STATE.EMPTY && ReserveEntity == null && IsReservation == false)
            {
                canReserve = true;
            }

            return canReserve;
        }

        public virtual void Reserve(Foup foup)
        {
            NodeState = RAILPORT_STATE.RESERVED;
            ReserveEntity = foup;
            IsReservation = true;
        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {

        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            Foup foup = port.Entity as Foup;
            SimLog log;

            string prevResourceId;
            string nextResourceId;
            string prevState;
            string nextState;

            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.RESERVE:

                    Reserve(foup);

                    if (SimEngine.Instance.IsAnimation)
                    {
                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.PORT_RESERVE);
                        simLogs.AddLog(log);
                    }

                    break;
                // 1. Port의 Foup이 OHT에 Loading됌.
                // => Port의 Foup 제거, Reserve 초기화
                case EXT_PORT.OHT_LOADING:

                    // Port에 Foup이 있어야 하므로 현재 Port의 상태는 Full
                    if ((RAILPORT_STATE)NodeState == RAILPORT_STATE.FULL)
                    {
                        LoadedEntity = null;
                        ReserveEntity = null;
                        IsReservation = false;
                        NodeState = RAILPORT_STATE.EMPTY;

                        if(SimEngine.Instance.IsAnimation)
                        {
                            log = new SimLog(simTime, simTime + 1, null, this, ANIMATION.UNLOAD);
                            simLogs.AddLog(log);
                        }

                        OHTNode oht = port.OHTNode as OHTNode;

                        // Foup State 변경되기 전 현재 State 저장
                        prevResourceId = foup.CurrentNode.Name;
                        prevState = foup.CurrentState.ToString();
                        // Foup Node/State 변경
                        foup.CurrentNode = oht;
                        foup.CurrentState = FOUP_STATE.OHT;
                        // Foup State 변경된 후 현재 State 저장
                        nextResourceId = foup.CurrentNode.Name;
                        nextState = foup.CurrentState.ToString();
                        // Foup State 변경 기록
                        Scheduler.Instance.RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                        //Console.WriteLine($"SimTime({simTime})에 STB : {this.Name}에 FoupID:" + foup.Name + " Load(RailPort -> OHT)");
                    }

                    break;
                // 2. OHT가 Foup을 Port에 Unloading함.
                // => Port에 Foup 탑재, Reserve에서 Full로 변경
                case EXT_PORT.OHT_UNLOADING:

                    // Port에 Foup이 이미 예약되어 있고 곧 Load될 예정이므로(Port 기준) 현재 Port의 상태는 Reserved
                    if ((RAILPORT_STATE)NodeState == RAILPORT_STATE.RESERVED)
                    {
                        LoadedEntity = foup;
                        LoadedEntity.TotalTravelingTime = LoadedEntity.TotalTravelingTime + (simTime - LoadedEntity.TravelingStartTime);
                        NodeState = RAILPORT_STATE.FULL;

                        if(SimEngine.Instance.IsAnimation)
                        {
                            log = new SimLog(simTime, simTime + 1, foup, this, ANIMATION.LOAD);
                            simLogs.AddLog(log);
                        }
                        //Console.WriteLine($"SimTime({simTime})에 STB : {this.Name}에 FoupID:" + foup.Name + " Load(OHT -> RailPort)");

                        // Foup State 변경되기 전 현재 State 저장
                        prevResourceId = foup.CurrentNode.Name;
                        prevState = foup.CurrentState.ToString();
                        // Foup Node/State 변경
                        foup.CurrentNode = this;
                        foup.CurrentState = FOUP_STATE.BUFFER;
                        // Foup State 변경된 후 현재 State 저장
                        nextResourceId = foup.CurrentNode.Name;
                        nextState = foup.CurrentState.ToString();
                        // Foup State 변경 기록
                        Scheduler.Instance.RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                        // 다음 스케줄의 설비로 갈 수 있는지 다시 확인!

                        FoupHistory nextFoupHistory = foup.Historys.FindNextHistory();

                        // 다음 스케줄이 있다면
                        if(nextFoupHistory != null)
                        {
                            ProcessEqpNode nextEqp = ModelManager.Instance.DicProcessEqpNode[nextFoupHistory.EqpID];

                            ProcessPortNode emptyProcPort = nextEqp.GetEmptyProcessPort();

                            if(emptyProcPort != null)
                            {
                                port = new SimPort(EXT_PORT.RESERVE, this, foup);
                                emptyProcPort.ExternalFunction(simTime, simLogs, port);

                                Scheduler.Instance.SendFoupToEqp(foup, this, emptyProcPort);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
