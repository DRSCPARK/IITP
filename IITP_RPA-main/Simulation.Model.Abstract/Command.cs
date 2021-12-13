using Simulation.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class Command : SimPort
    {
        private string _name;
        private Fab _fab;
        private int _priority;
        private DateTime _realCompletedTime;
        private DateTime _activatedDateTime;
        private DateTime _assignedTime;
        private DateTime _loadedTime;
        private DateTime _completedTime;
        bool _reticle;
        private double _transferingDistance;
        private OHTNode _oht;
        protected COMMAND_STATE _state;
        private List<RailLineNode> _lstRoute;
        private List<RailLineNode> _lstFirstRoute;
        private int _reroutingCount;
        
        [Browsable(true)]
        public string Name
        {
            get {
                if (_name == "")
                    return "Command_" + FromNode.Name + "_" + ToNode.Name + "_" + EntityName;
                else
                    return _name;
            }
        }
        [Browsable(false)]
        public Fab Fab
        {
            get { return _fab; }
        }

        [Browsable(true)]
        public int Priority
        {
            get { return _priority; }
        }

        [Browsable(true)]
        public COMMAND_STATE CommandState
        {
            get { return _state; }
            set { _state = value; }
        }

        [Browsable(false)]
        public Time ActivatedTime
        {
            get { return Time; }
            set { Time = value; }
        }

        [Browsable(false)]
        public DateTime ActivatedDateTime
        {
            get { return _activatedDateTime; }
            set { _activatedDateTime = value; }
        }

        [Browsable(true)]
        [DisplayName("Activated Time")]
        public string StringActivatedTime
        {
            get { return _activatedDateTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        /// <summary>
        /// OHT Dispatching 시점
        /// </summary>
        [Browsable(false)]
        public DateTime AssignedTime
        {
            get { return _assignedTime; }
            set { _assignedTime = value; }
        }

        [Browsable(true)]
        [DisplayName("Assigned Time")]
        public string StringAssignedTime
        {
            get { return _assignedTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        /// <summary>
        /// Foup OHT Loading 후의 시점
        /// </summary>
        [Browsable(false)]
        public DateTime LoadedTime
        {
            get { return _loadedTime; }
            set { _loadedTime = value; }
        }

        [Browsable(true)]
        [DisplayName("Loaded Time")]
        public string StringLoadedTime
        {
            get { return _loadedTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        /// <summary>
        /// Unloading(Deposit) 후의 시점(OHT Idle)
        /// </summary>
        [Browsable(false)]
        public DateTime CompletedTime
        {
            get { return _completedTime; }
            set { _completedTime = value; }
        }

        [Browsable(true)]
        [DisplayName("Completed Time")]
        public string StringCompletedTime
        {
            get { return _completedTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }


        [Browsable(false)]
        public DateTime RealCompletedTime
        {
            get { return _realCompletedTime; }
        }

        //AsignedTime - Activated Time
        [Browsable(false)]
        public double QueuedTime
        {
            get
            {
                DateTime activatedDateTime = SimResultDBManager.Instance.SimulationStartTime.AddSeconds(ActivatedTime.ToSecond());
                return (AssignedTime - activatedDateTime).TotalSeconds;
            }
        }

        [Browsable(false)]
        public double WaitingTime
        {
            get { return (LoadedTime - AssignedTime).TotalSeconds - AcquiringTime; }
        }

        [Browsable(false)]
        public double AcquiringTime
        {
            get
            {
                return ModelManager.Instance.OHTLoadingTime;
            }
        }

        [Browsable(false)]
        public double TransferingTime
        {
            get { return (CompletedTime - LoadedTime).TotalSeconds; }
        }

        [Browsable(false)]
        public double DepositTime
        {
            get { return ModelManager.Instance.OHTUnloadingTime; }
        }

        [Browsable(false)]
        public double TotalTime
        {
            get
            {
                DateTime activatedDateTime = SimResultDBManager.Instance.SimulationStartTime.AddSeconds(ActivatedTime.ToSecond());
                return (CompletedTime - activatedDateTime).TotalSeconds;
            }
        }

        [Browsable(true)]
        public bool Reticle
        {
            get { return _reticle; }
        }

        [Browsable(false)]
        public double TransferingDistance
        {
            get { return _transferingDistance; }
            set { _transferingDistance = value; }
        }

        [Browsable(false)]
        public NODE_TYPE FROM_EQ_TYPE
        {
            get
            {
                return (NODE_TYPE)((RailPortNode)FromNode).ConnectedEqp.NodeType;
            }
        }

        [Browsable(true)]
        public string FromPortName
        {
            get
            {
                if ((RailPortNode)FromNode != null)
                {
                    return ((RailPortNode)FromNode).Name;
                }
                else
                    return string.Empty;
            }
        }

        [Browsable(false)]
        public NODE_TYPE TO_EQ_TYPE
        {
            get
            { return (NODE_TYPE)((RailPortNode)ToNode).ConnectedEqp.NodeType; }
        }

        [Browsable(true)]
        public string ToPortName
        {
            get
            {
                if ((RailPortNode)ToNode != null)
                {
                    return ((RailPortNode)ToNode).Name;
                }
                else
                    return string.Empty;
            }
        }

        [Browsable(true)]
        public string OHTName
        {
            get
            {
                if (_oht != null)
                    return _oht.Name;
                else
                    return string.Empty;
            }
        }

        [Browsable(false)]
        public OHTNode OHTNode
        {
            get { return _oht; }
            set { _oht = value; }
        }

        [Browsable(false)]
        public List<RailLineNode> Route
        {
            get { return _lstRoute; }
            set { _lstRoute = value; }
        }

        [Browsable(false)]
        public List<RailLineNode> FirstRoute
        {
            get { return _lstFirstRoute; }
            set { _lstFirstRoute = value; }
        }

        [Browsable(false)]
        public string RouteIDString
        {
            get
            {
                string routeString = string.Empty;
                foreach(RailLineNode line in _lstRoute)
                {
                    routeString = routeString + line.ID + ModelManager.Instance.Separator;
                }

                return routeString;
            }
        }

        [Browsable(false)]
        public string FirstRouteIDString
        {
            get
            {
                string routeString = string.Empty;
                foreach (RailLineNode line in _lstFirstRoute)
                {
                    routeString = routeString + line.ID + ModelManager.Instance.Separator;
                }

                return routeString;
            }
        }


        [Browsable(true)]
        public int ReroutingCount
        {
            get
            { return _reroutingCount; }
            set
            { _reroutingCount = value; }
        }

        public Command(string name, FabSimNode fromNode, FabSimNode toNode, Time time, Foup entity, Fab fab, int priority)
    : base(EXT_PORT.COMMAND, fromNode, toNode, entity)
        {
            _name = name;
            _fab = fab;
            Time = time;
            FromNode = fromNode;
            ToNode = toNode;
            Entity = entity;
            _priority = priority;
            _reticle = false;
            _state = COMMAND_STATE.INACTIVE;
            _lstRoute = new List<RailLineNode>();
            _lstFirstRoute = new List<RailLineNode>();
            _reroutingCount = 0;
        }

        public Command(string name, FabSimNode fromNode, FabSimNode toNode, DateTime time, Foup entity, Fab fab, int priority, bool reticle, DateTime realTime)
            : base(EXT_PORT.COMMAND, fromNode, toNode, entity)
        {
            _name = name;
            _fab = fab;
            Time = (time - SimEngine.Instance.StartDateTime).TotalSeconds;
            _activatedDateTime = time;
            FromNode = fromNode;
            ToNode = toNode;
            Entity = entity;
            _priority = priority;
            _realCompletedTime = realTime;
            _reticle = reticle;
            _state = COMMAND_STATE.INACTIVE;
            _lstRoute = new List<RailLineNode>();
            _lstFirstRoute = new List<RailLineNode>();
            _reroutingCount = 0;
        }

        public Command(FabSimNode fromNode, FabSimNode toNode, Time time, Foup entity, Fab fab, int priority)
    : base(EXT_PORT.COMMAND, fromNode, toNode, entity)
        {
            _name = "Command_" + fab.Name + "_" + fromNode.Name + "_" + toNode.Name + "_" + time;
            _fab = fab;
            Time = time;
            _activatedDateTime = SimEngine.Instance.StartDateTime.AddSeconds(time.TotalSeconds);
            FromNode = fromNode;
            ToNode = toNode;
            Entity = entity;
            _priority = priority;
            _realCompletedTime = _activatedDateTime.AddSeconds(300);
            _reticle = false;
            _state = COMMAND_STATE.INACTIVE;
            _lstRoute = new List<RailLineNode>();
            _lstFirstRoute = new List<RailLineNode>();
            _reroutingCount = 0;
        }

        public Command(FabSimNode fromNode, FabSimNode toNode, Time time, Foup entity, Fab fab, int priority, bool reticle)
: base(EXT_PORT.COMMAND, fromNode, toNode, entity)
        {
            _name = "Command_" + fab.Name + "_" + fromNode.Name + "_" + toNode.Name + "_" + time;
            _fab = fab;
            Time = time;
            _activatedDateTime = SimEngine.Instance.StartDateTime.AddSeconds(time.TotalSeconds);
            FromNode = fromNode;
            ToNode = toNode;
            Entity = entity;
            _priority = priority;
            _realCompletedTime = _activatedDateTime.AddSeconds(300);
            _reticle = reticle;
            _state = COMMAND_STATE.INACTIVE;
            _lstRoute = new List<RailLineNode>();
            _reroutingCount = 0;
        }
    }
}