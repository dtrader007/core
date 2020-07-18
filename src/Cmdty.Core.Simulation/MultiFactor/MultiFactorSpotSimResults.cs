using System;
using JetBrains.Annotations;

namespace Cmdty.Core.Simulation.MultiFactor
{
    public sealed class MultiFactorSpotSimResults
    {
        public double[,] SpotPrices { get; }
        public double[,,] MarkovFactors { get; }

        public MultiFactorSpotSimResults([NotNull] double[,] spotPrices, [NotNull] double[,,] markovFactors)
        {
            SpotPrices = spotPrices ?? throw new ArgumentNullException(nameof(spotPrices));
            MarkovFactors = markovFactors ?? throw new ArgumentNullException(nameof(markovFactors));
        }
    }
}