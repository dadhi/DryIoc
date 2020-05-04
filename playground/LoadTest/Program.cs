using System;

namespace LoadTest
{
    public class Program
    {
        static /*async Task*/void Main(string[] args)
        {
            Console.WriteLine("Starting up!");

            InvalidProgramExceptionTest.Start();
            //SplitDependencyGraphTest.Start();
            LoadTestBenchmark.Start();

            Console.WriteLine("Success!");
            Console.ReadKey();
        }
    }
}