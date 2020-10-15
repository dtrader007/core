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
using Cmdty.Core.Simulation.MultiFactor;
using Cmdty.TimePeriodValueTypes;
using NUnit.Framework;

namespace Cmdty.Core.Simulation.Test
{
    [TestFixture]
    public sealed class MultiFactorParametersTest
    {

        [Test]
        public void For3FactorSeasonal_DailyGranularity_SeasonFactorVolEqualsHalfInputSeasonalVolOn1FebForLeapYearMultiples()
        {
            const double spotMeanReversion = 15.5;
            const double spotVol = 1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = 0.14;
            var start = new Day(2018, 1, 10);
            var end = new Day(2025, 6, 7);

            var multiFactorParams =
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end);

            double expectedFirstFebSeasonVol = seasonalVol / 2.0;

            double firstFeb18SeasonVol = multiFactorParams.Factors[2].Volatility[new Day(2018, 2, 1)];
            Assert.AreEqual(expectedFirstFebSeasonVol, firstFeb18SeasonVol);
            
            double firstFeb22SeasonVol = multiFactorParams.Factors[2].Volatility[new Day(2022, 2, 1)];
            Assert.AreEqual(expectedFirstFebSeasonVol, firstFeb22SeasonVol);
        }

        [Test]
        public void For3FactorSeasonal_HourlyGranularity_SeasonFactorVolEqualsHalfInputSeasonalVolOn1FebForLeapYearMultiples()
        {
            const double spotMeanReversion = 15.5;
            const double spotVol = 1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = 0.14;
            var start = new Hour(2018, 1, 10, 0);
            var end = new Hour(2025, 6, 7, 0);

            var multiFactorParams =
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end);

            double expectedFirstFebSeasonVol = seasonalVol / 2.0;

            double firstFeb18SeasonVol = multiFactorParams.Factors[2].Volatility[new Hour(2018, 2, 1, 0)];
            Assert.AreEqual(expectedFirstFebSeasonVol, firstFeb18SeasonVol);

