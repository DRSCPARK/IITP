using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Map.Native;
using DevExpress.XtraBars;
using DevExpress.XtraReports.Design;
using DevExpress.XtraSplashScreen;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Vml;
using Pinokio._3D;
using Pinokio._3D.Eyeshot;
using Pinokio.Core;
using Pinokio.Geometry;
using Pinokio.Map;
using Pinokio.Simulation;
using Pinokio.Simulation.Models.IK;
using Pinokio.UserControls;
using static DevExpress.Xpo.Logger.LogManager;
using TriangleNet.Logging;

namespace Pinokio.IKDT
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        static MainForm _mainForm;
        private EyeshotViewFrame _viewFrame;
        private IKShapeFactory _shapeFactory;
        private SimDocument _simDocument;
        private NModelManager _modelManager;
        private ZMQManager _communicationManager;
        private bool _isModelLoaded;

        static public MainForm Instance
        {
            get { return _mainForm; }
        }

        public SimDocument Simdocument
        {
            get { return _simDocument; }
        }

        public MainForm()
        {
            InitializeComponent();
            Initialize3DView();
            Initialize();
            InitializeCommunication();
        }
        public void Initialize()
        {
            _mainForm = this;
            _simDocument = new SimDocument();
            InitializeLogger();
            _isModelLoaded = false;

            OpenFileDialog openFileDlg = new OpenFileDialog()
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx|CSV Files|*.csv",
                Multiselect = false,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true
            };
            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                var path = openFileDlg.FileName;
                var parentPath = Directory.GetParent(path).FullName;
                var eventLogPath = Directory.GetParent(parentPath).FullName + "\\06. EventLog\\EventLogFIle.csv";

                CSVHelper.SetMainDBPath(path);
                CSVHelper.Init(eventLogPath);
                LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Simulation Initial Model Loaded!!");

                var modelFolder = Directory.GetParent(parentPath).FullName + "\\07. 3dModel";
                Initialize3DModel(modelFolder);
            }
            else
            {
                CSVHelper.Init("C:\\Users\\MNS\\source\\repos\\IKDT\\Pinokio.IKDT\\Pinokio.IKDT\\06. EventLog\\EventLogFIle.csv");
                var bin = Directory.GetParent(System.IO.Directory.GetCurrentDirectory());
                var parentPath = Directory.GetParent(bin.FullName);
                var modelFolder = parentPath.FullName + "\\07. 3dModel";
                Initialize3DModel(modelFolder);
            }
        }

        private void InitializeCommunication()
        {
            _communicationManager = new ZMQManager();
        }

        public void Initialize3DView()
        {
            _viewFrame = new EyeshotViewFrame();
            _viewFrame.MdiParent = this;
            _viewFrame.Show();
            _viewFrame.Dock = DockStyle.Fill;

            _shapeFactory = new IKShapeFactory(_viewFrame);
        }

        private void Update3DView()
        {
            _viewFrame = new EyeshotViewFrame();

            _viewFrame.Invoke(new MethodInvoker(delegate { _viewFrame.MdiParent = this; }));
            _viewFrame.Invoke(new MethodInvoker(delegate { _viewFrame.Show(); }));
            _viewFrame.Invoke(new MethodInvoker(delegate { _viewFrame.Dock = DockStyle.Fill; }));

            _shapeFactory = new IKShapeFactory(_viewFrame);
        }

        private void Initialize3DModel(string modelFolder)
        {
            EyeshotCADMart.AddObjModel("HANIL40T", modelFolder + @"\HANIL KNUCKLE PRESS 40 TON\HANIL KNUCKLE PRESS 40 TON.obj");
            EyeshotCADMart.AddObjModel("HANIL80T", modelFolder + @"\HANIL KNUCKLE PRESS 80 TON\HANIL KNUCKLE PRESS 80 TON.obj");
            EyeshotCADMart.AddObjModel("HANIL110T", modelFolder + @"\HANIL KNUCKLE PRESS 110 TON\HANIL KNUCKLE PRESS 110 TON.obj");
            EyeshotCADMart.AddObjModel("HANIL150T", modelFolder + @"\HANIL KNUCKLE PRESS 150 TON\HANIL KNUCKLE PRESS 150 TON.obj");

            EyeshotCADMart.AddObjModel("KYORI30T", modelFolder + @"\KYORI 30TON\KYORI 30TON.obj");
            EyeshotCADMart.AddObjModel("KYORI40T", modelFolder + @"\KYORI 40TON\KYORI 40TON.obj");
            EyeshotCADMart.AddObjModel("MITSUI60T", modelFolder + @"\MITSUI CHANSIN PRESS 60TON\MITSUI CHANSIN PRESS 60TON.obj");
        }

        private void InitializeLogger()
        {
            //Logger logger = new Logger();
            //logger.PrintInfoHandle += Console.WriteLine;

            LogHandler.LogInfoHandle += Console.WriteLine;
            LogHandler.LogErrorHandle += Console.WriteLine;
        }

        public void LoadModels(DataTable data)
        {
            var sw = new System.Diagnostics.Stopwatch();

            if (!_isModelLoaded)
            {
                _simDocument.GenerateFactoryModel("FACTORY1");
                _simDocument.SetDataLoader(CSVHelper.MainDBPath);

                sw.Start();

                LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Simulation Model Generating Start by Client!!");
                SplashScreenManager.ShowForm(this, typeof(ProgressIndicator), true, true);
                this.Invoke(new MethodInvoker(delegate { this.Enabled = false; }));

                var parentPath = Directory.GetParent(CSVHelper.MainDBPath).FullName;
                var eventLogPath = Directory.GetParent(parentPath).FullName + "\\06. EventLog\\EventLogFIle.csv";

                CSVHelper.Init(eventLogPath);

                _simDocument.LoadFactoryData(data, OnLoadedData_Invoked);
                sw.Stop();
                LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Simulation Model Generating Finish({sw.ElapsedMilliseconds})");

                SplashScreenManager.CloseForm(false, true);
                this.Invoke(new MethodInvoker(delegate { this.Enabled = true; }));
                _isModelLoaded = true;
            }
            else
            {
                _simDocument = new SimDocument();
                _simDocument.GenerateFactoryModel("FACTORY1");
                _simDocument.SetDataLoader(CSVHelper.MainDBPath);

                sw.Start();

                LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Simulation Model Generating Start by Client!!");
                SplashScreenManager.ShowForm(this, typeof(ProgressIndicator), true, true);

                _simDocument.LoadFactoryData(data, OnLoadedData_Invoked);
                sw.Stop();
                LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Simulation Model Generating Finish({sw.ElapsedMilliseconds})");

                SplashScreenManager.CloseForm(false, true);
                _isModelLoaded = true;
            }

            #region Init 3D Models
            sw.Reset();
            sw.Start();
            Load3dModels();
            LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Generating Shapes Finish({sw.ElapsedMilliseconds})");
            #endregion Init 3D Models
        }

        private void Load3dModels()
        {
            Update3DView();



            _shapeFactory.DefineDrawSettings(_simDocument.Factories["FACTORY1"]);
            _shapeFactory.AddModelShapes(_simDocument.ModelManager.Models);

            if (_viewFrame.InvokeRequired)
            {
                _viewFrame.Invoke(new MethodInvoker(delegate { _viewFrame.DrawAll(); }));
                _viewFrame.Invoke(new MethodInvoker(delegate { _viewFrame.ChangeViewMode(ViewMode.ThreeDim); }));
            }
            else
            {
                _viewFrame.DrawAll();
                _viewFrame.ChangeViewMode(ViewMode.ThreeDim);
            }
        }

        private void LoadModels()
        {
            if (!_isModelLoaded)
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Simulation Model Generating Start");

                SplashScreenManager.ShowForm(this, typeof(ProgressIndicator), true, true);
                this.Enabled = false;

                _simDocument.GenerateFactoryModel("FACTORY1");
                _simDocument.SetDataLoader(CSVHelper.MainDBPath);
                //_simDocument.GenerateFactoryModel("FACTORY2");
                _simDocument.LoadFactoryData(OnLoadedData_Invoked);

                sw.Stop();
                LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Simulation Model Generating Finish({sw.ElapsedMilliseconds})");

                SplashScreenManager.CloseForm(false, true);
                this.Enabled = true;
                _isModelLoaded = true;

                #region Init 3D Models
                sw.Reset();
                sw.Start();
                Load3dModels();
                LogHandler.AddLog(Pinokio.Core.LogLevel.Info, $"Generating Shapes Finish({sw.ElapsedMilliseconds})");
                #endregion Init 3D Models
            }
            else
            {
                MessageBox.Show("Already Loaded");
            }
        }

        public void RunSimulationAutomation()
        {
            if (_isModelLoaded)
            {
                SimParameter.SetStartDateTime(new DateTime(2022, 8, 1, 0, 0, 0, 0));

                SimParameter.SetEndOfSimulation(new SimTime(7948800)); // 3 Month

                SimParameter.SetWarmUp(false);

                _simDocument.Run();

                var newTimer = new SimTimer(new SimTime(50));
                newTimer.OnTimer += UpdateSimTime;
                _simDocument.Engine.AddTimer(newTimer);
            }
            else
            {
                MessageBox.Show("Load Model First");
            }
        }

        private SimTime _lastSimTime;
        private DateTime _lastTime;
        private double _maxSpeed = 0;

        private void UpdateSimTime()
        {
            SimTimeEditItem.EditValue = _simDocument.Engine.TimeNow.ToString();
            var simTimeDiff = _simDocument.Engine.TimeNow.TotalSeconds - _lastSimTime.TotalSeconds;
            var timeNow = DateTime.Now;
            TimeSpan realTimeDiff = (timeNow - _lastTime);
            SimSpeedEditItem.EditValue = (simTimeDiff / realTimeDiff.TotalSeconds).ToString();
            if (_maxSpeed < (simTimeDiff / realTimeDiff.TotalSeconds))
                _maxSpeed = (simTimeDiff / realTimeDiff.TotalSeconds);
            _lastSimTime = _simDocument.Engine.TimeNow;
            _lastTime = timeNow;
        }

        private void LoadBtn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_isModelLoaded)
            {
                MessageBox.Show("Simulation Model is Already Loaded!!");
                return;
            }

            LoadModels();
        }

        private void RunBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            RunSimulationAutomation();
        }

        public void OnLoadedData_Invoked(string dataName)
        {
            SplashScreenManager.Default.SendCommand(ProgressIndicator.WaitFormCommand.LoadData, dataName);
        }

        private void ServerOnOffBtn_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ServerOnOffBtn.Checked)
            {
                _communicationManager.StartServer();
                this.WindowState = FormWindowState.Normal;
                this.SendToBack();

            }
            else
            {
                _communicationManager.ServerClose();
                this.WindowState = FormWindowState.Maximized;
                this.BringToFront();
            }
        }
    }
}
