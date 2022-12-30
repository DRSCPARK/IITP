using System;
using System.Collections.Generic;
using Pinokio.Map;
using Pinokio.Geometry;
using Pinokio.Simulation.Models.IK;

namespace Pinokio.Simulation.FACTORY
{
    public class Factory : PinokioMap
    {
        #region Variables
        protected uint LastToolGroupId;
        protected uint LastWorkerId;

        private static uint _factoryId = 1;
        private static uint resourceId = 1;
        private NModelManager _modelManager;
        private MES _mes;

        private Dictionary<string, AoFactoryEquipment> _equipments;
        private Dictionary<string, Product> _products;
        private Dictionary<string, AoToolGroup> _toolGroups;
        private Dictionary<string, CoilKind> _coilKinds;
        private Dictionary<string, AoWorker> _workers;
        private Dictionary<string, AoPort> _portTypes;
        private Dictionary<string, string> _productToolgroup;
        private Dictionary<string, string> _toolGroupFactory;

        #endregion Variables

        #region Properties
        public MES MES { get => _mes; }
        public Dictionary<string, AoFactoryEquipment> Equipments { get => _equipments; }
        public Dictionary<string, AoWorker> Workers { get => _workers; }
        public Dictionary<string, AoPort> PortTypes { get => _portTypes; }
        public Dictionary<string, string> ProductToolGroup { get => _productToolgroup; }
        public Dictionary<string, string> ToolGroupFactory { get => _toolGroupFactory; }

        #endregion Properties

        #region Constructor
        public Factory(string factoryName, NModelManager nmodelManager) : base(_factoryId++, factoryName)
        {
            _modelManager = nmodelManager;
            _mes = (MES)_modelManager.AddFactoryModel("MES", factoryName);

            _equipments = new Dictionary<string, AoFactoryEquipment>();
            _products = new Dictionary<string, Product>();
            _toolGroups = new Dictionary<string, AoToolGroup>();
            _workers = new Dictionary<string, AoWorker>();
            _coilKinds = new Dictionary<string, CoilKind>();
            _portTypes = new Dictionary<string, AoPort>();
            _toolGroupFactory = new Dictionary<string, string>();
            _productToolgroup = new Dictionary<string, string>();
        }
        #endregion Constructor

        #region Initialize
        public override void Initialize()
        {
            base.Initialize();
            LastToolGroupId = 0;
            LastWorkerId = 0;
        }
        #endregion Initialize

        #region Generating Methods
        public override AoResource GenerateResource(string name, Vector3 pos, string type)
        {
            AoResource resource = null;

            ResourceType resourceType = ResourceType.None;

            if (!Enum.TryParse(type, true, out resourceType))
                resourceType = ResourceType.Process;

            switch (resourceType)
            {
                case ResourceType.Commit:
                    resource = new AoCommit(this.Id, ++LastResourceId, name);
                    break;
                case ResourceType.Complete:
                    resource = new AoComplete(this.Id, ++LastResourceId, name);
                    break;
                default:
                    resource = new AoFactoryEquipment(this.Id, ++LastResourceId, name);
                    break;
            }
            resource.SetSize(pos);
            resource.SetPosition(pos);
            return resource;
        }

        public AoToolGroup GenerateToolGroup(string name, string toolTypeString)
        {
            if (Enum.TryParse(toolTypeString, true, out FactoryToolType toolType))
            {
                return new AoToolGroup(this.Name, ++LastToolGroupId, name, toolType);
            }
            else
            {
                return null;
            }
        }

        public Product GenerateProduct(string name, string toolGroup)
        {
            var product = new Product() { Name = name, ToolGroup = toolGroup };
            return product;
        }

        public CoilKind GenerateCoilKind(string name, string productName)
        {
            var coilType = new CoilKind(name, productName);
            return coilType;
        }
        public AoWorker GenerateWorker(string name, string factoryName, double workingTime, string timeUnit, string type)
        {
            if (Enum.TryParse(type, true, out JobType jobType))
            {
                return new AoWorker(factoryName, workingTime, timeUnit, jobType, ++LastWorkerId, name);
            }
            return null;
        }

        public Plan GeneratePlan(string name, string coilKindName, uint standard, string standardUnit, DateTime startDate, DateTime dueDate, string factoryName)
        {
            var plan = new Plan()
            {
                Name = name,
                CoilKindName = coilKindName,
                Standard = standard,
                StandardUnit = standardUnit,
                StartDateTime = startDate,
                DueDateTime = dueDate,
                FactoryName = factoryName,
            };

            return plan;
        }
        #endregion Generating Methods

        #region Adding Methods
        public override void AddResource(AoResource resource)
        {
            base.AddResource(resource);

            SimModel model = null;
            if (resource is AoFactoryEquipment factoryEQP)
            {
                if (_toolGroups.ContainsKey(factoryEQP.ToolGroupName))
                {
                    var aoToolGroup = _toolGroups[factoryEQP.ToolGroupName];
                    model = _modelManager.AddFactoryModel(aoToolGroup.ToolType.ToString(), factoryEQP.Name);
                    model.SetAbstractObj(factoryEQP);
                    _mes.AddEquipment((Equipment)model);

                    _equipments.Add(factoryEQP.Name, factoryEQP);
                }
            }
        }

