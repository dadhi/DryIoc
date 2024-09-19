namespace DryIoc.Docs;

public class CreatingAndDisposingContainer : ITest
{
    public int Run()
    {
        new Creating_container_with_explicit_defaults().Example();
        new Adding_some_rules().Example();
        new Adding_some_rules_with_action().Example();
        new Disposing_container().Example();
        return 4;
    }
}

public class Decorators : ITest
{
    public int Run()
    {
        new Decorator_with_logger().Example();
        new Decorator_of_service_with_serviceKey().Example_with_condition();
        new Decorator_of_service_with_serviceKey().Example_with_DecoratorOf();
        new Decorator_of_service_with_key_and_type().Example_with_DecoratorOf_type_and_key();
        new Decorator_of_service_with_key_and_type().Example_with_DecoratorOf_type();
        new Nested_decorators().Example();
        new Nested_decorators_order().Example();
        new Open_generic_decorators().Example();
        new Decorator_of_generic_T_type().Example();
        new Decorator_of_generic_T_type_with_condition().Example();
        new Decorator_reuse().Example();
        new Decoratee_reuse().Example();
        new Decorator_of_wrapped_service().Example();
        new Nesting_decorators_of_wrapped_service().Example();
        new Collection_wrapper_of_non_keyed_and_keyed_services().Example();
        new Decorator_of_wrapper().Example();
        new Decorator_as_initializer().Example();
        new Reusing_the_scoped_service_from_the_parent_scope().Example();
        new Using_the_Decorator_directly_for_the_complex_initialization().Example();
        return 19;
    }
}

public class ErrorDetectionAndResolution : ITest
{
    public int Run()
    {
        new Unable_to_resolve_unknown_service().Example();
        new Unable_to_resolve_from_registered_services().Example();
        new No_current_scope_available().Example();
        new Recursive_dependency_detected().Example();
        new Allow_a_recursive_dependencies().Example();
        new Allow_recursive_dependency_in_DryIoc().Example();
        new Registrations_diagnostics().Example();
        new Validate_CaptiveDependency_example().Scoped_in_a_Singleton_should_be_reported_by_Validate();
        return 8;
    }
}

public class ExamplesContextBasedResolution : ITest
{
    public int Run()
    {
        new Log4net_logger_example().Example();
        new Serilog_logger_example().Example();
        return 2;
    }
}

public class Interception : ITest
{
    public int Run()
    {
        new Register_and_use_interceptor().Example();
        new Register_and_use_async_interceptor().Example().GetAwaiter().GetResult();
        new Register_and_use_interceptor_with_LinFu().Example();
        return 3;
    }
}

public class KindsOfChildContainer : ITest
{
    public int Run()
    {
        new ChildExample().Parent_and_child();
        new FacadeExample().Facade_for_tests();
        new Without_cache().Example();
        new Without_singletons().Example();
        new With_registrations_copy().Example();
        return 5;
    }
}

public class OpenGenerics : ITest
{
    public int Run()
    {
        new Register_open_generic().Example();
        new Open_generic_registrations().Example();
        new Closed_is_preferred_over_open_generic().Example();
        new Matching_open_generic_type_constraints().Example();
        new Fill_in_type_arguments_from_constraints().Example();
        new Generic_variance_thingy().Example();
        new Turn_off_generic_variance_in_collections().Example();
        return 7;
    }
}

public class RegisterResolve : ITest
{
    public int Run()
    {
        new Register_service_with_implementation_types().Example();
        new Register_service_with_implementation_runtime_types().Example();
        new Register_open_generic_service_with_implementation_runtime_types().Example();
        new Register_implementation_as_service_type().Example();
        new One_level_deep_registration_API().Example();
        new Singleton_service_registration().Example();
        new Multiple_default_registrations().Example();
        new Multiple_keyed_registrations().Example();
        new Resolve_commands_with_keys().Example();
        new Resolving_service_with_key_as_KeyValuePair().Example();
        new IsRegistered_examples().Example();
        new IsRegistered_with_key_examples().Example();
        new IsRegistered_for_wrapper_or_decorators().Example();
        new Check_if_resolvable().Example();
        new Get_specific_registration().Example();
        new RegisterMany_examples().Example();
        new Register_mapping().Example();
        new Register_mapping_with_RegisterMany().Example();
        new Register_delegate().Example();
        new Register_delegate_with_resolved_dependencies().Example();
        new Register_delegate_with_parameters().Example();
        new Register_delegate_returning_object().Example();
        new Register_instance_example().Example();
        new Example_of_scoped_and_singleton_instance().Example();
        new Typed_instance().Example();
        new Register_initializer().Example();
        new Register_initializer_for_any_object().Example();
        new RegisterInitializer_with_reuse_different_from_initialized_object().Example();
        new Register_placeholder().Example();
        new Register_disposer().Example();
        new Register_disposer_for_many_services().Example();
        return 31;
    }
}

