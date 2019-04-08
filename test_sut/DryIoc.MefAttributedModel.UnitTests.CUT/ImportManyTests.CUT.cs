using System.Collections.Generic;
using System.ComponentModel.Composition;
using DryIocAttributes;

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


    public interface IDep { }

    [Export(typeof(IDep))]
    [WithMetadata("---")]
    public class SomeDep : IDep { }

    [Export]
    public class RequiresManyOfType
    {
        public IEnumerable<IDep> Deps { get; private set; }

        public RequiresManyOfType([ImportMany(typeof(IDep))]IEnumerable<IDep> deps)
        {
            Deps = deps;
        }
    }

    [Export("blah", typeof(IDep))]
    [WithMetadata("dep")]
    public class BlahDep : IDep { }

    [Export("huh", typeof(IDep))]
    [WithMetadata("dep")]
    public class HuhDep : IDep { }

    [Export]
    public class RequiresManyOfName
    {
        public IEnumerable<IDep> Deps { get; private set; }

        public RequiresManyOfName([ImportMany("blah")]IEnumerable<IDep> deps)
        {
            Deps = deps;
        }
    }

    [Export]
    public class RequiresManyOfMeta
    {
        public IEnumerable<IDep> Deps { get; private set; }

        public RequiresManyOfMeta([ImportMany][WithMetadata("dep")]IEnumerable<IDep> deps)
        {
            Deps = deps;
        }
    }
}
