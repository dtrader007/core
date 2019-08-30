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
using JetBrains.Annotations;

namespace Cmdty.Core.Trees
{
    /// <summary>
    /// Class with method used to construct one-factor lognormal trinomial tree fitted to an initial forward curve.
    /// References:
    /// Hull J, and A White, 1994, “Numerical Procedures For Implementing Term Structure Models I: Single-Factor Models, The Journal of Derivatives, Fall, pp. 7-16.
    /// Clewlow L and Strickland C, "Valuing Energy Options in a One Factor Model Fitted to Forward Prices", April 1999, Available at SSRN: https://ssrn.com/abstract=160608 or http://dx.doi.org/10.2139/ssrn.160608
    /// </summary>
    public static class OneFactorTrinomialTree
    {

        public static TimeSeries<T, IReadOnlyList<TreeNode>> CreateTree<T>([NotNull] TimeSeries<T, double> forwardCurve, double meanReversion,
                                                [NotNull] TimeSeries<T, double> spotVolatilityCurve, double onePeriodTimeDelta)
            where T : ITimePeriod<T>
        {
            if (forwardCurve == null) throw new ArgumentNullException(nameof(forwardCurve));
            if (spotVolatilityCurve == null) throw new ArgumentNullException(nameof(spotVolatilityCurve));

            if (meanReversion <= 0)
                throw new ArgumentException("Mean reversion must be positive.", nameof(meanReversion));

            if (onePeriodTimeDelta <= 0)
                throw new ArgumentException("Time delta must be positive.", nameof(onePeriodTimeDelta));

            if (forwardCurve.Count < 2)
                throw new ArgumentException("Forward curve must contain at least 2 points.", nameof(forwardCurve));

            // TODO replace the two conditions below with call to new method on TimeSeries, e.g. indices are subset
            if (spotVolatilityCurve.IsEmpty)
                throw new ArgumentException("Volatility curve is empty.", nameof(spotVolatilityCurve));

            if (spotVolatilityCurve.Start.CompareTo(forwardCurve.Start) > 0 || spotVolatilityCurve.End.CompareTo(forwardCurve.End) < 0)
                throw new ArgumentException("Volatility curve does not contain a point for every point on the forward curve.", nameof(spotVolatilityCurve));

            int numPeriods = forwardCurve.Count;

            double expectedOuReturn = Math.Exp(-meanReversion * onePeriodTimeDelta) - 1;    // Equal to M in Hull & White 1994, as calculated in end note 5, p16
            double onePeriodOuVariance = (1 - Math.Exp(-2 * meanReversion * onePeriodTimeDelta)) / (2.0 * meanReversion);
            double treeSpacing = Math.Sqrt(3 * onePeriodOuVariance);

            // For jMax calc see Hull & White p12
            int jMax = Convert.ToInt32(Math.Ceiling(-0.184 / expectedOuReturn));

            int maxNumTreeLevels = jMax * 2 + 1;
            
            var bottomEdgeTransitionProbabilities = new Dictionary<int, TransitionProbabilities>();
            var middleNodeTypeTransitionProbabilities = new Dictionary<int, TransitionProbabilities>();
            var topEdgeTransitionProbabilities = new Dictionary<int, TransitionProbabilities>();
            
            // Calculate the probability of reaching each node using forward induction
            var nodeProbabilities = new double[numPeriods][];

            nodeProbabilities[0] = new[] {1.0}; // Current point as probability 1

            for (int i = 0; i < numPeriods - 1; i++) // Loop forward through time
            {
                int currentStepNumLevels = Math.Min(i * 2 + 1, maxNumTreeLevels);
                int indexAdjustToJ = (currentStepNumLevels - 1) / 2;

                int nextStepNumLevels = Math.Min((i + 1) * 2 + 1, maxNumTreeLevels);
                
                nodeProbabilities[i + 1] = new double[nextStepNumLevels];
                bool treeHasReachedWidestPoint = currentStepNumLevels == maxNumTreeLevels;

                for (int priceLevelIndex = 0; priceLevelIndex < currentStepNumLevels; priceLevelIndex++) // Loop through the tree price levels
                {
                    double currentNodeProbability = nodeProbabilities[i][priceLevelIndex];
                    (int nextStepTopIndex, int nextStepMiddleIndex, int nextStepBottomIndex) = 
                                GetNextStepIndexPositions(priceLevelIndex, treeHasReachedWidestPoint, maxNumTreeLevels);

                    int j = priceLevelIndex - indexAdjustToJ;

                    TransitionProbabilities transitionProbabilities = GetTransitionProbabilities(j, priceLevelIndex, 
                        maxNumTreeLevels, treeHasReachedWidestPoint, expectedOuReturn, 
                        bottomEdgeTransitionProbabilities, middleNodeTypeTransitionProbabilities, topEdgeTransitionProbabilities);

                    nodeProbabilities[i + 1][nextStepTopIndex] += currentNodeProbability * transitionProbabilities.TopProbability;
                    nodeProbabilities[i + 1][nextStepMiddleIndex] += currentNodeProbability * transitionProbabilities.MiddleProbability;
                    nodeProbabilities[i + 1][nextStepBottomIndex] += currentNodeProbability * transitionProbabilities.BottomProbability;
                }

            }

            // Calculate adjustment factor to ensure expected spot price equals current forward price
            // See Clewlow and Strickland equation 4.10 with discount factor cancelled out
            var adjustmentTerms = new double[numPeriods];
            for (int i = 0; i < numPeriods; i++)
            {
                int numPriceLevels = nodeProbabilities[i].Length;
                int indexAdjustToJ = (numPriceLevels - 1) / 2;

                double expectedExponentialOfOu = 0;
                double spotVolatility = spotVolatilityCurve[i];
                for (int priceLevelIndex = 0; priceLevelIndex < numPriceLevels; priceLevelIndex++)
                {
                    int j = priceLevelIndex - indexAdjustToJ;
                    double ouProcessValue = treeSpacing * j;
                    expectedExponentialOfOu += nodeProbabilities[i][priceLevelIndex] * Math.Exp(spotVolatility * ouProcessValue);
                }
                double forwardPrice = forwardCurve[i];
                adjustmentTerms[i] = Math.Log(forwardPrice / expectedExponentialOfOu);
            }

            // Populate results looping backward from end in order to populate transitions
            var resultNodes = new TreeNode[numPeriods][];

            // Populate nodes at all other time steps
            for (int i = numPeriods - 1; i >= 0; i--) // Loop back through time periods
            {
                int numPriceLevels = nodeProbabilities[i].Length;
                int indexAdjustToJ = (numPriceLevels - 1) / 2;

                double spotVolatility = spotVolatilityCurve[i];
                resultNodes[i] = new TreeNode[numPriceLevels];
                bool treeHasReachedWidestPoint = numPriceLevels == maxNumTreeLevels;
                for (int priceLevelIndex = 0; priceLevelIndex < numPriceLevels; priceLevelIndex++) // Loop through price levels
                {
                    NodeTransition[] nodeTransitions;
                    int j = priceLevelIndex - indexAdjustToJ;
                    if (i == numPeriods - 1)
                    {
                        nodeTransitions = new NodeTransition[0];
                    }
                    else
                    {
                        TransitionProbabilities transitionProbabilities = GetTransitionProbabilities(j, priceLevelIndex, 
                                    maxNumTreeLevels, treeHasReachedWidestPoint, expectedOuReturn, 
                                    bottomEdgeTransitionProbabilities, middleNodeTypeTransitionProbabilities, 
                                    topEdgeTransitionProbabilities);

                        (int nextPeriodTopNodeIndex, int nextPeriodMiddleNodeIndex, int nextPeriodBottomNodeIndex) =
                            GetNextStepIndexPositions(priceLevelIndex, treeHasReachedWidestPoint, maxNumTreeLevels);

                        var topTransition = new NodeTransition(transitionProbabilities.TopProbability,
                                                    resultNodes[i + 1][nextPeriodTopNodeIndex]);
                        var middleTransition = new NodeTransition(transitionProbabilities.MiddleProbability,
                                                    resultNodes[i + 1][nextPeriodMiddleNodeIndex]);
                        var bottomTransition = new NodeTransition(transitionProbabilities.BottomProbability,
                                                    resultNodes[i + 1][nextPeriodBottomNodeIndex]);
                        nodeTransitions = new[] { bottomTransition, middleTransition, topTransition };
                    }
                    double ouProcessValue = treeSpacing * j;
                    double nodeSpotPrice = Math.Exp(ouProcessValue * spotVolatility + adjustmentTerms[i]);

                    resultNodes[i][priceLevelIndex] = new TreeNode(nodeSpotPrice, nodeProbabilities[i][priceLevelIndex], priceLevelIndex, nodeTransitions);
                }

            }
            
            return new TimeSeries<T, IReadOnlyList<TreeNode>>(forwardCurve.Indices, resultNodes);
        }

