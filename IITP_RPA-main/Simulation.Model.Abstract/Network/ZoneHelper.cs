using Mono.CSharp;
using Simulation.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class ZoneHelper
    {
        #region Variable
        private static ZoneHelper _instance;
        private List<Bay> _bayBumpingSequences;
        #endregion

        public static ZoneHelper Instance
        {
            get { return _instance; }
        }

        public double IntraBayWidth { get; set; }

        public double IntraBayHeight { get; set; }

        public int IntraBayBumpingOHTMaxValue { get; set; }
        public int InterBayBumpingOHTMaxValue { get; set; }

        public ZoneHelper()
        {
            _instance = this;

            _bayBumpingSequences = new List<Bay>();

            IntraBayBumpingOHTMaxValue = 3;
            InterBayBumpingOHTMaxValue = 6;
        }


        public void DoConnectTwoIntraBay(IntraBay leftIB, IntraBay rightIB)
        {
            string fabName = leftIB.Fab.Name;
            string name = $"{leftIB.Name}_{rightIB.Name}";

            // 연결 점 찾기
            RailPointNode rp_leftIn = leftIB.Points[17];
            RailPointNode rp_rightOut = rightIB.Points[8];

            RailPointNode rp_leftOut = leftIB.Points[22];
            RailPointNode rp_rightIn = rightIB.Points[3];

            // x,y 기준은 rightIB
            double x = rightIB.Position.X;
            double y = rightIB.Position.Y;

            // SlopeDistance, IntervalDistance
            double xDiff = rp_rightOut.PosVec3.X - rp_leftIn.PosVec3.X;
            double slopeDistance = xDiff * 4 / 22;
            double intervalDistance = xDiff * 14 / 22;

            // Load ZCUs
            ZCU zcu5_rightIB = ModelManager.Instance.Zcus[fabName + "_ZCU_" + rightIB.Name + "_5"];
            ZCU zcu2_rightIB = ModelManager.Instance.Zcus[fabName + "_ZCU_" + rightIB.Name + "_2"];
            ZCU zcu8_leftIB = ModelManager.Instance.Zcus[fabName + "_ZCU_" + leftIB.Name + "_8"];
            ZCU zcu11_leftIB = ModelManager.Instance.Zcus[fabName + "_ZCU_" + leftIB.Name + "_11"];

            // Create New Points
            RailPointNode rp_rightOut1 = ModelManager.Instance.AddRailPoint(new Vector3(-slopeDistance, IntraBayHeight * 5 / 6, 0) + new Vector3(x, y, 0), "rp_" + fabName + "_" + (rightIB.Name + "_rightOut_" + 1).ToString(), fabName, zcu5_rightIB.Name, ZCU_TYPE.RESET);
            RailPointNode rp_rightOut2 = ModelManager.Instance.AddRailPoint(new Vector3(-slopeDistance - intervalDistance, IntraBayHeight * 5 / 6, 0) + new Vector3(x, y, 0), "rp_" + fabName + "_" + (rightIB.Name + "_rightOut_" + 2).ToString(), fabName, zcu8_leftIB.Name, ZCU_TYPE.STOP);
            RailPointNode rp_leftOut1 = ModelManager.Instance.AddRailPoint(new Vector3(-slopeDistance - intervalDistance, IntraBayHeight / 6, 0) + new Vector3(x, y, 0), "rp_" + fabName + "_" + (leftIB.Name + "_leftOut_" + 1).ToString(), fabName, zcu11_leftIB.Name, ZCU_TYPE.RESET);
            RailPointNode rp_leftOut2 = ModelManager.Instance.AddRailPoint(new Vector3(-slopeDistance, IntraBayHeight / 6, 0) + new Vector3(x, y, 0), "rp_" + fabName + "_" + (leftIB.Name + "_leftOut_" + 2).ToString(), fabName, zcu2_rightIB.Name, ZCU_TYPE.STOP);

            // Create New Lines
            RailLineNode rl_rightOut1 = ModelManager.Instance.AddRailLine("rl_" + fabName + "_" + (rightIB.Name + "_rightOut_" + 1).ToString(), fabName, rightIB, rp_rightOut, rp_rightOut1, true, 1000);
            rightIB.ToLines.Add(rl_rightOut1);
            rl_rightOut1.DicBay.Add(rightIB.Name, ZONE_LINE_TYPE.RESET);
            RailLineNode rl_rightOut2 = ModelManager.Instance.AddRailLine("rl_" + fabName + "_" + (rightIB.Name + "_rightOut_" + 2).ToString(), fabName, null, rp_rightOut1, rp_rightOut2, false, 5000);
            RailLineNode rl_rightOut3 = ModelManager.Instance.AddRailLine("rl_" + fabName + "_" + (rightIB.Name + "_rightOut_" + 3).ToString(), fabName, leftIB, rp_rightOut2, rp_leftIn, true, 1000);
            leftIB.FromLines.Add(rl_rightOut3);
            rl_rightOut3.DicBay.Add(leftIB.Name, ZONE_LINE_TYPE.STOP);
            RailLineNode rl_leftOut1 = ModelManager.Instance.AddRailLine("rl_" + fabName + "_" + (leftIB.Name + "_leftOut_" + 1).ToString(), fabName, leftIB, rp_leftOut, rp_leftOut1, true, 1000);
            leftIB.ToLines.Add(rl_leftOut1);
            rl_leftOut1.DicBay.Add(leftIB.Name, ZONE_LINE_TYPE.RESET);
            RailLineNode rl_leftOut2 = ModelManager.Instance.AddRailLine("rl_" + fabName + "_" + (leftIB.Name + "_leftOut_" + 2).ToString(), fabName, null, rp_leftOut1, rp_leftOut2, false, 5000);
            RailLineNode rl_leftOut3 = ModelManager.Instance.AddRailLine("rl_" + fabName + "_" + (leftIB.Name + "_leftOut_" + 3).ToString(), fabName, rightIB, rp_leftOut2, rp_rightIn, true, 1000);
            rightIB.FromLines.Add(rl_leftOut3);
            rl_leftOut3.DicBay.Add(rightIB.Name, ZONE_LINE_TYPE.STOP);
        }

        public void DoConnectMultiIntraBay(List<IntraBay> LstIB)
        {
            for (int baySequence = 0; baySequence < LstIB.Count - 1; baySequence++)
            {
                IntraBay leftIB = LstIB[baySequence];
                IntraBay rightIB = LstIB[baySequence + 1];

                DoConnectTwoIntraBay(leftIB, rightIB);
            }
        }

        public void SearchAlgorithm(Zone zone)
        {
            // Bay의 FromLine에서 ToNode 리스트 조회
            List<RailPointNode> fromPoints = zone.FromLines.Select(line => line.ToNode).ToList();
            // Bay의 ToLine에서 FromNode 리스트 조회
            List<RailPointNode> endPoints = zone.ToLines.Select(line => line.FromNode).ToList();

            // *** DFS 알고리즘 사용 ***
            // fromPoints 혹은 endPoints 즉 bay의 테두리에 포함되는 점 중 하나를 시작점으로 잡는다. 
            // fromPoints를 시작점으로 잡는다.
            foreach(RailPointNode currentPoint in fromPoints)
            {
                List<RailPointNode> visitedPoints = new List<RailPointNode>();

                Search(currentPoint, visitedPoints, zone.ToLines);

                List<RailLineNode> innerLines = new List<RailLineNode>();

                foreach (RailPointNode visitedPoint in visitedPoints)
                {
                    List<RailLineNode> toLines = visitedPoint.ToLines.Where(line => !zone.FromLines.Contains(line) && !zone.ToLines.Contains(line)).ToList();

                    innerLines.AddRange(toLines);
                }

                innerLines = innerLines.Distinct().ToList();

                foreach (RailLineNode innerLine in innerLines)
                {
                    // Point 추가
                    if (!zone.Points.Contains(innerLine.FromNode))
                    { zone.Points.Add(innerLine.FromNode); }

                    if (!zone.Points.Contains(innerLine.ToNode))
                    { zone.Points.Add(innerLine.ToNode); }

                    // Line 추가
                    if (!zone.Lines.Contains(innerLine))
                    {
                        zone.Lines.Add(innerLine);

                        if(zone.ZONE_TYPE == ZONE_TYPE.BAY)
                            innerLine.Bay = zone as Bay;
                    }


                    // Eqp 추가
                    foreach (RailPortNode port in innerLine.DicRailPort)
                    {
                        // Port 추가
                        if(!zone.Ports.Contains(port))
                            zone.Ports.Add(port);

                        if (zone.ZONE_TYPE == ZONE_TYPE.BAY && ((Bay)zone).Reticle == true && !ModelManager.Instance.DicReticlePort.ContainsKey(port.Name))
                            ModelManager.Instance.DicReticlePort.Add(port.Name, port);

                        if (port.ConnectedEqp is ProcessEqpNode && !zone.ProcessEqps.Contains(port.ConnectedEqp))
                            zone.ProcessEqps.Add(port.ConnectedEqp as ProcessEqpNode);
                    }
                }
            }
        }

        public void SearchBay(ReticleZone zone)
        {
            // Bay의 FromLine에서 ToNode 리스트 조회
            List<RailPointNode> fromPoints = zone.FromLines.Select(line => line.ToNode).ToList();
            string reticleBays = string.Empty;

            foreach ( RailLineNode line in zone.FromLines)
            {
                if(line.Bay != null && !zone.Bays.Contains(line.Bay))
                {
                    zone.Bays.Add(line.Bay);
                    line.Bay.Reticle = true;
                    reticleBays = reticleBays + line.Bay.Name + ", ";
                }
            }

            // Bay의 ToLine에서 FromNode 리스트 조회
            List<RailPointNode> endPoints = zone.ToLines.Select(line => line.FromNode).ToList();

            foreach (RailLineNode line in zone.ToLines)
            {
                if (line.Bay != null && !zone.Bays.Contains(line.Bay))
                {
                    zone.Bays.Add(line.Bay);
                    line.Bay.Reticle = true;
                    reticleBays = reticleBays + line.Bay.Name + ", ";
                }
            }
        }

        public void InitializeBumpingPort(Bay bay)
        {
            if (bay.Points.Count == 0 || bay.Ports.Count == 0)
            {
                foreach (RailLineNode toLine in bay.ToLines)
                    toLine.DicBay.Remove(bay.Name);

                foreach (RailLineNode fromLine in bay.FromLines)
                    fromLine.DicBay.Remove(bay.Name);

                ModelManager.Instance.Bays.Remove(bay.Name);
                bay.Fab.LstBay.Remove(bay.Name);
            }
            else
                SetBumpingPorts(bay);
        }

        public void Search(RailPointNode currentPoint,  List<RailPointNode> visitedPoints, List<RailLineNode> zoneToLines)
        {
            visitedPoints.Add(currentPoint);

            List<RailPointNode> nextNodes = currentPoint.ToLines.Where(line => !zoneToLines.Contains(line)).Select(line => line.ToNode).ToList();

            while(nextNodes.Any())
            {
                RailPointNode nextNode = nextNodes.First();

                nextNodes.Remove(nextNode);

                if(!visitedPoints.Contains(nextNode))
                {
                    Search(nextNode, visitedPoints, zoneToLines); 
                }
            }
        }

        #region Bumping Logic
        /// <summary>
        /// bay 내 Port 중 BumpingPort를 2개 선정한다.
        /// 선정 기준은 bay 내 ToLine 근처에 있는 Port들을 2개의 쌍으로 모두 고려한 다음
        /// 거리가 가장 먼 쌍을 BumpingPorts로 선정
        /// </summary>
        /// <param name="bay"></param>
        public void SetBumpingPorts(Bay bay)
        {
            // Bay 내 Port 중 거리가 가장 먼 port 쌍을 Bumping Port로 선정
            double maxDistance = 0;
            RailPortNode bumpingPort1 = null;
            RailPortNode bumpingPort2 = null;

            for(int index1 = 0; index1 < bay.Ports.Count - 1; index1++)
            {
                RailPortNode candidatePort1 = bay.Ports[index1];

                if (candidatePort1.Line.Length < 1400)
                    continue;

                for(int index2 = index1 + 1; index2 < bay.Ports.Count; index2++)
                {
                    RailPortNode candidatePort2 = bay.Ports[index2];

                    if (candidatePort2.Line.Length < 1400)
                        continue;

                    double crtDistance = Vector3.Distance(candidatePort1.PosVec3, candidatePort2.PosVec3);

                    if(crtDistance >= maxDistance)
                    {
                        maxDistance = crtDistance;

                        bumpingPort1 = candidatePort1;
                        bumpingPort2 = candidatePort2;
                    }
                }
            }

            if(bumpingPort1 != null)
                bay.BumpingPorts.Add(bumpingPort1);
            if(bumpingPort2 != null)
                bay.BumpingPorts.Add(bumpingPort2);
        }

        /// <summary>
        /// bay 내 BumpingPort를 반환한다. oht의 목적지와 같은 BumpingPort는 선정 대상에서 제외한다.
        /// </summary>
        /// <param name="oht"></param>
        /// <param name="bay"></param>
        /// <returns></returns>
        public RailPortNode GetBumpingPort(OHTNode oht, Bay bay)
        {
            RailPortNode otherLineBumpingPort = null;
            RailPortNode sameLineBumpingPort = null;

            if (bay == null)
                ;
            foreach(RailPortNode candidatePort in bay.BumpingPorts)
            {
                if(oht.CurRailLine.ID != candidatePort.Line.ID ) // 다른 라인
                {
                    otherLineBumpingPort = candidatePort;
                    break;
                }
                else if ( oht.CurDistance != candidatePort.Distance) // 라인은 같으나 다른 위치
                {
                    sameLineBumpingPort = candidatePort;
                }
            }

            if (otherLineBumpingPort != null)
                return otherLineBumpingPort;
            else if (sameLineBumpingPort != null)
                return sameLineBumpingPort;
            else
                return null;
        }

        /// <summary>
        /// Bay Bumping OHT가 Max일 때 밀어낼 이웃 bay 탐색
        /// </summary>
        public void IntializeBumpingRotation()
        {
            List<Bay> totalBays = ModelManager.Instance.Bays.Values.ToList();

            // Bay Position 할당
            for (int index = 0; index < totalBays.Count; index++)
            {
                Bay bay = totalBays[index];

                if (bay.Points.Count == 0)
                    break;

                double avgX = bay.Points.Average(point => point.PosVec3.X);
                double avgY = bay.Points.Average(point => point.PosVec3.Y);

                bay.SetPosition(new Vector3(avgX, avgY, 0));

                if(bay.Fab.Name == "M14B")
                {
                    string bayID = bay.Name.Split('_')[1];

                    switch(bayID)
                    {
                        case "A1":
                            AddNeighborBay(bay, "G1");
                            AddNeighborBay(bay, "A2");
                            break;
                        case "A2":
                            AddNeighborBay(bay, "A1");
                            AddNeighborBay(bay, "A3");
                            break;
                        case "A3":
                            AddNeighborBay(bay, "A2");
                            AddNeighborBay(bay, "A4");
                            break;
                        case "A4":
                            AddNeighborBay(bay, "A3");
                            AddNeighborBay(bay, "A5");
                            AddNeighborBay(bay, "G2-1");
                            break;
                        case "A5":
                            AddNeighborBay(bay, "A4");
                            AddNeighborBay(bay, "A6");
                            AddNeighborBay(bay, "G2-1");
                            break;
                        case "A6":
                            AddNeighborBay(bay, "A5");
                            AddNeighborBay(bay, "A7");
                            AddNeighborBay(bay, "G2-1");
                            break;
                        case "A7":
                            AddNeighborBay(bay, "A6");
                            AddNeighborBay(bay, "A8");
                            break;
                        case "A8":
                            AddNeighborBay(bay, "A7");
                            AddNeighborBay(bay, "B9");
                            break;
                        case "A9":
                            AddNeighborBay(bay, "A8");
                            AddNeighborBay(bay, "B0");
                            break;
                        case "B0":
                            AddNeighborBay(bay, "A9");
                            AddNeighborBay(bay, "B1");
                            break;
                        case "B1":
                            AddNeighborBay(bay, "B0");
                            AddNeighborBay(bay, "B2");
                            break;
                        case "B2":
                            AddNeighborBay(bay, "B1");
                            AddNeighborBay(bay, "B3");
                            break;
                        case "B3":
                            AddNeighborBay(bay, "B2");
                            AddNeighborBay(bay, "B4");
                            break;
                        case "B4":
                            AddNeighborBay(bay, "B3");
                            AddNeighborBay(bay, "B5");
                            break;
                        case "B5":
                            AddNeighborBay(bay, "B4");
                            AddNeighborBay(bay, "B6");
                            break;
                        case "B6":
                            AddNeighborBay(bay, "B5");
                            AddNeighborBay(bay, "B7");
                            break;
                        case "B7":
                            AddNeighborBay(bay, "B6");
                            AddNeighborBay(bay, "B8");
                            AddNeighborBay(bay, "G6-2");
                            break;
                        case "B8":
                            AddNeighborBay(bay, "B7");
                            AddNeighborBay(bay, "B9");
                            AddNeighborBay(bay, "G6-2");
                            break;
                        case "B9":
                            AddNeighborBay(bay, "B8");
                            AddNeighborBay(bay, "C0");
                            break;
                        case "C0":
                            AddNeighborBay(bay, "B9");
                            AddNeighborBay(bay, "C1");
                            break;
                        case "C1":
                            AddNeighborBay(bay, "C0");
                            AddNeighborBay(bay, "C2");
                            break;
                        case "C2":
                            AddNeighborBay(bay, "C1");
                            AddNeighborBay(bay, "C3");
                            break;
                        case "C3":
                            AddNeighborBay(bay, "C2");
                            AddNeighborBay(bay, "C4");
                            AddNeighborBay(bay, "G6-4");
                            break;
                        case "C4":
                            AddNeighborBay(bay, "C3");
                            AddNeighborBay(bay, "C5");
                            AddNeighborBay(bay, "G6-4");
                            break;
                        case "C5":
                            AddNeighborBay(bay, "C4");
                            AddNeighborBay(bay, "C6");
                            break;
                        case "C6":
                            AddNeighborBay(bay, "C5");
                            AddNeighborBay(bay, "C7");
                            AddNeighborBay(bay, "G6-7");
                            break;
                        case "C7":
                            AddNeighborBay(bay, "C6");
                            AddNeighborBay(bay, "C8");
                            AddNeighborBay(bay, "G6-7");
                            break;
                        case "C8":
                            AddNeighborBay(bay, "C7");
                            AddNeighborBay(bay, "C9");
                            break;
                        case "C9":
                            AddNeighborBay(bay, "C8");
                            AddNeighborBay(bay, "D0");
                            break;
                        case "D0":
                            AddNeighborBay(bay, "C9");
                            AddNeighborBay(bay, "D1");
                            break;
                        case "D1":
                            AddNeighborBay(bay, "D0");
                            AddNeighborBay(bay, "D2");
                            break;
                        case "D2":
                            AddNeighborBay(bay, "D1");
                            AddNeighborBay(bay, "D3");
                            break;
                        case "D3":
                            AddNeighborBay(bay, "D2");
                            AddNeighborBay(bay, "D3-1");
                            break;
                        case "D3-1":
                            AddNeighborBay(bay, "D3");
                            AddNeighborBay(bay, "D4");
                            break;
                        case "D4":
                            AddNeighborBay(bay, "D3-1");
                            AddNeighborBay(bay, "D5");
                            break;
                        case "D5":
                            AddNeighborBay(bay, "D4");
                            AddNeighborBay(bay, "D6");
                            break;
                        case "D6":
                            AddNeighborBay(bay, "D5");
                            AddNeighborBay(bay, "D7");
                            break;
                        case "D7":
                            AddNeighborBay(bay, "D6");
                            AddNeighborBay(bay, "D8");
                            break;
                        case "D8":
                            AddNeighborBay(bay, "D7");
                            AddNeighborBay(bay, "D9");
                            break;
                        case "D9":
                            AddNeighborBay(bay, "D8");
                            AddNeighborBay(bay, "E0");
                            break;
                        case "E0":
                            AddNeighborBay(bay, "D9");
                            AddNeighborBay(bay, "E1");
                            break;
                        case "E1":
                            AddNeighborBay(bay, "E0");
                            AddNeighborBay(bay, "E2");
                            break;
                        case "E2":
                            AddNeighborBay(bay, "E1");
                            AddNeighborBay(bay, "E3");
                            break;
                        case "E3":
                            AddNeighborBay(bay, "E2");
                            AddNeighborBay(bay, "E4");
                            break;
                        case "E4":
                            AddNeighborBay(bay, "E3");
                            AddNeighborBay(bay, "E5");
                            break;
                        case "E5":
                            AddNeighborBay(bay, "E4");
                            AddNeighborBay(bay, "E6");
                            break;
                        case "E6":
                            AddNeighborBay(bay, "E5");
                            AddNeighborBay(bay, "E7");
                            break;
                        case "E7":
                            AddNeighborBay(bay, "E6");
                            AddNeighborBay(bay, "E8");
                            break;
                        case "E8":
                            AddNeighborBay(bay, "E7");
                            AddNeighborBay(bay, "E9");
                            break;
                        case "E9":
                            AddNeighborBay(bay, "E8");
                            AddNeighborBay(bay, "F0");
                            break;
                        case "F0":
                            AddNeighborBay(bay, "E9");
                            AddNeighborBay(bay, "F1");
                            break;
                        case "F1":
                            AddNeighborBay(bay, "F0");
                            AddNeighborBay(bay, "F2");
                            break;
                        case "F2":
                            AddNeighborBay(bay, "F1");
                            AddNeighborBay(bay, "F3");
                            break;
                        case "F3":
                            AddNeighborBay(bay, "F2");
                            AddNeighborBay(bay, "F4");
                            break;
                        case "F4":
                            AddNeighborBay(bay, "F3");
                            AddNeighborBay(bay, "F5");
                            break;
                        case "F5":
                            AddNeighborBay(bay, "F4");
                            AddNeighborBay(bay, "F6");
                            break;
                        case "F6":
                            AddNeighborBay(bay, "F5");
                            AddNeighborBay(bay, "F7");
                            break;
                        case "F7":
                            AddNeighborBay(bay, "F6");
                            AddNeighborBay(bay, "F8");
                            break;
                        case "F8":
                            AddNeighborBay(bay, "F7");
                            AddNeighborBay(bay, "F9");
                            break;
                        case "F9":
                            AddNeighborBay(bay, "F8");
                            AddNeighborBay(bay, "G0");
                            break;
                        case "G0":
                            AddNeighborBay(bay, "F9");
                            AddNeighborBay(bay, "G1");
                            break;
                        case "G1":
                            AddNeighborBay(bay, "G0");
                            AddNeighborBay(bay, "A1");
                            break;
                        case "G2-1":
                            AddNeighborBay(bay, "A5");
                            AddNeighborBay(bay, "G2-3");
                            break;
                        case "G2-2":
                            AddNeighborBay(bay, "G2-1");
                            AddNeighborBay(bay, "G2-3");
                            break;
                        case "G2-3":
                            AddNeighborBay(bay, "G2-1");
                            AddNeighborBay(bay, "G2-2");
                            break;
                        case "G5-1":
                            AddNeighborBay(bay, "E6");
                            AddNeighborBay(bay, "G5-3");
                            break;
                        case "G5-2":
                            AddNeighborBay(bay, "G5-1");
                            AddNeighborBay(bay, "G5-3");
                            break;
                        case "G5-3":
                            AddNeighborBay(bay, "G5-2");
                            AddNeighborBay(bay, "G5-4");
                            break;
                        case "G5-4":
                            AddNeighborBay(bay, "G5-3");
                            AddNeighborBay(bay, "G5-5");
                            break;
                        case "G5-5":
                            AddNeighborBay(bay, "G5-4");
                            AddNeighborBay(bay, "G5-6");
                            break;
                        case "G5-6":
                            AddNeighborBay(bay, "G5-4");
                            AddNeighborBay(bay, "D5");
                            break;
                        case "G6-1":
                            AddNeighborBay(bay, "B7");
                            AddNeighborBay(bay, "G6-3");
                            break;
                        case "G6-2":
                            AddNeighborBay(bay, "G6-1");
                            AddNeighborBay(bay, "G6-3");
                            break;
                        case "G6-3":
                            AddNeighborBay(bay, "G6-2");
                            AddNeighborBay(bay, "G6-4");
                            break;
                        case "G6-4":
                            AddNeighborBay(bay, "G6-3");
                            AddNeighborBay(bay, "G6-5");
                            break;
                        case "G6-5":
                            AddNeighborBay(bay, "G6-4");
                            AddNeighborBay(bay, "G6-7");
                            break;
                        case "G6-7":
                            AddNeighborBay(bay, "G6-4");
                            AddNeighborBay(bay, "C7");
                            break;
                    }
                }
                else
                {
                    List<RailLineNode> visitedLines = new List<RailLineNode>();
                    foreach (RailLineNode line in bay.ToLines)
                    {
                        SearchNeighborBay(line, visitedLines, bay);

                        foreach (Bay neighborBay in bay.NeighborBay.ToArray())
                        {
                            if (neighborBay.BumpingPorts.Count == 0)
                                bay.NeighborBay.Remove(neighborBay);
                        }
                    }
                }
            }
        }

        private void AddNeighborBay(Bay bay, string firstBayName)
        {
            string bayName = bay.Fab.Name + "_" + firstBayName;
            Bay neighborBay = ModelManager.Instance.Bays[bayName];
            bay.NeighborBay.Add(neighborBay);
        }

        private void SearchNeighborBay(RailLineNode line, List<RailLineNode> visitedLines, Bay bay)
        {
            visitedLines.Add(line);

            if (line.Bay == null || line.Bay.Name == bay.Name)
            {
                foreach (RailLineNode toLine in line.ToNode.ToLines)
                {
                    if (!bay.NeighborBay.Contains(toLine.Bay) && !visitedLines.Contains(toLine))
                        SearchNeighborBay(toLine, visitedLines, bay);
                }
            }
            else if (line.Bay != null && line.Bay.Name != bay.Name && !bay.NeighborBay.Contains(line.Bay))
                bay.NeighborBay.Add(line.Bay);
        }

        /// <summary>
        /// curBay의 BumpingOHT가 Max라면 Bay Bumping Sequence를 통해 
        /// 그 다음 가야 할 Bay를 반환한다.
        /// </summary>
        /// <param name="curBay"></param>
        /// <returns></returns>
        public Bay FindNextBumpingBay(Bay curBay, bool isReticleOHT)
        {
            List<Bay> bays = null;

            if (curBay == null)
                ;

            if (isReticleOHT)
            {
                bays = curBay.NeighborBay.FindAll(b => b.Reticle == isReticleOHT);
            }
            else
                bays = curBay.NeighborBay;

            bays = bays.FindAll(b => b.BumpingOHTs.Count == bays.Min(mb => mb.BumpingOHTs.Count));
            Random random = new Random();
            int no = random.Next(0, bays.Count);

            if (bays == null || bays.Count == 0)
                return ModelManager.Instance.GetNearestBay(curBay.Fab.Name, curBay.Position, isReticleOHT);
            else
                return bays[no];
        }
        #endregion
    }
}