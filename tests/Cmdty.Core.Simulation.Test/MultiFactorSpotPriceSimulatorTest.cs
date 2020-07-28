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

using Cmdty.Core.Simulation.MultiFactor;
using Cmdty.TimePeriodValueTypes;
using Cmdty.TimeSeries;
using NUnit.Framework;

namespace Cmdty.Core.Simulation.Test
{
    [TestFixture]
    public sealed class MultiFactorSpotPriceSimulatorTest
    {

        // Tests to do:
        // Input parameters:
        //  - Zero vol - simulation equals curve
        //  - Single non-mean reverting factor
        //  - Two non-mean reverting factors
        //  - One more factors with high mean reversion:
        //      - Standard deviation of increments far in future go to zero
        // Outputs to test:
        //  - Expectation equals curve
        //  - Variance is expected
        //  - Characteristic function
        //  - Correlation of increments

        public MultiFactorSpotPriceSimulatorTest()
        {
            SimulateForZeroVolatility();
            SimulateForSingleNonMeanRevertingFactor();
        }

        private void SimulateForSingleNonMeanRevertingFactor()
        {
            
        }

        private void SimulateForZeroVolatility()
        {
            var multiFactorParameters = new MultiFactorParameters<Day>(new []
            {
                new Factor<Day>(0.0, new DoubleTimeSeries<Day>(new Day(2020, 07, 28), new []{0.0, 0.0, 0.0})), 
            }, new double[,]{{1.0}});



        }
    }
}