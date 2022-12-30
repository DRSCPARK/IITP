using Pinokio.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public class Worker : IKModel
    {
        #region Variables
        private SimModel _mes;
        private uint _processingCount;
        private uint _packagingCount;
        private double _workingTime;
        private SimTime _jobStartTime;
        private SimTime _jobEndTime;
        private double _totalThroughtput;
        private string _assignedEquipment;
        private const double _weekMinutes = 10080;
        #endregion Variables End

        #region KPI Variables
        private double _totalWorkingTime;
        private SimTime _lastGoToWorkTime;
        private double _utilization;
        #endregion KPI Variables End

        #region Properties
        public double WorkingTime { get => _workingTime; set => _workingTime = value; }
        public SimTime JobStartTime { get => _jobStartTime; set => _jobStartTime = value; }
        public SimTime JobEndTime { get => _jobEndTime; set => _jobEndTime = value; }
        public double TotalTroughput { get => _totalThroughtput; set => _totalThroughtput = value; }
        public string AssignedEquipment { get => _assignedEquipment; }
        public double Utilization { get => _utilization; set => _utilization = value; }

        #endregion Properies End

        public AoWorker Spec { get => (AoWorker)this.AbstractObj; }
        public Worker(uint id, string name) : base(id, name, ModelType.None)
        {
            _processingCount = 0;
            _packagingCount = 0;
            _totalWorkingTime = 0;
            _assignedEquipment = "NONE";
        }
        #region Initialize
        public void SetMES(SimModel mes)
        {
            _mes = mes;
        }
        public override void InitializeModel(EventCalendar eventCalendar)
        {
            base.InitializeModel(eventCalendar);
            EvtCalendar.AddEvent(0, this, new SimPort(WKIntPort.GoToFactory));
            EvtCalendar.AddEvent(_weekMinutes * 60, this, new SimPort(WKIntPort.CheckToBreak));
        }
        public void InitParameter(SimTime timeNow)
        {
            _workingTime = 0;
            _jobStartTime = 0;
            _jobEndTime = 0;
            _lastGoToWorkTime = timeNow;
        }
        #endregion Initialize

        #region Simulation
        public override void InternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((WKIntPort)port.Type)
            {
                case WKIntPort.CheckToBreak:
                    if (this.State is WorkerState.Idle)
                        EvtCalendar.AddEvent(timeNow, this, new SimPort(WKIntPort.LeaveTheFactory));
                    EvtCalendar.AddEvent(timeNow + (_weekMinutes * 60), this, new SimPort(WKIntPort.CheckToBreak));
                    break;

                case WKIntPort.GoToFactory:
                    this.SetState(WorkerState.Idle);
                    InitParameter(timeNow);
                    LogHandler.AddLog(LogLevel.Info, $"{this.Name} Go To {Spec.FactoryName} : {timeNow}");
                    _mes.ExternalTransition(timeNow, new SimPort(MESExtPort.RequestDispatch));
                    break;

                case WKIntPort.LeaveTheFactory:
                    this.SetState(WorkerState.Leave);
                    _totalWorkingTime += _workingTime;
                    EvtCalendar.AddEvent(timeNow + Spec.BreakTime, this, new SimPort(WKIntPort.GoToFactory));
                    LogHandler.AddLog(LogLevel.Info, $"{this.Name} Leave {Spec.FactoryName} : {timeNow} / Processing Count : {_processingCount} / Packaging Count : {_packagingCount}");
                    break;

                case WKIntPort.ProcessingFinish:
                    _processingCount++;
                    this.SetState(WorkerState.Idle);
                    _assignedEquipment = "NONE";
                    _jobEndTime = timeNow;
                    this._workingTime += (_jobEndTime - _jobStartTime).TotalSeconds;
                    if (!CheckToDoWork())
                        EvtCalendar.AddEvent(timeNow, this, new SimPort(WKIntPort.LeaveTheFactory));
                    break;

                case WKIntPort.PackagingFinish:
                    _packagingCount++;
                    var coil = (port.Data as List<Coil>).First();
                    //this.SetState(WorkerState.Idle);
                    //_assignedEquipment = "NONE";
                    //this._jobEndTime = timeNow;
                    //this._workingTime += (_jobEndTime - _jobStartTime).TotalSeconds;
                    //LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / Packaging Finished by {this.Name} / Coil Count : {(port.Data as List<Coil>).Count}");
                    LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / Packaging Finished by {this.Name} / Coil Name : {coil.Name}");
                    CSVHelper.AddEventLog(timeNow.ToString(), this.Name, "NULL", this.Name, coil.Name, coil.CoilKind.ProductName, coil.ReleasedSimTime.ToString(), coil.DueSimTime.ToString(), "PACKAGING_FINISH");

                    this.RemoveEntities((List<Coil>)port.Data);
                    _mes.ExternalTransition(timeNow, new SimPort(MESExtPort.PackagingFinish, this) { Data = port.Data });
                    //if (!CheckToDoWork())
                    //    EvtCalendar.AddEvent(timeNow, this, new SimPort(WKIntPort.LeaveTheFactory));
                    //else
                    //    _mes.ExternalTransition(timeNow, new SimPort(MESExtPort.RequestDispatch));
                    break;

                case WKIntPort.DeliveryFinish:
                    this.SetState(WorkerState.Idle);
                    _assignedEquipment = "NONE";
                    this._jobEndTime = timeNow;
                    this._workingTime += (_jobEndTime - _jobStartTime).TotalSeconds;

                    if (!CheckToDoWork())
                        EvtCalendar.AddEvent(timeNow, this, new SimPort(WKIntPort.LeaveTheFactory));
                    else
                        _mes.ExternalTransition(timeNow, new SimPort(MESExtPort.RequestDispatch));
                    break;

                case WKIntPort.DoNextJob:
                    var nextJob = (WorkerJob)port.Data;
                    double workingTime = CalculateWorkTime(nextJob);
                    this._jobStartTime = timeNow;

                    if (nextJob.Type is JobType.Processing)
                    {
                        var setup = nextJob.Coil.CoilKind.Setup;
                        _assignedEquipment = nextJob.EQP.Name;
                        nextJob.EQP.AssignWorker(this);

                        if (nextJob.EQP.CheckToSetup(setup))
                        {
                            this.SetState(WorkerState.SetUp);
                            nextJob.EQP.ExternalTransition(timeNow, new SimPort(EQPExtPort.SetUpStart, this, nextJob.Coil) { Data = setup });
                        }
                        else
                        {
                            this.SetState(WorkerState.Processing);
                            nextJob.EQP.Port.ExternalTransition(timeNow, new SimPort(BuffExtPort.RequestCoil, nextJob.Coil) { Data = nextJob.EQP.LoadingTime });
                        }
                    }
                    else
                    {
                        var batchCoil = nextJob.BatchCoil;
                        this.SetState(WorkerState.Packaging);
                        this.SetEntities(batchCoil);
                        this.SetTroughput(batchCoil);
                        _assignedEquipment = "PACKAGIING";
                        batchCoil.First().ValueAddedTime += workingTime;
                        EvtCalendar.AddEvent(timeNow + workingTime, this, new SimPort(WKIntPort.PackagingFinish) { Data = batchCoil });
                        LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / Packaging Start by {this.Name} / Coil : {batchCoil.First().Name} / Coil Count : {batchCoil.Count}");
                        CSVHelper.AddEventLog(timeNow.ToString(), "NULL", this.Name, this.Name, batchCoil.First().Name, batchCoil.First().CoilKind.ProductName, batchCoil.First().ReleasedSimTime.ToString(), batchCoil.First().DueSimTime.ToString(), "PACKAGING_START");
                    }

                    break;
            }
        }

        public override void ExternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((WKExtPort)port.Type)
            {
                case WKExtPort.Dispatch:
                    var type = this.Spec.JobType;
                    var jobQueue = (_mes as MES).JobQueue.FindAll(x => x.Type is JobType.Packaging);
                    double transportTime = 0;
                    double loadingTime = 0;

                    if (type is JobType.Packaging)
                    {
                        jobQueue = jobQueue.OrderBy(x => x.BatchCoil.First().DueSimTime).ToList();
                        foreach (var packaing in jobQueue)
                        {
                            transportTime = ((packaing.BatchCoil.First().CurrentPort as EQPPort).GetEquipment() as Equipment).EQPSpec.TPToComplete.GetNumber();

                            if (CheckWorkerCondition(packaing, transportTime))
                            {
                                EvtCalendar.AddEvent(timeNow, this, new SimPort(WKIntPort.DoNextJob) { Data = packaing });
                                this.SetState(WorkerState.Reserved);
                                (_mes as MES).RemoveJobQueue(packaing);
                                return;
                            }
                        }
                    }
                    else
                    {
                        jobQueue = (_mes as MES).JobQueue.FindAll(x => x.Type is JobType.Processing && x.EQP.State is EQPState.Idle);
                        jobQueue = jobQueue.OrderBy(x => x.Coil.DueSimTime).ToList();

                        foreach (var job in jobQueue)
                        {
                            loadingTime = job.EQP.LoadingTime;

                            if (CheckWorkerCondition(job, loadingTime))
                            {
                                EvtCalendar.AddEvent(timeNow, this, new SimPort(WKIntPort.DoNextJob) { Data = job });
                                LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / Transport Start by {this.Name} / Destination EQP: {job.EQP} / Coil : {job.Coil.Name}");
                                CSVHelper.AddEventLog(timeNow.ToString(), "COMMIT", job.EQP.Name, this.Name, job.Coil.Name, job.Coil.CoilKind.ProductName, job.Coil.ReleasedSimTime.ToString(), job.Coil.DueSimTime.ToString(), "TRANSPORT_EQP_START");
                                this.SetState(WorkerState.Reserved);
                                job.EQP.SetLoadingState();
                                (_mes as MES).RemoveJobQueue(job);
                                return;
                            }
                        }
                    }
                    break;

                case WKExtPort.ProcessingFinish:
                    int unloadingTime = (int)port.Data;
                    EvtCalendar.AddEvent(timeNow + unloadingTime, this, new SimPort(WKIntPort.ProcessingFinish));
                    break;

                case WKExtPort.DeliverToComplete:
                    transportTime = (double)port.Data;
                    EvtCalendar.AddEvent(timeNow + transportTime, this, new SimPort(WKIntPort.DeliveryFinish));
                    break;

                case WKExtPort.SetupFinish:
                    this.SetState(WorkerState.Processing);
                    break;

                case WKExtPort.ShowKPI:
                    int breakCount = (int)port.Data - 2;
                    _utilization = _totalWorkingTime / (timeNow - this.Spec.BreakTime * breakCount).TotalSeconds;
                    double utilization = _utilization * 100;
                    LogHandler.AddLog(LogLevel.Info, $" * Utilization Of {this.Spec.JobType} {this.Name} : {Math.Round(utilization, 3)}% ");
                    break;
            }
        }
        #endregion Simulation End

        #region Methods
        public bool CheckWorkerCondition(WorkerJob nextJob, double transportTime)
        {
            double toDoWorkingTime = CalculateWorkTime(nextJob);

            if (_workingTime + toDoWorkingTime + transportTime < Spec.WorkingTimeLimit) { return true; }
            else { return false; }
        }
        private double CalculateWorkTime(WorkerJob nextJob)
        {
            return nextJob.Type is JobType.Packaging ? GetTotalPackagingTime(nextJob.BatchCoil) : nextJob.Coil.CoilKind.ProcessingTime.GetNumber();
        }
        public bool CheckToDoWork()
        {
            if (_workingTime < Spec.WorkingTimeLimit) { return true; }
            else { return false; }
        }
        private double GetTotalPackagingTime(List<Coil> batchCoils)
        {
            double totalPackagingTime = 0;

            foreach (var coil in batchCoils)
                totalPackagingTime += coil.CoilKind.PackagingTime.GetNumber();

            return totalPackagingTime;
        }

        private void SetEntities(List<Coil> batchCoils)
        {
            foreach (var coil in batchCoils)
                this.Entities.Add(coil);
        }
        private void SetTroughput(List<Coil> batchCoils)
        {
            foreach (var coil in batchCoils)
                _totalThroughtput += coil.CoilKind.MaxThroughtput;
        }
        private void RemoveEntities(List<Coil> batchCoils)
        {
            foreach (var coil in batchCoils)
                this.Entities.Remove(coil);
        }
        #endregion Method EndS
    }
}
