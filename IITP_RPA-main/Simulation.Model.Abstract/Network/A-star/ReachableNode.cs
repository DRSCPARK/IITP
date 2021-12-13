//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Simulation.Model.Abstract
//{
//    public class ReachableNode
//    {
//        #region Variables        
//        public NetworkPointNode _nP;
//        public NetworkLineNode _nL;
//        #endregion

//        public ReachableNode(NetworkPointNode np, NetworkLineNode nl)
//        {
//            _nP = np;
//            _nL = nl;
//        }

//        public float TotalCost()
//        {
//            return _nP.TotalCost;
//        }

//        public float CompareTo(ReachableNode rn)
//        {
//            return _nP.TotalCost - rn._nP.TotalCost;        
//        }
//    }
//}
