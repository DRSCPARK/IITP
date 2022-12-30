using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinokio.Simulation.Models.IK
{
    public enum EQPIntPort
    {
        CoilRelease,
        CoilOut,
        LoadToUncoiler,
        FinishJob,
        SetupFinish,

        LoadStart,
        LoadFinish,
    }

    public enum EQPExtPort
    {
        PassCoil,
        SendToComplete,
        DoNextJob,
        Reserve,
        CheckToUnloadFinish,
        LoadStart,
        SetUpStart,
        ShowKPI,
    }

    public enum TGIntPort
    {
        Dispatch,
        ScheduleFirstDownTime,
        CheckToRepair,
        FinishJob,
    }

    public enum TGExtPort
    {
        NewJob,
        FinishJob,
        RequestNextJob,
        RequestToSendCoil,
    }

    public enum MESIntPort
    {
        StartJob,
        WeekAlram,
        Report,
        Arrive,
        Debug,
        EndOfSimulation,
    }

    public enum MESExtPort
    {
        ProcessingFinish,
        PackagingFinish,
        SendToPackaging,
        SendToProcessing,
        SendToEqpPort,
        SendToComplete,
        WorkerIdle,
        RequestDispatch,
    }

    public enum WKIntPort
    {
        CheckToBreak,
        GoToFactory,
        LeaveTheFactory,
        PackagingFinish,
        ProcessingFinish,
        DoProcessing,
        DoNextJob,
        DeliveryFinish,

    }

    public enum WKExtPort
    {
        DoProcessing,
        DoPackaging,
        CheckToDoAnotherJob,
        Dispatch,
        ProcessingFinish,
        DeliverToEQP,
        DeliverToComplete,
        SetupFinish,
        ShowKPI,

    }

    public enum BuffIntPort
    {
        LoadFinish,
        UnloadFinish,
        LoadToEQP,
    }

    public enum BuffExtPort
    {
        RequestCoil,   // EQP 상태가 00 이고 COIL을 LOAD 할 준비가 완료됐을때
        Reserve,
        LoadPort,
    }
}
