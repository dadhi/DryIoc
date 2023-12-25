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
            // new GHIssue619_FaultySingletonDependency().Run();

            RunAllTests();

            // new GHIssue555_ConcreteTypeDynamicRegistrations_is_not_working_with_MicrosoftDependencyInjectionRules().Run();
            // new GHIssue507_Transient_resolve_with_opening_scope_using_factory_func_in_singleton().Run();
            // new GHIssue243_Delegate_Factory_Resolving_Incremental_Improvement_over_Func_Wrapper().Run();
            // new GHIssue580_Scope_is_lost_in_IResolver_inside_scope_because_of_singleton().Run(); // todo: @fixme
            // new GHIssue169_Decorators().Run();
            // new ActionTests().Run();
            // new GHIssue116_ReOpened_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution().Run();
            // new GHIssue116_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution().Run();
            // new GHIssue550_Use_not_working_for_scoped_type_after_having_resolved_it_in_another_scope().Run();
            // new GHIssue546_Generic_type_constraint_resolution_doesnt_see_arrays_as_IEnumerable().Run();
            // new GHIssue536_DryIoc_Exception_in_a_Constructor_of_a_Dependency_does_tunnel_through_Resolve_call().Run();
            // new GHIssue535_Property_injection_does_not_work_when_appending_implementation_for_multiple_registration().Run();
            // new GHIssue532_WithUseInterpretation_still_use_DynamicMethod_and_ILEmit().Run();
            // new GHIssue506_WithConcreteTypeDynamicRegistrations_hides_failed_dependency_resolution().Run();
            // new GHIssue470_Regression_v5_when_resolving_Func_of_IEnumerable_of_IService_with_Parameter().Run();
            // new GHIssue101_Compile_time_generated_object_graph().Run();
            // new SO_Injecting_the_collection_of_interfaces_implemented_by_decorator().Run();
            // new SO_Child_Container_for_transients().Run();
            // new RegisterManyTests().Run();
            // ObjectLayoutInspector.TypeLayout.PrintLayout<Request>();
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

                new MefAttributedModel.UnitTests.AllowDefaultTests(),
                new MefAttributedModel.UnitTests.AttributedModelTests(),
                new MefAttributedModel.UnitTests.CodeGenerationTests(),
                new MefAttributedModel.UnitTests.DryIocMefCompatibilityTests(),
                new MefAttributedModel.UnitTests.ExportAsDecoratorTests(),
                new MefAttributedModel.UnitTests.ExportAsWrapperTests(),
                new MefAttributedModel.UnitTests.ExportFactoryTests(),
                new MefAttributedModel.UnitTests.ExportImportWithKeyTests(),
                new MefAttributedModel.UnitTests.ExportManyTests(),
                new MefAttributedModel.UnitTests.ImportAttributeTests(),
                new MefAttributedModel.UnitTests.ImportExternalTests(),
                new MefAttributedModel.UnitTests.ImportManyTests(),
                new MefAttributedModel.UnitTests.ImportWithMetadataTests(),
                new MefAttributedModel.UnitTests.InheritedExportTests(),
                new MefAttributedModel.UnitTests.ReuseAttributeTests(),
                new MefAttributedModel.UnitTests.MoreAttributedModelTests(),
            };
            var issueTests = new ITest[]
            {
                new DotnetWeekBlogExample(),
                new Messages_Test(),

                new SO_Child_Container_for_transients(),
                new SO_Injecting_the_collection_of_interfaces_implemented_by_decorator(),
                new SO_Decorator_not_being_constrained_correctly(),

                new InjectListOfDepsWithStringDeps(),
                new Issue_2_Can_inject_singleton_service_from_parent_container_After_it_was_resolved_from_parent(),
                new Issue_Can_resolve_singleton_with_Func_of_scoped_dependency(),
                new Issue_HandleVariance(),
                new Issue_InjectingSerilogLogger(),
                new Issue_InjectPrimitiveValueBasedOnRequest(),
                new Issue_Register_null_string(),
                new Issue_SupportForDynamicKeyword(),
                new Issue_UsingAsyncMethodAsMadeOf(),
                new Issue_Value_type_resolution_dependency(),
                
                new Issue26_DryIOCSingletonFailureTest(),
                new Issue45_UnregisterOpenGenericAfterItWasResolvedOnce(),
                new Issue46_ReuseInCurrentScopeForNestedDependenciesNotWorking(),
                new Issue58_HandleReusedObjectsIDisposableAndGCing(),
                new Issue64_ScopeAndChildContainerAccessAfterDisposal(),
                new Issue69_RecognizeGenericParameterConstraints(),

                new Issue107_NamedScopesDependingOnResolvedTypes(),
                new Issue152_ExponentialMemoryPerformanceWithRegardsToTheObjectGraphSize(),
                new Issue357_PartImportsSatisfied(),
                new Issue404_ConstructorWithResolvableArguments_does_not_take_into_account_parameter_service_key(),
                new Issue407_Cannot_resolve_MadeOf_params_explicitly(),
                new Issue416_Adding_always_true_condition_to_decorator_changes_the_decorated_outcome(),
                new Issue417_Performance_issue_with_Func(),
                new Issue422_Unable_to_resolve_decorator_with_named_dependency(),
                new Issue423_InnerScopeIsInjectedIntoSingleton(),
                new Issue429_Resolve_instance_from_named_scope_with_Func(),
                new Issue435_ReuseSingleton_prevents_the_correct_container_injection_within_explicit_resolve(),
                new Issue443_ConventionBasedOnParameterName(),
                new Issue446_Select_single_open_generic_impl_based_on_matching_closed_service_type(),
                new Issue451_Should_forbid_static_imports(),
                new Issue473_Unable_to_match_service_with_open_generic(),
                new Issue477_ArgumentException_while_resolving(),
                new Issue486_CustomDynamicRegistrationProvider(),
                new Issue488_DryIoc_ContainerException_if_using_WithDependencies(),
                new Issue496_Provide_builtin_method_to_post_initialize_instance_after_it_is_registered(),
                new Issue497_ConstructorWithResolvableArguments_is_not_working_properly(),
                new Issue500_Rule_WithConcreteTypeDynamicRegistrations_disables_allowDisposableTransient(),
                new Issue508_SelectLastRegisteredFactory_and_resolving_collection_of_open_generic_isnot_working_as_intended(),
                new Issue512_InResolutionScopeOf_is_not_working(),
                new Issue519_Dependency_of_singleton_not_working_when_using_child_container(),
                new Issue527_ErrorResolvemManyAfterUregister(),
                new Issue530_Multi_tenancy_support(),
                new Issue541_Dynamic_Registrations_dont_detect_circular_dependencies(),
                new Issue543_Dynamic_Registrations_dont_respect_shared_creation_policy(),
                new Issue545_Func_Of_Scoped(),
                new Issue546_Recursive_dependency_isnt_detected_in_large_object_graphs(),
                new Issue548_After_registering_a_factory_Func_is_returned_instead_of_the_result_of_Func(),
                new Issue554_Allow_Register_an_open_generic_service_type_with_closed_implementation_to_enable_variance(),
                new Issue561_Child_containers_and_singletons(),
                new Issue566_Named_service_not_replaced(),
                new Issue569_Replacing_Registration_clears_all_existing_registrations(),
                new Issue570_ArgumentNullThrownWhenMultipleConstructorsAndArgsDepsProvided(),
                new Issue572_Dynamic_Service_Keys(),
                new Issue574_IResolverContext_Use_instance_ShouldNotHaveSideEffectsOnOtherScopes(),
                new Issue577_InconsistentResolutionAndCacheAnomaly(),
                new Issue578_Specific_Service_Type_required_by_controller_not_resolving(),
                new Issue579_VerifyResolutions_strange_behaviour(),
                new Issue580_Same_service_instance_resolved_twice_when_decorator_is_used(),
                new Issue581_Constructor_injection_with_array_parameter(),
                new Issue596_RegisterMany_with_Factory(),
                new Issue603_async_actions_in_MVC(),

                new GHIssue4_Rule_for_Func_and_Lazy_to_be_resolved_even_without_requested_service_registered(),
                new GHIssue6_Open_generic_singleton_service_registration_that_satisfies_multiple_interfaces(),
                new GHIssue7_1_Context_based_injection(),
                new GHIssue7_2_Context_based_injection(),
                new GHIssue29_Resolve_caches_args_values(),
                new GHIssue32_Memory_leak_with_ResolveManyBehavior_AzLazyEnumerable(),
                new GHIssue37_MediatR_Polymorphic_Notification(),
                new GHIssue80_ScopedOrSingleton_extra_constructor_calls(),
                new GHIssue101_Compile_time_generated_object_graph(),
                new GHIssue114_Resolve_Action_T(),
                new GHIssue116_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution(),
                new GHIssue116_ReOpened_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution(),
                new GHIssue169_Decorators(),
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
                new GHIssue504_Add_IDictionary_wrapper(),
                new GHIssue506_WithConcreteTypeDynamicRegistrations_hides_failed_dependency_resolution(),
                new GHIssue507_Transient_resolve_with_opening_scope_using_factory_func_in_singleton(),
                new GHIssue508_Throws_when_lazy_resolve_after_explicit_create_using_factory_func_from_within_scope(),
                new GHIssue512_Optimize_injection_of_IResolverContext(),
                new GHIssue514_Avoid_using_the_Func_arguments_for_recursive_service_first_found_outside_the_Func(),
                new GHIssue516_Singleton_Decorator_to_Scoped_base_should_not_work_but_does(),
                new GHIssue518_Select_default_then_resolvable_constructor(),
                new GHIssue532_WithUseInterpretation_still_use_DynamicMethod_and_ILEmit(),
                new GHIssue535_Property_injection_does_not_work_when_appending_implementation_for_multiple_registration(),
                new GHIssue536_DryIoc_Exception_in_a_Constructor_of_a_Dependency_does_tunnel_through_Resolve_call(),
                new GHIssue546_Generic_type_constraint_resolution_doesnt_see_arrays_as_IEnumerable(),
                new GHIssue550_Use_not_working_for_scoped_type_after_having_resolved_it_in_another_scope(),
                new GHIssue554_System_NullReferenceException_Object_reference_not_set_to_an_instance_of_an_object(),
                new GHIssue555_ConcreteTypeDynamicRegistrations_is_not_working_with_MicrosoftDependencyInjectionRules(),
                new GHIssue557_WithFactorySelector_allows_to_Resolve_the_keyed_service_as_non_keyed(),
                new GHIssue559_Possible_inconsistent_behaviour(),
                new GHIssue574_Cannot_register_multiple_impls_in_child_container_with_default_service_key(),
                new GHIssue576_Extension_methods_not_being_handled_correctly_in_MadeOf_service_returning_expression(),
                new GHIssue580_Scope_is_lost_in_IResolver_inside_scope_because_of_singleton(),
                new GHIssue588_Container_IsDisposed_property_not_reflecting_own_scope_disposed_state(),
                new GHIssue608_Multiple_same_type_same_keyed(),
                new GHIssue610_CustomDynamicRegistrationProvider_ConstructorWithResolvableArguments(),
            };
            // var docsTests = new Func<int>[] // todo: @docs
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
                int testsPassed;
                try
                {
                    testsPassed = run();
#if DEBUG
                    // we don't need to list the tests one-by-one on CI, and it makes avoiding it saves 30% of time
                    var testsName = name ?? run.Method.DeclaringType.FullName;
                    Console.WriteLine($"{testsPassed,-4} of {testsName}");
#endif
                }
                catch (Exception ex)
                {
                    testsPassed = 0;
                    var testsName = name ?? run.Method.DeclaringType.FullName;
                    Console.WriteLine($"""
                    --------------------------------------------
                    ERROR: Tests `{testsName}` failed with
                    {ex}
                    --------------------------------------------
                    """);
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
