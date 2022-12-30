using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Simulation.Disc;

namespace Pinokio.Simulation.Models.IK
{
    public class ToolGroup : IKModel
    {
        #region Variables
        private SimModel _mes;
        private Dictionary<string, Equipment> _equipments;
        private DispatchingResult _lastResult;
        #endregion Variables

        #region Properties
        public AoToolGroup Spec { get => (AoToolGroup)this.AbstractObj; }
        public Dictionary<string, Equipment> Equipments { get => _equipments; }
        public DispatchingResult LastResult { get => _lastResult; }

        #endregion Properties

        #region Constructor
        public ToolGroup(uint id, string name) : base(id, name, ModelType.None)
        {
            _equipments = new Dictionary<string, Equipment>();
        }
        #endregion Constructor

        #region Initialize
        public void AddEquipment(Equipment equipment)
        {
            equipment.SetToolGroup(this);
            _equipments.Add(equipment.Name, equipment);
        }

        public void SetMES(SimModel mes)
        {
            _mes = mes;
        }

        public override void InitializeModel(EventCalendar eventCalendar)
        {
            base.InitializeModel(eventCalendar);
            this.Entities.Clear();
            _lastResult = new DispatchingResult(_equipments.Keys.ToList());
        }
        #endregion Initialize

        #region Simulation
        public override void InternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((TGIntPort)port.Type)
            {
                case TGIntPort.Dispatch:
                    // Dispatch Time : Lot Release 당시만 사용
                    if (this.Entities.Count == 0) return;
                    var toolgroupObsn = this.GetObservation(this.Name, timeNow);
                    var newResult = IKDispatcher.Dispatch(toolgroupObsn, _lastResult);
                    var reservedCoils = newResult.GetReservedCoilNames();

                    foreach (Coil coil in this.Entities)
                    {
                        if (coil.State is CoilState.Processing) continue;
                        else if (coil.State is CoilState.Released && reservedCoils.Contains(coil.Name))
                        {
                            coil.SetState(CoilState.Reserved);
                            var eqp = _equipments[newResult.GetReservingEQPName(coil.Name)];
                            _mes.ExternalTransition(timeNow, new SimPort(MESExtPort.SendToEqpPort, this, eqp.Port, coil));
                        }
                    }
                    break;

                case TGIntPort.FinishJob:
                    var equipment = port.FromModel as Equipment;
                    var onPortCoils = equipment.Port.Entities;
                    var finishedCoil = (Coil)port.Entity;
                    var productName = finishedCoil.CoilKind.ProductName;

                    this.Entities.Remove(finishedCoil);
                    equipment.Port.Entities.Remove(finishedCoil);
                    _lastResult[equipment.Name].Remove(finishedCoil.Name);
                    equipment.SetIdleState();

                    List<CoilBatch> batches = (_mes as MES).BatchInfo[productName];
                    CoilBatch coilBatch = batches.Find(x => x.Coils.Contains(finishedCoil));

                    if (coilBatch.Coils.Last().Name == finishedCoil.Name)
                    {
                        SetCoilsPackaging(coilBatch);
                        _mes.ExternalTransition(timeNow, new SimPort(MESExtPort.SendToPackaging) { Data = coilBatch });
                    }
                    else
                        finishedCoil.SetState(CoilState.Waiting);

                    _mes.ExternalTransition(timeNow, new SimPort(MESExtPort.RequestDispatch));
                    break;
            }
        }

        public override void ExternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((TGExtPort)port.Type)
            {
                case TGExtPort.FinishJob: // EQP 작업 끝난 후
                    int unloadingTime = (int)port.Data;
                    EvtCalendar.AddEvent(timeNow + unloadingTime, this, new SimPort(TGIntPort.FinishJob, port.FromModel as Equipment, port.Entity as Coil));
                    break;

                case TGExtPort.NewJob:
                    var coil = (Coil)port.Entity;
                    coil.SetState(CoilState.Released);
                    this.Entities.Add(coil);
                    EvtCalendar.AddEvent(timeNow, this, new SimPort(TGIntPort.Dispatch));
                    break;
            }
        }
        #endregion Simulation

        #region Observation
        public ToolGroupObsn GetObservation(string toolGroupName, SimTime timeNow)
        {
            var obsn = new ToolGroupObsn(toolGroupName, this.Spec.ToolType);
            var equipments = this._equipments;

            var tasks = new List<Task>();
            var eqpObsns = new Dictionary<string, EQPObsn>();
            tasks.Add(Task.Run(() =>
            {
                foreach (var eqp in equipments.Values)    //_equipments.Values
                {
                    var eqpName = eqp.Name;
                    var eqpObsn = eqp.GetObservation();
                    eqpObsns.Add(eqpName, eqpObsn);
                }
            }));
            obsn.EQPObservations = eqpObsns;

            var coilObsns = new Dictionary<string, CoilObsn>();
            tasks.Add(Task.Run(() =>
            {
                foreach (Coil coil in this.Entities)
                {
                    coilObsns.Add(coil.Name, coil.GetObservation(timeNow));
                }
            }));
            obsn.CoilObservations = coilObsns;
            Task.WaitAll(tasks.ToArray());

            return obsn;
        }
        #endregion Observation

        #region Methods
        private void SetCoilsPackaging(CoilBatch coilBatch)
        {
            foreach (var coil in coilBatch.Coils)
            {
                coil.SetState(CoilState.Packaging);
            }
        }
        #endregion Methods
    }
}
