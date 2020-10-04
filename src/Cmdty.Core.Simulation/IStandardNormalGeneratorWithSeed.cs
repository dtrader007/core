namespace Cmdty.Core.Simulation
{
    public interface IStandardNormalGeneratorWithSeed : IStandardNormalGenerator
    {
        void ResetSeed(int seed);
        void ResetRandomSeed();
    }
}