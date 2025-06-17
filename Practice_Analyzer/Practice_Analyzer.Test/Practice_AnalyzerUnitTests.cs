using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Practice_Analyzer.Test.CSharpCodeFixVerifier<
    Practice_Analyzer.Practice_AnalyzerAnalyzer,
    Practice_Analyzer.Practice_AnalyzerCodeFixProvider>;

namespace Practice_Analyzer.Test
{
    [TestClass]
    public class NamingConventionTests
    {
        // Тест для проверки имен классов
        [TestMethod]
        public async Task ClassNameStartsWithLower_ReportsDiagnostic()
        {
            var test = @"
public class {|#0:myClass|} 
{
}";

            var expected = VerifyCS.Diagnostic("NamingViolation")
                .WithLocation(0)
                .WithMessage("Class 'myClass' should start with uppercase letter");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        // Тест для приватных свойств
        [TestMethod]
        public async Task PrivatePropertyStartsWithLower_ReportsDiagnostic()
        {
            var test = @"
public class Test
{
    private string {|#0:myProperty|} { get; set; }
}";

            var expected = VerifyCS.Diagnostic("NamingViolation")
                .WithLocation(0)
                .WithMessage("Private property 'myProperty' should start with uppercase letter");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        // Тест для событий
        [TestMethod]
        public async Task EventStartsWithLower_ReportsDiagnostic()
        {
            var test = @"
using System;
public class Test
{
    public event Action {|#0:myEvent|};
}";

            var expected = VerifyCS.Diagnostic("NamingViolation")
                .WithLocation(0)
                .WithMessage("Event 'myEvent' should start with uppercase letter");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        // Тест для констант
        [TestMethod]
        public async Task ConstStartsWithLower_ReportsDiagnostic()
        {
            var test = @"
public class Test
{
    public const int {|#0:maxValue|} = 100;
}";

            var expected = VerifyCS.Diagnostic("NamingViolation")
                .WithLocation(0)
                .WithMessage("Constant 'maxValue' should start with uppercase letter");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        // Тест для локальных переменных
        [TestMethod]
        public async Task LocalVarStartsWithUpper_ReportsDiagnostic()
        {
            var test = @"
public class Test
{
    public void Method()
    {
        int {|#0:MyVar|} = 0;
    }
}";

            var expected = VerifyCS.Diagnostic("NamingViolation")
                .WithLocation(0)
                .WithMessage("Local variable 'MyVar' should start with lowercase letter");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        // Тест, когда все имена корректны - не должно быть диагностик
        [TestMethod]
        public async Task AllNamesCorrect_NoDiagnostics()
        {
            var test = @"
using System;
public class CorrectClass
{
    private string CorrectProperty { get; set; }
    public event Action CorrectEvent;
    public const int CorrectConstant = 100;
    
    public void Method()
    {
        int correctVar = 0;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }

    [TestClass]
    public class BreakDebuggerTests
    {
        [TestMethod]
        public async Task BreakDebuggerCall_ReportsDiagnostic()
        {
            var test = @"
public static class SyntaxNodeExtensions
{
    public static void BreakDebugger(this object node) {}
}

public class Test
{
    public void Method()
    {
        object node = null;
        {|#0:node.BreakDebugger()|};
    }
}";

            var expected = VerifyCS.Diagnostic("BreakDebuggerUsage")
                .WithLocation(0)
                .WithMessage("Avoid using 'BreakDebugger' method in production code");

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}