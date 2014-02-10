using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class ModelTreeValidationTests
    {
        [Test]
        public void When_SubModel_validated_by_UI_Then_RoorModel_should_reflect_that()
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

            var notified = false;
            root.Validator.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "IsValid")
                    notified = true;
            };

            root.SubModel = new SubModel() { StringSetting = "!" };

            Assert.That(notified, Is.True);
        }

        [Test]
        public void No_memory_leak_when_property_with_validator_is_reassigned()
        {
            
        }
    }

    public class RootModel : IDataErrorInfo, IValidated, INotifyPropertyChanged
    {
        public ModelValidator Validator { get; private set; }

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

            Validator = new ModelValidator("Root");
            Validator.BindPropertyValidators(this, x => x.SubModel);
            Validator.ValidateAll(); // make initial validation if required
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

    public class SubModel : IDataErrorInfo, IValidated
    {
        public ModelValidator Validator { get; private set; }

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
            Validator = new ModelValidator("Sub", new Dictionary<string, Func<string>>
            {
                { "StringSetting", () => string.IsNullOrEmpty(StringSetting) ? "Should be non empty." : null},
                { "NumberSetting", () => NumberSetting < 0 ? "Should be 0 or greater." : null }
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
    }

    public interface IValidated
    {
        ModelValidator Validator { get; }
    }

    public class ModelValidator : INotifyPropertyChanged, IDisposable
    {
        public bool IsValid { get; private set; }
        public Dictionary<string, string> Errors { get; private set; }

        public Dictionary<string, Func<string>> PropertyValidationRules
        {
            get { return _propertyValidationRules; }
        }

        public readonly string ID;

        private Dictionary<string, Func<string>> _propertyValidationRules;
        private Dictionary<string, Func<object, IValidated>> _propertySelectors;
        private Dictionary<string, ModelValidator> _propertyValidators;

        public ModelValidator(string id, Dictionary<string, Func<string>> propertyValidationRules = null)
        {
            if (string.IsNullOrEmpty("id")) throw new ArgumentNullException("id");
            ID = id;
            _propertyValidationRules = propertyValidationRules;
            Errors = new Dictionary<string, string>();
            IsValid = true;
        }

        public void BindPropertyValidators<TModel>(TModel model,
            params Expression<Func<TModel, IValidated>>[] propertySelectors)
            where TModel : INotifyPropertyChanged
        {
            _propertySelectors = new Dictionary<string, Func<object, IValidated>>();
            _propertyValidators = new Dictionary<string, ModelValidator>();

            foreach (var propertySelector in propertySelectors)
            {
                var getProperty = propertySelector.Compile();
                var property = getProperty(model);
                if (property != null && property.Validator != null)
                {
                    var validator = property.Validator;
                    validator.PropertyChanged += OnPropertyValidated;
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

            model.PropertyChanged += UpdateSubModelValidators;
        }

        private void UpdateSubModelValidators(object modelObject, PropertyChangedEventArgs e)
        {
            if (_propertySelectors == null)
                return;

            var propertyName = e.PropertyName;
            if (!string.IsNullOrEmpty(propertyName))
            {
                Func<object, IValidated> selectProperty;
                if (_propertySelectors.TryGetValue(propertyName, out selectProperty))
                    ReBindProperty(selectProperty(modelObject));
            }
            else
            {
                foreach (var propertySelector in _propertySelectors)
                    ReBindProperty(propertySelector.Value(modelObject));
            }
        }

        public string Validate(string propertyName)
        {
            if (_propertyValidationRules == null ||
                !_propertyValidationRules.ContainsKey(propertyName))
                return null;

            var error = _propertyValidationRules[propertyName].Invoke();
            OnValidated(propertyName, error);
            return error;
        }

        public void ValidateAll()
        {
            if (_propertyValidators != null)
                foreach (var validator in _propertyValidators)
                    validator.Value.ValidateAll();

            if (_propertyValidationRules != null)
                foreach (var validator in _propertyValidationRules)
                    OnValidated(validator.Key, validator.Value.Invoke(), IfSameValidity.NotifyAnyway);
        }

        public void Dispose()
        {
            if (_propertyValidators != null)
                foreach (var validator in _propertyValidators)
                    validator.Value.PropertyChanged -= OnPropertyValidated;
            
            _propertySelectors = null;
            _propertyValidators = null;
            _propertyValidationRules = null;
        }

        #region Implementation

        enum IfSameValidity { DontNotify, NotifyAnyway }

        private void OnValidated(string propertyName, string error,
            IfSameValidity ifSameValidity = IfSameValidity.DontNotify)
        {
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

        private void ReBindProperty(IValidated property)
        {
            if (property == null || property.Validator == null) 
                return;

            var validatorID = property.Validator.ID;
            ModelValidator oldValidator;
            if (_propertyValidators.TryGetValue(validatorID, out oldValidator))
                oldValidator.PropertyChanged -= OnPropertyValidated;

            var validator = property.Validator;
            validator.PropertyChanged += OnPropertyValidated;
            _propertyValidators[validatorID] = validator;
            
            validator.ValidateAll(); // enforce validation on property to notify parent model
        }

        private void OnPropertyValidated(object validatorObject, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                var validator = (ModelValidator)validatorObject;
                var error = validator.IsValid ? null : "Invalid";
                OnValidated(validator.ID, error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public static class ModelTools
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
            var propertyName = propertyAccessorParts.Last().Trim();
            return propertyName;
        }
    }
}
