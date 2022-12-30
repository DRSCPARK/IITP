using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pinokio.Simulation.Disc;

namespace Pinokio.Simulation.Models.IK
{
    public static class IKDispatcher
    {
        public static DispatchingResult Dispatch(ToolGroupObsn toolGroupObsn, DispatchingResult lastResult)
        {
            var eqpObsns = toolGroupObsn.EQPObservations;
            var coilObsns = toolGroupObsn.CoilObservations;

            List<CoilObsn> waitingCoils = (from coil in coilObsns.Values
                                           where coil.State == "Released"
                                           select coil).ToList();

            var result = new DispatchingResult(lastResult);

            List<EQPObsn> ableToReserveEQPs = (from eqp in eqpObsns.Values
                                                   /*where eqp.State == "Idle" && eqp.PortObsns.ContainingCoils.Count == 0*/
                                               select eqp).ToList();

            // 여기서 ableToReserveEQPs는 모든 EQP 여야할듯

            while (ableToReserveEQPs.Count > 0 && waitingCoils.Count > 0)
            {
                var eqp = GetEQP(result, ableToReserveEQPs);
                var coil = GetNextCoil(toolGroupObsn, result, eqp, waitingCoils);

                result[eqp.Name].Add(coil.Name);

                waitingCoils.Remove(coil);
            }

            return result;
        }
        private static EQPObsn GetEQP(DispatchingResult currentResult, List<EQPObsn> ableToResultEQPs)
        {
            var idleEQPs = new List<EQPObsn>();
            var candidates = new List<EQPObsn>();
            foreach (var eqp in ableToResultEQPs)
            {
                idleEQPs.Add(eqp);

                if (candidates.Count > 0)
                {
                    int currentMin = currentResult[candidates[0].Name].Count;
                    if (currentMin > currentResult[eqp.Name].Count)
                    {
                        candidates.Clear();
                        candidates.Add(eqp);
                    }
                    else if (currentMin == currentResult[eqp.Name].Count)
                        candidates.Add(eqp);
                }
                else
                    candidates.Add(eqp);
            }

            if (idleEQPs.Count > 0)
            {
                var idleCandidates = new List<EQPObsn>();
                foreach (var idleEQP in idleEQPs)
                {
                    if (candidates.Contains(idleEQP))
                        idleCandidates.Add(idleEQP);
                }

                if (idleCandidates.Any(x => x.PortObsns.ContainingCoils == null))
                    return idleCandidates.Find(x => x.PortObsns.ContainingCoils == null);

                if (idleCandidates.Any())
                    return idleCandidates.OrderBy(eqp => eqp.PortObsns.ContainingCoils.Count).First();
            }

            return candidates.OrderBy(eqp => eqp.ExpectedProcessEndTime).First();
        }
        private static CoilObsn GetNextCoil(ToolGroupObsn toolGroupObsn, DispatchingResult currentResult, EQPObsn targetEQP, List<CoilObsn> coils)
        {
            var candidates = new List<CoilObsn>(coils);

            return candidates.First();
        }
    }
}
