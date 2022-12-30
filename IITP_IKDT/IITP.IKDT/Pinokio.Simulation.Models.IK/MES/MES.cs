using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Pinokio.Core;
using Pinokio.Map;
using Pinokio.Simulation.Disc;
using Pinokio.Simulation.Models.IK;


namespace Pinokio.Simulation.Models.IK
{
    public class MES : DiscSimModel
    {
        private List<Complete> _completes;
        private List<Commit> _commits;
        private List<WorkerJob> _jobQueue;
        private Dictionary<string, Setup> _setups;
        private Dictionary<string, ToolGroup> _toolGroups;
        private Dictionary<string, Equipment> _equipments;
        private Dictionary<string, EQPPort> _ports;
        private Dictionary<string, Coil> _coils;
        private Dictionary<string, List<CoilBatch>> _products;
        private Dictionary<string, Worker> _workers;

        private int weekCount;
        private const double _weekMinutes = 10080;

        #region Properties
        public List<Complete> Completes { get => _completes; }
        public List<Commit> Commits { get => _commits; }
        public List<WorkerJob> JobQueue { get => _jobQueue; }
        public Dictionary<string, Setup> Setups { get => _setups; }
        public Dictionary<string, Equipment> Equipments { get => _equipments; }
        public Dictionary<string, Coil> Coils { get => _coils; }
        public Dictionary<string, ToolGroup> ToolGroups { get => _toolGroups; }
        public Dictionary<string, Worker> Workers { get => _workers; }
        public Dictionary<string, List<CoilBatch>> BatchInfo { get => _products; }
        #endregion Properties

        public MES(uint id, string fabName) : base(id, "MES_" + fabName, ModelType.None)
        {
            _completes = new List<Complete>();
            _commits = new List<Commit>();
            _jobQueue = new List<WorkerJob>();
            _setups = new Dictionary<string, Setup>();
            _toolGroups = new Dictionary<string, ToolGroup>();
            _equipments = new Dictionary<string, Equipment>();
            _ports = new Dictionary<string, EQPPort>();
            _coils = new Dictionary<string, Coil>();
            _products = new Dictionary<string, List<CoilBatch>>();
            _workers = new Dictionary<string, Worker>();
        }

        public void AddToolGroup(ToolGroup toolGroup)
        {
            if (!_toolGroups.ContainsKey(toolGroup.Name))
            {
                _toolGroups.Add(toolGroup.Name, toolGroup);
                toolGroup.SetMES(this);
            }
        }

        public void AddProducts(string productName)
        {
            if (_products.ContainsKey(productName) == false)
            {
                List<CoilBatch> coilBatches = new List<CoilBatch>();
                _products.Add(productName, coilBatches);
            }
        }

        public void AddEquipment(Equipment equipment)
        {
            equipment.SetMES(this);
            _equipments.Add(equipment.Name, equipment);
            var toolGroup = _toolGroups[equipment.EQPSpec.ToolGroupName];
            toolGroup.AddEquipment(equipment);
            if (equipment is Complete complete)
            {
                _completes.Add(complete);
            }
            else if (equipment is Commit commit)
            {
                _commits.Add(commit);
            }
        }

        public void AddPort(EQPPort port, string eqpName)
        {
            port.SetMES(this);
            if (!_ports.ContainsKey(port.Name))
            {
                if (eqpName == string.Empty)
                    return;

                if (_equipments.ContainsKey(eqpName))
                    _equipments[eqpName].AddPort(port);
            }
        }

        public void AddWorker(Worker worker)
        {
            if (_workers.ContainsKey(worker.Name) == false)
            {
                _workers.Add(worker.Name, worker);
                worker.SetMES(this);
            }
        }

        public void AddSetUp(Setup setUp)
        {
            if (_setups.ContainsKey(setUp.ProductName) == false)
                _setups.Add(setUp.ProductName, setUp);
        }

        public Setup GetSetUpInformation(string productName)
        {
            if (_setups.ContainsKey(productName))
                return _setups[productName];
            else
                return null;
        }

        public override void InitializeModel(EventCalendar eventCalendar)
        {
            base.InitializeModel(eventCalendar);
            weekCount = 1;
            EvtCalendar.AddEvent(_weekMinutes * 60, this, new SimPort(MESIntPort.WeekAlram));
            EvtCalendar.AddEvent(7948801, this, new SimPort(MESIntPort.EndOfSimulation));
        }

