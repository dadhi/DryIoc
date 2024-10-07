using DryIoc.ImTools;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Linq;
using UsingExample;
using DryIoc.FastExpressionCompiler.LightExpression;
using System.Text;

namespace DryIoc.IssuesTests;

[TestFixture]
public sealed class GHIssue503_Compile_time_container : ITest
{
    public int Run()
    {
        Generate_the_full_compile_time_container_code();
        return 1;
    }

    [Test]
    public void Generate_the_full_compile_time_container_code()
    {
        var c = new Container();

        var container = MyCompileTimeDI.RegisterInContainer().With(rules => rules.WithExpressionGenerationSettingsOnly());

        var result = container.GenerateResolutionExpressions(x => x.SelectMany(r =>
            MyCompileTimeDI.SpecifyResolutionRoots(r).EmptyIfNull()).Concat(MyCompileTimeDI.CustomResolutionRoots.EmptyIfNull()));

        string TrimUsings(string source)
        {
            source = source.Replace("DryIoc.", "");
            // todo: @wip remove unnecessary usings that's are System.Collections.Generic for KeyValuePair, etc.
            foreach (var x in MyCompileTimeDI.NamespaceUsings)
                source = source.Replace(x + ".", "");
            return source;
        }

        string Code(object x, int lineIdent = 0) =>
            x == null ? "null" :
            x is Expression e ? TrimUsings(e.ToCSharpString(new StringBuilder(), lineIdent).ToString()) :
            x is Request r ? Code(container.GetRequestExpression(r), lineIdent) :
            Code(container.GetConstantExpression(x, x.GetType(), true), lineIdent);

        // without `typeof`
        string TypeOnlyCode(Type type) => TrimUsings(type.ToCode(printGenericTypeArgs: true));

        string GetTypeNameOnly(string typeName) => typeName.Split('`').First().Split('.').Last();

        string CommaOptArg(string arg) => arg == "null" ? "" : ", " + arg;

        int getServiceBodyLineIdent = 16;

        var rootCodes = result.Roots.Select((r, i) =>
            new
            {
                ServiceType = r.Key.ServiceType,
                ServiceTypeCode = Code(r.Key.ServiceType),
                ServiceTypeOnlyCode = TypeOnlyCode(r.Key.ServiceType),
                ServiceKeyCode = Code(r.Key.ServiceKey),
                RequiredServiceTypeCode = Code(r.Key.Details.RequiredServiceType),
                ExpressionCode = Code(r.Value.Body, getServiceBodyLineIdent),
                CreateMethodName = "Get_" + GetTypeNameOnly(r.Key.ServiceType.Name) + "_" + i
            });

        var depCodes = result.ResolveDependencies.Select((r, i) =>
            new
            {
                ServiceType = Code(r.Key.ServiceType),
                ServiceTypeOnly = TypeOnlyCode(r.Key.ServiceType),
                ServiceKey = Code(r.Key.ServiceKey),
                ServiceKeyObject = r.Key.ServiceKey,
                Expression = Code(r.Value, getServiceBodyLineIdent),
                ExpressionObject = r.Value,
                RequiredServiceType = Code(r.Key.RequiredServiceType),
                PreResolveParent = Code(r.Key.Parent, getServiceBodyLineIdent + 8),
                PreResolveParentObject = r.Key.Parent,
                CreateMethodName = "GetDependency_" + GetTypeNameOnly(r.Key.ServiceType.Name) + "_" + i
            })
            .ToList();

        var includeVariants = container.Rules.VariantGenericTypesInResolvedCollection;
    }
}
