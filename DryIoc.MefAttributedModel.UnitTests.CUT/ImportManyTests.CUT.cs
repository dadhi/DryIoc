using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    public interface IPasswordHasher
    {
    }

    [Export(typeof(IPasswordHasher))]
    public class BCryptPasswordHasher : IPasswordHasher
    {
    }

    [Export(typeof(IPasswordHasher))]
    public class SCryptPasswordHasher : IPasswordHasher
    {
    }

    [Export(typeof(IPasswordHasher))]
    public class Rfc2898PasswordHasher : IPasswordHasher
    {
    }

    [Export]
    public class PasswordVerifier1 : IPartImportsSatisfiedNotification
    {
        [ImportingConstructor]
        public PasswordVerifier1([ImportMany]IPasswordHasher[] hashers)
        {
            Hashers = hashers;
        }

        public IPasswordHasher[] Hashers { get; set; }

        public void OnImportsSatisfied()
        {
            ImportsSatisfied = true;
        }

        public bool ImportsSatisfied { get; set; }
    }

    [Export]
    public class PasswordVerifier2 : IPartImportsSatisfiedNotification
    {
        [ImportingConstructor]
        public PasswordVerifier2([ImportMany]IEnumerable<IPasswordHasher> hashers)
        {
            Hashers = hashers;
        }

        public IEnumerable<IPasswordHasher> Hashers { get; set; }

        public void OnImportsSatisfied()
        {
            ImportsSatisfied = true;
        }

        public bool ImportsSatisfied { get; set; }
    }

    [Export]
    public class PasswordVerifier3 : IPartImportsSatisfiedNotification
    {
        [ImportMany]
        public IPasswordHasher[] Hashers { get; set; }

        public void OnImportsSatisfied()
        {
            ImportsSatisfied = true;
        }

        public bool ImportsSatisfied { get; set; }
    }

    [Export]
    public class PasswordVerifier4 : IPartImportsSatisfiedNotification
    {
        [ImportMany]
        public IEnumerable<IPasswordHasher> Hashers { get; set; }

        public void OnImportsSatisfied()
        {
            ImportsSatisfied = true;
        }

        public bool ImportsSatisfied { get; set; }
    }
}
