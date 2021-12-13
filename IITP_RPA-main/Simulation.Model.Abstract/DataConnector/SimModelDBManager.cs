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

namespace Simulation.Model.Abstract
{
    public class SimModelDBManager
    {
        private static SimModelDBManager _instance;
        private Catalog _cat;
        private OleDbConnection _conn;
        private string _DBPath;
        private string _pastDBPath;
        private string _connString;
        private string _connStrDetail;

        public static SimModelDBManager Instance { get { return _instance; } }
        public string DBPath { get { return _DBPath; } }
        public string connString { get { return _connString; } }
        public string connStrDetail { get { return _connStrDetail; } }

        public SimModelDBManager()
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
            if(_conn != null && _conn.State == System.Data.ConnectionState.Open)
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

        public void InsertIfCandidate(TABLE_TYPE table, List<object> values)
        {
            try
            {
                if(ConnectDB())
                {
                    string query = string.Empty;
                    query = "INSERT INTO " + table.ToString()
                        + " VALUES (" + GetValues(values) + ") FROM " + table.ToString()
                        + " SELECT * FROM " + TABLE_TYPE.CANDIDATE_PATH.ToString() + "WHERE NOT EXISTS (SELECT * FROM " + TABLE_TYPE.CANDIDATE_PATH.ToString()
                        + " WHERE " + CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString() + " = '" + values[0] + "'"
                        + " AND " + CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString() + " = " + values[1]
                        + " AND " + CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString() + " = "  +values[2]
                        + " AND " + CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString() + " = " + values[3]
                        + " AND " + CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString() + " = " + values[4]
                        + " AND " + CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString() + " = '"+  values[5] + "';);";

                    OleDbCommand cmd = new OleDbCommand(query, _conn);
                    cmd.ExecuteReader();
                }
            }
            catch(Exception ex)
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
                if (ConnectDB())
                {
                    string query = string.Empty;
                    query = "UPDATE " + table.ToString()
                        + " SET " + column.ToString() + " = " + value
                        + " WHERE " + criColumn.ToString() + " = " + "'" + criterion + "'";

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

        public bool DeleteAll(Enum table)
        {
            try
            {
                if(ConnectDB())
                {
                    string query = string.Empty;
                    query = "DELETE FROM " + table.ToString();

                    OleDbCommand cmd = new OleDbCommand(query, _conn);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _conn.Close();
                return false;
            }
        }

        public void SaveSimModel(string dbPath)
        {
            DisconnectDB();
            File.Delete(dbPath);

            _pastDBPath = _DBPath;
            _DBPath = dbPath;
            CreateDB();
            CreateTables(_cat);
            if(ConnectDB())
            {
                UploadSimulationModel();
            }
            DisconnectDB();
        }

        public void LoadSimModel(string dbPath)
        {
            _DBPath = dbPath;
            ConnectDB();
            DownloadSimModels();
        }


        #region Create Simulation Model
        public void DownloadSimModels()
        {
            Stopwatch totalStopwatch = new Stopwatch();
            totalStopwatch.Start();

            CreateSpecSimModels();
            CreateFabSimModels();
            CreateZcuSimModels();
            CreateBaySimModels();
            CreateAddressSimModels();
            CreateRailSimModels();
            CreateRailCutSimModels();
            ModelManager.Instance.InitializeZcu();
            CreateZoneNetworkSimModels();
            CreateEquipmentSimModels();
            CreateBufferSimModels();
            CreateStationSimModels();
            ModelManager.Instance.CreateNetworkLineStructure();
            CreateWipInitSimModels();
            CreateEqpHistorySimModels();
            CreateRouteSelectionSimModels();

            ModelManager.Instance.GetDataToFab();

            // ↓↓↓ Bay Network 파악 / BumpingRotation 순서 ↓↓↓
            foreach (Bay bay in ModelManager.Instance.Bays.Values.ToList())
            {
                ZoneHelper.Instance.SearchAlgorithm(bay);
                ZoneHelper.Instance.InitializeBumpingPort(bay);
            }

            ZoneHelper.Instance.IntializeBumpingRotation();
            // ↑↑↑ Bay Network 파악 / BumpingRotation 순서 ↑↑↑

            ModelManager.Instance.SetStopNResetLine();
            ModelManager.Instance.InitializeRailLineSpec();
            CreateVehicleSimModels();
            ModelManager.Instance.GetOHtDataOfFab();
            ModelManager.Instance.AddCommander(Scheduler.Instance.InactiveCommandList);
                totalStopwatch.Stop();
            Console.WriteLine("totalStopwatch Time: " + Convert.ToDouble(totalStopwatch.ElapsedMilliseconds).ToString() + "milliseconds");
        }

        public void CreateSpecSimModels()
        {
            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.SPEC.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                if (reader[SPEC_TABLE_COLUMN.NAME.ToString()].ToString() == SPEC_TABLE_ROW.SIMULATION_START_TIME.ToString())
                    SimEngine.Instance.StartDateTime = Convert.ToDateTime(reader[SPEC_TABLE_COLUMN.VALUE.ToString()].ToString());
                if (reader[SPEC_TABLE_COLUMN.NAME.ToString()].ToString() == SPEC_TABLE_ROW.SIMULATION_END_TIME.ToString())
                    SimEngine.Instance.EndDateTime = Convert.ToDateTime(reader[SPEC_TABLE_COLUMN.VALUE.ToString()].ToString());
                if (reader[SPEC_TABLE_COLUMN.NAME.ToString()].ToString() == SPEC_TABLE_ROW.OHT_SIZE.ToString())
                    ModelManager.Instance.OHTSize = Convert.ToDouble(reader[SPEC_TABLE_COLUMN.VALUE.ToString()]);
                if (reader[SPEC_TABLE_COLUMN.NAME.ToString()].ToString() == SPEC_TABLE_ROW.PORT_SIZE.ToString())
                    ModelManager.Instance.PortSize = new Vector2(Convert.ToDouble(reader[SPEC_TABLE_COLUMN.VALUE.ToString()]), Convert.ToDouble(reader[SPEC_TABLE_COLUMN.VALUE.ToString()]));
                if (reader[SPEC_TABLE_COLUMN.NAME.ToString()].ToString() == SPEC_TABLE_ROW.OHT_LOADING_TIME.ToString())
                    ModelManager.Instance.OHTLoadingTime = Convert.ToDouble(reader[SPEC_TABLE_COLUMN.VALUE.ToString()]);
                if (reader[SPEC_TABLE_COLUMN.NAME.ToString()].ToString() == SPEC_TABLE_ROW.OHT_UNLOADING_TIME.ToString())
                    ModelManager.Instance.OHTUnloadingTime = Convert.ToDouble(reader[SPEC_TABLE_COLUMN.VALUE.ToString()]);
                if (reader[SPEC_TABLE_COLUMN.NAME.ToString()].ToString() == SPEC_TABLE_ROW.OHT_MINIMUM_DISTANCE.ToString())
                    ModelManager.Instance.OHTMinimumDistance = Convert.ToDouble(reader[SPEC_TABLE_COLUMN.VALUE.ToString()]);
                if (reader[SPEC_TABLE_COLUMN.NAME.ToString()].ToString() == SPEC_TABLE_ROW.REROUTING_INDICATOR.ToString())
                    ModelManager.Instance.ReroutingIndicator = Convert.ToDouble(reader[SPEC_TABLE_COLUMN.VALUE.ToString()]);
            }
        }

        public void CreateFabSimModels()
        {
            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.FAB.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                ModelManager.Instance.AddFab(reader[FAB_TABLE_COLUMN.FAB_NAME.ToString()].ToString());
            }
        }

        public void CreateBaySimModels()
        {
            string bayName;
            string fabName;
            string type;
            bool reticle;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.BAY.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                fabName = reader[BAY_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                bayName = reader[BAY_TABLE_COLUMN.BAY_NAME.ToString()].ToString();
                type = reader[BAY_TABLE_COLUMN.TYPE.ToString()].ToString();
                reticle = Convert.ToBoolean(reader[(int)BAY_TABLE_COLUMN.RETICLE].ToString());

                ModelManager.Instance.AddBay(bayName, fabName, type, reticle);
            }
        }

        public void CreateAddressSimModels()
        {
            uint nodeID;
            string fabName;
            string name;
            double position_X;
            double position_Y;
            string zcuName;
            string zcuType;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.ADDRESS.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                nodeID = Convert.ToUInt32(reader[ADDRESS_TABLE_COLUMN.ID.ToString()]);
                fabName = reader[ADDRESS_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                name = reader[ADDRESS_TABLE_COLUMN.ADDRESS_NAME.ToString()].ToString();
                position_X = Convert.ToDouble(reader[ADDRESS_TABLE_COLUMN.POSITION_X.ToString()]);
                position_Y = Convert.ToDouble(reader[ADDRESS_TABLE_COLUMN.POSITION_Y.ToString()]);
                zcuName = reader[ADDRESS_TABLE_COLUMN.ZCU_NAME.ToString()].ToString();
                zcuType = reader[ADDRESS_TABLE_COLUMN.ZCU_TYPE.ToString()].ToString();

                RailPointNode point = ModelManager.Instance.AddRailPoint(nodeID, new Vector3(position_X, position_Y, 0), name, fabName, zcuName, zcuType);
            }
        }

        public void CreateZcuSimModels()
        {
            string fabName;
            string zcuName;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.ZCU.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                fabName = reader[ZCU_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                zcuName = reader[ZCU_TABLE_COLUMN.ZCU_NAME.ToString()].ToString();

                ModelManager.Instance.AddZcu(fabName, zcuName);
            }
        }

        public void CreateRailSimModels()
        {
            uint linkID;
            string fabName;
            string name;
            uint fromPointID;
            uint toPointID;
            double linkSpeed;
            bool isCurve;
            double distance;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.RAIL.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                linkID = Convert.ToUInt32(reader[RAIL_TABLE_COLUMN.ID.ToString()].ToString());
                fabName = reader[RAIL_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                name = reader[RAIL_TABLE_COLUMN.LINE_NAME.ToString()].ToString();
                fromPointID = Convert.ToUInt32(reader[RAIL_TABLE_COLUMN.FROM_ADDRESS_ID.ToString()].ToString());
                toPointID = Convert.ToUInt32(reader[RAIL_TABLE_COLUMN.TO_ADDRESS_ID.ToString()].ToString());
                linkSpeed = Convert.ToDouble(reader[RAIL_TABLE_COLUMN.MAX_SPEED.ToString()].ToString());
                isCurve = Convert.ToBoolean(reader[RAIL_TABLE_COLUMN.CURVE.ToString()].ToString());
                distance = Convert.ToDouble(reader[RAIL_TABLE_COLUMN.DISTANCE.ToString()].ToString());

                RailLineNode line = ModelManager.Instance.AddRailLine(linkID, name, fabName, fromPointID, toPointID, distance, isCurve, linkSpeed);
            }
        }

        public void CreateCandidatePathSimModels()
        {
            PathFinder.Instance.InitializeCandidatePaths();
            string fabName;
            uint fromPointID;
            uint toPointID;
            bool reticle;
            double distance;
            string candidatePath;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.CANDIDATE_PATH.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                fabName = reader[CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                fromPointID = Convert.ToUInt32(reader[CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString()].ToString());
                toPointID = Convert.ToUInt32(reader[CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString()].ToString());
                reticle = Convert.ToBoolean(reader[CANDIDATE_PATH_TABLE_COLUMN.RETICLE.ToString()].ToString());
                distance = Convert.ToDouble(reader[CANDIDATE_PATH_TABLE_COLUMN.DISTANCE.ToString()].ToString());
                candidatePath = reader[CANDIDATE_PATH_TABLE_COLUMN.CANDIDATE_PATH_ID.ToString()].ToString();

                if (candidatePath != string.Empty)
                    PathFinder.Instance.AddCandidatePathByMemory(fabName, fromPointID, toPointID, reticle, candidatePath);
            }
        }

        public List<CandidatePath> CreateCandidatePaths(string fabName, uint fromPointID, uint toPointID)
        {
            List<CandidatePath> candidatePaths = new List<CandidatePath>();

            bool reticle;
            double distance;
            string candidatePath;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.CANDIDATE_PATH.ToString()
                + " WHERE " + CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString() + " = '" + fabName + "'"
                + " AND " + CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString() + " = " + fromPointID + ""
                + " AND " + CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString() + " = " + toPointID + "";

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                fabName = reader[CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                fromPointID = Convert.ToUInt32(reader[CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString()].ToString());
                toPointID = Convert.ToUInt32(reader[CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString()].ToString());
                reticle = Convert.ToBoolean(reader[CANDIDATE_PATH_TABLE_COLUMN.RETICLE.ToString()].ToString());
                distance = Convert.ToDouble(reader[CANDIDATE_PATH_TABLE_COLUMN.DISTANCE.ToString()].ToString());
                candidatePath = reader[CANDIDATE_PATH_TABLE_COLUMN.CANDIDATE_PATH_ID.ToString()].ToString();

                if(candidatePath != string.Empty)
                candidatePaths.Add(PathFinder.Instance.AddCandidatePath(fabName, fromPointID, toPointID, reticle, candidatePath));
            }

            return candidatePaths;
        }

        public void CreateEquipmentSimModels()
        {
            uint id;
            string fabName;
            string eqpId;
            string processGroup;
            string type;
            string stepGroup;
            string bayName;
            string parentEqpId;
            int isActive;
            string presetId;
            double operatingRatio;
            int chamberCount;
            int minBatchSize;
            int maxBatchSize;
            double batchWaitTime;
            string status;
            DateTime statusChangeTime;
            string dispatcherType;
            string statusCode;
            int capacity;
            double width;
            double height;
            double position_X;
            double position_Y;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.EQUIPMENT.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                id = Convert.ToUInt32(reader[EQUIPMENT_TABLE_COLUMN.ID.ToString()].ToString());
                fabName = reader[EQUIPMENT_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                eqpId = reader[EQUIPMENT_TABLE_COLUMN.EQP_ID.ToString()].ToString();
                processGroup = reader[EQUIPMENT_TABLE_COLUMN.PROCESS_GROUP.ToString()].ToString();
                type = reader[EQUIPMENT_TABLE_COLUMN.TYPE.ToString()].ToString();
                stepGroup = reader[EQUIPMENT_TABLE_COLUMN.STEP_GROUP.ToString()].ToString();
                bayName = reader[EQUIPMENT_TABLE_COLUMN.BAY.ToString()].ToString();
                //parentEqpId = reader[EQUIPMENT_TABLE_COLUMN.PARENT_EQP_ID.ToString()].ToString();
                //isActive = Convert.ToInt32(reader[EQUIPMENT_TABLE_COLUMN.IS_ACTIVE.ToString()].ToString());
                //presetId = reader[EQUIPMENT_TABLE_COLUMN.PRESET_ID.ToString()].ToString();
                //operatingRatio = Convert.ToDouble(reader[EQUIPMENT_TABLE_COLUMN.OPERATING_RATIO.ToString()].ToString());
                //chamberCount = Convert.ToInt32(reader[EQUIPMENT_TABLE_COLUMN.CHAMBER_COUNT.ToString()].ToString());
                minBatchSize = Convert.ToInt32(reader[EQUIPMENT_TABLE_COLUMN.MIN_BATCH_SIZE.ToString()].ToString());
                maxBatchSize = Convert.ToInt32(reader[EQUIPMENT_TABLE_COLUMN.MAX_BATCH_SIZE.ToString()].ToString());
                //batchWaitTime = Convert.ToDouble(reader[EQUIPMENT_TABLE_COLUMN.BATCH_WAIT_TIME.ToString()].ToString());
                //status = reader[EQUIPMENT_TABLE_COLUMN.STATUS.ToString()].ToString();
                //statusChangeTime = Convert.ToDateTime(reader[EQUIPMENT_TABLE_COLUMN.STATUS_CHANGE_TIME.ToString()].ToString());
                //dispatcherType = reader[EQUIPMENT_TABLE_COLUMN.DISPATCHER_TYPE.ToString()].ToString();
                //statusCode = reader[EQUIPMENT_TABLE_COLUMN.STATUS_CODE.ToString()].ToString();
                capacity = Convert.ToInt32(reader[EQUIPMENT_TABLE_COLUMN.CAPACITY.ToString()].ToString());
                width = Convert.ToDouble(reader[EQUIPMENT_TABLE_COLUMN.WIDTH.ToString()].ToString());
                height = Convert.ToDouble(reader[EQUIPMENT_TABLE_COLUMN.HEIGHT.ToString()].ToString());
                position_X = Convert.ToDouble(reader[EQUIPMENT_TABLE_COLUMN.POSITION_X.ToString()].ToString());
                position_Y = Convert.ToDouble(reader[EQUIPMENT_TABLE_COLUMN.POSITION_Y.ToString()].ToString());

                switch (type)
                {
                    case "COMMIT":
                        CommitEqpNode commitEqp = ModelManager.Instance.AddCommitEqp(id, fabName, eqpId, processGroup, type, stepGroup, bayName);
                        commitEqp.Width = width;
                        commitEqp.Height = height;
                        commitEqp.PosVec3 = new Vector3(position_X, position_Y, 0);
                        commitEqp.NodeType = NODE_TYPE.COMMIT;
                        break; 
                    case "COMPLETE":
                        CompleteEqpNode completeEqp = ModelManager.Instance.AddCompleteEqp(id, fabName, eqpId, processGroup, type, stepGroup, bayName);
                        completeEqp.Width = width;
                        completeEqp.Height = height;
                        completeEqp.PosVec3 = new Vector3(position_X, position_Y, 0);
                        completeEqp.NodeType = NODE_TYPE.COMPLETE;
                        break;
                    default:
                        ProcessEqpNode eqp = ModelManager.Instance.AddProcessEqp(id, fabName, eqpId, processGroup, type, stepGroup, bayName, Convert.ToInt32(minBatchSize), Convert.ToInt32(maxBatchSize));
                        eqp.Width = width;
                        eqp.Height = height;
                        eqp.PosVec3 = new Vector3(position_X, position_Y, 0);
                        eqp.NodeType = NODE_TYPE.PROCESS;
                        break;
                }
            }
        }

        public void CreateBufferSimModels()
        {
            uint id;
            string fabName;
            string name;
            string type;
            uint capacity;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.BUFFER.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                id = Convert.ToUInt32(reader[BUFFER_TABLE_COLUMN.ID.ToString()].ToString());
                fabName = reader[BUFFER_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                name = reader[BUFFER_TABLE_COLUMN.NAME.ToString()].ToString();
                type = reader[BUFFER_TABLE_COLUMN.TYPE.ToString()].ToString();
                capacity = Convert.ToUInt32(reader[BUFFER_TABLE_COLUMN.CAPACITY.ToString()].ToString());

                ModelManager.Instance.AddTB(id, name, fabName, type, capacity);
            }
        }

        public void CreateStationSimModels()
        {
            uint id;
            string fabName;
            string type;
            string name;
            string eqpName;
            string railLineName;
            double distance;
            bool bumpAllowed;
            bool waitAllowed;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.STATION.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                id = Convert.ToUInt32(reader[STATION_TABLE_COLUMN.ID.ToString()].ToString());
                fabName = reader[STATION_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                type = reader[STATION_TABLE_COLUMN.INOUT_TYPE.ToString()].ToString();
                name = reader[STATION_TABLE_COLUMN.PORT_NAME.ToString()].ToString();
                eqpName = reader[STATION_TABLE_COLUMN.EQP_NAME.ToString()].ToString();
                railLineName = reader[STATION_TABLE_COLUMN.RAILLINE_NAME.ToString()].ToString();
                distance = Convert.ToDouble(reader[STATION_TABLE_COLUMN.DISTANCE.ToString()].ToString());
                waitAllowed = Convert.ToBoolean(reader[STATION_TABLE_COLUMN.WAIT_ALLOWED.ToString()].ToString());
                bumpAllowed = Convert.ToBoolean(reader[STATION_TABLE_COLUMN.BUMP_ALLOWED.ToString()].ToString());

                FabSimNode eqp = ModelManager.Instance.DicEqp[eqpName];

                RailLineNode railLine = ModelManager.Instance.DicRailLine[railLineName];

                // CommitEqpNode / CompleteEqpNode / ProcessEqpNode 3개 중 하나의 ProcessPort
                if(ModelManager.Instance.DicProcessEqpNode.ContainsKey(eqpName))
                {
                    ProcessPortNode processPort;

                    ProcessEqpNode processEqp = eqp as ProcessEqpNode;

                    switch (processEqp)
                    {
                        case CommitEqpNode commitEqp:
                            processPort = ModelManager.Instance.AddProcessPort(id, name, railLine, distance, commitEqp);
                            break;
                        case CompleteEqpNode completeEqp:
                            processPort = ModelManager.Instance.AddProcessPort(id, name, railLine, distance, completeEqp);
                            break;
                        default:
                            processPort = ModelManager.Instance.AddProcessPort(id, name, railLine, distance, processEqp);
                            break;
                    }
                }
                // Side Track Buffer
                else
                {
                    RailPortNode port = ModelManager.Instance.AddRailPort(id, fabName, name, railLineName, distance, eqpName, type);
                }
            }
        }

        public void CreateWipInitSimModels()
        {
            uint uID;
            string fabName;
            string lotId;
            string productId;
            string processId;
            string stepId;
            int lotQty;
            LOT_STATE lotState;
            string loadedEqp;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.WIP_INIT.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                //uID = Convert.ToUInt32(reader[VEHICLE_TABLE_COLUMN.ID.ToString()].ToString());
                fabName = reader[WIP_INIT_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                lotId = reader[WIP_INIT_TABLE_COLUMN.LOT_ID.ToString()].ToString();
                productId = reader[WIP_INIT_TABLE_COLUMN.PRODUCT_ID.ToString()].ToString();
                processId = reader[WIP_INIT_TABLE_COLUMN.PROCESS_ID.ToString()].ToString();
                stepId = reader[WIP_INIT_TABLE_COLUMN.STEP_ID.ToString()].ToString();
                lotQty = Convert.ToInt32(reader[WIP_INIT_TABLE_COLUMN.LOT_QTY.ToString()].ToString());
                lotState = (LOT_STATE)Enum.Parse(typeof(LOT_STATE), reader[WIP_INIT_TABLE_COLUMN.LOT_STATE.ToString()].ToString());
                loadedEqp = reader[WIP_INIT_TABLE_COLUMN.LOADED_EQP.ToString()].ToString();

                ProcessEqpNode currentEqp = null;

                if (loadedEqp != string.Empty)
                {
                    currentEqp = ModelManager.Instance.DicProcessEqpNode[loadedEqp];
                    currentEqp.NodeState = PROCESSEQP_STATE.PROCESSING;
                }

                FOUP_STATE foupState = lotState == LOT_STATE.RUN ? FOUP_STATE.PROCESSING : FOUP_STATE.BUFFER;

                Foup foup = ModelManager.Instance.AddFoup(fabName, lotId, stepId, processId, productId, string.Empty, currentEqp, foupState, lotQty);
            }
        }

        public void CreateEqpHistorySimModels()
        {
            uint uID;
            string fabName;
            string eqpId;
            string lotId;
            string stepId;
            DateTime arrivalTime;
            DateTime startTime;
            DateTime endTime;
            double waitTimeMin;
            string processId;
            double processingTimeMin;
            int lotQty;
            string productId;
            string stepType;
            STEP_GROUP stepGroup;
            string processGroup;
            int sequence;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.EQP_HISTORY.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                //uID = Convert.ToUInt32(reader[EQP_HISTORY_TABLE_COLUMN.ID.ToString()].ToString());
                fabName = reader[EQP_HISTORY_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                eqpId = reader[EQP_HISTORY_TABLE_COLUMN.EQP_ID.ToString()].ToString();
                lotId = reader[EQP_HISTORY_TABLE_COLUMN.LOT_ID.ToString()].ToString();
                stepId = reader[EQP_HISTORY_TABLE_COLUMN.STEP_ID.ToString()].ToString();
                arrivalTime = Convert.ToDateTime(reader[EQP_HISTORY_TABLE_COLUMN.ARRIVAL_TIME.ToString()].ToString());
                startTime = Convert.ToDateTime(reader[EQP_HISTORY_TABLE_COLUMN.START_TIME.ToString()].ToString());
                endTime = Convert.ToDateTime(reader[EQP_HISTORY_TABLE_COLUMN.END_TIME.ToString()].ToString());
                waitTimeMin = Convert.ToDouble(reader[EQP_HISTORY_TABLE_COLUMN.WAIT_TIME_MIN.ToString()].ToString());
                processId = reader[EQP_HISTORY_TABLE_COLUMN.PROCESS_ID.ToString()].ToString();
                processingTimeMin = Convert.ToDouble(reader[EQP_HISTORY_TABLE_COLUMN.PROCESSING_TIME_MIN.ToString()].ToString());
                lotQty = Convert.ToInt32(reader[EQP_HISTORY_TABLE_COLUMN.LOT_QTY.ToString()].ToString());
                productId = reader[EQP_HISTORY_TABLE_COLUMN.PRODUCT_ID.ToString()].ToString();
                stepType = reader[EQP_HISTORY_TABLE_COLUMN.STEP_TYPE.ToString()].ToString();
                stepGroup = (STEP_GROUP)Enum.Parse(typeof(STEP_GROUP), reader[EQP_HISTORY_TABLE_COLUMN.STEP_GROUP.ToString()].ToString());
                processGroup = reader[EQP_HISTORY_TABLE_COLUMN.PROCESS_GROUP.ToString()].ToString();
                sequence = Convert.ToInt32(reader[EQP_HISTORY_TABLE_COLUMN.SEQUENCE.ToString()].ToString());

                Foup foup = null;

                #region Create FoupHistory
                FoupHistory foupHistory = new FoupHistory();
                foupHistory.FabName = fabName;
                foupHistory.FoupID = lotId;
                foupHistory.EqpID = eqpId;
                foupHistory.StepID = stepId;
                foupHistory.ArrivalTime = arrivalTime;
                foupHistory.StartTime = startTime;
                foupHistory.EndTime = endTime;
                foupHistory.LotQty = lotQty;
                foupHistory.ProductID = productId;
                foupHistory.StepType = stepType;

                // FAB IN or FAB OUT
                switch (stepType)
                {
                    // 신규 Lot
                    case "FAB IN":
                        CommitEqpNode commitEqpNode = ModelManager.Instance.CommitEqpNode;

                        // 초기재공에 있다면
                        if (ModelManager.Instance.Foups.ContainsKey(lotId))
                        {
                            foup = ModelManager.Instance.Foups[lotId];
                            foup.CurrentEqp = commitEqpNode;
                            foup.CurrentNode = commitEqpNode;
                            foup.CurrentState = FOUP_STATE.READY_FOR_FAB_IN;
                        }
                        // 초기재공에 없다면
                        else
                        {
                            foup = ModelManager.Instance.AddFoup(fabName, lotId, stepId, processId, productId, stepType, commitEqpNode, FOUP_STATE.READY_FOR_FAB_IN, lotQty);
                            foup.CurrentEqp = commitEqpNode;
                        }
                        break;
                    default:
                        break;
                }
                #endregion
                #region Create EqpHistory
                EqpHistory eqpHistory = new EqpHistory();
                eqpHistory.FabName = fabName;
                eqpHistory.EqpID = eqpId;
                eqpHistory.FoupID = lotId;
                eqpHistory.StepID = stepId;
                eqpHistory.ArrivalTime = arrivalTime;
                eqpHistory.StartTime = startTime;
                eqpHistory.EndTime = endTime;
                eqpHistory.WaitTimeMin = waitTimeMin;
                eqpHistory.LotQty = lotQty;
                eqpHistory.ProductID = productId;
                eqpHistory.StepType = stepType;
                eqpHistory.Sequence = sequence;
                eqpHistory.ProcessingTimeMin = processingTimeMin;
                #endregion

                #region Add FoupHistory
                if (ModelManager.Instance.Foups.ContainsKey(lotId))
                {
                    foup = ModelManager.Instance.Foups[lotId];
                    foup.Historys.AddHistory(foupHistory);
                }
                #endregion

                #region Add EqpHistory
                ProcessEqpNode processEqp = ModelManager.Instance.DicProcessEqpNode[eqpId];

                processEqp.RemainHistorys.AddHistory(eqpHistory);
                #endregion
            }

            #region Sort Foup & ProcessEqp & CommitEqp & CompleteEqp
            foreach (Foup foup in ModelManager.Instance.Foups.Values)
            {
                foup.Historys.SortHistoryByStartTime();
                foup.Historys.SetFoupSequence();
            }

            foreach (ProcessEqpNode processEqp in ModelManager.Instance.DicProcessEqpNode.Values)
            {
                processEqp.RemainHistorys.SortHistoryBySequence();
            }
            #endregion
        }

        public void CreateVehicleSimModels()
        {
            uint uID;
            string fabName;
            string name;
            bool reticle;
            string railLineName;
            double distance;
            double ohtSpeed;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.VEHICLE.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();

            while (reader.Read() == true)
            {
                uID = Convert.ToUInt32(reader[VEHICLE_TABLE_COLUMN.ID.ToString()].ToString());
                name = reader[VEHICLE_TABLE_COLUMN.NAME.ToString()].ToString();
                fabName = reader[VEHICLE_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                reticle = Convert.ToBoolean(reader[VEHICLE_TABLE_COLUMN.RETICLE.ToString()].ToString());
                railLineName = reader[VEHICLE_TABLE_COLUMN.RAILLINE_NAME.ToString()].ToString();
                distance = Convert.ToDouble(reader[VEHICLE_TABLE_COLUMN.DISTANCE.ToString()].ToString());
                ohtSpeed = Convert.ToDouble(reader[VEHICLE_TABLE_COLUMN.SPEED.ToString()].ToString());

                OHTNode oht = ModelManager.Instance.AddOHT(uID, name, fabName, reticle, railLineName, distance, ohtSpeed);

                Fab fab = ModelManager.Instance.Fabs[fabName];
                RailLineNode line = ModelManager.Instance.DicRailLine[railLineName];
            }
        }

        public void CreateZoneNetworkSimModels()
        {
            string fabName;
            string zoneName;
            string zoneType;
            string lineName;
            string zoneLineType;

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.ZONE_NETWORK.ToString();

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                fabName = reader[ZONE_NETWORK_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                zoneName = reader[ZONE_NETWORK_TABLE_COLUMN.ZONE_NAME.ToString()].ToString();
                zoneType = reader[ZONE_NETWORK_TABLE_COLUMN.ZONE_TYPE.ToString()].ToString();
                lineName = reader[ZONE_NETWORK_TABLE_COLUMN.LINE_NAME.ToString()].ToString();
                zoneLineType = reader[ZONE_NETWORK_TABLE_COLUMN.ZONE_LINE_TYPE.ToString()].ToString();

                ZONE_TYPE zoneTypeEnum = (ZONE_TYPE)Enum.Parse(typeof(ZONE_TYPE), zoneType);
                ZONE_LINE_TYPE zoneLineTypeEnum = (ZONE_LINE_TYPE)Enum.Parse(typeof(ZONE_LINE_TYPE), zoneLineType);

                Fab fab = ModelManager.Instance.Fabs[fabName];
                RailLineNode line = ModelManager.Instance.DicRailLine[lineName];

                if (line == null)
                    continue;

                switch (zoneTypeEnum)
                {
                    case ZONE_TYPE.BAY:
                        Bay bay = ModelManager.Instance.Bays[zoneName];
                        line.DicBay.Add(zoneName, zoneLineTypeEnum);
                        line.Bay = bay;

                        switch (zoneLineTypeEnum)
                        {
                            case ZONE_LINE_TYPE.STOP:
                                bay.FromLines.Add(line);
                                break;
                            case ZONE_LINE_TYPE.RESET:
                                bay.ToLines.Add(line);
                                break;
                        }
                        break;
                    case ZONE_TYPE.HID:
                        break;
                }
            }
        }

        public void CreateRouteSelectionSimModels()
        {
            try
            {
                string fabName;
                string fromBayName;
                string toBayName;
                uint fromBayOutLineID;
                uint toBayInLineID;
                int minPriority;
                int maxPriority;
                string wayPointList;

                string query = string.Empty;
                query = "SELECT * FROM " + TABLE_TYPE.ROUTE_SELECTION.ToString();

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    fabName = reader[ROUTE_SELECTION_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                    fromBayName = reader[ROUTE_SELECTION_TABLE_COLUMN.FROM_BAY_NAME.ToString()].ToString();
                    toBayName = reader[ROUTE_SELECTION_TABLE_COLUMN.TO_BAY_NAME.ToString()].ToString();
                    fromBayOutLineID = Convert.ToUInt32(reader[ROUTE_SELECTION_TABLE_COLUMN.FROM_BAY_OUT_LINE_ID.ToString()].ToString());
                    toBayInLineID = Convert.ToUInt32(reader[ROUTE_SELECTION_TABLE_COLUMN.TO_BAY_IN_LINE_ID.ToString()].ToString());
                    minPriority = Convert.ToInt32(reader[ROUTE_SELECTION_TABLE_COLUMN.MIN_PRIORITY.ToString()].ToString());
                    maxPriority = Convert.ToInt32(reader[ROUTE_SELECTION_TABLE_COLUMN.MAX_PRIORITY.ToString()].ToString());
                    wayPointList = reader[ROUTE_SELECTION_TABLE_COLUMN.VIA_POINT_ID_LIST.ToString()].ToString();

                    Fab fab = ModelManager.Instance.Fabs[fabName];
                    Bay fromBay = ModelManager.Instance.Bays[fromBayName];
                    Bay toBay = ModelManager.Instance.Bays[toBayName];
                    RailLineNode fromBayOutline = ModelManager.Instance.GetSimNodebyID(fromBayOutLineID) as RailLineNode;
                    RailLineNode toBayInline = ModelManager.Instance.GetSimNodebyID(toBayInLineID) as RailLineNode;

                    string[] splittedString = wayPointList.Split(ModelManager.Instance.Separator);

                    Dictionary<string, RailPointNode> wayPoints = new Dictionary<string, RailPointNode>();
                    foreach (string pointID in splittedString)
                    {
                        if (pointID == "")
                            continue;

                        RailPointNode point = ModelManager.Instance.GetSimNodebyID(Convert.ToUInt32(pointID)) as RailPointNode;
                        wayPoints.Add(point.Name, point);
                    }

                    if (fromBayOutline == null || toBayInline == null)
                        continue;

                    RouteSelection routeSelection = new RouteSelection(fab, fromBay, toBay, fromBayOutline, toBayInline, minPriority, maxPriority, wayPoints);
                    ModelManager.Instance.AddRouteSelection(fab.Name, routeSelection);

                    //Route Selection Class 추가한 뒤에 생성하는 것 필요.
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("RouteSelection Table Error: " + ex.Message);
            }
        }

        public void CreateRailCutSimModels()
        {
            try
            {
                string fabName;
                uint lineID;

                string query = string.Empty;
                query = "SELECT * FROM " + TABLE_TYPE.RAIL_CUT.ToString();

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    fabName = reader[RAIL_CUT_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                    lineID = Convert.ToUInt32(reader[RAIL_CUT_TABLE_COLUMN.LINE_ID.ToString()].ToString());

                    Fab fab = ModelManager.Instance.Fabs[fabName];
                    RailLineNode line = ModelManager.Instance.GetSimNodebyID(lineID) as RailLineNode;

                    if (line == null)
                        continue;

                    ModelManager.Instance.RailCuts[fab.Name].Add(line);
                    //Rail Cut Class 추가한 뒤에 생성하는 것 필요.
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("RailCut Table Error: " + ex.Message);
            }
        }


        public bool IsCandidatePath()
        {
            int count = 0;

            if (ConnectDB())
            {
                string query = string.Empty;
                query = "SELECT COUNT(*) FROM " + TABLE_TYPE.CANDIDATE_PATH.ToString();

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                if (reader.Read() == true)
                {
                    count = Convert.ToInt32(reader[0].ToString());
                }
            }

            if (count != 0)
                return true;
            else
                return false;
        }

        public int SelectCommandCount()
        {
            int commandCount = 0;

            if (ConnectDB())
            {
                string query = string.Empty;
                query = "SELECT COUNT(*) FROM " + TABLE_TYPE.COMMAND.ToString();

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                if (reader.Read() == true)
                {
                    commandCount = Convert.ToInt32(reader[0].ToString());
                }
            }
            return commandCount;
        }

        public DateTime SelectFirstCommandActiveTime()
        {
            DateTime commandTime = DateTime.MinValue;

            if (ConnectDB())
            {
                string query = string.Empty;
                query = "SELECT MIN(TIME) FROM " + TABLE_TYPE.COMMAND.ToString();

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                if (reader.Read() == true)
                {
                    commandTime = Convert.ToDateTime(reader[0].ToString());
                }
            }
            return commandTime;
        }

        public DateTime SelectLastCommandActiveTime()
        {
            DateTime commandTime = DateTime.MinValue;

            if (ConnectDB())
            {
                string query = string.Empty;
                query = "SELECT MAX(TIME) FROM " + TABLE_TYPE.COMMAND.ToString();

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                if (reader.Read() == true)
                {
                    commandTime = Convert.ToDateTime(reader[0].ToString());
                }
            }
            return commandTime;
        }

        public void SelectCommand(DateTime simTime, int timeInterval)
        {
            if (ConnectDB())
            {
                string name;
                string fabName;
                DateTime time;
                string fromPort;
                string toPort;
                string foupName;
                int priority;
                DateTime realTime;
                bool reticle;

                string query = string.Empty;
                query = "SELECT * FROM " + TABLE_TYPE.COMMAND.ToString() + " WHERE " + COMMAND_TABLE_COLUMN.TIME + " >= #" + simTime.ToString("yyyy/MM/dd HH:mm:ss")
                    + "# AND " + COMMAND_TABLE_COLUMN.TIME + " < #" + (simTime.AddSeconds(timeInterval)).ToString("yyyy/MM/dd HH:mm:ss") + "#";

                OleDbCommand cmd = new OleDbCommand(query, _conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    name = reader[COMMAND_TABLE_COLUMN.NAME.ToString()].ToString();
                    fabName = reader[COMMAND_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                    time = Convert.ToDateTime(reader[COMMAND_TABLE_COLUMN.TIME.ToString()].ToString());
                    fromPort = reader[COMMAND_TABLE_COLUMN.FROM_STATION.ToString()].ToString();
                    toPort = reader[COMMAND_TABLE_COLUMN.TO_STATION.ToString()].ToString();
                    foupName = reader[COMMAND_TABLE_COLUMN.FOUP_NAME.ToString()].ToString();
                    priority = Convert.ToInt32(reader[COMMAND_TABLE_COLUMN.PRIORITY.ToString()].ToString());
                    realTime = Convert.ToDateTime(reader[COMMAND_TABLE_COLUMN.REAL_TIME.ToString()].ToString());
                    reticle = Convert.ToBoolean(reader[COMMAND_TABLE_COLUMN.RETICLE.ToString()]);

                    Scheduler.Instance.AddCommand(name, fabName, time, fromPort, toPort, foupName, priority, realTime, reticle);
                }
            }
        }

        public bool IsSameCandidatePath(string fabName, uint fromPointID, uint toPointID, bool reticle, double distance, string candidatePath)
        {
            int candidateCount = 0;
            List<CandidatePath> candidatePaths = new List<CandidatePath>();

            string query = string.Empty;
            query = "SELECT * FROM " + TABLE_TYPE.CANDIDATE_PATH.ToString()
                + " WHERE " + CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString() + " = '" + fabName + "'"
                + " AND " + CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString() + " = " + fromPointID
                + " AND " + CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString() + " = " + toPointID;
                ;

            OleDbCommand cmd = new OleDbCommand(query, _conn);
            cmd.ExecuteNonQuery();

            string tempFabName;
            uint tempFromPointID;
            uint tempToPointID;
            bool tempReticle;
            double tempDistance;
            string tempCandidatePath;

            OleDbDataReader reader = cmd.ExecuteReader();
            while (reader.Read() == true)
            {
                tempFabName = reader[CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                tempFromPointID = Convert.ToUInt32(reader[CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString()].ToString());
                tempToPointID = Convert.ToUInt32(reader[CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString()].ToString());
                tempReticle = Convert.ToBoolean(reader[CANDIDATE_PATH_TABLE_COLUMN.RETICLE.ToString()].ToString());
                tempDistance = Convert.ToDouble(reader[CANDIDATE_PATH_TABLE_COLUMN.DISTANCE.ToString()].ToString());
                tempCandidatePath = reader[CANDIDATE_PATH_TABLE_COLUMN.CANDIDATE_PATH_ID.ToString()].ToString();

                if (tempFabName == fabName
                    && tempFromPointID == fromPointID
                    && tempToPointID == toPointID
                    && tempReticle == reticle
                    && Math.Round(tempDistance, 2) == Math.Round(distance, 2)
                    && tempCandidatePath == candidatePath)
                    return true;
            }

            return false;
        }

        #endregion

        #region Save Simulation Model

        #region Create Simulation Model Table
        private void CreateTables(Catalog cat)
        {
            CreateSpecTable(cat);
            CreateFabTable(cat);
            CreateBayTable(cat);
            CreateZcuTable(cat);
            CreateAddressTable(cat);
            CreateRailTable(cat);
            CreateZoneNetworkTable(cat);
            CreateCandidatePathTable(cat);
            CreateEqpHistoryTable(cat);
            CreateEquipmentTable(cat);
            CreateBufferTable(cat);
            CreateStationTable(cat);
            CreateVehicleTable(cat);
            CreateCommandTable(cat);
            CreateWipInitTable(cat);
            CreateRouteSelectionTable(cat);
            CreateRailCutTable(cat);

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(cat.Tables);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(cat.ActiveConnection);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(cat);

        }

        private void CreateSpecTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.SPEC.ToString();
            tbl.Columns.Append(SPEC_TABLE_COLUMN.NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(SPEC_TABLE_COLUMN.VALUE.ToString(), DataTypeEnum.adLongVarWChar);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateFabTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.FAB.ToString();
            tbl.Columns.Append(FAB_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateBayTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.BAY.ToString();
            tbl.Columns.Append(BAY_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(BAY_TABLE_COLUMN.BAY_NAME.ToString(), DataTypeEnum.adVarWChar, 50);
            tbl.Columns.Append(BAY_TABLE_COLUMN.TYPE.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(BAY_TABLE_COLUMN.RETICLE.ToString(), DataTypeEnum.adBoolean);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateZcuTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.ZCU.ToString();
            tbl.Columns.Append(ZCU_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(ZCU_TABLE_COLUMN.ZCU_NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateZoneNetworkTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.ZONE_NETWORK.ToString();
            tbl.Columns.Append(ZONE_NETWORK_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(ZONE_NETWORK_TABLE_COLUMN.ZONE_NAME.ToString(), DataTypeEnum.adVarWChar, 50);
            tbl.Columns.Append(ZONE_NETWORK_TABLE_COLUMN.ZONE_TYPE.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(ZONE_NETWORK_TABLE_COLUMN.LINE_NAME.ToString(), DataTypeEnum.adLongVarWChar);
            tbl.Columns.Append(ZONE_NETWORK_TABLE_COLUMN.ZONE_LINE_TYPE.ToString(), DataTypeEnum.adVarWChar, 25);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateAddressTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.ADDRESS.ToString();
            tbl.Columns.Append(ADDRESS_TABLE_COLUMN.ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(ADDRESS_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(ADDRESS_TABLE_COLUMN.ADDRESS_NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(ADDRESS_TABLE_COLUMN.POSITION_X.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(ADDRESS_TABLE_COLUMN.POSITION_Y.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(ADDRESS_TABLE_COLUMN.ZCU_NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(ADDRESS_TABLE_COLUMN.ZCU_TYPE.ToString(), DataTypeEnum.adVarWChar, 25);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateRailTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.RAIL.ToString();
            tbl.Columns.Append(RAIL_TABLE_COLUMN.ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(RAIL_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(RAIL_TABLE_COLUMN.LINE_NAME.ToString(), DataTypeEnum.adLongVarWChar);
            tbl.Columns.Append(RAIL_TABLE_COLUMN.FROM_ADDRESS_ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(RAIL_TABLE_COLUMN.TO_ADDRESS_ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(RAIL_TABLE_COLUMN.MAX_SPEED.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(RAIL_TABLE_COLUMN.CURVE.ToString(), DataTypeEnum.adBoolean);
            tbl.Columns.Append(RAIL_TABLE_COLUMN.DISTANCE.ToString(), DataTypeEnum.adDouble);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateCandidatePathTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.CANDIDATE_PATH.ToString();
            tbl.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.RETICLE.ToString(), DataTypeEnum.adBoolean);
            tbl.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.DISTANCE.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.CANDIDATE_PATH_ID.ToString(), DataTypeEnum.adLongVarWChar);

            ADOX.Index index = new ADOX.Index();
            index.PrimaryKey = false;
            index.Name = "PK1_CANDIDATE_PATH";
            index.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString(), tbl.Columns[CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString()].Type, tbl.Columns[CANDIDATE_PATH_TABLE_COLUMN.FAB_NAME.ToString()].DefinedSize);
            tbl.Indexes.Append(index);


            index = new ADOX.Index();
            index.PrimaryKey = false;
            index.Name = "PK2_CANDIDATE_PATH";
            index.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString(), tbl.Columns[CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString()].Type, tbl.Columns[CANDIDATE_PATH_TABLE_COLUMN.FROM_ADDRESS_ID.ToString()].DefinedSize);
            tbl.Indexes.Append(index);

            index = new ADOX.Index();
            index.PrimaryKey = false;
            index.Name = "PK3_CANDIDATE_PATH";
            index.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString(), tbl.Columns[CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString()].Type, tbl.Columns[CANDIDATE_PATH_TABLE_COLUMN.TO_ADDRESS_ID.ToString()].DefinedSize);
            tbl.Indexes.Append(index);

            //index = new ADOX.Index();
            //index.PrimaryKey = true;
            //index.Name = "PK3_CANDIDATE_PATH";
            //index.Columns.Append(CANDIDATE_PATH_TABLE_COLUMN.RANK.ToString(), tbl.Columns[CANDIDATE_PATH_TABLE_COLUMN.RANK.ToString()].Type, tbl.Columns[CANDIDATE_PATH_TABLE_COLUMN.RANK.ToString()].DefinedSize);
            //tbl.Indexes.Append(index);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateEqpHistoryTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.EQP_HISTORY.ToString();

            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.EQP_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.LOT_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.STEP_ID.ToString(), DataTypeEnum.adVarWChar, 40);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.ARRIVAL_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.WAIT_TIME_MIN.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.START_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.END_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.LOT_QTY.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.PRODUCT_ID.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.PROCESS_ID.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.STEP_TYPE.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.STEP_GROUP.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.PROCESS_GROUP.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.SEQUENCE.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(EQP_HISTORY_TABLE_COLUMN.PROCESSING_TIME_MIN.ToString(), DataTypeEnum.adDouble);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateEquipmentTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.EQUIPMENT.ToString();

            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.EQP_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.PROCESS_GROUP.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.TYPE.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.STEP_GROUP.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.BAY.ToString(), DataTypeEnum.adVarWChar, 10);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.PARENT_EQP_ID.ToString(), DataTypeEnum.adVarWChar, 10);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.IS_ACTIVE.ToString(), DataTypeEnum.adInteger);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.PRESET_ID.ToString(), DataTypeEnum.adVarWChar, 10);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.OPERATING_RATIO.ToString(), DataTypeEnum.adDouble);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.CHAMBER_COUNT.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.MIN_BATCH_SIZE.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.MAX_BATCH_SIZE.ToString(), DataTypeEnum.adInteger);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.BATCH_WAIT_TIME.ToString(), DataTypeEnum.adDouble);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.STATUS.ToString(), DataTypeEnum.adVarWChar, 10);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.STATUS_CHANGE_TIME.ToString(), DataTypeEnum.adDBDate);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.DISPATCHER_TYPE.ToString(), DataTypeEnum.adVarWChar, 10);
            //tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.STATUS_CODE.ToString(), DataTypeEnum.adVarWChar, 10);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.CAPACITY.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.WIDTH.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.HEIGHT.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.POSITION_X.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(EQUIPMENT_TABLE_COLUMN.POSITION_Y.ToString(), DataTypeEnum.adDouble);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateBufferTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.BUFFER.ToString();
            tbl.Columns.Append(BUFFER_TABLE_COLUMN.ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(BUFFER_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(BUFFER_TABLE_COLUMN.NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(BUFFER_TABLE_COLUMN.TYPE.ToString(), DataTypeEnum.adVarWChar, 10);
            tbl.Columns.Append(BUFFER_TABLE_COLUMN.CAPACITY.ToString(), DataTypeEnum.adInteger);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateStationTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.STATION.ToString();
            tbl.Columns.Append(STATION_TABLE_COLUMN.ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(STATION_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(STATION_TABLE_COLUMN.INOUT_TYPE.ToString(), DataTypeEnum.adVarWChar, 10);
            tbl.Columns.Append(STATION_TABLE_COLUMN.PORT_NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(STATION_TABLE_COLUMN.EQP_NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(STATION_TABLE_COLUMN.RAILLINE_NAME.ToString(), DataTypeEnum.adLongVarWChar);
            tbl.Columns.Append(STATION_TABLE_COLUMN.DISTANCE.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(STATION_TABLE_COLUMN.WAIT_ALLOWED.ToString(), DataTypeEnum.adBoolean);
            tbl.Columns.Append(STATION_TABLE_COLUMN.BUMP_ALLOWED.ToString(), DataTypeEnum.adBoolean);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateVehicleTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.VEHICLE.ToString();
            tbl.Columns.Append(VEHICLE_TABLE_COLUMN.ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(VEHICLE_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(VEHICLE_TABLE_COLUMN.NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(VEHICLE_TABLE_COLUMN.RETICLE.ToString(), DataTypeEnum.adBoolean);
            tbl.Columns.Append(VEHICLE_TABLE_COLUMN.SPEED.ToString(), DataTypeEnum.adDouble);
            tbl.Columns.Append(VEHICLE_TABLE_COLUMN.RAILLINE_NAME.ToString(), DataTypeEnum.adLongVarWChar);
            tbl.Columns.Append(VEHICLE_TABLE_COLUMN.DISTANCE.ToString(), DataTypeEnum.adDouble);
            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateCommandTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.COMMAND.ToString();
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.NAME.ToString(), DataTypeEnum.adVarWChar, 150);
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.FROM_STATION.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.TO_STATION.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.FOUP_NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.PRIORITY.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.REAL_TIME.ToString(), DataTypeEnum.adDate);
            tbl.Columns.Append(COMMAND_TABLE_COLUMN.RETICLE.ToString(), DataTypeEnum.adBoolean);

            ADOX.Index index = new ADOX.Index();
            index.PrimaryKey = false;
            index.Name = "PK1_COMMAND";
            index.Columns.Append(COMMAND_TABLE_COLUMN.TIME.ToString(), tbl.Columns[COMMAND_TABLE_COLUMN.TIME.ToString()].Type, tbl.Columns[COMMAND_TABLE_COLUMN.TIME.ToString()].DefinedSize);
            tbl.Indexes.Append(index);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateWipInitTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.WIP_INIT.ToString();
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 50);
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.FOUP_ID.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.LOT_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.PRODUCT_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.PROCESS_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.STEP_ID.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.LOT_QTY.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.LOT_STATE.ToString(), DataTypeEnum.adVarWChar, 20);
            tbl.Columns.Append(WIP_INIT_TABLE_COLUMN.LOADED_EQP.ToString(), DataTypeEnum.adVarWChar, 100);

            ADOX.Index index = new ADOX.Index();
            index.PrimaryKey = false;
            index.Name = "PK1_WIP_INIT";
            index.Columns.Append(WIP_INIT_TABLE_COLUMN.LOT_ID.ToString(), tbl.Columns[WIP_INIT_TABLE_COLUMN.LOT_ID.ToString()].Type, tbl.Columns[WIP_INIT_TABLE_COLUMN.LOT_ID.ToString()].DefinedSize);
            tbl.Indexes.Append(index);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateRouteSelectionTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.ROUTE_SELECTION.ToString();
            tbl.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 50);
            tbl.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.FROM_BAY_NAME.ToString(), DataTypeEnum.adVarWChar, 25);
            tbl.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.TO_BAY_NAME.ToString(), DataTypeEnum.adVarWChar, 100);
            tbl.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.FROM_BAY_OUT_LINE_ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.TO_BAY_IN_LINE_ID.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.MIN_PRIORITY.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.MAX_PRIORITY.ToString(), DataTypeEnum.adInteger);
            tbl.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.VIA_POINT_ID_LIST.ToString(), DataTypeEnum.adLongVarWChar);

            ADOX.Index index = new ADOX.Index();
            index.PrimaryKey = false;
            index.Name = "PK1_ROUTE_SELECTION";
            index.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.FAB_NAME.ToString(), tbl.Columns[ROUTE_SELECTION_TABLE_COLUMN.FAB_NAME.ToString()].Type, tbl.Columns[ROUTE_SELECTION_TABLE_COLUMN.FAB_NAME.ToString()].DefinedSize);
            index.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.FROM_BAY_NAME.ToString(), tbl.Columns[ROUTE_SELECTION_TABLE_COLUMN.FROM_BAY_NAME.ToString()].Type, tbl.Columns[ROUTE_SELECTION_TABLE_COLUMN.FROM_BAY_NAME.ToString()].DefinedSize);
            index.Columns.Append(ROUTE_SELECTION_TABLE_COLUMN.TO_BAY_NAME.ToString(), tbl.Columns[ROUTE_SELECTION_TABLE_COLUMN.TO_BAY_NAME.ToString()].Type, tbl.Columns[ROUTE_SELECTION_TABLE_COLUMN.TO_BAY_NAME.ToString()].DefinedSize);
            tbl.Indexes.Append(index);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        private void CreateRailCutTable(Catalog cat)
        {
            Table tbl = new Table();
            tbl.Name = TABLE_TYPE.RAIL_CUT.ToString();
            tbl.Columns.Append(RAIL_CUT_TABLE_COLUMN.FAB_NAME.ToString(), DataTypeEnum.adVarWChar, 50);
            tbl.Columns.Append(RAIL_CUT_TABLE_COLUMN.LINE_ID.ToString(), DataTypeEnum.adInteger);

            ADOX.Index index = new ADOX.Index();
            index.PrimaryKey = false;
            index.Name = "PK1_RAIL_CUT";
            index.Columns.Append(RAIL_CUT_TABLE_COLUMN.FAB_NAME.ToString(), tbl.Columns[RAIL_CUT_TABLE_COLUMN.FAB_NAME.ToString()].Type, tbl.Columns[RAIL_CUT_TABLE_COLUMN.FAB_NAME.ToString()].DefinedSize);
            index.Columns.Append(RAIL_CUT_TABLE_COLUMN.LINE_ID.ToString(), tbl.Columns[RAIL_CUT_TABLE_COLUMN.LINE_ID.ToString()].Type, tbl.Columns[RAIL_CUT_TABLE_COLUMN.LINE_ID.ToString()].DefinedSize);
            tbl.Indexes.Append(index);

            cat.Tables.Append(tbl);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(tbl);
        }

        #endregion

        public void UploadSimulationModel()
        {
            UploadSpecTable();
            UploadFabTable();
            UploadBayTable();
            UploadZcuTable();
            UploadAddressTable();
            UploadRailTable();
            UploadZoneNetworkTable();
            UploadEqpHistoryTable();
            UploadEquipmentTable();
            UploadBufferTable();
            UploadStationTable();
            UploadVehicleTable();
            UploadWipInitTable();
            UploadRailCutTable();
            UploadRouteSelectionTable();
            MoveToCommandTable();
        }

        private void UploadSpecTable()
        {
            List<object> values = new List<object>();
            values.Add(SPEC_TABLE_ROW.SIMULATION_START_TIME.ToString());
            values.Add(SimEngine.Instance.StartDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
            Insert(TABLE_TYPE.SPEC, values);

            values.Clear();
            values.Add(SPEC_TABLE_ROW.SIMULATION_END_TIME.ToString());
            values.Add(SimEngine.Instance.EndDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
            Insert(TABLE_TYPE.SPEC, values);

            values.Clear();
            values.Add(SPEC_TABLE_ROW.OHT_SIZE.ToString());
            values.Add(ModelManager.Instance.OHTSize);
            Insert(TABLE_TYPE.SPEC, values);

            values.Clear();
            values.Add(SPEC_TABLE_ROW.PORT_SIZE.ToString());
            values.Add(ModelManager.Instance.PortSize.X);
            Insert(TABLE_TYPE.SPEC, values);

            values.Clear();
            values.Add(SPEC_TABLE_ROW.OHT_LOADING_TIME.ToString());
            values.Add(ModelManager.Instance.OHTLoadingTime);
            Insert(TABLE_TYPE.SPEC, values);

            values.Clear();
            values.Add(SPEC_TABLE_ROW.OHT_UNLOADING_TIME.ToString());
            values.Add(ModelManager.Instance.OHTUnloadingTime);
            Insert(TABLE_TYPE.SPEC, values);

            values.Clear();
            values.Add(SPEC_TABLE_ROW.OHT_MINIMUM_DISTANCE.ToString());
            values.Add(ModelManager.Instance.OHTMinimumDistance);
            Insert(TABLE_TYPE.SPEC, values);

            values.Clear();
            values.Add(SPEC_TABLE_ROW.REROUTING_INDICATOR.ToString());
            values.Add(ModelManager.Instance.ReroutingIndicator);
            Insert(TABLE_TYPE.SPEC, values);

        }

        private void UploadFabTable()
        {
            foreach (Fab fab in ModelManager.Instance.Fabs.Values)
            {
                List<object> values = new List<object>();
                values.Add(fab.Name);
                Insert(TABLE_TYPE.FAB, values);
            }
        }

        private void UploadBayTable()
        {
            foreach (Bay bay in ModelManager.Instance.Bays.Values)
            {
                List<object> values = new List<object>();
                values.Add(bay.Fab.Name);
                values.Add(bay.Name);
                values.Add(bay.BayType.ToString());
                values.Add(bay.Reticle);
                Insert(TABLE_TYPE.BAY, values);
            }
        }

        private void UploadZcuTable()
        {
            foreach (ZCU zcu in ModelManager.Instance.Zcus.Values)
            {
                List<object> values = new List<object>();
                values.Add(zcu.Fab.Name);
                values.Add(zcu.Name);
                Insert(TABLE_TYPE.ZCU, values);
            }
        }

        private void UploadAddressTable()
        {
            foreach (RailPointNode point in ModelManager.Instance.DicRailPoint.Values)
            {
                List<object> values = new List<object>();
                values.Add(point.ID);
                values.Add(point.Fab.Name);
                values.Add(point.Name);
                values.Add(point.PosVec3.X);
                values.Add(point.PosVec3.Y);
                values.Add(point.ZcuName);
                values.Add(point.ZcuType.ToString());
                Insert(TABLE_TYPE.ADDRESS, values);
            }
        }

        private void UploadRailTable()
        {
            foreach (RailLineNode line in ModelManager.Instance.DicRailLine.Values)
            {
                List<object> values = new List<object>();
                values.Add(line.ID);
                values.Add(line.Fab.Name);
                values.Add(line.Name);
                values.Add(line.FromNode.ID);
                values.Add(line.ToNode.ID);
                values.Add(line.MaxSpeed);
                values.Add(line.IsCurve);
                values.Add(line.Length);
                Insert(TABLE_TYPE.RAIL, values);
            }
        }

        private void UploadZoneNetworkTable()
        {
            foreach (RailLineNode line in ModelManager.Instance.DicRailLine.Values)
            {
                foreach (KeyValuePair<string, ZONE_LINE_TYPE> bay in line.DicBay)
                {
                    List<object> values = new List<object>();
                    values.Add(line.Fab.Name);
                    values.Add(bay.Key);
                    values.Add(ZONE_TYPE.BAY.ToString());
                    values.Add(line.Name);
                    values.Add(bay.Value.ToString());
                    Insert(TABLE_TYPE.ZONE_NETWORK, values);
                }
            }
        }

        public void UploadCandidatePath(string fabName, CandidatePath path)
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                ConnectDB();

            List<object> values = new List<object>();
            values.Add(fabName);
            values.Add(path.FromNode.ID);
            values.Add(path.ToNode.ID);
            values.Add(path.Reticle);
            values.Add(path.Length);
            values.Add(path.GetRailLineIDs());
            Insert(TABLE_TYPE.CANDIDATE_PATH, values);
        }

        private void UploadBufferTable()
        {
            foreach (BufferNode buffer in ModelManager.Instance.LstBufferNode)
            {
                List<object> values = new List<object>();
                values.Add(buffer.ID);
                values.Add(buffer.Fab.Name);
                values.Add(buffer.Name);
                values.Add(buffer.BufferType.ToString());
                values.Add(buffer.Capacity);
                Insert(TABLE_TYPE.BUFFER, values);
            }
        }

        private void UploadEqpHistoryTable()
        {
            foreach (ProcessEqpNode eqp in ModelManager.Instance.DicProcessEqpNode.Values)
            {
                foreach(EqpHistory history in eqp.HistoryList)
                {
                    List<object> values = new List<object>();
                    values.Add(eqp.Fab.Name);
                    values.Add(eqp.Name);
                    values.Add(history.FoupID);
                    values.Add(history.StepID);
                    values.Add(history.ArrivalTime);
                    values.Add(history.WaitTimeMin);
                    values.Add(history.StartTime);
                    values.Add(history.LotQty);
                    values.Add(history.ProductID);
                    values.Add(history.StepID); //PROCESS_ID
                    values.Add(history.StepType);
                    values.Add(history.StepGroup);
                    if (eqp.ProcessGroup != null)
                        values.Add(eqp.ProcessGroup.ToString());
                    else
                        values.Add(string.Empty);
                    values.Add(history.Sequence);
                    values.Add(history.ProcessingTimeMin);
                    Insert(TABLE_TYPE.EQP_HISTORY, values);
                }
            }
        }

        private void UploadEquipmentTable()
        {
            foreach (ProcessEqpNode eqp in ModelManager.Instance.DicProcessEqpNode.Values)
            {
                List<object> values = new List<object>();
                values.Add(eqp.ID);
                values.Add(eqp.Fab.Name);
                values.Add(eqp.Name);
                if (eqp.ProcessGroup != null)
                    values.Add(eqp.ProcessGroup.ToString());
                else
                    values.Add(string.Empty);
                values.Add(eqp.ProcessType.ToString());
                values.Add(eqp.StepGroup.ToString());
                values.Add(eqp.BayName);
                //values.Add(string.Empty); // PARENT_EQP_ID
                //values.Add(1); // IS_ACTIVE
                //values.Add(string.Empty); // PRESET_ID
                //values.Add(1.0); // OPERATING_RATIO
                //values.Add(string.Empty); // CHAMBER_COUNT
                values.Add(eqp.MinBatchSize);
                values.Add(eqp.MaxBatchSize);
                //values.Add(0.0); // BATCH_WAIT_TIME
                //values.Add(string.Empty); // STATUS
                //values.Add(new DateTime()); // STATUS_CHANGE_TIME
                //values.Add(string.Empty); // DISPATCHER_TYPE
                //values.Add(string.Empty); // STATUS_CODE
                values.Add(eqp.Capacity);
                values.Add(eqp.Width);
                values.Add(eqp.Height);
                values.Add(eqp.PosVec3.X);
                values.Add(eqp.PosVec3.Y);
                Insert(TABLE_TYPE.EQUIPMENT, values);
            }
        }

        private void UploadStationTable()
        {
            foreach (RailPortNode port in ModelManager.Instance.DicRailPort.Values)
            {
                List<object> values = new List<object>();
                values.Add(port.ID);
                values.Add(port.Fab.Name);
                values.Add(port.PortType.ToString());
                values.Add(port.Name);
                if (port.ConnectedEqp != null)
                    values.Add(port.ConnectedEqp.Name);
                else
                    values.Add(string.Empty);
                values.Add(port.Line.Name);
                values.Add(port.Distance);
                values.Add(port.WaitAllowed);
                values.Add(port.BumpAllowed); 
                Insert(TABLE_TYPE.STATION, values);
            }
        }

        private void UploadVehicleTable()
        {
            foreach (OHTNode oht in ModelManager.Instance.LstOHTNode)
            {
                List<object> values = new List<object>();
                values.Add(oht.ID);
                values.Add(oht.Fab.Name);
                values.Add(oht.Name);
                values.Add(oht.Reticle);
                values.Add(oht.Speed);
                values.Add(oht.CurRailLine.Name);
                values.Add(oht.CurDistance);
                Insert(TABLE_TYPE.VEHICLE, values);
            }
        }

        //private void UploadCommandTable()
        //{
        //    foreach (Command command in Scheduler.Instance.TotalCommandList)
        //    {
        //        List<object> values = new List<object>();
        //        values.Add(command.CommandName);
        //        values.Add(((FabSimNode)command.FromNode).Fab.Name);
        //        values.Add(command.ActivatedDateTime);
        //        values.Add(command.FromNode.Name);
        //        values.Add(command.ToNode.Name);
        //        if (command.Entity != null)
        //            values.Add(command.Entity.Name);
        //        else
        //            values.Add(string.Empty);
        //        values.Add(command.Priority);
        //        values.Add(command.RealCompletedTime);
        //        values.Add(command.Reticle);
        //        Insert(TABLE_TYPE.COMMAND, values);
        //    }
        //}

        private void UploadWipInitTable()
        {
            foreach (Foup foup in ModelManager.Instance.Foups.Values)
            {
                List<object> values = new List<object>();
                values.Add(foup.Fab.Name);
                values.Add(foup.Name);
                values.Add(foup.ProductID);
                values.Add(foup.CurrentProcessID);
                values.Add(foup.CurrentStepID);
                values.Add(foup.LotQty);
                values.Add(foup.CurrentState.ToString());
                values.Add(foup.CurrentEqp.Name);
                Insert(TABLE_TYPE.WIP_INIT, values);
            }
        }

        private void UploadRouteSelectionTable()
        {
            foreach (List<RouteSelection> routeSelections in ModelManager.Instance.RouteSelections.Values)
            {
                foreach(RouteSelection rs in routeSelections)
                {
                    List<object> values = new List<object>();
                    values.Add(rs.Fab.Name);
                    values.Add(rs.FromBay.Name);
                    values.Add(rs.ToBay.Name);
                    values.Add(rs.OutLine.ID);
                    values.Add(rs.InLine.ID);
                    values.Add(rs.MinPriority);
                    values.Add(rs.MaxPriority);
                    values.Add(rs.GetWayPointStrings());
                    Insert(TABLE_TYPE.ROUTE_SELECTION, values);
                }
            }
        }

        private void UploadRailCutTable()
        {
            foreach (List<RailLineNode> railCuts in ModelManager.Instance.RailCuts.Values)
            {
                foreach (RailLineNode line in railCuts)
                {
                    List<object> values = new List<object>();
                    values.Add(line.Fab.Name);
                    values.Add(line.ID);

                    Insert(TABLE_TYPE.RAIL_CUT, values);
                }
            }
        }

        public void UploadCommand(Command command)
        {
            if (ConnectDB())
            {
                List<object> values = new List<object>();
                values.Add(command.Name);
                values.Add(((FabSimNode)command.FromNode).Fab.Name);
                values.Add(command.ActivatedDateTime);
                values.Add(command.FromNode.Name);
                values.Add(command.ToNode.Name);
                if (command.Entity != null)
                    values.Add(command.Entity.Name);
                else
                    values.Add(string.Empty);
                values.Add(command.Priority);
                values.Add(command.RealCompletedTime);
                values.Add(command.Reticle);
                Insert(TABLE_TYPE.COMMAND, values);
            }
        }

        public void RemoveCommandData()
        {
            DeleteAll(TABLE_TYPE.COMMAND);
        }

        public void ParseCommandTable(string commandFilePath, string fabName)
        {
            DateTime start = DateTime.Now;

            DAO.DBEngine dbEngine = new DAO.DBEngine();
            DAO.Database db = dbEngine.OpenDatabase(_DBPath, null, null, ";pwd=");

            DAO.Recordset rs = db.OpenRecordset("COMMAND");

            DAO.Field[] myFields = new DAO.Field[9];

            for(int i = 0; i < rs.Fields.Count; i++)
            {
                myFields[i] = rs.Fields[i];
            }

            string line = string.Empty;

            using (StreamReader stream = new StreamReader(commandFilePath, Encoding.GetEncoding(0)))
            {
                while((line = stream.ReadLine()) != null)
                {
                    string[] splitedLine = line.Split(new char[] { ',' });
                    if (splitedLine.Count() != 8)
                        continue;

                    rs.AddNew();

                    string[] splitedTime = splitedLine[0].Split(new char[] { ' ' });
                    string[] splitedField = splitedLine[1].Split(new char[] { '"' });
                    myFields[0].Value = splitedField[1]; //Command Name
                    myFields[1].Value = fabName;
                    myFields[2].Value = SimEngine.Instance.StartDateTime.AddSeconds(Convert.ToDouble( splitedTime[2]));// time
                    splitedField = splitedLine[5].Split(new char[] { '"' });
                    myFields[3].Value = splitedField[1];//from Station
                    splitedField = splitedLine[6].Split(new char[] { '"' });
                    myFields[4].Value = splitedField[1]; // to Station
                    splitedField = splitedLine[4].Split(new char[] { '"' });
                    myFields[5].Value = splitedField[1]; // foup
                    myFields[6].Value = splitedLine[2]; //Priority
                    myFields[7].Value = 0;
                    if (myFields[0].Value.Contains("N4R"))
                        myFields[8].Value = true; // Reticle
                    else
                        myFields[8].Value = false; // Nomal
                    rs.Update();
                }
            }

            rs.Close();
            db.Close();

            double elapsedTimeInSeconds = DateTime.Now.Subtract(start).TotalSeconds;
            Console.WriteLine("Append took {0} seconds", elapsedTimeInSeconds);
        }

        public void MoveToCommandTable()
        {
            if (_pastDBPath != string.Empty)
            {
                DateTime start = DateTime.Now;

                // 저장과 관련된 DB 코드
                DAO.DBEngine dbEngine = new DAO.DBEngine();
                DAO.Database db = dbEngine.OpenDatabase(_DBPath, null, null, ";pwd=");
                DAO.Recordset rs = db.OpenRecordset("COMMAND");
                DAO.Field[] myFields = new DAO.Field[9];

                for (int i = 0; i < rs.Fields.Count; i++)
                {
                    myFields[i] = rs.Fields[i];
                }

                // Select와 관련된 DB 코드
                string connString = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" +
                                _pastDBPath + "; Persist Security Info=False";
                OleDbConnection conn = new OleDbConnection(connString);
                conn.Open();
                string query = "SELECT * FROM " + TABLE_TYPE.COMMAND.ToString();

                OleDbCommand cmd = new OleDbCommand(query, conn);
                cmd.ExecuteNonQuery();

                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    rs.AddNew();

                    myFields[0].Value = reader[COMMAND_TABLE_COLUMN.NAME.ToString()].ToString(); //Command Name
                    myFields[1].Value = reader[COMMAND_TABLE_COLUMN.FAB_NAME.ToString()].ToString();
                    myFields[2].Value = Convert.ToDateTime(reader[COMMAND_TABLE_COLUMN.TIME.ToString()].ToString());
                    myFields[3].Value = reader[COMMAND_TABLE_COLUMN.FROM_STATION.ToString()].ToString();
                    myFields[4].Value = reader[COMMAND_TABLE_COLUMN.TO_STATION.ToString()].ToString();
                    myFields[5].Value = reader[COMMAND_TABLE_COLUMN.FOUP_NAME.ToString()].ToString();
                    myFields[6].Value = Convert.ToInt32(reader[COMMAND_TABLE_COLUMN.PRIORITY.ToString()].ToString());
                    myFields[7].Value = Convert.ToDateTime(reader[COMMAND_TABLE_COLUMN.REAL_TIME.ToString()].ToString());
                    myFields[8].Value = Convert.ToBoolean(reader[COMMAND_TABLE_COLUMN.RETICLE.ToString()]);
                    rs.Update();
                }

                rs.Close();
                db.Close();
                conn.Close();

                double elapsedTimeInSeconds = DateTime.Now.Subtract(start).TotalSeconds;
                Console.WriteLine("Append took {0} seconds", elapsedTimeInSeconds);
            }
        }

        public void UpdateOHTSpeed(double ohtSpeed)
        {
            foreach (OHTNode oht in ModelManager.Instance.LstOHTNode)
            {
                oht.Speed = ohtSpeed;
                Update(TABLE_TYPE.VEHICLE, VEHICLE_TABLE_COLUMN.NAME, oht.Name, VEHICLE_TABLE_COLUMN.SPEED, ohtSpeed);
            }
        }

        private void UploadProductTable()
        {

        }
    }

    #endregion
}
