# Cmdty Core Library
[![Build Status](https://dev.azure.com/cmdty/github/_apis/build/status/cmdty.core?branchName=master)](https://dev.azure.com/cmdty/github/_build/latest?definitionId=3&branchName=master)
[![NuGet](https://img.shields.io/nuget/v/cmdty.core.svg)](https://www.nuget.org/packages/Cmdty.Core/)

Core tools for the valuation and optimisation of physical commodity assets. Currently under early stages of development.

## Getting Started

### Installing

```
PM> Install-Package Cmdty.Core -Version 0.1.0-beta2
```

### Building a Trinomial Tree
The following example shows how to build a trinomial tree, fitted to a forward curve, with time varying volatility.

```cs
const double meanReversion = 16.0;
const double timeDelta = 1.0 / 365.0;

TimeSeries<Day, double> forwardCurve = new TimeSeries<Day,double>.Builder
{
    {new Day(2019, 8, 25), 85.96 },
    {new Day(2019, 8, 26), 86.05 },
    {new Day(2019, 8, 27), 87.58 },
    {new Day(2019, 8, 28), 86.96 },
    {new Day(2019, 8, 29), 86.77 },
    {new Day(2019, 8, 30), 86.99 }
}.Build();


TimeSeries<Day, double> spotVolatility = new TimeSeries<Day, double>.Builder
{
    {new Day(2019, 8, 25),  0.675},
    {new Day(2019, 8, 26),  0.84},
    {new Day(2019, 8, 27),  0.845},
    {new Day(2019, 8, 28),  0.843},
    {new Day(2019, 8, 29),  0.834},
    {new Day(2019, 8, 30),  0.8125}

}.Build();

TimeSeries<Day, IReadOnlyList<TreeNode>> trinomialTree = OneFactorTrinomialTree.CreateTree(forwardCurve, meanReversion, spotVolatility, timeDelta);
```

For more details [samples/csharp/](https://github.com/cmdty/core/tree/master/samples/csharp).
## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
