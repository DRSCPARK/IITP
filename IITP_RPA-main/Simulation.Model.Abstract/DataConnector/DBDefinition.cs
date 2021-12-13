using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public enum DB_TYPE
    {
        SIMULATION_MODEL, SIMULATION_RESULT
    };

    #region Simulation Model
    public enum TABLE_TYPE
    {
        SPEC, FAB, BAY, ZCU, ADDRESS, RAIL,
        CANDIDATE_PATH, EQUIPMENT, BUFFER, STATION, VEHICLE, COMMAND,
        PRODUCT, STEP, PROCESS, PLAN, FOUP, ROUTE_SELECTION, RAIL_CUT,
        CONVEYOR,
        ZONE_NETWORK, EQP_HISTORY, WIP_INIT, FOUP_HISTORY,
        RESULT_MAIN, COMMAND_LOG, VEHICLE_LOG, EQP_LOG, FOUP_LOG
    }

    public enum SPEC_TABLE_COLUMN
    {
        NAME = 0, VALUE = 1
    }

    public enum SPEC_TABLE_ROW
    {
        SIMULATION_START_TIME = 0, SIMULATION_END_TIME = 1, OHT_SIZE = 2, PORT_SIZE = 3, OHT_LOADING_TIME = 4, OHT_UNLOADING_TIME = 5, OHT_MINIMUM_DISTANCE = 6, REROUTING_INDICATOR = 7,
    }

    public enum EQUIPMENT_TABLE_COLUMN
    {
        // VMS TABLE과 다른 점 
        // 1. LINE_ID -> FAB_NAME
        // 2. EQP_ID -> NAME
        // 3. SIM_TYPE -> TYPE
        // 4. WIDTH, HEIGHT, POSITION (X/Y) 추가

        ID = 1, FAB_NAME = 2, EQP_ID = 3, PROCESS_GROUP = 4, TYPE = 5, STEP_GROUP = 6, BAY = 7, PARENT_EQP_ID = 8,
        IS_ACTIVE = 9, PRESET_ID = 10, OPERATING_RATIO = 11, CHAMBER_COUNT = 12, MIN_BATCH_SIZE = 13, MAX_BATCH_SIZE = 14,
        BATCH_WAIT_TIME = 15, STATUS = 16, STATUS_CHANGE_TIME = 17, DISPATCHER_TYPE = 18, STATUS_CODE = 19, CAPACITY = 20,
        WIDTH = 21, HEIGHT = 22, POSITION_X = 23, POSITION_Y = 24
    }

    public enum PROCESS_TABLE_COLUMN
    {
        ID = 1, NAME = 2, JOB_TYPE = 3, JOB_ID = 4, TACT_TIME = 5, PROC_TIME = 6
    }

    public enum PRODUCT_TABLE_COLUMN
    {
        ID = 1, NAME = 2, OPERATION_ID_START = 3
    }

    public enum FAB_TABLE_COLUMN
    {
        FAB_NAME = 0
    }

    public enum ZCU_TABLE_COLUMN
    {
        FAB_NAME = 0, ZCU_NAME = 1,
    }
    public enum STATION_TABLE_COLUMN
    {
        ID = 0, FAB_NAME = 1, INOUT_TYPE = 2, PORT_NAME = 3, EQP_NAME = 4, RAILLINE_NAME = 5, DISTANCE = 6,  WAIT_ALLOWED = 7, BUMP_ALLOWED = 8
    }

    public enum ADDRESS_TABLE_COLUMN
    {
        ID = 0, FAB_NAME = 1, ADDRESS_NAME = 2, POSITION_X = 3, POSITION_Y = 4, ZCU_NAME = 5, ZCU_TYPE = 6
    }

    public enum RAIL_TABLE_COLUMN
    {
        ID = 0, FAB_NAME = 1, LINE_NAME = 2, FROM_ADDRESS_ID = 3, TO_ADDRESS_ID = 4, MAX_SPEED = 5, CURVE = 6, DISTANCE = 7
    }

    public enum BAY_TABLE_COLUMN
    {
        FAB_NAME = 0, BAY_NAME = 1, TYPE = 2, RETICLE = 3
    }

    public enum ZONE_NETWORK_TABLE_COLUMN
    {
        FAB_NAME = 0, ZONE_NAME = 1, ZONE_TYPE = 2, LINE_NAME = 3, ZONE_LINE_TYPE = 4
    }

    public enum BUFFER_TABLE_COLUMN
    {
        ID = 1, FAB_NAME = 2, NAME = 3, TYPE = 4, CAPACITY = 5
    }

    public enum VEHICLE_TABLE_COLUMN
    {
        ID = 1, FAB_NAME = 2, NAME = 3, RETICLE = 4, SPEED = 5, RAILLINE_NAME = 6, DISTANCE = 7
    }

    public enum WIP_INIT_TABLE_COLUMN
    {
        FAB_NAME = 1, FOUP_ID = 2, LOT_ID = 3, PRODUCT_ID = 4, PROCESS_ID = 5, STEP_ID = 6, LOT_QTY = 7, LOT_STATE = 8, LOADED_EQP = 9
    }

    public enum EQP_HISTORY_TABLE_COLUMN
    {
        FAB_NAME = 1, EQP_ID = 2, LOT_ID = 3, STEP_ID = 4, ARRIVAL_TIME= 5, START_TIME = 6, END_TIME = 7, WAIT_TIME_MIN = 8, PROCESS_ID = 9, PROCESSING_TIME_MIN = 10, LOT_QTY = 11,
        PRODUCT_ID = 12, STEP_TYPE = 13, STEP_GROUP = 14, PROCESS_GROUP = 15, SEQUENCE = 16
    }

    public enum COMMAND_TABLE_COLUMN
    {
        NAME = 1, FAB_NAME = 2, TIME = 3, FROM_STATION = 4, TO_STATION = 5, FOUP_NAME = 6, PRIORITY = 7, REAL_TIME = 8, RETICLE = 9
    }

    public enum CANDIDATE_PATH_TABLE_COLUMN
    {
        FAB_NAME = 0, FROM_ADDRESS_ID = 1, TO_ADDRESS_ID = 2, RETICLE = 3, DISTANCE = 4, CANDIDATE_PATH_ID = 5
    }

    public enum ROUTE_SELECTION_TABLE_COLUMN
    {
        FAB_NAME = 0, FROM_BAY_NAME = 1, TO_BAY_NAME = 2, FROM_BAY_OUT_LINE_ID = 3, TO_BAY_IN_LINE_ID = 4, MIN_PRIORITY = 5, MAX_PRIORITY = 6, VIA_POINT_ID_LIST = 7
    }

    public enum RAIL_CUT_TABLE_COLUMN
    {
        FAB_NAME = 0, LINE_ID = 1
    }

    #endregion

    #region Simulation Results

    public enum RESULT_MAIN_TABLE_COLUMN
    {
        NAME = 0, VALUE = 1
    }

    public enum RESULT_MAIN_TABLE_ROW
    {
        SIMULATION_START_TIME = 0, SIMULATION_END_TIME =1, FOUP_OHT_COUNT = 2, RETICLE_OHT_COUNT = 3
    }
    public enum COMMAND_LOG_TABLE_COLUMN
    {
        NAME = 0, FAB_NAME = 1, ACTIVATED_TIME = 2, COMPLETED_TIME = 3, FROM_STATION = 4, FROM_STATION_TYPE = 5, TO_STATION = 6, TO_STATION_TYPE = 7, FOUP_NAME = 8, PRIORITY = 9, RETICLE = 10, REAL_TIME = 11, ASSIGNED_TIME = 12, LOADED_TIME = 13, TRANSFERING_DISTANCE = 14, ROUTE_ID_LIST = 15, FIRST_ROUTE_ID_LIST = 16, REROUTING_COUNT = 17, OHT_NAME = 18
    }

    public enum EQP_LOG_TABLE_COLUMN
    {
        EQP_ID = 0, FOUP_ID = 1, PROCESS_TYPE = 2, PRODUCT_ID = 3, STEP_ID = 4, START_TIME = 5, END_TIME = 6, SIM_START_TIME = 7, SIM_END_TIME = 8, STEP_TYPE = 9, STEP_GROUP = 10,
    }
    public enum FOUP_LOG_TABLE_COLUMN
    {
        FOUP_ID = 0, PREV_RESOURCE_ID = 1, NEXT_RESOURCE_ID = 2, PREV_STATE = 3, NEXT_STATE = 4, SIM_DATE_TIME = 5,
    }

    public enum VEHICLE_LOG_TABLE_COLUMN
    {
        ID, MOVING_TIME, MOVING_LENGTH,
    }
    public enum RESULT_TABLE
    {
        PROCESS, MHE, PRODUCTION, SIM_CONFIG
    }

    public enum VEHICLE_TYPE
    {
        ALL, NORMAL, RETICLE
    }

    public enum EQ_TYPE
    {
        ALL, PROCESS, BUFFER
    }
    //제품이랑 OHT를 DB에 등록하는 방법
    //_dbMng.Insert(DBManager.PINOKIO_TABLE.PRODUCT, DBManager.PRODUCT_COLUMN.ID, 제품ID);
    //_dbMng.Insert(DBManager.PINOKIO_TABLE.OHT, DBManager.OHT_COLUMN.ID, OHT의ID);

    //위에 두개로 제품이랑 OHT ID 입력해서 DB에 레코드가 생기면,
    //해당 ID를 찾아서 데이터 입력하는 방법
    //_dbMng.Update(DBManager.PINOKIO_TABLE.PRODUCT, DBManager.PRODUCT_COLUMN.ID, 제품ID, DBManager.PRODUCT_COLUMN.PRODUCT_NAME, 제품이름);
    //_dbMng.Update(DBManager.PINOKIO_TABLE.PRODUCT, DBManager.PRODUCT_COLUMN.ID, 제품ID, DBManager.PRODUCT_COLUMN.AVG_CYCLE_TIME, 사이클타임);

    //_dbMng.Update(DBManager.PINOKIO_TABLE.OHT, DBManager.OHT_COLUMN.ID, OHT의ID, DBManager.OHT_COLUMN.MOVING_TIME, OHT의 무빙타임);
    //_dbMng.Update(DBManager.PINOKIO_TABLE.OHT, DBManager.OHT_COLUMN.ID, OHT의ID, DBManager.OHT_COLUMN.MOVING_LENGTH, OHT의 무빙길이);
    #endregion
}
