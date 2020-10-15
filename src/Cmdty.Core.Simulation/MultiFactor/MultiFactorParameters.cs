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
using TimeSeriesFactory = Cmdty.TimeSeries.TimeSeries;

namespace Cmdty.Core.Simulation.MultiFactor
{
    public sealed class MultiFactorParameters<T>
        where T : ITimePeriod<T>
    {
        public double[,] FactorCorrelations { get; } // TODO make immutable
        public IReadOnlyList<Factor<T>> Factors { get; }

        public MultiFactorParameters(double[,] factorCorrelations, params Factor<T>[] factors)
        {
            FactorCorrelations = factorCorrelations;
            Factors = factors.ToArray();
            if (factorCorrelations.GetLength(0) != factorCorrelations.GetLength(1))
                throw new ArgumentException($"Parameter {factorCorrelations} is not square.", nameof(factorCorrelations));
            if (Factors.Count != factorCorrelations.GetLength(0))
                throw new ArgumentException($"Parameters {factors} and {factorCorrelations} have inconsistent sizes.");
            if (Factors.Count == 0)
                throw new ArgumentException("Must provide at least one factor.", nameof(factors));

            // TODO full check of correlation, e.g. diagonals 1, within [-1, 1]. Positive semi-definite?
        }

        public int NumFactors => FactorCorrelations.GetLength(0);

    }

    public static class MultiFactorParameters
    {
        private const double DaysPerYear = 365.25;

        public static MultiFactorParameters<T> For1Factor<T>(double meanReversion, IReadOnlyDictionary<T, double> spotVolatility)
            where T : ITimePeriod<T>
        {
            return new MultiFactorParameters<T>(new double[,] { { 1.0 } }, new Factor<T>(meanReversion, spotVolatility));
        }

        public static MultiFactorParameters<T> For2Factors<T>(double factorCorr, Factor<T> factor1, Factor<T> factor2)
            where T : ITimePeriod<T>
        {
            var corrMatrix = new double[,] {{1.0, factorCorr}, {factorCorr, 1.0}};
            return new MultiFactorParameters<T>(corrMatrix, factor1, factor2);
        }

        /// <summary>
        /// Creates parameters to configure model similar to 3-factor presented in Boogert, A. and de Jong, C., 2011.
        /// Gas storage valuation using a multi-factor price process. The Journal of Energy Markets, 4(4), pp.29-52.
        /// </summary>
        public static MultiFactorParameters<T> For3FactorSeasonal<T>(double spotMeanReversion, double spotVol,
                                        double longTermVol, double seasonalVol, T start, T end)
            where T : ITimePeriod<T>
        {
            if (spotVol < 0)
                throw new ArgumentException("Spot vol must be non-negative.", nameof(spotVol));
            if (longTermVol < 0)
                throw new ArgumentException("Long-term vol must be non-negative.", nameof(longTermVol));
            if (seasonalVol < 0)
                throw new ArgumentException("Seasonal vol must be non-negative.", nameof(seasonalVol));

            // Factors are uncorrelated so use 3x3 identity matrix
            var factorCorrs = new double[3, 3];
            factorCorrs[0, 0] = 1.0;
            factorCorrs[1, 1] = 1.0;
            factorCorrs[2, 2] = 1.0;

            Day firstDayOfStart = start.First<Day>();
            var firstOfFeb = new DateTime(firstDayOfStart.Year, 2 /*feb*/, 1);
            T peakPeriod = TimePeriodFactory.FromDateTime<T>(firstOfFeb);
            DateTime peakPeriodStart = peakPeriod.Start;

            const double phase = Math.PI / 2.0;
            double amplitude = seasonalVol / 2.0;

            var seasonalVols = TimeSeriesFactory.FromMap(start, end, period =>
            {
                double yearsFromPeakPeriod = period.Start.Subtract(peakPeriodStart).TotalDays / DaysPerYear;
                return amplitude * Math.Sin(2.0 * Math.PI * yearsFromPeakPeriod + phase);
            });

            return new MultiFactorParameters<T>(factorCorrs, 
                Factor.ForConstVol(spotMeanReversion, start, end, spotVol),
                    Factor.ForConstVol(0.0, start, end, longTermVol),
                    new Factor<T>(0.0, seasonalVols));
        }

    }
}
