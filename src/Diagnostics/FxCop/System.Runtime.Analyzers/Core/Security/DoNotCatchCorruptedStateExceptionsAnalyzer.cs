// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

//using Microsoft.SecurityDevelopmentLifecycle.Diagnostics.Shared;

namespace System.Runtime.Analyzers
{
    public abstract class DoNotCatchCorruptedStateExceptionsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2153";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotCatchCorruptedStateExceptions), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotCatchCorruptedStateExceptionsMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.DoNotCatchCorruptedStateExceptionsDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Security,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: null,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        protected abstract Analyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationMSInternalTypes types);

        private static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create(Rule);

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return supportedDiagnostics;
            }
        }

        /// <summary>
        /// Initialize the analyzer.
        /// </summary>
        /// <param name="analysisContext">Analyzer Context.</param>
        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationStartAction(
                (context) =>
                {
                    var compilationTypes = new CompilationMSInternalTypes(context.Compilation);
                    if (compilationTypes.HandleProcessCorruptedStateExceptionsAttribute != null)
                    {
                        GetAnalyzer(context, compilationTypes);
                    }
                });
        }

        protected abstract class Analyzer
        {
            protected CompilationMSInternalTypes TypesOfInterest { get; private set; }

            protected Analyzer(CompilationMSInternalTypes compilationTypes)
            {
                TypesOfInterest = compilationTypes;
            }

            protected abstract void CheckNode(SyntaxNode methodNode, SemanticModel model, Action<Diagnostic> reportDiagnostic);

            /// <summary>
            /// Analyze the syntax node.
            /// </summary>
            /// <param name="context">Syntax Context.</param>
            public void AnalyzeNode(SyntaxNodeAnalysisContext context)
            {
                SyntaxNode node = context.Node;
                SemanticModel model = context.SemanticModel;
                IMethodSymbol method = model.GetDeclaredSymbol(node) as IMethodSymbol;
                if (method == null)
                {
                    return;
                }

                ImmutableArray<AttributeData> attributes = method.GetAttributes();
                if (attributes.FirstOrDefault(attribute => attribute.AttributeClass == TypesOfInterest.HandleProcessCorruptedStateExceptionsAttribute) != null)
                {
                    CheckNode(node, model, context.ReportDiagnostic);
                }
            }

            protected bool IsCatchTypeTooGeneral(ISymbol catchTypeSym)
            {
                return catchTypeSym == null
                        || catchTypeSym == TypesOfInterest.SystemException
                        || catchTypeSym == TypesOfInterest.SystemSystemException
                        || catchTypeSym == TypesOfInterest.SystemObject;
            }
        }
    }
}
