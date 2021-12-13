using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Engine;
using Simulation.Geometry;
using CSScriptLibrary;
using System.Diagnostics;
using System.Threading;

namespace Simulation.Model.Abstract
{
    /// <summary>
    /// Command를 생성하고, Dispatcher가 결정한 OHT에 할당하는 스케줄러 클래스
    /// 어떤 OHT가 수행할지 결정은 Dispatcher(MCS), 그 OHT가 어떤 경로로 오고 갈지 결정은 PathFinder(OCS)
    /// </summary>
    public class Scheduler
    {
        #region Variables
        static Scheduler _instance;
        Stopwatch sw;

        private List<Command> _totalCommandList;
        private List<Command> _inactiveCommandList;
        private List<Command> _queuedCommandList;
        private List<Command> _reservatedCommandList;
        private List<Command> _waitingCommandList;
        private List<Command> _transferringCommandList;
        private int _completedCommandCount;
        private int _completedFabACommandCount;
        private int _completedFabBCommandCount;
        private List<OHTNode> _rerouteOht;

        private Dictionary<uint, Dictionary<uint, List<double>>> _dicLength;
        private Dictionary<uint, Dictionary<uint, List<Vector3>>> _dicDirection;

        public List<Command> InactiveCommandList
        {
            get { return _inactiveCommandList; }
        }

        public List<Command> QueuedCommandList
        {
            get { return _queuedCommandList; }
        }

        public List<Command> QueuedCommandListFabA
        {
            get { return _queuedCommandList.FindAll(x=>x.Fab.Name == "M14A"); }
        }

        public List<Command> QueuedCommandListFabB
        {
            get { return _queuedCommandList.FindAll(x => x.Fab.Name == "M14B"); }
        }

        public List<Command> ReservatedCommandList
        {
            get { return _reservatedCommandList; }
        }

        public List<Command> ReservatedCommandListFabA
        {
            get { return _reservatedCommandList.FindAll(x => x.Fab.Name == "M14A"); }
        }

        public List<Command> ReservatedCommandListFabB
        {
            get { return _reservatedCommandList.FindAll(x => x.Fab.Name == "M14B"); }
        }

        public List<Command> WaitingCommandList
        {
            get { return _waitingCommandList; }
        }

        public List<Command> WaitingCommandListFabA
        {
            get { return _waitingCommandList.FindAll(x=>x.Fab.Name == "M14A"); }
        }


        public List<Command> WaitingCommandListFabB
        {
            get { return _waitingCommandList.FindAll(x => x.Fab.Name == "M14B"); }
        }
        public List<Command> TransferringCommandList
        {
            get { return _transferringCommandList; }
        }

        public List<Command> TransferringCommandListFabA
        {
            get { return _transferringCommandList.FindAll(x => x.Fab.Name == "M14A"); }
        }

        public List<Command> TransferringCommandListFabB
        {
            get { return _transferringCommandList.FindAll(x => x.Fab.Name == "M14B"); }
        }

        public int CompletedCommandCount
        {
            get { return _completedCommandCount; }
        }

        public int TotalCommandListCount
        {
            get { return _queuedCommandList.Count + _waitingCommandList.Count + _transferringCommandList.Count; }
        }

        public int TotalCommandListCountFabA
        {
            get { return QueuedCommandListFabA.Count + WaitingCommandListFabA.Count + TransferringCommandListFabA.Count; }
        }

        public int TotalCommandListCountFabB
        {
            get { return QueuedCommandListFabB.Count + WaitingCommandListFabB.Count + TransferringCommandListFabB.Count; }
        }

        public Dictionary<uint, Dictionary<uint, List<double>>> DicLength
        {
            get { return _dicLength; }
        }
        public Dictionary<uint, Dictionary<uint, List<Vector3>>> DicDirection
        {
            get { return _dicDirection; }
        }

        public int CompletrdCommamdCount
        {
            get
            {
                return _completedCommandCount;
            }
        }

        public int CompletedFabACommandCount
        {
            get { return _completedFabACommandCount; }
        }

        public int CompletedFabBCommandCount
        {
            get { return _completedFabBCommandCount; }
        }
        public static Scheduler Instance
        {
            get { return _instance; }
        }
        public Stopwatch StopWatch
        {
            get { return sw; }
        }

