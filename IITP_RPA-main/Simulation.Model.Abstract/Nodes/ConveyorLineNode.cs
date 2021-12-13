//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Simulation.Engine;
//using Simulation.Geometry;

//namespace Simulation.Model.Abstract
//{
//    public class ConveyorLineNode : SimNode
//    {
//        #region Variables
//        private List<uint> _lstEntityID;
//        private List<Time> _lstEntityTime;
//        private List<float> _lstEntityLength;
//        //private List<float> _lstEntityEndLength;
//        private float _intervalLength;
//        private float _speed;
//        private Vector3 _startPt;
//        private Vector3 _endPt;
//        private float _length;
//        Vector2 _direction;

//        public float IntervalLength
//        {
//            get { return _intervalLength; }
//            set { _intervalLength = value; }
//        }
//        public float Speed
//        {
//            get { return _speed; }
//            set { _speed = value; }
//        }
//        public Vector2 Direction
//        {
//            get { return _direction; }
//            set { _direction = value; }
//        }
        
//        #endregion
//        public ConveyorLineNode(uint ID, string name, Vector3 startPt, Vector3 endPt, float intervalLength = 1, float speed = 1)
//            : base(ID, name)
//        {
//            _startPt = startPt;
//            _endPt = endPt;
//            RefreshLength();
//            _intervalLength = intervalLength;
//            _speed = speed;
//        }
//        public override void InitializeNode(EventCalendar evtCal)
//        {
//            base.InitializeNode(evtCal);
//            _lstEntityID = new List<uint>();
//            _lstEntityLength = new List<float>();            
//            _lstEntityTime = new List<Time>();
//        }

//        public void RefreshLength()
//        {
//            _length = (float)Math.Sqrt((_startPt.X - _endPt.X) * (_startPt.X - _endPt.X) + (_startPt.Y - _endPt.Y) * (_startPt.Y - _endPt.Y));
//        }

//        public override void RequirePort(Time simTime, SimHistory simLogs)
//        {
//            if (ReserveEntites.Count() > 0)
//            {
//                SimNode node = OutLink.EndNode;
//                SimPort port = ReserveEntites[0];
//                node.ExternalFunction(simTime, simLogs, port);
//                ReserveEntites.RemoveAt(0);
//            }
//        }

//        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
//        {
//            int lstIdx = -1;
//            for (int i = 0; i < _lstEntityID.Count(); i++)
//                if (_lstEntityID[i] == port.Entity.ID)
//                {
//                    lstIdx = i;
//                    break;
//                }
//            float movingLength = (float)(simTime - _lstEntityEvtTime[0]) * _speed;

//            switch ((INT_PORT)port.PortType)
//            {
//                case INT_PORT.CONVEYOR_START_INTERVAL_POINT_MOVE:
//                    if (movingLength > 0)
//                    {
//                        if (movingLength + _lstEntityEndLength[lstIdx] >= _intervalLength) //start interval point 도착
//                        {
//                            _lstEntityEvtTime[lstIdx] = simTime;
//                            _lstEntityStartLength[lstIdx] = _lstEntityEndLength[lstIdx];
//                            _lstEntityEndLength[lstIdx] = _intervalLength;
//                            SimPort newPort = new SimPort(INT_PORT.CONVEYOR_START_INTERVAL_POINT_ARRIVE, this, port.Entity);
//                            EvtCalendar.AddEvent(simTime + (_lstEntityEndLength[lstIdx] - _lstEntityStartLength[lstIdx]) / _speed, this, newPort);
//                            SimLog log = new SimLog(simTime, simTime + (_lstEntityEndLength[lstIdx] - _lstEntityStartLength[lstIdx]) / _speed, port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[lstIdx], _direction);
//                            simLogs.AddLog(log);
//                        }
//                        else //start interval point 도중
//                        {
//                            _lstEntityEvtTime[lstIdx] = simTime;
//                            _lstEntityStartLength[lstIdx] = _lstEntityEndLength[lstIdx];
//                            _lstEntityEndLength[lstIdx] = _lstEntityEndLength[lstIdx] + movingLength;
//                            SimPort newPort = new SimPort(INT_PORT.CONVEYOR_START_INTERVAL_POINT_MOVE, this, port.Entity);
//                            EvtCalendar.AddEvent(simTime + movingLength / _speed, this, newPort);
//                            SimLog log = new SimLog(simTime, simTime + movingLength / _speed, port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[lstIdx], _direction);
//                            simLogs.AddLog(log);
//                        }                        
//                    }
//                    break;

