using Pinokio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public class Commit : Equipment
    {
        #region Variables
        private static uint _lastCoilId = 0;
        private Dictionary<string, int> _planIndices;
        #endregion Vriables

        #region Properties
        public AoCommit Spec { get => (AoCommit)this.AbstractObj; }
        public static uint LastCoilId { get => _lastCoilId; set => _lastCoilId = value; }
        public Dictionary<string, int> PlanIndices { get => _planIndices; set => _planIndices = value; }
        #endregion Properties

        #region Constructor
        public Commit(uint id, string name) : base(id, name, ModelType.Commit)
        {
            _planIndices = new Dictionary<string, int>();
        }
        #endregion Constructor

        #region Initialize
        public override void AddPort(EQPPort port)
        {
            base.AddPort(port);
        }

        public override void InitializeModel(EventCalendar eventCalendar)
        {
            base.InitializeModel(eventCalendar);
            _planIndices.Clear();

            foreach (var typeName in Spec.CoilKinds.Keys)
            {
                _planIndices.Add(typeName, 0);
                Plan firstPlan = Spec.Plans[typeName][0];
                var arrivalTime = new SimTime((firstPlan.StartDateTime - SimParameter.StartDateTime).TotalSeconds);
                EvtCalendar.AddEvent(arrivalTime, this, new SimPort(EQPIntPort.CoilRelease) { Data = typeName });
            }
        }

        public override void InternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((EQPIntPort)port.Type)
            {
                case EQPIntPort.CoilRelease:
                    var typeName = (string)port.Data;
                    int currentIndex = _planIndices[typeName];
                    Plan currentPlan = Spec.Plans[typeName][currentIndex];
                    var coil = new Coil(_lastCoilId, currentPlan.Name, timeNow, new SimTime(currentPlan.DueDateTime - SimParameter.StartDateTime).TotalSeconds);
                    coil.SetAbstractObj(Spec.CoilKinds[typeName]);
                    coil.SetCurrentPort(this);
                    this.Entities.Add(coil);
                    (MES as MES).UpdateCoilBatches(coil);
                    LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / {this.Name} / Coil Release / {coil.Name}");
                    CSVHelper.AddEventLog(timeNow.ToString(), this.Name, "NULL", "NULL", coil.Name, coil.CoilKind.ProductName, coil.ReleasedSimTime.ToString(), coil.DueSimTime.ToString(), "COMMIT");

                    MES.ExternalTransition(timeNow, new SimPort(MESExtPort.SendToProcessing, coil));

                    if (Spec.Plans[typeName].Count > currentIndex + 1)
                    {
                        Plan nextPlan = Spec.Plans[typeName][currentIndex + 1];
                        var arrivalTime = new SimTime((nextPlan.StartDateTime - SimParameter.StartDateTime).TotalSeconds);
                        EvtCalendar.AddEvent(arrivalTime, this, port);
                        _planIndices[typeName]++;
                    }
                    else
                    {
                        Console.WriteLine("Release Finish " + typeName);
                    }

                    this.Entities.Remove(coil);
                    break;
            }
        }
        #endregion
    }
}
