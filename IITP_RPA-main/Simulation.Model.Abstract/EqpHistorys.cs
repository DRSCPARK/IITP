using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Model.Abstract
{
    public class EqpHistory
    {
        public string FabName { get; set; }
        public string EqpID { get; set; }
        public string FoupID { get; set; }
        public string StepID { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime SimStartTime { get; set; }
        public DateTime SimEndTime { get; set; }
        public double WaitTimeMin { get; set; }
        public int LotQty { get; set; }
        public string ProductID { get; set; }
        public string StepType { get; set; }
        public string StepGroup { get; set; }
        public int Sequence { get; set; }
        public double ProcessingTimeMin { get; set; }
        public bool IsUsed { get; set; }

        public EqpHistory() { }
    }

    public class EqpHistorys
    {
        #region Variable
        private string _eqpId;
        private List<EqpHistory> _historys;

        public string EqpID
        {
            get { return _eqpId; }
            set { _eqpId = value; }
        }

        public List<EqpHistory> Historys
        {
            get { return _historys; }
            set { _historys = value; }
        }
        #endregion

        public EqpHistorys(string eqpId)
        {
            _eqpId = eqpId;
            _historys = new List<EqpHistory>();
        }

        public void AddHistory(EqpHistory history)
        {
            _historys.Add(history);
        }

        public void RemoveHistory(EqpHistory history)
        {
            _historys.Remove(history);
        }

        public int GetMinSequence()
        {
            int minSequence = 0;

            List<int> sequences = _historys.Select(h => h.Sequence).ToList();

            minSequence = sequences.Min();

            return minSequence;
        }

        public List<EqpHistory> FindNextHistories()
        {
            List<EqpHistory> historys = new List<EqpHistory>();

            if (_historys.Count > 0)
            {
                int nextSequence = GetMinSequence();

                while(true)
                {
                    List<EqpHistory> nextHistories = _historys.Where(h => h.Sequence == nextSequence).ToList();

                    if (nextHistories.Any())
                    {
                        // 해당 sequence의 Foup List가 TrackIn 준비중인지 확인
                        EqpHistory history = nextHistories.First();

                        if (history.IsUsed)
                        {
                            nextSequence++;
                            continue;
                        }
                        else
                        {
                            historys = nextHistories;
                            break;
                        }
                    }
                    else
                        break;
                }
            }

            return historys;
        }

        public EqpHistory FindFirstHistory(string foupId)
        {
            EqpHistory history = null;

            int index = 0;

            while (index < _historys.Count)
            {
                if (_historys[index].FoupID == foupId && _historys[index].IsUsed is false)
                {
                    history = _historys[index];
                    break;
                }

                index++;
            }

            return history;
        }

        public void SortHistoryBySequence()
        {
            _historys.Sort((h1, h2) => h1.Sequence.CompareTo(h2.Sequence));
        }
    }
}
