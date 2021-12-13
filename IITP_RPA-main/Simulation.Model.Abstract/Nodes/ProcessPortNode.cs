using Simulation.Engine;
using Simulation.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Simulation.Model.Abstract
{
    public class ProcessPortNode : RailPortNode
    {
        #region Member Variable

        public string ProcessPortName
        {
            get { return Name; }
        }

        #endregion

        public ProcessPortNode(uint ID, ProcessEqpNode connectProcess, Fab fab) : base(ID, "", fab, PORT_TYPE.BOTH)
        {
            ConnectedEqp = connectProcess;
            NodeState = PROCESSPORT_STATE.EMPTY;
        }

        public ProcessPortNode(uint ID, RailLineNode line, double distance, Fab fab) : base(ID, "", line, distance, fab, PORT_TYPE.BOTH)
        {
            NodeState = PROCESSPORT_STATE.EMPTY;
        }

        public ProcessPortNode(uint ID, RailLineNode line, double distance, ProcessEqpNode eqp, Fab fab) : base(ID, "", line, distance, fab, PORT_TYPE.BOTH)
        {
            NodeState = PROCESSPORT_STATE.EMPTY;

            eqp.DicRailPort.Add(this);
            eqp.ProcessPorts.Add(this);
            ConnectedEqp = eqp;
        }

        public ProcessPortNode(uint ID, string name, RailLineNode line, double distance, ProcessEqpNode eqp, Fab fab) : base(ID, name, line, distance, fab, PORT_TYPE.BOTH)
        {
            NodeState = PROCESSPORT_STATE.EMPTY;

            eqp.DicRailPort.Add(this);
            eqp.ProcessPorts.Add(this);
            ConnectedEqp = eqp;

            line.AddRailPort(this); // 이 코드를 추가함으로써 ProcessPortNode에도 Foup Loading&Unloading이 가능해졌습니다.

            WaitAllowed = true;
            BumpAllowed = true;
        }

        public ProcessPortNode(uint ID, string name, RailLineNode line, double distance, CommitEqpNode eqp, Fab fab) : base(ID, name, line, distance, fab, PORT_TYPE.BOTH)
        {
            NodeState = PROCESSPORT_STATE.EMPTY;

            eqp.DicRailPort.Add(this);
            eqp.ProcessPorts.Add(this);
            ConnectedEqp = eqp;

            line.AddRailPort(this); // 이 코드를 추가함으로써 ProcessPortNode에도 Foup Loading&Unloading이 가능해졌습니다.

            WaitAllowed = true;
            BumpAllowed = true;
        }

        public ProcessPortNode(uint ID, string name, RailLineNode line, double distance, CompleteEqpNode eqp, Fab fab) : base(ID, name, line, distance, fab, PORT_TYPE.BOTH)
        {
            NodeState = PROCESSPORT_STATE.EMPTY;

            eqp.DicRailPort.Add(this);
            eqp.ProcessPorts.Add(this);
            ConnectedEqp = eqp;

            line.AddRailPort(this); // 이 코드를 추가함으로써 ProcessPortNode에도 Foup Loading&Unloading이 가능해졌습니다.

            WaitAllowed = true;
            BumpAllowed = true;
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);
        }

        public override bool CanReserve()
        {
            bool canReserve = false;

            if((PROCESSPORT_STATE)NodeState == PROCESSPORT_STATE.EMPTY && ReserveEntity == null && IsReservation == false)
            {
                canReserve = true;
            }

            return canReserve;
        }

        public override void Reserve(Foup foup)
        {
            NodeState = PROCESSPORT_STATE.RESERVED;
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

            List<Foup> outWaitFoups;
            Foup outWaitFoup;
            Foup selectedFoup;
            List<Foup> notArriveFoups;

            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.RESERVE:

                    Reserve(foup);

                    if(SimEngine.Instance.IsAnimation)
                    {
                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.PORT_RESERVE);
                        simLogs.AddLog(log);
                    }

                    break;

                // 1. Port의 Foup이 OHT에 Loading됌.
                // => Port의 Foup 제거, Reserve 초기화
                case EXT_PORT.OHT_LOADING:

                    // Port에 Foup이 있어야 하므로 현재 Port의 상태는 Full
                    if ((PROCESSPORT_STATE)NodeState == PROCESSPORT_STATE.FULL)
                    {
                        LoadedEntity = null;
                        ReserveEntity = null;
                        IsReservation = false;
                        NodeState = PROCESSPORT_STATE.EMPTY;

                        OHTNode oht = port.OHTNode as OHTNode;

                        // Foup State 변경되기 전 현재 State 저장
                        prevResourceId = foup.CurrentNode.Name;
                        prevState = foup.CurrentState.ToString();
                        // Foup State 변경
                        foup.CurrentNode = oht;
                        foup.CurrentState = FOUP_STATE.OHT; 
                        // Foup State 변경된 후 현재 State 저장
                        nextResourceId = foup.CurrentNode.Name;
                        nextState = foup.CurrentState.ToString();
                        // Foup State 변경 기록
                        Scheduler.Instance.RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                        #region 다른 Foup을 부르는 코드 (CompleteEqp는 Loading할 일이 없으므로 케이스에서 제외)
                        // Foup을 Eqp로 보내고 
                        // 1. Eqp에 OutWait 상태의 Foup을 꺼낸다.
                        // 2. 1에서 Foup이 없을 시 Eqp에 예약된 Foup을 가져온다.
                        // 3. 2에서 Foup이 없을 시 Eqp에 도착하지 않은 Foup(다음 스케줄이 Eqp)을 가져온다.

                        switch (ConnectedEqp)
                        {
                            case CommitEqpNode commitEqp:

                                // *** 1. OutWaitFoups
                                outWaitFoups = commitEqp.GetOutWaitFoups();

                                // outWaitFoups의 첫번째 요소 반환. 없으면 null
                                outWaitFoup = outWaitFoups.Any() ? outWaitFoups.First() : null;

                                // 첫번째 요소 반환 성공 시 outWaitFoup 내보내기
                                if (outWaitFoup != null)
                                {
                                    // OutWaitFoup이 현재 포트에 예약
                                    Reserve(outWaitFoup);

                                    // OutWaitFoup이 현재 포트에 Load
                                    outWaitFoup.CurrentNode = this;
                                    LoadedEntity = outWaitFoup;
                                    NodeState = PROCESSPORT_STATE.FULL;

                                    if(SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, outWaitFoup, this, ANIMATION.LOAD);
                                        simLogs.AddLog(log);
                                    }

                                    // Eqp에서 OutWaitFoup 제거
                                    commitEqp.InternalEntities.Remove(outWaitFoup);

                                    // OutWaitFoup을 다음 목적지로 이동시키는 명령 하달
                                    Scheduler.Instance.SendFoupToNext(outWaitFoup, this);

                                    // switch문 종료
                                    break;
                                }

                                break;
                            case ProcessEqpNode procEqp:
                                // *** 1. OutWaitFoups
                                outWaitFoups = procEqp.GetOutWaitFoups();

                                // outWaitFoups의 첫번째 요소 반환. 없으면 null
                                outWaitFoup = outWaitFoups.Any() ? outWaitFoups.First() : null;

                                // 첫번째 요소 반환 성공 시 outWaitFoup 내보내기
                                if (outWaitFoup != null)
                                {
                                    // OutWaitFoup이 현재 포트에 예약
                                    Reserve(outWaitFoup);

                                    // OutWaitFoup이 현재 포트에 Load
                                    outWaitFoup.CurrentNode = this;
                                    LoadedEntity = outWaitFoup;
                                    NodeState = PROCESSPORT_STATE.FULL;

                                    if(SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, outWaitFoup, this, ANIMATION.LOAD);
                                        simLogs.AddLog(log);
                                    }

                                    // Eqp에서 OutWaitFoup 제거
                                    procEqp.InternalEntities.Remove(outWaitFoup);

                                    // OutWaitFoup을 다음 목적지로 이동시키는 명령 하달
                                    Scheduler.Instance.SendFoupToNext(outWaitFoup, this);

                                    // switch문 종료
                                    break;
                                }

                                // *** 2. Eqp의 ReserveList 조회 후 예약된 Foup 데려오기
                                // reserveList를 첫번째 요소부터 확인하면서 현재 State가 Buffer인 Foup 탐색
                                selectedFoup = null;

                                foreach (Foup reservedFoup in procEqp.ReserveList)
                                {
                                    if (reservedFoup.CurrentState == FOUP_STATE.BUFFER)
                                    {
                                        selectedFoup = reservedFoup;
                                        break; // 찾았으면 루프 종료
                                    }
                                }

                                if (selectedFoup != null)
                                {
                                    // selectedFoup을 현재 Eqp로 요청 (FromNode는 ProcessPort)

                                    // ProcessPort에 RequestFoup 예약
                                    Reserve(selectedFoup);

                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.PORT_RESERVE);
                                        simLogs.AddLog(log);
                                    }

                                    // RequestFoup 이동 명령 하달..
                                    RailPortNode stb = selectedFoup.CurrentNode as RailPortNode;
                                    Scheduler.Instance.SendFoupToEqp(selectedFoup, stb, this);

                                    // RequestEntities에 등록
                                    procEqp.RequestEntities.Add(selectedFoup);

                                    // switch문 종료
                                    break;
                                }

                                // *** 3. NotArriveFoups를 첫번째 요소부터 확인하면서 State가 Buffer인 Foup 탐색 
                                notArriveFoups = procEqp.NotArriveEntities();

                                if (notArriveFoups.Any())
                                {
                                    Foup notArriveFoup = notArriveFoups.First();

                                    // ProcessPort에 NotArriveFoup 예약
                                    Reserve(notArriveFoup);

                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.PORT_RESERVE);
                                        simLogs.AddLog(log);
                                    }

                                    // NotArriveFoup 이동 명령 하달..
                                    RailPortNode stb = notArriveFoup.CurrentNode as RailPortNode;
                                    Scheduler.Instance.SendFoupToEqp(notArriveFoup, stb, this);

                                    // RequestEntities에 등록
                                    procEqp.RequestEntities.Add(notArriveFoup);
                                }
                                else
                                {
                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, null, this, ANIMATION.UNLOAD);
                                        simLogs.AddLog(log);
                                    }
                                }

                                break;
                        }
                        #endregion
                    }

                    break;
                // 2. OHT가 Foup을 Port에 Unloading함.
                // => Port에 Foup 탑재 후 바로 Eqp로 들어감
                case EXT_PORT.OHT_UNLOADING:

                    // Port에 Foup이 이미 예약되어 있고 곧 Load될 예정이므로(Port 기준) 현재 Port의 상태는 Reserved
                    if ((PROCESSPORT_STATE)NodeState == PROCESSPORT_STATE.RESERVED)
                    {
                        // Load되고나서 EQP를 기다리기 위한 상태로 변함

                        // Foup State 변경되기 전 현재 State 저장
                        prevResourceId = foup.CurrentNode.Name;
                        prevState = foup.CurrentState.ToString();
                        // Foup Node/State 변경
                        foup.CurrentNode = this;
                        foup.CurrentState = FOUP_STATE.WAIT;
                        // Foup State 변경된 후 현재 State 저장
                        nextResourceId = foup.CurrentNode.Name;
                        nextState = foup.CurrentState.ToString();
                        // Foup State 변경 기록
                        Scheduler.Instance.RecordFoupHistory(foup.Name, prevResourceId, nextResourceId, prevState, nextState, SimEngine.Instance.SimDateTime);

                        foup.TotalTravelingTime = foup.TotalTravelingTime + (simTime - foup.TravelingStartTime);
                        LoadedEntity = foup;

                        // Eqp로 Foup이 들어감..
                        foup.CurrentNode = ConnectedEqp;
                        LoadedEntity = null;
                        ReserveEntity = null;
                        IsReservation = false;
                        NodeState = PROCESSPORT_STATE.EMPTY;

                        #region Unload된 Foup을 Eqp에 넣고 Eqp는 Track-In 시도
                        port = new SimPort(EXT_PORT.EQP_IN, this, foup);
                        switch(ConnectedEqp)
                        {
                            case CompleteEqpNode completeEqp:
                                completeEqp.ExternalFunction(simTime, simLogs, port);
                                break;
                            case ProcessEqpNode procEqp:
                                procEqp.ExternalFunction(simTime, simLogs, port);
                                break;
                        }
                        #endregion

                        #region 다른 Foup을 부르는 코드 (CommitEqp는 Unload될 일이 없으니 케이스에서 제외)
                        // Foup을 Eqp로 보내고 
                        // 1. Eqp에 OutWait 상태의 Foup을 꺼낸다.
                        // 2. 1에서 Foup이 없을 시 Eqp에 예약된 Foup을 가져온다.
                        // 3. 2에서 Foup이 없을 시 Eqp에 도착하지 않은 Foup(다음 스케줄이 Eqp)을 가져온다.

                        switch (ConnectedEqp)
                        {
                            case CompleteEqpNode completeEqp:

                                // *** 2. Eqp의 ReserveList 조회 후 예약된 Foup 데려오기
                                // reserveList를 첫번째 요소부터 확인하면서 현재 State가 Buffer인 Foup 탐색
                                selectedFoup = null;

                                foreach (Foup reservedFoup in completeEqp.ReserveList)
                                {
                                    if (reservedFoup.CurrentState == FOUP_STATE.BUFFER)
                                    {
                                        selectedFoup = reservedFoup;
                                        break; // 찾았으면 루프 종료
                                    }
                                }

                                if (selectedFoup != null)
                                {
                                    // selectedFoup을 현재 Eqp로 요청 (FromNode는 ProcessPort)

                                    // ProcessPort에 RequestFoup 예약
                                    Reserve(selectedFoup);

                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.PORT_RESERVE);
                                        simLogs.AddLog(log);
                                    }

                                    // RequestFoup 이동 명령 하달..
                                    RailPortNode stb = selectedFoup.CurrentNode as RailPortNode;
                                    Scheduler.Instance.SendFoupToEqp(selectedFoup, stb, this);

                                    // RequestEntities에 등록
                                    completeEqp.RequestEntities.Add(selectedFoup);

                                    // switch문 종료
                                    break;
                                }

                                // *** 3. NotArriveFoups를 첫번째 요소부터 확인하면서 State가 Buffer인 Foup 탐색 
                                notArriveFoups = completeEqp.NotArriveEntities();

                                if (notArriveFoups.Any())
                                {
                                    Foup notArriveFoup = notArriveFoups.First();

                                    // ProcessPort에 NotArriveFoup 예약
                                    Reserve(notArriveFoup);

                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.PORT_RESERVE);
                                        simLogs.AddLog(log);
                                    }

                                    // NotArriveFoup 이동 명령 하달..
                                    RailPortNode stb = notArriveFoup.CurrentNode as RailPortNode;
                                    Scheduler.Instance.SendFoupToEqp(notArriveFoup, stb, this);

                                    // RequestEntities에 등록
                                    completeEqp.RequestEntities.Add(notArriveFoup);
                                }
                                else
                                {
                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.UNLOAD);
                                        simLogs.AddLog(log);
                                    }
                                }

                                break;
                            case ProcessEqpNode procEqp:
                                // *** 1. OutWaitFoups
                                outWaitFoups = procEqp.GetOutWaitFoups();

                                // outWaitFoups의 첫번째 요소 반환. 없으면 null
                                outWaitFoup = outWaitFoups.Any() ? outWaitFoups.First() : null;

                                // 첫번째 요소 반환 성공 시 outWaitFoup 내보내기
                                if (outWaitFoup != null)
                                {
                                    // OutWaitFoup이 현재 포트에 예약
                                    Reserve(outWaitFoup);

                                    // OutWaitFoup이 현재 포트에 Load
                                    outWaitFoup.CurrentNode = this;
                                    LoadedEntity = outWaitFoup;
                                    NodeState = PROCESSPORT_STATE.FULL;

                                    if(SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, outWaitFoup, this, ANIMATION.LOAD);
                                        simLogs.AddLog(log);
                                    }

                                    // Eqp에서 OutWaitFoup 제거
                                    procEqp.InternalEntities.Remove(outWaitFoup);

                                    // OutWaitFoup을 다음 목적지로 이동시키는 명령 하달
                                    Scheduler.Instance.SendFoupToNext(outWaitFoup, this);

                                    // switch문 종료
                                    break;
                                }

                                // *** 2. Eqp의 ReserveList 조회 후 예약된 Foup 데려오기
                                // reserveList를 첫번째 요소부터 확인하면서 현재 State가 Buffer인 Foup 탐색
                                selectedFoup = null;

                                foreach (Foup reservedFoup in procEqp.ReserveList)
                                {
                                    if (reservedFoup.CurrentState == FOUP_STATE.BUFFER)
                                    {
                                        selectedFoup = reservedFoup;
                                        break; // 찾았으면 루프 종료
                                    }
                                }

                                if (selectedFoup != null)
                                {
                                    // selectedFoup을 현재 Eqp로 요청 (FromNode는 ProcessPort)

                                    // ProcessPort에 RequestFoup 예약
                                    Reserve(selectedFoup);

                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.PORT_RESERVE);
                                        simLogs.AddLog(log);
                                    }

                                    // RequestFoup 이동 명령 하달..
                                    RailPortNode stb = selectedFoup.CurrentNode as RailPortNode;
                                    Scheduler.Instance.SendFoupToEqp(selectedFoup, stb, this);

                                    // RequestEntities에 등록
                                    procEqp.RequestEntities.Add(selectedFoup);

                                    // switch문 종료
                                    break;
                                }

                                // *** 3. NotArriveFoups를 첫번째 요소부터 확인하면서 State가 Buffer인 Foup 탐색 
                                notArriveFoups = procEqp.NotArriveEntities();

                                if (notArriveFoups.Any())
                                {
                                    Foup notArriveFoup = notArriveFoups.First();

                                    // ProcessPort에 NotArriveFoup 예약
                                    Reserve(notArriveFoup);

                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.PORT_RESERVE);
                                        simLogs.AddLog(log);
                                    }

                                    // NotArriveFoup 이동 명령 하달..
                                    RailPortNode stb = notArriveFoup.CurrentNode as RailPortNode;
                                    Scheduler.Instance.SendFoupToEqp(notArriveFoup, stb, this);

                                    // RequestEntities에 등록
                                    procEqp.RequestEntities.Add(notArriveFoup);
                                }
                                else
                                {
                                    if (SimEngine.Instance.IsAnimation)
                                    {
                                        log = new SimLog(simTime, simTime + 1, this, ANIMATION.UNLOAD);
                                        simLogs.AddLog(log);
                                    }
                                }

                                break;
                        }
                        #endregion
                    }

                    break;
                default:
                    Console.WriteLine("Error...");
                    break;
            }
        }
    }
}