        public List<OHTNode> RerouteOHT
        {
            get { return _rerouteOht; }
        }
        #endregion
        
        public Scheduler()
        {
            _instance = this;
            _dicLength = new Dictionary<uint, Dictionary<uint, List<double>>>();
            _dicDirection = new Dictionary<uint, Dictionary<uint, List<Vector3>>>();

            _totalCommandList = new List<Command>();
            _inactiveCommandList = new List<Command>();
            _queuedCommandList = new List<Command>();
            _reservatedCommandList = new List<Command>();
            _waitingCommandList = new List<Command>();
            _transferringCommandList = new List<Command>();
            _rerouteOht = new List<OHTNode>();
            _completedCommandCount = 0;
            _completedFabACommandCount = 0;
            _completedFabBCommandCount = 0;
            sw = new Stopwatch();
        }

        public void ReportBufferImport(Time simTime, SimHistory simLogs, Command port)
        {
            _queuedCommandList.Add(port);
        }

        public void ReportProcessCompletion(Time simTime, SimHistory simLogs, Command port)
        {
            _queuedCommandList.Add(port);
        }
        public void ReportCommitEntityGeneration(CommitInterval commitNode, Command port)
        {
            _queuedCommandList.Add(port);
        }

        #region Command

        public void AddCommand(string name, string fabName, DateTime time, string fromPortName, string toPortName, string foupID, int priority, DateTime realTime, bool reticle)
        {
            Fab fab = ModelManager.Instance.Fabs[fabName];
            Foup foup;
            if (ModelManager.Instance.SimType == SIMULATION_TYPE.PRODUCTION && foupID != "")
                foup = ModelManager.Instance.Foups[foupID];
            else
                foup = null;

            RailPortNode fromPort = ModelManager.Instance.DicRailPort[fromPortName];
            RailPortNode toPort = ModelManager.Instance.DicRailPort[toPortName];
            Command command = new Command(name, fromPort, toPort, time, foup, fab, priority, reticle, realTime);

            _inactiveCommandList.Add(command);
        }

        public void AddCommand(string name, string fabName, double timeDouble, string fromPortName, string toPortName, string foupID, int priority)
        {
            Fab fab = ModelManager.Instance.Fabs[fabName];
            Time time = timeDouble;
            Foup foup;
            if (foupID != "")
                foup = ModelManager.Instance.Foups[foupID];
            else
                foup = null;

            RailPortNode fromPort = ModelManager.Instance.DicRailPort[fromPortName];
            RailPortNode toPort = ModelManager.Instance.DicRailPort[toPortName];

            Command command = null;

            // 1. ProcessPort -> ProcessPort
            if ((fromPort is ProcessPortNode) && (toPort is ProcessPortNode))
            {
                command = new Command(name, (ProcessPortNode)fromPort, (ProcessPortNode)toPort, time, foup, fab, priority);
            }
            // 2. STB -> ProcessPort
            else if ((fromPort is ProcessPortNode) is false && (toPort is ProcessPortNode))
            {
                command = new Command(name, fromPort, (ProcessPortNode)toPort, time, foup, fab, priority);
            }
            // 3. ProcessPort -> STB
            else if ((fromPort is ProcessPortNode) && (toPort is ProcessPortNode) is false)
            {
                command = new Command(name, (ProcessPortNode)fromPort, toPort, time, foup, fab, priority);
            }

            _queuedCommandList.Add(command);
        }
        public Command AddCommandForResult(string name, string fabName, DateTime time, string fromPortName, string toPortName, string foupID, int priority, DateTime realCompltedTime, bool reticle)
        {
            Fab fab = ModelManager.Instance.Fabs[fabName];
            Foup foup;
            if (foupID != "")
                foup = ModelManager.Instance.Foups[foupID];
            else
                foup = null;

            RailPortNode fromPort = ModelManager.Instance.DicRailPort[fromPortName];
            RailPortNode toPort = ModelManager.Instance.DicRailPort[toPortName];
            Command command = new Command(name, fromPort, toPort, time, foup, fab, priority, reticle, realCompltedTime);

            return command;
        }
        #endregion

        #region Production Simulation

