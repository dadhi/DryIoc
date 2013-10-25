using System;

namespace DryIoc.SpeedTestApp.Net40
{
    class Program
    {
        static void Main()
        {
            CompareClosureFieldAccess();
            Console.ReadKey();
        }

        private static void CompareClosureFieldAccess()
        {
            ClosureFieldsAccessSpeed.TestExpr();
            //Console.WriteLine();
            //ClosureFieldsAccessSpeed.TestExpr();
        }
    }
}
