using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Practice_Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Practice_AnalyzerAnalyzer : DiagnosticAnalyzer
    {
        // Диагностики для BreakDebugger
        public const string DiagnosticId = "BreakDebuggerUsage";
        private const string BreakDebuggerTitle = "BreakDebugger method call detected";
        private const string BreakDebuggerMessage = "Avoid using 'BreakDebugger' method in production code";
        
        // Диагностики для проверки именования
        public const string NamingDiagnosticId = "NamingViolation";
        private const string NamingTitle = "Incorrect naming convention";
        private const string PrivatePropertyMessage = "Private property '{0}' should start with uppercase letter";
        private const string EventMessage = "Event '{0}' should start with uppercase letter";
        private const string ConstMessage = "Constant '{0}' should start with uppercase letter";
        private const string ClassMessage = "Class '{0}' should start with uppercase letter";
        private const string LocalVarMessage = "Local variable '{0}' should start with lowercase letter";

        private static readonly DiagnosticDescriptor BreakDebuggerRule = new DiagnosticDescriptor(
            DiagnosticId, 
            BreakDebuggerTitle, 
            BreakDebuggerMessage, 
            "Usage", 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor NamingRule = new DiagnosticDescriptor(
            NamingDiagnosticId,
            NamingTitle,
            "{0}",
            "Naming",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(BreakDebuggerRule, NamingRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
            
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
            context.RegisterSymbolAction(AnalyzeEvent, SymbolKind.Event);
            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            context.RegisterSyntaxNodeAction(AnalyzeLocalDeclaration, SyntaxKind.LocalDeclarationStatement);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "BreakDebugger")
            {
                var methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
                
                if (methodSymbol?.ContainingType?.Name == "SyntaxNodeExtensions")
                {
                    var diagnostic = Diagnostic.Create(
                        BreakDebuggerRule, 
                        invocation.GetLocation());
                    
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            
            if (namedTypeSymbol.TypeKind == TypeKind.Class && 
                char.IsLower(namedTypeSymbol.Name[0]))
            {
                var diagnostic = Diagnostic.Create(
                    NamingRule,
                    namedTypeSymbol.Locations[0],
                    string.Format(ClassMessage, namedTypeSymbol.Name));
                
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;
            
            if (propertySymbol.DeclaredAccessibility == Accessibility.Private &&
                char.IsLower(propertySymbol.Name[0]))
            {
                var diagnostic = Diagnostic.Create(
                    NamingRule,
                    propertySymbol.Locations[0],
                    string.Format(PrivatePropertyMessage, propertySymbol.Name));
                
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeEvent(SymbolAnalysisContext context)
        {
            var eventSymbol = (IEventSymbol)context.Symbol;
            
            if (char.IsLower(eventSymbol.Name[0]))
            {
                var diagnostic = Diagnostic.Create(
                    NamingRule,
                    eventSymbol.Locations[0],
                    string.Format(EventMessage, eventSymbol.Name));
                
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeField(SymbolAnalysisContext context)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;
            
            if (fieldSymbol.IsConst && char.IsLower(fieldSymbol.Name[0]))
            {
                var diagnostic = Diagnostic.Create(
                    NamingRule,
                    fieldSymbol.Locations[0],
                    string.Format(ConstMessage, fieldSymbol.Name));
                
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;
            
            foreach (var variable in localDeclaration.Declaration.Variables)
            {
                if (variable.Identifier.Text.Length > 0 && 
                    char.IsUpper(variable.Identifier.Text[0]))
                {
                    var diagnostic = Diagnostic.Create(
                        NamingRule,
                        variable.Identifier.GetLocation(),
                        string.Format(LocalVarMessage, variable.Identifier.Text));
                    
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}