        /// <summary>
        /// OHT가 Foup을 Load한 뒤 Unload할 곳을 찾는 함수, 내려놓은 Port가 있는지 확인 필요 
        /// </summary>
        /// <param name="oht"></param>

        /// <summary>
        /// Commit이 OHT에게 Foup을 가져가라고 할당하는 함수
        /// </summary>
        /// <param name="simNode"></param>
        /// <param name="roadNode"></param>
        /// <param name="simTime"></param>
        /// <param name="simLogs"></param>
        /// <param name="entity"></param>
        public void commit(SimNode commitNode, Time simTime, SimHistory simLogs, SimEntity entity)
        {
            //oht scheduling
            RailPortNode portNode = (RailPortNode)commitNode.EVTOutLink.EndNode;
            OHTNode oht = Dispatcher.Instance.SelectOHT(simTime, commitNode);

            if (oht == null) //Idle인 OHT가 없으면 Queue에 추가
            {
                //                _queueReserve.Add(new KeyValuePair<SimEntity, SimNode>(entity, commitNode));
                return;
            }

            oht.NodeState = OHT_STATE.MOVE_TO_LOAD;
            ModelManager.Instance.ReadyOHTs.Remove(oht);
            //            SimPort newPort = new SimPort(EXT_PORT.RAILLINE_OHT_START, oht);
            //            oht.LstRailLine[0].ExternalFunction(simTime, simLogs, newPort);
        }

        public void ReserveProcess(SimEntity entity, SimNode node)
        {
            //순서를 어떻게?
            //            _queueReserve.Add(new KeyValuePair<SimEntity, SimNode>(entity, node));
        }
        #endregion

        #region Foup 이동 명령 하달
        public void SendFoupToNext(Foup foup, ProcessPortNode loadingPort)
        {
            // 목적지 설정
            RailPortNode unloadingPort = null; // OHT가 Foup을 Unload해 갈 RailPort 혹은 ProcessPort. 목적지 (아직 미정)

            // 1. 다음에 가야 할 장비가 있는지부터 확인.
            FoupHistory nextHistory = foup.Historys.FindNextHistory();

            // Foup의 다음 스케줄이 있다면
            if (nextHistory != null)
            {
                string nextEqpID = nextHistory.EqpID;
                ProcessEqpNode nextEqp = ModelManager.Instance.DicProcessEqpNode[nextEqpID];

                // ProcessPort 조회

                List<ProcessPortNode> candidatePorts = new List<ProcessPortNode>();

                candidatePorts = nextEqp.ProcessPorts;

                // 다음 가야 할 장비의 ProcessPort가 비어 있는지 체크, 비어 있으면 바로 예약
                foreach (ProcessPortNode candidatePort in candidatePorts)
                {
                    // ProcessPort가 예약 가능하면
                    if (candidatePort.CanReserve())
                    {
                        // 다음 가야 할 장비의 ProcessPort에 예약
                        SimPort port = new SimPort(EXT_PORT.RESERVE, null, foup);
                        candidatePort.ExternalFunction(SimEngine.Instance.TimeNow, SimEngine.Instance.SimHistory, port);                        

                        // 목적지 설정
                        unloadingPort = candidatePort;

                        break;
                    }
                }

                //  다음 가야 할 장비의 ProcessPort에 예약 못함 => 다음 가야 할 장비의 가장 가까운 STB 예약
                if (unloadingPort == null)
                {
                    Vector3 eqpPos = nextEqp.PosVec3;

                    foreach (RailPortNode candidatePort in WipHelper.Instance.GetAvailableSTBs(eqpPos))
                    {
                        if (candidatePort.CanReserve())
                        {
                            // STB에 예약
                            SimPort port = new SimPort(EXT_PORT.RESERVE, null, foup);
                            candidatePort.ExternalFunction(SimEngine.Instance.TimeNow, SimEngine.Instance.SimHistory, port);

                            // 목적지 설정
                            unloadingPort = candidatePort;

                            break;
                        }
                    }
                }
            }
            // Foup의 다음 스케줄이 없음..
            else
            {
                // Foup의 다음 스케줄이 없을 때 => 아무 STB 설정
                foreach (RailPortNode candidatePort in WipHelper.Instance.GetAvailableSTBs())
                {
                    if (candidatePort.CanReserve())
                    {
                        // STB에 예약
                        SimPort port = new SimPort(EXT_PORT.RESERVE, null, foup);
                        candidatePort.ExternalFunction(SimEngine.Instance.TimeNow, SimEngine.Instance.SimHistory, port);

                        // 목적지 설정
                        unloadingPort = candidatePort;

                        break;
                    }
                }
            }

            // UnloadingPort가 정해졌으니 이동 명령 하달
            string cmdName = $"{foup.Fab.Name}_COMMAND_FROM_{loadingPort.Name}_TO_{unloadingPort.Name}_MOVE";
            double timeDouble = SimEngine.Instance.TimeNow.TotalSeconds;
            string fromPortName = loadingPort.Name;
            string toPortName = unloadingPort.Name;
            string foupID = foup.Name;

            // Foup State 변경되기 전 현재 State 저장
            string prevResourceId = foup.CurrentNode.Name;
            string prevState = foup.CurrentState.ToString();
            // Foup State 변경
            foup.CurrentState = FOUP_STATE.WAIT_OHT; // OHT를 기다리는 상태로 변경
            string nextResourceId = foup.CurrentNode.Name;
            string nextState = foup.CurrentState.ToString();
            // Foup State 변경 기록
            RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

            AddCommand(cmdName, foup.Fab.Name, timeDouble, fromPortName, toPortName, foupID, 1);
        }

