using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    public class RailPointNode : FabSimNode
    {
        #region Variables
        private NetworkPointNode _nwPoint;
        private List<SimPort> _lstStopPort;
        private OHTNode _ohtNode;
        private RailLineNode _fromRailLine;
        private RailLineNode _toRailLine;
        private Time movingTime;
        private Time arrivalTime;
        private List<RailLineNode> lines;
        private ZCU _zcu;
        private ZCU_TYPE _zcuType;
        private List<RailLineNode> zcuStopNResetLines;

        [Browsable(true)]
        [DisplayName("Name")]
        public string RailPointName { get { return Name; } }

        [Browsable(false)]
        public NetworkPointNode NWPoint
        {
            get { return _nwPoint; }
            set { _nwPoint = value; }
        }

        [Browsable(true)]
        public List<RailLineNode> FromLines
        {
            get
            {
                List<RailLineNode> fromLines = new List<RailLineNode>();
                foreach (SimLink link in InLinks)
                {
                    fromLines.Add(link.StartNode as RailLineNode);
                }

                return fromLines;
            }
        }

        [Browsable(true)]
        public List<RailLineNode> ToLines
        {
            get
            {
                List<RailLineNode> toLines = new List<RailLineNode>();
                foreach (SimLink link in OutLinks)
                {
                    toLines.Add(link.EndNode as RailLineNode);
                }

                return toLines;
            }
        }

        [Browsable(true)]
        public List<SimPort> StopPorts
        { get { return _lstStopPort; } }

        [Browsable(true)]
        public ZCU_TYPE ZcuType
        {
            get { return _zcuType; }
            set { _zcuType = value; }
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
            set
            {
                if (!ModelManager.Instance.Zcus.ContainsKey(value))
                {
                    _zcu = ModelManager.Instance.AddZcu(Fab.Name, value);
                }
                else
                    _zcu = ModelManager.Instance.Zcus[value];
            }
        }

        [Browsable(false)]
        public Fab Fab { get { return base.Fab; } }

        [DisplayName("Fab")]
        [Browsable(true)]
        public string FabName { get { return Fab.Name; } }

        [Browsable(false)]
        public Enum NodeType { get { return base.NodeType; } }

        [Browsable(false)]
        public string NodeStateName { get { return base.NodeStateName; } }

        [Browsable(false)]
        public Vector3 PosVec3 { get { return base.PosVec3; } set { base.PosVec3 = value; } }

        [Browsable(true)]
        [DisplayName("Position(m)")]
        public Vector2 Position { get { return new Vector2(PosVec3.X / 1000, PosVec3.Y / 1000); } }

        [DisplayName("RailPort List")]
        [Browsable(true)]
        public List<RailPortNode> DicRailPort { get { return base.DicRailPort; } set { base.DicRailPort = value; } }

        [Browsable(false)]
        public RailLineNode ToLeftLine
        {
            get
            {
                foreach (RailLineNode line in ToLines)
                {
                    if (line.Left)
                        return line;
                }

                return null;
            }
        }

        [Browsable(false)]
        public RailLineNode ToRightLine
        {
            get
            {
                foreach (RailLineNode line in ToLines)
                {
                    if (line.Left == false)
                        return line;
                }
                return null;
            }
        }

        [Browsable(true)]
        public bool IsFull { get; set; }


        #endregion

        public RailPointNode(uint ID, Vector3 pos, string name, Fab fab, ZCU zcu, ZCU_TYPE zcuType)
            : base(ID, name, pos, fab)
        {
            IsFull = false;
            movingTime = 0;
            _zcu = zcu;
            _zcuType = zcuType;
            _lstStopPort = new List<SimPort>();
            zcuStopNResetLines = new List<RailLineNode>();
        }

        public RailPointNode(uint ID, string name, Fab fab, ZCU zcu, ZCU_TYPE zcuType)
            : base(ID, name, fab)
        {
            IsFull = false;
            movingTime = 0;
            _zcu = zcu;
            _zcuType = zcuType;
            _lstStopPort = new List<SimPort>();
            zcuStopNResetLines = new List<RailLineNode>();
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);
        }

        public RailLineNode GetAnotherDividedToLine(RailLineNode toLine)
        {
            foreach (RailLineNode lineTemp in ToLines)
            {
                if (toLine != lineTemp)
                    return lineTemp;
            }
            return null;
        }

        /// <summary>
        /// 분기하는 Point와 OHT 보내는 걸 번갈아가면서 결정하는 옆쪽 Point 소환
        /// </summary>
        /// <param name="toLine"></param>
        /// <returns></returns>
        public RailPointNode GetAnohterDividedStartingPoint(RailLineNode toLine)
        {
            foreach (RailLineNode lineTemp in ToLines)
            {
                if (toLine != lineTemp)
                {
                    RailLineNode anotherLine = lineTemp.ToNode.GetAnotherJoinedToLine(lineTemp);

                    if (anotherLine != null)
                        return anotherLine.FromNode;
                }
            }
            return null;
        }

        public RailPointNode GetAnotherJoinedStartingPoint(RailLineNode toLine)
        {
            RailPointNode to2Point = toLine.ToNode;

            foreach (RailLineNode lineTemp in to2Point.FromLines)
            {
                if (toLine != lineTemp)
                    return lineTemp.FromNode;
            }
            return null;
        }

        public RailLineNode GetAnotherJoinedToLine(RailLineNode toLine)
        {
            RailPointNode to2Point = toLine.ToNode;

            foreach (RailLineNode lineTemp in to2Point.FromLines)
            {
                if (toLine != lineTemp)
                    return lineTemp;
            }
            return null;
        }


        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch ((INT_PORT)port.PortType)
            {
                case INT_PORT.OHT_IN:
                    OHTNode oht = port.OHTNode as OHTNode;
                    RailLineNode fromLine = oht.LstRailLine[0];

                    if (oht.Name == "M14A_V_725")
                        ;

                    RailLineNode toLine = null;
                    if (oht.LstRailLine.Count > 1)
                        toLine = oht.LstRailLine[1];

                    foreach (SimPort portTemp in _lstStopPort.ToList())
                    {
                        if (portTemp.OHTNode.Name == oht.Name)
                            _lstStopPort.Remove(portTemp);
                    }

                    if (Zcu != null && ZcuType == ZCU_TYPE.STOP && !zcuStopNResetLines.Contains(toLine))
                    {
                        if(!Zcu.Ohts.Contains(oht))
                            Zcu.Ohts.Add(oht);

                        if (Zcu.StopOHTs.Contains(oht))
                        {
                            Zcu.StopOHTs.Remove(oht);
                        }
                        
                        Zcu.PlusOHTCount(oht);
                    }
                    else if (Zcu != null && ZcuType == ZCU_TYPE.RESET)
                    {
                        Zcu.Ohts.Remove(oht);
                        Zcu.MinusOHTCount(oht);
                        bool removeReservation = Zcu.RemoveReservation(oht);
                        if (!removeReservation)
                            ;
                        Zcu.ProcessReservation(simTime, simLogs, this);

                        if (oht.LstZCU.Count == 0)
                        {
                            oht.ReservationTime = Time.Zero;
                            oht.ZcuResetPointName = string.Empty;
                            oht.CurZcu = null;
                        }
                        else if (oht.LstZCU.Count == 1)
                        {
                            oht.ReservationTime = Time.Zero;
                            oht.ZcuResetPointName = string.Empty;
                            oht.CurZcu = null;
                            oht.LstZCU.Remove(Zcu);
                        }
                        else if(oht.LstZCU.Count > 1)
                        {
                            Time reservationTime = Time.Zero;
                            string resetPointName = string.Empty;
                            oht.LstZCU[1].getReservationInfo(oht.Name, ref reservationTime, ref resetPointName);
                            oht.ReservationTime = reservationTime;
                            oht.ZcuResetPointName = resetPointName;
                            oht.CurZcu = oht.LstZCU[1];
                            oht.LstZCU.Remove(Zcu);
                        }
                    }
                    else if(zcuStopNResetLines.Contains(toLine))
                    {
                        bool removeReservation = Zcu.RemoveReservation(oht);
                        if (!removeReservation)
                            ;
                        Zcu.ProcessReservation(simTime, simLogs, this);

                        if(oht.LstZCU.Count == 0)
                        {
                            oht.ReservationTime = Time.Zero;
                            oht.ZcuResetPointName = string.Empty;
                            oht.CurZcu = null;
                        }
                        else if (oht.LstZCU.Count == 1)
                        {
                            oht.ReservationTime = Time.Zero;
                            oht.ZcuResetPointName = string.Empty;
                            oht.CurZcu = null;
                            oht.LstZCU.Remove(Zcu);
                        }
                        else if (oht.LstZCU.Count > 1)
                        {
                            Time reservationTime = Time.Zero;
                            string resetPointName = string.Empty;
                            oht.LstZCU[1].getReservationInfo(oht.Name, ref reservationTime, ref resetPointName);
                            oht.ReservationTime = reservationTime;
                            oht.ZcuResetPointName = resetPointName;
                            oht.CurZcu = oht.LstZCU[1];
                            oht.LstZCU.Remove(Zcu);
                        }
                    }

                    _fromRailLine = fromLine;

                    SimPort newPort = new SimPort(EXT_PORT.OHT_OUT, this, oht);
                    _fromRailLine.ExternalFunction(simTime, simLogs, newPort);
                    break;
                default:
                    break;
            }
        }
        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.OHT_CHECK_IN:
                    OHTNode oht = port.OHTNode as OHTNode;

                    bool isEnableEnter = CheckEnter(simTime, oht);
                    if (isEnableEnter)
                    {
                        port.setType(INT_PORT.OHT_IN);
                        EvtCalendar.AddEvent(simTime, this, port);
                    }
                    else//통과를 못했다는건 ZCU Stop에 의해 멈췄다는 것. ZCU도 있고, Stop Point라는 뜻.
                    {
                        port.Time = simTime;

                        //ZCU가 null이 아니고 ZCU의 StopOHTs가 oht를 포함하지 않으면
                        if (Zcu != null && !Zcu.StopOHTs.Contains(oht))
                        {
                            Zcu.StopOHTs.Add(oht);
                            SimPort newPort = new SimPort(EXT_PORT.OHT_MOVE, this, oht);

                            bool isPort = false;
                            foreach (SimPort portTemp in _lstStopPort.ToList())
                            {
                                if (portTemp.OHTNode.Name == oht.Name)
                                    isPort = true;
                            }

                            if (isPort == false)
                                _lstStopPort.Add(newPort);

                            if (oht.CurZcu == null) //CurZcu가 없다는 것은 예약이 아직 안되어있다는 뜻.
                               Zcu.AddReservation(simTime, oht, this);
   
                            oht.CurRailLine.ExternalFunction(simTime, simLogs, newPort);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void SaveStopNResetLine()
        {
            List<RailLineNode> visitedLines = new List<RailLineNode>();

            foreach(RailLineNode toLine in ToLines)
            {
                SearchStopNResetLine(toLine, toLine, visitedLines);
            }
        }

        private void SearchStopNResetLine(RailLineNode line, RailLineNode connectedLine, List<RailLineNode> visitedLines)
        {
            visitedLines.Add(line);

            RailPointNode point = line.ToNode;       

            if(point.Zcu == null) // Topoint의 Zcu가 없으면 다음 라인들로 이동.
            {
                foreach(RailLineNode toLine in point.ToLines)
                {
                    if ( !visitedLines.Contains(toLine))
                        SearchStopNResetLine(toLine, connectedLine, visitedLines);
                }
            }
            else if(point.Zcu.Name != Zcu.Name) // 다른 Zcu가 나오면 라인을 저장.  
            {
                zcuStopNResetLines.Add(connectedLine);
            }
        }

        public bool CheckEnter(Time simTime, OHTNode oht)
        {
            bool isEnableEnter = false;

            if ((oht.LstRailLine.Count > 1 && zcuStopNResetLines.Contains(oht.LstRailLine[1]))
            || (Zcu != null && ZcuType == ZCU_TYPE.STOP && Zcu.IsAvailableToEnter(oht))
            || Zcu == null
            || ZcuType == ZCU_TYPE.NON
            || (Zcu != null && ZcuType == ZCU_TYPE.RESET))
                return true;

            return isEnableEnter;
        }

        private bool ContainsStop(SimPort port)
        {
            foreach (SimPort portTemp in _lstStopPort)
            {
                if (portTemp.OHTNode.ID == port.OHTNode.ID)
                    return true;
            }
            return false;
        }
    }
}
