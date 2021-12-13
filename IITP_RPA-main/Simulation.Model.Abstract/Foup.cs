//using Microsoft.Analytics.Interfaces;
//using Microsoft.Analytics.Types.Sql;
using Simulation.Engine;
using Simulation.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Simulation.Model.Abstract
{
    public class Foup : SimEntity
    {
        #region Variable
        private Fab _fab;
        private string _productId;
        private string _currentProcessId;
        private string _currentStepId;
        private string _currentStepType;
        private int _lotQty;
        private OHTNode _reservationOHT;
        private FOUP_STATE _currentState;
        private FabSimNode _currentEqp;
        private FoupHistorys _historys;
        private bool _isInitWait;
        private Vector3 _pos;

        protected Time _onTime;
        protected Time _dueDate;
        protected Time _totalTravelingTime;
        protected Time _travelingStartTime;
        protected uint _lotPriority;
        protected uint productType;

        public Fab Fab
        {
            get { return _fab; }
            set { _fab = value; }
        }

        public string CurrentNodeName
        {
            get
            {
                if (CurrentNode == null)
                    return string.Empty;
                else
                    return CurrentNode.Name;
            }
        }

        public string ProductID
        {
            get { return _productId; }
            set { _productId = value; }
        }

        public string CurrentProcessID
        {
            get { return _currentProcessId; }
            set { _currentProcessId = value; }
        }

        public string CurrentStepID
        {
            get { return _currentStepId; }
            set { _currentStepId = value; }
        }

        public string CurrentStepType
        {
            get { return _currentStepType; }
            set { _currentStepType = value; }
        }

        public int LotQty
        {
            get { return _lotQty; }
            set { _lotQty = value; }
        }

        public FOUP_STATE CurrentState
        {
            get { return _currentState; }
            set 
            { 
                _currentState = value;
            }
        }

        public FabSimNode CurrentEqp
        {
            get { return _currentEqp; }
            set { _currentEqp = value; }
        }

        public FoupHistorys Historys
        {
            get { return _historys; }
            set { _historys = value; }
        }

        public List<FoupHistory> HistoryList
        {
            get { return _historys.Historys; }
        }

        public bool IsInitWait
        {
            get { return _isInitWait; }
            set { _isInitWait = value; }
        }

        public Vector3 PosVec3
        {
            get { return _pos; }
            set { _pos = value; }
        }

        //TravelingTime 수집을 위해서 ProcessPortNode에서 Unload될 때마다 기록.
        public Time TravelingStartTime
        {
            get { return _travelingStartTime; }
            set { _travelingStartTime = value; }
        }

        //TravelingTime 수집을 위해서 ProcessPortNode에 Load 될 때마다 TravelingStartTime을 기준으로 기록.
        public Time TotalTravelingTime
        {
            get { return _totalTravelingTime; }
            set { _totalTravelingTime = value; }
        }

        //Commit 이후에 Complete까지 허용된 소요시간.
        public Time DueDate
        {
            get { return _dueDate; }
            set { _dueDate = value; }
        }

        //Target Complete Time
        public Time OnTime
        {
            get { return GenerateTime + DueDate; }
        }

        public uint LotPriority
        { get; set; }

        public OHTNode ReservationOHT
        {
            get { return _reservationOHT; }
            set { _reservationOHT = value; }
        }
        #endregion

        public Foup(uint ID, string name)
            : base(ID, name)
        {
            EntityType = ENTITY_TYPE.FOUP;
            DueDate = 1000000;
            _lotPriority = 1;
        }

        public Foup(string name)
        {
            this.Name = name;
            EntityType = ENTITY_TYPE.FOUP;
            DueDate = 1000000;
            _lotPriority = 1;
            _historys = new FoupHistorys(name);
            _isInitWait = false;
        }

        public Foup(uint ID, string name, uint productType)
            : base(ID, name)
        {
            EntityType = ENTITY_TYPE.FOUP;
            DueDate = 1000000;
            _lotPriority = 1;
            this.productType = productType;
        }

        public Time QTime(Time simTime)
        {
            return simTime - _travelingStartTime;
        }

    }
}