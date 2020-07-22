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

        public MultiFactorParameters(IEnumerable<Factor<T>> factors, double[,] factorCorrelations)
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

        public int NumFactors => FactorCorrelations.Length;

    }
}
