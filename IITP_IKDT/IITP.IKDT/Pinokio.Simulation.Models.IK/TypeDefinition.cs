using System.Collections.Generic;
using System.Text;

namespace Pinokio.Simulation.Models.IK
{
    public enum FactoryToolType
    {
        Commit,
        Complete,
        ProcessEQP,
    }

    public enum JobType
    {
        Processing,
        Packaging,
    }

    public enum PlanType
    {
        Process,
        Packaging,
        Commit,
        Complete,
    }

    public enum MessageType
    {
        SEND_LAYOUT_MESSAGE,
        SEND_MPT_MESSAGE,
        REQUEST_LAOUT_RESULT,
        REQUEST_MPT_RESULT,
    }
}
