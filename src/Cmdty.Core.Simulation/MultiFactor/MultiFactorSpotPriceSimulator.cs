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
using System.Linq;
using Cmdty.TimePeriodValueTypes;
using JetBrains.Annotations;
using MathNet.Numerics.LinearAlgebra;

namespace Cmdty.Core.Simulation.MultiFactor
{
    public sealed class MultiFactorSpotPriceSimulator<T>
        where T : ITimePeriod<T>
    {
        private readonly INormalGenerator _normalGenerator;
        private readonly double[] _forwardPrices;
        private readonly double[] _driftAdjustments;
        private readonly double[,] _reversionMultipliers;
        private readonly double[,] _spotVols;
        private readonly Matrix<double>[] _factorCovarianceSquareRoots;
        private readonly IReadOnlyList<T> _simulatedPeriods;


        public MultiFactorSpotPriceSimulator([NotNull] MultiFactorParameters<T> modelParameters, DateTime currentDateTime,
            [NotNull] IReadOnlyDictionary<T, double> forwardCurve, [NotNull] IEnumerable<T> simulatedPeriods, 
            Func<DateTime, DateTime, double> timeFunc, [NotNull] INormalGenerator normalGenerator) // TODO pass in random number generator factory
        {
            if (modelParameters == null) throw new ArgumentNullException(nameof(modelParameters));
            if (forwardCurve == null) throw new ArgumentNullException(nameof(forwardCurve));
            if (simulatedPeriods == null) throw new ArgumentNullException(nameof(simulatedPeriods));
            _normalGenerator = normalGenerator ?? throw new ArgumentNullException(nameof(normalGenerator));

            T[] simulatedPeriodsArray = simulatedPeriods.ToArray();

            int numPeriods = simulatedPeriodsArray.Length;
            int numFactors = modelParameters.NumFactors;

            if (numPeriods == 0)
                throw new ArgumentException(nameof(simulatedPeriods) + " argument cannot be empty.", nameof(simulatedPeriods));

            int numRandomDimensions = numPeriods * numFactors;
            if (!_normalGenerator.MatchesDimensions(numRandomDimensions))
                throw new ArgumentException($"Injected normal random generator is not set up to generate with {numRandomDimensions} dimensions.", nameof(normalGenerator));

            _forwardPrices = new double[numPeriods];
            _driftAdjustments = new double[numPeriods];
            _reversionMultipliers = new double[numPeriods, numFactors];
            _spotVols = new double[numPeriods, numFactors];
            _factorCovarianceSquareRoots = new Matrix<double>[numPeriods];
            _simulatedPeriods = simulatedPeriodsArray;

            double timeToMaturityPrevious = 0;
            for (int i = 0; i < simulatedPeriodsArray.Length; i++)
            {
                T period = simulatedPeriodsArray[i];
                if (period.Start <= currentDateTime) // TODO make comparison using day count function?
                    throw new ArgumentException($"All elements of {simulatedPeriods} must start after {currentDateTime}.", nameof(simulatedPeriods));
                
                if (i > 0)
                {
                    T lastPeriod = simulatedPeriodsArray[i - 1];
                    if (period.CompareTo(lastPeriod) < 0)
                        throw new ArgumentException(nameof(simulatedPeriods) + " argument must be sorted in ascending order.", nameof(simulatedPeriods));
                    if (period.Equals(lastPeriod))
                        throw new ArgumentException(nameof(simulatedPeriods) + $" argument cannot contain duplicated elements. More than one element with value {period} found.", nameof(simulatedPeriods));
                }

                if (!forwardCurve.TryGetValue(period, out double forwardPrice))
                    throw new ArgumentException($"Forward curve does not contain price for period {period}.", nameof(forwardCurve));

                _forwardPrices[i] = forwardPrice;

                double timeToMaturity = timeFunc(currentDateTime, period.Start);
                double timeIncrement = timeToMaturity - timeToMaturityPrevious;

                for (int j = 0; j < numFactors; j++)
                {
                    Factor<T> factor = modelParameters.Factors[j];
                    _spotVols[i, j] = factor.Volatility[period]; // TODO check if period in volatility
                    _reversionMultipliers[i, j] = Math.Exp(-factor.MeanReversion * timeIncrement);
                }

                _factorCovarianceSquareRoots[i] = CalcFactorCovarianceSquareRoots(modelParameters, timeIncrement);

                _driftAdjustments[i] = CalcDriftAdjustment(modelParameters, timeToMaturity, period);

                timeToMaturityPrevious = timeToMaturity;
            }

        }

        private static double CalcDriftAdjustment(MultiFactorParameters<T> modelParameters, double timeToMaturity, T period)
        {
            double sum = 0.0;

            // TODO use symmetry to speed up?
            // TODO formulate as matrix multiplication as quadratic form?
            for (int i = 0; i < modelParameters.NumFactors; i++)
            {
                Factor<T> factor1 = modelParameters.Factors[i];
                if (!factor1.Volatility.TryGetValue(period, out double vol1))
                    throw new ArgumentException($"Factor {i} of multi-factor model parameters does not contain vol for period {period}.", 
                        nameof(modelParameters));
                double meanReversion1 = factor1.MeanReversion;
                for (int j = 0; j < modelParameters.NumFactors; j++)
                {
                    Factor<T> factor2 = modelParameters.Factors[j];
                    if (!factor2.Volatility.TryGetValue(period, out double vol2))
                        throw new ArgumentException($"Factor {j} of multi-factor model parameters does not contain vol for period {period}.",
                            nameof(modelParameters));
                    double meanReversion2 = factor2.MeanReversion;

                    sum += vol1 * vol2 * modelParameters.FactorCorrelations[i, j] *
                           CalcFactorContExt(meanReversion1 + meanReversion2, timeToMaturity);
                }
            }

            return -sum / 2.0;
        }

        private static Matrix<double> CalcFactorCovarianceSquareRoots(MultiFactorParameters<T> modelParameters, double timeIncrement)
            
        {
            int numFactors = modelParameters.NumFactors;
            // TODO does Math.NET have a more efficient representation of symmetric and triangular matrices?
            Matrix<double> covarianceMatrix = Matrix<double>.Build.Dense(numFactors, numFactors);
            for (int i = 0; i < numFactors; i++)
            {
                for (int j = i; j < numFactors; j++)
                {
                    double meanReversionsSum = modelParameters.Factors[i].MeanReversion +
                                               modelParameters.Factors[j].MeanReversion;

                    double covariance = modelParameters.FactorCorrelations[i, j] *
                                        CalcFactorContExt(meanReversionsSum, timeIncrement);

                    covarianceMatrix[i, j] = covariance;
                    if (i != j)
                        covarianceMatrix[j, i] = covariance;
                }
            }

            return covarianceMatrix.Cholesky().Factor;
        }

        private static double CalcFactorContExt(double meanReversionsSum, double timeIncrement)
        {
            // TODO use some sort of tolerance?
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (meanReversionsSum == 0)
                return timeIncrement;

            return (1 - Math.Exp(-meanReversionsSum * timeIncrement)) / meanReversionsSum;
        }

        public MultiFactorSpotSimResults<T> Simulate(int numSims)
        {
            int numSteps = _forwardPrices.Length;
            int numFactors = _spotVols.GetLength(1);
            int randomNumDimensions = numFactors * numSteps;

            // TODO remove these allocation and pass in arrays as parameters?
            var spotPrices = new double[numSteps * numSims];
            var markovFactors = new double[numSteps * numSims * numFactors];

            // Avoid repeated heap allocation by instantiating arrays to be reused outside of loop
            var thisStepFactors = new double[numFactors];
            var factorStochasticTerm = new double[numFactors];
            var independentStandardNormals = new double[randomNumDimensions];

            for (int simIndex = 0; simIndex < numSims; simIndex++)
            {
                for (int i = 0; i < thisStepFactors.Length; i++)
                    thisStepFactors[i] = 0.0;

                _normalGenerator.Generate(independentStandardNormals, 0.0, 1.0);

                int standardNormalStartIndex = 0;
                for (int stepIndex = 0; stepIndex < numSteps; stepIndex++)
                {
                    var iidSpan = independentStandardNormals.AsSpan(standardNormalStartIndex);
                    CorrelateThisTimeStepRandoms(iidSpan, _factorCovarianceSquareRoots[stepIndex], factorStochasticTerm);

                    double sumFactorTimesVol = 0.0;
                    for (int factorIndex = 0; factorIndex < thisStepFactors.Length; factorIndex++)
                    {
                        double factorValue = thisStepFactors[factorIndex] * _reversionMultipliers[stepIndex, factorIndex] + factorStochasticTerm[factorIndex];
                        thisStepFactors[factorIndex] = factorValue;
                        // Put all simulations for same time step next to each other as this is how they will be access in LSMC
                        markovFactors[stepIndex * numFactors * numSims + factorIndex * numSims + simIndex] = factorValue;
                        sumFactorTimesVol += factorValue * _spotVols[stepIndex, factorIndex];
                    }

                    // TODO IMPORTANT - Math.Exp is slow, so look into vectorizing with MKL or Math.NET (if offered) or Agner Fog library
                    // Put all simulations for same time step next to each other as this is how they will be access in LSMC
                    spotPrices[stepIndex * numSims + simIndex] = _forwardPrices[stepIndex] * Math.Exp(_driftAdjustments[stepIndex] + sumFactorTimesVol);
                    standardNormalStartIndex += numFactors;
                }
            }
            
            return new MultiFactorSpotSimResults<T>(spotPrices, markovFactors, _simulatedPeriods, numSteps, numSims, numFactors);
        }

        private void CorrelateThisTimeStepRandoms(Span<double> independentStandardNormals, 
                        Matrix<double> factorCovarianceSquareRoot, double[] factorStochasticTerm)
        {
            // TODO either vectorize this for change factorCovarianceSquareRoot to an array
            for (int i = 0; i < factorStochasticTerm.Length; i++)
            {
                double sumProduct = 0.0;
                for (int j = 0; j <= i; j++) // Cholesky is lower triangular
                {
                    sumProduct += factorCovarianceSquareRoot[i, j] * independentStandardNormals[j];
                }
                factorStochasticTerm[i] = sumProduct;
            }
        }

        public void ResetNormalGenerator()
        {
            _normalGenerator.Reset();
        }

        public void ResetNormalGeneratorSeed(int seed)
        {
            INormalGeneratorWithSeed normalGeneratorWithSeed = GetGeneratorWithSeed();
            normalGeneratorWithSeed.ResetSeed(seed);
        }

        public void ResetNormalGeneratorRandomSeed()
        {
            INormalGeneratorWithSeed normalGeneratorWithSeed = GetGeneratorWithSeed();
            normalGeneratorWithSeed.ResetRandomSeed();
        }

        private INormalGeneratorWithSeed GetGeneratorWithSeed()
        {
            // TODO test error message is ok
            if (!(_normalGenerator is INormalGeneratorWithSeed normalGeneratorWithSeed))
                throw new InvalidOperationException($"Type of {nameof(INormalGenerator)} injected into constructor, {_normalGenerator.GetType().Name}, does not implement {nameof(INormalGeneratorWithSeed)}.");
            return normalGeneratorWithSeed;
        }

        public bool NormalGeneratorTypeHasSeed()
        {
            return _normalGenerator is INormalGeneratorWithSeed;
        }

    }
}
