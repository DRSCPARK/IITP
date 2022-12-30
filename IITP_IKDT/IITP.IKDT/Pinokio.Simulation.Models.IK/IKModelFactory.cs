using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public static class IKModelFactory
    {
        public static SimModel AddFactoryModel(this NModelManager modelManager, string group, string name)
        {
            SimModel simModel = null;
            switch (group)
            {
                case "MES":
                    simModel = new MES(modelManager.GetNextModelId(), name);
                    break;
                case "Commit":
                    simModel = new Commit(modelManager.GetNextModelId(), name);
                    break;
                case "Complete":
                    simModel = new Complete(modelManager.GetNextModelId(), name);
                    break;
                case "ProcessEQP":
                    simModel = new ProcessEQP(modelManager.GetNextModelId(), name);
                    break;
                case "EQPPort":
                    simModel = new EQPPort(modelManager.GetNextModelId(), name);
                    break;
                case "ToolGroup":
                    simModel = new ToolGroup(modelManager.GetNextModelId(), name);
                    break;
                case "Worker":
                    simModel = new Worker(modelManager.GetNextModelId(), name);
                    break;
            }
            modelManager.AddSimModel(simModel);
            return simModel;
        }

    }
}
