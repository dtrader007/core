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
using NUnit.Framework;

namespace Cmdty.Core.Common.Test
{
    [TestFixture]
    public sealed class PanelTest
    {
        [Test]
        public void CreateEmptyIntParameter_CreatesEmptyPanelWithNumColsEqualToIntParameter()
        {
            const int numCols = 4;
            Panel<string, int> panel = Panel<string, int>.CreateEmpty(numCols);

            Assert.IsTrue(panel.IsEmpty);
            Assert.AreEqual(numCols, panel.NumCols);
            Assert.AreEqual(0, panel.NumRows);
        }

        [Test]
        public void CreateEmpty_CreatesEmptyPanelWithZeroCols()
        {
            Panel<string, int> panel = Panel<string, int>.CreateEmpty();

            Assert.IsTrue(panel.IsEmpty);
            Assert.AreEqual(0, panel.NumCols);
            Assert.AreEqual(0, panel.NumRows);
        }

        private static Panel<string, int> CreateTestPanel()
        {
            var rowIndices = new[] {"row-one", "row-two", "row-three"};
            var panel = new Panel<string, int>(rowIndices, 2);

            panel[0][0] = 1;
            panel[0][1] = 2;
            panel[1][0] = 3;
            panel[1][1] = 4;
            panel[2][0] = 5;
            panel[2][1] = 6;

            return panel;
        }

        [Test]
        public void IndexerOneIntParameter_AsExpected()
        {
            Panel<string, int> panel = CreateTestPanel();
            Span<int> row = panel[1];
            Assert.AreEqual(3, row[0]);
            Assert.AreEqual(4, row[1]);
            Assert.AreEqual(2, row.Length);
        }

        [Test]
        public void IndexerOneIntParameter_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            Panel<string, int> panel = CreateTestPanel();
            try
            {
                // ReSharper disable once UnusedVariable
                Span<int> row = panel[3];
                Assert.Fail("IndexOutOfRangeException exception not thrown.");
            }
            catch (ArgumentOutOfRangeException) { }
        }

        [Test]
        public void GetRowMemoryIntParameter_IndexOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            Panel<string, int> panel = CreateTestPanel();
            Assert.Throws<ArgumentOutOfRangeException>(() => panel.GetRowMemory(-1));
        }

        [Test]
        public void IndexerOneRowIndexTypeParameter_AsExpected()
        {
            Panel<string, int> panel = CreateTestPanel();
            Span<int> row = panel["row-one"];
            Assert.AreEqual(1, row[0]);
            Assert.AreEqual(2, row[1]);
            Assert.AreEqual(2, row.Length);
        }

        [Test]
        public void IndexerOneRowIndexTypeParameter_IndexOutOfRange_ThrowsKeyNotFoundExceptionException()
        {
            Panel<string, int> panel = CreateTestPanel();
            try
            {
                // ReSharper disable once UnusedVariable
                Span<int> row = panel["not_key"];
                Assert.Fail("IndexOutOfRangeException exception not thrown.");
            }
            catch (KeyNotFoundException) { }
        }

        [Test]
        public void GetRowMemoryRowIndexTypeParameter_IndexOutOfRange_ThrowsKeyNotFoundException()
        {
            Panel<string, int> panel = CreateTestPanel();
            Assert.Throws<KeyNotFoundException>(() => panel.GetRowMemory("not_key"));
        }

        [Test]
        public void IndexerGetTwoIntParameters_AsExpected()
        {
            Panel<string, int> panel = CreateTestPanel();
            Assert.AreEqual(6, panel[2, 1]);
        }

        [Test]
        public void IndexerGetTwoIntParameters_IndicesOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            Panel<string, int> panel = CreateTestPanel();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var x = panel[-1, 1];
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var x = panel[3, 1];
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var x = panel[0, -1];
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var x = panel[0, 4];
            });
        }

        [Test]
        public void IndexerSetTwoIntParameters_UpdatesPanel()
        {
            Panel<string, int> panel = CreateTestPanel();
            const int updatedValue = 55;
            panel[2, 1] = updatedValue;
            Assert.AreEqual(updatedValue, panel[2, 1]);
        }

        [Test]
        public void IndexerSetTwoIntParameters_IndicesOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            Panel<string, int> panel = CreateTestPanel();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                panel[-1, 1] = 1;
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                panel[3, 1] = 1;
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                panel[0, -1] = 1;
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                panel[0, 4] = 1;
            });
        }

        [Test]
        public void IndexerGetRowIndexTypeAndIntParameters_AsExpected()
        {
            Panel<string, int> panel = CreateTestPanel();
            Assert.AreEqual(6, panel["row-three", 1]);
        }

        [Test]
        public void IndexerGetRowIndexTypeAndIntParameters_IndicesOutOfRange_Throws()
        {
            Panel<string, int> panel = CreateTestPanel();
            Assert.Throws<KeyNotFoundException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var x = panel["x", 1];
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var x = panel["row-one", -1];
            });
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var x = panel["row-one", 5];
            });
        }

        [Test]
        public void IndexerGetRowIndexTypeAndIntParameters_UpdatesPanel()
        {
            Panel<string, int> panel = CreateTestPanel();
            const int updatedValue = 55;
            panel["row-three", 1] = updatedValue;
            Assert.AreEqual(updatedValue, panel["row-three", 1]);
        }

        [Test]
        public void UseRawDataArray_AsExpected()
        {
            var rowIndices = new[] { "row-one", "row-two", "row-three" };
            var rawData = new[] {1, 2, 3, 4, 5, 6, 7}; // Last item will not be used
            const int numCols = 2;

            Panel<string, int> panel = Panel.UseRawDataArray(rawData, rowIndices, numCols);

            Assert.AreEqual(numCols, panel.NumCols);
            Assert.AreEqual(rowIndices.Length, panel.NumRows);
            Assert.AreEqual(rawData, panel.RawData);
            CollectionAssert.AreEqual(rowIndices, panel.RowIndices);
        }

        [Test]
        public void From2DArray_AsExpected()
        {
            var rowIndices = new[] { "row-one", "row-two", "row-three" };
            var data = new [,]
            {
                {1, 2},
                {3, 4},
                {5, 6}
            };
            Panel<string, int> panel = Panel.From2DArray(data, rowIndices);

            CollectionAssert.AreEqual(rowIndices, panel.RowIndices);
            CollectionAssert.AreEqual(new []{1, 2}, panel[0].ToArray());
            CollectionAssert.AreEqual(new []{3, 4}, panel[1].ToArray());
            CollectionAssert.AreEqual(new []{5, 6}, panel[2].ToArray());
        }

    }
}