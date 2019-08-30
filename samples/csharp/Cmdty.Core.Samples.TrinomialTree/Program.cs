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
using System.Globalization;
using System.Linq;
using Cmdty.Core.Trees;
using Cmdty.TimePeriodValueTypes;
using Cmdty.TimeSeries;

namespace Cmdty.Core.Samples.TrinomialTree
{
    class Program
    {
        static void Main(string[] args)
        {
            const double meanReversion = 16.0;
            const double timeDelta = 1.0 / 365.0;

            (TimeSeries<Day, double> forwardCurve, TimeSeries<Day, double> spotVolatility) =
                CreateForwardAndVolatilityCurves();

            TimeSeries<Day, IReadOnlyList<TreeNode>> trinomialTree = OneFactorTrinomialTree.CreateTree(forwardCurve, meanReversion, spotVolatility, timeDelta);

            Console.WriteLine($"Created a trinomial tree with {trinomialTree.Count} items, starting at {trinomialTree.Start}, ending at {trinomialTree.End}.");

            Console.WriteLine();

            Console.WriteLine("Example of iterating through the tree by date");
            Console.WriteLine();
            Console.WriteLine("Date        Price");

            int maxTreeWidth = trinomialTree[trinomialTree.Count - 1].Count;

            foreach ((Day day, IReadOnlyList<TreeNode> treeNodes) in trinomialTree)
            {
                int numNodes = treeNodes.Count;
                int paddingSize = (maxTreeWidth - numNodes) * 6 / 2;
                string padding = new String(' ', paddingSize);
                Console.WriteLine($"{day}: {padding}" + string.Join(" ", treeNodes.Select(node => node.Value.ToString("F2", CultureInfo.InvariantCulture))));
            }

            Console.WriteLine();
            Console.WriteLine("Example of traversing tree using transition links");
            Console.WriteLine();

            TreeNode treeNode = trinomialTree[0][0];
            var random = new Random(11);

            do
            {
                int nextMove = random.Next(0, 3);
                var transition = treeNode.Transitions[nextMove];
                treeNode = transition.DestinationNode;
                Console.WriteLine(treeNode);
            } while (!treeNode.IsTerminalNode);

            Console.ReadKey();
        }

        private static (TimeSeries<Day, double> forwardCurve, TimeSeries<Day, double> spotVolatility)
            CreateForwardAndVolatilityCurves()
        {
            TimeSeries<Day, double> forwardCurve = new TimeSeries<Day, double>.Builder
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

            TimeSeries<Day, double> spotVolatility = new TimeSeries<Day, double>.Builder
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
            return (forwardCurve, spotVolatility);
        }

    }
}