            double firstFeb22SeasonVol = multiFactorParams.Factors[2].Volatility[new Hour(2022, 2, 1, 0)];
            Assert.AreEqual(expectedFirstFebSeasonVol, firstFeb22SeasonVol);
        }

        [Test]
        public void For3FactorSeasonal_DailyGranularity_SeasonFactorVolAlmostEqualsHalfInputSeasonalVolOn1FebForNonLeapYearMultiples()
        {
            const double spotMeanReversion = 15.5;
            const double spotVol = 1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = 0.14;
            var start = new Day(2018, 1, 10);
            var end = new Day(2025, 6, 7);

            var multiFactorParams =
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end);

            double expectedFirstFebSeasonVol = seasonalVol / 2.0;

            double firstFeb19SeasonVol = multiFactorParams.Factors[2].Volatility[new Day(2019, 2, 1)];
            Assert.AreEqual(expectedFirstFebSeasonVol, firstFeb19SeasonVol, 1E-5);

            double firstFeb20SeasonVol = multiFactorParams.Factors[2].Volatility[new Day(2020, 2, 1)];
            Assert.AreEqual(expectedFirstFebSeasonVol, firstFeb20SeasonVol, 1E-5);
        }

        [Test]
        public void For3FactorSeasonal_HourlyGranularity_SeasonFactorVolAlmostEqualsHalfInputSeasonalVolOn1FebForNonLeapYearMultiples()
        {
            const double spotMeanReversion = 15.5;
            const double spotVol = 1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = 0.14;
            var start = new Hour(2018, 1, 10, 0);
            var end = new Hour(2025, 6, 7, 0);

            var multiFactorParams =
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end);

            double expectedFirstFebSeasonVol = seasonalVol / 2.0;

            double firstFeb19SeasonVol = multiFactorParams.Factors[2].Volatility[new Hour(2019, 2, 1, 0)];
            Assert.AreEqual(expectedFirstFebSeasonVol, firstFeb19SeasonVol, 1E-5);

            double firstFeb20SeasonVol = multiFactorParams.Factors[2].Volatility[new Hour(2020, 2, 1, 0)];
            Assert.AreEqual(expectedFirstFebSeasonVol, firstFeb20SeasonVol, 1E-5);
        }

        [Test]
        public void For3FactorSeasonal_DailyGranularity_SeasonFactorVolAlmostEqualsMinusHalfInputSeasonalVolOn3Aug()
        {
            const double spotMeanReversion = 15.5;
            const double spotVol = 1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = 0.14;
            var start = new Day(2018, 1, 10);
            var end = new Day(2025, 6, 7);

            var multiFactorParams =
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end);

            double expectedVol = -seasonalVol / 2.0;

            double thirdAug19SeasonVol = multiFactorParams.Factors[2].Volatility[new Day(2019, 8, 3)];
            Assert.AreEqual(expectedVol, thirdAug19SeasonVol, 1E-5);

            double thirdAug20SeasonVol = multiFactorParams.Factors[2].Volatility[new Day(2020, 8, 3)];
            Assert.AreEqual(expectedVol, thirdAug20SeasonVol, 1E-5);
        }

        [Test]
        public void For3FactorSeasonal_HourlyGranularity_SeasonFactorVolAlmostEqualsMinusHalfInputSeasonalVolOn3Aug()
        {
            const double spotMeanReversion = 15.5;
            const double spotVol = 1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = 0.14;
            var start = new Hour(2018, 1, 10, 0);
            var end = new Hour(2025, 6, 7, 0);

            var multiFactorParams =
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end);

            double expectedVol = -seasonalVol / 2.0;

            double thirdAug19SeasonVol = multiFactorParams.Factors[2].Volatility[new Hour(2019, 8, 3, 0)];
            Assert.AreEqual(expectedVol, thirdAug19SeasonVol, 1E-5);

            double thirdAug20SeasonVol = multiFactorParams.Factors[2].Volatility[new Hour(2020, 8, 3, 0)];
            Assert.AreEqual(expectedVol, thirdAug20SeasonVol, 1E-5);
        }

        [Test]
        public void For3FactorSeasonal_NegativeSpotMeanReversion_ThrowsArgumentException()
        {
            const double spotMeanReversion = -0.5;
            const double spotVol = 1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = 0.14;
            var start = new Day(2018, 1, 10);
            var end = new Day(2025, 6, 7);

            Assert.Throws<ArgumentException>(() =>
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end));
        }

        [Test]
        public void For3FactorSeasonal_NegativeSpotVol_ThrowsArgumentException()
        {
            const double spotMeanReversion = 0.5;
            const double spotVol = -1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = 0.14;
            var start = new Day(2018, 1, 10);
            var end = new Day(2025, 6, 7);

            Assert.Throws<ArgumentException>(() =>
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end));
        }

        [Test]
        public void For3FactorSeasonal_NegativeLongTermVol_ThrowsArgumentException()
        {
            const double spotMeanReversion = 0.5;
            const double spotVol = 1.15;
            const double longTermVol = -0.21;
            const double seasonalVol = 0.14;
            var start = new Day(2018, 1, 10);
            var end = new Day(2025, 6, 7);

            Assert.Throws<ArgumentException>(() =>
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end));
        }


        [Test]
        public void For3FactorSeasonal_NegativeSeasonalVol_ThrowsArgumentException()
        {
            const double spotMeanReversion = 0.5;
            const double spotVol = 1.15;
            const double longTermVol = 0.21;
            const double seasonalVol = -0.14;
            var start = new Day(2018, 1, 10);
            var end = new Day(2025, 6, 7);

            Assert.Throws<ArgumentException>(() =>
                MultiFactorParameters.For3FactorSeasonal(spotMeanReversion, spotVol, longTermVol, seasonalVol, start, end));
        }

    }
}
