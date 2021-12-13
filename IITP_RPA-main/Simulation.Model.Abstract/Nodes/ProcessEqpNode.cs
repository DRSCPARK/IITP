using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    /// <summary>
    /// 생산 시뮬레이션 전용 Node
    /// </summary>
    public class ProcessEqpNode : FabSimNode
    {
        #region Variables
        private PROCESS_TYPE _processType;
        private Foup _assignedEntity;
        private Time _totalProcTime;
        private STEP_GROUP _stepGroup;
        private string _processGroup;
        private string _bayName;
        private int _minBatchSize;
        private int _maxBatchSize;
        private bool _isReservation;
        private int _capacity;
        private List<BufferNode> _lstBuffer;
        private List<uint> _processIDs;
        private List<Foup> _reserveList;
        private List<Foup> _internalEntities;
        private List<Foup> _processingEntities;
        private List<Foup> _requestEntities;
        private List<ProcessPortNode> _processPorts;
        private EqpHistorys _remainHistorys;
        private double _processingInterval;
        private DateTime _trackInDateTime;
        private DateTime _availableTrackInDateTime;
        private DateTime _trackOutDateTime;
        private double _width;
        private double _height;
        private int _totalHistoryCount;

        [Browsable(true)]
        [DisplayName("Name")]
        public string ProcessEqpName
        {
            get { return Name; }
        }

        [Browsable(true)]
        public PROCESS_TYPE ProcessType
        {
            get { return _processType; }
            set { _processType = value; }
        }

        [Browsable(true)]
        public STEP_GROUP StepGroup
        {
            get { return _stepGroup; }
            set { _stepGroup = value; }
        }

        [Browsable(true)]
        public string ProcessGroup
        {
            get { return _processGroup; }
            set { _processGroup = value; }
        }

        [Browsable(true)]
        public string BayName
        {
            get 
            {
                if (_bayName == null)
                    return string.Empty;
                else
                    return _bayName; 
            }
            set { _bayName = value; }
        }

        [Browsable(true)]
        public int MinBatchSize
        {
            get { return _minBatchSize; }
            set { _minBatchSize = value; }
        }

        [Browsable(true)]
        public int MaxBatchSize
        {
            get { return _maxBatchSize; }
            set { _maxBatchSize = value; }
        }

        [Browsable(true)]
        public Time TotalProcessingTime
        {
            get { return _totalProcTime; }
            set { _totalProcTime = (Time)value; }
        }

        [Browsable(true)]
        public Foup AssingedEntity
        {
            get { return _assignedEntity; }
        }

        [Browsable(true)]
        public bool IsProcessing
        {
            get
            {
                bool isProcessing = false;

                if ((PROCESSEQP_STATE)NodeState == PROCESSEQP_STATE.PROCESSING)
                    isProcessing = true;

                return isProcessing;
            }
        }

        [Browsable(true)]
        public bool IsReservation
        {
            get { return _isReservation; }
            set { _isReservation = value; }
        }

        [Browsable(true)]
        public List<BufferNode> LstBuffer
        {
            get { return _lstBuffer; }
            set { _lstBuffer = value; }
        }

        [Browsable(true)]
        public List<Foup> ReserveList
        {
            get { return _reserveList; }
            set { _reserveList = value; }
        }

        [Browsable(true)]
        public List<Foup> InternalEntities
        {
            get { return _internalEntities; }
            set { _internalEntities = value; }
        }

        [Browsable(true)]
        public List<Foup> ProcessingEntities
        {
            get { return _processingEntities; }
            set { _processingEntities = value; }
        }

        [Browsable(true)]
        public List<Foup> RequestEntities
        {
            get { return _requestEntities; }
            set { _requestEntities = value; }
        }

        [Browsable(true)]
        public List<Foup> NextEntities
        {
            get { return GetNextFoups(); }
        }

        [Browsable(true)]
        public int Capacity
        {
            get { return _capacity; }
            set { _capacity = value; }
        }

        [Browsable(true)]
        public List<uint> PocessIDs
        {
            get { return _processIDs; }
            set { _processIDs = value; }
        }

        [Browsable(false)]
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [Browsable(false)]
        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        [Browsable(true)]
        public List<ProcessPortNode> ProcessPorts
        {
            get { return _processPorts; }
            set { _processPorts = value; }
        }

        [Browsable(false)]
        public EqpHistorys RemainHistorys
        {
            get { return _remainHistorys; }
            set { _remainHistorys = value; }
        }

        [Browsable(true)]
        public List<EqpHistory> HistoryList
        {
            get { return _remainHistorys.Historys; }
        }

        [Browsable(false)]
        public int TotalHistoryCount
        {
            get { return _totalHistoryCount; }
            set { _totalHistoryCount = value; }
        }
        #endregion

        public ProcessEqpNode(uint ID, string name, PROCESS_TYPE type, Fab fab)
            : base(ID, name, NODE_TYPE.PROCESS, fab)
        {
            _processIDs = new List<uint>();
            _processType = type;
            _reserveList = new List<Foup>();
            _remainHistorys = new EqpHistorys(name);
            NodeState = PROCESSEQP_STATE.IDLE;
        }

        public ProcessEqpNode(uint id, string eqpId, Fab fab, string processGroup, string processType, string stepGroup, string bayName)
    : base(id, eqpId, fab)
        {
            _processPorts = new List<ProcessPortNode>();
            _processGroup = processGroup;
            _processType = (PROCESS_TYPE)Enum.Parse(typeof(PROCESS_TYPE), processType.ToUpper());
            _stepGroup = (STEP_GROUP)Enum.Parse(typeof(STEP_GROUP), stepGroup);
            _bayName = bayName;
            _reserveList = new List<Foup>();
            _internalEntities = new List<Foup>();
            _processingEntities = new List<Foup>();
            _remainHistorys = new EqpHistorys(eqpId);
            _requestEntities = new List<Foup>();
            _trackInDateTime = new DateTime();
            _trackOutDateTime = new DateTime();
            NodeState = PROCESSEQP_STATE.IDLE;
        }

        public ProcessEqpNode(uint ID, Fab fab, string eqpId, string processGroup, string processType, string stepGroup, string bayName, int minBatchSize, int maxBatchSize)
            : base(ID, eqpId, NODE_TYPE.PROCESS, fab)
        {
            _processPorts = new List<ProcessPortNode>();
            _processIDs = new List<uint>();
            _processGroup = processGroup;
            _processType = (PROCESS_TYPE)Enum.Parse(typeof(PROCESS_TYPE), processType.ToUpper());
            _stepGroup = (STEP_GROUP)Enum.Parse(typeof(STEP_GROUP), stepGroup);
            _bayName = bayName;
            _minBatchSize = minBatchSize;
            _maxBatchSize = maxBatchSize;
            _reserveList = new List<Foup>();
            _internalEntities = new List<Foup>();
            _processingEntities = new List<Foup>();
            _remainHistorys = new EqpHistorys(eqpId);
            _requestEntities = new List<Foup>();
            _processingInterval = 600;
            _trackInDateTime = new DateTime();
            _trackOutDateTime = new DateTime();
            NodeState = PROCESSEQP_STATE.IDLE;
        }

        public ProcessEqpNode(uint ID, string name, Fab fab, int capacity, PROCESS_TYPE type)
    : base(ID, name, NODE_TYPE.PROCESS, fab)
        {
            _processPorts = new List<ProcessPortNode>();
            _processType = type;
            _lstBuffer = new List<BufferNode>();
            _capacity = capacity;
            _remainHistorys = new EqpHistorys(name);
            _isReservation = false;
            _reserveList = new List<Foup>();
            NodeState = PROCESSEQP_STATE.IDLE;
        }

        public ProcessEqpNode(uint ID, string name, Fab fab, int capacity, List<uint> processIDs, PROCESS_TYPE type)
            : base(ID, name, NODE_TYPE.PROCESS, fab)
        {
            _processPorts = new List<ProcessPortNode>();
            _processType = type;
            _lstBuffer = new List<BufferNode>();
            _capacity = capacity;
            _remainHistorys = new EqpHistorys(name);
            _processIDs = processIDs;
            _isReservation = false;
            _reserveList = new List<Foup>();
        }

        public ProcessEqpNode(uint ID, Vector3 pos, Vector3 size, string name, Fab fab, int capacity, List<uint> processIDs, PROCESS_TYPE type)
            : base(ID, name, pos, size, fab)
        {
            _processPorts = new List<ProcessPortNode>();
            _processType = type;
            _lstBuffer = new List<BufferNode>();
            _capacity = capacity;
            _remainHistorys = new EqpHistorys(name);
            _processIDs = processIDs;
            _isReservation = false;
            _reserveList = new List<Foup>();
        }

        public ProcessEqpNode(uint ID, Vector3 pos, Vector3 size, string name, Fab fab, int capacity, List<uint> processIDs, PROCESS_TYPE type, RailLineNode rl)
            : base(ID, name, pos, size, fab)
        {
            _processPorts = new List<ProcessPortNode>();
            _processType = type;
            _lstBuffer = new List<BufferNode>();
            _capacity = capacity;
            _remainHistorys = new EqpHistorys(name);
            _processIDs = processIDs;
            _isReservation = false;
            _reserveList = new List<Foup>();
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);

            NodeState = PROCESSEQP_STATE.IDLE;

            _reserveList = new List<Foup>();
            _internalEntities = new List<Foup>();
            _processingEntities = new List<Foup>();
            _totalProcTime = 0;


            if (ModelManager.Instance.SimType == SIMULATION_TYPE.PRODUCTION)
            {
                foreach (ProcessPortNode processPort in _processPorts)
                {
                    processPort.InitializeNode(evtCal);
                }

                SetProcessingInterval();

                Console.WriteLine($"{Name} {_processingInterval}");

                SimPort port;
                double totalSeconds;
                Time eventTime;
                Time arrivalTime;

                _totalHistoryCount = _remainHistorys.Historys.Count;

                // 첫번째 공정
                foreach (EqpHistory eqpHistory in _remainHistorys.Historys.Where(h => h.Sequence == 1).ToList())
                {
                    Foup foup = ModelManager.Instance.Foups[eqpHistory.FoupID];

                    FoupHistory foupHistory = foup.Historys.FindNextHistory();

                    totalSeconds = (eqpHistory.StartTime - SimEngine.Instance.StartDateTime).TotalSeconds;

                    eventTime = new Time(totalSeconds);

                    // 초기재공 TRACK-IN
                    if (eventTime == new Time(0))
                    {
                        // 초기재공 때는 WAIT였다가 시뮬레이션 시작하자마자 바로 RUN인 상태의 경우
                        if (foup.CurrentNode is RailPortNode)
                        {
                            RailPortNode stb = foup.CurrentNode as RailPortNode;
                            stb.LoadedEntity = null;
                            stb.ReserveEntity = null;
                            stb.IsReservation = false;
                            stb.NodeState = RAILPORT_STATE.EMPTY;
                        }

                        foup.CurrentNode = this;
                        foup.CurrentState = FOUP_STATE.WAIT_TRACK_IN;

                        // InternalEntities에 넣기
                        if (_internalEntities.Contains(foup) is false)
                            _internalEntities.Add(foup);

                        _availableTrackInDateTime = SimEngine.Instance.SimDateTime;

                        // Track-In Event
                        port = new SimPort(INT_PORT.TRACK_IN, this, foup);
                        // EqpHistory를 Port에 삽입
                        eqpHistory.IsUsed = true;
                        port.EqpHistory = eqpHistory;
                        // FoupHistory를 Port에 삽입
                        foupHistory.IsUsed = true;
                        foupHistory.SimStartTime = _availableTrackInDateTime;
                        port.FoupHistory = foupHistory;
                        // TRACK-IN
                        EvtCalendar.AddEvent(eventTime, this, port);
                    }
                }

                // ArrivalTime이 시뮬레이션 시작 시각과 같으면서 첫 스케줄이 현재 장비인 Foup은 미리 InternalEntities에 넣어놓는다.
                switch (ProcessType)
                {
                    // INLINE은 5번째 공정 수행하는 Foup들까지 InternalEntities에 넣어놓을 수 있으면 넣어놓는다.
                    case PROCESS_TYPE.INLINE:
                        #region INLINE
                        for (int sequence = 2; sequence <= 5; sequence++)
                        {
                            List<EqpHistory> sequenceHistorys = _remainHistorys.Historys.Where(h => h.Sequence == sequence).ToList();

                            if (sequenceHistorys.Any() is false)
                                break;

                            bool canEnter = true;

                            foreach (EqpHistory eqpHistory in sequenceHistorys)
                            {
                                Foup foup = ModelManager.Instance.Foups[eqpHistory.FoupID];

                                FoupHistory foupHistory = foup.Historys.FindNextHistory();

                                // 만약 n번째 공정 수행하는 Foup 중 Foup의 첫번째 스케줄이 현재 장비가 아닐 경우 for문 종료
                                if (foupHistory.EqpID != this.Name || foupHistory.ArrivalTime != SimEngine.Instance.StartDateTime || foupHistory.Sequence != 1)
                                {
                                    canEnter = false;
                                    break;
                                }

                                if (canEnter)
                                {
                                    // 초기재공 때는 WAIT였다가 시뮬레이션 시작하자마자 바로 RUN인 상태의 경우
                                    if (foup.CurrentNode is RailPortNode)
                                    {
                                        RailPortNode stb = foup.CurrentNode as RailPortNode;
                                        stb.LoadedEntity = null;
                                        stb.ReserveEntity = null;
                                        stb.IsReservation = false;
                                        stb.NodeState = RAILPORT_STATE.EMPTY;
                                    }

                                    foup.IsInitWait = true;
                                    foup.CurrentNode = this;
                                    foup.CurrentState = FOUP_STATE.WAIT_TRACK_IN;

                                    // InternalEntities에 넣기
                                    if (_internalEntities.Contains(foup) is false)
                                        _internalEntities.Add(foup);
                                    else
                                        ;

                                    // TRACK-IN 등록

                                    // Track-In Event
                                    port = new SimPort(INT_PORT.TRACK_IN, this, foup);
                                    // EqpHistory를 Port에 삽입
                                    eqpHistory.IsUsed = true;
                                    port.EqpHistory = eqpHistory;
                                    // FoupHistory를 Port에 삽입
                                    foupHistory.IsUsed = true;
                                    foupHistory.SimStartTime = _availableTrackInDateTime;
                                    port.FoupHistory = foupHistory;
                                    //INSPECT
                                    if (eqpHistory.StartTime != foupHistory.StartTime)
                                        ;
                                    // TRACK-IN
                                    eventTime = new Time((foupHistory.StartTime - SimEngine.Instance.StartDateTime).TotalSeconds);

                                    EvtCalendar.AddEvent(eventTime, this, port);

                                    _availableTrackInDateTime = foupHistory.StartTime.AddSeconds(_processingInterval);
                                }
                            }

                            if (canEnter is false)
                                break;
                        }
                        #endregion
                        break;
                    // BATCHINLINE은 3번째 공정 수행하는 Foup들까지 InternalEntities에 넣어놓을 수 있으면 넣어놓는다.
                    case PROCESS_TYPE.BATCHINLINE:
                        #region BATCHINLINE
                        // 두번째 공정

                        bool canSecondEnter = true;

                        List<EqpHistory> secondHistorys = _remainHistorys.Historys.Where(h => h.Sequence == 2).ToList();

                        if (secondHistorys.Any() is false)
                            canSecondEnter = false;

                        foreach (EqpHistory secondEqpHistory in secondHistorys)
                        {
                            Foup foup = ModelManager.Instance.Foups[secondEqpHistory.FoupID];

                            FoupHistory foupHistory = foup.Historys.FindNextHistory();

                            if (foupHistory.EqpID != this.Name || foupHistory.ArrivalTime != SimEngine.Instance.StartDateTime || foupHistory.Sequence != 1)
                            {
                                canSecondEnter = false;
                                break;
                            }
                        }

                        // 두번째 공정 Foup들 등록 가능
                        if (canSecondEnter)
                        {
                            foreach (EqpHistory secondEqpHistory in secondHistorys)
                            {
                                Foup foup = ModelManager.Instance.Foups[secondEqpHistory.FoupID];

                                FoupHistory foupHistory = foup.Historys.FindNextHistory();

                                // 초기재공 때는 WAIT였다가 시뮬레이션 시작하자마자 바로 RUN인 상태의 경우
                                if (foup.CurrentNode is RailPortNode)
                                {
                                    RailPortNode stb = foup.CurrentNode as RailPortNode;
                                    stb.LoadedEntity = null;
                                    stb.ReserveEntity = null;
                                    stb.IsReservation = false;
                                    stb.NodeState = RAILPORT_STATE.EMPTY;
                                }

                                foup.IsInitWait = true;
                                foup.CurrentNode = this;
                                foup.CurrentState = FOUP_STATE.WAIT_TRACK_IN;

                                // InternalEntities에 넣기
                                if (_internalEntities.Contains(foup) is false)
                                    _internalEntities.Add(foup);
                                else
                                    ;

                                // TRACK-IN 등록

                                // Track-In Event
                                port = new SimPort(INT_PORT.TRACK_IN, this, foup);
                                // EqpHistory를 Port에 삽입
                                secondEqpHistory.IsUsed = true;
                                port.EqpHistory = secondEqpHistory;
                                // FoupHistory를 Port에 삽입
                                foupHistory.IsUsed = true;
                                foupHistory.SimStartTime = _availableTrackInDateTime;
                                port.FoupHistory = foupHistory;
                                //INSPECT
                                if (secondEqpHistory.StartTime != foupHistory.StartTime)
                                    ;
                                // TRACK-IN
                                eventTime = new Time((secondEqpHistory.StartTime - SimEngine.Instance.StartDateTime).TotalSeconds);
                                EvtCalendar.AddEvent(eventTime, this, port);
                                _availableTrackInDateTime = secondEqpHistory.StartTime.AddSeconds(_processingInterval);
                            }

                            bool canThirdEnter = true;

                            List<EqpHistory> thirdHistorys = _remainHistorys.Historys.Where(h => h.Sequence == 3).ToList();

                            if (thirdHistorys.Any() is false)
                                canThirdEnter = false;

                            foreach (EqpHistory thirdEqpHistory in thirdHistorys)
                            {
                                Foup foup = ModelManager.Instance.Foups[thirdEqpHistory.FoupID];

                                FoupHistory foupHistory = foup.Historys.FindNextHistory();

                                if (foupHistory.EqpID != this.Name || foupHistory.ArrivalTime != SimEngine.Instance.StartDateTime || foupHistory.Sequence != 1)
                                {
                                    canThirdEnter = false;
                                    break;
                                }
                            }

                            if (canThirdEnter)
                            {
                                foreach (EqpHistory thirdEqpHistory in thirdHistorys)
                                {
                                    Foup foup = ModelManager.Instance.Foups[thirdEqpHistory.FoupID];

                                    FoupHistory foupHistory = foup.Historys.FindNextHistory();

                                    // 초기재공 때는 WAIT였다가 시뮬레이션 시작하자마자 바로 RUN인 상태의 경우
                                    if (foup.CurrentNode is RailPortNode)
                                    {
                                        RailPortNode stb = foup.CurrentNode as RailPortNode;
                                        stb.LoadedEntity = null;
                                        stb.ReserveEntity = null;
                                        stb.IsReservation = false;
                                        stb.NodeState = RAILPORT_STATE.EMPTY;
                                    }

                                    foup.IsInitWait = true;
                                    foup.CurrentNode = this;
                                    foup.CurrentState = FOUP_STATE.WAIT_TRACK_IN;

                                    // InternalEntities에 넣기
                                    if (_internalEntities.Contains(foup) is false)
                                        _internalEntities.Add(foup);
                                    else
                                        ;

                                    // TRACK-IN 등록

                                    // Track-In Event
                                    port = new SimPort(INT_PORT.TRACK_IN, this, foup);
                                    // EqpHistory를 Port에 삽입
                                    thirdEqpHistory.IsUsed = true;
                                    port.EqpHistory = thirdEqpHistory;
                                    // FoupHistory를 Port에 삽입
                                    foupHistory.IsUsed = true;
                                    foupHistory.SimStartTime = _availableTrackInDateTime;
                                    port.FoupHistory = foupHistory;
                                    //INSPECT
                                    if (thirdEqpHistory.StartTime != foupHistory.StartTime)
                                        ;
                                    // TRACK-IN
                                    eventTime = new Time((thirdEqpHistory.StartTime - SimEngine.Instance.StartDateTime).TotalSeconds);

                                    EvtCalendar.AddEvent(eventTime, this, port);

                                    _availableTrackInDateTime = thirdEqpHistory.StartTime.AddSeconds(_processingInterval);
                                }
                            }
                        }
                        #endregion
                        break;
                    // LOTBATCH는 2번째 공정 수행하는 Foup들까지 InternalEntities에 넣어놓을 수 있으면 넣어놓는다.
                    case PROCESS_TYPE.LOTBATCH:
                        #region LOTBATCH
                        // 두번째 공정
                        foreach (EqpHistory history in _remainHistorys.Historys.Where(h => h.Sequence == 2).ToList())
                        {
                            Foup foup = ModelManager.Instance.Foups[history.FoupID];

                            FoupHistory nextFoupHistory = foup.Historys.FindNextHistory();

                            // 만약 두번째 공정 수행하는 Foup 중 Foup의 첫번째 스케줄이 현재 장비가 아닐 경우 for문 종료
                            if (nextFoupHistory.EqpID != this.Name || nextFoupHistory.Sequence != 1)
                                break;

                            totalSeconds = (history.ArrivalTime - SimEngine.Instance.StartDateTime).TotalSeconds;

                            arrivalTime = new Time(totalSeconds);

                            // 두번째 공정이나 도착한 시간이 시뮬레이션 시작 시각과 같으므로 설비에서 대기
                            if (arrivalTime == new Time(0))
                            {
                                // 초기재공 때는 WAIT였다가 시뮬레이션 시작하자마자 바로 RUN인 상태의 경우
                                if (foup.CurrentNode is RailPortNode)
                                {
                                    RailPortNode stb = foup.CurrentNode as RailPortNode;
                                    stb.LoadedEntity = null;
                                    stb.ReserveEntity = null;
                                    stb.IsReservation = false;
                                    stb.NodeState = RAILPORT_STATE.EMPTY;
                                }

                                foup.IsInitWait = true;
                                foup.CurrentNode = this;
                                foup.CurrentState = FOUP_STATE.WAIT;

                                // InternalEntities에 넣기
                                if (_internalEntities.Contains(foup) is false)
                                    _internalEntities.Add(foup);
                                else
                                    ;
                            }
                        }
                        #endregion
                        break;
                }
            }
        }
        public void SetProcessingInterval()
        {
            // 투입 간격은 설비가 INLINE / BATCHINLINE인 경우에만 고려
            if (ProcessType == PROCESS_TYPE.INLINE || ProcessType == PROCESS_TYPE.BATCHINLINE)
            {
                if(_remainHistorys.Historys.Any() is false)
                {
                    _processingInterval = 0;
                    return;
                }    

                double processingInterval = int.MaxValue;

                // 공정 시작 시각이 시뮬레이션 시작 시각과 같지 않은 EqpHistory 조회
                List<EqpHistory> delayStartHistorys = _remainHistorys.Historys.Where(history => history.StartTime != SimEngine.Instance.StartDateTime).ToList();

                List<int> sequenceList = delayStartHistorys.Select(history => history.Sequence).Distinct().ToList();

                foreach(int sequence in sequenceList)
                {
                    if (sequence == sequenceList.Max())
                        break;

                    EqpHistory prevHistory = delayStartHistorys.Where(history => history.Sequence == sequence).First();
                    EqpHistory nextHistory = delayStartHistorys.Where(history => history.Sequence == sequence + 1).First();

                    double timeDiff = (nextHistory.StartTime - prevHistory.StartTime).TotalSeconds;

                    if (timeDiff <= processingInterval)
                        processingInterval = timeDiff;
                }

                _processingInterval = processingInterval;
            } 
            // 나머지 설비는 투입 간격 고려하지 않는다.
            else
                _processingInterval = 0;
        }

        public override List<string> SaveNode()
        {
            List<string> saveData = base.SaveNode();

            return saveData;
        }

        public void CreatePort(RailLineNode line)
        {
            int portCount = 4;
            double portSize = Properties.Settings.Default.PortWidthLength;
            for (int i = 0; i < portCount; i++)
            {
                ProcessPortNode port = ModelManager.Instance.AddProcessPort(this);

                if (line.StartPoint.X > PosVec3.X) // line이 오른쪽에 있을 때
                {
                    port.PosVec3 = new Geometry.Vector3(PosVec3.X + Size.X / 2 + portSize / 2, PosVec3.Y - Size.Y / 2 + i * portSize + portSize / 2, 10);
                }
                else // 라인이 왼쪽에 있을 때
                {
                    port.PosVec3 = new Geometry.Vector3(PosVec3.X - Size.X / 2 + portSize / 2, PosVec3.Y - Size.Y / 2 + i * portSize + portSize / 2, 10);
                }

                SimLink evtLink_1 = ModelManager.Instance.AddEvtSimLink(port, this);
                port.EVTOutLink = evtLink_1;
                this.EVTInLinks.Add(evtLink_1);

                SimLink evtLink_2 = ModelManager.Instance.AddEvtSimLink(this, port);
                this.EVTOutLink = evtLink_2;
                port.EVTInLinks.Add(evtLink_2);

                _connectPort.Add(port);
            }
        }

        public RailPortNode GetNearestStation(Vector3 point, RailLineNode line)
        {
            RailPortNode nearestStation = null;
            double shortestDistance = 0;

            foreach (RailPortNode port in line.DicRailPort)
            {
                if (nearestStation == null)
                {
                    nearestStation = port;
                    shortestDistance = Math.Abs(point.Y - nearestStation.PosVec3.Y);
                }

                double distance = Math.Abs(point.Y - port.PosVec3.Y);

                if (distance < shortestDistance)
                {
                    nearestStation = port;
                    shortestDistance = Math.Abs(point.Y - nearestStation.PosVec3.Y);
                }
            }

            return nearestStation;
        }

        protected virtual void RecordTotalProcTime(Time prcTime)
        {
            _totalProcTime += prcTime;
        }

        public int GetEmptyPortCount()
        {
            int count = 0;
            foreach (RailPortNode port in _connectPort)
            {
                if (port.NodeState.Equals(RAILPORT_STATE.EMPTY))
                {
                    count++;
                }
            }

            if (count == 0)
            {
                //STB에서 기다리는 entity가 있는지 check (- 해줄것)
            }

            return count;
        }

        public bool CanReserve()
        {
            bool canReserve = false;

            int outWaitCount = CheckOutWaitFoup();

            // ProcessType에 따라 예약 기준이 다르다.
            switch(ProcessType)
            {
                #region ProcessType Case
                case PROCESS_TYPE.INLINE:
                    // **예약 가능한 경우
                    // 1. Eqp의 OutWait 상태인 Foup이 4개 미만인 경우
                    // 2. InternalEntities 개수(15개)도 고려해야 하지만 그건 이따가 고려하기.
                    // !! Inline이기 때문에 장비가 Processing 중이어도 일단 무관함.

                    // 예약 가능 경우 1번 Case
                    if (outWaitCount < 4)
                        canReserve = true;
                    
                    break;
                case PROCESS_TYPE.BATCHINLINE:
                    // **예약 가능한 경우
                    // 1. Eqp의 OutWait 상태인 Foup이 4개 미만인 경우
                    // 2. InternalEntities 개수(15개)도 고려해야 하지만 그건 이따가 고려하기.
                    // !! BatchInline도 어쨌거나 Inline이기 때문에 장비가 Processing 중이어도 일단 무관함.

                    // 예약 가능 경우 1번 Case
                    if (outWaitCount < 4)
                        canReserve = true;

                    break;
                case PROCESS_TYPE.LOTBATCH:
                    // **예약 가능한 경우
                    // 1. Eqp의 OutWait 상태인 Foup이 4개 미만인 경우
                    // 2. InternalEntities 개수(15개)도 고려해야 하지만 그건 이따가 고려하기.
                    // !! LotBatch이기 때문에 장비가 Processing 중이면 TrackIn 불가..

                    // 예약 가능 경우 1번 Case
                    if (outWaitCount < 4)
                        canReserve = true;

                    break;
                // COMPLETE는 INLINE과 동일한 로직으로 처리
                case PROCESS_TYPE.COMPLETE:
                    // **예약 가능한 경우
                    // 1. Eqp의 OutWait 상태인 Foup이 4개 미만인 경우
                    // !! Complete의 경우 InternalEntities의 수용량이 무한하다고 가정
                    // !! Inline이기 때문에 장비가 Processing 중이어도 일단 무관함.

                    // 예약 가능 경우 1번 Case
                    if (outWaitCount < 4)
                        canReserve = true;
                    break;
                default:
                    break;
                #endregion
            }

            return canReserve;
        }

        // 해당 장비에 Foup 예약 함수
        public void Reserve(Foup foup)
        {
            // 예약 리스트에 Foup 추가
            _reserveList.Add(foup);
        }

        public int CheckOutWaitFoup()
        {
            int checkedFoups = 0;

            foreach(Foup foup in _internalEntities)
            {
                if (foup.CurrentState == FOUP_STATE.OUT_WAIT)
                    checkedFoups++;
            }

            return checkedFoups;
        }

        // 다음에 TrackIn시켜야 할 Foup 리스트.. (Sequence로 판단)
        public List<Foup> GetNextFoups()
        {
            List<Foup> foups = new List<Foup>();

            List<EqpHistory> nextHistories = _remainHistorys.FindNextHistories();

            foreach(string foupId in nextHistories.Select(history => history.FoupID))
            {
                Foup foup = ModelManager.Instance.Foups[foupId];
                foups.Add(foup);
            }

            return foups;
        }

        // Foup의 다음 스케줄이 해당 장비인데 Buffer에서 대기 중인 Foup 리스트 조회
        public List<Foup> NotArriveEntities()
        {
            List<Foup> notArriveEntities = new List<Foup>();

            foreach (string foupId in _remainHistorys.Historys.Select(history => history.FoupID))
            {
                Foup foup = ModelManager.Instance.Foups[foupId];

                FoupHistory nextHistory = foup.Historys.FindNextHistory();

                // Foup의 다음 스케줄이 있다.
                if (nextHistory != null)
                {
                    // 그 다음 스케줄이 해당 장비이면서 Buffer 위에 있다.
                    if ((nextHistory.EqpID == this.Name) && foup.CurrentState == FOUP_STATE.BUFFER)
                    { notArriveEntities.Add(foup); }
                }
            }

            return notArriveEntities;
        }

        public List<Foup> GetOutWaitFoups()
        {
            List<Foup> outWaitFoups = new List<Foup>();

            outWaitFoups = _internalEntities.Where(foup => foup.CurrentState == FOUP_STATE.OUT_WAIT).ToList();

            return outWaitFoups;
        }

        public ProcessPortNode GetEmptyProcessPort()
        {
            // 검사..
            foreach(ProcessPortNode procPort in _processPorts)
            {
                if(procPort.LoadedEntity != null)
                {
                    Foup loadedEntity = procPort.LoadedEntity as Foup;

                    if (loadedEntity.CurrentNode.Name != procPort.Name)
                        ;
                }
            }

            for (int i = 0; i < _processPorts.Count; i++)
            {
                if ((PROCESSPORT_STATE)_processPorts[i].NodeState == PROCESSPORT_STATE.EMPTY)
                {
                    return _processPorts[i];
                }
            }

            return null;
        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            SimLog log;

            INT_PORT inPort = (INT_PORT)port.PortType;

            Foup foup;
            EqpHistory eqpHistory;
            FoupHistory foupHistory;
            FoupHistory nextFoupHistory;
            List<Foup> notArriveFoups;
            List<Foup> nextFoups;

            string prevResourceId;
            string nextResourceId;
            string prevState;
            string nextState;

            switch (inPort)
            {
                case INT_PORT.TRACK_IN:

                    _trackInDateTime = SimEngine.Instance.SimDateTime;

                    foup = port.Entity as Foup;
                    foup.GenerateTime = simTime;

                    // Foup State 변경되기 전 현재 State 저장
                    prevResourceId = foup.CurrentNode.Name;
                    prevState = foup.CurrentState.ToString();
                    // Foup State 변경
                    foup.CurrentState = FOUP_STATE.PROCESSING;
                    // Foup State 변경된 후 현재 State 저장
                    nextResourceId = foup.CurrentNode.Name;
                    nextState = foup.CurrentState.ToString();
                    // Foup State 변경 기록
                    Scheduler.Instance.RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                    // processingEntities에 넣기
                    _processingEntities.Add(foup);

                    NodeState = PROCESSEQP_STATE.PROCESSING;

                    // Track-Out Event
                    eqpHistory = port.EqpHistory as EqpHistory;
                    foupHistory = port.FoupHistory as FoupHistory;
                    // Eqp history 삭제
                    this._remainHistorys.RemoveHistory(eqpHistory);
                    // Foup history 삭제
                    foup.Historys.RemoveHistory(foupHistory);

                    // 시뮬레이션 상에서의 Track_In 시각 저장 후 다시 port에 넣기. Track_Out 때 종료 시각 저장하기 위해
                    eqpHistory.SimStartTime = SimEngine.Instance.SimDateTime;

                    // 프로세싱 타임 (단위 : 초)
                    double processingTimeSeconds = (eqpHistory.EndTime - eqpHistory.StartTime).TotalSeconds;

                    Time eventTime = new Time(processingTimeSeconds);

                    Console.WriteLine($"SimTime({simTime})에 {this.Name}에서 Entity({foup.Name}) TRACK-IN({ProcessType.ToString()}) " +
                        $"RealDT : {eqpHistory.StartTime.ToString()} SimDT : {SimEngine.Instance.SimDateTime.ToString()}....");

                    if (SimEngine.Instance.IsAnimation)
                    {
                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.BUSY);
                        simLogs.AddLog(log);
                    }

                    port = new SimPort(INT_PORT.TRACK_OUT, this, foup);
                    port.EqpHistory = eqpHistory;
                    port.FoupHistory = foupHistory;
                    EvtCalendar.AddEvent(simTime + eventTime, this, port);

                    // 다음 스케줄이 현재 장비인 Foup 데려오기
                    nextFoups = GetNextFoups();

                    foreach(Foup nextFoup in nextFoups)
                    {
                        if(nextFoup.CurrentState == FOUP_STATE.BUFFER)
                        {
                            nextFoupHistory = nextFoup.Historys.FindNextHistory();

                            if(nextFoupHistory.EqpID == this.Name)
                            {
                                ProcessPortNode emptyProcPort = GetEmptyProcessPort();

                                if (emptyProcPort != null)
                                {
                                    port = new SimPort(EXT_PORT.RESERVE, this, nextFoup);
                                    emptyProcPort.ExternalFunction(simTime, simLogs, port);

                                    Scheduler.Instance.SendFoupToEqp(nextFoup, nextFoup.CurrentNode as RailPortNode, emptyProcPort);
                                }
                            }
                        }
                    }

                    break;
                case INT_PORT.TRACK_OUT:

                    _trackOutDateTime = SimEngine.Instance.SimDateTime;

                    SimEngine.Instance.FinishedEqpPlanCount += 1;

                    foup = port.Entity as Foup;

                    // initWait 해제
                    foup.IsInitWait = false;

                    // Foup State 변경되기 전 현재 State 저장
                    prevResourceId = foup.CurrentNode.Name;
                    prevState = foup.CurrentState.ToString();
                    // Foup State 변경
                    foup.CurrentState = FOUP_STATE.OUT_WAIT;
                    // Foup State 변경된 후 현재 State 저장
                    nextResourceId = foup.CurrentNode.Name;
                    nextState = foup.CurrentState.ToString();
                    // Foup State 변경 기록
                    Scheduler.Instance.RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                    // ProcessingEntities에서 빼기
                    _processingEntities.Remove(foup);

                    // processingEntities에 Foup이 없으면 IDLE로 상태 변경
                    if (_processingEntities.Any() is false)
                    {
                        NodeState = PROCESSEQP_STATE.IDLE;

                        Console.WriteLine($"SimTime({simTime})에 Eqp : {this.Name} IDLE...");

                        if (SimEngine.Instance.IsAnimation)
                        {
                            log = new SimLog(simTime, simTime + 1, this, ANIMATION.IDLE);
                            simLogs.AddLog(log);
                        }
                    }

                    // Track_In 때의 EqpHistory 다시 불러오기
                    eqpHistory = port.EqpHistory as EqpHistory;
                    // 시뮬레이션 상에서의 종료 시각 저장
                    eqpHistory.SimEndTime = SimEngine.Instance.SimDateTime;
                    // EqpHistory 처리 완료
                    Scheduler.Instance.FinishEqpHistory(eqpHistory);

                    Console.WriteLine($"SimTime({simTime})에 {this.Name}에서 Entity({foup.Name}) TRACK-OUT({ProcessType.ToString()})   " +
                        $"Remain Foups : ({_remainHistorys.Historys.Count}/{_totalHistoryCount})  Finished : {SimEngine.Instance.FinishedEqpPlanCount}" +
                        $"RealDT : {eqpHistory.EndTime.ToString()} SimDT : {SimEngine.Instance.SimDateTime}....");

                    // ***NEXT_EQP_RESERVE
                    // 다음 스케줄이 있다면 다음 장비의 예약 리스트에 등록 / 다음 스케줄의 장비가 현재 장비일 경우 Wait로 상태 변경
                    nextFoupHistory = foup.Historys.FindNextHistory();

                    // 스케줄이 있을 경우
                    if (nextFoupHistory != null)
                    {
                        string nextEqpID = nextFoupHistory.EqpID;

                        // Foup의 다음 스케줄이 현재 장비일 경우 Wait로 상태 변경 후 Switch 종료
                        if(nextEqpID == this.Name)
                        {
                            // Foup State 변경되기 전 현재 State 저장
                            prevResourceId = foup.CurrentNode.Name;
                            prevState = foup.CurrentState.ToString();
                            // Foup State 변경
                            foup.CurrentState = FOUP_STATE.WAIT;
                            // Foup State 변경된 후 현재 State 저장
                            nextResourceId = foup.CurrentNode.Name;
                            nextState = foup.CurrentState.ToString();
                            // Foup State 변경 기록
                            Scheduler.Instance.RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);
                        }
                        // Foup의 다음 스케줄이 다른 장비일 경우 다른 장비에 예약
                        else
                        {
                            ProcessEqpNode nextEqp = ModelManager.Instance.DicProcessEqpNode[nextEqpID];

                            nextEqp.Reserve(foup);

                            // ***TRY-RESERVE-PROCESSPORT
                            // Track-out되서 나온 Foup이 ProcessPort로 나가기 위해 예약 시도
                            ProcessPortNode processPort = GetEmptyProcessPort();

                            if (processPort != null)
                            {
                                if (processPort.CanReserve())
                                {
                                    // Processport에 예약
                                    processPort.Reserve(foup);

                                    _internalEntities.Remove(foup);

                                    // Processport 위에 foup 올려놓기
                                    foup.CurrentNode = processPort;
                                    processPort.LoadedEntity = foup;
                                    processPort.NodeState = PROCESSPORT_STATE.FULL;

                                    if(SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, foup, processPort, ANIMATION.LOAD);
                                        simLogs.AddLog(log);
                                    }

                                    Scheduler.Instance.SendFoupToNext(foup, processPort);
                                }
                            }
                        }
                    }

                    // ***CHECK-AVAILABLE-TRACK-IN-FOUP
                    // 현재 Foup이 Eqp에 들어오고 나서 Track-In 가능한 Foup 선별 후 Track-In 이벤트 등록

                    #region LOTBATCH
                    // Eqp의 Type이 LotBatch이고 현재 공정 수행 중이면 Track-In 불가
                    if (ProcessType == PROCESS_TYPE.LOTBATCH && IsProcessing)
                        break;

                    // LOTBATCH는 다음 처리할 Foup이 모두 InternalEntities에 있어야 하며 설비가 IDLE한 상황이어야 한다.
                    if(ProcessType == PROCESS_TYPE.LOTBATCH && IsProcessing is false)
                    {
                        // LOTBATCH 타입 설비에 IDLE한 상태인 경우

                        nextFoups = GetNextFoups();

                        bool isAllIn = true;

                        foreach(Foup nextFoup in nextFoups)
                        {
                            if (_internalEntities.Contains(nextFoup) is false || nextFoup.CurrentState != FOUP_STATE.WAIT)
                            {
                                isAllIn = false;
                                break;
                            }
                        } 

                        if(isAllIn)
                        {
                            foreach(Foup nextFoup in nextFoups)
                            {
                                // WAIT-TRACK-IN 상태로 변경

                                // Foup State 변경되기 전 현재 State 저장
                                prevResourceId = nextFoup.CurrentNode.Name;
                                prevState = nextFoup.CurrentState.ToString();
                                // Foup State 변경
                                nextFoup.CurrentState = FOUP_STATE.WAIT_TRACK_IN;
                                // Foup State 변경된 후 현재 State 저장
                                nextResourceId = nextFoup.CurrentNode.Name;
                                nextState = nextFoup.CurrentState.ToString();
                                // Foup State 변경 기록
                                Scheduler.Instance.RecordFoupHistory(nextFoup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                                _availableTrackInDateTime = SimEngine.Instance.SimDateTime;

                                // TRACK-IN 이벤트 등록
                                port = new SimPort(INT_PORT.TRACK_IN, this, nextFoup);
                                // EqpHistory를 Port에 삽입
                                eqpHistory = _remainHistorys.FindFirstHistory(nextFoup.Name);
                                eqpHistory.IsUsed = true;
                                port.EqpHistory = eqpHistory;
                                // FoupHistory를 Port에 삽입
                                foupHistory = nextFoup.Historys.FindNextHistory();
                                foupHistory.IsUsed = true;
                                foupHistory.SimStartTime = _availableTrackInDateTime;
                                port.FoupHistory = foupHistory;
                                //INSPECT
                                if (eqpHistory.StartTime != foupHistory.StartTime)
                                    ;
                                // TRACK-IN 
                                EvtCalendar.AddEvent(simTime, this, port);
                            }
                        }
                    }
                    #endregion

                    #region INLINE & BATCHINLINE
                    // INLINE & BATCHINLINE.. 투입 간격 고려
                    // Eqp의 남은 Sequence 리스트
                    if(ProcessType == PROCESS_TYPE.INLINE || ProcessType == PROCESS_TYPE.BATCHINLINE)
                    {
                        List<int> sequenceList = _remainHistorys.Historys.Select(history => history.Sequence).Distinct().ToList();

                        // Track-In 가능 시각
                        if (_availableTrackInDateTime <= SimEngine.Instance.SimDateTime)
                        { _availableTrackInDateTime = SimEngine.Instance.SimDateTime; }

                        foreach (int sequence in sequenceList)
                        {
                            List<string> candidateFoupNames = _remainHistorys.Historys.Where(history => history.Sequence == sequence).Select(history => history.FoupID).ToList();

                            List<Foup> candidateFoups = new List<Foup>();

                            foreach (string candidateFoupName in candidateFoupNames)
                            {
                                Foup candidateFoup = ModelManager.Instance.Foups[candidateFoupName];

                                candidateFoups.Add(candidateFoup);
                            }

                            bool isReadyToTrackIn = true;
                            bool isWaitToTrackIn = false;

                            foreach (Foup candidateFoup in candidateFoups)
                            {
                                if (_internalEntities.Contains(candidateFoup))
                                {
                                    if (candidateFoup.CurrentState == FOUP_STATE.WAIT)
                                        continue;
                                    else if (candidateFoup.CurrentState == FOUP_STATE.WAIT_TRACK_IN)
                                    {
                                        isWaitToTrackIn = true;
                                        break;
                                    }
                                    else if (candidateFoup.CurrentState == FOUP_STATE.PROCESSING)
                                    {
                                        isReadyToTrackIn = false;
                                        break;
                                    }
                                    else
                                        ;
                                }
                                else
                                {
                                    isReadyToTrackIn = false;
                                    break;
                                }
                            }

                            if (isWaitToTrackIn)
                                continue;

                            // Eqp의 InternalEntities에 다음 Foup들이 모두 있다면
                            if (isReadyToTrackIn)
                            {
                                foreach (Foup candidateFoup in candidateFoups)
                                {
                                    if (candidateFoup.IsInitWait)
                                    { _availableTrackInDateTime = SimEngine.Instance.SimDateTime; }

                                    // WAIT-TRACK-IN 상태로 변경

                                    // Foup State 변경되기 전 현재 State 저장
                                    prevResourceId = candidateFoup.CurrentNode.Name;
                                    prevState = candidateFoup.CurrentState.ToString();
                                    // Foup State 변경
                                    candidateFoup.CurrentState = FOUP_STATE.WAIT_TRACK_IN;
                                    // Foup State 변경된 후 현재 State 저장
                                    nextResourceId = candidateFoup.CurrentNode.Name;
                                    nextState = candidateFoup.CurrentState.ToString();
                                    // Foup State 변경 기록
                                    Scheduler.Instance.RecordFoupHistory(candidateFoup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                                    // TRACK-IN 이벤트 등록
                                    port = new SimPort(INT_PORT.TRACK_IN, this, candidateFoup);
                                    // EqpHistory를 Port에 삽입
                                    eqpHistory = _remainHistorys.FindFirstHistory(candidateFoup.Name);
                                    eqpHistory.IsUsed = true;
                                    port.EqpHistory = eqpHistory;
                                    // FoupHistory를 Port에 삽입
                                    foupHistory = candidateFoup.Historys.FindNextHistory();
                                    foupHistory.IsUsed = true;
                                    foupHistory.SimStartTime = _availableTrackInDateTime;
                                    port.FoupHistory = foupHistory;
                                    //INSPECT
                                    if (eqpHistory.StartTime != foupHistory.StartTime)
                                        ;
                                    // TRACK-IN 
                                    Time availabieTrackInTime = new Time((_availableTrackInDateTime - SimEngine.Instance.StartDateTime).TotalSeconds);
                                    EvtCalendar.AddEvent(availabieTrackInTime, this, port);
                                }

                                // 다음 가능한 Track-In 시각은 최근 Track-In 시각으로부터 투입 간격이 더해진 시각이다.
                                _availableTrackInDateTime = _availableTrackInDateTime.AddSeconds(_processingInterval);
                            }
                            else
                                break;
                        }
                    }
                    #endregion

                    // 다음 스케줄이 현재 장비인 Foup 데려오기
                    notArriveFoups = NotArriveEntities();

                    while (true)
                    {
                        if (notArriveFoups.Any())
                        {
                            Foup notArriveFoup = notArriveFoups.First();

                            // 이미 요청한 Foup일 수도 있으므로
                            if(notArriveFoup.CurrentState != FOUP_STATE.BUFFER)
                            {
                                notArriveFoups.Remove(notArriveFoup);

                                continue;
                            }

                            ProcessPortNode procPort = GetEmptyProcessPort();

                            if (procPort != null)
                            {
                                if (procPort.CanReserve())
                                {
                                    // ProcessPort에 NotArriveFoup 예약
                                    port = new SimPort(EXT_PORT.RESERVE, this, notArriveFoup);
                                    procPort.ExternalFunction(simTime, simLogs, port);

                                    // NotArriveFoup 이동 명령 하달..
                                    RailPortNode stb = notArriveFoup.CurrentNode as RailPortNode;
                                    Scheduler.Instance.SendFoupToEqp(notArriveFoup, stb, procPort);

                                    // RequestEntities에 등록
                                    _requestEntities.Add(notArriveFoup);

                                    notArriveFoups.Remove(notArriveFoup);
                                }
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }

                    break;
                default:
                    break;
            }
        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            Foup foup;
            EqpHistory eqpHistory;
            FoupHistory foupHistory;
            FoupHistory nextFoupHistory;
            List<Foup> nextFoups;

            string prevResourceId;
            string nextResourceId;
            string prevState;
            string nextState;

            switch ((EXT_PORT)port.PortType)
            {
                // Foup이 InternalEntities에 등록. 
                case EXT_PORT.EQP_IN:

                    foup = port.Entity as Foup;

                    foup.CurrentNode = this;

                    // InternalEntities에 넣기
                    if (_internalEntities.Contains(foup) is false)
                        _internalEntities.Add(foup);
                    // ReserveList에서 제거
                    _reserveList.Remove(foup);
                    // RequestList에서 제거
                    _requestEntities.Remove(foup);

                    // ***CHECK-AVAILABLE-TRACK-IN-FOUP
                    // 현재 Foup이 Eqp에 들어오고 나서 Track-In 가능한 Foup 선별 후 Track-In 이벤트 등록

                    #region LOTBATCH
                    // Eqp의 Type이 LotBatch이고 현재 공정 수행 중이면 Track-In 불가
                    if (ProcessType == PROCESS_TYPE.LOTBATCH && IsProcessing)
                        break;

                    // LOTBATCH는 다음 처리할 Foup이 모두 InternalEntities에 있어야 하며 설비가 IDLE한 상황이어야 한다.
                    if (ProcessType == PROCESS_TYPE.LOTBATCH && IsProcessing is false)
                    {
                        // LOTBATCH 타입 설비에 IDLE한 상태인 경우

                        nextFoups = GetNextFoups();

                        bool isAllIn = true;

                        foreach (Foup nextFoup in nextFoups)
                        {
                            if (_internalEntities.Contains(nextFoup) is false || nextFoup.CurrentState != FOUP_STATE.WAIT)
                            {
                                isAllIn = false;
                                break;
                            }
                        }

                        if (isAllIn)
                        {
                            foreach (Foup nextFoup in nextFoups)
                            {
                                // WAIT-TRACK-IN 상태로 변경

                                // Foup State 변경되기 전 현재 State 저장
                                prevResourceId = nextFoup.CurrentNode.Name;
                                prevState = nextFoup.CurrentState.ToString();
                                // Foup State 변경
                                nextFoup.CurrentState = FOUP_STATE.WAIT_TRACK_IN;
                                // Foup State 변경된 후 현재 State 저장
                                nextResourceId = nextFoup.CurrentNode.Name;
                                nextState = nextFoup.CurrentState.ToString();
                                // Foup State 변경 기록
                                Scheduler.Instance.RecordFoupHistory(nextFoup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                                _availableTrackInDateTime = SimEngine.Instance.SimDateTime;

                                // TRACK-IN 이벤트 등록
                                port = new SimPort(INT_PORT.TRACK_IN, this, nextFoup);
                                // EqpHistory를 Port에 삽입
                                eqpHistory = _remainHistorys.FindFirstHistory(nextFoup.Name);
                                eqpHistory.IsUsed = true;
                                port.EqpHistory = eqpHistory;
                                // FoupHistory를 Port에 삽입
                                foupHistory = nextFoup.Historys.FindNextHistory();
                                foupHistory.IsUsed = true;
                                foupHistory.SimStartTime = SimEngine.Instance.SimDateTime;
                                port.FoupHistory = foupHistory;
                                //INSPECT
                                if (eqpHistory.StartTime != foupHistory.StartTime)
                                    ;
                                // TRACK-IN 
                                EvtCalendar.AddEvent(simTime, this, port);
                            }
                        }
                    }
                    #endregion

                    #region INLINE & BATCHINLINE
                    // INLINE & BATCHINLINE.. 투입 간격 고려
                    // Eqp의 남은 Sequence 리스트
                    if(ProcessType == PROCESS_TYPE.INLINE || ProcessType == PROCESS_TYPE.BATCHINLINE)
                    {
                        List<int> sequenceList = _remainHistorys.Historys.Select(history => history.Sequence).Distinct().ToList();

                        // Track-In 가능 시각
                        if (_availableTrackInDateTime <= SimEngine.Instance.SimDateTime)
                        { _availableTrackInDateTime = SimEngine.Instance.SimDateTime; }

                        foreach (int sequence in sequenceList)
                        {
                            List<string> candidateFoupNames = _remainHistorys.Historys.Where(history => history.Sequence == sequence).Select(history => history.FoupID).ToList();

                            List<Foup> candidateFoups = new List<Foup>();

                            foreach (string candidateFoupName in candidateFoupNames)
                            {
                                Foup candidateFoup = ModelManager.Instance.Foups[candidateFoupName];

                                candidateFoups.Add(candidateFoup);
                            }

                            bool isReadyToTrackIn = true;
                            bool isWaitToTrackIn = false;

                            foreach (Foup candidateFoup in candidateFoups)
                            {
                                if (_internalEntities.Contains(candidateFoup))
                                {
                                    if (candidateFoup.CurrentState == FOUP_STATE.WAIT)
                                        continue;
                                    else if (candidateFoup.CurrentState == FOUP_STATE.WAIT_TRACK_IN)
                                    {
                                        isWaitToTrackIn = true;
                                        break;
                                    }
                                    else if (candidateFoup.CurrentState == FOUP_STATE.PROCESSING)
                                    {
                                        isReadyToTrackIn = false;
                                        break;
                                    }
                                    else
                                        ;
                                }
                                else
                                {
                                    isReadyToTrackIn = false;
                                    break;
                                }
                            }

                            if (isWaitToTrackIn)
                                continue;

                            // Eqp의 InternalEntities에 다음 Foup들이 모두 있다면
                            if (isReadyToTrackIn)
                            {
                                foreach (Foup candidateFoup in candidateFoups)
                                {
                                    if (candidateFoup.IsInitWait)
                                    { _availableTrackInDateTime = SimEngine.Instance.SimDateTime; }

                                    // WAIT-TRACK-IN 상태로 변경

                                    // Foup State 변경되기 전 현재 State 저장
                                    prevResourceId = candidateFoup.CurrentNode.Name;
                                    prevState = candidateFoup.CurrentState.ToString();
                                    // Foup State 변경
                                    candidateFoup.CurrentState = FOUP_STATE.WAIT_TRACK_IN;
                                    // Foup State 변경된 후 현재 State 저장
                                    nextResourceId = candidateFoup.CurrentNode.Name;
                                    nextState = candidateFoup.CurrentState.ToString();
                                    // Foup State 변경 기록
                                    Scheduler.Instance.RecordFoupHistory(candidateFoup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                                    // TRACK-IN 이벤트 등록
                                    port = new SimPort(INT_PORT.TRACK_IN, this, candidateFoup);
                                    // EqpHistory를 Port에 삽입
                                    eqpHistory = _remainHistorys.FindFirstHistory(candidateFoup.Name);
                                    eqpHistory.IsUsed = true;
                                    port.EqpHistory = eqpHistory;
                                    // FoupHistory를 Port에 삽입
                                    foupHistory = candidateFoup.Historys.FindNextHistory();
                                    foupHistory.IsUsed = true;
                                    foupHistory.SimStartTime = _availableTrackInDateTime;
                                    port.FoupHistory = foupHistory;
                                    //INSPECT
                                    if (eqpHistory.StartTime != foupHistory.StartTime)
                                        ;
                                    // TRACK-IN 
                                    Time availabieTrackInTime = new Time((_availableTrackInDateTime - SimEngine.Instance.StartDateTime).TotalSeconds);
                                    EvtCalendar.AddEvent(availabieTrackInTime, this, port);
                                }

                                // 다음 가능한 Track-In 시각은 최근 Track-In 시각으로부터 투입 간격이 더해진 시각이다.
                                _availableTrackInDateTime = _availableTrackInDateTime.AddSeconds(_processingInterval);
                            }
                            else
                                break;
                        }
                    }
                    #endregion

                    break;
                default:
                    break;
            }
        }
    }
}