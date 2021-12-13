//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Simulation.Engine;
//using Simulation.Geometry;

//namespace Simulation.Model.Abstract
//{
//    public class ConveyorPointNode : SimNode
//    {
//        #region Variables
//        private int _entityID;
//        #endregion

//        public ConveyorPointNode(uint ID, Vector3 pos, string name)
//            : base(ID, name, pos)
//        {
//        }
//        public override void InitializeNode(EventCalendar evtCal)
//        {
//            base.InitializeNode(evtCal);
//            _entityID = -1;
//        }

//        public override void RequirePort(Time simTime, SimHistory simLogs)
//        {
//            RefreshReceivable();
//            InLinks[0].StartNode.RequirePort(simTime, simLogs);
//        }

//        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
//        {
//        }

//        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
//        {
//            if (port.Entity.ID == 0)
//                Console.WriteLine("[" + simTime + "] : Conveyor Point : " + port.Entity.ID);
//            SimNode node = OutLink.EndNode;

//            switch ((EXT_PORT)port.PortType)
//            {
//                case EXT_PORT.PART:
//                    _entityID = (int)port.Entity.ID;
//                    OutputFunction(simTime, simLogs, port);
//                    break;

//            }
//        }

//        public override void OutputFunction(Time simTime, SimHistory simLogs, SimPort port)
//        {
//            SimNode node = OutLink.EndNode;

//            switch ((EXT_PORT)port.PortType)
//            {
//                case EXT_PORT.PART:
//                    if (node.Receivable)
//                    {
//                        _entityID = -1;
//                        node.ExternalFunction(simTime, simLogs, new SimPort(EXT_PORT.PART, this, port.Entity));
//                    }
//                    else
//                    {
//                        SimLog log = new SimLog(simTime, simTime, port.Entity, ANIMATION.PART_WAITING);

//                    }
//                    RefreshReceivable();
//                    break;

//            }
//        }

//        public void RefreshReceivable()
//        {
//            if (_entityID == -1 && OutLink.EndNode.Receivable)
//                Receivable = true;
//            else
//                Receivable = false;
//        }
//    }
//}
