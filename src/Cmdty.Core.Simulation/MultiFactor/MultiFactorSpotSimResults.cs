#region License
// Copyright (c) 2020 Jake Fowler
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without 
// restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following 
// conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Cmdty.TimePeriodValueTypes;
using JetBrains.Annotations;

namespace Cmdty.Core.Simulation.MultiFactor
{
    public sealed class MultiFactorSpotSimResults<T> : ISpotSimResults<T> where T : ITimePeriod<T>
    {
        private readonly Dictionary<T, int> _periodIndices;
        public double[] SpotPrices { get; }
        public double[] MarkovFactors { get; }
        public int NumSteps { get; }
        public int NumSims { get; }
        public int NumFactors { get; }
        public IReadOnlyList<T> SimulatedPeriods { get; }

        public MultiFactorSpotSimResults([NotNull] double[] spotPrices, [NotNull] double[] markovFactors,
            [NotNull] IEnumerable<T> simulatedPeriods, int numSteps, int numSims, int numFactors)
        {
            if (simulatedPeriods == null) throw new ArgumentNullException(nameof(simulatedPeriods));
            SpotPrices = spotPrices ?? throw new ArgumentNullException(nameof(spotPrices));
            MarkovFactors = markovFactors ?? throw new ArgumentNullException(nameof(markovFactors));
            if (spotPrices.Length != numSteps * numSims)
                throw new ArgumentException($"{nameof(spotPrices)} argument array size is inconsistent with {nameof(numSteps)} and " +
                                            $"{nameof(numSims)} arguments.");
            if (markovFactors.Length != numSteps * numSims * numFactors)
                throw new ArgumentException($"{nameof(markovFactors)} argument array size is inconsistent with {nameof(numSteps)}, " +
                                            $"{nameof(numSims)} and {nameof(numFactors)} arguments.");

            NumSteps = numSteps;
            NumSims = numSims;
            NumFactors = numFactors;
            _periodIndices = simulatedPeriods.Select((period, index) => new {period, index})
                .ToDictionary(pair => pair.period, pair => pair.index);
            SimulatedPeriods = _periodIndices.Keys.ToArray();
        }

        public ReadOnlyMemory<double> SpotPricesForPeriod(T period)
        {
            if (!_periodIndices.TryGetValue(period, out int stepIndex))
                throw new ArgumentException($"No simulation for period {period}.", nameof(period));
            return SpotPricesForStepIndex(stepIndex);
        }

        public ReadOnlyMemory<double> SpotPricesForStepIndex(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= NumSteps)
                throw new ArgumentException($"Step index must be in the interval [0, {NumSteps}].", nameof(stepIndex));

            int segmentStartIndex = stepIndex * NumSims;
            return new ReadOnlyMemory<double>(SpotPrices, segmentStartIndex, NumSims);
        }

        public ReadOnlyMemory<double> MarkovFactorsForPeriod(T period, int factorIndex)
        {
            if (!_periodIndices.TryGetValue(period, out int stepIndex))
                throw new ArgumentException($"No simulation for period {period}.", nameof(period));
            return MarkovFactorsForStepIndex(stepIndex, factorIndex);
        }

        public ReadOnlyMemory<double> MarkovFactorsForStepIndex(int stepIndex, int factorIndex)
        {
            if (stepIndex < 0 || stepIndex >= NumSteps)
                throw new ArgumentException($"Step index must be in the interval [0, {NumSteps}].", nameof(stepIndex));
            if (factorIndex < 0 || factorIndex >= NumFactors)
                throw new ArgumentException($"Factor index must be in the interval [0, {NumFactors}].", nameof(factorIndex));

            int segmentStartIndex = stepIndex * NumSims * NumFactors + factorIndex * NumSims;
            return new ReadOnlyMemory<double>(MarkovFactors, segmentStartIndex, NumSims);
        }

        // TODO methods for getting all simulated markov factors and spot prices for a particular sim number?
    }
}