public class RequiredServiceType : ITest
{
    public int Run()
    {
        new Required_service_type_is_implemented_by_resolution_type().Example();
        new Service_type_for_a_wrapper().Example();
        new Select_to_use_and_open_generic_type().Example();
        new Using_register_delegate_to_adapt_service_type().Example();
        new Required_service_type_to_adapt_the_object_dependency().Example();
        new Required_service_type_with_wrapper().Example();
        new Required_service_type_in_collection().Example();
        return 7;
    }
}

public class ReuseAndScopes : ITest
{
    public int Run()
    {
        new Disposable_transient_as_resolved_service().Example();
        new Disposable_transient_as_injected_dependency().Example();
        new Tracking_disposable_transient().Example();
        new Prevent_disposable_tracking_with_Func().Example();
        new Default_reuse_per_container().Example();
        new Singleton_reuse().Example();
        new Scoped_reuse_register_and_resolve().Example();
        new Scoped_reuse_resolve_wrapped_in_Lazy_outside_of_scope().Example();
        new Scoped_reuse_resolve_Lazy_with_scope_context().Example();
        new Nested_scopes_without_scope_context().Example();
        new Nested_scopes_with_scope_context().Example();
        new Named_open_scopes_and_scoped_to_name().Example();
        new Scoped_to_service_reuse().Example();
        new Emulating_openResolutionScope_setup().Example();
        new Scoped_to_service_reuse_with_dispose().Example();
        new Own_the_resolution_scope_disposal().Example();
        new Use_parent_reuse().Example();
        new Reuse_lifespan_mismatch_detection().Example();
        new Reuse_lifespan_mismatch_error_suppress().Example();
        new Avoiding_reuse_lifespan_mismatch_for_Func_or_Lazy_dependency().Example();
        return 20;
    }
}

public class RulesAndDefaultConventions : ITest
{
    public int Run()
    {
        new Register_many_implementation().Example();
        new Register_many_implementation_with_default_if_already_registered_behavior().Example();
        new Register_with_ifAlreadyReplaced_option().Example();
        new Select_last_registered_service().Example();
        new AsResolutionCall_setup().Example();
        new Filter_out_service_that_do_not_have_a_matching_scope().Example();
        new Turning_matching_scope_filtering_Off().Example();
        new Select_last_registered_factory_with_implicit_scope_selection().Example();
        new Automatically_injected_container_interfaces().Example();
        new Registering_container_interfaces_by_hand().Example();
        new Registering_container_interfaces_by_hand().Example_injecting_all_container_interfaces_without_registering_them();
        new Constructor_with_resolvable_arguments().Example();
        new Using_specific_ctor_or_factory_method().Example();
        new Specify_how_to_treat_unresolved_parameter_or_property().Example();
        new Using_factory_selector_to_change_the_default_preferred_service().Example();
        new ResolveMany_does_not_work_WithUnknownResolvers().Example_not_working();
        new ResolveMany_does_not_work_WithUnknownResolvers().Example_working();
        new Auto_register_unknown_service().Example();
        new Auto_concrete_dynamic_type_registrations().Example();
        new Throw_if_dependency_has_a_shorter_lifetime().Example();
        new Disable_captive_dependency_exception().Example();
        new Wrap_captive_dependency_in_Func().Example();
        new Register_disposable_transient().Example();
        new Silence_registering_disposable_transient_exception().Example();
        new Default_IfAlreadyRegistered().Example();
        new Default_IfAlreadyRegistered_AppendNotKeyed().Example();
        return 26;
    }
}

public class SelectConstructorOrFactoryMethod : ITest
{
    public int Run()
    {
        new Register_strongly_typed_service_with_expression().Example();
        new Register_with_reflection().Example();
        new Register_open_generics_with_reflection().Example();
        new Register_with_automatic_constructor_selection().Example();
        new Register_with_automatic_constructor_selection_for_entire_container().Example();
        new Register_with_static_factory_method().Example();
        new Select_factory_method_based_on_condition().Example();
        new Register_with_instance_factory_method().Example();
        new Register_with_instance_property().Example();
        new Register_open_generics().Example();
        new Register_open_generics_with_MefAttributedModel_extension().Example();
        return 11;
    }
}

