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
    }
}
