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
    public class CompleteEqpNode : ProcessEqpNode
    {
        #region Variable
        private List<Foup> _completeEntities;

        public List<Foup> CompleteEntities
        {
            get { return _completeEntities; }
            set { _completeEntities = value; }
        }
        #endregion

        public CompleteEqpNode(uint id, Fab fab, string eqpId, string processGroup, string processType, string stepGroup, string bayName)
            : base(id, eqpId, fab, processGroup, processType, stepGroup, bayName)
        {
            _completeEntities = new List<Foup>();
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            EvtCalendar = evtCal;
            Receivable = true;

            ReserveList = new List<Foup>();
            InternalEntities = new List<Foup>();
            ProcessingEntities = new List<Foup>();

            foreach (ProcessPortNode processPort in ProcessPorts)
            {
                processPort.InitializeNode(evtCal);
            }

            if (ModelManager.Instance.SimType == SIMULATION_TYPE.PRODUCTION)
            {
                SimPort port;
                double totalSeconds;
                Time eventTime;

                TotalHistoryCount = RemainHistorys.Historys.Count;

                foreach (EqpHistory eqpHistory in RemainHistorys.Historys.Where(h => h.Sequence == 1).ToList())
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
                        if (InternalEntities.Contains(foup) is false)
                        { InternalEntities.Add(foup); }

                        // Track-In Event
                        port = new SimPort(INT_PORT.TRACK_IN, this, foup);
                        // EqpHistory를 Port에 삽입
                        eqpHistory.IsUsed = true;
                        port.EqpHistory = eqpHistory;
                        // FoupHistory를 Port에 삽입
                        foupHistory.IsUsed = true;
                        foupHistory.SimStartTime = SimEngine.Instance.SimDateTime;
                        port.FoupHistory = foupHistory;
                        // TRACK-IN
                        totalSeconds = (eqpHistory.StartTime - SimEngine.Instance.StartDateTime).TotalSeconds;
                        eventTime = new Time(totalSeconds);
                        EvtCalendar.AddEvent(eventTime, this, port);
                    }
                }
            }
        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            SimLog log;

            INT_PORT inPort = (INT_PORT)port.PortType;
            EqpHistory eqpHistory;
            FoupHistory foupHistory;
            List<Foup> notArriveFoups;

            string prevResourceId;
            string nextResourceId;
            string prevState;
            string nextState;

            Foup foup;

            switch (inPort)
            {
                case INT_PORT.TRACK_IN:

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
                    ProcessingEntities.Add(foup);

                    NodeState = PROCESSEQP_STATE.PROCESSING;

                    if (SimEngine.Instance.IsAnimation)
                    {
                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.BUSY);
                        simLogs.AddLog(log);
                    }

                    // Track-Out Event
                    eqpHistory = port.EqpHistory as EqpHistory;
                    foupHistory = port.FoupHistory as FoupHistory;
                    // Eqp history 삭제
                    this.RemainHistorys.RemoveHistory(eqpHistory);
                    // Foup history 삭제
                    foup.Historys.RemoveHistory(foupHistory);

                    // 시뮬레이션 상에서의 Track_In 시각 저장 후 다시 port에 넣기. Track_Out 때 종료 시각 저장하기 위해
                    eqpHistory.SimStartTime = SimEngine.Instance.SimDateTime;

                    // 프로세싱 타임 (단위 : 초)
                    double processingTimeSeconds = (eqpHistory.EndTime - eqpHistory.StartTime).TotalSeconds;

                    Time eventTime = new Time(processingTimeSeconds);

                    Console.WriteLine($"SimTime({simTime})에 {this.Name}에서 Entity({foup.Name}) TRACK-IN({ProcessType.ToString()}) " +
                        $"RealDT : {eqpHistory.StartTime.ToString()} SimDT : {SimEngine.Instance.StartDateTime.AddSeconds(simTime.TotalSeconds).ToString()}....");

                    port = new SimPort(INT_PORT.TRACK_OUT, this, foup);
                    port.EqpHistory = eqpHistory;
                    port.FoupHistory = foupHistory;
                    EvtCalendar.AddEvent(simTime + eventTime, this, port);

                    // 다음 스케줄이 현재 장비인 Foup 데려오기
                    notArriveFoups = NotArriveEntities();

                    while (true)
                    {
                        if (notArriveFoups.Any())
                        {
                            Foup notArriveFoup = notArriveFoups.First();

                            // 이미 요청한 Foup일 수도 있으므로
                            if (notArriveFoup.CurrentState != FOUP_STATE.BUFFER)
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
                                    RequestEntities.Add(notArriveFoup);

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
                case INT_PORT.TRACK_OUT:

                    SimEngine.Instance.FinishedEqpPlanCount += 1;

                    foup = port.Entity as Foup;

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
                    ProcessingEntities.Remove(foup);

                    // processingEntities에 Foup이 없으면 IDLE로 상태 변경
                    if (ProcessingEntities.Any() is false)
                    {
                        NodeState = PROCESSEQP_STATE.IDLE;

                        Console.WriteLine($"SimTime({simTime})에 Eqp : {this.Name} IDLE...");

                        if (SimEngine.Instance.IsAnimation)
                        {
                            log = new SimLog(simTime, simTime + 1, this, ANIMATION.IDLE);
                            simLogs.AddLog(log);
                        }
                    }

                    Console.WriteLine($"SimTime({simTime})에 {this.Name}에서 Entity({foup.Name}) TRACK-OUT({ProcessType.ToString()})   Remain Foups : ({RemainHistorys.Historys.Count}/{TotalHistoryCount})  Finished : {SimEngine.Instance.FinishedEqpPlanCount}....");

                    Console.WriteLine($"SimTime({simTime})에 {this.Name}에서 Entity({foup.Name}) FAB-OUT({ProcessType.ToString()})....");

                    // Track_In 때의 EqpHistory 다시 불러오기
                    eqpHistory = port.EqpHistory as EqpHistory;
                    // 시뮬레이션 상에서의 종료 시각 저장
                    eqpHistory.SimEndTime = SimEngine.Instance.SimDateTime;
                    // EqpHistory 처리 완료
                    Scheduler.Instance.FinishEqpHistory(eqpHistory);

                    // InternalEntities에서 제거
                    InternalEntities.Remove(foup);
                    // CompleteEntities 추가
                    _completeEntities.Add(foup);

                    // ***CHECK-AVAILABLE-TRACK-IN-FOUP
                    // 현재 Foup이 Eqp에 들어오고 나서 Track-In 가능한 Foup 선별 후 Track-In 이벤트 등록

                    // Eqp의 남은 Sequence 리스트
                    List<int> sequenceList = RemainHistorys.Historys.Select(history => history.Sequence).Distinct().ToList();

                    foreach (int sequence in sequenceList)
                    {
                        List<string> candidateFoupNames = RemainHistorys.Historys.Where(history => history.Sequence == sequence).Select(history => history.FoupID).ToList();

                        List<Foup> candidateFoups = new List<Foup>();

                        foreach (string candidateFoupName in candidateFoupNames)
                        {
                            Foup candidateFoup = ModelManager.Instance.Foups[candidateFoupName];

                            candidateFoups.Add(candidateFoup);
                        }

                        foreach (Foup candidateFoup in candidateFoups)
                        {
                            if (InternalEntities.Contains(candidateFoup) && candidateFoup.CurrentState == FOUP_STATE.WAIT)
                            {
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

                                port = new SimPort(INT_PORT.TRACK_IN, this, candidateFoup);
                                // EqpHistory를 Port에 삽입
                                eqpHistory = RemainHistorys.FindFirstHistory(candidateFoup.Name);
                                eqpHistory.IsUsed = true;
                                port.EqpHistory = eqpHistory;
                                // FoupHistory를 Port에 삽입
                                foupHistory = candidateFoup.Historys.FindNextHistory();
                                foupHistory.IsUsed = true;
                                foupHistory.SimStartTime = SimEngine.Instance.SimDateTime;
                                port.FoupHistory = foupHistory;
                                // TRACK-IN
                                EvtCalendar.AddEvent(simTime, this, port);
                            }
                        }
                    }

                    // 다음 스케줄이 현재 장비인 Foup 데려오기
                    notArriveFoups = NotArriveEntities();

                    while (true)
                    {
                        if (notArriveFoups.Any())
                        {
                            Foup notArriveFoup = notArriveFoups.First();

                            // 이미 요청한 Foup일 수도 있으므로
                            if (notArriveFoup.CurrentState != FOUP_STATE.BUFFER)
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
                                    RequestEntities.Add(notArriveFoup);

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
            List<Foup> notArriveFoups;

            string prevResourceId;
            string nextResourceId;
            string prevState;
            string nextState;

            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.EQP_IN:

                    foup = port.Entity as Foup;

                    foup.CurrentNode = this;

                    // InternalEntities에 넣기
                    if (InternalEntities.Contains(foup) is false)
                    { InternalEntities.Add(foup); }
                    // ReserveList에서 제거
                    ReserveList.Remove(foup);
                    // RequestList에서 제거
                    RequestEntities.Remove(foup);

                    // ***CHECK-AVAILABLE-TRACK-IN-FOUP
                    // 현재 Foup이 Eqp에 들어오고 나서 Track-In 가능한 Foup 선별 후 Track-In 이벤트 등록

                    // Eqp의 남은 Sequence 리스트
                    List<int> sequenceList = RemainHistorys.Historys.Select(history => history.Sequence).Distinct().ToList();

                    foreach (int sequence in sequenceList)
                    {
                        List<string> candidateFoupNames = RemainHistorys.Historys.Where(history => history.Sequence == sequence).Select(history => history.FoupID).ToList();

                        List<Foup> candidateFoups = new List<Foup>();

                        foreach (string candidateFoupName in candidateFoupNames)
                        {
                            Foup candidateFoup = ModelManager.Instance.Foups[candidateFoupName];

                            candidateFoups.Add(candidateFoup);
                        }

                        foreach(Foup candidateFoup in candidateFoups)
                        {
                            if(InternalEntities.Contains(candidateFoup) && candidateFoup.CurrentState == FOUP_STATE.WAIT)
                            {
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

                                port = new SimPort(INT_PORT.TRACK_IN, this, candidateFoup);
                                // EqpHistory를 Port에 삽입
                                eqpHistory = RemainHistorys.FindFirstHistory(candidateFoup.Name);
                                eqpHistory.IsUsed = true;
                                port.EqpHistory = eqpHistory;
                                // FoupHistory를 Port에 삽입
                                foupHistory = candidateFoup.Historys.FindNextHistory();
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

                    // 다음 스케줄이 현재 장비인 Foup 데려오기
                    notArriveFoups = NotArriveEntities();

                    while (true)
                    {
                        if (notArriveFoups.Any())
                        {
                            Foup notArriveFoup = notArriveFoups.First();

                            // 이미 요청한 Foup일 수도 있으므로
                            if (notArriveFoup.CurrentState != FOUP_STATE.BUFFER)
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
                                    RequestEntities.Add(notArriveFoup);

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

    }
}
