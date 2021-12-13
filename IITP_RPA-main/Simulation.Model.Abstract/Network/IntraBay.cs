using System;
using System.Collections.Generic;
using Simulation.Geometry;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class IntraBay : Bay
    {
        public IntraBay(uint id, string name, Fab fab, bool reticle)
            : base(id, name, fab, reticle)
        {

        }

        public IntraBay(uint id, Vector3 pos, Vector3 size, string name, Fab fab, bool reticle)
            : base(id, pos, size, name, fab, reticle)
        {
            MakeRail2(fab.Name, pos.X, pos.Y, size.X, size.Y);
        }

        public void MakeProcess()
        {

        }

        /// <summary>
        /// 2차선 Intrabay rail 생성
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param
        private void MakeRail(string fabName, double x, double y, double width, double length)
        {
            double slopeDistance = 6;
            double intervalDistance = 10;

            ZCU zcu1 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 1);
            ZCU zcu2 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 2);
            ZCU zcu3 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 3);
            ZCU zcu4 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 4);
            ZCU zcu5 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 5);
            ZCU zcu6 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 6);
            ZCU zcu7 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 7);
            ZCU zcu8 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 8);
            ZCU zcu9 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 9);
            ZCU zcu10 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 10);
            ZCU zcu11 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 11);
            ZCU zcu12 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 12);
            ZCU zcu13 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 13);
            ZCU zcu14 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 14);
            ZCU zcu15 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 15);
            ZCU zcu16 = ModelManager.Instance.AddZcu(fabName, "ZCU_" + Name + "_" + 16);

            RailPointNode rp_1 = ModelManager.Instance.AddRailPoint(new Vector3(0, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 1).ToString(), fabName, null, ZCU_TYPE.NON);
            RailPointNode rp_2 = ModelManager.Instance.AddRailPoint(new Vector3(0, slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 2).ToString(), fabName, zcu1.Name, ZCU_TYPE.RESET);
            RailPointNode rp_3 = ModelManager.Instance.AddRailPoint(new Vector3(0, length / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 3).ToString(), fabName, zcu2.Name, ZCU_TYPE.STOP);
            RailPointNode rp_4 = ModelManager.Instance.AddRailPoint(new Vector3(0, length / 6 + slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 4).ToString(), fabName, zcu2.Name, ZCU_TYPE.RESET);
            RailPointNode rp_5 = ModelManager.Instance.AddRailPoint(new Vector3(0, length * 2 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 5).ToString(), fabName, zcu3.Name, ZCU_TYPE.STOP);
            RailPointNode rp_6 = ModelManager.Instance.AddRailPoint(new Vector3(0, length * 2 / 6 + slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 6).ToString(), fabName, zcu3.Name, ZCU_TYPE.RESET);
            RailPointNode rp_7 = ModelManager.Instance.AddRailPoint(new Vector3(0, length * 4 / 6 - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 7).ToString(), fabName, zcu4.Name, ZCU_TYPE.STOP);
            RailPointNode rp_8 = ModelManager.Instance.AddRailPoint(new Vector3(0, length * 4 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 8).ToString(), fabName, zcu4.Name, ZCU_TYPE.RESET);
            RailPointNode rp_9 = ModelManager.Instance.AddRailPoint(new Vector3(0, length * 5 / 6 - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 9).ToString(), fabName, zcu5.Name, ZCU_TYPE.STOP);
            RailPointNode rp_10 = ModelManager.Instance.AddRailPoint(new Vector3(0, length * 5 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 10).ToString(), fabName, zcu5.Name, ZCU_TYPE.RESET);
            RailPointNode rp_11 = ModelManager.Instance.AddRailPoint(new Vector3(0, length - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 11).ToString(), fabName, zcu6.Name, ZCU_TYPE.STOP);
            RailPointNode rp_12 = ModelManager.Instance.AddRailPoint(new Vector3(0, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 12).ToString(), fabName, null, ZCU_TYPE.NON);
            RailPointNode rp_13 = ModelManager.Instance.AddRailPoint(new Vector3(slopeDistance, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 13).ToString(), fabName, zcu6.Name, ZCU_TYPE.RESET);
            RailPointNode rp_14 = ModelManager.Instance.AddRailPoint(new Vector3(width - slopeDistance, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 14).ToString(), fabName, zcu7.Name, ZCU_TYPE.STOP);
            RailPointNode rp_15 = ModelManager.Instance.AddRailPoint(new Vector3(width, length, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 15).ToString(), fabName, null, ZCU_TYPE.NON);
            RailPointNode rp_16 = ModelManager.Instance.AddRailPoint(new Vector3(width, length - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 16).ToString(), fabName, zcu7.Name, ZCU_TYPE.RESET);
            RailPointNode rp_17 = ModelManager.Instance.AddRailPoint(new Vector3(width, length * 5 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 17).ToString(), fabName, zcu8.Name, ZCU_TYPE.STOP);
            RailPointNode rp_18 = ModelManager.Instance.AddRailPoint(new Vector3(width, length * 5 / 6 - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 18).ToString(), fabName, zcu8.Name, ZCU_TYPE.RESET);
            RailPointNode rp_19 = ModelManager.Instance.AddRailPoint(new Vector3(width, length * 4 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 19).ToString(), fabName, zcu9.Name, ZCU_TYPE.STOP);
            RailPointNode rp_20 = ModelManager.Instance.AddRailPoint(new Vector3(width, length * 4 / 6 - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 20).ToString(), fabName, zcu9.Name, ZCU_TYPE.RESET);
            RailPointNode rp_21 = ModelManager.Instance.AddRailPoint(new Vector3(width, length * 2 / 6 + slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 21).ToString(), fabName, zcu10.Name, ZCU_TYPE.STOP);
            RailPointNode rp_22 = ModelManager.Instance.AddRailPoint(new Vector3(width, length * 2 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 22).ToString(), fabName, zcu10.Name, ZCU_TYPE.RESET);
            RailPointNode rp_23 = ModelManager.Instance.AddRailPoint(new Vector3(width, length / 6 + slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 23).ToString(), fabName, zcu11.Name, ZCU_TYPE.STOP);
            RailPointNode rp_24 = ModelManager.Instance.AddRailPoint(new Vector3(width, length / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 24).ToString(), fabName, zcu11.Name, ZCU_TYPE.RESET);
            RailPointNode rp_25 = ModelManager.Instance.AddRailPoint(new Vector3(width, slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 25).ToString(), fabName, zcu12.Name, ZCU_TYPE.STOP);
            RailPointNode rp_26 = ModelManager.Instance.AddRailPoint(new Vector3(width, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 26).ToString(), fabName, null, ZCU_TYPE.NON);
            RailPointNode rp_27 = ModelManager.Instance.AddRailPoint(new Vector3(width - slopeDistance, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 27).ToString(), fabName, zcu12.Name, ZCU_TYPE.NON);
            RailPointNode rp_28 = ModelManager.Instance.AddRailPoint(new Vector3(slopeDistance, 0, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 28).ToString(), fabName, zcu1.Name, ZCU_TYPE.STOP);
            RailPointNode rp_29 = ModelManager.Instance.AddRailPoint(new Vector3(intervalDistance, intervalDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 29).ToString(), fabName, zcu13.Name, ZCU_TYPE.RESET);
            RailPointNode rp_30 = ModelManager.Instance.AddRailPoint(new Vector3(intervalDistance, length * 2 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 30).ToString(), fabName, zcu3.Name, ZCU_TYPE.STOP);
            RailPointNode rp_31 = ModelManager.Instance.AddRailPoint(new Vector3(intervalDistance, length * 2 / 6 + slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 31).ToString(), fabName, zcu3.Name, ZCU_TYPE.RESET);
            RailPointNode rp_32 = ModelManager.Instance.AddRailPoint(new Vector3(intervalDistance, length * 4 / 6 - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 32).ToString(), fabName, zcu4.Name, ZCU_TYPE.STOP);
            RailPointNode rp_33 = ModelManager.Instance.AddRailPoint(new Vector3(intervalDistance, length * 4 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 33).ToString(), fabName, zcu4.Name, ZCU_TYPE.RESET);
            RailPointNode rp_34 = ModelManager.Instance.AddRailPoint(new Vector3(intervalDistance, length - intervalDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 34).ToString(), fabName, zcu14.Name, ZCU_TYPE.STOP);
            RailPointNode rp_35 = ModelManager.Instance.AddRailPoint(new Vector3(intervalDistance + slopeDistance / 2, length - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 35).ToString(), fabName, zcu14.Name, ZCU_TYPE.RESET);
            RailPointNode rp_36 = ModelManager.Instance.AddRailPoint(new Vector3(width - intervalDistance - slopeDistance / 2, length - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 36).ToString(), fabName, zcu15.Name, ZCU_TYPE.STOP);
            RailPointNode rp_37 = ModelManager.Instance.AddRailPoint(new Vector3(width - intervalDistance, length - intervalDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 37).ToString(), fabName, zcu15.Name, ZCU_TYPE.RESET);
            RailPointNode rp_38 = ModelManager.Instance.AddRailPoint(new Vector3(width - intervalDistance, length * 4 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 38).ToString(), fabName, zcu9.Name, ZCU_TYPE.STOP);
            RailPointNode rp_39 = ModelManager.Instance.AddRailPoint(new Vector3(width - intervalDistance, length * 4 / 6 - slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 39).ToString(), fabName, zcu9.Name, ZCU_TYPE.RESET);
            RailPointNode rp_40 = ModelManager.Instance.AddRailPoint(new Vector3(width - intervalDistance, length * 2 / 6 + slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 40).ToString(), fabName, zcu10.Name, ZCU_TYPE.STOP);
            RailPointNode rp_41 = ModelManager.Instance.AddRailPoint(new Vector3(width - intervalDistance, length * 2 / 6, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 41).ToString(), fabName, zcu10.Name, ZCU_TYPE.RESET);
            RailPointNode rp_42 = ModelManager.Instance.AddRailPoint(new Vector3(width - intervalDistance, intervalDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 42).ToString(), fabName, zcu16.Name, ZCU_TYPE.STOP);
            RailPointNode rp_43 = ModelManager.Instance.AddRailPoint(new Vector3(width - intervalDistance - slopeDistance / 2, slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 43).ToString(), fabName, zcu16.Name, ZCU_TYPE.RESET);
            RailPointNode rp_44 = ModelManager.Instance.AddRailPoint(new Vector3(intervalDistance + slopeDistance / 2, slopeDistance, 0) + new Vector3(x, y, 0), "rp_" + fab.Name + "_" + (Name + "_" + 44).ToString(), fabName, zcu13.Name, ZCU_TYPE.STOP);

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
            points.Add(rp_17);
            points.Add(rp_18);
            points.Add(rp_19);
            points.Add(rp_20);
            points.Add(rp_21);
            points.Add(rp_22);
            points.Add(rp_23);
            points.Add(rp_24);
            points.Add(rp_25);
            points.Add(rp_26);
            points.Add(rp_27);
            points.Add(rp_28);
            points.Add(rp_29);
            points.Add(rp_30);
            points.Add(rp_31);
            points.Add(rp_32);
            points.Add(rp_33);
            points.Add(rp_34);
            points.Add(rp_35);
            points.Add(rp_36);
            points.Add(rp_37);
            points.Add(rp_38);
            points.Add(rp_39);
            points.Add(rp_40);
            points.Add(rp_41);
            points.Add(rp_42);
            points.Add(rp_43);
            points.Add(rp_44);

            //-------------------------
            RailLineNode rl_1 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 1).ToString(), fabName, this, rp_28, rp_2);
            RailLineNode rl_2 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 2).ToString(), fabName, this, rp_2, rp_3);
            RailLineNode rl_3 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 3).ToString(), fabName, this, rp_3, rp_4);
            RailLineNode rl_4 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 4).ToString(), fabName, this, rp_4, rp_5);
            RailLineNode rl_5 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 5).ToString(), fabName, this, rp_5, rp_6);
            RailLineNode rl_6 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 6).ToString(), fabName, this, rp_6, rp_7);
            RailLineNode rl_7 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 7).ToString(), fabName, this, rp_7, rp_8);
            RailLineNode rl_8 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 8).ToString(), fabName, this, rp_8, rp_9);
            RailLineNode rl_9 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 9).ToString(), fabName, this, rp_9, rp_10);
            RailLineNode rl_10 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 10).ToString(), fabName, this, rp_10, rp_11);
            RailLineNode rl_11 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 11).ToString(), fabName, this, rp_11, rp_13);
            RailLineNode rl_12 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 12).ToString(), fabName, this, rp_13, rp_14);
            RailLineNode rl_13 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 13).ToString(), fabName, this, rp_14, rp_16);
            RailLineNode rl_14 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 14).ToString(), fabName, this, rp_16, rp_17);
            RailLineNode rl_15 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 15).ToString(), fabName, this, rp_17, rp_18);
            RailLineNode rl_16 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 16).ToString(), fabName, this, rp_18, rp_19);
            RailLineNode rl_17 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 17).ToString(), fabName, this, rp_19, rp_20);
            RailLineNode rl_18 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 18).ToString(), fabName, this, rp_20, rp_21);
            RailLineNode rl_19 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 19).ToString(), fabName, this, rp_21, rp_22);
            RailLineNode rl_20 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 20).ToString(), fabName, this, rp_22, rp_23);
            RailLineNode rl_21 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 21).ToString(), fabName, this, rp_23, rp_24);
            RailLineNode rl_22 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 22).ToString(), fabName, this, rp_24, rp_25);
            RailLineNode rl_23 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 23).ToString(), fabName, this, rp_25, rp_27);
            RailLineNode rl_24 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 24).ToString(), fabName, this, rp_27, rp_28);
            RailLineNode rl_25 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 25).ToString(), fabName, this, rp_44, rp_29);
            RailLineNode rl_26 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 26).ToString(), fabName, this, rp_29, rp_30);
            RailLineNode rl_27 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 27).ToString(), fabName, this, rp_30, rp_31);
            RailLineNode rl_28 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 28).ToString(), fabName, this, rp_31, rp_32);
            RailLineNode rl_29 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 29).ToString(), fabName, this, rp_32, rp_33);
            RailLineNode rl_30 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 30).ToString(), fabName, this, rp_33, rp_34);
            RailLineNode rl_31 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 31).ToString(), fabName, this, rp_34, rp_35);
            RailLineNode rl_32 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 32).ToString(), fabName, this, rp_35, rp_36);
            RailLineNode rl_33 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 33).ToString(), fabName, this, rp_36, rp_37);
            RailLineNode rl_34 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 34).ToString(), fabName, this, rp_37, rp_38);
            RailLineNode rl_35 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 35).ToString(), fabName, this, rp_38, rp_39);
            RailLineNode rl_36 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 36).ToString(), fabName, this, rp_39, rp_40);
            RailLineNode rl_37 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 37).ToString(), fabName, this, rp_40, rp_41);
            RailLineNode rl_38 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 38).ToString(), fabName, this, rp_41, rp_42);
            RailLineNode rl_39 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 39).ToString(), fabName, this, rp_42, rp_43);
            RailLineNode rl_40 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 40).ToString(), fabName, this, rp_43, rp_44);
            RailLineNode rl_41 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 41).ToString(), fabName, this, rp_1, rp_2, true);
            RailLineNode rl_42 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 42).ToString(), fabName, this, rp_11, rp_12, true);
            RailLineNode rl_43 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 43).ToString(), fabName, this, rp_15, rp_16, true);
            RailLineNode rl_44 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 44).ToString(), fabName, this, rp_25, rp_26, true);
            RailLineNode rl_45 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 45).ToString(), fabName, this, rp_5, rp_31);
            RailLineNode rl_46 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 46).ToString(), fabName, this, rp_32, rp_8);
            RailLineNode rl_47 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 47).ToString(), fabName, this, rp_19, rp_39);
            RailLineNode rl_48 = ModelManager.Instance.AddRailLine("rl_" + fab.Name + "_" + (Name + "_" + 48).ToString(), fabName, this, rp_40, rp_22);

            //버퍼 올라갈 링크 리스트
            List<RailLineNode> lstRL = new List<RailLineNode>();

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
            lines.Add(rl_21);
            lines.Add(rl_22);
            lines.Add(rl_23);
            lines.Add(rl_24);
            lines.Add(rl_25);
            lines.Add(rl_26);
            lines.Add(rl_27);
            lines.Add(rl_28);
            lines.Add(rl_29);
            lines.Add(rl_30);
            lines.Add(rl_31);
            lines.Add(rl_32);
            lines.Add(rl_33);
            lines.Add(rl_34);
            lines.Add(rl_35);
            lines.Add(rl_36);
            lines.Add(rl_37);
            lines.Add(rl_38);
            lines.Add(rl_39);
            lines.Add(rl_40);
            lines.Add(rl_41);
            lines.Add(rl_42);
            lines.Add(rl_43);
            lines.Add(rl_44);
            lines.Add(rl_45);
            lines.Add(rl_46);
            lines.Add(rl_47);
            lines.Add(rl_48);
        }

        private void MakeRail2(string fabName, double x, double y, double width, double length)
        {
            double slopeDistance = 6;
            double intervalDistance = 10;

            #region ZCU 총 12개
            string zcu_name;

            // ZCU No.1
            zcu_name = fabName + "_ZCU_" + Name + "_" + 1;
            ZCU zcu1 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.2
            zcu_name = fabName + "_ZCU_" + Name + "_" + 2;
            ZCU zcu2 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.3
            zcu_name = fabName + "_ZCU_" + Name + "_" + 3;
            ZCU zcu3 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.4
            zcu_name = fabName + "_ZCU_" + Name + "_" + 4;
            ZCU zcu4 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.5
            zcu_name = fabName + "_ZCU_" + Name + "_" + 5;
            ZCU zcu5 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.6
            zcu_name = fabName + "_ZCU_" + Name + "_" + 6;
            ZCU zcu6 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.7
            zcu_name = fabName + "_ZCU_" + Name + "_" + 7;
            ZCU zcu7 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.8
            zcu_name = fabName + "_ZCU_" + Name + "_" + 8;
            ZCU zcu8 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.9
            zcu_name = fabName + "_ZCU_" + Name + "_" + 9;
            ZCU zcu9 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.10
            zcu_name = fabName + "_ZCU_" + Name + "_" + 10;
            ZCU zcu10 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.11
            zcu_name = fabName + "_ZCU_" + Name + "_" + 11;
            ZCU zcu11 = ModelManager.Instance.AddZcu(fabName, zcu_name);

            // ZCU No.12
            zcu_name = fabName + "_ZCU_" + Name + "_" + 12;
            ZCU zcu12 = ModelManager.Instance.AddZcu(fabName, zcu_name);
            #endregion

            #region Rail Point 총 32개
            Vector3 rp_pos;
            string rp_name;
            string zcuName;
            ZCU_TYPE zcuType;

            // RailPoint No.1
            rp_pos = new Vector3(0, 0, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 1).ToString();
            zcuName = zcu1.Name;
            zcuType = ZCU_TYPE.NON;
            RailPointNode rp_1 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.2
            rp_pos = new Vector3(0, slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 2).ToString();
            zcuName = zcu1.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_2 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.3
            rp_pos = new Vector3(0, length * 1 / 6, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 3).ToString();
            zcuName = zcu2.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_3 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.4
            rp_pos = new Vector3(0, length * 1 / 6 + slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 4).ToString();
            zcuName = zcu2.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_4 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.5
            rp_pos = new Vector3(0, length * 2 / 6 + intervalDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 5).ToString();
            zcuName = zcu3.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_5 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.6
            rp_pos = new Vector3(0, length * 2 / 6 + intervalDistance + slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 6).ToString();
            zcuName = zcu3.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_6 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.7
            rp_pos = new Vector3(0, length * 4 / 6 - intervalDistance - slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 7).ToString();
            zcuName = zcu4.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_7 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.8
            rp_pos = new Vector3(0, length * 4 / 6 - intervalDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 8).ToString();
            zcuName = zcu4.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_8 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.9
            rp_pos = new Vector3(0, length * 5 / 6 - slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 9).ToString();
            zcuName = zcu5.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_9 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.10
            rp_pos = new Vector3(0, length * 5 / 6, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 10).ToString();
            zcuName = zcu5.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_10 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.11
            rp_pos = new Vector3(0, length - slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 11).ToString();
            zcuName = zcu6.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_11 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.12
            rp_pos = new Vector3(0, length, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 12).ToString();
            zcuName = zcu6.Name;
            zcuType = ZCU_TYPE.NON;
            RailPointNode rp_12 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.13
            rp_pos = new Vector3(slopeDistance, length, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 13).ToString();
            zcuName = zcu6.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_13 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.14
            rp_pos = new Vector3(width - slopeDistance, length, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 14).ToString();
            zcuName = zcu7.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_14 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.15
            rp_pos = new Vector3(width, length, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 15).ToString();
            zcuName = zcu7.Name;
            zcuType = ZCU_TYPE.NON;
            RailPointNode rp_15 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.16
            rp_pos = new Vector3(width, length - slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 16).ToString();
            zcuName = zcu7.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_16 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.17
            rp_pos = new Vector3(width, length * 5 / 6, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 17).ToString();
            zcuName = zcu8.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_17 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.18
            rp_pos = new Vector3(width, length * 5 / 6 - slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 18).ToString();
            zcuName = zcu8.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_18 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.19
            rp_pos = new Vector3(width, length * 4 / 6 - intervalDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 19).ToString();
            zcuName = zcu9.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_19 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.20
            rp_pos = new Vector3(width, length * 4 / 6 - intervalDistance - slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 20).ToString();
            zcuName = zcu9.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_20 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.21
            rp_pos = new Vector3(width, length * 2 / 6 + intervalDistance + slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 21).ToString();
            zcuName = zcu10.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_21 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.22
            rp_pos = new Vector3(width, length * 2 / 6 + intervalDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 22).ToString();
            zcuName = zcu10.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_22 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.23
            rp_pos = new Vector3(width, length / 6 + slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 23).ToString();
            zcuName = zcu11.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_23 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.24
            rp_pos = new Vector3(width, length / 6, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 24).ToString();
            zcuName = zcu11.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_24 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.25
            rp_pos = new Vector3(width, slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 25).ToString();
            zcuName = zcu12.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_25 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.26
            rp_pos = new Vector3(width, 0, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 26).ToString();
            zcuName = zcu12.Name;
            zcuType = ZCU_TYPE.NON;
            RailPointNode rp_26 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.27
            rp_pos = new Vector3(width - slopeDistance, 0, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 27).ToString();
            zcuName = zcu12.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_27 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.28
            rp_pos = new Vector3(slopeDistance, 0, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 28).ToString();
            zcuName = zcu1.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_28 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.29
            rp_pos = new Vector3(slopeDistance, length * 2 / 6 + intervalDistance + slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 29).ToString();
            zcuName = zcu3.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_29 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.30
            rp_pos = new Vector3(width - slopeDistance, length * 2 / 6 + intervalDistance + slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 30).ToString();
            zcuName = zcu10.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_30 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.31
            rp_pos = new Vector3(width - slopeDistance, length * 4 / 6 - intervalDistance - slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 31).ToString();
            zcuName = zcu9.Name;
            zcuType = ZCU_TYPE.RESET;
            RailPointNode rp_31 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);

            // RailPoint No.32
            rp_pos = new Vector3(slopeDistance, length * 4 / 6 - intervalDistance - slopeDistance, 0) + new Vector3(x, y, 0);
            rp_name = "rp_" + fab.Name + "_" + (Name + "_" + 32).ToString();
            zcuName = zcu4.Name;
            zcuType = ZCU_TYPE.STOP;
            RailPointNode rp_32 = ModelManager.Instance.AddRailPoint(rp_pos, rp_name, fabName, zcuName, zcuType);
            #endregion

            #region Rail Line 총 34개
            string rl_name;
            RailPointNode startPoint;
            RailPointNode endPoint;
            bool isCurve;

            // RailLine No.1
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 1).ToString();
            startPoint = rp_28;
            endPoint = rp_2;
            isCurve = true;
            RailLineNode rl_1 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.2
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 2).ToString();
            startPoint = rp_2;
            endPoint = rp_3;
            isCurve = false;
            RailLineNode rl_2 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.3
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 3).ToString();
            startPoint = rp_3;
            endPoint = rp_4;
            isCurve = false;
            RailLineNode rl_3 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.4
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 4).ToString();
            startPoint = rp_4;
            endPoint = rp_5;
            isCurve = false;
            RailLineNode rl_4 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.5
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 5).ToString();
            startPoint = rp_5;
            endPoint = rp_6;
            isCurve = false;
            RailLineNode rl_5 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.6
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 6).ToString();
            startPoint = rp_6;
            endPoint = rp_7;
            isCurve = false;
            RailLineNode rl_6 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.7
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 7).ToString();
            startPoint = rp_7;
            endPoint = rp_8;
            isCurve = false;
            RailLineNode rl_7 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.8
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 8).ToString();
            startPoint = rp_8;
            endPoint = rp_9;
            isCurve = false;
            RailLineNode rl_8 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.9
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 9).ToString();
            startPoint = rp_9;
            endPoint = rp_10;
            isCurve = false;
            RailLineNode rl_9 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.10
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 10).ToString();
            startPoint = rp_10;
            endPoint = rp_11;
            isCurve = false;
            RailLineNode rl_10 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.11
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 11).ToString();
            startPoint = rp_11;
            endPoint = rp_13;
            isCurve = true;
            RailLineNode rl_11 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.12
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 12).ToString();
            startPoint = rp_13;
            endPoint = rp_14;
            isCurve = false;
            RailLineNode rl_12 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.13
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 13).ToString();
            startPoint = rp_14;
            endPoint = rp_16;
            isCurve = true;
            RailLineNode rl_13 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.14
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 14).ToString();
            startPoint = rp_16;
            endPoint = rp_17;
            isCurve = false;
            RailLineNode rl_14 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.15
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 15).ToString();
            startPoint = rp_17;
            endPoint = rp_18;
            isCurve = false;
            RailLineNode rl_15 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.16
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 16).ToString();
            startPoint = rp_18;
            endPoint = rp_19;
            isCurve = false;
            RailLineNode rl_16 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.17
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 17).ToString();
            startPoint = rp_19;
            endPoint = rp_20;
            isCurve = false;
            RailLineNode rl_17 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.18
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 18).ToString();
            startPoint = rp_20;
            endPoint = rp_21;
            isCurve = false;
            RailLineNode rl_18 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.19
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 19).ToString();
            startPoint = rp_21;
            endPoint = rp_22;
            isCurve = false;
            RailLineNode rl_19 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.20
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 20).ToString();
            startPoint = rp_22;
            endPoint = rp_23;
            isCurve = false;
            RailLineNode rl_20 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.21
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 21).ToString();
            startPoint = rp_23;
            endPoint = rp_24;
            isCurve = false;
            RailLineNode rl_21 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.22
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 22).ToString();
            startPoint = rp_24;
            endPoint = rp_25;
            isCurve = false;
            RailLineNode rl_22 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 5000);

            // RailLine No.23
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 23).ToString();
            startPoint = rp_25;
            endPoint = rp_27;
            isCurve = true;
            RailLineNode rl_23 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.24
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 24).ToString();
            startPoint = rp_27;
            endPoint = rp_28;
            isCurve = false;
            RailLineNode rl_24 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.25
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 25).ToString();
            startPoint = rp_5;
            endPoint = rp_29;
            isCurve = true;
            RailLineNode rl_25 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.26
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 26).ToString();
            startPoint = rp_29;
            endPoint = rp_30;
            isCurve = false;
            RailLineNode rl_26 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.27
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 27).ToString();
            startPoint = rp_30;
            endPoint = rp_22;
            isCurve = true;
            RailLineNode rl_27 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.28
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 28).ToString();
            startPoint = rp_19;
            endPoint = rp_31;
            isCurve = true;
            RailLineNode rl_28 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.29
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 29).ToString();
            startPoint = rp_31;
            endPoint = rp_32;
            isCurve = false;
            RailLineNode rl_29 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.30
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 30).ToString();
            startPoint = rp_32;
            endPoint = rp_8;
            isCurve = true;
            RailLineNode rl_30 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.31
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 31).ToString();
            startPoint = rp_1;
            endPoint = rp_2;
            isCurve = false;
            RailLineNode rl_31 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.32
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 32).ToString();
            startPoint = rp_11;
            endPoint = rp_12;
            isCurve = false;
            RailLineNode rl_32 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.33
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 33).ToString();
            startPoint = rp_15;
            endPoint = rp_16;
            isCurve = false;
            RailLineNode rl_33 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);

            // RailLine No.34
            rl_name = "rl_" + fab.Name + "_" + (Name + "_" + 34).ToString();
            startPoint = rp_25;
            endPoint = rp_26;
            isCurve = false;
            RailLineNode rl_34 = ModelManager.Instance.AddRailLine(rl_name, fabName, this, startPoint, endPoint, isCurve, 1000);
            #endregion

            #region Add Rail Point & Rail Line
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
            points.Add(rp_17);
            points.Add(rp_18);
            points.Add(rp_19);
            points.Add(rp_20);
            points.Add(rp_21);
            points.Add(rp_22);
            points.Add(rp_23);
            points.Add(rp_24);
            points.Add(rp_25);
            points.Add(rp_26);
            points.Add(rp_27);
            points.Add(rp_28);
            points.Add(rp_29);
            points.Add(rp_30);
            points.Add(rp_31);
            points.Add(rp_32);

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
            lines.Add(rl_21);
            lines.Add(rl_22);
            lines.Add(rl_23);
            lines.Add(rl_24);
            lines.Add(rl_25);
            lines.Add(rl_26);
            lines.Add(rl_27);
            lines.Add(rl_28);
            lines.Add(rl_29);
            lines.Add(rl_30);
            lines.Add(rl_31);
            lines.Add(rl_32);
            lines.Add(rl_33);
            lines.Add(rl_34);
            #endregion
        }
    }
}
