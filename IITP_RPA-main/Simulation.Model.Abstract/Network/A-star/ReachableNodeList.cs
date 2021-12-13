//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Simulation.Model.Abstract
//{
//    public class ReachableNodeList
//    {
//        #region Variables        
//        List<ReachableNode> _lstRN;
//        List<NetworkPointNode> _lstNP;
//        Dictionary<NetworkPointNode, ReachableNode> _dicNPRN;
//        #endregion

//        public ReachableNodeList()
//        {
//            _lstRN = new List<ReachableNode>();
//            _lstNP = new List<NetworkPointNode>();
//            _dicNPRN = new Dictionary<NetworkPointNode, ReachableNode>();
//        }

//        public void AddReachableNode(ReachableNode rn)
//        {
//            //이미 있는 경우
//            for(int i=0; i < _lstNP.Count(); i++)
//            {
//                if(_lstNP[i] == rn._nP)
//                {
//                    ReachableNode oldRN = GetReachableNodeFromNode(rn._nP);
//                    if (rn._nL.Length < oldRN._nL.Length)
//                        oldRN._nL = rn._nL;
                    
//                    return;
//                }
//            }

//            //없는 경우
//            _lstRN.Add(rn);
//            _lstNP.Add(rn._nP);
//            _dicNPRN.Add(rn._nP, rn);
//        }

//        public List<ReachableNode> ReachableNodes()
//        {
//            return _lstRN;
//        }

//        public void RemoveReachableNode(ReachableNode rn)
//        {
//            for(int i=0; i < _lstRN.Count(); i++)
//            {
//                if (_lstRN[i] == rn)
//                {
//                    _lstRN.RemoveAt(i);
//                    break;
//                }                    
//            }

//            for(int i=0; i < _lstNP.Count(); i++)
//            {
//                if (_lstNP[i] == rn._nP)
//                {
//                    _lstNP.RemoveAt(i);
//                    break;
//                }  
//            }
//        }

//        public bool HasNode(NetworkPointNode n)
//        {
//            for(int i=0; i < _lstNP.Count(); i++)
//            {
//                if (_lstNP[i] == n)
//                    return true;
//            }
//            return false;
//        }

//        public ReachableNode GetReachableNodeFromNode(NetworkPointNode n)
//        {
//            ReachableNode it = _dicNPRN[n];

//            List<ReachableNode> lstRn = _dicNPRN.Values.ToList();

//            if (it != lstRn[lstRn.Count() - 1])
//                return it;
//            else
//                return null;
//        }

//        public void SortReachableNodes()
//        {
//            for (int i = 0; i < _lstRN.Count(); i++)
//            {
//                for (int j = 0; j < _lstRN.Count() - 1; j++)
//                {
//                    if (cmp(_lstRN[j].TotalCost(), _lstRN[j + 1].TotalCost()) == 1)
//                    {
//                        ReachableNode temp = _lstRN[j];
//                        _lstRN[j] = _lstRN[j + 1];
//                        _lstRN[j + 1] = temp;
//                    }
//                }
//            }
//        }

//        public int cmp(double a, double b)
//        {
//            if (a > b) return 1;
//            else if (a < b) return -1;
//            else return 0;
//        }

//        public void Clear()
//        {
//            _lstRN.Clear();
//            _lstNP.Clear();
//            _dicNPRN.Clear();
//        }

//    }
//}
