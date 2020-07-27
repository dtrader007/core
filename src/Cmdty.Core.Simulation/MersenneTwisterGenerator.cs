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

using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;

namespace Cmdty.Core.Simulation
{
    public sealed class MersenneTwisterGenerator : INormalGeneratorWithSeed
    {
        // TODO make field RandomSource type and inject in MersenneTwister? Probably not worth if for now.
        private MersenneTwister _randomSource;
        private int _seed;
        private bool? _threadSafe;

        public MersenneTwisterGenerator(int seed, bool threadSafe)
        {
            _randomSource = new MersenneTwister(seed, threadSafe);
            _seed = seed;
            _threadSafe = threadSafe;
        }

        public MersenneTwisterGenerator(bool threadSafe)
        {
            _randomSource = new MersenneTwister(threadSafe);
            _threadSafe = threadSafe;
        }

        public MersenneTwisterGenerator(int seed)
        {
            _randomSource = new MersenneTwister(seed);
        }

        public MersenneTwisterGenerator()
        {
            _randomSource = new MersenneTwister(RandomSeed.Robust());
        }

        public void Generate(double[] randomNormals, double mean, double standardDeviation)
        {
            Normal.Samples(_randomSource, randomNormals, mean, standardDeviation);
        }

        // TODO Generate method which accepts Span<double> as parameter (requires update to Math.NET library)

        public void Reset()
        {
            _randomSource = _threadSafe.HasValue ? new MersenneTwister(_seed, _threadSafe.Value) : new MersenneTwister(_seed);
        }

        public void ResetSeed(int seed)
        {
            _randomSource = _threadSafe.HasValue ? new MersenneTwister(seed, _threadSafe.Value) : new MersenneTwister(seed);
            _seed = seed;
        }

        public void ResetRandomSeed()
        {
            _randomSource = new MersenneTwister(RandomSeed.Robust());
        }

    }
}
