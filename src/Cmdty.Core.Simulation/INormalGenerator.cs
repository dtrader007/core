namespace Cmdty.Core.Simulation
{
    public interface INormalGenerator
    {
        void Generate(double[] randomNormals, double mean, double standardDeviation);
        void Reset();
    }
}