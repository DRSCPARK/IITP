using System;
using System.Linq;
using ClosedXML.Excel;
using Pinokio.Map;
using Pinokio.Core;
using Pinokio.Geometry;
using Pinokio.Simulation.Models.IK;
using System.Data;

namespace Pinokio.Simulation.FACTORY
{
    public class FactoryDataLoader : MapLoader
    {
        private IXLWorkbook _workbook;
        public delegate void OnLoadDataEventHandler(string dataName);
        public event OnLoadDataEventHandler OnLoadData = null;

        public FactoryDataLoader(Factory factory, string path) : base(factory)
        {
            //var curDirectory = Directory.GetCurrentDirectory();
            //var binDirectory = Directory.GetParent(curDirectory).FullName;
            //var ikModelDirectory = Directory.GetParent(binDirectory).FullName;
            //_workbook = new XLWorkbook(ikModelDirectory + @"\04. DB\IKTechDB.xlsx");

            _workbook = new XLWorkbook(path);

        }

        protected override void LoadByRL(System.Data.DataTable data)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            // Data Parsing
            if (!LoadToolGroupData()) throw new Exception("Fail To Load   ToolGroup   Information");

            switch (data.TableName)
            {
                case "LAYOUT":
                    if (!LoadEquipmentByRL(data)) throw new Exception("Fail To Load   Equipment   Information");
                    if (!LoadEqpPortData()) throw new Exception("Fail To Load   Port        Information");
                    if (!LoadProductData()) throw new Exception("Fail To Load   Product     Information");
                    if (!LoadSetupData()) throw new Exception("Fail To Load   Setup       Information");
                    if (!LoadCoilData()) throw new Exception("Fail To Load   Coil        Information");
                    if (!LoadWorkerData()) throw new Exception("Fail To Load   Worker      Information");
                    if (!LoadPlanData()) throw new Exception("Fail To Load   Plan        Information");
                    break;

                case "MPT":
                    if (!LoadEquipmentData()) throw new Exception("Fail To Load   Equipment   Information");
                    if (!LoadEqpPortData()) throw new Exception("Fail To Load   Port        Information");
                    if (!LoadProductData()) throw new Exception("Fail To Load   Product     Information");
                    if (!LoadSetupData()) throw new Exception("Fail To Load   Setup       Information");
                    if (!LoadCoilData()) throw new Exception("Fail To Load   Coil        Information");
                    if (!LoadWorkerByRL(data)) throw new Exception("Fail To Load   Worker      Information");
                    if (!LoadPlanData()) throw new Exception("Fail To Load   Plan        Information");
                    break;
            }
            sw.Stop();
            LogHandler.AddLog(LogLevel.Info, $"     Load Finish: Generating Simulaiton Models({sw.ElapsedMilliseconds})");
        }

        protected override void Load()
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            if (!LoadToolGroupData()) throw new Exception("Fail To Load   ToolGroup   Information");
            if (!LoadEquipmentData()) throw new Exception("Fail To Load   Equipment   Information");
            if (!LoadEqpPortData()) throw new Exception("Fail To Load   Port        Information");
            if (!LoadProductData()) throw new Exception("Fail To Load   Product     Information");
            if (!LoadSetupData()) throw new Exception("Fail To Load   Setup       Information");
            if (!LoadCoilData()) throw new Exception("Fail To Load   Coil        Information");
            if (!LoadWorkerData()) throw new Exception("Fail To Load   Worker      Information");
            if (!LoadPlanData()) throw new Exception("Fail To Load   Plan        Information");

