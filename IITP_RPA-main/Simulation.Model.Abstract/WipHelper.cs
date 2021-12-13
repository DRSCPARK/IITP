using Simulation.Engine;
using Simulation.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class WipHelper
    {
        #region Variable
        private static WipHelper _wipHelper;

        public static WipHelper Instance
        {
            get { return _wipHelper; }
        }
        #endregion

        public WipHelper()
        {
            _wipHelper = this;
        }

        // Buffer 위에 있는 WIP은 다음 공정 수행하는 장비가 속해 있는 IntraBay의 임의의 Buffer에 놓도록 한다.
        public void InitializeWips()
        {
            foreach (Foup foup in ModelManager.Instance.Foups.Values)
            {
                FabSimNode eqp = foup.CurrentEqp;

                // 현재 ProcessEqp에서 Run 중이거나
                if (eqp is ProcessEqpNode)
                {

                }
                // 현재 Buffer 위에서 Wait 중이거나
                else if (eqp is null)
                {
                    FoupHistorys historys = foup.Historys;
                    List<RailPortNode> availableSTBs = new List<RailPortNode>();

                    if (historys.Historys.Count == 0)
                    {
                        List<RailPortNode> interBaySTBs = GetInterBaySTBs();

                        availableSTBs = interBaySTBs.Where(stb => (RAILPORT_STATE)stb.NodeState == RAILPORT_STATE.EMPTY).ToList();
                    }
                    else
                    {
                        FoupHistory firstHistory = historys.FindNextHistory();
                        string firstEqpID = firstHistory.EqpID;
                        FabSimNode firstEqp = ModelManager.Instance.DicEqp[firstEqpID];

                        if (firstEqp is ProcessEqpNode)
                        {
                            ProcessEqpNode processEqp = firstEqp as ProcessEqpNode;

                            string bayName = processEqp.BayName;

                            availableSTBs = FindIntraBaySTBs(bayName);

                            if (availableSTBs.Count == 0)
                                availableSTBs = GetAvailableSTBs(processEqp.PosVec3);
                        }
                        // Commit or Complete
                        else
                        {
                            availableSTBs = GetAvailableSTBs(firstEqp.PosVec3);
                        }
                    }

                    if (availableSTBs.Count > 0)
                    {
                        RailPortNode availableSTB = availableSTBs.First();

                        foup.CurrentNode = availableSTB;
                        foup.CurrentState = FOUP_STATE.BUFFER;
                        availableSTB.LoadedEntity = foup;
                        foup.PosVec3 = availableSTB.PosVec3 + new Vector3(0.5, 0.5, 0);
                        availableSTB.NodeState = RAILPORT_STATE.FULL;
                    }
                }
            }
        }

        public List<RailPortNode> GetAllSTBs()
        {
            List<RailPortNode> allSTBs = new List<RailPortNode>();

            List<RailPortNode> railPorts = ModelManager.Instance.DicRailPort.Values.ToList();

            allSTBs = railPorts.Where(port => (port is ProcessPortNode) is false).ToList();

            return allSTBs;
        }

        public List<RailPortNode> GetAvailableSTBs()
        {
            List<RailPortNode> availableSTBs = new List<RailPortNode>();

            List<RailPortNode> totalSTBs = GetAllSTBs();

            availableSTBs = totalSTBs.Where(stb => (RAILPORT_STATE)stb.NodeState == RAILPORT_STATE.EMPTY).ToList();

            return availableSTBs;
        }

        public List<RailPortNode> GetAvailableSTBs(Vector3 eqpPos)
        {
            List<RailPortNode> availableSTBs = new List<RailPortNode>();

            List<RailPortNode> totalSTBs = GetAllSTBs();

            availableSTBs = totalSTBs.Where(stb => (RAILPORT_STATE)stb.NodeState == RAILPORT_STATE.EMPTY).ToList();

            if (availableSTBs.Any())
            {
                availableSTBs.Sort((o1, o2) => (Vector3.Distance(eqpPos, o1.PosVec3).CompareTo(Vector3.Distance(eqpPos, o2.PosVec3))));

                return availableSTBs;
            }
            else
                return null;
        }

        public List<RailPortNode> GetInterBaySTBs()
        {
            List<RailPortNode> interbaySTBs = new List<RailPortNode>();

            List<RailPortNode> totalSTBs = GetAllSTBs();

            interbaySTBs = totalSTBs.Where(stb => stb.Name.Contains("InterBay")).ToList();

            return interbaySTBs;
        }

        public List<RailPortNode> FindIntraBaySTBs(string bayName)
        {
            List<RailPortNode> availableSTBs = new List<RailPortNode>();

            List<RailPortNode> totalSTBs = GetAllSTBs();

            List<RailPortNode> baySTBs = totalSTBs.Where(stb => stb.Name.Contains(bayName)).ToList();

            availableSTBs = baySTBs.Where(stb => (RAILPORT_STATE)stb.NodeState == RAILPORT_STATE.EMPTY).ToList();

            return availableSTBs;
        }
    }
}
