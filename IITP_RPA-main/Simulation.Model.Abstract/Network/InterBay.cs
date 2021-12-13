using System;
using System.Collections.Generic;
using Simulation.Geometry;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Engine;

namespace Simulation.Model.Abstract
{
    public class InterBay : Bay
    {

        public InterBay(uint id, string name, Fab fab, bool reticle)
            : base(id, name, fab, reticle)
        {
            BayType = BAY_TYPE.INTERBAY;
        }

        public InterBay(uint id, Vector3 pos, Vector3 size, string name, Fab fab, bool reticle)
            : base(id, pos, size, name, fab, reticle)
        {
            BayType = BAY_TYPE.INTERBAY;
            MakeRail(pos.X, pos.Y, size.X, size.Y);
        }

        public InterBay(uint id, Vector3 pos, Vector3 size, string name, Fab fab, bool reticle, List<IntraBay> topIntraBays, List<IntraBay> bottomIntraBays)
            : base(id, pos, size, name, fab, reticle)
        {
            BayType = BAY_TYPE.INTERBAY;
            //            MakeRail(pos.X, pos.Y, size.X, size.Y, topIntraBays, bottomIntraBays);
            MakeCurvedRail(pos.X, pos.Y, size.X, size.Y, topIntraBays, bottomIntraBays);
        }

        public void MakeCommit(Engine.Time startTime, Engine.Time intervalTime, int entityCount, float width)
        {
            Vector3 position = new Vector3(-width, pos.Y + size.Y / 2, 0);
            RailPortNode rs = GetCommitStationNode(position);
            CommitInterval cm = ModelManager.Instance.AddCommitInterval(position, "commit", Fab.Name, "Product_1", startTime, intervalTime, entityCount, rs);
        }

        public void MakeComplete(float width)
        {
            Vector3 position = new Vector3(pos.X + size.X + width, pos.Y + size.Y / 2, 0);
            RailPortNode rs = GetCompleteStationNode(position);
            CompleteNode complete = ModelManager.Instance.AddComplete(position, "complete", fab.Name, rs);
        }

        private RailPortNode GetCommitStationNode(Vector3 commitStationPosition)
        {
            RailPortNode nearestStationNode = null;
            double shortestDistance = 0;
            foreach (RailPortNode port in ports)
            {
                if (nearestStationNode == null)
                {
                    nearestStationNode = port;
                    shortestDistance = Vector3.Distance(nearestStationNode.PosVec3, commitStationPosition);
                }
                double distance = Vector3.Distance(port.PosVec3, commitStationPosition);

                if (distance < shortestDistance)
                {
                    nearestStationNode = port;
                    shortestDistance = distance;
                }
            }

            return nearestStationNode;
        }

        private RailPortNode GetCompleteStationNode(Vector3 commitStationPosition)
        {
            RailPortNode nearestStationNode = null;

            double shortestDistance = 0;
            foreach (RailPortNode port in ports)
            {
                if (nearestStationNode == null)
                {
                    nearestStationNode = port;
                    shortestDistance = Vector3.Distance(nearestStationNode.PosVec3, commitStationPosition);
                }
                double distance = Vector3.Distance(nearestStationNode.PosVec3, commitStationPosition);

                if (distance < shortestDistance)
                {
                    nearestStationNode = port;
                    shortestDistance = distance;
                }
            }

            return nearestStationNode;
        }