//                case INT_PORT.CONVEYOR_START_INTERVAL_POINT_ARRIVE:
//                    if (_lstEntityID[0] == port.Entity.ID)//컨베이어라인에 들어왔을 때 아무것도 없을 경우
//                    {
//                        _lstEntityEvtTime[0] = simTime;
//                        _lstEntityStartLength[0] = _lstEntityEndLength[0];
//                        _lstEntityEndLength[0] = _length - _intervalLength;
//                        Receivable = true;
                        
//                        SimPort newPort = new SimPort(INT_PORT.CONVEYOR_END_INTERVAL_POINT_ARRIVE, this, port.Entity);
//                        EvtCalendar.AddEvent(simTime + ((_lstEntityEndLength[0] - _lstEntityStartLength[0]) / _speed), this, newPort);
//                        SimLog log = new SimLog(simTime, simTime + ((_lstEntityEndLength[0] - _lstEntityStartLength[0]) / _speed), port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[lstIdx], _direction);
//                        simLogs.AddLog(log);
//                        InLinks[0].StartNode.RequirePort(simTime, simLogs);
//                    }
//                    else // 컨베이어라인에 들어왔을 때 앞에 entity 있는 경우
//                    {
//                        if (movingLength > 0)
//                        {
//                            _lstEntityEvtTime[lstIdx] = simTime;
//                            _lstEntityStartLength[lstIdx] = _lstEntityEndLength[lstIdx];
//                            _lstEntityEndLength[lstIdx] = _lstEntityEndLength[lstIdx] + movingLength;
//                            Receivable = true;
//                            SimPort newPort = new SimPort(INT_PORT.CONVEYOR_END_INTERVAL_POINT_MOVE, this, port.Entity);
//                            EvtCalendar.AddEvent(simTime + movingLength / _speed, this, newPort);
//                            SimLog log = new SimLog(simTime, simTime + movingLength / _speed, port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[lstIdx], _direction);
//                            simLogs.AddLog(log);
//                            InLinks[0].StartNode.RequirePort(simTime, simLogs);
//                        }
//                    }
//                    break;

//                case INT_PORT.CONVEYOR_END_INTERVAL_POINT_MOVE:
//                    if (movingLength > 0)
//                    {
//                        if (movingLength + _lstEntityEndLength[lstIdx] >= _length - _intervalLength) //end interval point 도착
//                        {
//                            _lstEntityEvtTime[lstIdx] = simTime;
//                            _lstEntityStartLength[lstIdx] = _lstEntityEndLength[lstIdx];
//                            _lstEntityEndLength[lstIdx] = _length - _intervalLength;
//                            SimPort newPort = new SimPort(INT_PORT.CONVEYOR_END_INTERVAL_POINT_ARRIVE, this, port.Entity);
//                            EvtCalendar.AddEvent(simTime + (_lstEntityEndLength[lstIdx] - _lstEntityStartLength[lstIdx]) / _speed, this, newPort);
//                            SimLog log = new SimLog(simTime, simTime + (_lstEntityEndLength[lstIdx] - _lstEntityStartLength[lstIdx]) / _speed, port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[lstIdx], _direction);
//                            simLogs.AddLog(log);
//                        }
//                        else //end interval point 도중
//                        {
//                            _lstEntityEvtTime[lstIdx] = simTime;
//                            _lstEntityStartLength[lstIdx] = _lstEntityEndLength[lstIdx];
//                            _lstEntityEndLength[lstIdx] = _lstEntityEndLength[lstIdx] + movingLength;
//                            SimPort newPort = new SimPort(INT_PORT.CONVEYOR_END_INTERVAL_POINT_MOVE, this, port.Entity);
//                            EvtCalendar.AddEvent(simTime + movingLength / _speed, this, newPort);
//                            SimLog log = new SimLog(simTime, simTime + movingLength / _speed, port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[lstIdx], _direction);
//                            simLogs.AddLog(log);
//                        }
//                    }
//                    break;