        #region Simulation
        public override void InternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((MESIntPort)port.Type)
            {
                case MESIntPort.Debug:
                    break;
                case MESIntPort.WeekAlram:
                    LogHandler.AddLog(LogLevel.Info, $" ********** {this.Name} WEEK {weekCount} END !!! / Remain Job Count : {_jobQueue.Count} **********");
                    weekCount++;
                    EvtCalendar.AddEvent(timeNow + (_weekMinutes * 60), this, new SimPort(MESIntPort.WeekAlram));
                    break;
                case MESIntPort.Arrive:
                    var currentPort = port.FromModel as EQPPort;
                    (port.Entity as Coil).SetCurrentPort(currentPort);
                    currentPort.ExternalTransition(timeNow, new SimPort(BuffExtPort.LoadPort, port.Entity));
                    break;
                case MESIntPort.EndOfSimulation:
                    LogHandler.AddLog(LogLevel.Info, $" *************** END OF SIMULATION : {timeNow - 1} / Remain Job Count : {_jobQueue.Count} ***************");
                    double processerCount = 0;
                    double packagerCount = 0;

                    foreach (var worker in _workers.Values)
                    {
                        worker.ExternalTransition(timeNow, new SimPort(WKExtPort.ShowKPI, this) { Data = weekCount });
                        if (worker.Spec.JobType is JobType.Processing)
                        {
                            Complete.ProcessingUtils += worker.Utilization;
                            processerCount++;
                        }
                        else
                        {
                            Complete.PackagingUtils += worker.Utilization;
                            packagerCount++;
                        }
                    }
                    Complete.ProcessingUtils /= processerCount;
                    Complete.PackagingUtils /= packagerCount;

                    foreach (var complete in _completes)
                    {
                        if (complete.Name == "COMPLETE2")
                            break;
                        complete.ExternalTransition(timeNow, new SimPort(EQPExtPort.ShowKPI));
                    }
                    LogHandler.AddLog(LogLevel.Info, $" ***********************************************************************************");

                    break;
            }
        }

        public override void ExternalTransition(SimTime timeNow, SimPort port)
        {
            switch ((MESExtPort)port.Type)
            {
                case MESExtPort.RequestDispatch:
                    var idleWorkers = _workers.Values.Where(x => x.State is WorkerState.Idle);

                    foreach (var idleWorker in idleWorkers)
                    {
                        idleWorker.ExternalTransition(timeNow, new SimPort(WKExtPort.Dispatch));
                    }
                    break;

                case MESExtPort.PackagingFinish:
                    var coilBatch = (List<Coil>)(port.Data);
                    double transportTime = 0;

                    RemoveCoilBatches(coilBatch);
                    foreach (var _coil in coilBatch)
                    {
                        Complete complete = _completes.Find(x => x.Spec.CoilKinds.ContainsKey(_coil.CoilKind.Name));
                        transportTime = ((_coil.CurrentPort as EQPPort).GetEquipment() as Equipment).EQPSpec.TPToComplete.GetNumber();
                        _coil.ValueAddedTime += transportTime;
                        complete.ExternalTransition(timeNow, new SimPort(EQPExtPort.SendToComplete, (port.FromModel as Worker), _coil) { Data = transportTime });
                        LogHandler.AddLog(LogLevel.Info, $"{timeNow,-7} / Transport Start by {(port.FromModel).Name} / Destination EQP: {complete.Name} / Coil : {_coil.Name}");
                        CSVHelper.AddEventLog(timeNow.ToString(), (port.FromModel).Name, complete.Name, (port.FromModel).Name, _coil.Name, _coil.CoilKind.ProductName, _coil.ReleasedSimTime.ToString(), _coil.DueSimTime.ToString(), "TRANSPORT_COMPLETE_START");
                    }
                    port.FromModel.ExternalTransition(timeNow, new SimPort(WKExtPort.DeliverToComplete) { Data = transportTime });
                    break;

                case MESExtPort.SendToPackaging:
                    var batchCoils = (CoilBatch)port.Data;
                    double processingTime = 0;

                    foreach (var batch in batchCoils.Coils)
                        processingTime += batch.CoilKind.PackagingTime.GetNumber();

                    AddJobQueue(timeNow, processingTime, batchCoils.Coils);

                    break;

                case MESExtPort.SendToProcessing: // COMMIT >> TOOL GROUP
                    var coil = (Coil)port.Entity;
                    _coils.Add(coil.Name, coil);
                    string toolGroupName = coil.CoilKind.ToolGroupName;
                    ToolGroup nextToolGroup = _toolGroups[toolGroupName];
                    nextToolGroup.ExternalTransition(timeNow, new SimPort(TGExtPort.NewJob, coil));
                    break;

                case MESExtPort.SendToEqpPort:
                    coil = (Coil)port.Entity;
                    var fromPort = coil.CurrentPort;
                    var toPort = port.ToModel as EQPPort;
                    transportTime = (toPort.GetEquipment() as Equipment).EQPSpec.TPFromCommit.GetNumber();
                    EvtCalendar.AddEvent(timeNow + transportTime, this, new SimPort(MESIntPort.Arrive, toPort, coil));
                    CSVHelper.AddEventLog(timeNow.ToString(), "COMMIT", toPort.GetEquipment().Name, null, coil.Name, coil.CoilKind.ProductName, coil.ReleasedSimTime.ToString(), coil.DueSimTime.ToString(), "TRANSPORT_EQP_FINISH");
                    coil.ValueAddedTime += transportTime;
                    break;
            }
        }
        #endregion Simulation

