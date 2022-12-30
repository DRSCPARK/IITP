using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public static class CSVHelper
    {
        private static string _resultPath = string.Empty;
        private static string _mainDBPath = string.Empty;
        public static string ResultPath { get => _resultPath; }
        public static string MainDBPath { get => _mainDBPath; }

        public static void Init(string path)
        {
            //var curDirectory = Directory.GetCurrentDirectory();
            //var binDirectory = Directory.GetParent(curDirectory).FullName;
            //var ikModelDirectory = Directory.GetParent(binDirectory).FullName;
            //_resultPath = ikModelDirectory + @"\06. EventLog\EventLogFIle.csv";

            _resultPath = path;
            RemoveCSV();
            AddEventLog("TNOW", "FROM_MODEL", "TO_MODEL", "WORKER", "ENTITY", "PRODUCT", "RELEASE", "DUEDATE", "TYPE");
        }

        public static void RemoveCSV()
        {
            File.Delete(_resultPath);
        }
        public static void SetMainDBPath(string path)
        {
            _mainDBPath = path;
        }

        public static void SaveCurConsoleLine()
        {
            using (var stream = File.AppendText(_resultPath))
            {
                string csvRow = string.Format("{0}", int.Parse(Console.ReadLine()).ToString());
                stream.WriteLine(csvRow);
            }
        }

        public static void AddEventLog(string tnow, string fromModel, string toModel, string worker, string entity, string product, string releaseTime, string dueDate, string eventType)
        {
            using (var sw = new StreamWriter(_resultPath, true, Encoding.UTF8))
            {
                var row = $"{tnow}," +
                    $"{fromModel}," +
                    $"{toModel}," +
                    $"{worker}," +
                    $"{entity}," +
                    $"{product}," +
                    $"{releaseTime}," +
                    $"{dueDate}," +
                    $"{eventType}";

                sw.WriteLine(row);
                sw.Close();
            }
        }
    }
}
