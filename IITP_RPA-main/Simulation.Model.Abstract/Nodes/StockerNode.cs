using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simulation.Engine;
using System.ComponentModel;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    public class StockerNode : BufferNode
    {
        #region Member Variable


        public bool IsEmpty
        {
            get { return (LstEntity.Count == 0); }
        }

        #endregion

        public StockerNode()
            : base(0, "", BUFFER_TYPE.STOCKER, null)
        {
        }

        public StockerNode(uint ID, string name, Fab fab)
            : base(ID, name, BUFFER_TYPE.STOCKER, fab)
        {
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);
            NodeState = STOCKER_STATE.EMPTY;
        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch ((STOCKER_STATE)NodeState)
            {
                case STOCKER_STATE.EMPTY:
                    {
                        if (port.PortType.Equals(EXT_PORT.RESERVE))
                        {
                            NodeState = RAILPORT_STATE.RESERVED;
                        }
                        else
                            Console.WriteLine("External Func. error in STB Class, EMPTY, RESERVE ");

                        break;
                    }
                case STOCKER_STATE.RESERVED:
                    {
                        if (port.PortType.Equals(EXT_PORT.LOAD))
                        {
                            NodeState = RAILPORT_STATE.FULL;
                            LstEntity.Add(port.Entity);

                            SimLog log = new SimLog(simTime, simTime + 1, null, this, ANIMATION.STOCKER_PART_COUNT);
                            simLogs.AddLog(log);

                            //process에 넣어달라고 요청! (처음부터 요청하면 stb에 entity가 도착하기전에 빼갈수도 있음. 나중에 oht 운반중에 path를 바꿔서 process로 가도록 수정)
                            Scheduler.Instance.ReserveProcess(port.Entity, this);
                        }
                        else if (port.PortType.Equals(EXT_PORT.REQUEST_PART))
                        {
                            SimLog log = new SimLog(simTime + 0.1, simTime + 0.1, null, this, ANIMATION.STOCKER_PART_COUNT);
                            simLogs.AddLog(log);

                            OutputFunction(simTime, simLogs, port);
                            NodeState = RAILPORT_STATE.EMPTY;
                        }
                        else
                            Console.WriteLine("External Func. error in STB Class, RESERVED, PART_IN");

                        break;
                    }
                case STOCKER_STATE.FULL:
                    {
                        if (port.PortType.Equals(EXT_PORT.RESERVE))
                        {
                            NodeState = RAILPORT_STATE.RESERVED;
                        }
                        else
                            Console.WriteLine("External Func. error in STB Class, FULL, Not RESERVE");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public override void OutputFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch ((STOCKER_STATE)NodeState)
            {
                case STOCKER_STATE.RESERVED:
                    {
                        if (port.PortType.Equals(EXT_PORT.REQUEST_PART))
                        {
                            // !Part out
                            SimPort newPort = new SimPort(EXT_PORT.PART, this, port.Entity);
                            newPort.OHTNode = port.OHTNode;
                            port.OHTNode.ExternalFunction(simTime, simLogs, newPort);

                            LstEntity.Remove(port.Entity); //이러면 OHT에 들어간 Entity도 null이 되나? 확인해볼것!!
                        }
                        else
                            Console.WriteLine("External Func. error in STB Class, STB_State.RESERVED, EXT_PORT.PART_REQ");

                        break;
                    }
            }

        }

        public override uint GetRemainCapacity()
        {
            return (uint)(Capacity - LstEntity.Count);
        }

        public double GetDistance(Vector3 position)
        {
            return (position.X - PosVec3.X) * (position.X - PosVec3.X) + (position.Y - PosVec3.Y) * (position.Y - PosVec3.Y);
        }
    }
}
