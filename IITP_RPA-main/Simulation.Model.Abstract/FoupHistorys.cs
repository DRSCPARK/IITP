using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class FoupHistory
    {
        public string FabName { get; set; }
        public string FoupID { get; set; }
        public string EqpID { get; set; }
        public string StepID { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime SimStartTime { get; set; }
        public int LotQty { get; set; }
        public string ProductID { get; set; }
        public string StepType { get; set; }
        public int Sequence { get; set; }
        public bool IsUsed { get; set; }
        public FoupHistory() { }
    }

    public class FoupHistorys
    {
        #region Variable
        private string _foupId;
        private List<FoupHistory> _historys;

        public string FoupID
        {
            get { return _foupId; }
            set { _foupId = value; }
        }

        public List<FoupHistory> Historys
        {
            get { return _historys; }
            set { _historys = value; }
        }
        #endregion

        public FoupHistorys(string foupId)
        {
            _foupId = foupId;
            _historys = new List<FoupHistory>();
        }

        public void AddHistory(FoupHistory history)
        {
            _historys.Add(history);
        }

        public void RemoveHistory()
        {
            _historys.Remove(_historys.First());
        }

        public void RemoveHistory(FoupHistory history)
        {
            _historys.Remove(history);
        }

        public FoupHistory FindNextHistory()
        {
            FoupHistory history = null;

            if (_historys.Any() is false)
                return history;

            int index = 0;

            while(index < _historys.Count)
            {
                if(_historys[index].IsUsed is false)
                {
                    history = _historys[index];
                    break;
                }

                index++;
            }

            return history;
        }

        public void SortHistoryByStartTime()
        {
            _historys.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime));
        }

        public void SetFoupSequence()
        {
            int sequence = 1;

            for(int index = 0; index < _historys.Count; index++)
            {
                _historys[index].Sequence = sequence;

                sequence++;
            }
        }
    }
}
