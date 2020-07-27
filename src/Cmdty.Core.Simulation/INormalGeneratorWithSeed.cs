namespace Cmdty.Core.Simulation
{
    public interface INormalGeneratorWithSeed : INormalGenerator
    {
        void ResetSeed(int seed);
        void ResetRandomSeed();
    }
}