using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DryIoc.IssuesTests;
using DryIoc.Testing;

namespace DryIoc.UnitTests
{
    public class Program
    {
        public static void Main()
        {
            RunAllTests();
    
            // new ContainerTests().Run();
            // new Issue548_After_registering_a_factory_Func_is_returned_instead_of_the_result_of_Func().Run();
            // new GHIssue399_Func_dependency_on_Singleton_resolved_under_scope_breaks_after_disposing_scope_when_WithFuncAndLazyWithoutRegistration().Run();
            // new DryIoc.UnitTests.DecoratorTests().Run();
            //new GHIssue380_ExportFactory_throws_Container_disposed_exception().Run();
        }

        public static void RunAllTests()
        {
            var failed = false;
            var totalTestPassed = 0;
            void Run(Func<int> run, string name = null)
            {
                var testsName = name ?? run.Method.DeclaringType.FullName;
                try
                {
                    var testsPassed = run();
                    totalTestPassed += testsPassed;
                    Console.WriteLine($"{testsPassed,-4} of {testsName}");
                }
                catch (Exception ex)
                {
                    failed = true;
                    Console.WriteLine($"ERROR: Tests `{testsName}` failed with '{ex}'");
                }
            }

            var sw = Stopwatch.StartNew();

            Console.WriteLine();
            Console.WriteLine("NETCOREAPP2.1: Running UnitTests and IssueTests...");
            Console.WriteLine();

            var tests = new ITest[] {
                new ContainerTests(),
                new GHIssue378_InconsistentResolutionFailure(),
                new GHIssue380_ExportFactory_throws_Container_disposed_exception(),
                new Issue548_After_registering_a_factory_Func_is_returned_instead_of_the_result_of_Func(),
                new GHIssue399_Func_dependency_on_Singleton_resolved_under_scope_breaks_after_disposing_scope_when_WithFuncAndLazyWithoutRegistration(),
            };

            // Parallel.ForEach(tests, x => Run(x.Run)); // todo: @perf enable and test when more tests are added
            foreach (var x in tests) Run(x.Run);

            if (failed)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: Some tests are FAILED!");
                Environment.ExitCode = 1; // error exit code
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"{totalTestPassed,-4} of all tests are passing in {sw.ElapsedMilliseconds} ms.");
        }
    }
}
