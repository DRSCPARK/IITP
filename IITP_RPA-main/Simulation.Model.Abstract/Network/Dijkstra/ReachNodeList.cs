using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class ReachNodeList
    {
        #region Member Variable

        private List<ReachNode> _rachableNodeList;
        private Dictionary<uint, NetworkPointNode> _dicPoint; //Key: RailPointNodeID
        private Dictionary<uint, ReachNode> _reachableNodeDic; //Key: NetworkPointNodeID

        #endregion

        public ReachNodeList()
        {
            _rachableNodeList = new List<ReachNode>();
            _dicPoint = new Dictionary<uint, NetworkPointNode>();
            _reachableNodeDic = new Dictionary<uint, ReachNode>();
        }

        public void AddReachableNode(ReachNode reachNode)
        {
            NetworkPointNode networkPoint = reachNode._networkPoint;
            uint reachNodeID = networkPoint.CoreNode.ID;
            if (_dicPoint.ContainsKey(reachNodeID))
            {
                ReachNode oldRN = GetReachableNodeFromNode(networkPoint);
                if (reachNode._networkLine.TotalCost < oldRN._networkLine.TotalCost)
                    oldRN._networkLine = reachNode._networkLine;
            }
            else
            {
                _rachableNodeList.Add(reachNode);
                _dicPoint.Add(reachNodeID, networkPoint);
                _reachableNodeDic.Add(networkPoint.ID, reachNode);
            }
        }

        public List<ReachNode> ReachableNodes
        {
            get { return _rachableNodeList; }
        }

        public ReachNode GetMinReachNodeByTotalCost()
        {
            double minTotalCost = float.MaxValue;
            ReachNode minNode = null;
            foreach(ReachNode node in _rachableNodeList)
            {
                if(node.TotalCost < minTotalCost || minTotalCost == float.MaxValue)
                {
                    minTotalCost = node.TotalCost;
                    minNode = node;
                }
            }

            return minNode;
        }

        public void RemoveReachableNode(ReachNode reachNode)
        {
            _rachableNodeList.Remove(reachNode);
            _dicPoint.Remove(reachNode._networkPoint.CoreNode.ID);
        }

        public bool HasNode(NetworkPointNode node)
        {
            return _dicPoint.ContainsKey(node.CoreNode.ID);
        }

        public ReachNode GetReachableNodeFromNode(NetworkPointNode node)
        {
            if (_reachableNodeDic.ContainsKey(node.ID))
            {
                return _reachableNodeDic[node.ID];
            }
            else
            {
                return null;
            }
        }

        public void SortReachableNodes()
        {
            _rachableNodeList.Sort();
            //IEnumerable<ReachNode> sort =
            //    from node in _rachableNodeList
            //    orderby node.TotalCost
            //    select node;

            //_rachableNodeList = sort.ToList();
        }

        public void Clear()
        {
            this._rachableNodeList.Clear();
            this._dicPoint.Clear();
            this._reachableNodeDic.Clear();
        }
    }
}
