#region License
// Copyright (c) 2019 Jake Fowler
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
using Cmdty.TimePeriodValueTypes;
using Cmdty.TimeSeries;

namespace Cmdty.Core.Trees
{
    public static class OneFactorTrinomialTree
    {

        public static TimeSeries<T, IReadOnlyList<TreeNode>> CreateTree<T>(TimeSeries<T, double> forwardCurve, double lambda, 
                                                                    TimeSeries<T, double> volatility)
            where T : ITimePeriod<T>
        {
            // TODO check forwardCurve for null
            if (lambda <= 0) // TODO allow to be zero for non-mean reverting case?
                throw new ArgumentException("Mean reversion must be positive.", nameof(lambda));

            if (forwardCurve.Count < 2)
                throw new ArgumentException("Forward curve must contain at least 2 points.", nameof(forwardCurve));

            // TODO replace the two conditions below with call to new method on TimeSeries, e.g. indices are subset
            if (volatility.IsEmpty)
                throw new ArgumentException("Volatility curve is empty.", nameof(volatility));

            if (volatility.Start.CompareTo(forwardCurve.Start) > 0 || volatility.End.CompareTo(forwardCurve.End) < 0)
                throw new ArgumentException("Volatility curve does not contain a point for every point on the forward curve.", nameof(volatility));




            throw new NotImplementedException();
        }

    }
}
