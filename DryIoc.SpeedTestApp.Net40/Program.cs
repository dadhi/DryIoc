using System;
using DryIoc.SpeedTestApp.Net40.Tests;
using DryIoc.UnitTests.Net40.Playground;

namespace DryIoc.SpeedTestApp.Net40
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

        private static void CompareImmutableCollectionSpeed()
        {
            TestImmutableCollectionSpeeds.TestAccess();
            Console.WriteLine();
            TestImmutableCollectionSpeeds.TestAccess();
        }

        private static void CompareClosureFieldAccess()
        {
            ClosureFieldsAccessSpeed.TestExpr();
            //Console.WriteLine();
            //ClosureFieldsAccessSpeed.TestExpr();
        }
    }
}
