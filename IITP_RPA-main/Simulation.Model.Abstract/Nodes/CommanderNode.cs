using Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class CommanderNode : SimNode
    {
        int _totalCommandCount;
        int _activeCommandCount;
        List<Command> _inactiveCommands;
        int assignInterval;
        int selectInterval;

        public List<Command> LstInactiveCommand
        { 
            get { return _inactiveCommands; } 
            set { _inactiveCommands = value; }
        }

        public CommanderNode(uint id, List<Command> inactiveCommands)
        {
            this.ID = id;
            _inactiveCommands = inactiveCommands;
            assignInterval = 1;
            selectInterval = 600;
        }

        public override void InitializeNode(EventCalendar evtCal)
        {
            SimPort port;
            base.InitializeNode(evtCal);

            _totalCommandCount = SimModelDBManager.Instance.SelectCommandCount();

            if(ModelManager.Instance.SimType == SIMULATION_TYPE.MATERIAL_HANDLING && _totalCommandCount > 0)
            {
                port = new SimPort(INT_PORT.COMMAND_SELECT);
                evtCal.AddEvent(0, this, port);
            }

            port = new SimPort(INT_PORT.COMMAND_ASSIGN);
            evtCal.AddEvent(0.01, this, port);
        }


        public override void ExternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {

        }

        public override void InternalFunction(Time simTime, SimHistory simLogs, SimPort port)
        {
            switch(port.PortType)
            {
                case INT_PORT.COMMAND_SELECT:
                    SimModelDBManager.Instance.SelectCommand(SimEngine.Instance.StartDateTime.AddSeconds(simTime.TotalSeconds), selectInterval);
                    Scheduler.Instance.ActivateCommand(simTime);
                    EvtCalendar.AddEvent(simTime + selectInterval, this, port);
                    break;
                case INT_PORT.COMMAND_ASSIGN:
                    if (ModelManager.Instance.SimType == SIMULATION_TYPE.MATERIAL_HANDLING)
                        Scheduler.Instance.ActivateCommand(simTime);

                    Scheduler.Instance.AssignCommands(simTime, simLogs);
                    EvtCalendar.AddEvent(simTime + assignInterval, this, port);
                    break;
            }
        }

    }
}
