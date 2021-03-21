using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cmdty.Core.Simulation.MultiFactor;
using Cmdty.TimePeriodValueTypes;
using Cmdty.Core.Simulation;

namespace Cmdty.Core.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var _seed = 12;
            var _currentDate = new DateTime(2020, 07, 27);

            var _dailyForwardCurve = new Dictionary<Day, double>
            {
                {new Day(2020, 08, 01), 56.85 },
                {new Day(2021, 01, 15), 59.08 },
                {new Day(2021, 07, 30), 62.453 }
            };

            int numSims = 1000;
            double meanReversion = 0.0;
            bool antithetic = true;

            var spotVols = new Dictionary<Day, double>
            {
                {new Day(2020, 08, 01), 0.45},
                {new Day(2021, 01, 15), 0.42},
                {new Day(2021, 07, 30), 0.33}
            };

            var _singleNonMeanRevertingFactorParams = MultiFactorParameters.For1Factor(meanReversion, spotVols);

            Day[] simulatedPeriods = _dailyForwardCurve.Keys.OrderBy(day => day).ToArray();
            var normalSimulator = new MersenneTwisterGenerator(_seed, antithetic);

            var simulator = new MultiFactorSpotPriceSimulator<Day>(
                _singleNonMeanRevertingFactorParams,
                _currentDate,
                _dailyForwardCurve,
                simulatedPeriods,
                TimeFunctions.Act365,
                normalSimulator
                );

            var simResults = simulator.Simulate(numSims);

            ReadOnlyMemory<double> simulatedSpotPrices0 =
                    simResults.SpotPricesForStepIndex(0);

            ReadOnlyMemory<double> simulatedSpotPrices1 =
                    simResults.SpotPricesForStepIndex(1);

            ReadOnlyMemory<double> simulatedSpotPrices2 =
                    simResults.SpotPricesForStepIndex(2);

            using (StreamWriter file = new StreamWriter("sims.csv"))
            {
                foreach (var i in new [] {0,1,2})
                {
                    ReadOnlyMemory<double> simulatedSpotPrices = simResults.SpotPricesForStepIndex(i);
                    foreach (var item in simulatedSpotPrices.ToArray())
                    {
                        file.Write(item + ",");
                    }
                    file.Write(Environment.NewLine);
                }

            }

                Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
