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
                                                                    TimeSeries<T, double> volatility, double onePeriodTimeDelta)
            where T : ITimePeriod<T>
        {
            // TODO check forwardCurve for null
            if (lambda <= 0) // TODO allow to be zero for non-mean reverting case?
                throw new ArgumentException("Mean reversion must be positive.", nameof(lambda));

            if (onePeriodTimeDelta <= 0)
                throw new ArgumentException("Time delta must be positive.", nameof(onePeriodTimeDelta));

            if (forwardCurve.Count < 2)
                throw new ArgumentException("Forward curve must contain at least 2 points.", nameof(forwardCurve));

            // TODO replace the two conditions below with call to new method on TimeSeries, e.g. indices are subset
            if (volatility.IsEmpty)
                throw new ArgumentException("Volatility curve is empty.", nameof(volatility));

            if (volatility.Start.CompareTo(forwardCurve.Start) > 0 || volatility.End.CompareTo(forwardCurve.End) < 0)
                throw new ArgumentException("Volatility curve does not contain a point for every point on the forward curve.", nameof(volatility));

            int numPeriods = forwardCurve.Count;

            int maxNumTreeLevels = 0; // TODO calc = jmax * 2 + 1


            // Calculate node prices using forward induction
            var nodePrices = new double[numPeriods][];
            var nodeProbabilities = new double[numPeriods][];
            var transitionProbabilities = new double[numPeriods - 1][][];



            // Populate results
            var resultNodes = new TreeNode[numPeriods][];

            // Populate nodes at end, with no forward transitions
            int lastPeriodIndex = numPeriods - 1;
            int numLevelsAtEnd = nodePrices[lastPeriodIndex].Length;
            resultNodes[lastPeriodIndex] = new TreeNode[numLevelsAtEnd];
            for (int j = 0; j < numLevelsAtEnd; j++) // Loop through price levels
            {
                resultNodes[lastPeriodIndex][j] = new TreeNode(nodePrices[lastPeriodIndex][j], 
                        nodeProbabilities[lastPeriodIndex][j], new NodeTransition[0]);
            }

            // Populate nodes at all other time steps
            for (int i = numPeriods - 2; i >= 0; i--) // Loop back through time periods
            {
                bool treeHasReachedWidestPoint = nodePrices[i].Length == maxNumTreeLevels;
                for (int j = 0; j < nodePrices[i].Length; j++) // Loop through price levels
                {
                    int nextPeriodTopNodeIndex;
                    int nextPeriodMiddleNodeIndex;
                    int nextPeriodBottomNodeIndex;

                    if (treeHasReachedWidestPoint && j == 0)
                    {
                        nextPeriodTopNodeIndex = 2;
                        nextPeriodMiddleNodeIndex = 1;
                        nextPeriodBottomNodeIndex = 0;
                    }
                    else if (treeHasReachedWidestPoint && j == maxNumTreeLevels - 1)
                    {
                        nextPeriodTopNodeIndex = maxNumTreeLevels - 1;
                        nextPeriodMiddleNodeIndex = maxNumTreeLevels - 2;
                        nextPeriodBottomNodeIndex = maxNumTreeLevels - 3;
                    }
                    else
                    {
                        nextPeriodTopNodeIndex = j + 2;
                        nextPeriodMiddleNodeIndex = j + 1;
                        nextPeriodBottomNodeIndex = j;
                    }
                    var topTransition = new NodeTransition(transitionProbabilities[i][j][2], resultNodes[i + 1][nextPeriodTopNodeIndex]);
                    var middleTransition = new NodeTransition(transitionProbabilities[i][j][1], resultNodes[i + 1][nextPeriodMiddleNodeIndex]);
                    var bottomTransition = new NodeTransition(transitionProbabilities[i][j][0], resultNodes[i + 1][nextPeriodBottomNodeIndex]);
                    var nodeTransitions = new NodeTransition[] {bottomTransition, middleTransition, topTransition};

                    resultNodes[i][j] = new TreeNode(nodePrices[i][j], nodeProbabilities[i][j], nodeTransitions);
                }

            }
            
            return new TimeSeries<T, IReadOnlyList<TreeNode>>(forwardCurve.Indices, resultNodes);
        }

    }
}
