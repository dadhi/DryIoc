using System;
using System.Collections.Generic;
using System.Linq;
using FastExpressionCompiler.LightExpression;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue152_ExponentialMemoryPerformanceWithRegardsToTheObjectGraphSize
    {
        [Test]
        public void Main()
        {
            var c = new Container();

            c.Register<AggQ>(Reuse.InCurrentScope);
            c.Register<AggP>(Reuse.InCurrentScope);
            c.Register<Root>(Reuse.InCurrentScope);
            RegisterIn(c);

            using (var scope = c.OpenScope())
            {
                var rootExpr = scope.Resolve<LambdaExpression>(typeof(Root));
                var rootCode = rootExpr.CodeString;
                var nestedLambdas = rootCode.Count(ch => ch == '$');
                Assert.AreEqual(2603, nestedLambdas);
            }
        }

        [Test]
        public void Main_negative()
        {
            var c = new Container(rules => rules.WithoutDependencyDepthToSplitObjectGraph());
            c.Register<AggQ>(Reuse.InCurrentScope);
            c.Register<AggP>(Reuse.InCurrentScope);
            c.Register<Root>(Reuse.InCurrentScope);
            RegisterIn(c);

            using (var scope = c.OpenScope())
            {
                var rootExpr = scope.Resolve<LambdaExpression>(typeof(Root));
                var rootStr = rootExpr.ToString();
                var resolveCallIndex = rootStr.IndexOf("\"Resolve\"", StringComparison.InvariantCulture);
                Assert.AreEqual(-1, resolveCallIndex);
            }
        }

        public interface IP { }
        public interface IQ { }
        public class AggP { public AggP(IEnumerable<IP> ps) { } }
        public class AggQ { public AggQ(IEnumerable<IQ> qs) { } }
        public class Root { public Root(IEnumerable<AggQ> qs) { } }

        class P1 : IP { public P1() { } }
        class P2 : IP { public P2() { } }
        class P3 : IP { public P3() { } }
        class P4 : IP { public P4() { } }
        class P5 : IP { public P5() { } }
        class P6 : IP { public P6() { } }
        class P7 : IP { public P7() { } }
        class P8 : IP { public P8() { } }
        class P9 : IP { public P9() { } }
        class P10 : IP { public P10() { } }
        class P11 : IP { public P11() { } }
        class P12 : IP { public P12() { } }
        class P13 : IP { public P13() { } }
        class P14 : IP { public P14() { } }
        class P15 : IP { public P15() { } }
        class P16 : IP { public P16() { } }
        class P17 : IP { public P17() { } }
        class P18 : IP { public P18() { } }
        class P19 : IP { public P19() { } }
        class P20 : IP { public P20() { } }
        class P21 : IP { public P21() { } }
        class P22 : IP { public P22() { } }
        class P23 : IP { public P23() { } }
        class P24 : IP { public P24() { } }
        class P25 : IP { public P25() { } }
        class P26 : IP { public P26() { } }
        class P27 : IP { public P27() { } }
        class P28 : IP { public P28() { } }
        class P29 : IP { public P29() { } }
        class P30 : IP { public P30() { } }
        class P31 : IP { public P31() { } }
        class P32 : IP { public P32() { } }
        class P33 : IP { public P33() { } }
        class P34 : IP { public P34() { } }
        class P35 : IP { public P35() { } }
        class P36 : IP { public P36() { } }
        class P37 : IP { public P37() { } }
        class P38 : IP { public P38() { } }
        class P39 : IP { public P39() { } }
        class P40 : IP { public P40() { } }
        class P41 : IP { public P41() { } }
        class P42 : IP { public P42() { } }
        class P43 : IP { public P43() { } }
        class P44 : IP { public P44() { } }
        class P45 : IP { public P45() { } }
        class P46 : IP { public P46() { } }
        class P47 : IP { public P47() { } }
        class P48 : IP { public P48() { } }
        class P49 : IP { public P49() { } }
        class P50 : IP { public P50() { } }
        class Q1 : IQ { public Q1(AggP ps) { } }
        class Q2 : IQ { public Q2(AggP ps) { } }
        class Q3 : IQ { public Q3(AggP ps) { } }
        class Q4 : IQ { public Q4(AggP ps) { } }
        class Q5 : IQ { public Q5(AggP ps) { } }
        class Q6 : IQ { public Q6(AggP ps) { } }
        class Q7 : IQ { public Q7(AggP ps) { } }
        class Q8 : IQ { public Q8(AggP ps) { } }
        class Q9 : IQ { public Q9(AggP ps) { } }
        class Q10 : IQ { public Q10(AggP ps) { } }
        class Q11 : IQ { public Q11(AggP ps) { } }
        class Q12 : IQ { public Q12(AggP ps) { } }
        class Q13 : IQ { public Q13(AggP ps) { } }
        class Q14 : IQ { public Q14(AggP ps) { } }
        class Q15 : IQ { public Q15(AggP ps) { } }
        class Q16 : IQ { public Q16(AggP ps) { } }
        class Q17 : IQ { public Q17(AggP ps) { } }
        class Q18 : IQ { public Q18(AggP ps) { } }
        class Q19 : IQ { public Q19(AggP ps) { } }
        class Q20 : IQ { public Q20(AggP ps) { } }
        class Q21 : IQ { public Q21(AggP ps) { } }
        class Q22 : IQ { public Q22(AggP ps) { } }
        class Q23 : IQ { public Q23(AggP ps) { } }
        class Q24 : IQ { public Q24(AggP ps) { } }
        class Q25 : IQ { public Q25(AggP ps) { } }
        class Q26 : IQ { public Q26(AggP ps) { } }
        class Q27 : IQ { public Q27(AggP ps) { } }
        class Q28 : IQ { public Q28(AggP ps) { } }
        class Q29 : IQ { public Q29(AggP ps) { } }
        class Q30 : IQ { public Q30(AggP ps) { } }
        class Q31 : IQ { public Q31(AggP ps) { } }
        class Q32 : IQ { public Q32(AggP ps) { } }
        class Q33 : IQ { public Q33(AggP ps) { } }
        class Q34 : IQ { public Q34(AggP ps) { } }
        class Q35 : IQ { public Q35(AggP ps) { } }
        class Q36 : IQ { public Q36(AggP ps) { } }
        class Q37 : IQ { public Q37(AggP ps) { } }
        class Q38 : IQ { public Q38(AggP ps) { } }
        class Q39 : IQ { public Q39(AggP ps) { } }
        class Q40 : IQ { public Q40(AggP ps) { } }
        class Q41 : IQ { public Q41(AggP ps) { } }
        class Q42 : IQ { public Q42(AggP ps) { } }
        class Q43 : IQ { public Q43(AggP ps) { } }
        class Q44 : IQ { public Q44(AggP ps) { } }
        class Q45 : IQ { public Q45(AggP ps) { } }
        class Q46 : IQ { public Q46(AggP ps) { } }
        class Q47 : IQ { public Q47(AggP ps) { } }
        class Q48 : IQ { public Q48(AggP ps) { } }
        class Q49 : IQ { public Q49(AggP ps) { } }
        class Q50 : IQ { public Q50(AggP ps) { } }

        public static void RegisterIn(Container c)
        {
            c.Register<IP, P1>(Reuse.InCurrentScope);
            c.Register<IP, P2>(Reuse.InCurrentScope);
            c.Register<IP, P3>(Reuse.InCurrentScope);
            c.Register<IP, P4>(Reuse.InCurrentScope);
            c.Register<IP, P5>(Reuse.InCurrentScope);
            c.Register<IP, P6>(Reuse.InCurrentScope);
            c.Register<IP, P7>(Reuse.InCurrentScope);
            c.Register<IP, P8>(Reuse.InCurrentScope);
            c.Register<IP, P9>(Reuse.InCurrentScope);
            c.Register<IP, P10>(Reuse.InCurrentScope);
            c.Register<IP, P11>(Reuse.InCurrentScope);
            c.Register<IP, P12>(Reuse.InCurrentScope);
            c.Register<IP, P13>(Reuse.InCurrentScope);
            c.Register<IP, P14>(Reuse.InCurrentScope);
            c.Register<IP, P15>(Reuse.InCurrentScope);
            c.Register<IP, P16>(Reuse.InCurrentScope);
            c.Register<IP, P17>(Reuse.InCurrentScope);
            c.Register<IP, P18>(Reuse.InCurrentScope);
            c.Register<IP, P19>(Reuse.InCurrentScope);
            c.Register<IP, P20>(Reuse.InCurrentScope);
            c.Register<IP, P21>(Reuse.InCurrentScope);
            c.Register<IP, P22>(Reuse.InCurrentScope);
            c.Register<IP, P23>(Reuse.InCurrentScope);
            c.Register<IP, P24>(Reuse.InCurrentScope);
            c.Register<IP, P25>(Reuse.InCurrentScope);
            c.Register<IP, P26>(Reuse.InCurrentScope);
            c.Register<IP, P27>(Reuse.InCurrentScope);
            c.Register<IP, P28>(Reuse.InCurrentScope);
            c.Register<IP, P29>(Reuse.InCurrentScope);
            c.Register<IP, P30>(Reuse.InCurrentScope);
            c.Register<IP, P31>(Reuse.InCurrentScope);
            c.Register<IP, P32>(Reuse.InCurrentScope);
            c.Register<IP, P33>(Reuse.InCurrentScope);
            c.Register<IP, P34>(Reuse.InCurrentScope);
            c.Register<IP, P35>(Reuse.InCurrentScope);
            c.Register<IP, P36>(Reuse.InCurrentScope);
            c.Register<IP, P37>(Reuse.InCurrentScope);
            c.Register<IP, P38>(Reuse.InCurrentScope);
            c.Register<IP, P39>(Reuse.InCurrentScope);
            c.Register<IP, P40>(Reuse.InCurrentScope);
            c.Register<IP, P41>(Reuse.InCurrentScope);
            c.Register<IP, P42>(Reuse.InCurrentScope);
            c.Register<IP, P43>(Reuse.InCurrentScope);
            c.Register<IP, P44>(Reuse.InCurrentScope);
            c.Register<IP, P45>(Reuse.InCurrentScope);
            c.Register<IP, P46>(Reuse.InCurrentScope);
            c.Register<IP, P47>(Reuse.InCurrentScope);
            c.Register<IP, P48>(Reuse.InCurrentScope);
            c.Register<IP, P49>(Reuse.InCurrentScope);
            c.Register<IP, P50>(Reuse.InCurrentScope);
            c.Register<IQ, Q1>(Reuse.InCurrentScope);
            c.Register<IQ, Q2>(Reuse.InCurrentScope);
            c.Register<IQ, Q3>(Reuse.InCurrentScope);
            c.Register<IQ, Q4>(Reuse.InCurrentScope);
            c.Register<IQ, Q5>(Reuse.InCurrentScope);
            c.Register<IQ, Q6>(Reuse.InCurrentScope);
            c.Register<IQ, Q7>(Reuse.InCurrentScope);
            c.Register<IQ, Q8>(Reuse.InCurrentScope);
            c.Register<IQ, Q9>(Reuse.InCurrentScope);
            c.Register<IQ, Q10>(Reuse.InCurrentScope);
            c.Register<IQ, Q11>(Reuse.InCurrentScope);
            c.Register<IQ, Q12>(Reuse.InCurrentScope);
            c.Register<IQ, Q13>(Reuse.InCurrentScope);
            c.Register<IQ, Q14>(Reuse.InCurrentScope);
            c.Register<IQ, Q15>(Reuse.InCurrentScope);
            c.Register<IQ, Q16>(Reuse.InCurrentScope);
            c.Register<IQ, Q17>(Reuse.InCurrentScope);
            c.Register<IQ, Q18>(Reuse.InCurrentScope);
            c.Register<IQ, Q19>(Reuse.InCurrentScope);
            c.Register<IQ, Q20>(Reuse.InCurrentScope);
            c.Register<IQ, Q21>(Reuse.InCurrentScope);
            c.Register<IQ, Q22>(Reuse.InCurrentScope);
            c.Register<IQ, Q23>(Reuse.InCurrentScope);
            c.Register<IQ, Q24>(Reuse.InCurrentScope);
            c.Register<IQ, Q25>(Reuse.InCurrentScope);
            c.Register<IQ, Q26>(Reuse.InCurrentScope);
            c.Register<IQ, Q27>(Reuse.InCurrentScope);
            c.Register<IQ, Q28>(Reuse.InCurrentScope);
            c.Register<IQ, Q29>(Reuse.InCurrentScope);
            c.Register<IQ, Q30>(Reuse.InCurrentScope);
            c.Register<IQ, Q31>(Reuse.InCurrentScope);
            c.Register<IQ, Q32>(Reuse.InCurrentScope);
            c.Register<IQ, Q33>(Reuse.InCurrentScope);
            c.Register<IQ, Q34>(Reuse.InCurrentScope);
            c.Register<IQ, Q35>(Reuse.InCurrentScope);
            c.Register<IQ, Q36>(Reuse.InCurrentScope);
            c.Register<IQ, Q37>(Reuse.InCurrentScope);
            c.Register<IQ, Q38>(Reuse.InCurrentScope);
            c.Register<IQ, Q39>(Reuse.InCurrentScope);
            c.Register<IQ, Q40>(Reuse.InCurrentScope);
            c.Register<IQ, Q41>(Reuse.InCurrentScope);
            c.Register<IQ, Q42>(Reuse.InCurrentScope);
            c.Register<IQ, Q43>(Reuse.InCurrentScope);
            c.Register<IQ, Q44>(Reuse.InCurrentScope);
            c.Register<IQ, Q45>(Reuse.InCurrentScope);
            c.Register<IQ, Q46>(Reuse.InCurrentScope);
            c.Register<IQ, Q47>(Reuse.InCurrentScope);
            c.Register<IQ, Q48>(Reuse.InCurrentScope);
            c.Register<IQ, Q49>(Reuse.InCurrentScope);
            c.Register<IQ, Q50>(Reuse.InCurrentScope);
        }
    }
}