        /// <summary>
        /// 2차선 interBay rail 생성
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        private void MakeRail(double x, double y, double width, double length)
        {
            RailPointNode rp_1 = ModelManager.Instance.AddRailPoint(new Vector3(0, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_2 = ModelManager.Instance.AddRailPoint(new Vector3(width / 3, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 2).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_3 = ModelManager.Instance.AddRailPoint(new Vector3(width * 2 / 3, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 3).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_4 = ModelManager.Instance.AddRailPoint(new Vector3(width, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 4).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_5 = ModelManager.Instance.AddRailPoint(new Vector3(width / 10, length / 3, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 5).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_6 = ModelManager.Instance.AddRailPoint(new Vector3(width / 3, length / 3, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 6).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_7 = ModelManager.Instance.AddRailPoint(new Vector3(width * 2 / 3, length / 3, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 7).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_8 = ModelManager.Instance.AddRailPoint(new Vector3(width * 9 / 10, length / 3, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 8).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_9 = ModelManager.Instance.AddRailPoint(new Vector3(width / 10, length * 2 / 3, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 9).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_10 = ModelManager.Instance.AddRailPoint(new Vector3(width / 3, length * 2 / 3, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 10).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_11 = ModelManager.Instance.AddRailPoint(new Vector3(width * 2 / 3, length * 2 / 3, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 11).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_12 = ModelManager.Instance.AddRailPoint(new Vector3(width * 9 / 10, length * 2 / 3, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 12).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_13 = ModelManager.Instance.AddRailPoint(new Vector3(0, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 13).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_14 = ModelManager.Instance.AddRailPoint(new Vector3(width / 3, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 14).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_15 = ModelManager.Instance.AddRailPoint(new Vector3(width * 2 / 3, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 15).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            RailPointNode rp_16 = ModelManager.Instance.AddRailPoint(new Vector3(width, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 16).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);

            points.Add(rp_1);
            points.Add(rp_2);
            points.Add(rp_3);
            points.Add(rp_4);
            points.Add(rp_5);
            points.Add(rp_6);
            points.Add(rp_7);
            points.Add(rp_8);
            points.Add(rp_9);
            points.Add(rp_10);
            points.Add(rp_11);
            points.Add(rp_12);
            points.Add(rp_13);
            points.Add(rp_14);
            points.Add(rp_15);
            points.Add(rp_16);

            //-------------------------
            RailLineNode rl_1 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 1).ToString(), fab.Name, this, rp_2, rp_1);
            RailLineNode rl_2 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 2).ToString(), fab.Name, this, rp_3, rp_2);
            RailLineNode rl_3 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 3).ToString(), fab.Name, this, rp_4, rp_3);
            RailLineNode rl_4 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 4).ToString(), fab.Name, this, rp_6, rp_5);
            RailLineNode rl_5 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 5).ToString(), fab.Name, this, rp_7, rp_6);
            RailLineNode rl_6 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 6).ToString(), fab.Name, this, rp_8, rp_7);
            RailLineNode rl_7 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 7).ToString(), fab.Name, this, rp_9, rp_10);
            RailLineNode rl_8 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 8).ToString(), fab.Name, this, rp_10, rp_11);
            RailLineNode rl_9 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 9).ToString(), fab.Name, this, rp_11, rp_12);
            RailLineNode rl_10 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 10).ToString(), fab.Name, this, rp_13, rp_14);
            RailLineNode rl_11 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 11).ToString(), fab.Name, this, rp_14, rp_15);
            RailLineNode rl_12 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 12).ToString(), fab.Name, this, rp_15, rp_16);
            RailLineNode rl_13 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 13).ToString(), fab.Name, this, rp_1, rp_13);
            RailLineNode rl_14 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 14).ToString(), fab.Name, this, rp_2, rp_6);
            RailLineNode rl_15 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 15).ToString(), fab.Name, this, rp_7, rp_3);
            RailLineNode rl_16 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 16).ToString(), fab.Name, this, rp_16, rp_4);
            RailLineNode rl_17 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 17).ToString(), fab.Name, this, rp_5, rp_9);
            RailLineNode rl_18 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 18).ToString(), fab.Name, this, rp_12, rp_8);
            RailLineNode rl_19 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 19).ToString(), fab.Name, this, rp_10, rp_14);
            RailLineNode rl_20 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 20).ToString(), fab.Name, this, rp_15, rp_11);

            //버퍼 올라갈 링크 리스트
            lines.Add(rl_1);
            lines.Add(rl_2);
            lines.Add(rl_3);
            lines.Add(rl_4);
            lines.Add(rl_5);
            lines.Add(rl_6);
            lines.Add(rl_7);
            lines.Add(rl_8);
            lines.Add(rl_9);
            lines.Add(rl_10);
            lines.Add(rl_11);
            lines.Add(rl_12);
            lines.Add(rl_13);
            lines.Add(rl_14);
            lines.Add(rl_15);
            lines.Add(rl_16);
            lines.Add(rl_17);
            lines.Add(rl_18);
            lines.Add(rl_19);
            lines.Add(rl_20);
        }

        /// <summary>
        /// 2차선 interBay rail 생성
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        private void MakeCurvedRail(double x, double y, double width, double length, List<IntraBay> topIntraBays, List<IntraBay> bottomIntraBays)
        {
            double slopeDistance = 6;
            List<RailPointNode> overPoints = new List<RailPointNode>();
            List<RailPointNode> underPoints = new List<RailPointNode>();

            RailPointNode rp_1 = ModelManager.Instance.AddRailPoint(new Vector3(0, slopeDistance / 2, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            points.Add(rp_1);
            RailPointNode rp_2 = ModelManager.Instance.AddRailPoint(new Vector3(0, length - slopeDistance / 2, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            points.Add(rp_2);
            RailPointNode rp_3 = ModelManager.Instance.AddRailPoint(new Vector3(slopeDistance / 2, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            points.Add(rp_3);
            RailPointNode rp_4 = ModelManager.Instance.AddRailPoint(new Vector3(width - slopeDistance / 2, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            points.Add(rp_4);
            RailPointNode rp_5 = ModelManager.Instance.AddRailPoint(new Vector3(width, length - slopeDistance / 2, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            points.Add(rp_5);
            RailPointNode rp_6 = ModelManager.Instance.AddRailPoint(new Vector3(width, slopeDistance / 2, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            points.Add(rp_6);
            RailPointNode rp_7 = ModelManager.Instance.AddRailPoint(new Vector3(width - slopeDistance / 2, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            points.Add(rp_7);
            RailPointNode rp_8 = ModelManager.Instance.AddRailPoint(new Vector3(slopeDistance / 2, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, string.Empty, ZCU_TYPE.NON);
            points.Add(rp_8);

            overPoints.Add(rp_3);
            overPoints.Add(rp_4);
            underPoints.Add(rp_8);
            underPoints.Add(rp_7);

            //-------------------------
            RailLineNode rl_1 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_1, rp_2, false, 5000);
            lines.Add(rl_1);
            RailLineNode rl_2 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_2, rp_3, true, 1000);
            lines.Add(rl_2);
            //            RailLineNode rl_3 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_3, rp_4);
            //            lines.Add(rl_3);
            RailLineNode rl_4 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_4, rp_5, true, 1000);
            lines.Add(rl_4);
            RailLineNode rl_5 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_5, rp_6, false, 5000);
            lines.Add(rl_5);
            RailLineNode rl_6 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_6, rp_7, true, 1000);
            lines.Add(rl_6);
            //            RailLineNode rl_7 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_7, rp_8);
            //            lines.Add(rl_7);
            RailLineNode rl_8 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_8, rp_1, true, 1000);
            lines.Add(rl_8);

            slopeDistance = 3.5;

            int zcuNo = 0;
            //상단 IntraBay에 연결
            if (topIntraBays != null)
            {
                foreach (IntraBay intraBay in topIntraBays)
                {
                    RailPointNode leftBottomPointByZcu = intraBay.GetLeftBottomPointByZCU();
                    RailPointNode rightBottomPointByZcu = intraBay.GetRightBottomPointByZCU();
                    RailPointNode leftBottomPoint = intraBay.GetLeftBottomPoint();
                    RailPointNode rightBottomPoint = intraBay.GetRightBottomPoint();

                    //왼쪽 아래
                    RailPointNode rp = ModelManager.Instance.AddRailPoint(new Vector3(leftBottomPoint.PosVec3.X - slopeDistance, rp_4.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, leftBottomPointByZcu.Zcu.Name, ZCU_TYPE.STOP);
                    points.Add(rp);
                    overPoints.Add(rp);

                    RailLineNode rl = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp, leftBottomPoint, true, 1000);
                    lines.Add(rl);

                    this.toLines.Add(rl);
                    rl.DicBay.Add(this.Name, ZONE_LINE_TYPE.RESET);
                    rl.Bay = this;
                    intraBay.FromLines.Add(leftBottomPoint.ToLines[0]);
                    leftBottomPoint.ToLines[0].DicBay.Add(intraBay.Name, ZONE_LINE_TYPE.STOP);
                    leftBottomPoint.ToLines[0].Bay = intraBay;

                    //분기 라인을 표현하기 위한 포인트 
                    rp = ModelManager.Instance.AddRailPoint(new Vector3(leftBottomPoint.PosVec3.X, rp_4.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, leftBottomPointByZcu.Zcu.Name, ZCU_TYPE.RESET);
                    points.Add(rp);
                    overPoints.Add(rp);
                    zcuNo++;

                    //오른쪽 아래
                    rp = ModelManager.Instance.AddRailPoint(new Vector3(rightBottomPoint.PosVec3.X + slopeDistance, rp_4.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, rightBottomPointByZcu.Zcu.Name, ZCU_TYPE.RESET);
                    points.Add(rp);
                    overPoints.Add(rp);

                    rl = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rightBottomPoint, rp, true, 1000);
                    lines.Add(rl);

                    this.fromLines.Add(rl);
                    rl.DicBay.Add(this.Name, ZONE_LINE_TYPE.STOP);
                    rl.Bay = this;
                    intraBay.ToLines.Add(rightBottomPoint.FromLines[0]);
                    rightBottomPoint.FromLines[0].DicBay.Add(intraBay.Name, ZONE_LINE_TYPE.RESET);
                    rightBottomPoint.FromLines[0].Bay = intraBay;

                    //합류 라인을 표현하기 위한 포인트
                    rp = ModelManager.Instance.AddRailPoint(new Vector3(rightBottomPoint.PosVec3.X, rp_4.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, rightBottomPointByZcu.Zcu.Name, ZCU_TYPE.STOP);
                    points.Add(rp);
                    overPoints.Add(rp);
                    zcuNo++;

                }
            }

            //하단 Intrabay에 연결
            if (bottomIntraBays != null)
            {
                foreach (IntraBay intraBay in bottomIntraBays)
                {
                    RailPointNode topLeftPointByZcu = intraBay.GetTopLeftPointByZCU();
                    RailPointNode topRightPointByZcu = intraBay.GetTopRightPointByZCU();
                    RailPointNode topLeftPoint = intraBay.GetTopLeftPoint();
                    RailPointNode topRightPoint = intraBay.GetTopRightPoint();

                    // 왼쪽
                    RailPointNode rp = ModelManager.Instance.AddRailPoint(new Vector3(topLeftPoint.PosVec3.X - slopeDistance, rp_8.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, topLeftPointByZcu.Zcu.Name, ZCU_TYPE.RESET);
                    points.Add(rp);
                    underPoints.Add(rp);

                    RailLineNode rl = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, topLeftPoint, rp, true, 1000);
                    lines.Add(rl);

                    this.fromLines.Add(rl);
                    rl.DicBay.Add(this.Name, ZONE_LINE_TYPE.STOP);
                    rl.Bay = this;
                    intraBay.ToLines.Add(topLeftPoint.FromLines[0]);
                    topLeftPoint.FromLines[0].DicBay.Add(intraBay.Name, ZONE_LINE_TYPE.RESET);
                    topLeftPoint.FromLines[0].Bay = intraBay;

                    rp = ModelManager.Instance.AddRailPoint(new Vector3(topLeftPoint.PosVec3.X, rp_8.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, topLeftPointByZcu.Zcu.Name, ZCU_TYPE.STOP);
                    points.Add(rp);
                    underPoints.Add(rp);
                    zcuNo++;

                    //오른쪽
                    rp = ModelManager.Instance.AddRailPoint(new Vector3(topRightPoint.PosVec3.X + slopeDistance, rp_8.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, topRightPointByZcu.Zcu.Name, ZCU_TYPE.STOP);
                    points.Add(rp);
                    underPoints.Add(rp);

                    rl = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp, topRightPoint, true, 1000);
                    lines.Add(rl);

                    this.toLines.Add(rl);
                    rl.DicBay.Add(this.Name, ZONE_LINE_TYPE.RESET);
                    rl.Bay = this;
                    intraBay.FromLines.Add(topRightPoint.ToLines[0]);
                    topRightPoint.ToLines[0].DicBay.Add(intraBay.Name, ZONE_LINE_TYPE.STOP);
                    topRightPoint.ToLines[0].Bay = intraBay;

                    rp = ModelManager.Instance.AddRailPoint(new Vector3(topRightPoint.PosVec3.X, rp_8.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, topRightPointByZcu.Zcu.Name, ZCU_TYPE.RESET);
                    points.Add(rp);
                    underPoints.Add(rp);
                    zcuNo++;
                }
            }

            // 같은 행의 점들 x 좌표 순으로 정렬 (역순)
            overPoints = overPoints.OrderBy(p => p.PosVec3.X).ToList();

            // 같은 행의 점들 x 좌표 순으로 정렬
            underPoints = underPoints.OrderByDescending(p => p.PosVec3.X).ToList();

            // 내부 폐곡선 기본 포인트 생성 ↓↓↓
            double additionalDistance = 10;
            double additionalDistance2 = 2.5;
            slopeDistance = 5;

            RailPointNode rp_Inner_1 = ModelManager.Instance.AddRailPoint(new Vector3(slopeDistance + additionalDistance, slopeDistance + additionalDistance2, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
            points.Add(rp_Inner_1);
            RailPointNode rp_Inner_2 = ModelManager.Instance.AddRailPoint(new Vector3(slopeDistance + additionalDistance, length - slopeDistance - additionalDistance2, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
            points.Add(rp_Inner_2);
            RailPointNode rp_Inner_3 = ModelManager.Instance.AddRailPoint(new Vector3(slopeDistance + additionalDistance + additionalDistance2, length - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
            points.Add(rp_Inner_3);
            RailPointNode rp_Inner_4 = ModelManager.Instance.AddRailPoint(new Vector3(width - slopeDistance - additionalDistance - additionalDistance2, length - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
            points.Add(rp_Inner_4);
            RailPointNode rp_Inner_5 = ModelManager.Instance.AddRailPoint(new Vector3(width - slopeDistance - additionalDistance, length - slopeDistance - additionalDistance2, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
            points.Add(rp_Inner_5);
            RailPointNode rp_Inner_6 = ModelManager.Instance.AddRailPoint(new Vector3(width - slopeDistance - additionalDistance, slopeDistance + additionalDistance2, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
            points.Add(rp_Inner_6);
            RailPointNode rp_Inner_7 = ModelManager.Instance.AddRailPoint(new Vector3(width - slopeDistance - additionalDistance - additionalDistance2, slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
            points.Add(rp_Inner_7);
            RailPointNode rp_Inner_8 = ModelManager.Instance.AddRailPoint(new Vector3(slopeDistance + additionalDistance + additionalDistance2, slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
            points.Add(rp_Inner_8);

            double outerUnderY = rp_8.PosVec3.Y;
            double outerOverY = rp_3.PosVec3.Y;
            double innerUnderY = rp_Inner_8.PosVec3.Y;
            double innerOverY = rp_Inner_3.PosVec3.Y;

            List<RailPointNode> innerUnderPoints = new List<RailPointNode>();
            List<RailPointNode> innerOverPoints = new List<RailPointNode>();

            RailLineNode rl_Inner_1 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_1, rp_Inner_2, false, 5000);
            lines.Add(rl_Inner_1);
            RailLineNode rl_Inner_2 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_2, rp_Inner_3, true, 1000);
            lines.Add(rl_Inner_2);
            //            RailLineNode rl_3 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_3, rp_4);
            //            lines.Add(rl_3);
            RailLineNode rl_Inner_4 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_4, rp_Inner_5, true, 1000);
            lines.Add(rl_Inner_4);
            RailLineNode rl_Inner_5 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_5, rp_Inner_6, false, 5000);
            lines.Add(rl_Inner_5);
            RailLineNode rl_Inner_6 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_6, rp_Inner_7, true, 1000);
            lines.Add(rl_Inner_6);
            //            RailLineNode rl_7 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_7, rp_8);
            //            lines.Add(rl_7);
            RailLineNode rl_Inner_8 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_8, rp_Inner_1, true, 1000);
            lines.Add(rl_Inner_8);
            // 내부 폐곡선 기본 포인트 생성 ↑↑↑

            // 외부 폐곡선과 내부 폐곡선을 연결하기 위한 외부 폐곡선의 포인트 생성 ↓↓↓
            double additionalDistance3 = 1.5;

            List<RailPointNode> outerUnderPoints = new List<RailPointNode>();
            List<RailPointNode> outerOverPoints = new List<RailPointNode>();

            int overZCUNo = 1;
            int underZCUNo = 1;
            int centerZCUNo = 1;

            if (topIntraBays != null)
            {
                outerOverPoints = overPoints.ToList();
                // 양 끝점 삭제
                outerOverPoints.Remove(outerOverPoints.First());
                outerOverPoints.Remove(outerOverPoints.Last());

                for (int idx = 1; idx < topIntraBays.Count; idx++)
                {
                    RailPointNode leftPoint = outerOverPoints[idx * 4 - 1];
                    RailPointNode rightPoint = outerOverPoints[idx * 4];

                    double pointInterval = Math.Abs(leftPoint.PosVec3.X - rightPoint.PosVec3.X);

                    ZCU zcu_Over_Left = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Over_" + overZCUNo);
                    overZCUNo++;
                    ZCU zcu_Over_Right = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Over_" + overZCUNo);
                    overZCUNo++;

                    RailPointNode rp_Outer_Left = ModelManager.Instance.AddRailPoint(new Vector3(leftPoint.PosVec3.X + pointInterval * (1 / (double)5), leftPoint.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Over_Left.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Outer_Left);
                    overPoints.Add(rp_Outer_Left);
                    outerOverPoints.Add(rp_Outer_Left);

                    RailPointNode rp_Outer_Right = ModelManager.Instance.AddRailPoint(new Vector3(leftPoint.PosVec3.X + pointInterval * (4 / (double)5), leftPoint.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Over_Right.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Outer_Right);
                    overPoints.Add(rp_Outer_Right);
                    outerOverPoints.Add(rp_Outer_Right);

                    RailPointNode rp_Inner_Over_Left = ModelManager.Instance.AddRailPoint(rp_Outer_Left.PosVec3 + new Vector3(additionalDistance3 * 2.2, -slopeDistance, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Over_Left.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Inner_Over_Left);
                    innerOverPoints.Add(rp_Inner_Over_Left);

                    RailPointNode rp_Inner_Over_Right = ModelManager.Instance.AddRailPoint(rp_Outer_Right.PosVec3 + new Vector3(-additionalDistance3 * 2.2, -slopeDistance, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Over_Right.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Inner_Over_Right);
                    innerOverPoints.Add(rp_Inner_Over_Right);

                    // ZCU를 위한 포인트 생성
                    // 왼쪽
                    RailPointNode rp_Inner_Over_Left_Left = ModelManager.Instance.AddRailPoint(rp_Inner_Over_Left.PosVec3 + new Vector3(-additionalDistance3 * 2.2, 0, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Over_Left.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Inner_Over_Left_Left);
                    innerOverPoints.Add(rp_Inner_Over_Left_Left);

                    RailPointNode rp_Outer_Left_Right = ModelManager.Instance.AddRailPoint(rp_Outer_Left.PosVec3 + new Vector3(additionalDistance3 * 2.2, 0, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Over_Left.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Outer_Left_Right);
                    overPoints.Add(rp_Outer_Left_Right);
                    outerOverPoints.Add(rp_Outer_Left_Right);

                    // 오른쪽
                    RailPointNode rp_Inner_Over_Right_Right = ModelManager.Instance.AddRailPoint(rp_Inner_Over_Right.PosVec3 + new Vector3(additionalDistance3 * 2.2, 0, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Over_Right.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Inner_Over_Right_Right);
                    innerOverPoints.Add(rp_Inner_Over_Right_Right);

                    RailPointNode rp_Outer_Right_Left = ModelManager.Instance.AddRailPoint(rp_Outer_Right.PosVec3 + new Vector3(-additionalDistance3 * 2.2, 0, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Over_Right.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Outer_Right_Left);
                    overPoints.Add(rp_Outer_Right_Left);
                    outerOverPoints.Add(rp_Outer_Right_Left);

                    // 외부 폐곡선, 내부 폐곡선 연결
                    RailLineNode rl_Connect_Over_1 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Outer_Left, rp_Inner_Over_Left, true, 1000);
                    lines.Add(rl_Connect_Over_1);
                    RailLineNode rl_Connect_Over_2 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_Over_Right, rp_Outer_Right, true, 1000);
                    lines.Add(rl_Connect_Over_2);
                }
            }

            if (bottomIntraBays != null)
            {
                outerUnderPoints = underPoints.ToList();
                // 양 끝점 삭제
                outerUnderPoints.Remove(outerUnderPoints.First());
                outerUnderPoints.Remove(outerUnderPoints.Last());

                for (int idx = 1; idx < bottomIntraBays.Count; idx++)
                {
                    RailPointNode leftPoint = outerUnderPoints[idx * 4];
                    RailPointNode rightPoint = outerUnderPoints[idx * 4 - 1];

                    double pointInterval = Math.Abs(leftPoint.PosVec3.X - rightPoint.PosVec3.X);

                    ZCU zcu_Under_Left = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Under_" + underZCUNo);
                    underZCUNo++;
                    ZCU zcu_Under_Right = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Under_" + underZCUNo);
                    underZCUNo++;

                    RailPointNode rp_Outer_Left = ModelManager.Instance.AddRailPoint(new Vector3(rightPoint.PosVec3.X - pointInterval * (4 / (double)5), rightPoint.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Under_Left.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Outer_Left);
                    underPoints.Add(rp_Outer_Left);
                    outerUnderPoints.Add(rp_Outer_Left);

                    RailPointNode rp_Outer_Right = ModelManager.Instance.AddRailPoint(new Vector3(rightPoint.PosVec3.X - pointInterval * (1 / (double)5), rightPoint.PosVec3.Y, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Under_Right.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Outer_Right);
                    underPoints.Add(rp_Outer_Right);
                    outerUnderPoints.Add(rp_Outer_Right);

                    RailPointNode rp_Inner_Under_Left = ModelManager.Instance.AddRailPoint(rp_Outer_Left.PosVec3 + new Vector3(additionalDistance3 * 2.2, slopeDistance, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Under_Left.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Inner_Under_Left);
                    innerUnderPoints.Add(rp_Inner_Under_Left);

                    RailPointNode rp_Inner_Under_Right = ModelManager.Instance.AddRailPoint(rp_Outer_Right.PosVec3 + new Vector3(-additionalDistance3 * 2.2, slopeDistance, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Under_Right.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Inner_Under_Right);
                    innerUnderPoints.Add(rp_Inner_Under_Right);

                    // ZCU를 위한 포인트 생성
                    // 왼쪽
                    RailPointNode rp_Outer_Under_Left_Right = ModelManager.Instance.AddRailPoint(rp_Outer_Left.PosVec3 + new Vector3(additionalDistance3 * 2.2, 0, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Under_Left.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Outer_Under_Left_Right);
                    underPoints.Add(rp_Outer_Under_Left_Right);
                    outerUnderPoints.Add(rp_Outer_Under_Left_Right);

                    RailPointNode rp_Inner_Under_Left_Left = ModelManager.Instance.AddRailPoint(rp_Inner_Under_Left.PosVec3 + new Vector3(-additionalDistance3 * 2.2, 0, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Under_Left.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Inner_Under_Left_Left);
                    innerUnderPoints.Add(rp_Inner_Under_Left_Left);

                    // 오른쪽
                    RailPointNode rp_Outer_Under_Right_Left = ModelManager.Instance.AddRailPoint(rp_Outer_Right.PosVec3 + new Vector3(-additionalDistance3 * 2.2, 0, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Under_Right.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Outer_Under_Right_Left);
                    underPoints.Add(rp_Outer_Under_Right_Left);
                    outerUnderPoints.Add(rp_Outer_Under_Right_Left);

                    RailPointNode rp_Inner_Under_Right_Right = ModelManager.Instance.AddRailPoint(rp_Inner_Under_Right.PosVec3 + new Vector3(additionalDistance3 * 2.2, 0, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Under_Right.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Inner_Under_Right_Right);
                    innerUnderPoints.Add(rp_Inner_Under_Right_Right);

                    // 외부 폐곡선, 내부 폐곡선 연결
                    RailLineNode rl_Connect_Under_1 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Outer_Right, rp_Inner_Under_Right, true, 1000);
                    lines.Add(rl_Connect_Under_1);
                    RailLineNode rl_Connect_Under_2 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_Under_Left, rp_Outer_Left, true, 1000);
                    lines.Add(rl_Connect_Under_2);
                }
            }

            // topIntraBays가 없으니 InnerBottomPoints 만들자
            if (topIntraBays == null)
            {
                for (int idx = 0; idx < innerUnderPoints.Count; idx++)
                {
                    RailPointNode innerOverPoint = ModelManager.Instance.AddRailPoint(new Vector3(innerUnderPoints[idx].PosVec3.X, innerOverY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
                    points.Add(innerOverPoint);
                    innerOverPoints.Add(innerOverPoint);
                }

                innerOverPoints = innerOverPoints.OrderBy(point => point.PosVec3.X).ToList();

                for (int idx = 0; idx < innerOverPoints.Count; idx++)
                {
                    RailPointNode outerOverPoint = ModelManager.Instance.AddRailPoint(new Vector3(innerOverPoints[idx].PosVec3.X, outerOverY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
                    points.Add(outerOverPoint);
                    overPoints.Add(outerOverPoint);
                    outerOverPoints.Add(outerOverPoint);
                }

                // 외부 폐곡선, 내부 폐곡선 연결
                for (int idx = 0; idx < innerOverPoints.Count; idx += 4)
                {
                    RailPointNode rp_Outer_Over_Left = outerOverPoints[idx];
                    RailPointNode rp_Inner_Over_Left = innerOverPoints[idx + 1];

                    RailLineNode rl_Connect_Over_1 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Outer_Over_Left, rp_Inner_Over_Left, true, 1000);
                    lines.Add(rl_Connect_Over_1);

                    RailPointNode rp_Inner_Over_Right = innerOverPoints[idx + 2];
                    RailPointNode rp_Outer_Over_Right = outerOverPoints[idx + 3];

                    RailLineNode rl_Connect_Over_2 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_Over_Right, rp_Outer_Over_Right, true, 1000);
                    lines.Add(rl_Connect_Over_2);

                    // ZCU 설정
                    ZCU zcu_Over_Left = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Over_" + overZCUNo);
                    overZCUNo++;
                    ZCU zcu_Over_Right = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Over_" + overZCUNo);
                    overZCUNo++;

                    // 왼쪽
                    RailPointNode rp_Outer_Over_Left_Right = outerOverPoints[idx + 1];
                    RailPointNode rp_Inner_Over_Left_Left = innerOverPoints[idx];

                    rp_Outer_Over_Left.ZcuName = zcu_Over_Left.Name;
                    rp_Outer_Over_Left.ZcuType = ZCU_TYPE.STOP;
                    rp_Outer_Over_Left_Right.ZcuName = zcu_Over_Left.Name;
                    rp_Outer_Over_Left_Right.ZcuType = ZCU_TYPE.RESET;
                    rp_Inner_Over_Left_Left.ZcuName = zcu_Over_Left.Name;
                    rp_Inner_Over_Left_Left.ZcuType = ZCU_TYPE.STOP;
                    rp_Inner_Over_Left.ZcuName = zcu_Over_Left.Name;
                    rp_Inner_Over_Left.ZcuType = ZCU_TYPE.RESET;

                    // 오른쪽
                    RailPointNode rp_Outer_Over_Right_Left = outerOverPoints[idx + 2];
                    RailPointNode rp_Inner_Over_Right_Right = innerOverPoints[idx + 3];

                    rp_Inner_Over_Right.ZcuName = zcu_Over_Right.Name;
                    rp_Inner_Over_Right.ZcuType = ZCU_TYPE.STOP;
                    rp_Inner_Over_Right_Right.ZcuName = zcu_Over_Right.Name;
                    rp_Inner_Over_Right_Right.ZcuType = ZCU_TYPE.RESET;
                    rp_Outer_Over_Right.ZcuName = zcu_Over_Right.Name;
                    rp_Outer_Over_Right.ZcuType = ZCU_TYPE.RESET;
                    rp_Outer_Over_Right_Left.ZcuName = zcu_Over_Right.Name;
                    rp_Outer_Over_Right_Left.ZcuType = ZCU_TYPE.STOP;
                }
            }

            // bottomIntraBays가 없으니 InnerTopPoints 만들자
            if (bottomIntraBays == null)
            {
                for (int idx = 0; idx < innerOverPoints.Count; idx++)
                {
                    RailPointNode innerUnderPoint = ModelManager.Instance.AddRailPoint(new Vector3(innerOverPoints[idx].PosVec3.X, innerUnderY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
                    points.Add(innerUnderPoint);
                    innerUnderPoints.Add(innerUnderPoint);
                }

                innerUnderPoints = innerUnderPoints.OrderBy(point => point.PosVec3.X).ToList();

                for (int idx = 0; idx < innerUnderPoints.Count; idx++)
                {
                    RailPointNode outerUnderPoint = ModelManager.Instance.AddRailPoint(new Vector3(innerUnderPoints[idx].PosVec3.X, outerUnderY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, "", ZCU_TYPE.NON);
                    points.Add(outerUnderPoint);
                    underPoints.Add(outerUnderPoint);
                    outerUnderPoints.Add(outerUnderPoint);
                }

                // 외부 폐곡선, 내부 폐곡선 연결
                for (int idx = 0; idx < innerUnderPoints.Count; idx += 4)
                {
                    RailPointNode rp_Inner_Under_Left = innerUnderPoints[idx + 1];
                    RailPointNode rp_Outer_Under_Left = outerUnderPoints[idx];

                    RailLineNode rl_Connect_Over_1 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Inner_Under_Left, rp_Outer_Under_Left, true, 1000);
                    lines.Add(rl_Connect_Over_1);

                    RailPointNode rp_Outer_Under_Right = outerUnderPoints[idx + 3];
                    RailPointNode rp_Inner_Under_Right = innerUnderPoints[idx + 2];

                    RailLineNode rl_Connect_Over_2 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Outer_Under_Right, rp_Inner_Under_Right, true, 1000);
                    lines.Add(rl_Connect_Over_2);

                    // ZCU 설정
                    ZCU zcu_Under_Left = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Under_" + underZCUNo);
                    underZCUNo++;
                    ZCU zcu_Under_Right = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Under_" + underZCUNo);
                    underZCUNo++;

                    // 왼쪽
                    RailPointNode rp_Inner_Under_Left_Left = innerUnderPoints[idx];
                    RailPointNode rp_Outer_Under_Left_Right = outerUnderPoints[idx + 1];

                    rp_Inner_Under_Left.ZcuName = zcu_Under_Left.Name;
                    rp_Inner_Under_Left.ZcuType = ZCU_TYPE.STOP;
                    rp_Inner_Under_Left_Left.ZcuName = zcu_Under_Left.Name;
                    rp_Inner_Under_Left_Left.ZcuType = ZCU_TYPE.RESET;
                    rp_Outer_Under_Left.ZcuName = zcu_Under_Left.Name;
                    rp_Outer_Under_Left.ZcuType = ZCU_TYPE.RESET;
                    rp_Outer_Under_Left_Right.ZcuName = zcu_Under_Left.Name;
                    rp_Outer_Under_Left_Right.ZcuType = ZCU_TYPE.STOP;

                    // 오른쪽
                    RailPointNode rp_Outer_Under_Right_Left = outerUnderPoints[idx + 2];
                    RailPointNode rp_Inner_Under_Right_Right = innerUnderPoints[idx + 3];

                    rp_Outer_Under_Right.ZcuName = zcu_Under_Right.Name;
                    rp_Outer_Under_Right.ZcuType = ZCU_TYPE.STOP;
                    rp_Outer_Under_Right_Left.ZcuName = zcu_Under_Right.Name;
                    rp_Outer_Under_Right_Left.ZcuType = ZCU_TYPE.RESET;
                    rp_Inner_Under_Right.ZcuName = zcu_Under_Right.Name;
                    rp_Inner_Under_Right.ZcuType = ZCU_TYPE.RESET;
                    rp_Inner_Under_Right_Right.ZcuName = zcu_Under_Right.Name;
                    rp_Inner_Under_Right_Right.ZcuType = ZCU_TYPE.STOP;
                }
            }

            innerOverPoints = innerOverPoints.OrderBy(p => p.PosVec3.X).ToList();
            innerUnderPoints = innerUnderPoints.OrderBy(p => p.PosVec3.X).ToList();

            // Center 생성 후 잇기
            List<RailPointNode> innerOverCenterPoints = new List<RailPointNode>();
            List<RailPointNode> innerUnderCenterPoints = new List<RailPointNode>();

            int count = 1;

            for (int idx = 3; idx < innerOverPoints.Count; idx += 4)
            {
                if (idx == innerOverPoints.Count - 1)
                    break;

                RailPointNode rp_Inner_Over_Left = innerOverPoints[idx];
                RailPointNode rp_Inner_Over_Right = innerOverPoints[idx + 1];

                RailPointNode rp_Inner_Under_Left = innerUnderPoints[idx];
                RailPointNode rp_Inner_Under_Right = innerUnderPoints[idx + 1];

                double centerX = (rp_Inner_Over_Left.PosVec3.X + rp_Inner_Over_Right.PosVec3.X) / 2;

                ZCU zcu_Center = ModelManager.Instance.AddZcu(fab.Name, "ZCU_" + Name + "_Center_" + centerZCUNo);
                centerZCUNo++;

                if (count % 2 == 1)
                {
                    RailPointNode rp_Over_Center = ModelManager.Instance.AddRailPoint(new Vector3(centerX, innerOverY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Center.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Over_Center);
                    innerOverCenterPoints.Add(rp_Over_Center);
                    RailPointNode rp_Under_Center = ModelManager.Instance.AddRailPoint(new Vector3(centerX, innerUnderY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Center.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Under_Center);
                    innerUnderCenterPoints.Add(rp_Under_Center);
                    RailPointNode rp_Under_Center_Right = ModelManager.Instance.AddRailPoint(new Vector3(centerX + 5, innerUnderY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Center.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Under_Center_Right);
                    innerUnderCenterPoints.Add(rp_Under_Center_Right);

                    RailLineNode rl_Connect_Center = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Over_Center, rp_Under_Center, true, 1000);
                    lines.Add(rl_Connect_Center);
                }
                else
                {
                    RailPointNode rp_Over_Center_Left = ModelManager.Instance.AddRailPoint(new Vector3(centerX - 5, innerOverY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Center.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Over_Center_Left);
                    innerOverCenterPoints.Add(rp_Over_Center_Left);

                    RailPointNode rp_Over_Center = ModelManager.Instance.AddRailPoint(new Vector3(centerX, innerOverY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Center.Name, ZCU_TYPE.RESET);
                    points.Add(rp_Over_Center);
                    innerOverCenterPoints.Add(rp_Over_Center);
                    RailPointNode rp_Under_Center = ModelManager.Instance.AddRailPoint(new Vector3(centerX, innerUnderY, 0), "rp_" + fab.Name + "_" + (Name + "_" + points.Count + 1).ToString(), fab.Name, zcu_Center.Name, ZCU_TYPE.STOP);
                    points.Add(rp_Under_Center);
                    innerUnderCenterPoints.Add(rp_Under_Center);

                    RailLineNode rl_Connect_Center = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, rp_Under_Center, rp_Over_Center, true, 1000);
                    lines.Add(rl_Connect_Center);
                }

                count++;
            }

            innerUnderPoints.AddRange(innerUnderCenterPoints);
            innerOverPoints.AddRange(innerOverCenterPoints);

            innerUnderPoints.Add(rp_Inner_8);
            innerUnderPoints.Add(rp_Inner_7);

            innerOverPoints.Add(rp_Inner_3);
            innerOverPoints.Add(rp_Inner_4);

            #region 포인트 정렬
            // 같은 행의 점들 x 좌표 순으로 정렬
            overPoints = overPoints.OrderBy(p => p.PosVec3.X).ToList();

            // 같은 행의 점들 x 좌표 순으로 정렬 (역순)
            underPoints = underPoints.OrderByDescending(p => p.PosVec3.X).ToList();

            // 같은 행의 점들 x 좌표 순으로 정렬 
            innerOverPoints = innerOverPoints.OrderBy(p => p.PosVec3.X).ToList();

            // 같은 행의 점들 x 좌표 순으로 정렬 (역순)
            innerUnderPoints = innerUnderPoints.OrderByDescending(p => p.PosVec3.X).ToList();
            #endregion
            // 외부 폐곡선과 내부 폐곡선을 연결하기 위한 외부 폐곡선의 포인트 생성 ↑↑↑

            // 내부 폐곡선 위쪽 행 RailLine들 생성
            for (int i = 0; i < innerOverPoints.Count - 1; i++)
            {
                RailLineNode rl = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, innerOverPoints[i], innerOverPoints[i + 1], false, 5000);
                lines.Add(rl);
            }

            // 내부 폐곡선 아래쪽 행 RailLine들 생성
            for (int i = 0; i < innerUnderPoints.Count - 1; i++)
            {
                RailLineNode rl = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, innerUnderPoints[i], innerUnderPoints[i + 1], false, 5000);
                lines.Add(rl);
            }

            //위쪽 행 RailLine들 생성
            for (int i = 0; i < overPoints.Count - 1; i++)
            {
                RailLineNode rl = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, overPoints[i], overPoints[i + 1], false, 5000);
                lines.Add(rl);
            }

            //아래쪽 행 RailLine들 생성
            for (int i = 0; i < underPoints.Count - 1; i++)
            {
                RailLineNode rl = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + lines.Count + 1).ToString(), fab.Name, this, underPoints[i], underPoints[i + 1], false, 5000);
                lines.Add(rl);
            }
        }
    }
}
