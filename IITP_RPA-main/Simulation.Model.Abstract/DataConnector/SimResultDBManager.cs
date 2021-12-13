using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ADOX;
using System.Data.OleDb;
using DAO = Microsoft.Office.Interop.Access.Dao;
using Simulation.Model.Abstract;
using System.IO;
using Simulation.Geometry;
using Simulation.Engine;
using System.Diagnostics;
using System.Windows.Forms;

namespace Simulation.Model.Abstract
{
    public class SimResultDBManager
    {
        private static SimResultDBManager _instance;
        private Catalog _cat;
        private OleDbConnection _conn;
        private string _DBPath;
        private string _connString;
        private string _connStrDetail;

        public static SimResultDBManager Instance { get { return _instance; } }
        public string DBPath
        {
            get { return _DBPath; }
            set { _DBPath = value; }
        }
        public string connString { get { return _connString; } }
        public string connStrDetail { get { return _connStrDetail; } }

        public DateTime SimulationStartTime { get; set; }
        public DateTime SimulationEndTime { get; set; }

        public int FoupOHTCount { get; set; }
        public int ReticleOHTCount { get; set; }

        public SimResultDBManager()
        {
            _instance = this;
            _DBPath = string.Empty;
            _connString = string.Empty;
            _connStrDetail = string.Empty;
        }

        private void CreateDB()
        {
            if (!File.Exists(_DBPath))
            {
                _cat = new Catalog();
                string createStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _DBPath;
                _cat.Create(createStr);
            }
        }

