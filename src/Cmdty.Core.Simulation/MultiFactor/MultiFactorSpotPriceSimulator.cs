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
using System.Text;
using System.Threading.Tasks;
using Cmdty.TimePeriodValueTypes;
using Cmdty.TimeSeries;
using JetBrains.Annotations;
using MathNet.Numerics.LinearAlgebra;

namespace Cmdty.Core.Simulation.MultiFactor
{
    public sealed class MultiFactorSpotPriceSimulator<T>
        where T : ITimePeriod<T>
    {
        private readonly int numSteps; // TODO get rid of
        private readonly int numFactors; // TODO get rid of
        private readonly double[] _forwardPriceRatios;
        private readonly double[] _spotDriftAdjustments;
        private readonly double[,] _reversionMultipliers;
        private readonly double[,] _spotVols;
        private readonly Matrix<double>[] _factorCovarianceSquareRoots;

        public MultiFactorSpotPriceSimulator([NotNull] MultiFactorParameters<T> modelParameters, DateTime currentDateTime,
            [NotNull] TimeSeries<T, double> forwardCurve, [NotNull] IEnumerable<T> simulatedPeriods) // TODO pass in random number generator factory
        {
            if (modelParameters == null) throw new ArgumentNullException(nameof(modelParameters));
            if (forwardCurve == null) throw new ArgumentNullException(nameof(forwardCurve));
            if (simulatedPeriods == null) throw new ArgumentNullException(nameof(simulatedPeriods));

            T[] simulatedPeriodsArray = simulatedPeriods.ToArray();

            int numPeriods = simulatedPeriodsArray.Length;
            int numFactors = modelParameters.NumFactors;

            if (numPeriods == 0)
                throw new ArgumentException(nameof(simulatedPeriods) + " argument cannot be empty.", nameof(simulatedPeriods));
            
            _forwardPriceRatios = new double[numPeriods];
            _spotDriftAdjustments = new double[numPeriods];
            _reversionMultipliers = new double[numPeriods, numFactors];
            _spotVols = new double[numPeriods, numFactors];
            _factorCovarianceSquareRoots = new Matrix<double>[numPeriods];

            for (int i = 0; i < simulatedPeriodsArray.Length; i++)
            {
                T simulatedPeriod = simulatedPeriodsArray[i];
                if (simulatedPeriod.Start <= currentDateTime) // TODO make comparison using day count function?
                    throw new ArgumentException($"All elements of {simulatedPeriods} must start after {currentDateTime}.", nameof(simulatedPeriods));
                if (i > 0)
                {
                    T lastPeriod = simulatedPeriodsArray[i - 1];
                    if (simulatedPeriod.CompareTo(lastPeriod) < 0)
                        throw new ArgumentException(nameof(simulatedPeriods) + " argument must be sorted in ascending order.", nameof(simulatedPeriods));
                    if (simulatedPeriod.Equals(lastPeriod))
                        throw new ArgumentException(nameof(simulatedPeriods) + $" argument cannot contain duplicated elements. More than one element with value {simulatedPeriod} found.", nameof(simulatedPeriods));
                }

                for (int j = 0; j < numFactors; j++)
                {
                    Factor<T> factor = modelParameters.Factors[j];
                    _spotVols[i, j] = factor.Volatility[simulatedPeriod];

                }

            }

        }

        public MultiFactorSpotSimResults Simulate(int numSims)
        {
            var spotPrices = new double[numSteps, numSims];
            double[,,] markovFactors = new double[numSteps, numSims, numFactors];

            throw new NotImplementedException();

            return new MultiFactorSpotSimResults(spotPrices, markovFactors);
        }

    }
}
