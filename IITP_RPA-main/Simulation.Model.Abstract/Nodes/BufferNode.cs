using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simulation.Engine;
using Simulation.Geometry;

namespace Simulation.Model.Abstract
{
    /// <summary>
    /// Stocker와 Lifter 모델링에 사용 예정, 다수개의 Port를 가지고 있음.
    /// </summary>
    public class BufferNode : FabSimNode
    {
        #region Variables
        private BUFFER_TYPE _bufferType;
        private uint _capacity;
        private List<SimEntity> _lstSimEntity;
        private ProcessEqpNode _processNode;
        
        public BUFFER_TYPE BufferType
        {   get { return _bufferType; } }
        public ProcessEqpNode ProcessEqpNode
        {
            get { return _processNode; }
        }
        public List<SimEntity> LstEntity
        {
            get { return _lstSimEntity; }
        }
        public uint Capacity
        { get { return _capacity; } }

        #endregion

        public BufferNode()
            : base(0, "", null)
        {
            NodeType = NODE_TYPE.BUFFER;
            _capacity = 1;
        }
        public BufferNode(uint ID, Vector3 pos, string name, Fab fab, uint capa, BUFFER_TYPE type)
            : base(ID, name, pos, fab)
        {
            NodeType = NODE_TYPE.BUFFER;
            _bufferType = type;
            _capacity = capa;
            _lstSimEntity = new List<SimEntity>();
        }

        public BufferNode(uint ID, string name, Fab fab, uint capa, BUFFER_TYPE type)
    : base(ID, name, fab)
        {
            NodeType = NODE_TYPE.BUFFER;
            _bufferType = type;
            _capacity = capa;
            _lstSimEntity = new List<SimEntity>();
        }

        public BufferNode(uint ID, string name, BUFFER_TYPE Type, Fab fab)
    : base(ID, name, NODE_TYPE.BUFFER, fab)
        {
            NodeType = NODE_TYPE.BUFFER;
            this._bufferType = Type;
            _capacity = 1;
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            base.InitializeNode(evtCal);
            

        }

        public void SetProcessEqpNode(ProcessEqpNode processNode)
        {
            _processNode = processNode;
        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {            
        }

        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            //SimNode node = OutLink.EndNode;

            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.PART:
                    //Console.WriteLine("[" + simTime + "] : Buffer Part Ext ID : " + port.Entity.ID);
                    port.FromNode = this;
                    Scheduler.Instance.ReportBufferImport(simTime, simLogs, (Command)port);
                    _lstSimEntity.Add(port.Entity);
                    RefreshReceivable();
                    //OutputFunction(simTime, simLogs, port);
                    break;

                case EXT_PORT.REQUEST_PART:
                    OutputFunction(simTime, simLogs, port);
                    break;

                default:
                    break;
            }
        }

        public override void OutputFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            SimNode node = OutLinks[0].EndNode;
            
            switch ((EXT_PORT)port.PortType)
            {
                case EXT_PORT.PART:
                    if (node.Receivable)
                    {                      
                        _lstSimEntity.RemoveAt(_lstSimEntity.Count() - 1);
                        node.ExternalFunction(simTime, simLogs, new SimPort(EXT_PORT.PART, this, port.Entity));

                        if (Receivable == false)
                        {
                            RefreshReceivable();
                            if (Receivable == true)
                            {
                                SimPort rqPort = new SimPort(EXT_PORT.REQUEST_PART, this);
                                OutLinks[0].StartNode.ExternalFunction(simTime, simLogs, rqPort);
                            } 
                        }
                    }
                    else
                    {
                        ReserveEntites.Add(port);
                    }
                    break;
            }
        }

        public void RefreshReceivable()
        {
            if (_lstSimEntity.Count() == _capacity)
                Receivable = false;
            else if (_lstSimEntity.Count() > _capacity)
                Console.WriteLine("Capa. ERROR");
            else
                Receivable = true;
        }

        public override SimEntity RequestEntity(SimNode simNode)
        {
            if (simNode is OHTNode)
            {
                SimEntity entity = _lstSimEntity[0];
                _lstSimEntity.RemoveAt(0);
//                _isReservation = false;
                if (entity == null)
                    ;

                return entity;
            }
            else
            {
                Console.WriteLine("??");
                return null;
            }
        }

        public virtual uint GetRemainCapacity()
        {
            return 0;
        }
    }
}
