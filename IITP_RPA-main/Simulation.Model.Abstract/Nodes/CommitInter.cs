using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    public struct CommitIntervalData
    {
        public string _entityName;
        public double _startTime;
        public double _interTime;
        public int _entityCount;

        public CommitIntervalData(string entityName, double startTime, double interTime, int entityCount)
        {
            _entityName = entityName;
            _startTime = startTime;
            _interTime = interTime;
            _entityCount = entityCount;
        }
    }

    public class CommitInterval : CommitNode
    {
        #region Member Variable

        private List<CommitIntervalData> _intervalData;
        private List<SimEntity> _lstEntity;
        //[TypeConverter(typeof(CommitInterData))]
        public List<CommitIntervalData> IntervalData
        {
            get { return _intervalData; }
            set { _intervalData = value; }
        }
        public List<SimEntity> LstEntity
        {
            get { return _lstEntity; }
        }
        #endregion

        public CommitInterval()
            : base(0, "", null, COMMIT_TYPE.INTERARRIVAL)
        {
            _intervalData = new List<CommitIntervalData>();
        }
        public CommitInterval(uint ID, string name, Fab fab)
            : base(ID, name, fab, COMMIT_TYPE.INTERARRIVAL)
        {
            _intervalData = new List<CommitIntervalData>();
        }
        
        public CommitInterval(uint ID, Vector3 pos, string name, Fab fab, string entityName, Time startTime, Time interarrivalTime, int entityCount)
            : base(ID, name, pos, fab, COMMIT_TYPE.INTERARRIVAL)
        {
            _intervalData = new List<CommitIntervalData>();
//            _intervalData.Add(new CommitIntervalData(entityName, (double)startTime, (double)interarrivalTime, entityCount));
        }

        public void AddCommitIntervalData(string entityName, Time startTime, Time interarrivalTime, int entityCount)
        {
            _intervalData.Add(new CommitIntervalData(entityName, (double)startTime, (double)interarrivalTime, entityCount));
        }

        public override List<string> SaveNode()
        {
            List<string> saveData = base.SaveNode();
            //             foreach (KeyValuePair<String, CommitData> genTime in IntervalTime)
            //             {
            //                 saveData.Add(genTime.Key);
            //                 saveData.Add(genTime.Value.TimeStart.ToString());
            //                 saveData.Add(genTime.Value.TimeInter.ToString());
            //                 saveData.Add(genTime.Value.EntityCount.ToString());
            //             }
            return saveData;
        }
        public override void LoadNode(List<string> loadData)
        {
            //             for (int i = 0; i < loadData.Count; i += 2)
            //             {
            //                 CommitData data = new CommitData();
            //                 data.TimeStart = Convert.ToDouble(loadData[i + 1]);
            //                 data.TimeInter = Convert.ToDouble(loadData[i + 2]);
            //                 data.EntityCount = Convert.ToInt32(loadData[i + 3]);
            //                 _intervalTime.Add(loadData[i], data);
            //             }
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);

            _lstEntity = new List<SimEntity>();
            int i = 0;
            foreach (CommitIntervalData interData in _intervalData)
            {
                INT_PORT intPort = INT_PORT.PART_GENERATE;
                Foup entity = ModelManager.Instance.AddFoup(interData._entityName);

                SimPort port = new SimPort(intPort, this, entity);
                EvtCalendar.AddEvent(interData._startTime + i, this, port);

                i++;
            }
        }

        public override void RequirePort(Time simTime, SimHistory simLogs)
        {
            if(ReserveEntites.Count()>0)
            {
                SimNode node = OutLinks[0].EndNode;
                SimPort port = ReserveEntites[0];
                node.ExternalFunction(simTime, simLogs, port);
                ReserveEntites.RemoveAt(0);
            }            
        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {            
            INT_PORT inPort = (INT_PORT)port.PortType;
            switch (inPort)
            {
                case INT_PORT.PART_GENERATE:
                    Console.WriteLine("[" + simTime + "] : Commit Part Gen ID : " + port.Entity.ID);

                    OutputFunction(simTime, simLogs, port);

                    _lstEntity.Add(port.Entity);

                    //스케줄러에 entity 생성 보고
                    Scheduler.Instance.ReportCommitEntityGeneration(this, (Command)port);

                    INT_PORT intPort = INT_PORT.PART_GENERATE;
                    SimEntity entity = ModelManager.Instance.AddFoup(port.Entity.Name);

                    SimPort newPort = new SimPort(intPort, this, entity);
                    EvtCalendar.AddEvent(simTime + GetIntervalTime(entity.Name), this, newPort);

                    break;

                default:
                    //Console.WriteLine("internal Func. error in CommitInter class");
                    break;
            }
        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.REQUEST_PART:
                    OutputFunction(simTime, simLogs, port);
                    break;

                case EXT_PORT.RESERVE:
                    _lstEntity.Add(port.Entity);
                    break;
            }
        }

        public override void OutputFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            SimNode node = EVTOutLink.EndNode;
            
            switch ((INT_PORT)port.PortType)
            {
                case INT_PORT.PART_GENERATE:
                    if(node is RailPortNode)
                    {
                        _lstEntity.Add(port.Entity);
                        Scheduler.Instance.commit(this, simTime, simLogs, port.Entity);

                        break;
                    }
                    else
                    {
                        if (node.Receivable)
                        {
                            SimPort newPort = new SimPort(EXT_PORT.PART, this, port.Entity);
                            node.ExternalFunction(simTime, simLogs, newPort);
                        }
                        else
                        {
                            ReserveEntites.Add(port);
                        }
                    }                    
                    break;

                default:
                    break;
            }

            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.REQUEST_PART:

                    SimPort newPort = null;

                    SimEntity en = port.Entity;
                    newPort = new SimPort(EXT_PORT.PART, this, en);
//                    _lstEntity.RemoveAt(i);
                    port.FromNode.ExternalFunction(simTime, simLogs, newPort);

                    // !Part out
                    SimLog log = new SimLog(simTime + 0.1, simTime + 0.1, en, this, ANIMATION.PART_ELIMINATION);
                    simLogs.AddLog(log);

                    ((Foup)en).TravelingStartTime = simTime;
                    break;
            }
        }

        public override SimEntity RequestEntity(SimNode simNode)
        {
            if(simNode is OHTNode)
            {
                SimEntity entity = _lstEntity[0];                
                _lstEntity.RemoveAt(0);

                return entity;
            }
            else
            {
                Console.WriteLine("??");
                return null;
            }
        }

        public Time GetIntervalTime(string entityName)
        {
            foreach (CommitIntervalData data in _intervalData)
            {
                if (data._entityName.Equals(entityName))
                    return (Time)data._interTime;
            }
            return 0;
        }
    }
}
