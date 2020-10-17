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
using System.Diagnostics;
using System.Linq;

namespace Cmdty.Core.Common
{
    [DebuggerDisplay("{" + nameof(NumRows) + "}x{" + nameof(NumCols) + "}")]
    public sealed class Panel<TIndex, TData>
    {
        public TData[] RawData { get; }
        private readonly Dictionary<TIndex, int> _indices;

        public Panel(IEnumerable<TIndex> indices, int numCols)
        {
            if (indices == null) throw new ArgumentNullException(nameof(indices));
            if (numCols < 0)
                throw new ArgumentException("Number of columns must be non-negative.", nameof(numCols));
            NumCols = numCols;
            _indices = indices.Select((index, i) => (index, i)).ToDictionary(pair => pair.index, pair => pair.i * numCols);
            NumRows = _indices.Count;
            RawData = new TData[NumRows * NumCols];
        }

        public int NumRows { get; }

        public int NumCols { get; }

        public IEnumerable<TIndex> RowIndices => _indices.Keys;

        public bool IsEmpty => RawData.Length == 0;

        public Span<TData> this[int rowIndex] => new Span<TData>(RawData, rowIndex * NumCols, NumCols);

        public Span<TData> this[TIndex rowIndex] => new Span<TData>(RawData, _indices[rowIndex], NumCols);

        public TData this[int rowIndex, int colIndex] => RawData[rowIndex * NumCols + colIndex];

        public TData this[TIndex rowIndex, int colIndex] => RawData[_indices[rowIndex] + colIndex];
        
        public static Panel<TIndex, TData> CreateEmpty(int numCols) => new Panel<TIndex, TData>(new TIndex[0], numCols);

        public static Panel<TIndex, TData> CreateEmpty() => new Panel<TIndex, TData>(new TIndex[0], 0);

    }
}
