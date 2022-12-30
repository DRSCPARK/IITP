using Pinokio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public class Complete : Equipment
    {
        public AoComplete Spec { get => (AoComplete)this.AbstractObj; }

        public static int OnTimeDeliveryCount;
        public static uint TotalCoilCount;
        public static double CycleTime;
        public static double TotalCycleTime = 0;
        public static double Ratio = 0;
        public static double ProcessingUtils = 0;
        public static double PackagingUtils = 0;

        public Complete(uint id, string name) : base(id, name, ModelType.Complete)
        { }

        #region Initialize
        public override void AddPort(EQPPort port)
        {
            base.AddPort(port);
            this.SetInitLocaiton(port.Location);
        }
        public override void InitializeModel(EventCalendar eventCalendar)
        {
            base.InitializeModel(eventCalendar);
            OnTimeDeliveryCount = 0;
            TotalCoilCount = 0;
            CycleTime = 0;
            TotalCycleTime = 0;
            Ratio = 0;
            ProcessingUtils = 0;
            PackagingUtils = 0;
        }
        #endregion

        #region Simulation
        public override void InternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((EQPIntPort)(port.Type))
            {
                case EQPIntPort.CoilOut:
                    bool onDelivered = false;
                    Coil coil = port.Entity as Coil;
                    TotalCoilCount++;
                    CycleTime = coil.ValueAddedTime;
                    TotalCycleTime += CycleTime;

                    if (timeNow < coil.DueSimTime)
                    {
                        onDelivered = true;
                        OnTimeDeliveryCount++;
                    }

                    LogHandler.AddLog(LogLevel.Info, $" Coil Out : {coil.Name} / Cycle Time : {CycleTime} / On Time Delivered : {onDelivered} / Coil Out Time : {timeNow}");
                    CSVHelper.AddEventLog(timeNow.ToString(), this.Name, "NULL", (port.FromModel).Name, coil.Name, coil.CoilKind.ProductName, coil.ReleasedSimTime.ToString(), coil.DueSimTime.ToString(), "COMPLETE");

                    break;
            }
        }

        public override void ExternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((EQPExtPort)(port.Type))
            {
                case EQPExtPort.SendToComplete:
                    double transportTime = (double)(port.Data);
                    EvtCalendar.AddEvent(timeNow + transportTime, this, new SimPort(EQPIntPort.CoilOut, (port.FromModel as Worker), port.Entity));
                    break;
                case EQPExtPort.ShowKPI:
                    Ratio = (double)OnTimeDeliveryCount / (double)TotalCoilCount;
                    var ratio = Ratio * 100;
                    LogHandler.AddLog(LogLevel.Info, $" * Total Complete Coil Count {TotalCoilCount} / On Time Delivered Coil Count : {OnTimeDeliveryCount} / Ratio : {ratio}%");
                    break;
            }
        }

        #endregion Simulation
    }
}
