using System;
using DryIoc.SpeedTestApp.Tests;
using DryIoc.UnitTests.Net40.Playground;

namespace DryIoc.SpeedTestApp
{
    class Program
    {
        static void Main()
        {
            CompareArrayCreationSpeed();
            Console.ReadKey();
        }

        private static void CompareArrayCreationSpeed()
        {
            TestEnumerableCreationSpeed.Compare();
            Console.WriteLine();
            TestEnumerableCreationSpeed.Compare();
        }

        private static void CompareClosureFieldAccess()
        {
            ClosureFieldsAccessSpeed.TestExpr();
            //Console.WriteLine();
            //ClosureFieldsAccessSpeed.TestExpr();
        }
    }
}
