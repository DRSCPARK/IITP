using Simulation.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Simulation.Model.Abstract
{
    [Serializable]
    public class KPIManager
    {
        static KPIManager _kpiManager;

        static public KPIManager Instance
        {
            get { return _kpiManager; }
        }

        [Category("KPI")]
        [DisplayName("Through-Put")]
        public int ThroughPut
        {
            get; set;
        }

        [Category("KPI")]
        [DisplayName("On-Time Delivery Count")]
        public int OnTimeDeliveryCount
        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
            get; set;
        }

        [Category("KPI")]
        [DisplayName("On-Time Delivery Rate")]
        public double OnTimeDeliveryRate
        {
            get
            {
                if (ThroughPut == 0)
                    return 0;
                else
                    return Math.Round(Convert.ToDouble(OnTimeDeliveryCount) / Convert.ToDouble(ThroughPut) * 100,2);
            }
        }

        public List<double> TravelingTimeList
        {
            get; set;
        }

        [Category("KPI")]
        [DisplayName("Average Traveling Time")]
        public double AvgTravelingTime
        {
            get
            {
                if (TravelingTimeList.Count == 0)
                    return 0;
                else
                    return TravelingTimeList.Average();
            }
        }

        public int DelayCount
        {
            get; set;
        }

        public int RequestCount
        {
            get; set;
        }

        public Time TotalJamTime
        {
            get; set;
        }

        public Time TotalSimTime
        {
            get; set;
        }

        [Category("KPI")]
        [DisplayName("Delay Rate")]
        public double DelayRate
        {
            get
            {
                if (RequestCount == 0)
                    return 0;
                else
                    return Math.Round(Convert.ToDouble(DelayCount) / Convert.ToDouble(RequestCount) * 100, 2);
            }
        }

        [Category("KPI")]
        [DisplayName("Jam Rate")]
        public double JamRate
        {
            get
            {
                if (TotalSimTime == 0)
                    return 0;
                else
                    return Math.Round(Convert.ToDouble(TotalJamTime.TotalSeconds) / Convert.ToDouble(TotalSimTime.TotalSeconds) * 100, 2);
            }
        }

        public KPIManager( )
        {
            _kpiManager = this;
            TravelingTimeList = new List<double>();
        }
    }
}