            sw.Stop();
            LogHandler.AddLog(LogLevel.Info, $"     Load Finish: Generating Simulaiton Models({sw.ElapsedMilliseconds})");
        }

        protected bool LoadEquipmentData()
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Equipment"); }
            try
            {
                var ws = _workbook.Worksheet("Equipment");
                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string eqpName = row.Cell(1).Value.ToString();
                    string toolGroup = row.Cell(2).Value.ToString();
                    string factoryName = row.Cell(3).Value.ToString();

                    double loadingTime = Convert.ToDouble(row.Cell(4).Value);
                    double unLoadingTime = Convert.ToDouble(row.Cell(5).Value);

                    double width = Convert.ToDouble(row.Cell(6).Value);
                    double depth = Convert.ToDouble(row.Cell(7).Value);
                    double height = Convert.ToDouble(row.Cell(8).Value);

                    double x = Convert.ToDouble(row.Cell(9).Value);
                    double y = Convert.ToDouble(row.Cell(10).Value);
                    double z = Convert.ToDouble(row.Cell(11).Value);

                    double tpFromCommit = Convert.ToDouble(row.Cell(12).Value);
                    double tpToComplete = Convert.ToDouble(row.Cell(13).Value);

                    factory.SetToolGroupFactory(toolGroup, factoryName);

                    if (factory.Name != factoryName)
                        continue;

                    var resource = Map.GenerateResource(eqpName, new Vector3(x, y, z), toolGroup);

                    if (resource != null && resource is AoFactoryEquipment factoryEQP)
                    {
                        factoryEQP.SetToolGroupName(toolGroup);
                        factoryEQP.SetSize(new Vector3(width, depth, height));
                        factoryEQP.SetLoadingTime(new Distribution(DistributionType.Const) { Mean = loadingTime * 60 });
                        factoryEQP.SetUnloadingTime(new Distribution(DistributionType.Const) { Mean = unLoadingTime * 60 });
                        factoryEQP.SetTPFromCommit(new Distribution(DistributionType.Const) { Mean = tpFromCommit * 60 });
                        factoryEQP.SetTPToComplete(new Distribution(DistributionType.Const) { Mean = tpToComplete * 60 });
                    }

                    //ToolGroupHelper.ToolGroupId.Add(eqpName, toolGroup);

                    Map.AddResource(resource);
                }
                return true;
            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.ToString());
                return false;
            }
        }
        protected bool LoadEqpPortData()
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Port"); }
            try
            {
                var ws = _workbook.Worksheet("Port");
                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string portName = row.Cell(1).Value.ToString();
                    string factoryName = row.Cell(2).Value.ToString();
                    string portType = row.Cell(3).Value.ToString();
                    string eqpName = portName.Split('_').First();

                    if (factory.Name != factoryName)
                        continue;

                    AoPort port = factory.AddPortType(portName, portType);
                    factory.AddPort(portName, port, eqpName, portType);
                }

                return true;
            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }
        protected bool LoadEquipmentByRL(System.Data.DataTable data)
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Equipment"); }

            try
            {
                var ws = _workbook.Worksheet("Equipment");

                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string eqpName = row.Cell(1).Value.ToString();
                    string toolGroup = row.Cell(2).Value.ToString();
                    string factoryName = row.Cell(3).Value.ToString();
                    double loadingTime;
                    double unLoadingTime;
                    double width;
                    double depth;
                    double height;
                    double x;
                    double y;
                    double z;
                    double tpFromCommit;
                    double tpToComplete;

                    if (eqpName.Contains("COMMIT") || eqpName.Contains("COMPLETE"))
                    {
                        loadingTime = Convert.ToDouble(row.Cell(4).Value);
                        unLoadingTime = Convert.ToDouble(row.Cell(5).Value);

                        width = Convert.ToDouble(row.Cell(6).Value);
                        depth = Convert.ToDouble(row.Cell(7).Value);
                        height = Convert.ToDouble(row.Cell(8).Value);

                        x = Convert.ToDouble(row.Cell(9).Value);
                        y = Convert.ToDouble(row.Cell(10).Value);
                        z = Convert.ToDouble(row.Cell(11).Value);

                        tpFromCommit = Convert.ToDouble(row.Cell(12).Value);
                        tpToComplete = Convert.ToDouble(row.Cell(13).Value);
                    }
                    else
                    {
                        var targetRLRow = data.Select("Name = '" + eqpName + "'").First();

                        loadingTime = Convert.ToDouble(row.Cell(4).Value);
                        unLoadingTime = Convert.ToDouble(row.Cell(5).Value);

                        width = Convert.ToDouble(targetRLRow["Width"]);
                        depth = Convert.ToDouble(targetRLRow["Depth"]);
                        height = Convert.ToDouble(row.Cell(8).Value);

                        x = Convert.ToDouble(targetRLRow["PositionX"]);
                        y = Convert.ToDouble(targetRLRow["PositionY"]);
                        z = Convert.ToDouble(row.Cell(11).Value);

                        tpFromCommit = Convert.ToDouble(targetRLRow["TpToResource"]);
                        tpToComplete = Convert.ToDouble(targetRLRow["TpFromResource"]);
                    }

                    factory.SetToolGroupFactory(toolGroup, factoryName);

                    if (factory.Name != factoryName)
                        continue;

                    var resource = Map.GenerateResource(eqpName, new Vector3(x, y, z), toolGroup);

                    if (resource != null && resource is AoFactoryEquipment factoryEQP)
                    {
                        factoryEQP.SetToolGroupName(toolGroup);
                        factoryEQP.SetSize(new Vector3(width, depth, height));
                        factoryEQP.SetLoadingTime(new Distribution(DistributionType.Const) { Mean = loadingTime * 60 });
                        factoryEQP.SetUnloadingTime(new Distribution(DistributionType.Const) { Mean = unLoadingTime * 60 });
                        factoryEQP.SetTPFromCommit(new Distribution(DistributionType.Const) { Mean = tpFromCommit * 60 });
                        factoryEQP.SetTPToComplete(new Distribution(DistributionType.Const) { Mean = tpToComplete * 60 });
                    }

                    Map.AddResource(resource);
                }

                return true;
            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }
        protected bool LoadProductData()
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Product"); }
            try
            {
                var ws = _workbook.Worksheet("Product");
                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string productName = row.Cell(1).Value.ToString();
                    string toolGroup = row.Cell(2).Value.ToString();

                    var product = factory.GenerateProduct(productName, toolGroup);

                    factory.AddProduct(product);
                    factory.SetProductToolGroup(productName, toolGroup);
                }
                return true;
            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }
        protected bool LoadSetupData()
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Product"); }
            try
            {
                var ws = _workbook.Worksheet("Product");
                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string productName = row.Cell(1).Value.ToString();
                    string setUpName = row.Cell(3).Value.ToString();
                    var setUpTime = row.Cell(4).Value;

                    if (setUpTime.ToString() == "")
                        continue;

                    var offset = 0;
                    var setUp = new Setup();
                    setUp.SetSetupData(setUpName, productName, Convert.ToDouble(setUpTime), "Const", offset);

                    factory.AddSetUp(setUp);
                }
                return true;
            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }
        protected bool LoadPlanData()
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Plan"); }
            try
            {
                var ws = _workbook.Worksheet("Plan");
                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string name = row.Cell(1).Value.ToString();
                    string coilKindName = row.Cell(2).Value.ToString();

                    var planFactory = factory.GetFactoryName(coilKindName);

                    if (planFactory != factory.Name)
                        continue;

                    uint standard = Convert.ToUInt16(row.Cell(3).Value);
                    string standardUnit = row.Cell(4).Value.ToString();
                    DateTime startDate = DateTime.FromOADate((double)row.Cell(5).Value);
                    DateTime dueDate = DateTime.FromOADate((double)row.Cell(6).Value);

                    var plan = factory.GeneratePlan(name, coilKindName, standard, standardUnit, startDate, dueDate, factory.Name);
                    factory.AddPlan(plan);
                }
                return true;
            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }
        protected bool LoadCoilData()
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Coil"); }
            try
            {
                var ws = _workbook.Worksheet("Coil");
                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string name = row.Cell(1).Value.ToString();
                    string productName = row.Cell(2).Value.ToString();
                    double throughtput = Convert.ToDouble(row.Cell(3).Value);

                    double processingTime = Convert.ToDouble(row.Cell(4).Value);
                    double ptOffSet = Convert.ToDouble(row.Cell(5).Value);
                    string ptDistribution = row.Cell(6).Value.ToString();

                    double packagingTime = Convert.ToDouble(row.Cell(7).Value);
                    double pgOffSet = Convert.ToDouble(row.Cell(8).Value);
                    string pgDistribution = row.Cell(9).Value.ToString();

                    var coilKind = factory.GenerateCoilKind(name, productName);
                    coilKind.SetMaxThroughtput(throughtput);
                    coilKind.SetProcessingTime(ptDistribution, processingTime, ptOffSet);
                    coilKind.SetPackagingTime(pgDistribution, packagingTime, pgOffSet);
                    coilKind.SetToolGroupName(factory.GetToolGroupName(productName));

                    Setup setup = factory.GetSetUpInformation(productName);
                    coilKind.SetSetupData(setup);
                    factory.AddCoilKind(coilKind);
                }

                return true;
            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }

        protected bool LoadWorkerByRL(System.Data.DataTable data)
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Worker"); }

            try
            {
                foreach (DataRow row in data.Rows)
                {
                    string unit = "Week";
                    string type = "";
                    string factoryName = "FACTORY1";
                    int workerCount = 0;
                    int workingTime = 0;

                    int WorkingTimeMin = Convert.ToInt32(row["WorkingTimeMin"]);
                    int ProcessWorkerCount = Convert.ToInt32(row["ProcessWorkerCount"]);
                    int PackagingWorkerCount = Convert.ToInt32(row["PackagingWorkerCount"]);

                    for (int index = 0; index < ProcessWorkerCount; index++)
                    {
                        string name = "WORKER";
                        workerCount++;
                        name += workerCount.ToString();
                        workingTime = WorkingTimeMin;
                        type = "Processing";
                        AoWorker worker = factory.GenerateWorker(name, factoryName, workingTime, unit, type);

                        factory.AddWorker(worker);
                    }
                    for (int index = 0; index < PackagingWorkerCount; index++)
                    {
                        string name = "WORKER";
                        workerCount++;
                        name += workerCount.ToString();
                        workingTime = WorkingTimeMin;
                        type = "Packaging";
                        AoWorker worker = factory.GenerateWorker(name, factoryName, workingTime, unit, type);

                        factory.AddWorker(worker);
                    }
                    break;
                }
                return true;
            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }

        protected bool LoadWorkerData()
        {
            var factory = (Factory)this.Map;
            if (OnLoadData != null) { OnLoadData("Worker"); }
            try
            {
                var ws = _workbook.Worksheet("Worker");
                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string name = row.Cell(1).Value.ToString();
                    string factoryName = row.Cell(2).Value.ToString();
                    double workingTime = Convert.ToDouble(row.Cell(3).Value);
                    string unit = row.Cell(4).Value.ToString();
                    string type = row.Cell(5).Value.ToString();

                    if (factory.Name != factoryName)
                        continue;

                    AoWorker worker = factory.GenerateWorker(name, factoryName, workingTime, unit, type);

                    factory.AddWorker(worker);
                }
                return true;

            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }

        protected bool LoadToolGroupData()
        {
            var factory = (Factory)this.Map;

            if (OnLoadData != null) { OnLoadData("ToolGroup"); }
            try
            {
                var ws = _workbook.Worksheet("ToolGroup");
                for (int i = 2; i <= ws.RowsUsed().Count(); i++)
                {
                    var row = ws.Row(i);
                    string toolGroupName = row.Cell(1).Value.ToString();
                    string toolType = row.Cell(2).Value.ToString();

                    var toolGroup = factory.GenerateToolGroup(toolGroupName, toolType);

                    factory.AddToolGroup(toolGroup);
                }
                return true;

            }
            catch (Exception e)
            {
                LogHandler.AddLog(LogLevel.Error, e.Message.ToString());
                return false;
            }
        }
    }
}
