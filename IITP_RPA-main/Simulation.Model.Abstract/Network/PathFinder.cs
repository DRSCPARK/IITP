using Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Simulation.Model.Abstract
{
    public class PathFinder
    {
        static PathFinder _instance;
        public Stopwatch pathFinderStopWatch;
        public Stopwatch candidatePathStopWatch;
        private float distanceWeight;
        private float ohtCountWeight;
        public int candidatePathUseCount;
        public int dijkstraUseCount;
        public int candidatePathSaveCount;
        private Dictionary<string, Dictionary<uint, Dictionary<uint, List<CandidatePath>>>> _candidatePaths;
        private Dictionary<string, Dictionary<uint, Dictionary<uint, List<CandidatePath>>>> _reticleCandidatePaths;

        public Dictionary<string, Dictionary<uint, Dictionary<uint, List<CandidatePath>>>> CandidatePaths
        {
            get { return _candidatePaths; }
        }

        public Dictionary<string, Dictionary<uint, Dictionary<uint, List<CandidatePath>>>> ReticleCandidatePaths
        {
            get { return _reticleCandidatePaths; }
        }

        public float DistanceWeight
        {
            get { return distanceWeight; }
        }

        public float OHTCountWeight
        {
            get { return ohtCountWeight; }
        }

        public bool IsUsingCandidatePath
        { get; set; }

        public bool IsSavingCandidatePath
        { get; set; }

        public static PathFinder Instance
        { get { return _instance; } }


        public PathFinder()
        {
            _instance = this;
            pathFinderStopWatch = new Stopwatch();
            candidatePathStopWatch = new Stopwatch();
            _candidatePaths = new Dictionary<string, Dictionary<uint, Dictionary<uint, List<CandidatePath>>>>();
            _reticleCandidatePaths = new Dictionary<string, Dictionary<uint, Dictionary<uint, List<CandidatePath>>>>();
            distanceWeight = 1;
            ohtCountWeight = 500;
            candidatePathUseCount = 0;
            dijkstraUseCount = 0;
            candidatePathSaveCount = 0;

        }

        public List<RailLineNode> GetPath(Time simTime, OHTNode oht )
        {
            RailLineNode stopLine = null;
            double stopAvailableLineDistance = 0;
            List<RailLineNode> listStoppingLine = new List<RailLineNode>();
            oht.CurRailLine.GetStopAvailableDistance(simTime, oht, ref listStoppingLine, ref stopLine, ref stopAvailableLineDistance);

            if (stopLine == null)
                return null;


            List<RailLineNode> path = FindPath(stopLine, stopAvailableLineDistance, oht.DestinationPort.Line, oht.DestinationPort.Distance);

            for (int i = 0; i < listStoppingLine.Count; i++)
            {
                RailLineNode stoppingline = listStoppingLine[i];
                path.Insert(i, stoppingline);
            }

            if (oht.CurRailLineName != path[0].Name)
                ;

            return path;
        }

        public List<RailLineNode> FindPath(RailLineNode curRailLine, double curDistance, RailLineNode destRailLine, double destDistance )
        {
            pathFinderStopWatch.Start();
            List<RailLineNode> lstRL = new List<RailLineNode>();

            RouteSelection routeSelection = null;
            if (IsRouteSelection(curRailLine, destRailLine, ref routeSelection))
            {
                lstRL = GetRouteSelectionCaseRouteByDijkstra(curRailLine, destRailLine, routeSelection);
            }
            else
            {
                lstRL = DijkstraPath(curRailLine, curDistance, destRailLine, destDistance);
            }


            pathFinderStopWatch.Stop();
            return lstRL;
        }

        private void SaveCandidatePath(List<RailLineNode> lstRL, Fab fab)
        {
            RailLineNode line = lstRL[0];

            while(line.ToNode.ToLines.Count == 1)
            {
                lstRL.Remove(line);
                if (line.ToNode.ToLines.Count > 0)
                    line = line.ToNode.ToLines[0];
            }
            lstRL.Remove(line);
            RailPointNode fromPoint = line.ToNode;

            if(lstRL.Count > 0)
            {
                line = lstRL[lstRL.Count - 1];
                while(line.FromNode.FromLines.Count == 1)
                {
                    lstRL.Remove(line);
                    line = line.FromNode.FromLines[0];
                }
                lstRL.Remove(line);
            }

            RailPointNode toPoint = line.FromNode;

            if (lstRL.Count == 0)
                return;

            CandidatePath candidatePath = new CandidatePath(fab, fromPoint, toPoint, false);
            candidatePath.RailLines = lstRL;

            bool IsSameCandidatePath = SimModelDBManager.Instance.IsSameCandidatePath(fab.Name, fromPoint.ID, toPoint.ID, candidatePath.Reticle, candidatePath.Length, candidatePath.GetRailLineIDs());
        
            if(!IsSameCandidatePath)
            {
                SimModelDBManager.Instance.UploadCandidatePath(fab.Name, candidatePath);
                candidatePathSaveCount++;
            }
        }

        public List<RailLineNode> DijkstraPath(RailLineNode fromLine, double fromDistance, RailLineNode toLine, double toDistance)
        {
            List<RailLineNode> shortestLines = new List<RailLineNode>();
            List<RailPointNode> shortestPoints = new List<RailPointNode>();
            NetworkLineNode nwLine = null;
            bool isSavingLine = false;

            if (fromLine == toLine) //출발지와 목적지가 같은 line에 있는 경우
            {
                if (fromDistance <= toDistance) //출발지가 목적지를 지나가지 않은 경우
                {
                    shortestLines.Add(fromLine);
                }
                else //출발지가 목적지를 지나간 경우 (일반적인 경우와 같음)
                {
                    shortestLines = GetOrdinaryCaseRouteByDijkstra(fromLine, toLine);
                }
            }
            //출발지와 목적지 사이에 point 한개만 있는 경우     -----x1------->o--------x2------>
            else if (fromLine.ToNode == toLine.FromNode)
            {
                shortestLines.Add(fromLine);
                shortestLines.Add(toLine);
                shortestPoints.Add(fromLine.OutLinks[0].EndNode as RailPointNode);
            }
            else if(IsInOrderWithSameNWLine(fromLine, toLine, ref nwLine))
            {
                foreach(RailLineNode rl in nwLine.RailLines)
                {
                    if(rl.Name == fromLine.Name)
                    {
                        isSavingLine = true;
                        shortestLines.Add(rl);
                    }
                    else if(isSavingLine)
                    {
                        shortestLines.Add(rl);

                        if (rl.Name == toLine.Name)
                            break;
                    }
                }
            }
            else //일반적인 경우
            {
                shortestLines = GetOrdinaryCaseRouteByDijkstra(fromLine, toLine);
            }

            return shortestLines;
        }

        public List<RailLineNode> TestDijkstraPath(RailLineNode fromLine, double fromDistance, RailLineNode toLine, double toDistance)
        {

            Stopwatch sw = new Stopwatch();

            sw.Start();

            List<RailLineNode> shortestLines = new List<RailLineNode>();
            List<RailPointNode> shortestPoints = new List<RailPointNode>();
            NetworkLineNode nwLine = null;
            bool isSavingLine = false;

            if (fromLine == toLine) //출발지와 목적지가 같은 line에 있는 경우
            {
                if (fromDistance <= toDistance) //출발지가 목적지를 지나가지 않은 경우
                {
                    shortestLines.Add(fromLine);
                }
                else //출발지가 목적지를 지나간 경우 (일반적인 경우와 같음)
                {
                    shortestLines = GetOrdinaryCaseRouteByDijkstra(fromLine, toLine);
                }
            }
            //출발지와 목적지 사이에 point 한개만 있는 경우     -----x1------->o--------x2------>
            else if (fromLine.ToNode == toLine.FromNode)
            {
                shortestLines.Add(fromLine);
                shortestLines.Add(toLine);
                shortestPoints.Add(fromLine.OutLinks[0].EndNode as RailPointNode);
            }
            else if (IsInOrderWithSameNWLine(fromLine, toLine, ref nwLine))
            {
                foreach (RailLineNode rl in nwLine.RailLines)
                {
                    if (rl.Name == fromLine.Name)
                    {
                        isSavingLine = true;
                        shortestLines.Add(rl);
                    }
                    else if (isSavingLine)
                    {
                        shortestLines.Add(rl);

                        if (rl.Name == toLine.Name)
                            break;
                    }
                }
            }
            else //일반적인 경우
            {
                shortestLines = GetOrdinaryCaseRouteByDijkstra(fromLine, toLine);
            }

            sw.Stop();

            Console.WriteLine("DIJKSTRA ELAPSED TIME ::" + sw.Elapsed.TotalMilliseconds);
           

            return shortestLines;
        }


        private bool IsInOrderWithSameNWLine(RailLineNode fromLine, RailLineNode toLine, ref NetworkLineNode nwLine)
        {
            foreach(NetworkLineNode nl in fromLine.NWLines)
            {
                if(toLine.NWLines.Contains(nl))
                {
                    int fromLineSequence = -1;
                    int toLineSequence = -1;
                    for(int i = 0; i < nl.RailLines.Count(); i++) 
                    {
                        RailLineNode line = nl.RailLines[i];

                        if (line.Name == fromLine.Name)
                            fromLineSequence = i;
                        else if (line.Name == toLine.Name)
                            toLineSequence = i;
                    }

                    if(fromLineSequence < toLineSequence)
                    {
                        nwLine = nl;

                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRouteSelection(RailLineNode fromLine, RailLineNode toLine, ref RouteSelection routeSelection)
        {
            Fab fab = fromLine.Fab;
            foreach(RouteSelection rs in fab.RouteSelections)
            {
                if((rs.FromBay.Lines.Contains(fromLine)|| rs.FromBay.ToLines.Contains(fromLine)) 
                    && (rs.ToBay.Lines.Contains(toLine) || rs.ToBay.FromLines.Contains(toLine)))
                {
                    routeSelection = rs;
                    return true;
                }
            }

            return false;
        }

        private List<RailLineNode> GetOrdinaryCaseRouteByDijkstra(RailLineNode fromLine, RailLineNode toLine)
        {
            List<RailLineNode> shortestLines = new List<RailLineNode>();
            RailPointNode startPointNode = fromLine.ToNode;
            List<RailLineNode> fromLines = new List<RailLineNode>();
            fromLines.Add(fromLine);
            while (startPointNode.NWPoint == null)
            {
                fromLines.Add(startPointNode.ToLines[0]);
                startPointNode = startPointNode.ToLines[0].ToNode;
            }
            Dictionary<double, List<RailLineNode>> dicCandidateRoutes = new Dictionary<double, List<RailLineNode>>();
            foreach (NetworkLineNode nwLine in toLine.NWLines)
             {
                Dijkstra di = new Dijkstra();
                double score = double.MaxValue;
                List<RailLineNode> toLines = new List<RailLineNode>();
                RailPointNode endPointNode = nwLine.RailLines[0].FromNode;
                foreach (RailLineNode railLineNodeInNWLine in nwLine.RailLines)
                {
                    toLines.Add(railLineNodeInNWLine);
                    if (railLineNodeInNWLine == toLine)
                        break;
                }
                List<RailLineNode> shortestLinesTemp = new List<RailLineNode>();
                List<RailPointNode> shortestPoints = new List<RailPointNode>();
                di.DijkstraOptimalPath(startPointNode, endPointNode, null, shortestPoints, shortestLinesTemp, ref score);
                int i = 0;
                foreach (RailLineNode fromLineTemp in fromLines)
                {
                    shortestLinesTemp.Insert(i, fromLineTemp);
                    score = score + fromLineTemp.TotalCost;
                    i++;
                }
                foreach (RailLineNode toLineTemp in toLines)
                {
                    shortestLinesTemp.Add(toLineTemp);
                    score = score + toLineTemp.TotalCost;
                }
                dicCandidateRoutes[score] = shortestLinesTemp;
            }
            var CandidateRoutes = dicCandidateRoutes.OrderBy(x => x.Key);
            shortestLines = CandidateRoutes.First().Value;
            return shortestLines; 
        }

        //2021.03.31 transitNode Pair Dijkstra 경로 찾기
        //public List<RailLineNode> GetTranistNodeRouteByDijkstra(RailLineNode outerPointLine, RailLineNode innerPointLine)
        //{
        //    List<RailLineNode> shortestLines = new List<RailLineNode>();
        //    RailPointNode startPointNode = outerPointLine.ToNode;
        //    List<RailLineNode> fromLines = new List<RailLineNode>();
        //    fromLines.Add(outerPointLine);

        //    while (startPointNode.NWPoint == null)
        //    {
        //        fromLines.Add(startPointNode.ToLines[0]);
        //        startPointNode = startPointNode.ToLines[0].ToNode;
        //    }

        //    Dictionary<double, List<RailLineNode>> dicCandidateRoutes = new Dictionary<double, List<RailLineNode>>();
        //    foreach (NetworkLineNode nwLine in innerPointLine.NWLines)
        //    {
        //        Dijkstra di = new Dijkstra();
        //        double score = double.MaxValue;
        //        List<RailLineNode> toLines = new List<RailLineNode>();
        //        RailPointNode endPointNode = nwLine.RailLines[0].FromNode;
        //        foreach (RailLineNode railLineNodeInNWLine in nwLine.RailLines)
        //        {
        //            toLines.Add(railLineNodeInNWLine);
        //            if (railLineNodeInNWLine == innerPointLine)
        //                break;
        //        }

                  
        //        List<RailLineNode> shortestLinesTemp = new List<RailLineNode>();
        //        List<RailPointNode> shortestPoints = new List<RailPointNode>();
        //        di.DijkstraOptimalPath(startPointNode, endPointNode, null, shortestPoints, shortestLinesTemp, ref score);
        //        int i = 0;
        //        foreach (RailLineNode fromLineTemp in fromLines)
        //        {
        //            shortestLinesTemp.Insert(i, fromLineTemp);
        //            score = score + fromLineTemp.TotalCost;
        //            i++;
        //        }
        //        foreach (RailLineNode toLineTemp in toLines)
        //        {
        //            shortestLinesTemp.Add(toLineTemp);
        //            score = score + toLineTemp.TotalCost;
        //        }
        //        dicCandidateRoutes[score] = shortestLinesTemp;
        //    }   

        //    var CandidateRoutes = dicCandidateRoutes.OrderBy(x => x.Key);
        //    shortestLines = CandidateRoutes.First().Value;
        //    return shortestLines;
        //}

        ////2021.03.31 transitNode Pair Dijkstra 경로 찾기

        public List<RailLineNode> GetRouteSelectionCaseRouteByDijkstra(RailLineNode fromLine, RailLineNode toLine, RouteSelection rs)
        {
            List<RailLineNode> shortestRoute = new List<RailLineNode>();
            //startLine에서 FromBayOutLine까지 구하고
            List<RailLineNode> startLineToFromBayOutLineRoute = DijkstraPath(fromLine, 0, rs.OutLine, 0);
            shortestRoute.AddRange(startLineToFromBayOutLineRoute);

            // 처음엔 FromBayOutLine부터 viaPoint ToLine까지
            // viaPoint전 Line부터 다음 viaPoint ToLine까지
            // 마지막엔 viaPoint전 Line부터 ToBayInLine까지
            //(중복 제거를 위해 시작과 끝 라인 삭제)

            RailLineNode viaFromLine = rs.OutLine;
            RailLineNode viaToLine = null;

            for(int i = 0; i < rs.ViaPoints.Count; i++) 
            {
                RailPointNode viaPoint = rs.ViaPoints.ElementAt(i).Value;
                viaToLine = viaPoint.ToLines[0];

                List<RailLineNode> viaRoute = DijkstraPath(viaFromLine, 0, viaToLine, 0);
                viaRoute.Remove(viaFromLine);
                shortestRoute.AddRange(viaRoute);
                viaFromLine = viaToLine;
            }

            viaToLine = rs.InLine;
            List<RailLineNode> lastViaRoute = DijkstraPath(viaFromLine, 0, viaToLine, 0);
            lastViaRoute.Remove(viaFromLine);
            shortestRoute.AddRange(lastViaRoute);


            //ToLine의 ToNode부터 EndPointNode까지
            List<RailLineNode> ToBayInLineToEndLineRoute = DijkstraPath(rs.InLine, 0, toLine, 0);
            ToBayInLineToEndLineRoute.Remove(rs.InLine);
            shortestRoute.AddRange(ToBayInLineToEndLineRoute);

            return shortestRoute;
        }


        public bool KShortestPath(RailLineNode fromLine, double fromDistance, RailLineNode toLine, double toDistance, ref List<RailLineNode> optimalLines, bool reticle = false)
        {
            bool isSuccess = true;
            candidatePathStopWatch.Start();

            List<RailLineNode> fromLinesTemp = new List<RailLineNode>();

            RailPointNode startPointNode = fromLine.ToNode as RailPointNode;
            while (startPointNode.OutLinks.Count == 1)  //분기 Point 기준으로 Candidated Path가 있어서 분기 Point를 찾는 while 문
            {
                fromLinesTemp.Add((RailLineNode)startPointNode.ToLines[0]);
                startPointNode = (RailPointNode)startPointNode.ToLines[0].ToNode;

                if ((RailLineNode)startPointNode.ToLines[0] == toLine) // 분기가 없어서 쭉 가다가 경로 찾은 경우
                {
                    fromLinesTemp.Add((RailLineNode)startPointNode.ToLines[0]);
                    fromLinesTemp.Insert(0, fromLine);
                    optimalLines = fromLinesTemp;
                    return isSuccess;
                }
            }

            List<RailLineNode> toLinesTemp = new List<RailLineNode>();

            RailPointNode endPointNode = toLine.FromNode; //도착 Port가 있는 Line의 From Point
            while (endPointNode.FromLines.Count == 1) //분기 Point 기준으로 Candidated Path가 있어서 분기 Point를 찾는 while 문
            {
                toLinesTemp.Insert(0, (RailLineNode)endPointNode.FromLines[0]);
                endPointNode = (RailPointNode)endPointNode.FromLines[0].FromNode;
            }

            if (fromLine == toLine) //출발지와 목적지가 같은 line에 있는 경우


            {
                if (fromDistance <= toDistance) //출발지가 목적지를 지나가지 않은 경우, OK
                {
                    optimalLines.Add(fromLine);
                }
                else //출발지가 목적지를 지나간 경우 (일반적인 경우와 같음) , OK
                {
                    optimalLines = GetOptimalPath(startPointNode, endPointNode);

                    //모아놓았던 From, To Line들을 추가해주는 함수 필요 
                    for (int i = 0; i < fromLinesTemp.Count; i++)
                        optimalLines.Insert(i, fromLinesTemp[i]);

                    for (int i = 0; i < toLinesTemp.Count; i++)  // 뒤쪽 라인부터 저장되어 있음.
                        optimalLines.Add(toLinesTemp[i]);

                    optimalLines.Insert(0, fromLine);
                    optimalLines.Add(toLine);
                }
            }
            //출발지와 목적지 사이에 point 한개만 있는 경우     -----x1------->o--------x2------>
            else if (fromLine.FromNode == toLine.FromNode)
            {
                optimalLines .Add(fromLine);
                optimalLines.Add(toLine);
            }
            else //일반적인 경우
            {
                if (startPointNode != endPointNode)
                    optimalLines  = GetOptimalPath(startPointNode, endPointNode);

                //모아놓았던 From, To Line들을 추가해주는 함수 필요 
                for (int i = 0; i < fromLinesTemp.Count; i++)
                    optimalLines.Insert(i, fromLinesTemp[i]);

                for (int i = 0; i < toLinesTemp.Count; i++)
                    optimalLines .Add(toLinesTemp[i]);

                optimalLines .Insert(0, fromLine);
                optimalLines .Add(toLine);
            }

            candidatePathStopWatch.Stop();
            candidatePathUseCount++;
            return isSuccess;
        }

        public List<RailLineNode> DownloadOptimalPath(RailPointNode fromNode, RailPointNode toNode)
        {
            try
            {
                List<CandidatePath> listCandidatePath = SimModelDBManager.Instance.CreateCandidatePaths(fromNode.Fab.Name, fromNode.ID, toNode.ID);

                IEnumerable<CandidatePath> sort =
                    from can in listCandidatePath
                    orderby can.Length
                    select can;

                return sort.ToList()[0].RailLines.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DownloadOptimalPath, FromNode: " + fromNode.ID + " toNode: " + toNode.ID + " Message: " + ex.Message);
                return null;
            }
        }

        public List<RailLineNode> GetOptimalPath(RailPointNode fromNode, RailPointNode toNode)
        {
            try
            {
                List<CandidatePath> listCandidatePath = _candidatePaths[fromNode.Fab.Name][fromNode.ID][toNode.ID];

                IEnumerable<CandidatePath> sort =
                    from can in listCandidatePath
                    orderby can.Length
                    select can;

                if (sort.ToList().Count == 0)
                    return null;
                else
                {
                    return sort.ToList()[0].RailLines.ToList();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DownloadOptimalPath, FromNode: " + fromNode.ID + " toNode: " + toNode.ID + " Message: " + ex.Message);
                return null;
            }
        }

        public void RemoveCandidatePaths()
        {
            foreach(Dictionary<uint, Dictionary<uint, List<CandidatePath>>> fabCandidatePaths in _candidatePaths.Values)
            {
                foreach(Dictionary<uint, List<CandidatePath>> fromCandidatePaths in fabCandidatePaths.Values)
                {
                    foreach(List<CandidatePath> lstCandidatePaths in fromCandidatePaths.Values)
                    {
                        lstCandidatePaths.Clear();
                    }
                }
            }

            SimModelDBManager.Instance.DeleteAll(TABLE_TYPE.CANDIDATE_PATH);
        }

        public bool IsNewBetterPath(List<RailLineNode> oldLines, List<RailLineNode> newLines, out double oldPathScore, out double newPathScore, out bool isSamePath)
        {
            bool newIsBetter = false;
            isSamePath = true;

            oldPathScore = GetRouteScore(oldLines);
            newPathScore = GetRouteScore(newLines);

            if(oldLines.Count == newLines.Count)
            {
                for (int i = 0; i < oldLines.Count; i++)
                {
                    if(oldLines[i].Name != newLines[i].Name)
                    {
                        isSamePath = false;
                        break;
                    }
                }
            }

            if (oldPathScore > newPathScore + ModelManager.Instance.ReroutingIndicator)
                return true;
            else
                return false;
        }

        private double GetRouteScore(List<RailLineNode> lines)
        {
            return lines.Sum(x => x.TotalCost);
        }

        #region Finding CandidatePath

        public void InitializeCandidatePaths()
        {
            _candidatePaths = new Dictionary<string, Dictionary<uint, Dictionary<uint, List<CandidatePath>>>>();

            foreach(Fab fab in ModelManager.Instance.Fabs.Values)
            {
                Dictionary<uint, Dictionary<uint, List<CandidatePath>>> fromCandidatePaths = new Dictionary<uint, Dictionary<uint, List<CandidatePath>>>();

                foreach(RailPointNode fromNode in fab.DicRailPoint.Values)
                {
                    Dictionary<uint, List<CandidatePath>> dic = new Dictionary<uint, List<CandidatePath>>();

                    if (fromNode.OutLinks.Count > 1)
                        fromCandidatePaths.Add(fromNode.ID, dic);
                    else
                        continue;

                    foreach(RailPointNode toNode in fab.DicRailPoint.Values)
                    {
                        if(fromNode != toNode && toNode.InLinks.Count> 1)
                        {
                            List<CandidatePath> candidateRoutesByFromToNode = new List<CandidatePath>();
                            dic.Add(toNode.ID, candidateRoutesByFromToNode);
                        }
                    }
                }

                _candidatePaths.Add(fab.Name, fromCandidatePaths);
            }
        }
        public void CalShortestPathforSave(string fabName, int candidatePathCount)
        {
            int threadCount = 8;

            int parallelCalculationCount = ModelManager.Instance.Fabs[fabName].DicRailPoint.Count / threadCount;
            CandidateThread[] ctps = new CandidateThread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(AddCandidatePathByThread));

                CandidateThread ctp;
                if (i < threadCount - 1)
                    ctp = new CandidateThread(fabName, parallelCalculationCount * i, parallelCalculationCount * (i + 1) - 1, candidatePathCount, thread);
                else
                    ctp = new CandidateThread(fabName, parallelCalculationCount * i, ModelManager.Instance.Fabs[fabName].DicRailPoint.Count - 1, candidatePathCount, thread);

                ctps[i] = ctp;
                thread.Start(ctp);
            }

            foreach (CandidateThread ctp in ctps)
            {
                ctp.thread.Join();
            }
        }

        //public double GetPathScore(CandidatePath c)
        //{
        //    return c.Length;
        //}

        /// <summary>
        /// CandidatePath를 Dijkstra로 찾아서 추가하는 함수
        /// </summary>
        /// <param name="candidateThreadParameter"></param>
        public void AddCandidatePathByThread(object candidateThreadParameter)
        {
            string fabName = ((CandidateThread)candidateThreadParameter).fabName;
            int startPoint = ((CandidateThread)candidateThreadParameter).startPoint;
            int endPoint = ((CandidateThread)candidateThreadParameter).endPoint;
            int candidatePathCount = ((CandidateThread)candidateThreadParameter).candidatePathCount;
            bool isDone = ((CandidateThread)candidateThreadParameter).isDone;

            Fab fab = ModelManager.Instance.Fabs[fabName];

            for (; startPoint <= endPoint; startPoint++)
            {
                RailPointNode start = fab.DicRailPoint.ElementAt(startPoint).Value;

                if (start.ToLines.Count > 1)
                {
                    for (int j = 0; j < fab.DicRailPoint.Count(); j++)
                    {
                        RailPointNode end = fab.DicRailPoint.ElementAt(j).Value;

                        if (end.FromLines.Count > 1)
                        {
                            Dijkstra dijkstra = new Dijkstra();
                            List<CandidatePath> candidatePathsForFromToNode = dijkstra.FindCandidatePaths(start, end, candidatePathCount);

                            foreach (CandidatePath path in candidatePathsForFromToNode)
                                SimModelDBManager.Instance.UploadCandidatePath(fabName, path);
                        }
                    }
                }
            }

            isDone = true;
        }

        public CandidatePath AddCandidatePath(string fabName, uint fromPointID, uint toPointID, bool reticle, string candidatePath)
        {
            Fab fab = ModelManager.Instance.Fabs[fabName];
            RailPointNode fromPoint = (RailPointNode)ModelManager.Instance.SimNodes[fromPointID];
            RailPointNode toPoint = (RailPointNode)ModelManager.Instance.SimNodes[toPointID];
            CandidatePath path = new CandidatePath(fab, fromPoint, toPoint, reticle, candidatePath);

            return path;
        }

        public void AddCandidatePathByMemory(string fabName, uint fromPointID, uint toPointID, bool reticle, string candidatePath)
        {
            Fab fab = ModelManager.Instance.Fabs[fabName];
            RailPointNode fromPoint = (RailPointNode)ModelManager.Instance.SimNodes[fromPointID];
            RailPointNode toPoint = (RailPointNode)ModelManager.Instance.SimNodes[toPointID];
            CandidatePath path = new CandidatePath(fab, fromPoint, toPoint, reticle, candidatePath);

            PathFinder.Instance.CandidatePaths[fabName][fromPointID][toPointID].Add(path);
        }

        private class CandidateThread
        {
            public string fabName;
            public int startPoint;
            public int endPoint;
            public int candidatePathCount;
            public bool isDone;
            public Thread thread;

            public CandidateThread(string fabName, int startPoint, int endPoint, int candidatePathCount, Thread thread)
            {
                this.fabName = fabName;
                this.startPoint = startPoint;
                this.endPoint = endPoint;
                this.candidatePathCount = candidatePathCount;
                this.thread = thread;
            }
        }

        #endregion





    }
}
