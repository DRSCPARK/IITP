using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Core;
using Pinokio.Simulation.Disc;

namespace Pinokio.Simulation.Models.IK
{
    public class Equipment : IKModel
    {
        private SimModel _mes;
        private SimModel _toolGroup;
        private EQPPort _port;

        private string _currentSetUpName;

        private Worker _assignedWorker;
        private SimTime _lastProcessEndTime;
        private SimTime _expectedProcessEndTime;
        private SimTime _setupFinishTime;
        private double _eventStartTime;
        private double _totalValueAddedTime;

        private const int _loadingTime = 5 * 60;
        private const int _unLoadingTime = 5 * 60;

        protected SimModel MES { get => _mes; }
        public AoFactoryEquipment EQPSpec { get => (AoFactoryEquipment)this.AbstractObj; }
        public SimModel ToolGroup { get => _toolGroup; }
        public EQPPort Port { get => _port; }
        public double EventStartTime { get => _eventStartTime; set => _eventStartTime = value; }
        public Worker AssignedWorker { get => _assignedWorker; }
        public int LoadingTime { get => _loadingTime; }
        public int UnLoadingTime { get => _unLoadingTime; }

        public Equipment(uint id, string name, Enum type) : base(id, name, type)
        {
            this.SetState(EQPState.Idle);
            _lastProcessEndTime = 0;
            _expectedProcessEndTime = 0;
            _setupFinishTime = 0;
            _assignedWorker = null;
        }

        public void SetMES(SimModel mes)
        {
            _mes = mes;
        }

        public void SetToolGroup(SimModel toolGroup)
        {
            _toolGroup = toolGroup;
        }

        public virtual void AddPort(EQPPort port)
        {
            _port = port;
            port.SetEquipment(this);
        }

        public override void InitializeModel(EventCalendar eventCalendar)
        {
            base.InitializeModel(eventCalendar);
            this.SetState(EQPState.Idle);
        }

        #region Simulation
        public override void InternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((EQPIntPort)port.Type)
            {
                case EQPIntPort.SetupFinish:
                    EQPPort eqpPort = this.Port;
                    var coil = (Coil)port.Entity;
                    this.SetState(EQPState.Reserved);
                    _totalValueAddedTime += (timeNow.TotalSeconds - _eventStartTime);
                    _eventStartTime = timeNow.TotalSeconds;
                    LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / {this.Name} / {this.State} / SETUP Finish / {coil.Name}");
                    CSVHelper.AddEventLog(timeNow.ToString(), this.Name, "NULL", _assignedWorker.Name, coil.Name, coil.CoilKind.ProductName, coil.ReleasedSimTime.ToString(), coil.DueSimTime.ToString(), "SETUP_FINISH");
                    _assignedWorker.ExternalTransition(timeNow, new SimPort(WKExtPort.SetupFinish));
                    eqpPort.ExternalTransition(timeNow, new SimPort(BuffExtPort.RequestCoil, coil) { Data = LoadingTime });
                    break;

                case EQPIntPort.FinishJob:
                    if (this.State is EQPState.Busy)
                    {
                        this.SetState(EQPState.UnLoading);
                        coil = (Coil)port.Entity;
                        this.Entities.Remove(coil);
                        _lastProcessEndTime = timeNow;
                        _totalValueAddedTime += (timeNow.TotalSeconds - _eventStartTime);
                        _eventStartTime = timeNow.TotalSeconds;
                        LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / {this.Name} / {this.State} / Process Finished by {_assignedWorker} / {port.Entity}");
                        CSVHelper.AddEventLog(timeNow.ToString(), this.Name, "NULL", _assignedWorker.Name, coil.Name, coil.CoilKind.ProductName, coil.ReleasedSimTime.ToString(), coil.DueSimTime.ToString(), "PROCESS_FINISH");
                        _assignedWorker.ExternalTransition(timeNow, new SimPort(WKExtPort.ProcessingFinish) { Data = UnLoadingTime });
                        ToolGroup.ExternalTransition(timeNow, new SimPort(TGExtPort.FinishJob, this, coil) { Data = UnLoadingTime });
                        _assignedWorker = null;
                    }
                    break;
            }
        }

        public override void ExternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((EQPExtPort)port.Type)
            {
                case EQPExtPort.SetUpStart:
                    var setup = (Setup)port.Data;
                    var coil = port.Entity as Coil;
                    coil.SetState(CoilState.SetUp);
                    this.SetState(EQPState.SetUp);
                    var setupTime = setup.Time.GetNumber();
                    EvtCalendar.AddEvent(timeNow + setupTime, this, new SimPort(EQPIntPort.SetupFinish, coil));
                    LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / {this.Name} / {this.State} / SETUP Start by {_assignedWorker} / {coil.Name}");
                    CSVHelper.AddEventLog(timeNow.ToString(), this.Name, "NULL", ((port.FromModel) as Worker).Name, coil.Name, coil.CoilKind.ProductName, coil.ReleasedSimTime.ToString(), coil.DueSimTime.ToString(), "SETUP_START");
                    _setupFinishTime = timeNow + setupTime;
                    _currentSetUpName = setup.Name;
                    break;

                case EQPExtPort.PassCoil:
                    coil = (Coil)port.Entity;
                    coil.SetState(CoilState.Processing);
                    this.SetState(EQPState.Busy);
                    this.Entities.Add(coil);
                    var processingTime = coil.GetProcessingTime();
                    coil.ValueAddedTime += processingTime;
                    _expectedProcessEndTime = timeNow + processingTime;
                    EvtCalendar.AddEvent(timeNow + processingTime, this, new SimPort(EQPIntPort.FinishJob, coil));
                    LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / {this.Name} / {this.State} / Process Start by {_assignedWorker} / {coil.Name}");
                    CSVHelper.AddEventLog(timeNow.ToString(), this.Name, "NULL", _assignedWorker.Name, coil.Name, coil.CoilKind.ProductName, coil.ReleasedSimTime.ToString(), coil.DueSimTime.ToString(), "PROCESS_START");
                    break;
            }
        }
        #endregion Simultion

        #region Observation
        public virtual EQPObsn GetObservation()
        {
            var obsn = new EQPObsn()
            {
                Name = this.Name,
                State = this.State.ToString(),
                ExpectedProcessEndTime = _expectedProcessEndTime.ToSecond(),
                LastProcessEndTime = _lastProcessEndTime.ToSecond(),
                SetupFinishTime = _setupFinishTime.TotalSeconds,
                CurrentSetUpName = _currentSetUpName,
                PortObsns = new PortObsn()
                {
                    Name = _port.Name,
                    State = _port.State.ToString(),
                    ContainingCoils = _port.Coils,
                }
            };
            return obsn;
        }
        #endregion Observation

        #region Methods
        public bool CheckToSetup(Setup setup)
        {
            if (setup is null) return false;
            else
            {
                if (this._currentSetUpName is null)
                    return true;
                else if (setup.Name != this._currentSetUpName)
                    return true;
                else
                    return false;
            }
        }
        public void SetIdleState()
        {
            this.SetState(EQPState.Idle);
        }
        public void SetLoadingState()
        {
            this.SetState(EQPState.Loading);
        }
        public void AssignWorker(Worker worker)
        {
            this._assignedWorker = worker;
        }
        #endregion Methods
    }
}
