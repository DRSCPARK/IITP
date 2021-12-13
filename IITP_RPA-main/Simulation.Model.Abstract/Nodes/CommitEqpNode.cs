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
    public class CommitEqpNode : ProcessEqpNode
    {
        #region Variable

        #endregion

        public CommitEqpNode(uint id, Fab fab, string eqpId, string processGroup, string processType, string stepGroup, string bayName)
            : base(id, eqpId, fab, processGroup, processType, stepGroup, bayName)
        { }

        public override void InitializeNode(EventCalendar evtCal)
        {
            EvtCalendar = evtCal;
            Receivable = true;

            if (ModelManager.Instance.SimType == SIMULATION_TYPE.PRODUCTION)
            {
                SimPort port;
                double totalSeconds;
                Time eventTime;

                TotalHistoryCount = RemainHistorys.Historys.Count;

                foreach (EqpHistory eqpHistory in RemainHistorys.Historys)
                {
                    Foup foup = ModelManager.Instance.Foups[eqpHistory.FoupID];

                    FoupHistory foupHistory = foup.Historys.FindNextHistory();

                    // InternalEntities에 넣기
                    foup.CurrentNode = this;
                    foup.CurrentState = FOUP_STATE.READY_FOR_FAB_IN;
                    // InternalEntities에 넣기
                    if (InternalEntities.Contains(foup) is false)
                    { InternalEntities.Add(foup); }

                    // Track-In Event
                    port = new SimPort(INT_PORT.TRACK_IN, this, foup);

                    totalSeconds = (eqpHistory.StartTime - SimEngine.Instance.StartDateTime).TotalSeconds;

                    eventTime = new Time(totalSeconds);

                    eqpHistory.IsUsed = true;
                    foupHistory.IsUsed = true;

                    foupHistory.SimStartTime = SimEngine.Instance.SimDateTime;

                    port.EqpHistory = eqpHistory;
                    port.FoupHistory = foupHistory;

                    EvtCalendar.AddEvent(eventTime, this, port);
                }
            }
        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            SimLog log;

            INT_PORT inPort = (INT_PORT)port.PortType;

            Foup foup;
            EqpHistory eqpHistory;
            FoupHistory foupHistory;

            string prevResourceId;
            string nextResourceId;
            string prevState;
            string nextState;

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

                    // Track-Out Event
                    eqpHistory = port.EqpHistory as EqpHistory;
                    foupHistory = port.FoupHistory as FoupHistory;
                    // Eqp history 삭제
                    this.RemainHistorys.RemoveHistory(eqpHistory);
                    // Foup history 삭제
                    foup.Historys.RemoveHistory(foupHistory);

                    // 시뮬레이션 상에서의 Track_In 시각 저장 후 다시 port에 넣기. Track_Out 때 종료 시각 저장하기 위해
                    eqpHistory.SimStartTime = SimEngine.Instance.StartDateTime.AddSeconds(simTime.TotalSeconds);

                    // 프로세싱 타임 (단위 : 초)
                    double processingTimeSeconds = (eqpHistory.EndTime - eqpHistory.StartTime).TotalSeconds;

                    Time eventTime = new Time(processingTimeSeconds);

                    Console.WriteLine($"SimTime({simTime})에 {this.Name}에서 Entity({foup.Name}) TRACK-IN({ProcessType.ToString()}) " +
                        $"RealDT : {eqpHistory.StartTime.ToString()} SimDT : {SimEngine.Instance.StartDateTime.AddSeconds(simTime.TotalSeconds).ToString()}....");

                    if (SimEngine.Instance.IsAnimation)
                    {
                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.BUSY);
                        simLogs.AddLog(log);
                    }

                    port = new SimPort(INT_PORT.TRACK_OUT, this, foup);
                    port.EqpHistory = eqpHistory;
                    port.FoupHistory = foupHistory;
                    EvtCalendar.AddEvent(simTime + eventTime, this, port);

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

                    Console.WriteLine($"SimTime({simTime})에 Commit에서 Entity({foup.Name}) TRACK-OUT({ProcessType.ToString()})   Remain Foups : ({RemainHistorys.Historys.Count}/{TotalHistoryCount})  Finished : {SimEngine.Instance.FinishedEqpPlanCount}....");

                    // Track_In 때의 EqpHistory 다시 불러오기
                    eqpHistory = port.EqpHistory as EqpHistory;
                    // 시뮬레이션 상에서의 종료 시각 저장
                    eqpHistory.SimEndTime = SimEngine.Instance.StartDateTime.AddSeconds(simTime.TotalSeconds);
                    // EqpHistory 처리 완료
                    Scheduler.Instance.FinishEqpHistory(eqpHistory);

                    // ***NEXT_EQP_RESERVE
                    // 다음 스케줄이 있다면 다음 장비의 예약 리스트에 등록
                    FoupHistory nextFoupHistory = foup.Historys.FindNextHistory();

                    // 스케줄이 있을 경우
                    if (nextFoupHistory != null)
                    {
                        string nextEqpID = nextFoupHistory.EqpID;
                        ProcessEqpNode nextEqp = ModelManager.Instance.DicProcessEqpNode[nextEqpID];

                        nextEqp.Reserve(foup);
                    }

                    // 현재 Eqp에서 나가야 하므로 ProcessPort 예약 시도.
                    foreach (ProcessPortNode procPort in ProcessPorts)
                    {
                        // 예약 가능 여부 확인
                        if (procPort.CanReserve())
                        {
                            // Processport에 예약
                            port = new SimPort(EXT_PORT.RESERVE, this, foup);
                            procPort.ExternalFunction(simTime, simLogs, port);

                            // Processport 위에 foup 올려놓기
                            foup.CurrentNode = procPort;
                            procPort.LoadedEntity = foup;
                            procPort.NodeState = PROCESSPORT_STATE.FULL;

                            if(SimEngine.Instance.IsAnimation)
                            {
                                log = new SimLog(simTime + 1, simTime + 2, foup, procPort, ANIMATION.LOAD);
                                simLogs.AddLog(log);
                            }

                            Scheduler.Instance.SendFoupToNext(foup, procPort);

                            break;
                        }
                    }

                    break;
                default:
                    Console.WriteLine("internal Func. error in CommitInter class");
                    break;
            }
        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {

        }
    }
}
