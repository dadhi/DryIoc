/*
 * Unabtrusive Validator for model/view-model designed to work with nested models hierarchy. 
 * Only requirement for models is implementing simple IValidated interface.
 * Supports enabling/disabling of validation and automatic tracking of nested model reassignements.
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
 * - improved: Optimized case when property with validator reassigned to itself. That will prevent uneccessary re-binding.
 * 
 * v1.0.0
 * - Initial release.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
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
            Assert.That(root.Validator.Errors["Sub"], Is.EqualTo("Invalid"));

            root.SubModel.StringSetting = "Not empty value";
            Assert.That(root.Validator.IsValid, Is.True);
            Assert.That(root.Validator.Errors["Sub"], Is.Null);
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
        public void Disabling_validation_should_invalidate_model_bindings()
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
            root.Validator.Enabled = false;
            Assert.That(root.SubModel.Validator.Enabled, Is.False);

            root.SubModel.Validator.Enabled = true;
            Assert.That(root.SubModel.Validator.Enabled, Is.False);
        }

        [Test]
        public void Only_parent_could_enable_property_validation_disabled_by_parent()
        {
            var root = new RootModel();
            root.Validator.Enabled = false;
            root.SubModel.Validator.Enabled = true;
            Assert.That(root.SubModel.Validator.Enabled, Is.False);

            root.Validator.Enabled = true;
            Assert.That(root.SubModel.Validator.Enabled, Is.True);
        }
    }

    #region CUT

    public class RootModel : IDataErrorInfo, IValidated, INotifyPropertyChanged
    {
        public Validator Validator { get; private set; }

        public void InvalidateBindings()
        {
            OnPropertyChanged(string.Empty);
        }

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

            Validator = new Validator(this, "Root");
            Validator.BindPropertyValidators(this, x => x.SubModel);
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

        public void InvalidateBindings()
        {
            OnPropertyChanged(string.Empty);
        }

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
            Validator = new Validator(this, "Sub", new Dictionary<string, Func<string>>
            {
                {this.GetPropertyName(x => StringSetting), () => string.IsNullOrEmpty(StringSetting) ? "Should be non empty." : null},
                {this.GetPropertyName(x => NumberSetting), () => NumberSetting < 0 ? "Should be 0 or greater." : null}
            });
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

        void InvalidateBindings();
    }

    public sealed class Validator : INotifyPropertyChanged, IDisposable
    {
        public readonly string ID;

        // These two properties could be used for binding to UI.
        public bool IsValid { get; private set; }
        public Dictionary<string, string> Errors { get; private set; }

        public bool Enabled
        {
            get { return _enabled; }
            set { Enable(value); }
        }

        public delegate bool CanProceedEnabledChanging(bool wasChangedByParent, bool nowChangedByParent);
        public static CanProceedEnabledChanging CanProceedEnabledChangingDefault = PreventDirectChangeIfChangedByParent;

        public Validator(
            IValidated model,
            string id,
            Dictionary<string, Func<string>> propertyValidationRules = null,
            bool enabledInitially = true,
            CanProceedEnabledChanging canProceedEnabledChanging = null)
        {
            if (string.IsNullOrEmpty("id")) throw new ArgumentNullException("id");
            ID = id;
            Errors = new Dictionary<string, string>();
            IsValid = true;

            _model = new WeakReference(model);
            _propertyValidationRules = propertyValidationRules;
            _enabled = enabledInitially;
            _canProceedEnabledChanging = canProceedEnabledChanging ?? CanProceedEnabledChangingDefault;
        }

        public void BindPropertyValidators<TModel>(TModel model,
            params Expression<Func<TModel, IValidated>>[] propertySelectors)
            where TModel : class, INotifyPropertyChanged
        {
            if (model == null) throw new ArgumentNullException("model");
            if (propertySelectors == null || propertySelectors.Length == 0)
                throw new ArgumentNullException("propertySelectors");

            _propertySelectors = new Dictionary<string, Func<object, IValidated>>();
            _propertyValidators = new Dictionary<string, Validator>();

            foreach (var propertySelector in propertySelectors)
            {
                var getProperty = propertySelector.Compile();
                var property = getProperty(model);
                if (property != null && property.Validator != null)
                {
                    var validator = property.Validator;
                    validator.PropertyChanged += OnModelPropertyValidated;
                    _propertyValidators[validator.ID] = validator;
                }

                var modelObjectParamExpr = Expression.Parameter(typeof(object), "model");
                var getModelPropertyExpr = Expression.Lambda<Func<object, IValidated>>(
                    Expression.Invoke(propertySelector, Expression.Convert(modelObjectParamExpr, typeof(TModel))),
                    modelObjectParamExpr);
                var getModelProperty = getModelPropertyExpr.Compile();
                var propertyName = model.GetPropertyName(propertySelector);
                _propertySelectors.Add(propertyName, getModelProperty);
            }

            model.PropertyChanged += OnModelPropertyChanged;
        }

        public string Validate(string propertyName)
        {
            if (!_enabled ||
                _propertyValidationRules == null ||
                !_propertyValidationRules.ContainsKey(propertyName))
                return null;

            var error = _propertyValidationRules[propertyName].Invoke();
            OnValidated(propertyName, error);
            return error;
        }

        public void ValidateAll()
        {
            if (!_enabled)
                return;

            if (_propertyValidators != null)
                foreach (var validator in _propertyValidators)
                    validator.Value.ValidateAll();

            if (_propertyValidationRules != null)
                foreach (var validator in _propertyValidationRules)
                    OnValidated(validator.Key, validator.Value.Invoke(), IfSameValidity.NotifyAnyway);

            InvalidateModelBindings();
        }

        public void Dispose()
        {
            if (_propertyValidators != null)
                foreach (var validator in _propertyValidators)
                    validator.Value.PropertyChanged -= OnModelPropertyValidated;

            _propertySelectors = null;
            _propertyValidators = null;
            _propertyValidationRules = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Implementation

        private readonly WeakReference _model;
        private readonly CanProceedEnabledChanging _canProceedEnabledChanging;

        private Dictionary<string, Func<string>> _propertyValidationRules;
        private Dictionary<string, Func<object, IValidated>> _propertySelectors;
        private Dictionary<string, Validator> _propertyValidators;
        private bool _enabled;
        private bool _enabledChangedByParent;

        private static bool PreventDirectChangeIfChangedByParent(bool wasChangedByParent, bool nowChangedByParent)
        {
            return !wasChangedByParent || nowChangedByParent;
        }

        enum IfSameValidity { DontNotify, NotifyAnyway }

        private void OnValidated(string propertyName, string error,
            IfSameValidity ifSameValidity = IfSameValidity.DontNotify)
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
            if (IsValid != oldValidity || ifSameValidity == IfSameValidity.NotifyAnyway)
                OnPropertyChanged("IsValid");
        }

        private void Enable(bool value, bool byParent = false)
        {
            if (!_canProceedEnabledChanging(_enabledChangedByParent, byParent))
                return;

            _enabledChangedByParent = byParent;

            var wasEnabled = _enabled;
            _enabled = value;

            if (wasEnabled && !_enabled)
            {
                if (!IsValid)
                {
                    Errors.Clear();
                    OnPropertyChanged("Errors");

                    IsValid = true;
                    OnPropertyChanged("IsValid");

                    InvalidateModelBindings();
                }
            }
            else if (!wasEnabled && _enabled)
            {
                ValidateAll();
            }

            if (wasEnabled != _enabled && _propertyValidators != null)
                foreach (var validator in _propertyValidators.Values)
                    validator.Enable(_enabled, byParent: true);
        }

        private void OnModelPropertyChanged(object modelObject, PropertyChangedEventArgs e)
        {
            if (_propertySelectors == null)
                return;

            var propertyName = e.PropertyName;
            if (!string.IsNullOrEmpty(propertyName))
            {
                Func<object, IValidated> selectProperty;
                if (_propertySelectors.TryGetValue(propertyName, out selectProperty))
                    ReBindModelProperty(selectProperty(modelObject));
            }
            else
            {
                foreach (var propertySelector in _propertySelectors)
                    ReBindModelProperty(propertySelector.Value(modelObject));
            }
        }

        private void ReBindModelProperty(IValidated property)
        {
            if (property == null || property.Validator == null)
                return;

            var validator = property.Validator;

            Validator oldValidator;
            if (_propertyValidators.TryGetValue(validator.ID, out oldValidator) &&
                validator == oldValidator)
                return;

            if (oldValidator != null)
                oldValidator.PropertyChanged -= OnModelPropertyValidated;

            validator.PropertyChanged += OnModelPropertyValidated;
            _propertyValidators[validator.ID] = validator;

            validator.ValidateAll(); // enforce validation on property to notify parent model
        }

        private void OnModelPropertyValidated(object validatorObject, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                var validator = (Validator)validatorObject;
                var error = validator.IsValid ? null : "Invalid";
                OnValidated(validator.ID, error);
            }
        }

        private void InvalidateModelBindings()
        {
            var model = _model.Target as IValidated;
            if (model != null)
                model.InvalidateBindings();
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public static class ReflectionTools
    {
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