        public void SendFoupToEqp(Foup foup, RailPortNode stb, ProcessPortNode unloadingPort)
        {
            RailPortNode loadingPort = stb;

            // UnloadingPort가 정해졌으니 이동 명령 하달
            string cmdName = $"{foup.Fab.Name}_COMMAND_FROM_{loadingPort.Name}_TO_{unloadingPort.Name}_MOVE";
            double timeDouble = SimEngine.Instance.TimeNow.TotalSeconds;
            string fromPortName = loadingPort.Name;
            string toPortName = unloadingPort.Name;
            string foupID = foup.Name;

            // Foup State 변경되기 전 현재 State 저장
            string prevResourceId = foup.CurrentNode.Name;
            string prevState = foup.CurrentState.ToString();
            // Foup Node/State 변경
            foup.CurrentState = FOUP_STATE.WAIT_OHT;
            // Foup State 변경된 후 현재 State 저장
            string nextResourceId = foup.CurrentNode.Name;
            string nextState = foup.CurrentState.ToString();
            // Foup State 변경 기록
            RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

            AddCommand(cmdName, foup.Fab.Name, timeDouble, fromPortName, toPortName, foupID, 1);
        }
        #endregion

        #region Material Handling Simulation

        /// <summary>
        /// Command 추가할 때마다 그리고 OHT Idle해질 때마다 Command 실행해주는 함수
        /// </summary>
        /// <param name="simTime"></param>
        /// <param name="simLogs"></param>
        public void AssignCommands(Time simTime, SimHistory simLogs)
        {
            OHTNode oht = null;

            _queuedCommandList.Sort((x, y) =>
            {
                int compare = y.Priority.CompareTo(x.Priority);
                if (compare != 0)
                    return compare;

                return x.ActivatedTime.CompareTo(y.ActivatedTime);
            });

            foreach (Command command in _queuedCommandList.ToList())
            {
                if (ModelManager.Instance.ReadyOHTs.Count == 0)
                    break;

                bool isAssigned = Dispatcher.Instance.CheckIdleOHTforCommand(ref oht, command, simTime);

                if (isAssigned)
                {
                    RailPortNode commandFromPort = command.FromNode as RailPortNode;
                    // oht가 BumpingOHT였는지 체크
                    // 만약 BumpingOHT였으면... 
                    // 1. BumpingBay에서 해당 oht 제거 
                    // 2. oht의 Bumping 관련 변수 null 혹은 false로 설정
                    if (oht.IsBumping)
                    {
                        oht.IsBumping = false;
                        Bay bumpingBay = oht.BumpingBay;
                        if (bumpingBay != null)
                            bumpingBay.BumpingOHTs.Remove(oht);

                        oht.BumpingBay = null;
                    }

                    if (oht.CurBay != null && oht.CurBay.DepositOHTs.Contains(oht))
                        oht.CurBay.DepositOHTs.Remove(oht);

                    // AssignedTime 설정
                    command.OHTNode = oht;
                    _queuedCommandList.Remove(command);

                    if (oht.NodeState is OHT_STATE.IDLE)
                    {
                        if(!_waitingCommandList.Contains(command))
                        {
                            command.CommandState = COMMAND_STATE.WAITING;
                            _waitingCommandList.Add(command);
                        }

                        oht.SetLstRailLine(oht.CandidateRoute);

                        command.AssignedTime = SimEngine.Instance.StartDateTime.AddSeconds(simTime.TotalSeconds);
                        ModelManager.Instance.ReadyOHTs.Remove(oht);
                        GoToAcquiringPort(simTime, simLogs, oht, command);
                    }
                    else
                    {
                        if(!_reservatedCommandList.Contains(command))
                        {
                            command.CommandState = COMMAND_STATE.RESERVATED;
                            _reservatedCommandList.Add(command);
                        }
                        ModelManager.Instance.ReadyOHTs.Remove(oht);
                        oht.DispatchedCommand = command;
                        oht.SetLstDispatchedRailLine(oht.CandidateRoute);
                        if (oht.DestinationPort.Line.Name != oht.LstDispatchedLine[0].Name)
                            ;
                    }
                }
            }
        }

