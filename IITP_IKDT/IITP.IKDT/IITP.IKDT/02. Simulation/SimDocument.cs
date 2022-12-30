using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Pinokio.Map;
using Pinokio.Map.Algorithms;
using Pinokio.Simulation;
using Pinokio.Simulation.Disc;
using Pinokio.Simulation.FACTORY;
using Pinokio._3D.Eyeshot;
using Pinokio.Simulation.Models.IK;
using Pinokio._3D;
using System.IO;
using Pinokio.Core;
using System.Windows.Forms;

namespace Pinokio.IKDT
{
    public class SimDocument
    {
        private NModelManager _modelManager;
        private DiscSimEngine _engine;
        private Dictionary<string, Factory> _factories;
        private DataTable _initData;
        private List<FactoryDataLoader> _dataLoader;

        public DiscSimEngine Engine { get => _engine; }
        public NModelManager ModelManager { get => _modelManager; }
        public Dictionary<string, Factory> Factories { get => _factories; }

        public SimDocument()
        {
            _factories = new Dictionary<string, Factory>();
            _modelManager = new NModelManager();
            _engine = new DiscSimEngine();
            _dataLoader = new List<FactoryDataLoader>();
        }

        public void GenerateFactoryModel(string factoryName)
        {
            var factory = new Factory(factoryName, _modelManager);
            _factories.Add(factoryName, factory);
        }

        public void SetDataLoader(string path)
        {
            foreach (var factory in _factories.Values)
            {
                var loader = new FactoryDataLoader(factory, path);
                _dataLoader.Add(loader);
            }
        }

        public void LoadFactoryData(FactoryDataLoader.OnLoadDataEventHandler onLoadedAction = null)
        {
            foreach (var loader in _dataLoader)
            {
                if (onLoadedAction != null)
                { loader.OnLoadData += new FactoryDataLoader.OnLoadDataEventHandler(onLoadedAction); }
                loader.LoadStart();
                //_pathFinder.CaculateSubGraphPath(fab.Id);
            }
        }

        public void LoadFactoryData(DataTable data, FactoryDataLoader.OnLoadDataEventHandler onLoadedAction = null)
        {
            foreach (var loader in _dataLoader)
            {
                if (onLoadedAction != null)
                { loader.OnLoadData += new FactoryDataLoader.OnLoadDataEventHandler(onLoadedAction); }
                loader.LoadStartByRL(data);
            }
        }

        public List<EyeshotDrawSetting> GetSimulationDrawSettings()
        {
            var drawSettings = new List<EyeshotDrawSetting>();
            foreach (Factory factory in _factories.Values)
            {
                foreach (AoPort port in factory.PortTypes.Values)
                {
                    var drawSetting = new EyeshotDrawSetting(true, port.Name)
                    {
                        Size = port.Size,
                        MainColor = ColorDefinition.Port_Idle
                    };
                    drawSettings.Add(drawSetting);
                }
            }
            return drawSettings;
        }

        public void Run()
        {
            _engine.InitializeModels(_modelManager.Models.Values.ToList());
            _engine.Run();
        }
    }
}