//                case INT_PORT.CONVEYOR_END_INTERVAL_POINT_ARRIVE:
//                    if (OutLink.EndNode.Receivable)
//                    {
//                        _lstEntityEvtTime[lstIdx] = simTime;
//                        _lstEntityStartLength[lstIdx] = _lstEntityEndLength[lstIdx];
//                        _lstEntityEndLength[lstIdx] = _lstEntityEndLength[lstIdx] + _intervalLength;
//                        SimPort newPort = new SimPort(INT_PORT.CONVEYOR_END_POINT, this, port.Entity);
//                        EvtCalendar.AddEvent(simTime + _intervalLength / _speed, this, newPort);
//                        SimLog log = new SimLog(simTime, simTime + _intervalLength / _speed, port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[lstIdx], _direction);
//                        simLogs.AddLog(log);

//                        //움직이는 순간 모두 체크 코드 추가해야함..
//                    }
//                    break;

//                case INT_PORT.CONVEYOR_END_POINT:
//                    OutputFunction(simTime, simLogs, port);
//                    break;



//                default:
//                    break;
//            }
//        }
//        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
//        {
//            if (port.Entity.ID == 0)
//                Console.WriteLine("[" + simTime + "] : Conveyor Line: " + port.Entity.ID);

//            switch ((EXT_PORT)port.PortType)
//            {
//                case EXT_PORT.PART: //컨베이어 들어오면 무조건 start_interval_point 까지 이동
//                    Receivable = false;
//                    _lstEntityID.Add(port.Entity.ID);
//                    _lstEntityEvtTime.Add(simTime);
//                    _lstEntityStartLength.Add(0);

//                    if (_lstEntityID[0] == port.Entity.ID) //컨베이어에 아무것도 없는 상태에 들어온 경우
//                    {
//                        _lstEntityEndLength.Add(_intervalLength * _speed);
//                        SimPort newPort = new SimPort(INT_PORT.CONVEYOR_START_INTERVAL_POINT_ARRIVE, this, port.Entity);
//                        SimLog log = new SimLog(simTime, simTime + _intervalLength / _speed, port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[0], _direction);
//                        simLogs.AddLog(log);
//                        EvtCalendar.AddEvent(simTime + _intervalLength / _speed, this, newPort);
//                    }
//                    else
//                    {
//                        float movingLength = (_lstEntityEndLength[0] - _lstEntityStartLength[0]);
//                        if (movingLength >= _intervalLength) //start interval point 까지
//                        {
//                            _lstEntityEndLength.Add(_intervalLength * _speed);
//                            SimPort newPort = new SimPort(INT_PORT.CONVEYOR_START_INTERVAL_POINT_ARRIVE, this, port.Entity);
//                            SimLog log = new SimLog(simTime, simTime + _intervalLength / _speed, port.Entity, this, ANIMATION.PART_MOVE_ON_CONVEYOR, (double)_speed, (double)_lstEntityStartLength[_lstEntityStartLength.Count()-1], _direction);
//                            simLogs.AddLog(log);
//                            EvtCalendar.AddEvent(simTime + _intervalLength / _speed, this, newPort);
//                        }
//                        else //start interval point 도착 전
//                        {
//                            _lstEntityEndLength.Add(movingLength * _speed);
//                            SimPort newPort = new SimPort(INT_PORT.CONVEYOR_START_INTERVAL_POINT_MOVE, this, port.Entity);
//                            EvtCalendar.AddEvent(simTime + movingLength / _speed, this, newPort);
//                        }
//                    }
//                    break;

//                default:
//                    break;
//            }
//        }
//        public override void OutputFunction(Time simTime, SimHistory simLogs, SimPort port)
//        {
//            if (port.PortType is INT_PORT)
//            {
//                switch ((INT_PORT)port.PortType)
//                {
//                    case INT_PORT.CONVEYOR_END_POINT:
//                        _lstEntityID.RemoveAt(0);
//                        _lstEntityEvtTime.RemoveAt(0);
//                        _lstEntityStartLength.RemoveAt(0);
//                        _lstEntityEndLength.RemoveAt(0);
//                        OutLink.EndNode.ExternalFunction(simTime, simLogs, new SimPort(EXT_PORT.PART, this, port.Entity));

//                        break;
//                }
//            }
//        }
//    }
//}