        public void GoToAcquiringPort(Time simTime, SimHistory simLogs, OHTNode oht, Command command)
        {
            oht.NodeState = OHT_STATE.MOVE_TO_LOAD;
            oht.Command = command;
            oht.Destination = oht.Command.FromNode;
            oht.DestinationPort = oht.Command.FromNode as RailPortNode;
            oht.CurDistance = oht.CurRailLine.GetDistanceAtTime(oht, simTime);
            oht.DestinationPort.IsReservation = true;

            if (ModelManager.Instance.SimType == SIMULATION_TYPE.PRODUCTION)
                ((Foup)command.Entity).ReservationOHT = oht;

            if(oht.CurZcu != null)
            {
                string oldResetPointName = oht.ZcuResetPointName;

                if(oht.ZcuResetPointName == string.Empty)
                {
                    oht.CurZcu = null;
                }
                else
                {
                    RailPointNode oldResetPoint = ModelManager.Instance.DicRailPoint[oht.ZcuResetPointName];
                    oht.CurZcu.ChangeReservationResetPoint(oht);

                    RailPointNode newResetPoint = ModelManager.Instance.DicRailPoint[oht.ZcuResetPointName];
                    oht.CurZcu.ProcessReservation(simTime, simLogs, oldResetPoint);
                    oht.CurZcu.ProcessReservation(simTime, simLogs, newResetPoint);
                }
            }

            SimPort port = new SimPort(EXT_PORT.REVISE, oht.CurRailLine, oht);
            oht.CurRailLine.ExternalFunction(simTime, simLogs, port);
        }

        public void ActivateCommand(Time simTime)
        {
            foreach (Command command in _inactiveCommandList.ToList())
            {
                if (command.Fab.RailCuts.Contains(((RailPortNode)command.FromNode).Line) || command.Fab.RailCuts.Contains(((RailPortNode)command.ToNode).Line))
                    _inactiveCommandList.Remove(command);
                else if (command.Time <= simTime)
                {
                    command.CommandState = COMMAND_STATE.QUEUED;
                    _queuedCommandList.Add(command);
                    _inactiveCommandList.Remove(command);
                }
            }
        }

        public void ExecuteCommand(Command command, Time time)
        {
            DateTime loadedTime = SimEngine.Instance.StartDateTime.AddSeconds(time.TotalSeconds);
            command.LoadedTime = loadedTime;
            command.CommandState = COMMAND_STATE.TRANSFERRING;
            _transferringCommandList.Add(command);
            _waitingCommandList.Remove(command);

            #region 예측 시점 코드 개발

            #endregion
        }

        public void FinishCommand(Command command, Time time)
        {
            DateTime completedTime = SimEngine.Instance.StartDateTime.AddSeconds(time.TotalSeconds);
            command.CompletedTime = completedTime;
            SimResultDBManager.Instance.UploadCompletedCommandTrendLog(command);

            command.CommandState = COMMAND_STATE.COMPLETED;
            _transferringCommandList.Remove(command);
            _completedCommandCount++;

            if (command.Fab.Name == "M14A")
                _completedFabACommandCount++;
            else if (command.Fab.Name == "M14B")
                _completedFabBCommandCount++;
        }

