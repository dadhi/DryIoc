using System;
using System.Diagnostics;
using DryIoc.IssuesTests;
// using DryIoc.Docs;

namespace DryIoc.UnitTests
{
    public class Program
    {
        public static void Main()
        {
            RunAllTests();

            // new GHIssue470_Regression_v5_when_resolving_Func_of_IEnumerable_of_IService_with_Parameter().Run();

            // ObjectLayoutInspector.TypeLayout.PrintLayout<Request>();

            // new GHIssue101_Compile_time_generated_object_graph().Run();
        }

        public static void RunAllTests()
        {
            var unitTests = new ITest[]
            {
                new ActionTests(),
                new ArrayToolsTest(),
                new AsyncExecutionFlowScopeContextTests(),
                new ChildContainerTests(),
                new CompositePatternTests(),
                new ConstructionTests(),
                new ContainerTests(),
                new ContextDependentResolutionTests(),
                new DecoratorConditionTests(),
                new DecoratorTests(),
                new DelegateFactoryTests(),
                new DiagnosticsTests(),
                new DynamicFactoryTests(),
                new DynamicRegistrationsTests(),
                new EnumerableAndArrayTests(),
                new FuncTests(),
                new IfAlreadyRegisteredTests(),
                new IfUnresolvedTests(),
                new InitializerTests(),
                new InjectionRulesTests(),
                new KeyValuePairResolutionTests(),
                new LazyEnumerableTests(),
                new LazyTests(),
                new MetadataTests(),
                new NewTests(),
                new OpenGenericsTests(),
                new PrimitiveValueInjectionTests(),
                new PrintTests(),
                new PropertyResolutionTests(),
                new RegisterInstanceTests(),
                new RegisterManyTests(),
                new RegisterPlaceholderTests(),
                new RegisterWithNonStringServiceKeyTests(),
                new RequiredServiceTypeTests(),
                new ResolveManyTests(),
                new ReuseInCurrentScopeTests(),
                new RulesTests(),
                new SelectConstructorWithAllResolvableArgumentTests(),
                new StronglyTypeConstructorAndParametersSpecTests(),
                new ThrowTests(),
                new TypeCSharpNameFormattingTests(),
                new TypeToolsTests(),
                new UnregisterTests(),
                new WipeCacheTests(),
                new WrapperTests(),
            };
            var issueTests = new ITest[]
            {
                new DotnetWeekBlogExample(),
                new Issue_Can_resolve_singleton_with_Func_of_scoped_dependency(),
                new Issue_HandleVariance(),
                new Issue_InjectingSerilogLogger(),
                new ParameterResolutionFixture(),
                new Issue_Register_null_string(),
                new Issue_SupportForDynamicKeyword(),
                new Issue_UsingAsyncMethodAsMadeOf(),
                new Issue_Value_type_resolution_dependency(),
                new Issue152_ExponentialMemoryPerformanceWithRegardsToTheObjectGraphSize(),
                new Issue497_ConstructorWithResolvableArguments_is_not_working_properly(),
                new Issue548_After_registering_a_factory_Func_is_returned_instead_of_the_result_of_Func(),
                new GHIssue4_Rule_for_Func_and_Lazy_to_be_resolved_even_without_requested_service_registered(),
                new GHIssue6_Open_generic_singleton_service_registration_that_satisfies_multiple_interfaces(),
                new GHIssue7_1_Context_based_injection(),
                new GHIssue7_2_Context_based_injection(),
                new GHIssue29_Resolve_caches_args_values(),
                new GHIssue32_Memory_leak_with_ResolveManyBehavior_AzLazyEnumerable(),
                new GHIssue80_ScopedOrSingleton_extra_constructor_calls(),
                new GHIssue101_Compile_time_generated_object_graph(),
                new GHIssue180_Option_nullable_int_argument_with_not_null_default_value(),
                new GHIssue191_Optional_IResolverContext_argument_in_Func_of_service(),
                new GHIssue198_Open_generics_resolve_fails_if_there_is_a_static_constructor(),
                new GHIssue289_Think_how_to_make_Use_to_directly_replace_scoped_service_without_special_asResolutionCall_setup(),
                new GHIssue323_Add_registration_setup_option_to_avoidResolutionScopeTracking(),
                new GHIssue378_InconsistentResolutionFailure(),
                new GHIssue380_ExportFactory_throws_Container_disposed_exception(),
                new GHIssue390_NullReferenceException_on_Unregister(),
                new GHIssue391_Deadlock_during_Resolve(),
                new GHIssue399_Func_dependency_on_Singleton_resolved_under_scope_breaks_after_disposing_scope_when_WithFuncAndLazyWithoutRegistration(),
                new GHIssue402_Inconsistent_transient_disposable_behavior_when_using_Made(),
                new GHIssue406_Allow_the_registration_of_the_partially_closed_implementation_type(),
                new GHIssue460_Getting_instance_from_parent_scope_even_if_replaced_by_Use(),
                new GHIssue461_Transient_IDisposable_factory_method_with_custom_constructor_args(),
                new GHIssue470_Regression_v5_when_resolving_Func_of_IEnumerable_of_IService_with_Parameter(),
                new GHIssue471_Regression_v5_using_Rules_SelectKeyedOverDefaultFactory(),
                new GHIssue495_Automatically_generate_Resolution_calls_for_the_missing_registrations_to_avoid_manual_RegisterPlaceholder(),
            };
            // var docsTests = new Func<int>[] 
            // { 
            //     () => { new Nested_decorators_order().Example(); return 1; }
            // };

            Scope.WaitForScopedServiceIsCreatedTimeoutTicks = 50; // @important

            var totalPassed = 0;
            var sw = Stopwatch.StartNew();
            totalPassed += RunTests(unitTests, "UnitTests");
            totalPassed += RunTests(issueTests, "IssueTests");
            Console.WriteLine($"\nTotal {totalPassed} of tests are passing in {sw.ElapsedMilliseconds} ms.");

            int Run(Func<int> run, string name = null)
            {
                var testsName = name ?? run.Method.DeclaringType.FullName;
                int testsPassed;
                try
                {
                    testsPassed = run();
                    Console.WriteLine($"{testsPassed,-4} of {testsName}");
                }
                catch (Exception ex)
                {
                    testsPassed = 0;
                    Console.WriteLine($"ERROR: Tests `{testsName}` failed with '{ex}'");
                }
                return testsPassed;
            }

            int RunTests(ITest[] tests, string name)
            {
                Console.WriteLine($"\n{name} - running on .NET Core..\n");
                var somePassed = 0;
                var someFailed = false;
                var sw = Stopwatch.StartNew();
                foreach (var x in tests)
                {
                    var passed = Run(x.Run);
                    if (passed > 0) somePassed += passed;
                    else someFailed = true;
                }
                if (!someFailed)
                    Console.WriteLine($"\n{somePassed} {name} are passing in {sw.ElapsedMilliseconds} ms.");
                else
                {
                    Console.WriteLine("\nFAILURE! Some tests are FAILED!");
                    Console.WriteLine($"\nThe rest {somePassed} of {name} are passing in {sw.ElapsedMilliseconds} ms.");
                    Environment.ExitCode = 1; // error exit code
                }
                return somePassed;
            }
        }
    }
}
