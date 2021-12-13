using Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class OhtThread
    {
        public int startOht;
        public int endOht;
        public bool isDone;
        public Thread thread;
        public Time simTime;

        public SimHistory simLogs;

        public OhtThread(int startOht, int endOht, Thread thread)
        {
            this.startOht = startOht;
            this.endOht = endOht;
            this.thread = thread;
        }

        public OhtThread(int startOht, int endOht, Thread thread, Time simTime, SimHistory simLogs)
        {
            this.startOht = startOht;
            this.endOht = endOht;
            this.thread = thread;
            this.simTime = simTime;
            this.simLogs = simLogs;
        }
    }
}
