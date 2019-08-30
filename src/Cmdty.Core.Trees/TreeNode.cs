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

using System.Collections.Generic;

namespace Cmdty.Core.Trees
{
    /// <summary>
    /// Represents a node on a recombining tree.
    /// </summary>
    public sealed class TreeNode
    {
        public double Value { get; }
        public double Probability { get; }
        public int ValueLevelIndex { get; }
        public IReadOnlyList<NodeTransition> Transitions { get; }

        public TreeNode(double value, double probability, int valueLevelIndex, IReadOnlyList<NodeTransition> transitions)
        {
            Value = value;
            Probability = probability;
            ValueLevelIndex = valueLevelIndex;
            Transitions = transitions;
        }

        public bool IsTerminalNode => Transitions.Count == 0;

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}, {nameof(Probability)}: {Probability}, {nameof(ValueLevelIndex)}: {ValueLevelIndex}, {nameof(Transitions)}.Count: {Transitions.Count}";
        }

    }
}