        public void FinishEqpHistory(EqpHistory eqpHistory)
        {
            SimResultDBManager.Instance.UploadEqpLog(eqpHistory);
        }

        public void RecordFoupHistory(string foupId, string prevResourceId, string nextResourceId, string prevState, string nextState, DateTime simDateTime)
        {
            SimResultDBManager.Instance.UploadFoupLog(foupId, prevResourceId, nextResourceId, prevState, nextState, simDateTime);
        }

        /// <summary>
        /// Idle한 OHT 특정 Bay의 Bay 유입점으로 보내는 로직
        /// 도착하면 로직을 다시 돌린다. 같은 Bay로 가라고 하면 Bay안에서 다시 유입점으로 최단거리 도나? 확인 필요
        /// 현재는 그냥 아무 Station이나 보내자.
        /// Idle한 OHT 특정 Bay로 가는 로직도 스크립트로 지원하자!!
        /// </summary>
        /// <param name="oht"></param>
        public void LetIdleOHTGoToBay(OHTNode oht, Time simTime, SimHistory simLogs)
        {
            // Command를 완료한 OHT를 Bumping 모드로 변환

            Bay bumpingBay = oht.CurBay; // oht의 BumpingBay는 현재 머무르고 있는 Bay

            if (bumpingBay == null || bumpingBay.NeighborBay.Count == 0 || bumpingBay.BumpingPorts.Count == 0)
                bumpingBay = ModelManager.Instance.GetNearestBay(oht.Fab.Name, oht.CurRailLine.ToNode.PosVec3);

            if(!bumpingBay.BumpingOHTs.Contains(oht))
                bumpingBay.BumpingOHTs.Add(oht); // BumpingBay의 BumpingOHT에 해당 oht 등록
            
            oht.BumpingBay = bumpingBay;
            oht.IsBumping = true;

            // BumpingBay 내 BumpingPort 선정..
            oht.CurDistance = oht.CurRailLine.GetDistanceAtTime(oht, simTime);

            RailPortNode railPort = ZoneHelper.Instance.GetBumpingPort(oht, bumpingBay);
            SimNode destNode = null;
            if (railPort != null)
                destNode = railPort.ConnectedEqp;

            //출발지 목적지 계산
            oht.Destination = destNode;
            oht.DestinationPort = railPort;
            oht.CurDistance = oht.CurRailLine.GetDistanceAtTime(oht, simTime);

            if (PathFinder.Instance.IsUsingCandidatePath && SimModelDBManager.Instance.IsCandidatePath())
                oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);
            else
                oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);

            if (oht.CurZcu != null)
            {
                string oldResetPointName = oht.ZcuResetPointName;

                if (oht.ZcuResetPointName == string.Empty)
                {
                    oht.CurZcu = null;

                }
                else
                {
                    RailPointNode oldResetPoint = ModelManager.Instance.DicRailPoint[oht.ZcuResetPointName];
                    oht.CurZcu.ChangeReservationResetPoint(oht);

                    RailPointNode newResetPoint = ModelManager.Instance.DicRailPoint[oht.ZcuResetPointName];
                    oht.CurZcu.ProcessReservation(simTime, simLogs, oldResetPoint);
                    oht.CurZcu.ProcessReservation(simTime, simLogs, newResetPoint);
                }
            }

            if(simTime != 0)
            {
                SimPort newPort = new SimPort(EXT_PORT.REVISE, oht);
                newPort.OHTNode = oht;
                oht.CurRailLine.ExternalFunction(simTime, simLogs, newPort);
            }

