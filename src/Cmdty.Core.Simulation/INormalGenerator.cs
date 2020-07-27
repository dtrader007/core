namespace Cmdty.Core.Simulation
{
    public interface INormalGenerator
    {
        void Generate(double[] randomNormals, double mean, double standardDeviation);
        void Reset();
        bool MatchesDimensions(int numDimensions); // Necessary for stuff like Sobol which has to be set up with specific number of dimensions
    }
}