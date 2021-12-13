
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    /// <summary>
    /// 거리 기준으로 K개의 Shortest Path를 찾아주는 클래스
    /// FromNode to ToNode 기준으로 K-개만큼 찾음.
    /// </summary>
    public class Dijkstra
    {

        #region Member Variable

        /* Variables for Dijkstra algorithm */
        private Dictionary<uint, NetworkPointNode> _pointNodes = new Dictionary<uint, NetworkPointNode>(); // Key RailPointNode
        private ReachNodeList _reachableNodes;
        //병렬 계산을 위해 selectedLine을 Dijkstra 클래스 변수로 추가
        private Dictionary<uint, NetworkLineNode> _selectedLines; //Key NetworkPointID
        private Dictionary<uint, float> _pointTotalCost; //Key NetworkPointID
        private Dictionary<uint, bool> _lineSpurState; //Key NetworkLineID
        #endregion

        public Dijkstra()
        {
            _pointNodes = new Dictionary<uint, NetworkPointNode>();
            _reachableNodes = new ReachNodeList();
            //            _selectedLines = new Dictionary<NetworkPointNode, NetworkLineNode>();
            //            _pointTotalCost = new Dictionary<NetworkPointNode, float>();
            //            _lineDistanceScores = new Dictionary<NetworkLineNode, float>();

            _selectedLines = new Dictionary<uint, NetworkLineNode>();
            _pointTotalCost = new Dictionary<uint, float>();
            _lineSpurState = new Dictionary<uint, bool>();

            //Point, Line 수만큼 생성 및 초기화
            foreach (NetworkPointNode point in ModelManager.Instance.NWPoints.ToArray())
                _pointTotalCost.Add(point.ID, -1);

            foreach (NetworkLineNode line in ModelManager.Instance.lstNWLines.ToArray())
                _lineSpurState.Add(line.ID, false);
        }

        #region Dijkstra algorithm shortest path
        //public Dictionary<int, List<RailLineNode>> FindKshortestPath(RailPointNode fromNode, RailPointNode toNode, int ShortestPathCount)
        //{
        //    List<CandidatePath> candidatePaths = new List<CandidatePath>();
        //    List<RailLineNode> shortestLines = new List<RailLineNode>();
        //    List<RailPointNode> shortestPoints = new List<RailPointNode>();

        //    double totalScore = double.MaxValue;
        //    DijkstraOptimalPath(fromNode, toNode, null, shortestPoints, shortestLines, ref totalScore);

        //    CandidatePath candidatePath = new CandidatePath(fromNode.Fab, fromNode, toNode, false);
        //    candidatePath.RailLines = shortestLines;
        //    candidatePath.RailPoints = shortestPoints;

        //    Dictionary<int, List<RailLineNode>> DicCandidatePath = new Dictionary<int, List<RailLineNode>>();
        //    List<RailLineNode> kShortestPath = new List<RailLineNode>();
        //    List<RailLineNode> extraPath = new List<RailLineNode>();
        //    int candidatePathCount = 1;

        //    candidatePaths.Add(candidatePath);

        //    DicCandidatePath.Add(candidatePathCount, candidatePath.RailLines);

        //    shortestPoints = candidatePath.RailPoints;
        //    shortestLines = candidatePath.RailLines;

        //    //k번째 Shortest Path 인근 Candidate 후보들 전부 찾기
        //    foreach (var shortestPoint in shortestPoints)
        //    {
        //        candidatePathCount++;
        //        extraPath.Clear();

        //        foreach (var toLine in shortestPoint.ToLines)
        //        {
        //            if (shortestLines.Contains(toLine)) continue;

        //            kShortestPath = PathFinder.Instance.DijkstraPath(toLine, 0, toNode.FromLines[0], toNode.FromLines[0].Length);

        //            foreach (var shortestLine in shortestLines)
        //            {
        //                extraPath.Add(shortestLine);
        //                if (shortestLine.FromNode == shortestPoint) break;

        //            }
        //            kShortestPath.InsertRange(0, extraPath);
        //            kShortestPath.Distinct().ToList();
        //        }

        //        DicCandidatePath.Add(candidatePathCount, kShortestPath);

        //    }
        //    return DicCandidatePath;

        //}

        public List<CandidatePath> FindCandidatePaths(RailPointNode fromNode, RailPointNode toNode, int ShortestPathCount)
        {
            List<CandidatePath> candidatePaths = new List<CandidatePath>();
            List<RailLineNode> shortestLines = new List<RailLineNode>();
            List<RailPointNode> shortestPoints = new List<RailPointNode>();
            double totalScore = double.MaxValue;
            DijkstraOptimalPath(fromNode, toNode, null, shortestPoints, shortestLines, ref totalScore);
            CandidatePath candidatePath = new CandidatePath(fromNode.Fab, fromNode, toNode, false);
            candidatePath.RailLines = shortestLines;
            candidatePath.RailPoints = shortestPoints;

            candidatePaths.Add(candidatePath);

            for (int i = 0; i < ShortestPathCount; i++)
            {
                if (candidatePaths.Count == i)
                    return candidatePaths;

                candidatePath = candidatePaths[i];
                shortestPoints = candidatePath.RailPoints;
                shortestLines = candidatePath.RailLines;

                //k번째 Shortest Path 인근 Candidate 후보들 전부 찾기
                for (int j = 0; j < shortestLines.Count; j++)
                {
                    List<RailLineNode> shortestLinesTemp = shortestLines.ToList();
                    List<RailPointNode> shortestPointsTemp = shortestPoints.ToList();

                    for (int k = shortestLines.Count - 1; k >= j; k--)
                    {
                        shortestLinesTemp.RemoveAt(k);
                        shortestPointsTemp.RemoveAt(k + 1);
                    }

                    RailPointNode spurNode = shortestPointsTemp[shortestPointsTemp.Count - 1];
                    shortestPointsTemp.Remove(spurNode);

                    //DistanceScore 설정, spurNode 다음 Link 중 선택되었던 Link들은 거리점수 최대로, 나머지는 실제 거리
                    SetDistanceScoreAroundSpurNodes(shortestPoints[j], candidatePaths);

                    List<RailLineNode> spurLinks = new List<RailLineNode>();
                    List<RailPointNode> spurNodes = new List<RailPointNode>();

                    //RootNode들을 제외한 경로 설정
                    bool findPath = DijkstraOptimalPath(spurNode, toNode, shortestPointsTemp, spurNodes, spurLinks, ref totalScore);

                    if (!findPath)
                        continue;

                    shortestLinesTemp.AddRange(spurLinks);
                    shortestPointsTemp.AddRange(spurNodes);

                    bool isSamePath = false;

                    foreach (var candidateRouteTemp in candidatePaths)
                    {
                        isSamePath = CheckListEquals(candidateRouteTemp.RailPoints, shortestPointsTemp);

                        if (isSamePath)
                            break;
                    }

                    if (!isSamePath)
                    {
                        candidatePath = new CandidatePath(fromNode.Fab, fromNode, toNode, false);
                        candidatePath.RailLines = shortestLinesTemp;
                        candidatePath.RailPoints = shortestPointsTemp;
                        candidatePaths.Add(candidatePath);
                    }
                }

                candidatePaths = candidatePaths.OrderBy(C => C.Length).ToList();
            }

            while (candidatePaths.Count > ShortestPathCount)
            {
                candidatePaths.RemoveAt(ShortestPathCount);
            }

            return candidatePaths;
        }

        public bool DijkstraOptimalPath(RailPointNode fromNode, RailPointNode toNode, List<RailPointNode> rootPoints, List<RailPointNode> shortestPoints, List<RailLineNode> shortestLines, ref double totalScore)
        {
            List<NetworkLineNode> lstNWLine = new List<NetworkLineNode>();
            List<NetworkPointNode> lstNWPoint = new List<NetworkPointNode>();
            Dictionary<uint, NetworkPointNode> networkRootPoints = new Dictionary<uint, NetworkPointNode>();

            if (rootPoints != null)
            {
                foreach (RailPointNode pointNode in rootPoints)
                    networkRootPoints.Add(pointNode.NWPoint.ID, pointNode.NWPoint);
            }

            bool isPath = DijkstraOptimalPath(fromNode.NWPoint, toNode.NWPoint, networkRootPoints, lstNWPoint, lstNWLine, ref totalScore);

            if (isPath != true)
                return false;

            for (int i = 0; i < lstNWPoint.Count; i++)
                shortestPoints.Add(lstNWPoint[i].CoreNode as RailPointNode);

            for (int i = 0; i < lstNWLine.Count; i++)
                shortestLines.AddRange(lstNWLine[i].RailLines);

            return true;
        }

        public bool DijkstraOptimalPath(NetworkPointNode fromNode, NetworkPointNode toNode, Dictionary<uint, NetworkPointNode> rootPoints, List<NetworkPointNode> shortestPoints, List<NetworkLineNode> shortestLines, ref double totalScore)
        {
            _pointNodes.Clear();
            _reachableNodes.Clear();

            NetworkPointNode crtNode = toNode;

            _pointTotalCost[crtNode.ID] = 0;
            _pointNodes.Add(crtNode.ID, crtNode);

            ReachNode crtReachNode;

            while (crtNode != fromNode)
            {
                
                AddReachableNodes(crtNode, _pointNodes, rootPoints, _reachableNodes);

                if (_reachableNodes.ReachableNodes.Count == 0)
                    return false;

                crtReachNode = _reachableNodes.GetMinReachNodeByTotalCost();
                _reachableNodes.RemoveReachableNode(crtReachNode);
                crtNode = crtReachNode._networkPoint;
                uint crtNodeID = crtNode.ID;

                if (_selectedLines.ContainsKey(crtNodeID))
                    _selectedLines[crtNodeID] = crtReachNode._networkLine;
                else
                    _selectedLines.Add(crtNodeID, crtReachNode._networkLine);

                _pointNodes.Add(crtNodeID, crtNode);
            }

            crtNode = fromNode;

            totalScore = 0;
            totalScore = _pointTotalCost[fromNode.ID];

            while (crtNode != toNode)
            {
                shortestPoints.Add(crtNode);
                shortestLines.Add(_selectedLines[crtNode.ID]);
                crtNode = GetToNeighbour(crtNode);

            }
            if (crtNode != null)
                shortestPoints.Add(crtNode);

            InitializeNetwork();

            return true;
        }

        private void InitializeNetwork()
        {
            foreach (var key in _pointTotalCost.Keys.ToList())
            {
                _pointTotalCost[key] = -1;
            }
        }
        private void AddReachableNodes(NetworkPointNode crtNode, Dictionary<uint, NetworkPointNode> resultNodes, Dictionary<uint, NetworkPointNode> rootPoints, ReachNodeList reachableNodes)
        {
            NetworkPointNode neighbourNode;
            ReachNode reachNode;

            float crtCost = _pointTotalCost[crtNode.ID];
            foreach (NetworkLineNode line in crtNode.inLines)
            {
                if (_lineSpurState[line.ID] == true)
                    continue;
                else if (!line.IsAvailable)
                    continue;
                neighbourNode = GetNeighbour(crtNode, line);

                if (neighbourNode == null) //들어오는 라인의 startPoint만 통과
                    continue;

                if (rootPoints != null && rootPoints.ContainsKey(neighbourNode.ID)) //이미 가기로 정해져있는 Root 경로를 제외한 탐색
                    continue;

                if (!_selectedLines.ContainsKey(crtNode.ID) || neighbourNode != GetNeighbour(crtNode, _selectedLines[crtNode.ID]))
                {
                    if (!resultNodes.ContainsKey(neighbourNode.ID))
                    {
                        if (reachableNodes.HasNode(neighbourNode))
                        {
                            if (crtCost + line.TotalCost < _pointTotalCost[neighbourNode.ID])
                            {
                                reachNode = reachableNodes.GetReachableNodeFromNode(neighbourNode);
                                reachNode._networkLine = line;
                                reachNode.TotalCost = _pointTotalCost[neighbourNode.ID] = crtCost + line.TotalCost;
                            }
                        }
                        else
                        {
                            reachNode = new ReachNode(neighbourNode, line);
                            reachableNodes.AddReachableNode(reachNode);
                            reachNode.TotalCost = _pointTotalCost[neighbourNode.ID] = crtCost + line.TotalCost;
                        }
                    }
                }
            }
        }

        private NetworkPointNode GetToNeighbour(NetworkPointNode node)
        {
            if (_selectedLines[node.ID].FromNode == node)
                return _selectedLines[node.ID].ToNode;
            else
                return null;// link.FromNode;
        }

        private NetworkPointNode GetNeighbour(NetworkPointNode node, NetworkLineNode line)
        {
            if (line.FromNode == node || _lineSpurState[line.ID] == true)
                return null;//link.ToNode;
            else
                return line.FromNode;
        }

        private void UpdateReachNodesTotalCost(NetworkPointNode node, ReachNodeList reachableNodes)
        {
            float crtCost = _pointTotalCost[node.ID];
            float minCost = crtCost;
            foreach (ReachNode reachNode in reachableNodes.ReachableNodes)
            {
                if (reachNode.TotalCost < minCost)
                    minCost = (float)reachNode.TotalCost;
            }
            _pointTotalCost[node.ID] = minCost;
        }



        /// <summary>
        /// 방문했던 Line들에 가지 않기 위해 방문 Line들의 거리점수를 최대값으로 바꾸는 함수
        /// </summary>
        /// <param name="spurNode"></param>
        /// <param name="candidatePaths"></param>
        private void SetDistanceScoreAroundSpurNodes(RailPointNode spurNode, List<CandidatePath> candidatePaths)
        {
            foreach (var tupleCandidateRoute in candidatePaths)
            {
                CandidatePath candidateRoute = tupleCandidateRoute;
                List<RailPointNode> roadNodes = candidateRoute.RailPoints;
                List<RailLineNode> roadLinks = candidateRoute.RailLines;

                if (roadNodes.Contains(spurNode))
                {
                    List<NetworkLineNode> nwLines = roadLinks[roadNodes.IndexOf(spurNode)].NWLines;
                    for (int i = 0; i < nwLines.Count; i++)
                    {
                        NetworkLineNode nwLine = nwLines[i];
                        _lineSpurState[nwLine.ID] = true;
                    }
                }
            }
        }

        public bool CheckListEquals(List<RailPointNode> value, List<RailPointNode> tempValue)
        {
            bool isEqual = true;

            if (object.ReferenceEquals(value, tempValue))
            {
                //같은 인스턴스면 true
                isEqual = true;
            }
            else if (value == null || tempValue == null
                || value.Count != tempValue.Count)
            {
                //어느 한 쪽이 null이거나, 요소의 수가 다를 때는 false
                isEqual = false;
            }
            else
            {
                //1개 1개씩 요소 비교
                for (int i = 0; i < value.Count; i++)
                {
                    //ary1의 요소의 Equals메소드에서, ary2의 요소와 같은지를 비교
                    if (!value[i].Equals(tempValue[i]))
                    {
                        //1개라도 같지 않은 요소가 있으면 false
                        isEqual = false;
                        break;
                    }
                }
            }
            return isEqual;
        }
        #endregion
    }

    public class SimpleDijkstra
    {
        #region Variable
        private float[,] _graph;
        private int[,] _refNode;
        private int _nodeCount;
        //private Dictionary<int, List<int>> _dicShortestPath;
        //private Dictionary<int, string> _dicShortestPathString;
        //private Dictionary<int, StringBuilder> _dicShortestPathSB;
        #endregion

        public SimpleDijkstra(float[,] graph, int nodeCount)
        {
            _graph = graph;
            _nodeCount = nodeCount;
            _refNode = new int[_nodeCount, _nodeCount];
            //_dicShortestPath = new Dictionary<int, List<int>>();
            //_dicShortestPathString = new Dictionary<int, string>();
            //_dicShortestPathSB = new Dictionary<int, StringBuilder>();

            for (int i = 0; i < _nodeCount; i++)
                for (int j = 0; j < _nodeCount; j++)
                {
                    _refNode[i, j] = -1;
                    //init DicShortest
                    //_dicShortestPath[i * _nodeCount + j] = new List<int>();
                    //_dicShortestPathSB[i * _nodeCount + j] = new StringBuilder();
                }
        }

        //public void DijkstraAlgo(int[,] graph, int source, int verticesCount, int[,] refNode)
        //public void DijkstraAlgo(RailPointNode fromNode, RailPointNode toNode, List<RailPointNode> rootPoints, List<RailPointNode> shortestPoints, List<RailLineNode> shortestLines, ref double totalScore)
        //{
        //    //System.Diagnostics.Stopwatch watch1 = new System.Diagnostics.Stopwatch();
        //    //System.Diagnostics.Stopwatch watch2 = new System.Diagnostics.Stopwatch();
        //    //System.Diagnostics.Stopwatch watch3 = new System.Diagnostics.Stopwatch();
        //    //watch1.Start();

        //    int[] distance = new int[_nodeCount];
        //    bool[] shortestPathTreeSet = new bool[_nodeCount];

        //    for (int i = 0; i < _nodeCount; ++i)
        //    {
        //        distance[i] = int.MaxValue;
        //        shortestPathTreeSet[i] = false;
        //    }
        //    distance[toNode.SNIdx] = 0;

        //    //watch1.Stop();
        //    //watch2.Start();

        //    for (int count = 0; count < _nodeCount - 1; ++count)
        //    {
        //        int u = MinimumDistance(distance, shortestPathTreeSet, _nodeCount);
        //        shortestPathTreeSet[u] = true;

        //        for (int v = 0; v < _nodeCount; ++v)
        //            if (!shortestPathTreeSet[v] && Convert.ToBoolean(-_graph[u, v]) && distance[u] != int.MaxValue && distance[u] + _graph[u, v] < distance[v])
        //            {
        //                distance[v] = distance[u] + _graph[u, v];
        //                _refNode[v] = u;
        //            }

        //        if (shortestPathTreeSet[fromNode.SNIdx]) //출발지까지 계산 끝났다면... 나감
        //            break;
        //    }
        //    //watch2.Stop();


        //    //watch3.Start();
        //    //shortest path points & lines
        //    int nodeIdx = fromNode.SNIdx;
        //    int fromIdx = -1;
        //    int toIdx = -1;
        //    while (true)
        //    {
        //        if (toIdx != -1 && fromIdx != -1)
        //        {
        //            for (int i = 0; i < ModelManager.Instance.DicSNRailLine[toIdx * _nodeCount + fromIdx].Count; i++)
        //                shortestLines.Add(ModelManager.Instance.DicSNRailLine[toIdx * _nodeCount + fromIdx][i]);
        //        }

        //        shortestPoints.Add(ModelManager.Instance.DicSNRailPoint[nodeIdx]);

        //        if (nodeIdx == toNode.SNIdx)
        //            break;

        //        else
        //        {
        //            fromIdx = nodeIdx;
        //            toIdx = _refNode[nodeIdx];

        //            nodeIdx = _refNode[nodeIdx];
        //        }

        //    }
        //    //watch3.Stop();

        //    //Console.WriteLine("111111------------------------------>>>" + watch1.ElapsedTicks.ToString() + " / " + watch2.ElapsedTicks.ToString() + " / " + watch3.ElapsedTicks.ToString());

        //    //shortest path lines



        //    //for (int i = 0; i < _nodeCount; i++)
        //    //{
        //    //    int nodeIdx = i;
        //    //    while (true)
        //    //    {
        //    //        if (nodeIdx == -1)
        //    //            break;
        //    //        else
        //    //        {
        //    //            _dicShortestPath[source * verticesCount + i].Add(nodeIdx);
        //    //            nodeIdx = refNode[source, nodeIdx];
        //    //        }
        //    //    }
        //    //}
        //    //Print(distance, verticesCount);
        //}

        //        public Dictionary<int, List<int>> Dijkstra()
        //public Dictionary<int, string> DijkstraString()
        //{
        //    //            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        //    //            watch.Start();
        //    //            Parallel.For(0, _nodeCount, i =>
        //    for (int i = 0; i < _nodeCount; i++)
        //    {
        //        DijkstraAlgo(i, _nodeCount);
        //    };
        //    //        });
        //    //            watch.Stop();
        //    //            Console.WriteLine(watch.Elapsed.ToString());


        //    return _dicShortestPathString;
        //}

//        public Dictionary<int, List<int>> Dijkstra()
//        {
//            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
//            watch.Start();
//            Parallel.For(0, _nodeCount, i =>
////            for (int i = 0; i < _nodeCount; i++)
//            {
//                DijkstraAlgo(i, _nodeCount);
////            };
//        });
//            watch.Stop();
////            Console.WriteLine(watch.Elapsed.ToString());


//            return _dicShortestPath;
//        }

        public int[,] Dijkstra()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Parallel.For(0, _nodeCount, i =>
            //            for (int i = 0; i < _nodeCount; i++)
            {
                DijkstraAlgo(i, _nodeCount);
                //            };
            });
            watch.Stop();
            Console.WriteLine(watch.Elapsed.ToString());


            return _refNode;
        }

        public void DijkstraAlgo(int source, int _nodeCount)
        {
            float[] distance = new float[_nodeCount];
            bool[] shortestPathTreeSet = new bool[_nodeCount];

            for (int i = 0; i < _nodeCount; ++i)
            {
                distance[i] = int.MaxValue;
                shortestPathTreeSet[i] = false;
            }

            distance[source] = 0;

            //watch1.Stop();
            //watch2.Start();

            int minIndex = source;

            for (int count = 0; count < _nodeCount - 1; ++count)
            {
                //                int u = MinimumDistance(distance, shortestPathTreeSet, _nodeCount);
                int u = (int)minIndex;
                float min = float.MaxValue;
                shortestPathTreeSet[u] = true;

                NetworkPointNode fromNWPoint = ModelManager.Instance.NWPoints[u];

                foreach(NetworkLineNode toNWLine in fromNWPoint.outLines)
                {
                    NetworkPointNode toNWPoint = toNWLine.ToNode;
                    int v = (int)toNWPoint.ID;
                    if (!shortestPathTreeSet[v] && Convert.ToBoolean(-_graph[u, v]) && distance[u] != int.MaxValue && distance[u] + _graph[u, v] < distance[v])
                    {
                        distance[v] = distance[u] + _graph[u, v];
                        _refNode[source, v] = u;

                        if (shortestPathTreeSet[v] == false && distance[v] <= min)
                        {
                            min = distance[v];
                            minIndex = v;
                        }
                    }
                }

                if(min == float.MaxValue)
                {
                    for (int v = 0; v < _nodeCount; ++v)
                    {
                        if (shortestPathTreeSet[v] == false && distance[v] <= min)
                        {
                            min = distance[v];
                            minIndex = v;
                        }
                    }
                }


                //for (int v = 0; v < _nodeCount; ++v)
                //    if (!shortestPathTreeSet[v] && Convert.ToBoolean(-_graph[u, v]) && distance[u] != int.MaxValue && distance[u] + _graph[u, v] < distance[v])
                //    {
                //        distance[v] = distance[u] + _graph[u, v];
                //        _refNode[source, v] = u;
                //    }
            }


//            for (int i = 0; i < _nodeCount; i++)
//            {
//                int nodeIdx = i;
////                List<int> path = _dicShortestPath[source * _nodeCount + i];
//                while (true)
//                {
//                    if (nodeIdx == -1)
//                        break;
//                    else
//                    {
////                        path.Add(nodeIdx);
////                        _dicShortestPathSB[source * _nodeCount + i].Append(nodeIdx);
////                        _dicShortestPathSB[source * _nodeCount + i].Append(';');
//                        nodeIdx = _refNode[source, nodeIdx];
//                    }
//                }
////                _dicShortestPathString.Add(source * _nodeCount + i, _dicShortestPathSB[source * _nodeCount + i].ToString());
//            }

            //watch3.Start();
            //shortest path points & lines
            //int nodeIdx = fromNode.SNIdx;
            //int fromIdx = -1;
            //int toIdx = -1;
            //while (true)
            //{
            //    if (toIdx != -1 && fromIdx != -1)
            //    {
            //        for (int i = 0; i < ModelManager.Instance.DicSNRailLine[toIdx * _nodeCount + fromIdx].Count; i++)
            //            shortestLines.Add(ModelManager.Instance.DicSNRailLine[toIdx * _nodeCount + fromIdx][i]);
            //    }

            //    shortestPoints.Add(ModelManager.Instance.DicSNRailPoint[nodeIdx]);

            //    if (nodeIdx == toNode.SNIdx)
            //        break;

            //    else
            //    {
            //        fromIdx = nodeIdx;
            //        toIdx = _refNode[nodeIdx];

            //        nodeIdx = _refNode[nodeIdx];
            //    }

            //}


            //watch3.Stop();

            //Console.WriteLine("111111------------------------------>>>" + watch1.ElapsedTicks.ToString() + " / " + watch2.ElapsedTicks.ToString() + " / " + watch3.ElapsedTicks.ToString());

            //shortest path lines



            //for (int i = 0; i < _nodeCount; i++)
            //{
            //    int nodeIdx = i;
            //    while (true)
            //    {
            //        if (nodeIdx == -1)
            //            break;
            //        else
            //        {
            //            _dicShortestPath[source * verticesCount + i].Add(nodeIdx);
            //            nodeIdx = refNode[source, nodeIdx];
            //        }
            //    }
            //}
            //Print(distance, verticesCount);
        }

        private int MinimumDistance(float[] distance, bool[] shortestPathTreeSet, int verticesCount)
        {
            float min = float.MaxValue;
            int minIndex = 0;


            for (int v = 0; v < verticesCount; ++v)
            {
                if (shortestPathTreeSet[v] == false && distance[v] <= min)
                {
                    min = distance[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }
    }
}
