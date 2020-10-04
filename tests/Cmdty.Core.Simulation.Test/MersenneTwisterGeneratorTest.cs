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

using NUnit.Framework;

namespace Cmdty.Core.Simulation.Test
{
    [TestFixture]
    public sealed class MersenneTwisterGeneratorTest
    {

        [Test]
        public void Generate_Antithetic_GeneratedMeanEqualsInputZero()
        {
            const int numSims = 26;
            const int randomArrayLen = 10;
            var randoms = new double[numSims][];
            var mtGen = new MersenneTwisterGenerator(true);

            for (int i = 0; i < numSims; i++)
            {
                randoms[i] = new double[randomArrayLen];
                mtGen.Generate(randoms[i]);
            }

            for (int i = 0; i < randomArrayLen; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < numSims; j++)
                {
                    sum += randoms[j][i];
                }
                double sampleMean = sum / numSims;
                Assert.AreEqual(0, sampleMean);
            }

        }
    }
}
