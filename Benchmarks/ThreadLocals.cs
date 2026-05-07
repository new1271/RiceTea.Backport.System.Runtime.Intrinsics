namespace RiceTea.Benchmark;

internal static class ThreadLocals
{
    private static readonly ThreadLocal<Random> _random = new(() => new Random());

    public static Random Random => _random.Value;
}