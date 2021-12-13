using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Engine;
using Simulation.Geometry;
using CSScriptLibrary;

namespace Simulation.Model.Abstract
{
    public class Dispatcher
    {
        private static Dispatcher _instance;
        private Vector2 ohtPosition = Vector2.One;
        string dynamicRoutingScript;

        public static Dispatcher Instance
        {
            get { return _instance; }
        }

        public Dispatcher( )
        {
            _instance = this;

            dynamicRoutingScript = @"
                using Simulation.Model.Abstract;
                double GetPathScore(CandidatePath c)
                {
                    c.WeightSumScore = c.Length;
                    return c.WeightSumScore;
                }";

            GetDynamicRoutingLogic(dynamicRoutingScript);
        }



        public void GetScriptAlgorithm(string script, dynamic scriptDynamic)
        {
            scriptDynamic = CSScript.Evaluator.CreateDelegate(script);
        }

        public void GetDynamicRoutingLogic(string scriptLogic)
        {
//            ScoreFunctionByScript = CSScript.Evaluator.CreateDelegate(scriptLogic);
        }


        public static SimNode DefaultDispatching(List<SimLink> outLinks, Foup foup)
        {
            return new SimNode();
        }

        #region Production Simulation
        public RailPointNode DecisionDestination(RailPointNode roadNode, Foup foup, out SimNode destNode)
        {
            destNode = null;

            if (foup.LstProcess.Count == 0)
            {
                foreach (KeyValuePair<uint, SimNode> ky in ModelManager.Instance.SimNodes)
                {
                    if (ky.Value is CompleteNode)
                    {
                        destNode = ky.Value;
                        return (RailPointNode)((CompleteNode)ky.Value).OutLinks[0].EndNode;
                    }
                }
            }

            foreach (KeyValuePair<uint, SimNode> ky in ModelManager.Instance.SimNodes)
            {
                if (ky.Value is ProcessEqpNode)
                {
                    if (((ProcessEqpNode)ky.Value).NodeState.Equals(PROCESSEQP_STATE.NOT_FULL))
                    {
                        for (int i = 0; i < ((ProcessEqpNode)ky.Value).PocessIDs.Count; i++)
                        {
                            if (foup.LstProcess[0].Equals(((ProcessEqpNode)ky.Value).PocessIDs[i]))
                            {
                                if (((ProcessEqpNode)ky.Value).OutLinks[0].EndNode is StockerNode)
                                {
                                    StockerNode node = (StockerNode)((ProcessEqpNode)ky.Value).OutLinks[0].EndNode;

                                    if (node.OutLinks[0].EndNode is RailPointNode)
                                    {
                                        destNode = node;
                                        ((ProcessEqpNode)ky.Value).NodeState = PROCESSEQP_STATE.RESERVED;
                                        foup.TargetNode = ky.Value;
                                        return (RailPointNode)node.OutLinks[0].EndNode;
                                    }

                                }
                                else if (((ProcessEqpNode)ky.Value).OutLinks[0].EndNode is RailPointNode)
                                {
                                    destNode = (ProcessEqpNode)ky.Value;
                                    ((ProcessEqpNode)ky.Value).NodeState = PROCESSEQP_STATE.RESERVED;
                                    foup.TargetNode = ky.Value;
                                    return (RailPointNode)(((ProcessEqpNode)ky.Value).OutLinks[0].EndNode);
                                }
                                else if (((ProcessEqpNode)ky.Value).OutLinks[0].EndNode is CompleteNode)
                                {
                                    destNode = (ProcessEqpNode)ky.Value;
                                    ((ProcessEqpNode)ky.Value).NodeState = PROCESSEQP_STATE.RESERVED;
                                    foup.TargetNode = ky.Value;
                                    RailPointNode DesNode = null;
                                    foreach (SimLink SL in ky.Value.InLinks)
                                    {
                                        if (SL.StartNode is RailPointNode)
                                            DesNode = (RailPointNode)SL.StartNode;
                                    }
                                    return DesNode;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public ProcessEqpNode ProcessDispatching(uint nextOperationID)
        {
            List<ProcessEqpNode> processList = new List<ProcessEqpNode>();
            foreach (ProcessEqpNode node in ModelManager.Instance.DicProcessEqpNode.Values)
            {
                if (node.PocessIDs.Contains(nextOperationID) && node.GetEmptyPortCount() > 0)
                    processList.Add(node);
            }

            if (processList.Count == 0)
                return null;

            processList.Sort(compareProcess); //내림차순이어야?

            return processList[0];
        }

        private int compareProcess(ProcessEqpNode x, ProcessEqpNode y)
        {
            return x.GetEmptyPortCount().CompareTo(y.GetEmptyPortCount());
        }
        #endregion

        #region Material Handling Simulation

        public bool CheckIdleOHTforCommand(ref OHTNode oht, Command command, Time simTime)
        {
            if (command.Fab.ReadyOHTs.Count() > 0)
            {
                oht = DispatchOHT(command, simTime);

                if (oht != null && oht.Fab.Name == command.Fab.Name)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// 나중에 simPort의 출발지점과 가까운 OHT를 결정하는 스크립트 지원. OHT Dispatching.
        /// </summary>
        /// <returns></returns>
        private OHTNode DispatchOHT(Command command, Time simTime)
        {
            double optimalDistance = double.MaxValue;
            List<RailLineNode> optimalRoute = new List<RailLineNode>();
            OHTNode optimalOHT = null;

            List<OHTNode> candidateOHTs = null;
            RailPortNode fromPort = command.FromNode as RailPortNode;

            Vector3 fromPortPos = fromPort.PosVec3;

            Bay fromBay = fromPort.Line.Bay;

            if(fromBay != null)
            {
                // From Bay 내 Bumping OHT가 있다면 Bumping OHT 반환
                if (fromBay.DispatchableOHTs.Any())
                {
                    if (command.Reticle)
                        candidateOHTs = fromBay.DispatchableOHTs.FindAll(x => x.Reticle == true && x.DispatchedCommand == null);
                    else
                        candidateOHTs = fromBay.DispatchableOHTs.FindAll(x => x.Reticle == false && x.DispatchedCommand == null);

                    if (candidateOHTs.Count == 0)
                        return null;

                    optimalOHT = GetOptimalOHT(simTime, candidateOHTs, command);

                    return optimalOHT;
                }
            }
            else
            {
                optimalOHT = GetOptimalOHT(simTime, command.Fab.ReadyOHTs, command);
                double candidateRouteDistance = optimalOHT.CandidateRouteDistance;
                if (candidateRouteDistance <= 300000
                    || (candidateRouteDistance <= 400000 && (simTime - command.Time) >= 120)
                    || (candidateRouteDistance <= 500000 && (simTime - command.Time) >= 180)
                    || (candidateRouteDistance <= 600000 && (simTime - command.Time) >= 240))
                    return optimalOHT;
                else
                    return null;
            }

            return null;
            //// 없다면 제일 가까운 OHT로..
        }
        #endregion

        private OHTNode GetOptimalOHT(Time simTime, List<OHTNode> listOHT, Command command)
        {
            CandidateOHT[] listCandidateOHT = new CandidateOHT[listOHT.Count];

            Parallel.For(0, listOHT.Count, i =>
            {
                OHTNode candidateOHT = listOHT[i];

                List<RailLineNode> route;
                double stopAvailableLineDistance = 0;
                RailLineNode stopLine = null;
                List<RailLineNode> listStoppingLine = new List<RailLineNode>();
                candidateOHT.CurRailLine.GetStopAvailableDistance(simTime, candidateOHT, ref listStoppingLine, ref stopLine, ref stopAvailableLineDistance);

                if (stopLine == null)
                    return;

                if (candidateOHT.NodeState is OHT_STATE.MOVE_TO_UNLOAD || candidateOHT.NodeState is OHT_STATE.UNLOADING)
                    route = PathFinder.Instance.FindPath(candidateOHT.DestinationPort.Line, candidateOHT.DestinationPort.Distance, ((RailPortNode)command.FromNode).Line, ((RailPortNode)command.FromNode).Distance);
                else
                {
                    route = PathFinder.Instance.FindPath(stopLine, stopAvailableLineDistance, ((RailPortNode)command.FromNode).Line, ((RailPortNode)command.FromNode).Distance);

                    for (int j = 0; j < listStoppingLine.Count; j++)
                    {
                        RailLineNode stoppingline = listStoppingLine[j];
                        route.Insert(j, stoppingline);
                    }
                }

                CandidateOHT coht = new CandidateOHT(candidateOHT, route, route.Sum(x => x.Length));
                listCandidateOHT[i] = coht;
            });


            double optimalDistance = double.MaxValue;
            List<RailLineNode> optimalRoute = new List<RailLineNode>();
            OHTNode optimalOHT = null;

            foreach (CandidateOHT coht in listCandidateOHT)
            {
                if (optimalOHT == null)
                {
                    optimalRoute = coht.CandidateRoute;
                    optimalDistance = coht.CandidateDistance;
                    optimalOHT = coht.oht;
                }
                else if (coht.oht != null && optimalDistance > coht.CandidateDistance)
                {
                    optimalRoute = coht.CandidateRoute;
                    optimalDistance = coht.CandidateDistance;
                    optimalOHT = coht.oht;
                }
            }

            //foreach (OHTNode candidateOHT in listOHT)
            //{
            //    List<RailLineNode> route;
            //    double totalMovingDistance = double.MaxValue;
            //    double stopAvailableLineDistance = 0;
            //    RailLineNode stopLine = null;
            //    List<RailLineNode> listStoppingLine = new List<RailLineNode>();
            //    candidateOHT.CurRailLine.GetStopAvailableDistance(simTime, candidateOHT, ref listStoppingLine, ref stopLine, ref stopAvailableLineDistance);

            //    if (stopLine == null)
            //        continue;

            //    if (candidateOHT.NodeState is OHT_STATE.MOVE_TO_UNLOAD || candidateOHT.NodeState is OHT_STATE.UNLOADING)
            //        route = PathFinder.Instance.FindPath(candidateOHT.DestinationPort.Line, candidateOHT.DestinationPort.Distance, ((RailPortNode)command.FromNode).Line, ((RailPortNode)command.FromNode).Distance);
            //    else
            //    {
            //        route = PathFinder.Instance.FindPath(stopLine, stopAvailableLineDistance, ((RailPortNode)command.FromNode).Line, ((RailPortNode)command.FromNode).Distance);

            //        for (int i = 0; i < listStoppingLine.Count; i++)
            //        {
            //            RailLineNode stoppingline = listStoppingLine[i];
            //            route.Insert(i, stoppingline);
            //        }
            //    }

            //    if (((RailPortNode)command.FromNode).Line.Name != route.Last().Name)
            //        ;

            //    totalMovingDistance = route.Sum(x => x.Length);

            //    if (optimalOHT == null)
            //    {
            //        optimalRoute = route;
            //        optimalDistance = totalMovingDistance;
            //        optimalOHT = candidateOHT;
            //    }
            //    else if (optimalDistance > totalMovingDistance)
            //    {
            //        optimalRoute = route;
            //        optimalDistance = totalMovingDistance;
            //        optimalOHT = candidateOHT;
            //    }
            //}

            optimalOHT.CandidateRoute = optimalRoute;
            optimalOHT.CandidateRouteDistance = optimalDistance;

            return optimalOHT;
        }


        public struct CandidateOHT
        {
            public OHTNode oht;
            public List<RailLineNode> CandidateRoute;
            public double CandidateDistance;

            public CandidateOHT(OHTNode oht, List<RailLineNode> candidateRoute, double candidateDistance)
            {
                this.oht = oht;
                this.CandidateRoute = candidateRoute;
                this.CandidateDistance = candidateDistance;
            }
        }

        public OHTNode SelectOHT(Time simTime, SimNode toNode)
        {
            OHTNode ohtCandidate = null;
            double ohtCandidatePoint = 0;
            //놀고 있는 OHT 선택
            foreach (OHTNode oht in ModelManager.Instance.LstOHTNode)
            {
                if (oht.IsWorking == false && (OHT_STATE)oht.NodeState == OHT_STATE.IDLE)
                {
                    if (PathFinder.Instance.IsUsingCandidatePath && SimModelDBManager.Instance.IsCandidatePath())
                        oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);
                    else
                        oht.SetLstRailLine(PathFinder.Instance.GetPath(simTime, oht), false);

                    CompareWithDistanceForOHTAssign(ohtCandidate, ohtCandidatePoint, oht);
                }
            }

            return ohtCandidate;
        }

        private void CompareWithDistanceForOHTAssign(OHTNode ohtCandidate, double ohtCandidatePoint, OHTNode oht)
        {
            if (ohtCandidate == null) // 첫 OHT는 비교대상이 없음.
            {
                ohtCandidatePoint = oht.GetDistanceToDestination();
                ohtCandidate = oht;
            }
            else
            {
                double ohtPoint = oht.GetDistanceToDestination();

                //여기에 OHT Assign 비교 함수 넣으면 될 듯
                if (ohtPoint < ohtCandidatePoint)
                {
                    ohtCandidate.InitializeLstRailLine();
                    ohtCandidatePoint = ohtPoint;
                    ohtCandidate = oht;
                }
            }
        }

        public SimNode GetBuffer()
        {
            List<BufferNode> buffer = new List<BufferNode>();
            foreach (SimNode node in ModelManager.Instance.SimNodes.Values)
            {
                if (node is BufferNode)
                    buffer.Add((BufferNode)node);
            }

            buffer.Sort(compareBuffer);

            return buffer[buffer.Count - 1];
        }

        public SimNode GetSTB(Vector2 ohtPosition)
        {
            List<TBNode> stbs = new List<TBNode>();
            foreach (SimNode node in ModelManager.Instance.SimNodes.Values)
            {
                if (node is TBNode && (RAILPORT_STATE)((TBNode)node).NodeState == RAILPORT_STATE.EMPTY)
                    stbs.Add((TBNode)node);
            }

            this.ohtPosition = ohtPosition;
            stbs.Sort(compareSTB);

            if (stbs.Count != 0)
                return stbs[stbs.Count - 1];
            else
                return null;
        }

        private int compareBuffer(BufferNode x, BufferNode y)
        {
            //기준을 어떻게??
            return x.GetRemainCapacity().CompareTo(y.GetRemainCapacity());
        }

        private int compareSTB(TBNode x, TBNode y)
        {
            return x.GetDistance(ohtPosition).CompareTo(y.GetDistance(ohtPosition));
        }
    }
}
