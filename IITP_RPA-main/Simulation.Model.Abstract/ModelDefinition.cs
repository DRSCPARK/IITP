using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{

    public enum SIMULATION_TYPE
    {
        MATERIAL_HANDLING, PRODUCTION
    }
    public enum INT_PORT
    {
        RAILLINE_INIT, OHT_OUT, OHT_IN, OHT_PASS, RAILLINE_REVISE, OHT_CHECK_OUT, OHT_MOVE,//new
        PART_GENERATE, //commit
        END_JOB, //process
        CONVEYOR_START_INTERVAL_POINT_MOVE, CONVEYOR_START_INTERVAL_POINT_ARRIVE,
        CONVEYOR_END_INTERVAL_POINT_MOVE, CONVEYOR_END_INTERVAL_POINT_ARRIVE, CONVEYOR_END_POINT, //conveyor line
        RAILLINE_OHT_INIT, RAILLINE_OHT_MOVE, RAILLINE_OHT_PASS_NEXTRAILLINE, RAILLINE_OHT_OUT_INTERVAL_AREA,  //railLine
        OHT_INIT, OHT_UNLOADING, OHT_LOADING, OHT_MOVE_TO_UNLOAD, OHT_MOVE_TO_LOAD, OHT_MOVE_TO_IDLE, OHT_IDLE,//ohtNode
        COMMAND_ASSIGN, COMMAND_SELECT, RESERVE, TRACK_IN, TRACK_OUT, TRY_RESERVE_PROCESSPORT, PORT_LOAD, PORT_UNLOAD, ARRIVE_EQP, WAIT_DEPART,
        EQP_IN, EQP_OUT, PROCESSPORT_EMPTY,
    }

    public enum EXT_PORT
    {
        OHT_IN, OHT_OUT, OHT_MOVE, OHT_CHECK_IN, REVISE, OHT_CHECK_OUT,

        PART, REQUEST_PART,
        RAILLINE_OHT_ENTER, RAILLINE_OHT_START,
        OHT_ARRIVE,

        RESERVE, LOAD,
        WORK_START, UNLOAD,
        READY, ARRIVE_EQP,
        OHT_PARKING,
        COMMAND,

        EQP_IN, EQP_OUT, OHT_LOADING, OHT_UNLOADING, // (Port와 Eqp ==> EQP_IN, EQP_OUT) (Port와 OHT ==> OHT_LOADING, OHT_UNLOADING)

        SEND_FOUP_TO_NEXT, SEND_FOUP_TO_EQP,
    }

    public enum ANIMATION
    {
        PART_GENERATION, PART_WAITING, PART_MOVE_ON_CONVEYOR, PART_ELIMINATION, STOCKER_PART_COUNT,
        OHT_IDLE, OHT_MOVE_TO_LOAD, OHT_LOADING, OHT_MOVE_TO_UNLOAD, OHT_UNLOADING, OHT_MOVE,
        OHT_MOVE_ACC, //가감등속
        OHT_MOVE_TO_REST, //OHT_MOVE는 곧 지우자 
        LOAD, EMPTY, BUSY, IDLE, UNLOAD, PORT_RESERVE,
    }


    #region NodeType
    public enum COMMIT_TYPE
    {
        INTERARRIVAL
    }

    public enum BUFFER_TYPE
    {
        LIFTER, STOCKER, RSTB, LSTB
    }
    public enum PROCESS_TYPE
    {
        SINGLE, INLINE, BATCHINLINE, LOTBATCH,
        COMMIT, COMPLETE
    }
    public enum STEP_GROUP
    {
        CLEAN, CMP, DIFF, ETCH, IMP, METAL, PHOTO, TF
    }
    public enum TOOL_GROUP
    {
        PHOTO, FURNACE, MISC, WETETCH, DRYETCH, IMPL, PROBE,
    }
    public enum COMPLETE_TYPE
    {
        NORMAL,
    }
    public enum BAY_TYPE
    {
        INTRABAY, INTERBAY
    }

    public enum ZONE_TYPE
    {
        BAY, HID, RETICLE
    }

    public enum ZONE_LINE_TYPE
    {
        STOP, RESET, NON
    }

    public enum OHT_TYPE
    {
        NORMAL, RETICLE
    }

    public enum ENTITY_TYPE
    {
        FOUP
    }

    public enum ZCU_TYPE
    {
        STOP, RESET, NON
    }

    public enum NODE_TYPE
    {
        COMMIT, BUFFER, PROCESS, COMPLETE,
        TRNASPORT, CONVEYORLINK, CONVEYORNODE,
        MHE, OHT, SHELF, ROUTE, PORT,
        TYPE_COUNT
    }

    public enum PORT_TYPE
    {
        IN, OUT, BOTH
    }

    public enum BUFFER_QUEUE_TYPE
    {
        FIFO,
    }

    public enum BUFFER_CAPACITY_TYPE
    {
        INFINITE, FINITE
    }
    #endregion

    #region State

    public enum PROCESSEQP_STATE
    {
        IDLE, RESERVED, LOADING, PROCESSING, UNLOADING, NOT_FULL
    }

    public enum PROCESSPORT_STATE
    {
        EMPTY, RESERVED, FULL,
    }

    public enum RAILPORT_STATE
    {
        EMPTY, RESERVED, FULL,
    }


    public enum OHT_STATE
    {
        IDLE, MOVE_TO_LOAD, LOADING, MOVE_TO_UNLOAD, UNLOADING,
    }

    public enum STB_STATE
    {
        EMPTY, RESERVED, FULL,
    }

    public enum STOCKER_STATE
    {
        EMPTY, RESERVED, FULL
    }

    public enum FOUP_STATE
    {
        PROCESSING, BUFFER, OHT, READY_FOR_FAB_IN, WAIT, OUT_WAIT, WAIT_OHT, WAIT_TRACK_IN,
    }

    public enum LOT_STATE
    {
        RUN, WAIT
    }

    public enum COMMAND_STATE
    {
        INACTIVE, QUEUED, RESERVATED, WAITING, ACQUIRING, TRANSFERRING, DEPOSITED, COMPLETED 
    }
    #endregion

    #region Naming

    public enum RETICLE_ZONE_NAME
    {
        RETICLE_ZONE 
    };

    #endregion
}
