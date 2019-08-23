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

        public OneFactorTrinomialTreeTest()
        {
            // TODO set up _forwardCurve and _volatilityCurve
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
            throw new NotImplementedException();
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