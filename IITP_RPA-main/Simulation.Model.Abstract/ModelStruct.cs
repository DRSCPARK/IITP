using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Engine;
using System.Collections;
using System.ComponentModel;

namespace Simulation.Model.Abstract
{

    public struct CommitData
    {
        public Time TimeStart;
        public Time TimeInter;
        public int EntityCount;
    }

    public class CommitIntervalDataCollection : CollectionBase
    {
        public CommitIntervalData this[int index]
        {
            get { return (CommitIntervalData)List[index]; }
        }

        public void Add(CommitIntervalData emp)
        {
            List.Add(emp);
        }

        public void Remove(CommitIntervalData emp)
        {
            List.Remove(emp);
        }
    }

    public class CommitIntervalData
    {
        #region Private  Variables

        private SimEntity _part;
        private double _startTime;
        private double _interTime;
        private int _partCount;

        #endregion

        #region Public Properties

        [Category("CommitData")]
        [DisplayName("Part")]
        [Description("-------")]
        [TypeConverter(typeof(SimEntityListConverter))]
        public SimEntity Part
        {
            get { return _part; }
            set { _part = value; }
        }

        [Category("CommitData")]
        [DisplayName("Start Time")]
        [Description("-------")]
        public double StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        [Category("CommitData")]
        [DisplayName("Inter Time")]
        [Description("-------")]
        public double InterTime
        {
            get { return _interTime; }
            set { _interTime = value; }
        }

        [Category("CommitData")]
        [DisplayName("Part Count")]
        [Description("-------")]
        public int PartCount
        {
            get { return _partCount; }
            set { _partCount = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Interval (");
            if (_part == null)
                sb.Append("");
            else
                sb.Append(_part.Name);
            sb.Append(")");

            return sb.ToString();
        }
        #endregion
    }
}