        private static (int topIndex, int middleIndex, int bottomIndex) GetNextStepIndexPositions(int currentStepIndex,
                                    bool treeHasReachedWidestPoint, int maxNumTreeLevels)
        {
            if (treeHasReachedWidestPoint && currentStepIndex == 0)
            {
                return (topIndex: 2, middleIndex: 1, bottomIndex: 0);
            }

            if (treeHasReachedWidestPoint && currentStepIndex == maxNumTreeLevels - 1)
            {
                return (topIndex: maxNumTreeLevels - 1, middleIndex: maxNumTreeLevels - 2, bottomIndex: maxNumTreeLevels - 3);
            }

            if (treeHasReachedWidestPoint)
            {
                return (topIndex: currentStepIndex + 1, middleIndex: currentStepIndex, bottomIndex: currentStepIndex - 1);
            }

            return (topIndex: currentStepIndex + 2, middleIndex: currentStepIndex + 1, bottomIndex: currentStepIndex);
        }

        private static TransitionProbabilities GetTransitionProbabilities(
            int j,
            int arrayIndex,
            int maxNumTreePriceLevels,
            bool treeHasReachedWidestPoint,
            double expectedOuReturn,
            Dictionary<int, TransitionProbabilities> bottomEdgeTransitionProbabilities,
            Dictionary<int, TransitionProbabilities> middleNodeTypeTransitionProbabilities,
            Dictionary<int, TransitionProbabilities> topEdgeTransitionProbabilities)
        {
            TransitionProbabilities transitionProbabilities;
            if (treeHasReachedWidestPoint && arrayIndex == 0)
            {
                // Bottom node
                if (bottomEdgeTransitionProbabilities.TryGetValue(j, out transitionProbabilities))
                    return transitionProbabilities;

                double jSquaredTimesMSquared = j * j * expectedOuReturn * expectedOuReturn;
                double jTimesM = j * expectedOuReturn;

                double probabilityUp = 1.0 / 6.0 + (jSquaredTimesMSquared - jTimesM) / 2.0;
                double probabilityMiddle = -1.0 / 3.0 - jSquaredTimesMSquared + 2 * jTimesM;
                double probabilityDown = 7.0 / 6.0 + (jSquaredTimesMSquared - 3.0 * jTimesM) / 2.0;

                transitionProbabilities = new TransitionProbabilities(probabilityUp, probabilityMiddle, probabilityDown);
                bottomEdgeTransitionProbabilities.Add(j, transitionProbabilities);
            }
            else if (treeHasReachedWidestPoint && arrayIndex == maxNumTreePriceLevels - 1)
            {
                // Top node
                if (topEdgeTransitionProbabilities.TryGetValue(j, out transitionProbabilities))
                    return transitionProbabilities;

                double jSquaredTimesMSquared = j * j * expectedOuReturn * expectedOuReturn;
                double jTimesM = j * expectedOuReturn;

                double probabilityUp = 7.0 / 6.0 + (jSquaredTimesMSquared + 3.0 * jTimesM) / 2.0;
                double probabilityMiddle = -1.0 / 3.0 - jSquaredTimesMSquared - 2.0 * jTimesM;
                double probabilityDown = 1.0 / 6.0 + (jSquaredTimesMSquared + jTimesM) / 2.0;

                transitionProbabilities = new TransitionProbabilities(probabilityUp, probabilityMiddle, probabilityDown);
                topEdgeTransitionProbabilities.Add(j, transitionProbabilities);
            }
            else
            {
                // Central node
                if (middleNodeTypeTransitionProbabilities.TryGetValue(j, out transitionProbabilities))
                    return transitionProbabilities;

                double jSquaredTimesMSquared = j * j * expectedOuReturn * expectedOuReturn;
                double jTimesM = j * expectedOuReturn;

                double probabilityUp = 1.0 / 6.0 + (jSquaredTimesMSquared + jTimesM) / 2.0;
                double probabilityMiddle = 2.0 / 3.0 - jSquaredTimesMSquared;
                double probabilityDown = 1.0 / 6.0 + (jSquaredTimesMSquared - jTimesM) / 2.0;

                transitionProbabilities = new TransitionProbabilities(probabilityUp, probabilityMiddle, probabilityDown);
                middleNodeTypeTransitionProbabilities.Add(j, transitionProbabilities);
            }
            return transitionProbabilities;

        }

        private class TransitionProbabilities
        {
            public double TopProbability { get; }
            public double MiddleProbability { get; }
            public double BottomProbability { get; }

            public TransitionProbabilities(double topProbability, double middleProbability, double bottomProbability)
            {
                TopProbability = topProbability;
                MiddleProbability = middleProbability;
                BottomProbability = bottomProbability;
            }

            public override string ToString()
            {
                return $"{nameof(TopProbability)}: {TopProbability}, {nameof(MiddleProbability)}: {MiddleProbability}, {nameof(BottomProbability)}: {BottomProbability}";
            }

        }

    }
}