public class SpecifyDependencyAndPrimitiveValues : ITest
{
    public int Run()
    {
        new Resolving_with_a_service_type().Example();
        new Fail_to_resolve_from_the_multiple_registered_services().Example();
        new Using_the_enum_service_key().Example();
        new Using_the_enum_service_key_and_parameter_specification().Example();
        new Specifying_IfUnresolved_for_the_parameter().Example();
        new Specifying_IfUnresolved_for_the_property().Example();
        new Specifying_the_default_value_for_the_unresolved_parameter().Example();
        new Respecting_the_csharp_optional_arguments().Example();
        new Injecting_the_value_of_a_primitive_type().Example_via_RegisterInstance();
        new Injecting_the_value_of_a_primitive_type().Example_via_RegisterInstance_and_ServiceKey();
        new Injecting_the_value_of_a_primitive_type().Example_via_strongly_typed_spec();
        new Injecting_the_value_of_a_primitive_type().Example_via_strongly_typed_spec_and_direct_argument_spec();
        new Injecting_the_value_of_a_primitive_type().Example_via_RegisterDelegate();
        new Injecting_the_custom_value_depending_on_context().Example();
        new Injecting_the_custom_value_with_condition_setup().Example();
        new Full_spec_with_reflection().Example();
        new The_spec_with_strongly_typed_Made().Example();
        new The_spec_Parameters().Example();
        new The_spec_chain().Example();
        new Match_the_parameter_name_to_the_service_key().Problem();
        new Match_the_parameter_name_to_the_service_key().Solution();
        new Match_the_parameter_name_to_the_service_key().Solution_drop_MadeOf_part();
        new Match_the_parameter_name_to_the_service_key().Solution_matching_all_registration_parameters();
        new Match_the_parameter_name_to_the_service_key().Solution_with_strongly_typed_parameters();
        new Match_the_parameter_name_to_the_service_key().Solution_with_the_rule_applied_on_container_level();
        return 25;
    }
}

public class ThreadSafety : ITest
{
    public int Run()
    {
        new Resolving_singleton_in_parallel().Example();
        return 1;
    }
}

public class UsingInTestsWithMockingLibrary : ITest
{
    public int Run()
    {
        new NSubstitute_example().Example();
        new NSubstitute_example_with_singleton_mocks().Example();
        new NSubstitute_example_with_singleton_mocks().Example_of_mocking_the_open_generic_dependency();
        new Moq_example_with_test_container().Example();
        return 4;
    }
}

public class Wrappers : ITest
{
    public int Run()
    {
        new Wrapper_example().Example();
        new Lazy_and_Func_require_services_to_be_registered().Example();
        new Func_works_without_registration().Example();
        new Passed_argument_was_not_used().Example();
        new Func_with_args_and_reuse().Example();
        new Func_with_args_with_rule_ignoring_reuse().Example();
        new Func_with_single_argument_to_resolve_service_by_key().Example();
        new Providing_metadata().Example();
        new Filtering_based_on_metadata().Example();
        new Resolve_value_out_of_metadata_dictionary().Example();
        new Collection_of_Lazy_things().Example();
        new Filtering_not_resolved_services().Example();
        new Collection_with_custom_order().Example();
        new Both_open_and_closed_generic_included_in_collection().Example();
        new Covariant_generics_collection().Example();
        new Covariant_generics_collection_suppressed().Example();
        new DryIoc_composite_pattern().Example();
        new Prefer_composite_when_resolving_a_single_service().Example();
        new LazyEnumerable_example().Example();
        new Specify_LazyEnumerable_per_dependency().Example();
        new Specify_to_use_LazyEnumerable_for_all_IEnumerable().Example();
        new Dictionary_of_services_with_their_keys().Example();
        new Resolve_expression().Example();
        new Swap_container_in_factory_delegate().Example();
        new User_defined_wrappers().Example();
        new Non_generic_wrapper().Example();
        new Non_generic_wrapper().Example_with_closed_generic_wrapper();
        return 27;
    }
}

public class MefAttributedModel : ITest
{
    public int Run()
    {
        new Basic_example().Example();
        new Export_and_Import_used_separately().Example();
        new Export_example().Example();
        new Using_InheritedExport().Example();
        new DryIocAttributes_ExportMany().Example();
        new ExportMany_with_Except_and_NonPublic_options().Example();
        new Import_specification().Example();
        new Exporting_disposable_transient().Example();
        new Exporting_with_TrackDisposableTransient().Example();
        return 9;
    }
}