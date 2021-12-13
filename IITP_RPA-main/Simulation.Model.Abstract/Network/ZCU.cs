using Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    /// <summary>
    /// ZCU의 기능
    /// Stop, Reset Point가 모두 같으면 후발 OHT도 같이 지나갈 수 있다. 
    /// Stop은 같은데 Reset은 다르면 후발 OHT도 같이 지나갈 수 있다.
    /// Stop도 다르고 Reset도 다르면 후발 OHT도 같이 지나갈 수 있다.
    /// Stop은 다르고 Reset은 같으면 후발 OHT가 Stop Point에서 기다렸다가 선발 OHT가 나가면 들어감.
    /// </summary>
    public class ZCU
    {
        private string _name;
        private Dictionary<string, RailPointNode> _fromPoints; // Key: stopPointName
        private Dictionary<string, RailPointNode> _toPoints; // Key: resetPointName
        private List<RailLineNode> _lines;
        private List<OHTNode> _ohts;
        private Fab _fab;
        private Dictionary<Tuple<string, string>, List<ZCUReservation>> _reservations; //Key: stopPointName, resetPointName
        private Dictionary<Tuple<string, string>, List<OHTNode>> _routeOHTs; //Key: stopPointName, resetPointName
        private List<OHTNode> _stopOHTs;
        public string Name
        {
            get { return _name; }
        }

        public Fab Fab
        {
            get { return _fab; }
        }

        public List<OHTNode> Ohts
        {
            get
            {
                return _ohts;
            }
        }

        public bool IsBusy
        {
            get
            {
                if (_ohts.Count > 0)
                    return true;
                else
                    return false;
            }
        }
        public Dictionary<string, RailPointNode> FromPoints
        {
            get { return _fromPoints; }
        }

        public Dictionary<string, RailPointNode> ToPoints
        {
            get { return _toPoints; }
        }

        public List<RailLineNode> Lines
        {
            get { return _lines; }
        }

        public Dictionary<Tuple<string, string>, List<ZCUReservation>> Reservations
        {
            get { return _reservations; }
        }

        public Dictionary<Tuple<string, string>, List<OHTNode>> RouteOHTs
        {
            get { return _routeOHTs; }
        }
        public List<OHTNode> StopOHTs
        {
            get
            {
                return _stopOHTs;
            }
        }

        public ZCU(string name, Fab fab)
        {
            _name = name;
            _fab = fab;
            _ohts = new List<OHTNode>();
            _fromPoints = new Dictionary<string, RailPointNode>();
            _toPoints = new Dictionary<string, RailPointNode>();
            _lines = new List<RailLineNode>();
            _reservations = new Dictionary<Tuple<string, string>, List<ZCUReservation>>();
            _routeOHTs = new Dictionary<Tuple<string, string>, List<OHTNode>>();
            _stopOHTs = new List<OHTNode>();
        }

        public void SetLines()
        {
            _lines.Clear();
            foreach (RailPointNode inPoint in _fromPoints.Values)
            {
                List<RailPointNode> railPointNodes = new List<RailPointNode>();
                List<RailLineNode> lines = new List<RailLineNode>();
                InitializeReservationNohtCount(inPoint, railPointNodes, lines, inPoint.Name);

                //foreach (RailLineNode line in inPoint.ToLines)
                //{
                //    RailLineNode lineTemp = line;
                //    RailPointNode outPoint = line.ToNode;
                //    if (!_lines.Contains(lineTemp))
                //    {
                //        _lines.Add(lineTemp);
                //        lineTemp.Zcu = this;
                //    }
                //    outPoint = lineTemp.ToNode;
                    
                //    if (outPoint.ToLines.Count == 0)
                //        continue;

                //    lineTemp = outPoint.ToLines[0];
                //    while (outPoint.ZcuType != ZCU_TYPE.RESET)
                //    {
                //        if (!_lines.Contains(lineTemp))
                //        {
                //            _lines.Add(lineTemp);
                //            lineTemp.Zcu = this;
                //        }
                //        outPoint = lineTemp.ToNode;
                //        lineTemp = outPoint.ToLines[0];
                //    }
                //}
            }
        }

        public void InitializeReservationNohtCount(RailPointNode currentPoint, List<RailPointNode> visitedPoints, List<RailLineNode> zoneToLines, string stopPointName)
        {
            visitedPoints.Add(currentPoint);

            List<RailLineNode> nextLines = currentPoint.ToLines.Where(line => !zoneToLines.Contains(line)).ToList();

            foreach(RailLineNode nextLine in nextLines)
            {
                Lines.Add(nextLine);
                nextLine.Zcu = this;
            }
            List<RailPointNode> nextNodes = nextLines.Select(line => line.ToNode).ToList();

            while (nextNodes.Any())
            {
                RailPointNode nextNode = nextNodes.First();

                nextNodes.Remove(nextNode);

                if (nextNode.ZcuName == Name && nextNode.ZcuType == ZCU_TYPE.RESET)
                {
                    RailPointNode resetFindingNode = nextNode.ToLines[0].ToNode;
                    List<RailLineNode> additionalLines = new List<RailLineNode>();
                    additionalLines.Add(nextNode.ToLines[0]);
                    while(resetFindingNode.ZcuType == ZCU_TYPE.NON)
                    {
                        resetFindingNode = resetFindingNode.ToLines[0].ToNode;
                        additionalLines.Add(resetFindingNode.ToLines[0]);
                    }

                    if(resetFindingNode.ZcuName == Name && resetFindingNode.Name != nextNode.Name)
                    {
                        nextNode.Zcu = null;
                        nextNode.ZcuType = ZCU_TYPE.NON;
                        nextNode = resetFindingNode;
                        zoneToLines.AddRange(additionalLines);
                    }

                    if (!ToPoints.ContainsKey(nextNode.Name))
                        ToPoints.Add(nextNode.Name, nextNode);

                    Tuple<string, string> key = new Tuple<string, string>(stopPointName, nextNode.Name);

                    if(!Reservations.ContainsKey(key))
                    {
                        Reservations.Add(key, new List<ZCUReservation>());
                        RouteOHTs.Add(key, new List<OHTNode>());
                        Lines.AddRange(zoneToLines);
                    }
                }
                else if(nextNode.ZcuType != ZCU_TYPE.NON && nextNode.ZcuName != Name)
                {
                    Tuple<string, string> key = new Tuple<string, string>(stopPointName, stopPointName);

                    if(!Reservations.ContainsKey(key))
                    {
                        Reservations.Add(new Tuple<string, string>(stopPointName, stopPointName), new List<ZCUReservation>());
                        RouteOHTs.Add(new Tuple<string, string>(stopPointName, stopPointName), new List<OHTNode>());
                    }
                }
                else if (!visitedPoints.Contains(nextNode))
                {
                    InitializeReservationNohtCount(nextNode, visitedPoints, zoneToLines, stopPointName);
                }
            }
        }

        public void InitializeReservationNohtCount()
        {
            foreach(string stopPointName in _fromPoints.Keys)
            {
                foreach(string resetPointName in _toPoints.Keys)
                {
                    _reservations.Add(new Tuple<string, string>(stopPointName, resetPointName), new List<ZCUReservation>());
                    _routeOHTs.Add(new Tuple<string, string>(stopPointName, resetPointName), new List<OHTNode>());
                }
            }
        }

        public bool IsEnterZCU(OHTNode oht)
        {
            foreach (OHTNode zcuOht in _ohts.ToList())
            {
                if (zcuOht == oht) //들어가고자 하는 OHT와 이미 안에 있는 OHT가 같다? 
                    continue;

                bool isInZcuOht = false;
                foreach (RailLineNode line in _lines) //ZCU 안에 있는 OHT가 ZCU 라인 안에 있는게 맞다는걸 확인.
                {
                    if (line.ListOHT.Contains(zcuOht))
                    {
                        isInZcuOht = true;
                        break;
                    }
                }

                if (isInZcuOht == false && _ohts.Contains(zcuOht)) // 라인 안에는 없는데 ZCU가 가진 OHT에는 포함되어 있는 경우 지워버림.
                {
                    _ohts.Remove(zcuOht);
                    zcuOht.LstZCU.Remove(this);

                    if (zcuOht.LstZCU.Count == 0)
                        zcuOht.ReservationTime = Time.Zero;

                    continue;
                }

                //내부에 있는 OHT와 경로가 겹치는 경우 못들어오게 함.... 수정하자.  
                if (oht.LstRailLine.Count >= 1 && zcuOht.LstRailLine.Contains(oht.LstRailLine[0]))
                    return false;
                else if (oht.LstRailLine.Count >= 2 && (zcuOht.LstRailLine.Contains(oht.LstRailLine[0]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[1])))
                    return false;
                else if (oht.LstRailLine.Count >= 3 && (zcuOht.LstRailLine.Contains(oht.LstRailLine[0]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[1]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[2])))
                    return false;
                else if (oht.LstRailLine.Count >= 4 && (zcuOht.LstRailLine.Contains(oht.LstRailLine[0]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[1]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[2]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[3])))
                    return false;
                else if (oht.LstRailLine.Count >= 5 && (zcuOht.LstRailLine.Contains(oht.LstRailLine[0]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[1]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[2]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[3]) || zcuOht.LstRailLine.Contains(oht.LstRailLine[4])))
                    return false;
            }
            return true;
        }

        public void PlusOHTCount(OHTNode oht)
        {
            string stopPointName = string.Empty;
            string resetPointName = string.Empty;
            GetStopNResetPoint(oht, ref stopPointName, ref resetPointName);
            Tuple<string, string> stopNreset = new Tuple<string, string>(stopPointName, resetPointName);

            if(!_routeOHTs[stopNreset].Contains(oht))
                _routeOHTs[stopNreset].Add(oht);
        }

        public void MinusOHTCount(OHTNode oht)
        {
            if (Name == "ZCU_A3_12" && (oht.Name == "M14A_V_359" || oht.Name == "M14A_V_278" || oht.Name == "M14A_V_341"))
                ;

//            List<OHTNode> containedOHTList = null;
            foreach (List<OHTNode> routeOHTList in RouteOHTs.Values)
            {
                if (routeOHTList.Contains(oht))
                {
                    routeOHTList.Remove(oht);
//                    containedOHTList = routeOHTList;
//                    break;
                }

                int idx = 0;
                while(idx < routeOHTList.Count)
                {
                    OHTNode routeOHT = routeOHTList[idx];

                    if (!this.Lines.Contains(routeOHT.CurRailLine) && !HasReservation(routeOHT))
                        routeOHTList.Remove(routeOHT);
                    else
                        ++idx;
                }
            }

//            if(containedOHTList != null)
//                containedOHTList.Remove(oht);
        }

        /// Stop, Reset Point가 모두 같고 다른 Stop, 같은 Reset Point인 예약이 없으면 후발 OHT도 같이 지나갈 수 있다.
        /// Stop은 같은데 Reset은 다르면 후발 OHT도 같이 지나갈 수 있다.
        /// Stop도 다르고 Reset도 다르면 후발 OHT도 같이 지나갈 수 있다.
        /// Stop은 다르고 Reset은 같으면 후발 OHT가 Stop Point에서 기다렸다가 선발 OHT가 나가면 들어감.
        /// StopPoint에 도착해서 들어갈 수 있는지 확인하는 함수
        public bool IsAvailableToEnter(OHTNode oht)
        {
            try
            {
                string stopPointName = string.Empty;
                string resetPointName = string.Empty;
                GetStopNResetPoint(oht, ref stopPointName, ref resetPointName);
                Tuple<string, string> stopNreset = new Tuple<string, string>(stopPointName, resetPointName);

                foreach (KeyValuePair<Tuple<string, string>, List<OHTNode>> kvpOHTs in _routeOHTs)
                {
                    if (kvpOHTs.Key.Item1 != stopPointName
                        && kvpOHTs.Key.Item2 == resetPointName
                        && kvpOHTs.Value.Count > 0)
                    {
                        return false; //Stop은 다른데 Reset은 같은 OHT가 이미 안에 있는 경우 false
                    }
                }

                ZCUReservation ohtReservation = null;

                foreach(ZCUReservation reservation in Reservations[stopNreset])
                {
                    if (reservation.OHT.Name == oht.Name)
                        ohtReservation = reservation;
                }

                //Reset이 같은 StopOHT 중 가장 예약시간이 빠른 OHT를 탐색
                OHTNode fastestReservationStopOHT = null;
                Time reservationTime = -1;
                foreach (OHTNode stopOHT in _stopOHTs)
                {
                    if (stopOHT.ZcuResetPointName == resetPointName)
                    {
                        if (reservationTime == -1)
                        {
                            fastestReservationStopOHT = stopOHT;
                            reservationTime = stopOHT.ReservationTime;
                        }
                        else if (reservationTime > stopOHT.ReservationTime)
                        {
                            fastestReservationStopOHT = stopOHT;
                            reservationTime = stopOHT.ReservationTime;
                        }
                    }
                }

                // Reset이 같은 StopOHT 중 가장 예약시간이 빠른 OHT가 통과를 문의하는 OHT라면 보내준다. 
                if (fastestReservationStopOHT != null && fastestReservationStopOHT.Name == oht.Name)
                    return true;

                foreach (KeyValuePair<Tuple<string, string>, List<ZCUReservation>> kvpReservation in Reservations)
                {
                    List<ZCUReservation> compReservationList = kvpReservation.Value;

                    //Stop은 다른데 Reset은 같은 경우
                    if (kvpReservation.Key.Item1 != stopPointName && kvpReservation.Key.Item2 == resetPointName) 
                    {
                        if (ohtReservation == null && compReservationList.Count > 0)
                        {
                            return false;
                        }
                        else if (compReservationList.Count > 0 && compReservationList[0].ReservationTime < ohtReservation.ReservationTime)
                        {
                            return false;   // reset이 다른 OHT가 먼저 예약했으면 false
                        }
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("ZCU IsAvailableToEnter Error: " + ex.Message);
                return false;
            }
        }

        public bool GetStopNResetPoint(OHTNode oht, ref string stopPointName, ref string resetPointName)
        {
            if(oht.LstRailLine.Count == 1)
            {
                RailLineNode line = oht.LstRailLine[0];
                if (line.FromNode.ZcuType == ZCU_TYPE.STOP && _fromPoints.ContainsKey(line.FromNode.Name))
                    stopPointName = line.FromNode.Name;

                if (line.ToNode.ZcuType == ZCU_TYPE.RESET && line.ToNode.Zcu.Name == Name && _toPoints.ContainsKey(line.ToNode.Name))
                    resetPointName = line.ToNode.Name;
            }
            else
            {
                foreach (RailLineNode line in oht.LstRailLine)
                {
                    if (line.FromNode.ZcuType == ZCU_TYPE.STOP && _fromPoints.ContainsKey(line.FromNode.Name))
                        stopPointName = line.FromNode.Name;

                    if (line.ToNode.ZcuType == ZCU_TYPE.RESET && _toPoints.ContainsKey(line.ToNode.Name))
                        resetPointName = line.ToNode.Name;

                    if (stopPointName != string.Empty && resetPointName == string.Empty && line.ToNode.ZcuType != ZCU_TYPE.NON && line.ToNode.Zcu.Name != Name) // StopNReset case
                    {
                        resetPointName = stopPointName;
                        return true;
                    }
                    if(stopPointName != string.Empty && resetPointName != string.Empty)
                        break;
                }




            }

            if (stopPointName != string.Empty && resetPointName != string.Empty)
                return true;
            else if (stopPointName != string.Empty && resetPointName == string.Empty)
            {
                RailPointNode totoNode = oht.LstRailLine.Last().ToNode.ToLines[0].ToNode;

                bool meetAnotherZCU = false;
                while(resetPointName == null || !meetAnotherZCU)
                {
                    if (totoNode.ZcuName == Name && totoNode.ZcuType == ZCU_TYPE.RESET)
                    {
                        resetPointName = totoNode.Name;
                        return true;
                    }
                    else if (totoNode.ZcuType != ZCU_TYPE.NON && totoNode.ZcuName != Name)
                    {
                        meetAnotherZCU = true;
                    }
                    else
                        totoNode = totoNode.ToLines[0].ToNode;
                }
            }

            return false;
        }

        /// <summary>
        /// 변경된 경로를 반영해서 resetpoint 수정하고 지나갈 수 있는지 확인 후 무브 고고 
        /// </summary>
        /// <param name="oht"></param>
        public void ChangeReservationResetPoint(OHTNode oht)
        {
            ZCUReservation reservation = null;
            foreach (KeyValuePair<Tuple<string, string>, List<ZCUReservation>> kvpReservation in Reservations)
            {
                foreach (ZCUReservation reserTemp in kvpReservation.Value)
                {
                    if(reserTemp.OHT.Name == oht.Name)
                    {
                        reservation = reserTemp;
                    }
                }
            }

            if (reservation == null)
                return;

            string newStopPointName = string.Empty;
            string newResetPointName = string.Empty;

            GetStopNResetPoint(oht, ref newStopPointName, ref newResetPointName);

            if (newStopPointName == string.Empty || newResetPointName == string.Empty)
                return;

            RailPointNode newStopPoint = ModelManager.Instance.DicRailPoint[newStopPointName];
            RailPointNode newResetPoint = ModelManager.Instance.DicRailPoint[newResetPointName];

            if (newStopPointName != reservation.StopPoint.Name || newResetPointName != reservation.ResetPoint.Name)
            {
                bool isRemoved = Reservations[new Tuple<string, string>(reservation.StopPoint.Name, reservation.ResetPoint.Name)].Remove(reservation);

                if (!isRemoved)
                    ;
                reservation.StopPoint = newStopPoint;
                reservation.ResetPoint = newResetPoint;
                Reservations[new Tuple<string, string>(newStopPointName, newResetPointName)].Add(reservation);
                Reservations[new Tuple<string, string>(newStopPointName, newResetPointName)].Sort((x1, x2) => x1.ReservationTime.CompareTo(x2.ReservationTime));
                oht.ZcuResetPointName = newResetPointName;
            }
        }

        public bool ContainsReservation(OHTNode oht)
        {
            string stopPointName = string.Empty;
            string resetPointName = string.Empty;
            GetStopNResetPoint(oht, ref stopPointName, ref resetPointName);

            if (stopPointName == string.Empty && resetPointName == string.Empty)
                return false;

            foreach (ZCUReservation reservation in Reservations[new Tuple<string, string>(stopPointName, resetPointName)])
            {
                if (oht.Name == reservation.OHT.Name)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// resetPoint에서 OHT가 나갈 때
        /// 해당 resetPoint로 이동중인 OHT가 없고,
        /// 해당 resetPoint로 들어오고싶은 OHT가 StopPoint에서 대기중이면 들어오게 하는 함수
        /// </summary>
        /// <param name="simTime"></param>
        /// <param name="simLogs"></param>
        /// <param name="resetPoint"></param>
        public bool ProcessReservation(Time simTime, SimHistory simLogs, RailPointNode resetPoint)
        {
            ZCUReservation earlestReservation = null;
            RailPointNode earlestPoint = null;

            if (_stopOHTs.Count == 0)
                return false;

            foreach (KeyValuePair<Tuple<string, string>, List<OHTNode>> kvpOHTs in _routeOHTs)
            {
                if (kvpOHTs.Key.Item2 == resetPoint.Name
                    && kvpOHTs.Value.Count > 0)
                    return false; //Stop은 다른데 Reset은 같은 OHT가 이미 안에 있는 경우 false
            }

            OHTNode fastestReservationStopOHT = null;
            Time reservationTime = -1;
            foreach(OHTNode stopOHT in _stopOHTs)
            {
                if(stopOHT.ZcuResetPointName == resetPoint.Name)
                {
                    if(reservationTime == -1)
                    {
                        fastestReservationStopOHT = stopOHT;
                        reservationTime = stopOHT.ReservationTime;
                    }
                    else if(reservationTime > stopOHT.ReservationTime)
                    {
                        fastestReservationStopOHT = stopOHT;
                        reservationTime = stopOHT.ReservationTime;
                    }
                }
            }

            if (fastestReservationStopOHT == null)
                return false;

            ZCUReservation reservation = null;
            foreach (KeyValuePair<Tuple<string, string>, List<ZCUReservation>> kvpReservation in Reservations)
            {
                if (kvpReservation.Key.Item2 == resetPoint.Name)
                {
                    foreach(ZCUReservation tempReservation in kvpReservation.Value)
                    {
                        if(tempReservation.OHT.Name == fastestReservationStopOHT.Name)
                        {
                            reservation = tempReservation;
                            break;
                        }
                    }
                }
            }

            //예약이 있고, 예약 건 OHT이름이랑 StopPoint에 대기하고 있는 OHT랑 이름이 같을 때,
            if(reservation != null)
            {
                string newStopPointName = string.Empty;
                string newResetPointName = string.Empty;
                GetStopNResetPoint(reservation.OHT, ref newStopPointName, ref newResetPointName);

                RailPointNode stopPoint = reservation.StopPoint;

                if (stopPoint.StopPorts.Count == 0)
                    return false;

                stopPoint.StopPorts[0].setType(INT_PORT.OHT_IN);
                stopPoint.EvtCalendar.AddEvent(simTime, stopPoint, stopPoint.StopPorts[0]);
                stopPoint.StopPorts.Remove(stopPoint.StopPorts[0]);

                return true;
            }

            return false;
        }

        public void AddReservation(Time time, OHTNode oht, RailPointNode stopPoint)
        {
            string stopPointName = string.Empty;
            string resetPointName = string.Empty;
            GetStopNResetPoint(oht, ref stopPointName, ref resetPointName);
            Tuple<string, string> stopNreset = new Tuple<string, string>(stopPointName, resetPointName);
            RailPointNode resetPoint = ModelManager.Instance.DicRailPoint[resetPointName];
            ZCUReservation reservation = new ZCUReservation(time, oht, stopPoint, resetPoint);

            //foreach (ZCUReservation zcuReserv in _reservations[stopNreset])
            //{
            //    if (zcuReserv.OHT.Name != oht.Name && zcuReserv.OHT.CurRailLineName == oht.CurRailLineName)
            //    {
            //        int reservOHTIdx = zcuReserv.OHT.CurRailLine.ListOHT.IndexOf(zcuReserv.OHT);
            //        int ohtIdx = oht.CurRailLine.ListOHT.IndexOf(oht);
            //    }
            //}

            //이미 있으면 추가 하지 않음.
            ZCUReservation pastOHTReservation = null;
            foreach(List<ZCUReservation> listZcuReservation in _reservations.Values)
            {
                foreach(ZCUReservation zcuReserv in listZcuReservation)
                {
                    if (zcuReserv.OHT.Name == oht.Name && (zcuReserv.StopPoint.Name != stopPointName || zcuReserv.ResetPoint.Name != resetPointName))
                    {
                        pastOHTReservation = zcuReserv;
                        break;
                    }
                    else if (zcuReserv.OHT.Name == oht.Name && zcuReserv.StopPoint.Name == stopPointName && zcuReserv.ResetPoint.Name == resetPointName)
                        return;
                }
            }

            //이전 OHT예약을 삭제
            if(pastOHTReservation != null)
            {
                reservation.ReservationTime = pastOHTReservation.ReservationTime > reservation.ReservationTime ? reservation.ReservationTime : pastOHTReservation.ReservationTime;
                Tuple<string, string> pastStopNreset = new Tuple<string, string> (pastOHTReservation.StopPoint.Name, pastOHTReservation.ResetPoint.Name);
                Reservations[pastStopNreset].Remove(pastOHTReservation);
                oht.LstZCU.Remove(this);
                if (oht.LstZCU.Count == 0)
                    oht.ReservationTime = Time.Zero;
            }

            Time backReservationTime = Time.MaxValue; 
            foreach (List<ZCUReservation> listZcuReservation in _reservations.Values)
            {
                foreach (ZCUReservation zcuReservation in listZcuReservation)
                {
                    if (zcuReservation.StopPoint.Name != stopPointName)
                        continue;

                    bool isBackReservationOHT = false;

                    //먼저 예약된 OHT가 나보다 뒤쪽 라인에 있는 OHT인지 확인.
                    foreach (RailLineNode fromLine in oht.CurRailLine.FromNode.FromLines)
                    {
                        if (zcuReservation.OHT.CurRailLine == fromLine)
                        {
                            isBackReservationOHT = true;
                            break;
                        }
                    }

                    //먼저 예약한 애가 같은 라인에 있는데 나보다 뒤쪽에 있거나 뒤쪽 라인에 있으면 
                    if ((zcuReservation.OHT.CurRailLineName == oht.CurRailLineName && zcuReservation.OHT.CurDistance < oht.CurDistance && zcuReservation.ReservationTime < reservation.ReservationTime
                        || (isBackReservationOHT && zcuReservation.ReservationTime < reservation.ReservationTime))
                        && backReservationTime > zcuReservation.ReservationTime)
                    {
                        backReservationTime = zcuReservation.ReservationTime - 0.01;
                    }
                }
            }

            if (reservation.ReservationTime > backReservationTime)
                reservation.ReservationTime = backReservationTime;

            _reservations[stopNreset].Add(reservation);
            _reservations[stopNreset].Sort((x1, x2) => x1.ReservationTime.CompareTo(x2.ReservationTime));

            //foreach(ZCUReservation zcuReserv in _reservations[stopNreset])
            //{
            //    if (zcuReserv.OHT.Name == oht.Name)
            //        break;
            //    else if(zcuReserv.OHT.Name != oht.Name && zcuReserv.OHT.CurRailLineName == oht.CurRailLineName)
            //    {
            //        int reservOHTIdx = zcuReserv.OHT.CurRailLine.ListOHT.IndexOf(zcuReserv.OHT);
            //        int ohtIdx = oht.CurRailLine.ListOHT.IndexOf(oht);
            //    }
            //}

            if(oht.LstZCU.Count ==0)
            {
                if (oht.ReservationTime != Time.Zero)
                    ;
                oht.ReservationTime = reservation.ReservationTime;
                oht.ZcuResetPointName = resetPointName;
                oht.CurZcu = this;
                oht.LstZCU.Add(this);
            }
            else
            {
                oht.LstZCU.Add(this);
            }
        }

        public bool RemoveReservation(OHTNode oht)
        {
            foreach (List<ZCUReservation> reservationList in _reservations.Values)
            {
                foreach(ZCUReservation reservation in reservationList.ToList())
                {
                    if (reservation.OHT.Name == oht.Name)
                    {
                        reservationList.Remove(reservation);
                        return true;
                    }
                }
            }

            return false;
        }

        public void getReservationInfo(string ohtName, ref Time time, ref string resetPointName)
        {
            foreach(KeyValuePair<Tuple<string, string>, List<ZCUReservation>> kvpReservationList in _reservations)
            {
                foreach(ZCUReservation reservation in kvpReservationList.Value)
                {
                    if (reservation.OHT.Name == ohtName)
                    {
                        time = reservation.ReservationTime;
                        resetPointName = reservation.ResetPoint.Name;
                    }
                }
            }
        }

        public bool IsFirstReservation(OHTNode oht)
        {
            string stopPointName = string.Empty;
            string resetPointName = string.Empty;
            GetStopNResetPoint(oht, ref stopPointName, ref resetPointName);
            Tuple<string, string> stopNreset = new Tuple<string, string>(stopPointName, resetPointName);

            ZCUReservation ohtReservation = null;

            if (!Reservations.Keys.Contains(stopNreset))
                return false;

            foreach (ZCUReservation zcuReserv in Reservations[stopNreset])
            {
                if (zcuReserv.OHT.Name == oht.Name)
                {
                    ohtReservation = zcuReserv;
                    break;
                }
            }

            //null이면 예약도 안했으니 첫 예약이 아님.
            if (ohtReservation == null)
                return false;


            foreach (KeyValuePair<Tuple<string, string>, List<ZCUReservation>> kvpReservation in Reservations)
            {
                if (kvpReservation.Key.Item1 != stopPointName && kvpReservation.Key.Item2 == resetPointName)
                {
                    List<ZCUReservation> compReservationList = kvpReservation.Value;

                    if (compReservationList.Count > 0 && compReservationList[0].ReservationTime < ohtReservation.ReservationTime)
                    {
                        return false;   // Stop은 다른데 Reset은 같은 OHT가 먼저 예약했으면 false
                    }
                }
            }

            foreach (KeyValuePair<Tuple<string, string>, List<OHTNode>> kvpOHTs in _routeOHTs)
            {
                if (kvpOHTs.Key.Item2 == resetPointName
                    && kvpOHTs.Value.Count > 0)
                    return false; //Stop은 다른데 Reset은 같은 OHT가 이미 안에 있는 경우 false
            }

            return true;
        }


        public bool HasReservation(OHTNode oht)
        {
            string stopPointName = string.Empty;
            string resetPointName = string.Empty;
            GetStopNResetPoint(oht, ref stopPointName, ref resetPointName);
            Tuple<string, string> stopNreset = new Tuple<string, string>(stopPointName, resetPointName);

            ZCUReservation ohtReservation = null;

            if (!Reservations.Keys.Contains(stopNreset))
                return false;

            foreach (ZCUReservation zcuReserv in Reservations[stopNreset])
            {
                if (zcuReserv.OHT.Name == oht.Name)
                {
                    ohtReservation = zcuReserv;
                    break;
                }
            }

            if (ohtReservation != null)
                return true;
            else 
                return false;
        }

        public Time GetScheduleTimeOfFrontOHT(OHTNode oht)
        {
            Time scheduleTime = -1;
            string stopPointName = string.Empty;
            string resetPointName = string.Empty;
            GetStopNResetPoint(oht, ref stopPointName, ref resetPointName);
            Tuple<string, string> stopNreset = new Tuple<string, string>(stopPointName, resetPointName);

            foreach (KeyValuePair<Tuple<string, string>, List<OHTNode>> kvpOHTs in _routeOHTs)
            {
                if ( kvpOHTs.Value.Count > 0)
                {
                    OHTNode frontOHT = kvpOHTs.Value.Last();

                    if (frontOHT.CurRailLine.LstOHTPosData[frontOHT.CurRailLine.ListOHT.IndexOf(frontOHT)].Count > 0)
                    {
                        scheduleTime = frontOHT.CurRailLine.LstOHTPosData[frontOHT.CurRailLine.ListOHT.IndexOf(frontOHT)].Last()._endTime;
                        break;
                    }
                }
            }

            if(scheduleTime == -1)
            {
                foreach(KeyValuePair<Tuple<string,string>, List<ZCUReservation>> kvpReservation in Reservations)
                {
                    if(kvpReservation.Key.Item1 != stopPointName && kvpReservation.Key.Item2 == resetPointName)
                    {
                        List<ZCUReservation> compReservationList = kvpReservation.Value;
                        if (compReservationList.Count > 0)
                        {
                            OHTNode frontOHT = compReservationList[0].OHT;

                            if(oht.ReservationTime > frontOHT.ReservationTime && frontOHT.CurRailLine.LstOHTPosData[frontOHT.CurRailLine.ListOHT.IndexOf(frontOHT)].Count > 0)
                            {
                                Time tempEndTime = frontOHT.CurRailLine.LstOHTPosData[frontOHT.CurRailLine.ListOHT.IndexOf(frontOHT)].Last()._endTime;

                                scheduleTime = tempEndTime < scheduleTime ? scheduleTime : tempEndTime;
                            }
                        }
                    }
                }
            }

            return scheduleTime;
        }
    }
}
