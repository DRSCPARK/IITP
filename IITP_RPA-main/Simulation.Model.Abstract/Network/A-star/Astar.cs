using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class AstartCal
    {
        //#region Variables        
        //private static List<RailPointNode> _lstClosedSet = new List<RailPointNode>();
        //private static List<RailPointNode> _lstOpenSet = new List<RailPointNode>();

        //#endregion

        //public AstartCal()
        //{            
        //}               
        //public static void InitParam()
        //{
        //    for (int i = 0; i < ModelManager.Instance.LstRailPoint.Count(); i++)
        //        ModelManager.Instance.LstRailPoint[i].InitShortestPathParam();
        //}
        //public static List<RailPointNode> AstarShortestPath(RailPointNode start, RailPointNode goal, List<RailLineNode> shortestLink)
        //{
        //    InitParam();

        //    _lstClosedSet.Clear();
        //    _lstOpenSet.Clear();
        //    // 출발 위치를 openset에 넣고 초기값을 설정한다.
        //    _lstOpenSet.Add(start);

        //    start.g_score = 0;
        //    start.h_score = heuristic_cost_estimate(start, goal);
        //    start.f_score = start.h_score + start.g_score;

        //    // openset(방문 예정인 node를 기록해 둠)이 비어있지 않은 경우 실행.
        //    while(_lstOpenSet.Count() != 0)
        //    {
        //        // openset에서 f_score가 가장 적은 node를 하나 꺼내온다.
        //        RailPointNode x = PopOpenSet();

        //        // goal에 도달했다면 while 루프를 중단한다.
        //        if (x == goal)
        //            break;

        //        // closedset(이미 방문한 node를 기록해둠)에 x를 등록한다.
        //        _lstClosedSet.Add(x);

        //        // x에 연결된 node를 찾는다. !!!!! 기존 C++에서는 방향성 없었나.. 이건 방향성 있게 약간 수정
        //        for(int i=0; i < x.OutLinks.Count(); i++)
        //        {
        //            if(x.OutLinks[i].EndNode is RailLineNode)
        //            {
        //                if(((RailLineNode)x.OutLinks[i].EndNode).FromNode == x)
        //                {
        //                    RailPointNode y = ((RailLineNode)x.OutLinks[i].EndNode).ToNode;

        //                    // 찾은 node y가 이미 방문한 node이면 다음 과정을 수행하지 않는다.
        //                    if (SearchCloseSet(y))
        //                        continue;

        //                    bool tentative_is_better = false;
        //                    // node y 까지의 잠정적인 g_score를 계산한다.
        //                    double tentative_g_score = x.g_score + ((RailLineNode)x.OutLinks[i].EndNode).Length / 1;// 여기서 1은 라인의 속도.. 반도체에서는 OHT 속도..

        //                    // node y가 openset에 없다면 openset에 추가하고 y의 상태를 업데이트 한다.
        //                    if (!SearchOpenSet(y))
        //                    {
        //                        _lstOpenSet.Add(y);
        //                        tentative_is_better = true;
        //                    }

        //                    // node y가 openset에 있으면서 잠정적인 g_score가 y의 현재 g_socre보다 적은 경우
        //                    // y의 상태를 업데이트 한다.
        //                    else if (tentative_g_score < y.g_score)
        //                        tentative_is_better = true;

        //                    else
        //                        tentative_is_better = false;

        //                    if (tentative_is_better)
        //                    {
        //                        // y의 상태를 업데이트 한다.
        //                        y.came_from = x;
        //                        y.g_score = tentative_g_score;
        //                        y.h_score = heuristic_cost_estimate(y, goal);
        //                        y.f_score = y.g_score + y.h_score;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    List<RailPointNode> lstShortestPath = ReverseFollow(start, goal);

        //    lstShortestPath.Reverse();
        //    //find shortestLinks..
        //    for (int i = 1; i < lstShortestPath.Count; i++)
        //    {
        //        for (int j = 0; j < lstShortestPath[i].InLinks.Count; j++)
        //        {
        //            if ((RailPointNode)lstShortestPath[i].InLinks[j].StartNode.InLinks[0].StartNode == lstShortestPath[i - 1])
        //            {
        //                shortestLink.Add((RailLineNode)lstShortestPath[i].InLinks[j].StartNode);
        //                break;
        //            }
        //        }
        //    }
        //    return lstShortestPath;
        //    //return ReverseFollow(start, goal);
        //}

        //public static double heuristic_cost_estimate(RailPointNode start, RailPointNode goal)
        //{
        //    // start에서 goal 까지의 휴리스틱 코스트를 추정한다.
        //    // 여기서는 간단히 start와 goal 간의 거리를 사용하였다.

        //    double dx = start.PosVec3.X - goal.PosVec3.X;
        //    double dy = start.PosVec3.Y - goal.PosVec3.Y;

        //    return Math.Sqrt(dx * dx + dy * dy);
        //}

        //public static bool SearchCloseSet(RailPointNode id)
        //{
        //    for (int i = 0; i < _lstClosedSet.Count(); i++)
        //        if (id == _lstClosedSet[i])
        //            return true;

        //    return false;
        //}

        //public static bool SearchOpenSet(RailPointNode id)
        //{
        //    for (int i = 0; i < _lstOpenSet.Count(); i++)
        //        if (id == _lstOpenSet[i])
        //            return true;

        //    return false;
        //}

        //public static List<RailPointNode> ReverseFollow(RailPointNode start, RailPointNode goal)
        //{
        //    List<RailPointNode> lstPath = new List<RailPointNode>();

        //    lstPath.Add(goal);
        //    RailPointNode curNP = goal;
        //    while (true)
        //    {
        //        curNP = curNP.came_from;
        //        lstPath.Add(curNP);
        //        if (curNP == start)
        //            break;
        //    }

        //    return lstPath;
        //}
        
        //public static RailPointNode PopOpenSet()
        //{
        //    RailPointNode min_i = null;
        //    double min_f = 10000000000;
        //    RailPointNode min_it = null;

        //    for(int i=0; i < _lstOpenSet.Count(); i++)
        //    {
        //        if(_lstOpenSet[i].f_score < min_f)
        //        {
        //            min_i = _lstOpenSet[i];
        //            min_f = _lstOpenSet[i].f_score;
        //            min_it = _lstOpenSet[i];
        //        }
        //    }

        //    // 꺼내간 요소는 삭제한다
        //    for(int i=0; i < _lstOpenSet.Count(); i++)
        //    {
        //        if(_lstOpenSet[i] == min_it)
        //        {
        //            _lstOpenSet.RemoveAt(i);
        //            return min_i;
        //        }
        //    }

        //    //여기까지 오면 오류..
        //    return null;
        //}


    }
}
