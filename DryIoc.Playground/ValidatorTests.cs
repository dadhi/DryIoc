/*
 * Unobtrusive Validator for model/view-model designed to work with nested models hierarchy. 
 * Only requirement for models is implementing IValidated interface which should create Validator.
 * Supports enabling/disabling of validation and automatic tracking of nested model reassignment.
 * 
 *  v4.2.1
 * - fixed: Enabled property changed raised only when Enabled value changed.
 * 
 * v4.2.0
 * - added: Property changed notification about Enabled changed.
 * 
 * v4.1.0
 * - fixed: CanChangeEnabled to be more flexible and take all available information pieces about how Enabled was changed and changing. To allow
 *          cases such as disabling property enabled by parent.
 * 
 * v4.0.0
 * - changed: Made Validator implement IEnumerable<KeyValuePair<string, Validator.GetValidationErrorOrNull>>.
 * - changed: Enabled collection initializer on Validator to Add validation rules without specifying Dictionary type.
 * 
 * v3.0.0
 * - changed: To convention based property validators discovery and binding.
 * - changed: To automatic model PropertyChanged discovery instead of IValidated.InvalidateBindings.
 * - changed: No need for Validator.ID. Using its property name in parent validator.
 * - changed: No dependency on ReflectionTools.GetPropertyName.
 * 
 * v2.2.0
 * - changed: Replaced enable change policy with CanProceedEnabledChanging handler for more flexibility.
 * 
 * v2.1.0
 * - added: Validator policy how to set enabled on properties.
 * 
 * v2.0.0
 * - changed: Added model bindings invalidation. 
 * 
 * v1.1.1
 * - fixed: Enabled change is not propagated into property validators.
 * 
 * v1.1.0:
 * - improved: Optimized case when property with validator reassigned to itself. That will prevent unnecessary re-binding.
 * 
 * v1.0.0
 * - Initial release.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace Validation
{
    [TestFixture]
    public class ValidatorTests
    {
        [Test]
        public void When_SubModel_validated_by_UI_Then_RootModel_should_reflect_that()
        {
            var root = new RootModel();
            Assert.That(root.Validator.IsValid, Is.False);
            Assert.That(root.Validator.Errors["SubModel"], Is.EqualTo("Invalid"));

            root.SubModel.StringSetting = "Not empty value";
            Assert.That(root.Validator.IsValid, Is.True);
            Assert.That(root.Validator.Errors["SubModel"], Is.Null);
        }

        [Test]
        public void SubModel_notifies_Root_validator_only_when_validity_changed()
        {
            var root = new RootModel();
            Assert.That(root.Validator.IsValid, Is.False);

            var notified = false;
            var validator = root.Validator;
            validator.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "IsValid")
                    notified = true;
            };

            root.SubModel.NumberSetting = -1;
            Assert.That(root.Validator.IsValid, Is.False);
            Assert.That(notified, Is.False);

            root.SubModel.NumberSetting = 2;
            root.SubModel.StringSetting = "!";

            Assert.That(root.Validator.IsValid, Is.True);
            Assert.That(notified, Is.True);
        }

        [Test]
        public void Should_notify_when_property_with_validator_is_reassigned()
        {
            var root = new RootModel();
            Assert.That(root.Validator.IsValid, Is.False);

            var notified = false;
            root.Validator.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "IsValid")
                    notified = true;
            };

            root.SubModel = new SubModel { StringSetting = "!" };

            Assert.That(notified, Is.True);
            Assert.That(root.Validator.IsValid, Is.True);
        }

        [Test]
        public void No_memory_leak_when_property_with_validator_is_reassigned()
        {
            var root = new RootModel();
            var rootWeakRef = new WeakReference(root);

            var oldSubModel = root.SubModel;
            root.SubModel = new SubModel { StringSetting = "!" };

            root = null;
            GC.Collect(GC.MaxGeneration);

            Assert.That(rootWeakRef.IsAlive, Is.False);
            GC.KeepAlive(oldSubModel);
        }

        [Test]
        public void Disabling_validation_should_make_model_valid()
        {
            var root = new RootModel();
            Assert.That(root.Validator.IsValid, Is.False);

            root.Validator.Enabled = false;
            Assert.That(root.Validator.IsValid, Is.True);
        }

        [Test]
        public void Enabling_validation_should_revalidate_model()
        {
            var root = new RootModel();
            root.Validator.Enabled = false;
            Assert.That(root.Validator.IsValid, Is.True);

            root.Validator.Enabled = true;
            Assert.That(root.Validator.IsValid, Is.False);
        }

        [Test]
        public void Disabling_validation_for_root_should_disable_sub_models()
        {
            var root = new RootModel();
            root.Validator.Enabled = false;

            Assert.That(root.SubModel.Validator.Enabled, Is.False);
        }

        [Test]
        public void Disabling_validation_should_notify_That_all_model_properties_changed_In_order_to_reset_UI()
        {
            var root = new RootModel();
            var invalidated = false;
            root.PropertyChanged += (sender, e) =>
                invalidated = string.IsNullOrEmpty(e.PropertyName);

            root.Validator.Enabled = false;

            Assert.That(invalidated, Is.True);
        }

        [Test]
        public void Disabled_parent_should_prevent_enabling_of_property_validation()
        {
            var root = new RootModel();
            root.Validator.Enabled = false;                         // disable parent validation
            Assert.That(root.SubModel.Validator.Enabled, Is.False); // check that property validation disabled too

            root.SubModel.Validator.Enabled = true;                 // enable root validation

            Assert.That(root.SubModel.Validator.Enabled, Is.False); // check that property enabled too 
        }

        [Test]
        public void Only_parent_could_enable_property_validation_disabled_by_parent()
        {
            var root = new RootModel();
            root.Validator.Enabled = false;                         // disable root validation
            root.SubModel.Validator.Enabled = true;                 // then try to enable property validation
            Assert.That(root.SubModel.Validator.Enabled, Is.False); // check that enabling property is failed, prevented by parent.

            root.Validator.Enabled = true;                          // enable root validation

            Assert.That(root.SubModel.Validator.Enabled, Is.True);  // check that property enabled too 
        }

        [Test]
        public void Only_parent_could_enable_property_validation_disabled_by_parent_After_it_was_disabled_by_property()
        {
            var root = new RootModel();
            root.SubModel.Validator.Enabled = false;                // disable property validation
            root.Validator.Enabled = false;                         // disable root validation, for property it changes nothing cause it was already disabled 

            root.SubModel.Validator.Enabled = true;                 // try to enable property validation

            Assert.That(root.SubModel.Validator.Enabled, Is.False); // check that it is prevented, cause root is still disabled
        }

        [Test]
        public void Disabling_property_enabled_by_parent_should_work()
        {
            var root = new RootModel();
            root.Validator.Enabled = true;                          // enable root validation, no effect cause it was enabled by default

            root.SubModel.Validator.Enabled = false;                // disable property validation

            Assert.That(root.SubModel.Validator.Enabled, Is.False); // check that property validation is successfully disabled
        }

        [Test]
        public void Notifying_on_all_model_property_changes_should_work_for_PropertyChanged_implemented_in_base_model_class()
        {
            var concrete = new Concrete();

            var notified = false;
            concrete.PropertyChanged += (sender, args) => notified = true;

            concrete.Count = -1;

            Assert.That(notified, Is.True);
        }
    }

    #region CUT

    public class Concrete : PropertyChangedBase, IValidated
    {
        private int _count;
        public Validator Validator { get; private set; }

        public Concrete()
        {
            Validator = new Validator(this)
            {
                { "Count", () => Count < 0 ? "Count should be positive." : null }
            };
            Validator.ValidateAll();
        }

        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }
    }

    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RootModel : IDataErrorInfo, IValidated, INotifyPropertyChanged
    {
        public Validator Validator { get; private set; }

        private SubModel _subModel;
        public SubModel SubModel
        {
            get { return _subModel; }
            set
            {
                _subModel = value;
                OnPropertyChanged("SubModel");
            }
        }

        public RootModel()
        {
            SubModel = new SubModel();

            Validator = new Validator(this);
            Validator.ValidateAll(); // call it for initial validation if required
        }

        #region IDataErrorInfo

        public string this[string propertyName]
        {
            get { return Validator.Validate(propertyName); }
        }

        public string Error
        {
            get { return null; }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class SubModel : INotifyPropertyChanged, IDataErrorInfo, IValidated
    {
        public Validator Validator { get; private set; }

        private string _stringSetting;
        public string StringSetting
        {
            get { return _stringSetting; }
            set
            {
                _stringSetting = value;
                var error = this["StringSetting"]; // emulating UI validation
            }
        }

        private int _numberSetting;
        public int NumberSetting
        {
            get { return _numberSetting; }
            set
            {
                _numberSetting = value;
                var error = this["NumberSetting"]; // emulating UI validation
            }
        }

        public SubModel()
        {
            Validator = new Validator(this)
            {
                {this.GetPropertyName(x => StringSetting), () => string.IsNullOrEmpty(StringSetting) ? "Should be non empty." : null},
                {this.GetPropertyName(x => NumberSetting), () => NumberSetting < 0 ? "Should be 0 or greater." : null}
            };
        }

        #region IDataErroInfo

        public string this[string propertyName]
        {
            get { return Validator.Validate(propertyName); }
        }

        public string Error
        {
            get { return null; }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #endregion

    public interface IValidated
    {
        Validator Validator { get; }
    }

    public sealed class Validator : INotifyPropertyChanged, IDisposable, IEnumerable<KeyValuePair<string, Validator.GetValidationErrorOrNull>>
    {
        // These two properties could be used for binding to UI.
        public bool IsValid { get; private set; }
        public Dictionary<string, string> Errors { get; private set; }

        public bool Enabled
        {
            get { return _enabled; }
            set { Enable(value); }
        }

        public delegate bool CanChangeEnabled(bool enabled, bool changedByParent, bool enabling, bool changingByParent);
        public static CanChangeEnabled CanChangeEnabledDefault = PreventEnablingIfDisabledByParent;

        public Validator(
            INotifyPropertyChanged model,
            bool enabledInitially = true,
            CanChangeEnabled canChangeEnabled = null)
        {
            if (model == null) throw new ArgumentNullException("model");

            _model = new WeakReference(model);
            _propertyValidationRules = new Dictionary<string, GetValidationErrorOrNull>();
            _enabled = enabledInitially;
            _canChangeEnabled = canChangeEnabled ?? CanChangeEnabledDefault;

            IsValid = true;
            Errors = new Dictionary<string, string>();

            BindValidatedProperties(model);
        }

        public delegate string GetValidationErrorOrNull();
        public void Add(string propertyName, GetValidationErrorOrNull validationRule)
        {
            _propertyValidationRules[propertyName] = validationRule;
        }

        public string Validate(string propertyName)
        {
            if (!_enabled ||
                !_propertyValidationRules.ContainsKey(propertyName))
                return null;

            var error = _propertyValidationRules[propertyName].Invoke();
            OnPropertyValidated(propertyName, error);
            return error;
        }

        public void ValidateAll()
        {
            if (!_enabled)
                return;

            if (_subscribedPropertyValidators != null)
                foreach (var validator in _subscribedPropertyValidators)
                    validator.Value.ValidateAll();

            if (_propertyValidationRules != null)
                foreach (var validator in _propertyValidationRules)
                    OnPropertyValidated(validator.Key, validator.Value.Invoke(), notifyIfSameValidity: true);

            NotifyAllModelPropertiesChanged();
        }

        #region IDisposable

        public void Dispose()
        {
            if (_subscribedPropertyValidators != null)
                foreach (var validator in _subscribedPropertyValidators)
                    validator.Value.PropertyChanged -= OnPropertyValidatorValidityChanged;

            _validatedPropertySelectors = null;
            _subscribedPropertyValidators = null;
            _propertyValidationRules = null;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<KeyValuePair<string, GetValidationErrorOrNull>> GetEnumerator()
        {
            return _propertyValidationRules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation

        private readonly WeakReference _model;
        private readonly CanChangeEnabled _canChangeEnabled;

        private Dictionary<string, GetValidationErrorOrNull> _propertyValidationRules;
        private Dictionary<string, Func<object, IValidated>> _validatedPropertySelectors;
        private Dictionary<string, Validator> _subscribedPropertyValidators;
        private bool _enabled;
        private bool _enabledChangedByParent;

        private static bool PreventEnablingIfDisabledByParent(bool enabled, bool changedByParent, bool enabling, bool changingByParent)
        {
            if (!enabled && changedByParent && enabling && !changingByParent)
                return false;
            return true;
        }

        private void BindValidatedProperties(INotifyPropertyChanged model)
        {
            var validatedProperties = model.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => typeof(IValidated).IsAssignableFrom(p.PropertyType)).ToArray();

            if (validatedProperties.Length == 0)
                return;

            _validatedPropertySelectors = new Dictionary<string, Func<object, IValidated>>();
            _subscribedPropertyValidators = new Dictionary<string, Validator>();

            for (var i = 0; i < validatedProperties.Length; i++)
            {
                var propertyInfo = validatedProperties[i];
                var property = (IValidated)propertyInfo.GetValue(model, null);
                var propertyName = propertyInfo.Name;
                if (property != null && property.Validator != null)
                {
                    var validator = property.Validator;
                    validator.PropertyChanged += OnPropertyValidatorValidityChanged;
                    _subscribedPropertyValidators[propertyName] = validator;
                }

                _validatedPropertySelectors.Add(propertyName, m => (IValidated)propertyInfo.GetValue(m, null));
            }

            model.PropertyChanged += OnValidatedPropertyChanged;
        }

        private void OnValidatedPropertyChanged(object modelObject, PropertyChangedEventArgs e)
        {
            if (_validatedPropertySelectors == null)
                return;

            var propertyName = e.PropertyName;
            if (!string.IsNullOrEmpty(propertyName))
            {
                Func<object, IValidated> selectProperty;
                if (_validatedPropertySelectors.TryGetValue(propertyName, out selectProperty))
                    ReBindValidatedProperty(selectProperty(modelObject), propertyName);
            }
            else
            {
                foreach (var propertySelector in _validatedPropertySelectors)
                    ReBindValidatedProperty(propertySelector.Value(modelObject), propertySelector.Key);
            }
        }

        private void ReBindValidatedProperty(IValidated property, string propertyName)
        {
            if (property == null || property.Validator == null)
                return;

            var validator = property.Validator;

            Validator oldValidator;
            if (_subscribedPropertyValidators.TryGetValue(propertyName, out oldValidator) &&
                validator == oldValidator)
                return;

            if (oldValidator != null)
                oldValidator.PropertyChanged -= OnPropertyValidatorValidityChanged;

            validator.PropertyChanged += OnPropertyValidatorValidityChanged;
            _subscribedPropertyValidators[propertyName] = validator;

            validator.ValidateAll(); // enforce validation on property to notify parent model
        }

        private void OnPropertyValidatorValidityChanged(object validatorObject, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsValid")
                return;

            var validator = (Validator)validatorObject;
            var error = validator.IsValid ? null : "Invalid";
            var propertyName = _subscribedPropertyValidators.Where(x => x.Value == validator).Select(x => x.Key).FirstOrDefault();
            if (propertyName == null)
                throw new InvalidOperationException("Unknown property validator we never subscribed for.");

            OnPropertyValidated(propertyName, error);
        }

        private void OnPropertyValidated(string propertyName, string error, bool notifyIfSameValidity = false)
        {
            if (!_enabled)
                return;

            var errorsChanged = false;
            string oldError;
            if (!Errors.TryGetValue(propertyName, out oldError) || error != oldError)
            {
                errorsChanged = true;
                Errors[propertyName] = error;
                OnPropertyChanged("Errors");
            }

            var oldValidity = IsValid;
            if (errorsChanged)
                IsValid = Errors.All(e => string.IsNullOrEmpty(e.Value));
            if (IsValid != oldValidity || notifyIfSameValidity)
                OnPropertyChanged("IsValid");
        }

        private void Enable(bool enabling, bool changingByParent = false)
        {
            if (!_canChangeEnabled(_enabled, _enabledChangedByParent, enabling, changingByParent))
                return;

            _enabledChangedByParent = changingByParent;

            var enabled = _enabled;
            _enabled = enabling;

            // first changed enabled for all validated properties
            if (enabled != enabling && _subscribedPropertyValidators != null)
                foreach (var validator in _subscribedPropertyValidators)
                    validator.Value.Enable(enabling, changingByParent: true);

            if (enabled && !enabling)
            {
                if (!IsValid)
                {
                    Errors.Clear();
                    OnPropertyChanged("Errors");
                    
                    IsValid = true;
                    OnPropertyChanged("IsValid");

                    NotifyAllModelPropertiesChanged();
                }
            }
            else if (!enabled && enabling)
            {
                ValidateAll();
            }

            if (enabled != !enabling)
                OnPropertyChanged("Enabled");
        }

        private void NotifyAllModelPropertiesChanged()
        {
            var model = _model.Target as INotifyPropertyChanged;
            if (model != null)
                model.RaisePropertyChanged(string.Empty);
        }

        #endregion
    }

    public static class ReflectionTools
    {
        public static void RaisePropertyChanged(this INotifyPropertyChanged source, string propertyName)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            FieldInfo propertyChanged = null;
            for (var modelType = source.GetType(); modelType != null && propertyChanged == null; modelType = modelType.BaseType)
                propertyChanged = modelType.GetField("PropertyChanged",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

            if (propertyChanged == null)
                throw new InvalidOperationException("Unable to find PropertyChanged event in model and its base types.");

            var handler = (MulticastDelegate)propertyChanged.GetValue(source);
            if (handler == null) // There are no subscribers to event yet.
                return;

            var subscribers = handler.GetInvocationList();
            var eventArgs = new object[] { source, new PropertyChangedEventArgs(propertyName) };
            for (var i = 0; i < subscribers.Length; i++)
                subscribers[i].Method.Invoke(subscribers[i].Target, eventArgs);
        }

        public static string GetPropertyName<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> propertySelector)
        {
            var expr = propertySelector.ToString();
            var exprParts = expr.Split(new[] { "=>" }, StringSplitOptions.None);
            if (exprParts.Length != 2)
                throw new InvalidOperationException("Unable to get property from expression: " + expr);
            var propertyAccessor = exprParts[1].Trim();
            var propertyAccessorParts = propertyAccessor.Split('.');
            if (propertyAccessorParts.Length < 2)
                throw new InvalidOperationException("Unable to get property from expression: " + expr);
            var propertyName = propertyAccessorParts.Last();
            return propertyName.Trim();
        }
    }
}
