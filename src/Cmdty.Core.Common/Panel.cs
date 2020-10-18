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
using System.Runtime.InteropServices;

namespace Cmdty.Core.Common
{
    [DebuggerDisplay("{" + nameof(NumRows) + "}x{" + nameof(NumCols) + "}")]
    public sealed class Panel<TIndex, TData>
    {
        public TData[] RawData { get; }
        private readonly Dictionary<TIndex, int> _indices;

        internal Panel(TData[] rawData, IEnumerable<TIndex> indices, int numCols)
        {
            if (indices == null) throw new ArgumentNullException(nameof(indices));
            if (numCols < 0)
                throw new ArgumentException("Number of columns must be non-negative.", nameof(numCols));
            NumCols = numCols;
            _indices = indices.Select((index, i) => (index, i)).ToDictionary(pair => pair.index, pair => pair.i * numCols);
            NumRows = _indices.Count;
            if (rawData.Length < NumRows * NumCols) // Raw Data can be larger than necessary to facilitate reusing memory, e.g. by using ArrayPool
                throw new ArgumentException("Raw Data array is not big enough.", nameof(rawData));
            RawData = rawData;
        }

        public Panel(IEnumerable<TIndex> rowIndices, int numCols)
        {
            if (rowIndices == null) throw new ArgumentNullException(nameof(rowIndices));
            if (numCols < 0)
                throw new ArgumentException("Number of columns must be non-negative.", nameof(numCols));
            NumCols = numCols;
            _indices = rowIndices.Select((index, i) => (index, i)).ToDictionary(pair => pair.index, pair => pair.i * numCols);
            NumRows = _indices.Count;
            RawData = new TData[NumRows * NumCols];
        }

        public int NumRows { get; }

        public int NumCols { get; }

        public IEnumerable<TIndex> RowIndices => _indices.Keys;

        public bool IsEmpty => RawData.Length == 0;

        public Span<TData> this[int rowIndex] 
        {
            get
            {
                if (rowIndex >= NumRows) // Necessary check as RawData might be bigger than needed
                    throw new ArgumentOutOfRangeException(nameof(rowIndex));
                return new Span<TData>(RawData, rowIndex * NumCols, NumCols);
            }
        }
        
        public Span<TData> this[TIndex rowIndex] => new Span<TData>(RawData, _indices[rowIndex], NumCols);

        public Memory<TData> GetRowMemory(int rowIndex)
        {
            if (rowIndex >= NumRows)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            return new Memory<TData>(RawData, rowIndex * NumCols, NumCols);
        }

        public Memory<TData> GetRowMemory(TIndex rowIndex) =>
            new Memory<TData>(RawData, _indices[rowIndex], NumCols);

        // Note that precondition checks aren't put in shared method as this can't be inlined due to throwing exception
        public TData this[int rowIndex, int colIndex]
        {
            get
            {
                if (rowIndex >= NumRows || rowIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(rowIndex));
                if (colIndex >= NumCols || colIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(colIndex));
                return RawData[rowIndex * NumCols + colIndex];
            }
            set
            {
                if (rowIndex >= NumRows || rowIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(rowIndex));
                if (colIndex >= NumCols || colIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(colIndex));
                RawData[rowIndex * NumCols + colIndex] = value;
            }
        } 

        public TData this[TIndex rowIndex, int colIndex]
        {
            get
            {
                if (colIndex >= NumCols || colIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(colIndex));
                return RawData[_indices[rowIndex] + colIndex];
            }
            set
            {
                if (colIndex >= NumCols || colIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(colIndex));
                RawData[_indices[rowIndex] + colIndex] = value;
            }
        }
        
        public static Panel<TIndex, TData> CreateEmpty(int numCols) => new Panel<TIndex, TData>(new TIndex[0], numCols);

        public static Panel<TIndex, TData> CreateEmpty() => new Panel<TIndex, TData>(new TIndex[0], 0);
        
    }

    public static class Panel
    {
        public static Panel<TIndex, TData> UseRawDataArray<TIndex, TData>
                (TData[] rawData, IEnumerable<TIndex> rowIndices, int numCols) => 
                            new Panel<TIndex, TData>(rawData, rowIndices, numCols);

        public static Panel<TIndex, TData> From2DArray<TIndex, TData>(TData[,] data, IEnumerable<TIndex> rowIndices)
        {
            int numCols = data.GetLength(1);
            var rawData = new TData[data.Length];
            // Not using Array.Copy or Buffer.BlockCopy as .NET multi-dimensional array is not necessarily in contiguous memory
            int idx = 0;
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    rawData[idx++] = data[i, j];
                }
            }

            return UseRawDataArray(rawData, rowIndices, numCols);
        }

    }

}
