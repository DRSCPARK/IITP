using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public enum EQPState
    {
        Idle,
        Busy,
        Reserved,
        SetUp,
        UnLoading,
        Loading,
    }

    public enum PortState
    {
        Empty,
        Full,
        Reserved,
    }

    public enum CoilState
    {
        Released,
        Processing,
        Reserved,  // Dispatching 직후
        OnPort,

        Loading,
        Unloading,
        Waiting, // (For Packaging)
        Packaging,
        SetUp,
    }

    public enum WorkerState
    {
        Processing,
        Packaging,
        Idle,
        Leave,
        Reserved,
        SetUp,
    }
}