        public bool ConnectDB()
        {
            string cntPath = System.IO.Directory.GetCurrentDirectory();

            _connString = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" +
                _DBPath + "; Persist Security Info=False";

            try
            {
                if (_conn == null || _conn.State != System.Data.ConnectionState.Open || _conn.ConnectionString != _connString)
                {
                    _conn = new OleDbConnection(_connString);
                    _conn.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public void DisconnectDB()
        {
            if (_conn != null && _conn.State == System.Data.ConnectionState.Open)
                _conn.Close();
        }

        public void Insert(TABLE_TYPE table, List<object> values)
        {
            try
            {
                if (ConnectDB())
                {
                    string query = string.Empty;
                    query = "INSERT INTO " + table.ToString()
                        + " VALUES (" + GetValues(values) + ")";

                    OleDbCommand cmd = new OleDbCommand(query, _conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _conn.Close();
            }
        }

        private string GetValues(List<object> values)
        {
            string rest = ", ";
            string valuesString = string.Empty;

            for (int i = 0; i < values.Count; i++)
            {
                if (i == 0)
                {
                    valuesString = GetValueStringWithQuotation(values[i]);
                }
                else if (i > 0 && i < values.Count - 1)
                {
                    valuesString = valuesString + rest + GetValueStringWithQuotation(values[i]);
                }
                else
                {
                    valuesString = valuesString + rest + GetValueStringWithQuotation(values[i]);
                }
            }

            return valuesString;
        }

        private string GetValueStringWithQuotation(object value)
        {
            string quotation = "'";

            if (value is bool)
                return value.ToString().ToUpper();
            else
                return quotation + value.ToString() + quotation;
        }

        public void Update(Enum table, Enum criColumn, string criterion, Enum column, object value)
        {
            try
            {
                string query = string.Empty;
                query = "UPDATE " + table.ToString()
                    + " SET " + column.ToString() + " = " + value
                    + " WHERE " + criColumn.ToString() + " = " + "'" + criterion + "'";

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _conn.Close();
            }
        }

        public void CreateSimResults()
        {
            _DBPath = System.IO.Directory.GetCurrentDirectory() + "/SimulationResult_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second +  ".accdb";
            CreateDB();
            CreateTables(_cat);
            UploadResultMainTable();
        }

        public void LoadSimResults(string dbPath)
        {
            _DBPath = dbPath;
            ConnectDB();
            DownloadSimulationResults();
        }

        #region Create Simulation Results

        private void DownloadSimulationResults()
        {
            SelectResultMain();
//            CreateCompletedCommandTrendLog();
//            CreateOhtOperationRate();
        }

        public void SelectResultMain()
        {
            if(ConnectDB())
            {
                string query = string.Empty;
                query = "SELECT * FROM " + TABLE_TYPE.RESULT_MAIN.ToString();

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                while(reader.Read() == true)
                {
                    if (reader[RESULT_MAIN_TABLE_COLUMN.NAME.ToString()].ToString() == RESULT_MAIN_TABLE_ROW.SIMULATION_START_TIME.ToString())
                        SimulationStartTime = Convert.ToDateTime(reader[RESULT_MAIN_TABLE_COLUMN.VALUE.ToString()]);
                    if (reader[RESULT_MAIN_TABLE_COLUMN.NAME.ToString()].ToString() == RESULT_MAIN_TABLE_ROW.SIMULATION_END_TIME.ToString())
                        SimulationEndTime = Convert.ToDateTime(reader[RESULT_MAIN_TABLE_COLUMN.VALUE.ToString()]);
                    if (reader[RESULT_MAIN_TABLE_COLUMN.NAME.ToString()].ToString() == RESULT_MAIN_TABLE_ROW.FOUP_OHT_COUNT.ToString())
                        FoupOHTCount = Convert.ToInt32(reader[RESULT_MAIN_TABLE_COLUMN.VALUE.ToString()]);
                    if (reader[RESULT_MAIN_TABLE_COLUMN.NAME.ToString()].ToString() == RESULT_MAIN_TABLE_ROW.RETICLE_OHT_COUNT.ToString())
                        ReticleOHTCount = Convert.ToInt32(reader[RESULT_MAIN_TABLE_COLUMN.VALUE.ToString()]);

                }
            }
        }


        private void CreateCompletedCommandTrendLog()
        {

        }

        private void CreateOhtOperationRate()
        {

        }

        public List<Command> SelectCommandLog(DateTime fromTime, DateTime toTime, string selectedFabName)
        {
            List<Command> commands = new List<Command>();
            try
            {
                if (ConnectDB())
                {
                    string name;
                    string fabName;
                    DateTime activatedTime;
                    DateTime completedTime;
                    string fromStation;
                    string toStation;
                    string foupName;
                    int priority;
                    DateTime realTime;
                    bool reticle;
                    DateTime assignedTime;
                    DateTime loadedTime;
                    double transferingDistance;
                    string routeString;
                    string firstRouteString;
                    int reroutingCount;
                    string ohtName;

                    string query = string.Empty;
                    query = GetCommandLogQuery(fromTime, toTime, selectedFabName);

                    OleDbCommand cmd = new OleDbCommand(query, _conn);
                    cmd.ExecuteNonQuery();

                    OleDbDataReader reader = cmd.ExecuteReader();
                    while (reader.Read() == true)
                    {
                        name = reader[COMMAND_LOG_TABLE_COLUMN.NAME.ToString()].ToString();
                        fabName = reader[COMMAND_LOG_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                        activatedTime = Convert.ToDateTime(reader[COMMAND_LOG_TABLE_COLUMN.ACTIVATED_TIME.ToString()].ToString());
                        completedTime = Convert.ToDateTime(reader[COMMAND_LOG_TABLE_COLUMN.COMPLETED_TIME.ToString()].ToString());
                        fromStation = reader[COMMAND_LOG_TABLE_COLUMN.FROM_STATION.ToString()].ToString();
                        toStation = reader[COMMAND_LOG_TABLE_COLUMN.TO_STATION.ToString()].ToString();
                        foupName = reader[COMMAND_LOG_TABLE_COLUMN.FOUP_NAME.ToString()].ToString();
                        priority = Convert.ToInt32(reader[COMMAND_LOG_TABLE_COLUMN.PRIORITY.ToString()].ToString());
                        realTime = Convert.ToDateTime(reader[COMMAND_LOG_TABLE_COLUMN.REAL_TIME.ToString()].ToString());
                        reticle = Convert.ToBoolean(reader[COMMAND_LOG_TABLE_COLUMN.RETICLE.ToString()].ToString());
                        assignedTime = Convert.ToDateTime(reader[COMMAND_LOG_TABLE_COLUMN.ASSIGNED_TIME.ToString()].ToString());
                        loadedTime = Convert.ToDateTime(reader[COMMAND_LOG_TABLE_COLUMN.LOADED_TIME.ToString()].ToString());
                        Command command = Scheduler.Instance.AddCommandForResult(name, fabName, activatedTime, fromStation, toStation, foupName, priority, realTime, reticle);
                        command.CompletedTime = completedTime;
                        command.AssignedTime = assignedTime;
                        command.LoadedTime = loadedTime;
                        command.TransferingDistance = Convert.ToDouble(reader[COMMAND_LOG_TABLE_COLUMN.TRANSFERING_DISTANCE.ToString()].ToString());
                        routeString = reader[COMMAND_LOG_TABLE_COLUMN.ROUTE_ID_LIST.ToString()].ToString();
                        string[] splittedString = routeString.Split(ModelManager.Instance.Separator);

                        foreach(string lineID in splittedString)
                        {
                            if (lineID == "")
                                continue;

                            RailLineNode line = ModelManager.Instance.GetSimNodebyID(Convert.ToUInt32(lineID)) as RailLineNode;
                            command.Route.Add(line);
                        }

                        firstRouteString = reader[COMMAND_LOG_TABLE_COLUMN.FIRST_ROUTE_ID_LIST.ToString()].ToString();
                        splittedString = firstRouteString.Split(ModelManager.Instance.Separator);

                        foreach (string lineID in splittedString)
                        {
                            if (lineID == "")
                                continue;

                            RailLineNode line = ModelManager.Instance.GetSimNodebyID(Convert.ToUInt32(lineID)) as RailLineNode;
                            command.FirstRoute.Add(line);
                        }

                        command.ReroutingCount = Convert.ToInt32(reader[COMMAND_LOG_TABLE_COLUMN.REROUTING_COUNT.ToString()].ToString());
                        ohtName = reader[COMMAND_LOG_TABLE_COLUMN.OHT_NAME.ToString()].ToString();

                        OHTNode oht = ModelManager.Instance.GetOHT(ohtName);
                        command.OHTNode = oht;
                        command.CommandState = COMMAND_STATE.COMPLETED;
                        commands.Add(command);
                    }
                }

                return commands;
            }
            catch(Exception e)
            {
                MessageBox.Show("검색 조건이 잘못되었습니다.");
            }

            return commands;
        }

        public string GetCommandLogQuery(DateTime fromTime, DateTime toTime, string fabName)
        {
            string query = "SELECT * FROM " + TABLE_TYPE.COMMAND_LOG.ToString() + " WHERE " + COMMAND_LOG_TABLE_COLUMN.ACTIVATED_TIME + " >= #" + fromTime.ToString("yyyy/MM/dd HH:mm:ss")
    + "# AND " + COMMAND_LOG_TABLE_COLUMN.ACTIVATED_TIME + " < #" + toTime.ToString("yyyy/MM/dd HH:mm:ss") + "#";

            if (fabName != string.Empty)
                query = query + " AND " + COMMAND_LOG_TABLE_COLUMN.FAB_NAME + " = " + $"\"{fabName}\"";

            return query;
        }
        public List<EqpHistory> SelectEqpLog(DateTime fromTime, DateTime toTime, string eqpId)
        {
            List<EqpHistory> eqpHistorys = new List<EqpHistory>();

            if (ConnectDB())
            {
                string foupId;
                string processType;
                string productId;
                string stepId;
                DateTime startTime;
                DateTime endTime;
                DateTime simStartTime;
                DateTime simEndTime;
                string stepType;
                string stepGroup;

                string query = string.Empty;
                query = "SELECT * FROM " + TABLE_TYPE.EQP_LOG.ToString() + " WHERE " + EQP_LOG_TABLE_COLUMN.SIM_END_TIME + " >= #" + fromTime.ToString("yyyy/MM/dd HH:mm:ss")
                    + "# AND " + EQP_LOG_TABLE_COLUMN.SIM_END_TIME + " < #" + toTime.ToString("yyyy/MM/dd HH:mm:ss") + "# AND " + EQP_LOG_TABLE_COLUMN.EQP_ID + " = " + $"\"{eqpId}\"";

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    foupId = reader[EQP_LOG_TABLE_COLUMN.FOUP_ID.ToString()].ToString();
                    processType = reader[EQP_LOG_TABLE_COLUMN.PROCESS_TYPE.ToString()].ToString();
                    productId = reader[EQP_LOG_TABLE_COLUMN.PRODUCT_ID.ToString()].ToString();
                    stepId = reader[EQP_LOG_TABLE_COLUMN.STEP_ID.ToString()].ToString();
                    startTime = Convert.ToDateTime(reader[EQP_LOG_TABLE_COLUMN.START_TIME.ToString()].ToString());
                    endTime = Convert.ToDateTime(reader[EQP_LOG_TABLE_COLUMN.END_TIME.ToString()].ToString());
                    simStartTime = Convert.ToDateTime(reader[EQP_LOG_TABLE_COLUMN.SIM_START_TIME.ToString()].ToString());
                    simEndTime = Convert.ToDateTime(reader[EQP_LOG_TABLE_COLUMN.SIM_END_TIME.ToString()].ToString());
                    stepType = reader[EQP_LOG_TABLE_COLUMN.STEP_TYPE.ToString()].ToString();
                    stepGroup = reader[EQP_LOG_TABLE_COLUMN.STEP_GROUP.ToString()].ToString();

                    EqpHistory eqpHistory = new EqpHistory();

                    eqpHistory.EqpID = eqpId;
                    eqpHistory.FoupID = foupId;
                    eqpHistory.ProductID = productId;
                    eqpHistory.StepID = stepId;
                    eqpHistory.StartTime = startTime;
                    eqpHistory.EndTime = endTime;
                    eqpHistory.SimStartTime = simStartTime;
                    eqpHistory.SimEndTime = simEndTime;
                    eqpHistory.StepType = stepType;
                    eqpHistory.StepGroup = stepGroup;

                    eqpHistorys.Add(eqpHistory);
                }
            }

            return eqpHistorys;
        }

        public List<EqpHistory> SelectAllEqpLog(DateTime fromTime, DateTime toTime)
        {
            List<EqpHistory> eqpHistorys = new List<EqpHistory>();

            if (ConnectDB())
            {
                string eqpId;
                string foupId;
                string processType;
                string productId;
                string stepId;
                DateTime startTime;
                DateTime endTime;
                DateTime simStartTime;
                DateTime simEndTime;
                string stepType;
                string stepGroup;

                string query = string.Empty;
                query = "SELECT * FROM " + TABLE_TYPE.EQP_LOG.ToString() + " WHERE " + EQP_LOG_TABLE_COLUMN.SIM_END_TIME + " >= #" + fromTime.ToString("yyyy/MM/dd HH:mm:ss")
                    + "# AND " + EQP_LOG_TABLE_COLUMN.SIM_END_TIME + " < #" + toTime.ToString("yyyy/MM/dd HH:mm:ss") + "#";

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    eqpId = reader[EQP_LOG_TABLE_COLUMN.EQP_ID.ToString()].ToString();
                    foupId = reader[EQP_LOG_TABLE_COLUMN.FOUP_ID.ToString()].ToString();
                    processType = reader[EQP_LOG_TABLE_COLUMN.PROCESS_TYPE.ToString()].ToString();
                    productId = reader[EQP_LOG_TABLE_COLUMN.PRODUCT_ID.ToString()].ToString();
                    stepId = reader[EQP_LOG_TABLE_COLUMN.STEP_ID.ToString()].ToString();
                    startTime = Convert.ToDateTime(reader[EQP_LOG_TABLE_COLUMN.START_TIME.ToString()].ToString());
                    endTime = Convert.ToDateTime(reader[EQP_LOG_TABLE_COLUMN.END_TIME.ToString()].ToString());
                    simStartTime = Convert.ToDateTime(reader[EQP_LOG_TABLE_COLUMN.SIM_START_TIME.ToString()].ToString());
                    simEndTime = Convert.ToDateTime(reader[EQP_LOG_TABLE_COLUMN.SIM_END_TIME.ToString()].ToString());
                    stepType = reader[EQP_LOG_TABLE_COLUMN.STEP_TYPE.ToString()].ToString();
                    stepGroup = reader[EQP_LOG_TABLE_COLUMN.STEP_GROUP.ToString()].ToString();

                    EqpHistory eqpHistory = new EqpHistory();

                    eqpHistory.EqpID = eqpId;
                    eqpHistory.FoupID = foupId;
                    eqpHistory.ProductID = productId;
                    eqpHistory.StepID = stepId;
                    eqpHistory.StartTime = startTime;
                    eqpHistory.EndTime = endTime;
                    eqpHistory.SimStartTime = simStartTime;
                    eqpHistory.SimEndTime = simEndTime;
                    eqpHistory.StepType = stepType;
                    eqpHistory.StepGroup = stepGroup;

                    eqpHistorys.Add(eqpHistory);
                }
            }

            return eqpHistorys;
        }

        #endregion

        #region Save Simulation Results
        #region Create Simulation Results Table

        private void CreateTables(Catalog cat)
        {
            CreateResultMainTable(cat);
            CreateCompletedCommandTrendLogTable(cat);
            CreateEqpLogTable(cat);
            CreateFoupLogTable(cat);
            //            CreateOhtOperatingRateTable(cat);

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(cat.Tables);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(cat.ActiveConnection);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(cat);
        }

        private void CreateResultMainTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.RESULT_MAIN.ToString();
            tbl.Columns.Append(RESULT_MAIN_TABLE_COLUMN.NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(RESULT_MAIN_TABLE_COLUMN.VALUE.ToString(), DataTypeEnum.adLongVarWChar);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateCompletedCommandTrendLogTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.COMMAND_LOG.ToString();
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.NAME.ToString(), DataTypeEnum.adVarWChar, 150);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.ACTIVATED_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.COMPLETED_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.FROM_STATION.ToString(), DataTypeEnum.adVarWChar, 80);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.FROM_STATION_TYPE.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.TO_STATION.ToString(), DataTypeEnum.adVarWChar, 80);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.TO_STATION_TYPE.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.FOUP_NAME.ToString(), DataTypeEnum.adVarWChar, 80);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.PRIORITY.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.RETICLE.ToString(), DataTypeEnum.adBoolean);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.REAL_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.ASSIGNED_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.LOADED_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.TRANSFERING_DISTANCE.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.ROUTE_ID_LIST.ToString(), DataTypeEnum.adLongVarWChar);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.FIRST_ROUTE_ID_LIST.ToString(), DataTypeEnum.adLongVarWChar);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.REROUTING_COUNT.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(COMMAND_LOG_TABLE_COLUMN.OHT_NAME.ToString(), DataTypeEnum.adVarWChar, 30);

            ADOX.Index index = new ADOX.Index();
            index.PrimaryKey = false;
            index.Name = "PK1_COMMAND_LOG";
            index.Columns.Append(COMMAND_LOG_TABLE_COLUMN.ACTIVATED_TIME.ToString(), tbl.Columns[COMMAND_LOG_TABLE_COLUMN.ACTIVATED_TIME.ToString()].Type, tbl.Columns[COMMAND_LOG_TABLE_COLUMN.ACTIVATED_TIME.ToString()].DefinedSize);
            tbl.Indexes.Append(index);


            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateEqpLogTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.EQP_LOG.ToString();
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.EQP_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.FOUP_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.PROCESS_TYPE.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.PRODUCT_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.STEP_ID.ToString(), DataTypeEnum.adVarWChar, 80);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.START_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.END_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.SIM_START_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.SIM_END_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.STEP_TYPE.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(EQP_LOG_TABLE_COLUMN.STEP_GROUP.ToString(), DataTypeEnum.adVarWChar, 100);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateFoupLogTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.FOUP_LOG.ToString();
            tbl.Columns.Append(FOUP_LOG_TABLE_COLUMN.FOUP_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(FOUP_LOG_TABLE_COLUMN.PREV_RESOURCE_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(FOUP_LOG_TABLE_COLUMN.NEXT_RESOURCE_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(FOUP_LOG_TABLE_COLUMN.PREV_STATE.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(FOUP_LOG_TABLE_COLUMN.NEXT_STATE.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(FOUP_LOG_TABLE_COLUMN.SIM_DATE_TIME.ToString(), DataTypeEnum.adDate);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateOhtOperatingRateTable(Catalog cat)
        {

        }

        #endregion

        #region Upload Simulation Results

        private void UploadSimulationResults()
        {
            UploadResultMainTable();
//            UploadOhtOperationRateTable();
        }

        private void UploadResultMainTable()
        {
            List<object> values = new List<object>();
            values.Add(RESULT_MAIN_TABLE_ROW.SIMULATION_START_TIME.ToString());
            values.Add(SimEngine.Instance.StartDateTime);
            Insert(TABLE_TYPE.RESULT_MAIN, values);

            values.Clear();
            values.Add(RESULT_MAIN_TABLE_ROW.SIMULATION_END_TIME.ToString());
            values.Add(SimEngine.Instance.EndDateTime);
            Insert(TABLE_TYPE.RESULT_MAIN, values);
        }

        private void UploadOhtOperationRateTable()
        {

        }

        public void UploadCompletedCommandTrendLog(Command command)
        {
            if (_conn == null || _conn.State != System.Data.ConnectionState.Open)
                ConnectDB();

            List<object> values = new List<object>();
            values.Add(command.Name);
            values.Add(command.Fab.Name);
            values.Add(command.ActivatedDateTime);
            values.Add(command.CompletedTime);
            values.Add(command.FromNode.Name);
            values.Add(command.FROM_EQ_TYPE.ToString());
            values.Add(command.ToNode.Name);
            values.Add(command.TO_EQ_TYPE.ToString());
            if (command.Entity == null)
                values.Add(string.Empty);
            else
                values.Add(command.Entity.Name);
            values.Add(command.Priority);
            values.Add(command.Reticle);
            values.Add(command.RealCompletedTime);
            values.Add(command.AssignedTime);
            values.Add(command.LoadedTime);
            values.Add(command.TransferingDistance);
            values.Add(command.RouteIDString);
            values.Add(command.FirstRouteIDString);
            values.Add(command.ReroutingCount);
            values.Add(command.OHTName);
            Insert(TABLE_TYPE.COMMAND_LOG, values);
        }

        public void UploadEqpLog(EqpHistory eqpHistory)
        {
            if (_conn == null || _conn.State != System.Data.ConnectionState.Open)
                ConnectDB();

            List<object> values = new List<object>();

            ProcessEqpNode processEqp = ModelManager.Instance.DicProcessEqpNode[eqpHistory.EqpID];

            // ProcessType
            string processType = processEqp.ProcessType.ToString();
            // StepGroup
            string stepGroup = processEqp.StepGroup.ToString();

            values.Add(eqpHistory.EqpID);
            values.Add(eqpHistory.FoupID);
            values.Add(processType);
            values.Add(eqpHistory.ProductID);
            values.Add(eqpHistory.StepID);
            values.Add(eqpHistory.StartTime);
            values.Add(eqpHistory.EndTime);
            values.Add(eqpHistory.SimStartTime);
            values.Add(eqpHistory.SimEndTime);
            values.Add(eqpHistory.StepType);
            values.Add(stepGroup);

            Insert(TABLE_TYPE.EQP_LOG, values);
        }
        public void UploadFoupLog(string foupId, string prevResourceId, string nextResourceId, string prevState, string nextState, DateTime simDateTime)
        {
            if (_conn == null || _conn.State != System.Data.ConnectionState.Open)
                ConnectDB();

            List<object> values = new List<object>();

            values.Add(foupId);
            values.Add(prevResourceId);
            values.Add(nextResourceId);
            values.Add(prevState);
            values.Add(nextState);
            values.Add(simDateTime);

            Insert(TABLE_TYPE.FOUP_LOG, values);
        }

        #endregion
        #endregion
    }

}
