using System;
using System.Diagnostics;
using DryIoc.IssuesTests;

namespace DryIoc.UnitTests
{
    public class Program
    {
        public static void Main()
        {
            RunAllTests();

            // new GHIssue579_Scope_is_lost_in_IResolver_inside_scope_because_of_singleton().Run();

            // new GHIssue574_Cannot_register_multiple_impls_in_child_container_with_default_service_key().Run();
            // new GHIssue576_Extension_methods_not_being_handled_correctly_in_MadeOf_service_returning_expression().Run();
            // new GHIssue116_ReOpened_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution().Run();
            // new RequiredPropertiesTests().Run();            
            // new PropertyResolutionTests().Run();            
            // new IssuesTests.Samples.DefaultReuseTest().Run();
            // new GHIssue391_Deadlock_during_Resolve().Run();
            // new GHIssue559_Possible_inconsistent_behaviour().Run();
            // new GHIssue557_WithFactorySelector_allows_to_Resolve_the_keyed_service_as_non_keyed().Run();
            // new GHIssue565_Is_ignoring_ReuseScoped_setting_expected_behaviour_when_also_set_to_openResolutionScope().Run();
            // new GHIssue555_ConcreteTypeDynamicRegistrations_is_not_working_with_MicrosoftDependencyInjectionRules().Run();
            // new GHIssue554_System_NullReferenceException_Object_reference_not_set_to_an_instance_of_an_object().Run();
            // new GHIssue536_DryIoc_Exception_in_a_Constructor_of_a_Dependency_does_tunnel_through_Resolve_call().Run();
            // new GHIssue535_Property_injection_does_not_work_when_appending_implementation_for_multiple_registration().Run();
            // new GHIssue532_WithUseInterpretation_still_use_DynamicMethod_and_ILEmit().Run();
            // new GHIssue506_WithConcreteTypeDynamicRegistrations_hides_failed_dependency_resolution().Run();
        }

        public static void RunAllTests()
        {
            Scope.WaitForScopedServiceIsCreatedTimeoutTicks = 50; // @important
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
            Console.WriteLine("Running UnitTests and IssueTests (.NET Core) ...");
            Console.WriteLine();

            var tests = new ITest[] 
            {
                new ContainerTests(),
                new OpenGenericsTests(),
                new DynamicRegistrationsTests(),
                new PropertyResolutionTests(),
                new RequiredPropertiesTests(),
                new SelectConstructorWithAllResolvableArgumentTests(),
                new Issue107_NamedScopesDependingOnResolvedTypes(),
                new Issue548_After_registering_a_factory_Func_is_returned_instead_of_the_result_of_Func(),
                new GHIssue116_ReOpened_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution(),
                new GHIssue378_InconsistentResolutionFailure(),
                new GHIssue380_ExportFactory_throws_Container_disposed_exception(),
                new GHIssue391_Deadlock_during_Resolve(),
                new GHIssue399_Func_dependency_on_Singleton_resolved_under_scope_breaks_after_disposing_scope_when_WithFuncAndLazyWithoutRegistration(),
                new GHIssue402_Inconsistent_transient_disposable_behavior_when_using_Made(),
                new GHIssue406_Allow_the_registration_of_the_partially_closed_implementation_type(),
                new GHIssue460_Getting_instance_from_parent_scope_even_if_replaced_by_Use(),
                new GHIssue470_Regression_v5_when_resolving_Func_of_IEnumerable_of_IService_with_Parameter(),
                new GHIssue471_Regression_v5_using_Rules_SelectKeyedOverDefaultFactory(),
                new GHIssue506_WithConcreteTypeDynamicRegistrations_hides_failed_dependency_resolution(),
                new GHIssue507_Transient_resolve_with_opening_scope_using_factory_func_in_singleton(),
                new GHIssue508_Throws_when_lazy_resolve_after_explicit_create_using_factory_func_from_within_scope(),
                new GHIssue532_WithUseInterpretation_still_use_DynamicMethod_and_ILEmit(),
                new GHIssue536_DryIoc_Exception_in_a_Constructor_of_a_Dependency_does_tunnel_through_Resolve_call(),
                new GHIssue546_Generic_type_constraint_resolution_doesnt_see_arrays_as_IEnumerable(),
                new GHIssue554_System_NullReferenceException_Object_reference_not_set_to_an_instance_of_an_object(),
                new GHIssue555_ConcreteTypeDynamicRegistrations_is_not_working_with_MicrosoftDependencyInjectionRules(),
                new GHIssue557_WithFactorySelector_allows_to_Resolve_the_keyed_service_as_non_keyed(),
                new GHIssue559_Possible_inconsistent_behaviour(),
                new GHIssue565_Is_ignoring_ReuseScoped_setting_expected_behaviour_when_also_set_to_openResolutionScope(),
                new GHIssue576_Extension_methods_not_being_handled_correctly_in_MadeOf_service_returning_expression(),
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
