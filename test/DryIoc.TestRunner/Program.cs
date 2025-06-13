using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DryIoc.IssuesTests;

namespace DryIoc.UnitTests;

public class Program
{
    public static void Main()
    {
        new RegisterAttributeTests().Run();

        // new GHIssue223_IAsyncDisposable().Run();

        // new GHIssue685_Creating_scopes_via_funcs_is_not_threadsafe_and_fails_sporadically_with_NullRef_exception().Run();
        // new GHIssue678_Scope_is_lost_in_disposable_service().Run();
        // new RulesTests().Run();

        // new GHIssue672_Wrong_decorator_parameter_with_custom_args().Run();

        // Rules.UnsafeResetDefaultRulesToUseCompilationOnly();
        // new GHIssue623_Scoped_service_decorator().Run();

        // new GHIssue503_Compile_time_container().Run();
        // new GHIssue667_Resolve_with_serviceKey_does_not_invoke_factory_selector().Run();

        RunAllTests();
    }

    public static void RunAllTests()
    {
#if USE_COMPILATION_ONLY
        Console.WriteLine("USE_COMPILATION_ONLY=true");
        Rules.UnsafeResetDefaultRulesToUseCompilationOnly();
#endif
        // note: @important to remember to do the Thread.Sleep in tests less that this setting, 
        // if you don't intentionally want the Error.WaitForScopedServiceIsCreatedTimeoutExpired exception, 
        // e.g. see GHIssue337_Singleton_is_created_twice, GHIssue391_Deadlock_during_Resolve, Issue157_ContainerResolveFactoryIsNotThreadSafe
#if DEBUG
        Scope.WaitForScopedServiceIsCreatedTimeoutMilliseconds = 2_000;
#else
        Scope.WaitForScopedServiceIsCreatedTimeoutMilliseconds = 150;
#endif
        var perfMemoryTests = new ITest[]
        {
            new Issue152_ExponentialMemoryPerformanceWithRegardsToTheObjectGraphSize(),
        };

        var unitTests = new ITest[]
        {
            new Docs.CreatingAndDisposingContainer(),
            new Docs.Decorators(),
            new Docs.ErrorDetectionAndResolution(),
            new Docs.ExamplesContextBasedResolution(),
            new Docs.Interception(),
            new Docs.KindsOfChildContainer(),
            new Docs.OpenGenerics(),
            new Docs.RegisterResolve(),
            new Docs.RequiredServiceType(),
            new Docs.ReuseAndScopes(),
            new Docs.RulesAndDefaultConventions(),
            new Docs.SelectConstructorOrFactoryMethod(),
            new Docs.SpecifyDependencyAndPrimitiveValues(),
            new Docs.ThreadSafety(),
            new Docs.UsingInTestsWithMockingLibrary(),
            new Docs.Wrappers(),
            new Docs.MefAttributedModel(),

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

            new Syntax.Autofac.UnitTests.Issue123_TipsForMigrationFromAutofac(),
            new Syntax.Autofac.UnitTests.Issue123_TipsForMigrationFromAutofac_WithParameter(),

            new Microsoft.DependencyInjection.Specification.Tests.ValidateCaptiveDependencyTests(),
            new Microsoft.DependencyInjection.Specification.Tests.GetRequiredServiceTests(),
            new Microsoft.DependencyInjection.Specification.Tests.ServicesTests(),

            new CommonServiceLocator.UnitTests.DryIocServiceLocatorTests(),
        };

        var issuesTests = new ITest[]
        {
            new DotnetWeekBlogExample(),
            new Messages_Test(),
            new IssuesTests.MetadataProxies.MetadataViewTests(),
            new IssuesTests.Interception.InterceptionTests(),
            new IssuesTests.Interception.WrapAsLazyTests(),
            new IssuesTests.Interception.GHIssue50_Questions_about_property_field_can_not_be_injected(),
            new IssuesTests.Samples.AutoWiring(),
            new IssuesTests.Samples.ConstructorSelectionTests(),
            new IssuesTests.Samples.DefaultReuseTest(),
            new IssuesTests.Samples.GettingStarted(),
            new IssuesTests.Samples.LazyRegistrationInfoStepByStep(),
            new IssuesTests.Samples.OpenScopeTests(),
            new IssuesTests.Samples.PrismXamarinForms(),
            new IssuesTests.Samples.PubSub(),
            new IssuesTests.Samples.ResolveMocksForNonRegisteredServices(),
            new IssuesTests.Samples.SelectConstructorWithAllResolvableArguments(),

            new SO_Child_Container_for_transients(),
            new SO_Decorator_not_being_constrained_correctly(),
            new SO_DryIoC_pass_param_to_constructor_of_open_generic_service_based_on_generic_type_parameter(),
            new SO_Injecting_the_collection_of_interfaces_implemented_by_decorator(),
            new SO_Open_Generics_Registration(),

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

            // note: Explicit, relies on GC
            // new Issue58_HandleReusedObjectsIDisposableAndGCing(),

            // BitBucket issues from older times
            new Issue26_DryIOCSingletonFailureTest(),
            new Issue45_UnregisterOpenGenericAfterItWasResolvedOnce(),
            new Issue46_ReuseInCurrentScopeForNestedDependenciesNotWorking(),
            new Issue64_ScopeAndChildContainerAccessAfterDisposal(),
            new Issue69_RecognizeGenericParameterConstraints(),
            new Issue80_SupportWrappersForAllInterfacesImplementedByArray(),
            new Issue81_SupportForOptionalParametersInConstructor(),
            new Issue85_SkipResolutionForPropertiesAndFieldsAlreadySetInConstructor(),
            new Issue85_SkipResolutionForPropertiesAndFieldsAlreadySetInConstructor.CanSetPropAndFieldWithExpressionTreeInNet35(),
            new Issue86_SkipIndexerOnAllPropertiesInjection(),
            new Issue107_NamedScopesDependingOnResolvedTypes(),
            new Issue110_HidingMultipleContravariantImplementationsBehindComposite(),
            new Issue110_SupportContravarianceInResolveMany(),
            new Issue116_InvokeFactoryConstructorTwoTimes(),
            new Issue122_DecoratorOfLazyResultsInStackOverflowException(),
            new Issue123_TipsForMigrationFromAutofac_WithParameter(),
            new Issue123_TipsForMigrationFromAutofac(),
            new Issue128_ResolveFailsWithSingletonsInOuterScope(),
            new Issue135_DecoratorsIgnoredInChildScope(),
            new Issue143_Mixing_closed_and_open_generics_subsumes_the_latter(),
            new Issue144_NonPublic_property_as_FactoryMethod_causes_unexplained_NRE(),
            new Issue145_SimplifyDefiningOfOpenGenericFactoryMethod(),
            new Issue148_NestedOptionalDependenciesPreventTheOuterDependencyFromInstantiating(),
            new Issue153_ContextDependentResolutionOnlyWorksForTheVeryFirstContext(),
            new Issue157_ContainerResolveFactoryIsNotThreadSafe(),
            new Issue158_WrappingDependencyInLazyResultsInTheLossOfLifespanValidation(),
            new Issue159_Context_based_injection_doesnt_work_with_InjectPropertiesAndFields(),
            new Issue160_NestingOfDecoratorsOfWrappedServiceUsesOnlyFirstDecorator(),
            new Issue164_EventAggregatorImpl(),
            new Issue166_Disposing_Facade_is_disposing_facade_parent(),
            new Issue168_RegisterInstanceWithIfAlreadyRegisteredReplaceReplacesWrongRegistration(),
            new Issue169_FalseAlarmCheckingScopeLifetimeConsistencyForFuncWrapper(),
            new Issue178_FallbackContainerDisposesInstanceRegisteredInParent(),
            new Issue181_RegisterInstanceUnregister(),
            new Issue184_ReuseInNamedChildrenOfNamedScopes(),
            new Issue200_MultipleInstancesForSingletonCreatedWhenContainerIsSharedAmongMultipleThreads(),
            new Issue201_MultiThreadingIssueWhenRegisterInstanceUsedWithinOpenScope(),
            new Issue212_ResolveManyOfObjectWithGenericRequiredServiceTypeIsFailingWithArgumentException(),
            new Issue213_LazySingletonsShouldBeResolvedAfterContainerIsDisposed(),
            new Issue224_EnumerableWrappedInFuncLosesTheInformationAboutFuncWrapperCausingIncorrectScopeLifetimeValidation(),
            new Issue230_CustomInitializerAttachedToLazilyResolvedDependencyIsCalledOncePerResolution(),
            new Issue247_Collection_wrapper_resolved_from_Facade_does_not_count_parent_container_registrations(),
            new Issue251_AutoRegisterTypesFromDifferentNamespaceAndAssemblies(),
            new Issue262_Using_attributes_to_inject_primitive_variables(),
            new Issue264_IfAlreadyRegisteredReplaceCanSpanMultipleRegistrations(),
            new Issue267_False_alarm_about_recursive_dependency_when_using_decorator(),
            new Issue274_Lazy_resolution_of_dependency_registered_with_ReuseInResolutionScope_breaks_subsequent_resolutions_without_wrappers(),
            new Issue277_Custom_value_for_dependency_evaluated_to_null_is_interpreted_as_no_custom_value(),
            new Issue278_Arg_Of_does_not_recognize_service_key_of_non_primitive_type(),
            new Issue281_MakeAutofacMigrationEasier(),
            new Issue287_Add_IfUnresolved_ReturnDefaultIfNotRegistered_policy(),
            new Issue300_Exception_when_reusing_objects(),
            new Issue304_Add_option_to_pass_values_for_some_dependencies_on_Resolve(),
            new IssuesTests.Interception.Issue310_Problems_with_Decorators_and_Service_Keys(),
            new Issue311_Resolve_does_not_work_with_InThread_registration(),
            new Issue313_Support_non_public_constructors_with_ConstructorWithResolvableArguments(),
            new Issue315_Decorator_of_wrapper(),
            new Issue318_RegisterInstance_doesnt_honour_current_OpenScope(),
            new Issue328_Lazy_collection_resolve_behavior_in_and_out_of_scope(),
            new Issue333_Container_should_resolve_IEnumerable_instances_registered_without_serviceKey(),
            new Issue339_GenericDecoratorWithConstraints(),
            new Issue344_TransientDisposableValidation(),
            new Issue355_UnexpectedSingletonDisposal(),
            new Issue357_PartImportsSatisfied(),
            new Issue366_Facade_Returns_Null_for_ResolveMany_Fallback(),
            new Issue367_MefRulesBreakMadeParameters(),
            new Issue377_Support_custom_IReuse_with_MEF_attributes(),
            new Issue378_Resolve_a_Single_Instance_InWebRequest(),
            new Issue382_Different_instances_of_interface_with_Reuse_InCurrentNamedScope(),
            new Issue387_ArgumentException_with_initiliazer(),
            new Issue394_Reimporting_services(),
            new Issue396_ResolveMany_appears_not_to_use_UnknownServiceResolver(),
            new Issue397_ActionExportsTypeConversion(),
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
            new Issue522_IncorrectApplicationOfSingletonReuse(),
            new Issue527_ErrorResolvemManyAfterUregister(),
            new Issue530_Multi_tenancy_support(),
            new Issue533_Exporting_WPF_UserControl_causes_NullReferenceException(),
            new Issue541_Dynamic_Registrations_dont_detect_circular_dependencies(),
            new Issue543_Dynamic_Registrations_dont_respect_shared_creation_policy(),
            new Issue544_WithTrackingDisposableTransients_may_downgrade_Singletons_to_Transients(),
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

            // Now on GitHub
            new GHIssue4_Rule_for_Func_and_Lazy_to_be_resolved_even_without_requested_service_registered(),
            new GHIssue6_Open_generic_singleton_service_registration_that_satisfies_multiple_interfaces(),
            new GHIssue7_1_Context_based_injection(),
            new GHIssue7_2_Context_based_injection(),
            new GHIssue29_Resolve_caches_args_values(),
            new GHIssue32_Memory_leak_with_ResolveManyBehavior_AzLazyEnumerable(),
            new GHIssue37_MediatR_Polymorphic_Notification(),
            new GHIssue41_RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType(),
            new GHIssue44_Real_world_benchmark_the_unit_work_or_say_the_controller_resolution(),
            new GHIssue45_Consider_expression_interpretation_to_speed_up_first_time_resolution(),
            new GHIssue61_Rules_SelectLastRegisteredFactory_does_not_account_for_OpenGenerics(),
            new GHIssue63_Func_wrapper_resolving(),
            new GHIssue66_Cannot_instantiate_DictionaryT(),
            new GHIssue80_ScopedOrSingleton_extra_constructor_calls(),
            new GHIssue100_ResolveMany_with_Meta_does_NOT_work(),
            new GHIssue101_Compile_time_generated_object_graph(),
            new GHIssue105_Resolve_uses_args_values_to_resolve_dependencies_too(),
            new GHIssue107_Resolve_still_caches_args_values_when_using_instance_factory_method(),
            new GHIssue109_Using_Func_wrapper_and_FastExpressionCompiler(),
            new GHIssue114_Resolve_Action_T(),
            new GHIssue116_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution(),
            new GHIssue116_ReOpened_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution(),
            new GHIssue118_Validate_issue(),
            new GHIssue125_DryIoC_throws_Exception_if_registering_two_classes_with_common_base(),
            new GHIssue139_StackOverflow_exception(),
            new GHIssue142_Request_stack(),
            new GHIssue147_Added_RegisterDelegate_with_the_list_of_dependencies_to_inject__not_to_Resolve(),
            new DryIocTest.GHIssue151_Resolve_problem_with_ThreadScopeContext(),
            new GHIssue169_Decorators(),
            new GHIssue171_Wrong_IContainer_resolved(),
            new GHIssue173_Validate_method_throws_TypeInitializationException_for_OpenGenericTypeKey(),
            new GHIssue179_MadeOf_Parameters_do_not_follow_Reuse_setting(),
            new GHIssue180_Option_nullable_int_argument_with_not_null_default_value(),
            new GHIssue184_ReflectionTools_returns_static_constructors(),
            new GHIssue188_Custom_delegate_wrapper_resolving(),
            new GHIssue191_Optional_IResolverContext_argument_in_Func_of_service(),
            new GHIssue192_DryIOC_new_Transient_Disposable(),
            new GHIssue196_Private_and_public_Constructors_in_generic_classes(),
            new GHIssue198_Open_generics_resolve_fails_if_there_is_a_static_constructor(),
            new GHIssue211_FEC_sometimes_is_20_precent_slower(),
            new GHIssue215_RegisterInitializer_causes_additional_call_to_Dispose_when_container_is_disposed(),
            new GHIssue223_IAsyncDisposable(),
            new GHIssue228_Updated_DryIoc_from_4_to_4_1_in_Unity_Engine_project_keyed_register_resolve_wont_work_anymore(),
            new GHIssue233_Add_RegisterDelegate_with_parameters_returning_object_for_the_requested_runtime_known_service_type(),
            new GHIssue235_Add_RegisterDelegateMany_RegisterInstanceMany_UseMany_to_fill_the_API_gap(),
            new GHIssue237_UseInstance_with_interface_based_serviceType_does_not_replace_previous_instance(),
            new GHIssue243_Delegate_Factory_Resolving_Incremental_Improvement_over_Func_Wrapper(),
            new GHIssue248_WithConcreteTypeDynamicRegistrations_condition_gets_called_with_serviceKey_always_null(),
            new GHIssue254_ResolveMany_if_singleton_decorators_decorates_the_first_item_only(),
            new GHIssue259_Possibility_to_prevent_disposal_in_child_container(),
            new GHIssue267_MS_DI_Incorrect_resolving_for_generic_types(),
            new GHIssue283_Open_generic_decorator_is_not_applied_to_RegisterInstance(),
            new GHIssue288_Recursive_resolution_ignores_current_scope_Use_instance(),
            new GHIssue289_Think_how_to_make_Use_to_directly_replace_scoped_service_without_special_asResolutionCall_setup(),
            new GHIssue290_ScopedTo_to_Singleton_does_not_work(),
            new GHIssue294_in_parameter_modifier_breaks_service_resolution(),
            new GHIssue295_useParentReuse_does_not_respects_parent_reuse(),
            new GHIssue297_Can_RegisterMany_ignore_already_registered_services_based_on_reuse(),

            new GHIssue301_Breakage_in_scoped_enumeration_in_v4(),
            new GHIssue303_Open_Generic_Singleton_do_not_provide_same_instance_for_Resolve_and_ResolveMany(),
            new GHIssue307_Lift_up_the_requirement_for_the_Export_attribute_for_RegisterExports(),
            new GHIssue314_Expose_the_usual_IfAlreadyRegistered_option_parameter_for_RegisterMapping(),
            new GHIssue315_Combining_RegisterDelegate_with_TrackingDisposableTransients_rule_throws_TargetParameterCountException(),
            new Microsoft.DependencyInjection.Specification.Tests.GHIssue317_Error_for_register_IOptions_in_prism(),
            new GHIssue323_Add_registration_setup_option_to_avoidResolutionScopeTracking(),
            new GHIssue332_Delegate_returning_null_throws_exception_RegisteredDelegateResultIsNotOfServiceType(),
            new GHIssue337_Singleton_is_created_twice(),
            new GHIssue338_Child_container_disposes_parent_container_singletons(),
            new GHIssue340_WaitForItemIsSet_does_never_end(),
            new GHIssue343_Scope_validation_for_Transient_does_not_work_as_expected(),
            new GHIssue344_Scope_is_disposed_before_parent_when_using_facade(),
            new GHIssue347_The_AsResolutionCall_option_and_or_WithFuncAndLazyWithoutRegistration_rule_are_not_respected(),
            new GHIssue348_Create_a_child_container_without_WithNoMoreRegistrationAllowed_flag(),
            new GHIssue349_MEF_ReuseAttribute_treats_ScopeName_as_string_instead_of_object_type(),
            new GHIssue350_Wrong_scoped_resolve(),
            new GHIssue352_Consider_resolving_the_variance_compatible_open_generic_the_same_as_for_collection_of_open_generics(),
            new GHIssue353_Provide_a_way_to_add_new_temporary_registrations(),
            new GHIssue355_Auto_mocking_feature_for_unit_testing(),
            new GHIssue367_Resolve_with_FactoryMethod_of_instance_throws_ContainerException(),
            new GHIssue369_Child_container_and_openResolutionScope_in_one_test(),
            new GHIssue376_MessedUpStackTrace(),
            new GHIssue378_InconsistentResolutionFailure(),
            new GHIssue380_ExportFactory_throws_Container_disposed_exception(),
            new GHIssue387_Nested_container_returns_a_new_instance_for_singletons(),
            new GHIssue390_NullReferenceException_on_Unregister(),
            new GHIssue391_Deadlock_during_Resolve(),
            new GHIssue399_Func_dependency_on_Singleton_resolved_under_scope_breaks_after_disposing_scope_when_WithFuncAndLazyWithoutRegistration(),

            new GHIssue402_Inconsistent_transient_disposable_behavior_when_using_Made(),
            new GHIssue406_Allow_the_registration_of_the_partially_closed_implementation_type(),
            new GHIssue417_Performance_degradation_with_dynamic_registrations_in_v4_compared_to_v2(),
            new GHIssue418_ResolveCovariantBaseType(),
            new Microsoft.DependencyInjection.Specification.Tests.GHIssue429_Memory_leak_on_MS_DI_with_Disposable_Transient(),
            new Microsoft.DependencyInjection.Specification.Tests.GHIssue432_Resolving_interfaces_with_contravariant_type_parameter_fails_with_RegisteringImplementationNotAssignableToServiceType_error(),
            new GHIssue434_ReturnDefaultIfNotRegistered_is_not_respected_between_scopes(),
            new Microsoft.DependencyInjection.Specification.Tests.GHIssue435_hangfire_use_dryioc_report_ContainerIsDisposed(),
            new GHIssue446_Resolving_a_record_without_registration_causes_a_StackOverflowException(),
            new GHIssue448_Dynamic_registration_does_not_work_for_resolve_with_service_key(),
            new GHIssue449_Optional_dependency_shouldnt_treat_its_dependencies_as_optional(),
            new GHIssue451_Resolve_compiler_generated_settings_regression(),
            new GHIssue460_Getting_instance_from_parent_scope_even_if_replaced_by_Use(),
            new GHIssue461_Transient_IDisposable_factory_method_with_custom_constructor_args(),
            new GHIssue470_Regression_v5_when_resolving_Func_of_IEnumerable_of_IService_with_Parameter(),
            new GHIssue471_Regression_v5_using_Rules_SelectKeyedOverDefaultFactory(),
            new GHIssue483_Used_instance_is_not_checked_in_the_upper_scopes_in_some_case(),
            new GHIssue489_Resolve_with_IfUnresolved_ReturnDefaultIfNotRegistered_throws_if_dependency_is_not_registered(),
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
            new GHIssue619_FaultySingletonDependency(),
            new GHIssue623_Scoped_service_decorator(),
            new GHIssue653_KeyValuePair_exposes_internal_DryIoc_structures(),
            new GHIssue659_Can_I_inspect_a_scope_for_all_the_dependencies_resolved_in_the_scope(),
            new GHIssue669_Unable_to_resolve_type_with_optional_arguments_with_both_MefAttributedModel_and_MS_DI(),
            new GHIssue678_Scope_is_lost_in_disposable_service(),
            new GHIssue685_Creating_scopes_via_funcs_is_not_threadsafe_and_fails_sporadically_with_NullRef_exception(),
        };

        var totalPassed = 0;
        var sw = Stopwatch.StartNew();

        var unitTestsTask = Task.Run(() => RunTests(unitTests, "UnitTests;Docs"));
        var issuesTestsTask = Task.Run(() => RunTests(issuesTests, "IssuesTests"));
        var perfMemoryTestsTask = Task.Run(() => RunTests(perfMemoryTests, "PerfMemoryTests"));
        Task.WaitAll(unitTestsTask, issuesTestsTask, perfMemoryTestsTask);

        var unitTestCount = unitTestsTask.IsCompleted ? unitTestsTask.Result : 0;
        var issuesTestCount = issuesTestsTask.IsCompleted ? issuesTestsTask.Result : 0;
        var perfMemoryTestCount = perfMemoryTestsTask.IsCompleted ? perfMemoryTestsTask.Result : 0;

        totalPassed = unitTestCount + issuesTestCount + perfMemoryTestCount;

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
            var somePassed = 0;
            var someFailed = 0;
            var sw = Stopwatch.StartNew();
            foreach (var x in tests)
            {
                var passed = Run(x.Run);
                if (passed > 0) somePassed += passed;
                else ++someFailed;
            }
            if (someFailed == 0)
                Console.WriteLine($"\n{name}: {somePassed} tests are passing in {sw.ElapsedMilliseconds} ms.");
            else
            {
                Console.WriteLine($"\n{name}: {someFailed} of tests FAILED! Remaining {somePassed} tests are passing in {sw.ElapsedMilliseconds} ms.");
                Environment.ExitCode = 1; // error exit code
            }
            return somePassed;
        }
    }
}
