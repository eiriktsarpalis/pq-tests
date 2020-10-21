using BenchmarkDotNet.Running;

namespace PriorityQueue.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var switcher = new BenchmarkSwitcher(assembly);
            switcher.Run(args);
        }
    }
}
