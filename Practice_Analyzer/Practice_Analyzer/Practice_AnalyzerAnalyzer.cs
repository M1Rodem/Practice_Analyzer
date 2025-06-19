using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;

namespace Practice_Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Practice_AnalyzerAnalyzer : DiagnosticAnalyzer
    {
        // Для BreakDebugger
        public const string BreakDebuggerId = "BREAKDEBYGGER01";
        private const string BreakDebuggerTitle = "BreakDebugger usage";
        private const string BreakDebuggerMessage = "Don't call BreakDebugger() method";

        // Для проверки именования
        public const string NamingId = "NAMINGID01";
        private const string NamingTitle = "Naming style";

        private static readonly DiagnosticDescriptor BreakDebuggerRule = new DiagnosticDescriptor(
            BreakDebuggerId,
            BreakDebuggerTitle,
            BreakDebuggerMessage,
            "Usage",
            DiagnosticSeverity.Warning,
            true);

        private static readonly DiagnosticDescriptor NamingRule = new DiagnosticDescriptor(
            NamingId,
            NamingTitle,
            "{0}",
            "Naming",
            DiagnosticSeverity.Warning,
            true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(BreakDebuggerRule, NamingRule);

        public override void Initialize(AnalysisContext context)
        {
            Debug.WriteLine("Анализатор запущен!");

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // ПроверкаBreakDebugger
            context.RegisterSyntaxNodeAction(CheckBreakDebugger, SyntaxKind.InvocationExpression);

            // Проверки именования
            context.RegisterSymbolAction(CheckClass, SymbolKind.NamedType);
            context.RegisterSymbolAction(CheckField, SymbolKind.Field);
            context.RegisterSymbolAction(CheckProperty, SymbolKind.Property);
            context.RegisterSymbolAction(CheckEvent, SymbolKind.Event);
            context.RegisterSymbolAction(CheckParameter, SymbolKind.Parameter);
            context.RegisterSymbolAction(CheckMethod, SymbolKind.Method);
            context.RegisterSyntaxNodeAction(CheckLocalVariable, SyntaxKind.LocalDeclarationStatement);
        }

        private void CheckBreakDebugger(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.ToString().Contains("BreakDebugger"))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(BreakDebuggerRule, invocation.GetLocation()));
            }
        }

        private void CheckClass(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            if (namedType.TypeKind != TypeKind.Class) return;

            var name = namedType.Name;
            if (string.IsNullOrEmpty(name)) return;

            if (char.IsLower(name[0]))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        namedType.Locations[0],
                        $"Имя класса '{name}' должно начинаться с заглавной буквы"));
            }
        }

        private void CheckField(SymbolAnalysisContext context)
        {
            var field = (IFieldSymbol)context.Symbol;
            var name = field.Name;

            if (string.IsNullOrEmpty(name)) return;

            // Пропускаем приватные поля с подчеркиванием (разрешены)
            if (field.DeclaredAccessibility == Accessibility.Private && name.StartsWith("_"))
                return;

            // Проверка на подчеркивание
            if (name.StartsWith("_"))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        field.Locations[0],
                        $"Поле '{name}' не должно начинаться с подчеркивания"));
                return;
            }

            // Проверка констант
            if (field.IsConst && char.IsLower(name[0]))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        field.Locations[0],
                        $"Константа '{name}' должна начинаться с заглавной буквы"));
            }
        }

        private void CheckProperty(SymbolAnalysisContext context)
        {
            var property = (IPropertySymbol)context.Symbol;
            var name = property.Name;

            if (property.DeclaredAccessibility == Accessibility.Private &&
                char.IsLower(name[0]))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        property.Locations[0],
                        $"Приватное свойство '{name}' должно начинаться с заглавной буквы"));
            }
        }

        private void CheckEvent(SymbolAnalysisContext context)
        {
            var eventSymbol = (IEventSymbol)context.Symbol;
            var name = eventSymbol.Name;

            if (char.IsLower(name[0]))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        eventSymbol.Locations[0],
                        $"Событие '{name}' должно начинаться с заглавной буквы"));
            }
        }

        private void CheckParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;
            var name = parameter.Name;

            if (name.StartsWith("_"))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        parameter.Locations[0],
                        $"Параметр '{name}' не должен начинаться с подчеркивания"));
            }
            else if (char.IsUpper(name[0]))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        parameter.Locations[0],
                        $"Параметр '{name}' должен начинаться со строчной буквы"));
            }
        }

        private void CheckMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;
            var name = method.Name;

            if (method.IsStatic && method.ContainingType?.IsStatic == true)
            {
                return; // пропуск статических методов в статических классах
            }

            if (name == "Main" && method.MethodKind == MethodKind.Ordinary)
            {
                return; // пропуск Main
            }

            // Пропускаем конструкторы и специальные методы
            if (method.MethodKind == MethodKind.Constructor ||
                method.MethodKind == MethodKind.Destructor ||
                method.MethodKind == MethodKind.PropertyGet ||
                method.MethodKind == MethodKind.PropertySet)
            {
                return;
            }

            if (name.StartsWith("_"))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        method.Locations[0],
                        $"Метод '{name}' не должен начинаться с подчеркивания"));
            }
            else if (char.IsUpper(name[0]))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        NamingRule,
                        method.Locations[0],
                        $"Метод '{name}' должен начинаться со строчной буквы"));
            }
        }

        private void CheckLocalVariable(SyntaxNodeAnalysisContext context)
        {
            var declaration = (LocalDeclarationStatementSyntax)context.Node;

            foreach (var variable in declaration.Declaration.Variables)
            {
                var name = variable.Identifier.Text;

                if (name.StartsWith("_"))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            NamingRule,
                            variable.Identifier.GetLocation(),
                            $"Локальная переменная '{name}' не должна начинаться с подчеркивания"));
                }
                else if (char.IsUpper(name[0]))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            NamingRule,
                            variable.Identifier.GetLocation(),
                            $"Локальная переменная '{name}' должна начинаться со строчной буквы"));
                }
            }
        }
    }
}