            // BumpingBay에서 돌고 있는 BumpingOHT 대수 검사
            // BumpingOHT 대수가 Max일 경우 BumpingBay 내 가장 오래된 OHT(첫번째 요소)를
            // 그 다음 Bay로 보낸다.
            if (bumpingBay.IsMaxBumping && ModelManager.Instance.Bays.Min(b=>b.Value.BumpingOHTs.Count) < ZoneHelper.Instance.IntraBayBumpingOHTMaxValue)
            {
                OHTNode oldestOHT = bumpingBay.BumpingOHTs.First();

                Bay nextBay = ZoneHelper.Instance.FindNextBumpingBay(bumpingBay, oldestOHT.Reticle);

                bumpingBay.BumpingOHTs.Remove(oldestOHT);
                List<Bay> visitedBay = new List<Bay>();
                LetOldestOHTGoToNextBay(oldestOHT, simTime, simLogs, nextBay, visitedBay);
            }
        }

        /// <summary>
        /// BumpingOHT를 다른 BumpingPort로 보내는 것
        /// </summary>
        /// <param name="oht"></param>
        /// <param name="simTime"></param>
        /// <param name="simLogs"></param>
        public void LetOHTContinueBumping(OHTNode oht, Time simTime, SimHistory simLogs)
        {
            // Idle해진 OHT의 Bumping 설정
            oht.IsBumping = true;

            Bay bumpingBay = oht.BumpingBay;

            RailPortNode railPort = ZoneHelper.Instance.GetBumpingPort(oht, bumpingBay);
            SimNode destNode = railPort.ConnectedEqp;

            //출발지 목적지 계산
            oht.Destination = destNode;
            oht.DestinationPort = railPort;
            oht.CurDistance = oht.CurRailLine.GetDistanceAtTime(oht, simTime);

            if (PathFinder.Instance.IsUsingCandidatePath && SimModelDBManager.Instance.IsCandidatePath())
                oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);
            else
                oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);

//            oht.NodeState = OHT_STATE.IDLE;
            SimPort newPort = new SimPort(EXT_PORT.REVISE, oht);
            newPort.OHTNode = oht;
            oht.CurRailLine.ExternalFunction(simTime, simLogs, newPort);
        }

        /// <summary>
        /// oht(oldestOHT)를 targetBay(그 다음 Bay)로 보내는 것
        /// </summary>
        /// <param name="oht"></param>
        /// <param name="simTime"></param>
        /// <param name="simLogs"></param>
        /// <param name="targetBay"></param>
        public void LetOldestOHTGoToNextBay(OHTNode oht, Time simTime, SimHistory simLogs, Bay targetBay, List<Bay> visitedBays)
        {
            // Idle해진 OHT의 Bumping 설정
            oht.IsBumping = true;

            visitedBays.Add(targetBay);
            if(!targetBay.BumpingOHTs.Contains(oht))
                targetBay.BumpingOHTs.Add(oht);

            oht.BumpingBay = targetBay;

            RailPortNode railPort = ZoneHelper.Instance.GetBumpingPort(oht, targetBay);
            SimNode destNode = railPort.ConnectedEqp;

            //출발지 목적지 계산
            oht.Destination = destNode;
            oht.DestinationPort = railPort;
            oht.CurDistance = oht.CurRailLine.GetDistanceAtTime(oht, simTime);

            if (PathFinder.Instance.IsUsingCandidatePath && SimModelDBManager.Instance.IsCandidatePath())
                oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);
            else 
                oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);

            if(simTime > 0)
            {
                //            oht.NodeState = OHT_STATE.IDLE;
                SimPort newPort = new SimPort(EXT_PORT.REVISE, oht);
                newPort.OHTNode = oht;
                oht.CurRailLine.ExternalFunction(simTime, simLogs, newPort);
            }

            // targetBay에서 돌고 있는 BumpingOHT 대수 검사
            // BumpingOHT 대수가 Max일 경우 targetBay 내 가장 오래된 OHT(첫번째 요소)를
            // 그 다음 Bay로 보낸다.            
            if (targetBay.IsMaxBumping )
            {
                OHTNode oldestOHT = targetBay.BumpingOHTs.First();

                Bay curBay = oldestOHT.BumpingBay;

                Bay nextBay = ZoneHelper.Instance.FindNextBumpingBay(curBay, oldestOHT.Reticle);

                if (nextBay == null)
                    ;

                if(!visitedBays.Contains(nextBay))
                {
                    targetBay.BumpingOHTs.Remove(oldestOHT);

                    LetOldestOHTGoToNextBay(oldestOHT, simTime, simLogs, nextBay, visitedBays);
                }

            }
        }
        #endregion
    }
}
