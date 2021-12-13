using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class TransitControl
    {
        #region Private Variables
        private List<RailPointNode> _outerPoints;
        private List<RailPointNode> _innerPoints;

        private Dictionary<Bay, List<RailPointNode>> _bayOuterPoints;
        private Dictionary<Bay, List<RailPointNode>> _bayInnerPoints;

        private OuterPathTable _outerPathTable;
        private TransitPairTable _transitPairTable;
        private InnerPathTable _innerPathTable;
        #endregion

        #region Public Variables
        static TransitControl _instance;
        public Dictionary<Bay, List<RailPointNode>> BayOuterPoints { get { return _bayOuterPoints; } }
        public Dictionary<Bay, List<RailPointNode>> BayInnerPoints { get { return _bayInnerPoints; } }
        public OuterPathTable OuterPathTable { get { return _outerPathTable; } }
        public TransitPairTable TransitPairTable { get { return _transitPairTable; } }
        public InnerPathTable InnerPathTable { get { return _innerPathTable; } }

        public static TransitControl Instance { get { return _instance; } }
        #endregion

        public TransitControl()
        {
            _instance = this;
             
            _bayInnerPoints = new Dictionary<Bay, List<RailPointNode>>();
            _bayOuterPoints = new Dictionary<Bay, List<RailPointNode>>();
        }

        public void Initialize()
        {
            InitializeTransitNode();
            InitializeTransitPairTable();
            InitializeOuterPathTable();
            InitializeInnerPathTable();
        }

        public void InitializeTransitNode()
        {
            _bayInnerPoints.Clear();
            _bayOuterPoints.Clear();

            // Set Inner,Outer Point
            foreach (var bay in ModelManager.Instance.Bays.Values)
            {
                if(bay.BayType == BAY_TYPE.INTRABAY)
                {
                    var totalPoints = bay.Points;

                    var bayInnerPoints = new List<RailPointNode>();
                    var bayOuterPoints = new List<RailPointNode>();

                    foreach (var point in totalPoints)
                    {
                        string[] nodeName = point.Name.Split('_');

                        if (nodeName[nodeName.Length - 1] == "2" || nodeName[nodeName.Length - 1] == "4" ||
                               nodeName[nodeName.Length - 1] == "16" || nodeName[nodeName.Length - 1] == "18")
                        { bayInnerPoints.Add(point); }

                        if (nodeName[nodeName.Length - 1] == "9" || nodeName[nodeName.Length - 1] == "11" ||
                              nodeName[nodeName.Length - 1] == "23" || nodeName[nodeName.Length - 1] == "25")
                        { bayOuterPoints.Add(point); }
                    }

                    if (bay.Name == "B1" || bay.Name == "A1")
                    {
                        bayInnerPoints.RemoveAt(1);
                        bayOuterPoints.RemoveAt(0);
                    }
                    else if (bay.Name == "B18" || bay.Name == "A18")
                    {
                        bayInnerPoints.RemoveAt(3);
                        bayOuterPoints.RemoveAt(2);
                    }

                    _bayInnerPoints.Add(bay, bayInnerPoints);
                    _bayOuterPoints.Add(bay, bayOuterPoints);
                }
                
            }
        }

        public void InitializeTransitPairTable()
        {
            _innerPoints = new List<RailPointNode>();
            _outerPoints = new List<RailPointNode>();

            var bays = ModelManager.Instance.Bays.Values;
            var intraBays = bays.Where(bay => bay.BayType == BAY_TYPE.INTRABAY).ToList();

            foreach (var bay in intraBays)
            {
                var bayInnerPoints = _bayInnerPoints[bay];
                var bayOuterPoints = _bayOuterPoints[bay];

                _innerPoints.AddRange(bayInnerPoints);
                _outerPoints.AddRange(bayOuterPoints);
            }

            // Initializing TransitPairTable
            _transitPairTable = new TransitPairTable(_outerPoints, _innerPoints);

            for(int i = 0; i < intraBays.Count; i++)
            {
                var startBay = intraBays[i];

                for(int j = 0; j < intraBays.Count; j++)
                {
                    // 동일 Bay 내 Path는 고려 대상에서 제외
                    if (i == j) continue;

                    // i != j인 경우만 고려
                    var endBay = intraBays[j];

                    var outerPoints = _bayOuterPoints[startBay];
                    var innerPoints = _bayInnerPoints[endBay];

                    foreach(var outerPoint in outerPoints)
                    {
                        foreach(var innerPoint in innerPoints)
                        { _transitPairTable.AssignPath(outerPoint, innerPoint); }
                    }
                }
            }
        }

        public void InitializeOuterPathTable()
        {
            var processPorts = ModelManager.Instance.DicRailPort.Values.Where(port => port is ProcessPortNode).ToList();

            _outerPathTable = new OuterPathTable(processPorts, _outerPoints);

            foreach(var bay in BayOuterPoints.Keys)
            {
                foreach (var processPort in processPorts)
                {
                    if (processPort.Line.BayName == bay.Name)
                    {
                        var outerPoints = BayOuterPoints[bay];

                        foreach (var outerPoint in outerPoints)
                        { _outerPathTable.AssignPath(processPort, outerPoint); }
                    }
                }
            }
        }

        public void InitializeInnerPathTable()
        {
            var processPorts = ModelManager.Instance.DicRailPort.Values.Where(port => port is ProcessPortNode).ToList();

            _innerPathTable = new InnerPathTable(_innerPoints, processPorts);

            foreach (var bay in BayInnerPoints.Keys)
            {
                var innerPoints = BayInnerPoints[bay];

                foreach(var innerPoint in innerPoints)
                {
                    foreach (var processPort in processPorts)
                    {
                        if (processPort.Line.BayName == bay.Name)
                        { _innerPathTable.AssignPath(innerPoint, processPort); }
                    }
                }
            }
        }
    }
}
