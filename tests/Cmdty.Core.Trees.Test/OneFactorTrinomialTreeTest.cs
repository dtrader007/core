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
using System.Linq;
using Cmdty.TimePeriodValueTypes;
using Cmdty.TimeSeries;
using NUnit.Framework;

namespace Cmdty.Core.Trees.Test
{
    [TestFixture]
    public sealed class OneFactorTrinomialTreeTest
    {
        private readonly TimeSeries<Day, double> _forwardCurve;
        private readonly TimeSeries<Day, double> _volatilityCurve;
        private const double MeanReversion = 0.32;
        private const double TimeDelta = 1.0/365;

        public OneFactorTrinomialTreeTest()
        {
            _forwardCurve = new TimeSeries<Day, double>.Builder
                        {
                            {new Day(2019, 8, 23),  54.76},
                            {new Day(2019, 8, 24),  45.24},
                            {new Day(2019, 8, 25),  44.03},
                            {new Day(2019, 8, 26),  55.64},
                            {new Day(2019, 8, 27),  56.36},
                            {new Day(2019, 8, 28),  57.35},
                            {new Day(2019, 8, 29),  56.01},
                            {new Day(2019, 8, 30),  54.73},
                            {new Day(2019, 8, 31),  41.05},
                            {new Day(2019, 9, 1),  40.25},
                            {new Day(2019, 9, 2),  53.25},
                            {new Day(2019, 9, 3),  53.73},
                            {new Day(2019, 9, 4),  52.26},
                            {new Day(2019, 9, 5),  52.95},
                            {new Day(2019, 9, 6),  51.22},
                            {new Day(2019, 9, 7),  46.86},
                            {new Day(2019, 9, 8),  45.99},
                        }.Build();

            _volatilityCurve = new TimeSeries<Day, double>.Builder
                        {
                            {new Day(2019, 8, 23),  0.86},
                            {new Day(2019, 8, 24),  0.67},
                            {new Day(2019, 8, 25),  0.675},
                            {new Day(2019, 8, 26),  0.84},
                            {new Day(2019, 8, 27),  0.845},
                            {new Day(2019, 8, 28),  0.843},
                            {new Day(2019, 8, 29),  0.834},
                            {new Day(2019, 8, 30),  0.8125},
                            {new Day(2019, 8, 31),  0.522},
                            {new Day(2019, 9, 1),  0.499},
                            {new Day(2019, 9, 2),  0.734},
                            {new Day(2019, 9, 3),  0.73},
                            {new Day(2019, 9, 4),  0.723},
                            {new Day(2019, 9, 5),  0.793},
                            {new Day(2019, 9, 6),  0.74},
                            {new Day(2019, 9, 7),  0.406},
                            {new Day(2019, 9, 8),  0.394},
                        }.Build();
                    }

        [Test]
        [Ignore("Tree hasn't been fully implemented yet")]
        public void CreateTree_ExpectedSpotPriceEqualsForwardPrice()
        {
            TimeSeries<Day, IReadOnlyList<TreeNode>> tree = CreateTestTree();

            foreach ((Day day, IReadOnlyList<TreeNode> nodes) in tree)
            {
                double treeExpectedSpotPrice = nodes.Sum(node => node.Value * node.Probability);
                double forwardPrice = _forwardCurve[day];
                Assert.AreEqual(forwardPrice, treeExpectedSpotPrice);
            }

        }

        [Test]
        public void CreateTree_MeanReversionNegative_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                OneFactorTrinomialTree.CreateTree(_forwardCurve, -0.1, _volatilityCurve, 1.0/365));
        }


        private TimeSeries<Day, IReadOnlyList<TreeNode>> CreateTestTree()
        {
            return OneFactorTrinomialTree.CreateTree(_forwardCurve, MeanReversion, _volatilityCurve, TimeDelta);
        }

        [Test]
        [Ignore("Tree hasn't been fully implemented yet")]
        public void CreateTree_AllNodeProbabilitiesArePositive()
        {
            TimeSeries<Day, IReadOnlyList<TreeNode>> tree = CreateTestTree();

            foreach (IReadOnlyList<TreeNode> treeNodes in tree.Data)
            {
                foreach (var treeNode in treeNodes)
                {
                    Assert.Greater(0.0, treeNode.Probability);
                }
            }
        }

        [Test]
        [Ignore("Tree hasn't been fully implemented yet")]
        public void CreateTree_SumOfNodeTransitionProbabilitiesEqualsOne()
        {
            TimeSeries<Day, IReadOnlyList<TreeNode>> tree = CreateTestTree();

            foreach (IReadOnlyList<TreeNode> treeNodes in tree.Data)
            {
                foreach (var treeNode in treeNodes)
                {
                    double sumTransitionProbabilities = treeNode.Transitions.Sum(transition => transition.Probability);
                    Assert.AreEqual(1.0, sumTransitionProbabilities);
                }
            }
        }

        [Test]
        [Ignore("Tree hasn't been fully implemented yet")]
        public void CreateTree_SumOfNodeProbabilitiesEqualsOne()
        {
            TimeSeries<Day, IReadOnlyList<TreeNode>> tree = CreateTestTree();

            foreach (IReadOnlyList<TreeNode> treeNodes in tree.Data)
            {
                double sumNodeProbabilities = treeNodes.Sum(node => node.Probability);
                Assert.AreEqual(1.0, sumNodeProbabilities);
            }
        }



    }
}