        public AoPort AddPortType(string portName, string typeString)
        {
            if (!_portTypes.ContainsKey(portName))
            {
                if (Enum.TryParse<PortType>(typeString, true, out PortType type))
                {
                    var port = new AoPort(this.Id, portName, type);
                    _portTypes.Add(portName, port);
                    return port;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return _portTypes[portName];
            }
        }

        public void AddPort(string name, AoPort portType, string eqpName, string typeString)
        {
            if (portType.PortType is PortType.Commit || portType.PortType is PortType.Complete)
                typeString = "EQPPort";

            var portModel = (EQPPort)_modelManager.AddFactoryModel(typeString, name);

            portModel.SetAbstractObj(portType);

            _mes.AddPort(portModel, eqpName);
        }

        public void AddToolGroup(AoToolGroup toolGroup)
        {
            if (!_toolGroups.ContainsKey(toolGroup.Name))
            {
                _toolGroups.Add(toolGroup.Name, toolGroup);
                var toolGroupModel = _modelManager.AddFactoryModel("ToolGroup", toolGroup.Name);
                _mes.AddToolGroup((ToolGroup)toolGroupModel);

                toolGroupModel.SetAbstractObj(toolGroup);

                //ToolGroupHelper.ToolGroup.Add(toolGroup.Name, (ToolGroup)toolGroupModel);
            }
        }
        public void AddProduct(Product product)
        {
            _products.Add(product.Name, product);
            _mes.AddProducts(product.Name);
        }

        public void AddCoilKind(CoilKind coilKind)
        {
            if (_products.ContainsKey(coilKind.ProductName))
            {
                _coilKinds.Add(coilKind.Name, coilKind);

                string commitName = null;
                string completeName = null;

                string factoryName = GetFactoryName(coilKind.ProductName);

                switch (factoryName)
                {
                    case "FACTORY1":
                        commitName = "COMMIT1";
                        completeName = "COMPLETE1";
                        break;
                    case "FACTORY2":
                        commitName = "COMMIT2";
                        completeName = "COMPLETE2";
                        break;
                }

                if (_equipments.ContainsKey(commitName))
                {
                    var commit = (AoCommit)_equipments[commitName];
                    commit.AddCoilKind(coilKind);
                }

                if (_equipments.ContainsKey(completeName))
                {
                    var complete = (AoComplete)_equipments[completeName];
                    complete.AddCoilKind(coilKind);
                }
            }
        }
        public void AddPlan(Plan plan)
        {
            if (_coilKinds.ContainsKey(plan.CoilKindName))
            {
                var coilKind = _coilKinds[plan.CoilKindName];

                string commitName = null;
                string factoryName = GetFactoryName(coilKind.ProductName);

                switch (factoryName)
                {
                    case "FACTORY1":
                        commitName = "COMMIT1";
                        break;
                    case "FACTORY2":
                        commitName = "COMMIT2";
                        break;
                }
                if (_equipments.ContainsKey(commitName))
                {
                    var commit = (AoCommit)_equipments[commitName];
                    commit.AddPlan(plan.CoilKindName, plan);
                }
            }
        }
        public void AddWorker(AoWorker worker)
        {
            if (_workers.ContainsKey(worker.Name) == false)
            {
                _workers.Add(worker.Name, worker);
                var workerModel = _modelManager.AddFactoryModel("Worker", worker.Name);
                _mes.AddWorker((Worker)workerModel);
                workerModel.SetAbstractObj(worker);
            }
        }

        public void AddSetUp(Setup setUp)
        {
            _mes.AddSetUp(setUp);
        }
        #endregion Adding Methods

        public Setup GetSetUpInformation(string productName)
        {
            return _mes.GetSetUpInformation(productName);
        }

        public string GetFactoryName(string productName)
        {
            if (productName.Contains("Coil"))
                productName = _coilKinds[productName].ProductName;

            string toolGroup = _productToolgroup[productName];
            string factoryName = _toolGroupFactory[toolGroup];

            return factoryName;
        }
        public string GetToolGroupName(string productName)
        {
            return _productToolgroup[productName];
        }

        public void SetProductToolGroup(string productName, string toolGroupName)
        {
            if (toolGroupName == "COMMIT" || toolGroupName == "COMPLETE")
                return;

            if (_productToolgroup.ContainsKey(productName) == false)
                _productToolgroup.Add(productName, toolGroupName);
        }

        public void SetToolGroupFactory(string toolGroupName, string factoryName)
        {
            if (toolGroupName == "COMMIT" || toolGroupName == "COMPLETE")
                return;

            if (_toolGroupFactory.ContainsKey(toolGroupName) == false)
                _toolGroupFactory.Add(toolGroupName, factoryName);
        }
    }
}