        #region Methods
        public double GetTransportTime(SimModel fromModel, SimModel toMdoel)
        {
            double transportTime = 60;

            var position = toMdoel.Position;

            return transportTime;
        }

        private void AddJobQueue(SimTime timeNow, double processingTime, List<Coil> batchCoil)
        {
            var jobQueue = _jobQueue.FindAll(x => x.Type is JobType.Packaging);

            foreach (var q in jobQueue)
            {
                if (q.BatchCoil.First().Name == batchCoil.First().Name)
                    return;
            }

            WorkerJob job = new WorkerJob();
            job.Type = JobType.Packaging;
            job.EnqueueTime = timeNow;
            job.ProcessingTime = processingTime;
            job.BatchCoil = batchCoil;
            _jobQueue.Add(job);
        }

        public void AddCoilQueue(SimTime timeNow, double processingTime, Equipment eqp, Coil coil)
        {
            WorkerJob job = new WorkerJob();
            job.Type = JobType.Processing;
            job.EnqueueTime = timeNow;
            job.ProcessingTime = processingTime;
            job.EQP = eqp;
            job.Coil = coil;
            _jobQueue.Add(job);
        }

        public void RemoveJobQueue(WorkerJob job)
        {
            //_jobQueue.Remove(job);  //에러나는지 확인

            List<WorkerJob> jobQueue;

            if (job.Type is JobType.Processing)
            {
                jobQueue = _jobQueue.FindAll(x => x.Type is JobType.Processing);
                if (jobQueue.Any(x => x.Coil.Name == job.Coil.Name))
                {
                    var queueIndex = _jobQueue.FindIndex(x => x.Type is JobType.Processing && x.Coil.Name == job.Coil.Name);
                    _jobQueue.RemoveAt(queueIndex);
                    return;
                }
            }
            else
            {
                jobQueue = _jobQueue.FindAll(x => x.Type is JobType.Packaging);
                foreach (var q in jobQueue)
                {
                    if (q.BatchCoil.First().Name == job.BatchCoil.First().Name)
                    {
                        var queueIndex = _jobQueue.FindIndex(x => x.Type is JobType.Packaging && x.BatchCoil.First().Name == job.BatchCoil.First().Name);
                        _jobQueue.RemoveAt(queueIndex);
                        return;
                    }
                }
            }
        }

        public CoilBatch GetCoilBatch(Coil coil)
        {
            string productName = coil.CoilKind.ProductName;
            List<CoilBatch> batches = _products[productName];
            return batches.Find(x => x.CoilKindName == coil.CoilKind.Name && x.StartTime == coil.ReleasedSimTime && x.DueDate == coil.DueSimTime);
        }

        public void UpdateCoilBatches(Coil coil)
        {
            string productName = coil.CoilKind.ProductName;

            foreach (var batch in _products[productName])
            {
                //if (batch.CoilKindName == coil.CoilKind.Name && batch.StartTime == coil.ReleasedSimTime && batch.DueDate == coil.DueSimTime)
                //{
                //    batch.Coils.Add(coil);
                //    return;
                //}

                if (batch.Coils.First().Name == coil.Name)
                { // 이경우 있으면안됨
                    batch.Coils.Add(coil);
                    return;
                }
            }

            CoilBatch newBatch = new CoilBatch(coil);
            _products[productName].Add(newBatch);
        }

        public void RemoveCoilBatches(List<Coil> coilBatch)
        {
            Coil standardCoil = coilBatch.First();
            string productName = standardCoil.CoilKind.ProductName;

            foreach (var batch in _products[productName])
            {
                if (batch.CoilKindName == standardCoil.CoilKind.Name && batch.StartTime == standardCoil.ReleasedSimTime && batch.DueDate == standardCoil.DueSimTime)
                {
                    _products[productName].Remove(batch);
                    return;
                }
            }
        }
        #endregion Methods
    }
}
