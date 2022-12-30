using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

using Pinokio.Core;
using Pinokio.Map;
using Pinokio.Simulation.Disc;

namespace Pinokio.Simulation.Models.IK
{
    /// <summary>
    /// Class of EQP Uncoiler Port with Unlimited Capacity
    /// </summary>
    public class EQPPort : IKModel
    {
        private List<Coil> _coils;
        private SimModel _equipment;
        private SimModel _mes;

        public AoPort Spec { get => (AoPort)this.AbstractObj; }
        public List<Coil> Coils { get => _coils; }
        public EQPPort(uint id, string name) : base(id, name, ModelType.Port)
        {

        }

        public override void SetAbstractObj(AbstractObject aObj)
        {
            base.SetAbstractObj(aObj);
        }

        public void SetEquipment(SimModel simModel)
        {
            _equipment = simModel;
        }

        public void SetMES(SimModel mes)
        {
            _mes = mes;
        }

        public SimModel GetEquipment()
        {
            return _equipment;
        }

        public override void InitializeModel(EventCalendar eventCalendar)
        {
            base.InitializeModel(eventCalendar);
            this.SetState(PortState.Empty);
            this.Entities.Clear();
        }

        #region Simulation
        public override void InternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((BuffIntPort)port.Type)
            {
                case BuffIntPort.LoadToEQP:
                    var coil = (Coil)port.Entity;
                    this.Entities.Remove(coil);
                    _equipment.ExternalTransition(timeNow, new SimPort(EQPExtPort.PassCoil, coil));
                    break;
            }
        }

        public override void ExternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((BuffExtPort)port.Type)
            {
                case BuffExtPort.LoadPort:  // Coil Arrival
                    var coil = (Coil)port.Entity;
                    this.Entities.Add(coil);
                    coil.SetState(CoilState.OnPort);
                    (_mes as MES).AddCoilQueue(timeNow, coil.CoilKind.ProcessingTime.GetNumber(), (_equipment as Equipment), coil);   // Coil EQP 도착 당시에 큐 ADD
                    if ((_equipment as Equipment).State is EQPState.Idle)
                        _mes.ExternalTransition(timeNow, new SimPort(MESExtPort.RequestDispatch));
                    break;

                case BuffExtPort.RequestCoil:  // EQP에서 요청 >> Uncoiler에 코일 로딩
                    coil = (Coil)port.Entity;
                    var loadingTime = (int)port.Data;
                    coil.SetState(CoilState.Loading);
                    EvtCalendar.AddEvent(timeNow + loadingTime, this, new SimPort(BuffIntPort.LoadToEQP, coil));
                    LogHandler.AddLog(LogLevel.Info, $" Loading Start / EQP: {this._equipment} / Coil : {coil.Name} / TNOW : {timeNow}");
                    break;
            }
        }
        #endregion Simulation
    }
}
