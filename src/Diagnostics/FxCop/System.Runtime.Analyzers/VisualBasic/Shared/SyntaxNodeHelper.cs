// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace System.Runtime.Analyzers
{
    public sealed class BasicSyntaxNodeHelper : SyntaxNodeHelper
    {
        private BasicSyntaxNodeHelper instance = new BasicSyntaxNodeHelper();

        public static BasicSyntaxNodeHelper Default { get { return instance; } }

        private BasicSyntaxNodeHelper()
        {
        }

        public override bool ContainsMethodCall(SyntaxNode node, Func<string, bool> predicate)
        {
            if (node == null)
            {
                return false;
            }

            return node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any(
                (InvocationExpressionSyntax child) =>
                {
                    return child.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(name => predicate(name.Identifier.ValueText));
                });
        }

        public override IMethodSymbol GetCallerMethodSymbol(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node == null)
            {
                return null;
            }

            MethodBlockSyntax declaration = node.AncestorsAndSelf().OfType<MethodBlockSyntax>().FirstOrDefault();
            if (declaration != null)
            {
                return semanticModel.GetDeclaredSymbol(declaration);
            }

            ConstructorBlockSyntax constructor = node.AncestorsAndSelf().OfType<ConstructorBlockSyntax>().FirstOrDefault();
            if (constructor != null)
            {
                return semanticModel.GetDeclaredSymbol(constructor);
            }

            return null;
        }

        public override ITypeSymbol GetEnclosingTypeSymbol(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node == null)
            {
                return null;
            }

            ModuleBlockSyntax declaration = node.AncestorsAndSelf().OfType<ModuleBlockSyntax>().FirstOrDefault();

            if (declaration == null)
            {
                return null;
            }

            return semanticModel.GetDeclaredSymbol(declaration);
        }

        public override ITypeSymbol GetClassDeclarationTypeSymbol(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node == null)
            {
                return null;
            }

            SyntaxKind kind = node.Kind();
            if (kind == SyntaxKind.ClassBlock)
            {
                return semanticModel.GetDeclaredSymbol((ClassBlockSyntax)node);
            }

            return null;
        }

        public override SyntaxNode GetAssignmentLeftNode(SyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

            SyntaxKind kind = node.Kind();
            if (kind == SyntaxKind.SimpleAssignmentStatement)
            {
                return ((AssignmentStatementSyntax)node).Left;
            }

            if (kind == SyntaxKind.VariableDeclarator)
            {
                return ((VariableDeclaratorSyntax)node).Names.First();
            }

            if (kind == SyntaxKind.NamedFieldInitializer)
            {
                return ((NamedFieldInitializerSyntax)node).Name;
            }

            return null;
        }

        public override SyntaxNode GetAssignmentRightNode(SyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

            SyntaxKind kind = node.Kind();
            if (kind == SyntaxKind.SimpleAssignmentStatement)
            {
                return ((AssignmentStatementSyntax)node).Right;
            }

            if (kind == SyntaxKind.VariableDeclarator)
            {
                VariableDeclaratorSyntax decl = ((VariableDeclaratorSyntax)node);
                if (decl.Initializer != null)
                {
                    return decl.Initializer.Value;
                }
                if (decl.AsClause != null)
                {
                    return decl.AsClause;
                }
            }

            if (kind == SyntaxKind.NamedFieldInitializer)
            {
                return ((NamedFieldInitializerSyntax)node).Expression;
            }

            return null;
        }

        public override SyntaxNode GetMemberAccessExpressionNode(SyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

            SyntaxKind kind = node.Kind();
            if (kind == SyntaxKind.SimpleMemberAccessExpression)
            {
                return ((MemberAccessExpressionSyntax)node).Expression;
            }

            return null;
        }

        public override SyntaxNode GetMemberAccessNameNode(SyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

            SyntaxKind kind = node.Kind();
            if (kind == SyntaxKind.SimpleMemberAccessExpression)
            {
                return ((MemberAccessExpressionSyntax)node).Name;
            }

            return null;
        }

        public override SyntaxNode GetInvocationExpressionNode(SyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

            SyntaxKind kind = node.Kind();
            if (kind != SyntaxKind.InvocationExpression)
            {
                return null;
            }

            return ((InvocationExpressionSyntax)node).Expression;
        }

        public override SyntaxNode GetCallTargetNode(SyntaxNode node)
        {
            if (node != null)
            {
                SyntaxKind kind = node.Kind();
                if (kind == SyntaxKind.InvocationExpression)
                {
                    var callExpr = ((InvocationExpressionSyntax)node).Expression;
                    return GetMemberAccessNameNode(callExpr) ?? callExpr;
                }
                else if (kind == SyntaxKind.ObjectCreationExpression)
                {
                    return ((ObjectCreationExpressionSyntax)node).Type;
                }
            }

            return null;
        }

        public override SyntaxNode GetDefaultValueForAnOptionalParameter(SyntaxNode declNode, int paramIndex)
        {
            var methodDecl = declNode as MethodBlockBaseSyntax;
            if (methodDecl != null)
            {
                ParameterListSyntax paramList = methodDecl.BlockStatement.ParameterList;
                if (paramIndex < paramList.Parameters.Count)
                {
                    EqualsValueSyntax equalsValueNode = paramList.Parameters[paramIndex].Default;
                    if (equalsValueNode != null)
                    {
                        return equalsValueNode.Value;
                    }
                }
            }
            return null;
        }

        protected override IEnumerable<SyntaxNode> GetCallArgumentExpressionNodes(SyntaxNode node, CallKind callKind)
        {
            if (node != null)
            {
                ArgumentListSyntax argList = null;
                SyntaxKind kind = node.Kind();
                if ((kind == SyntaxKind.InvocationExpression) && ((callKind & CallKind.Invocation) != 0))
                {
                    var invocationNode = (InvocationExpressionSyntax)node;
                    argList = invocationNode.ArgumentList;
                }
                else if ((kind == SyntaxKind.ObjectCreationExpression) && ((callKind & CallKind.ObjectCreation) != 0))
                {
                    var invocationNode = (ObjectCreationExpressionSyntax)node;
                    argList = invocationNode.ArgumentList;
                }
                if (argList != null)
                {
                    return from arg in argList.Arguments
                           select arg.GetExpression();
                }
            }

            return Enumerable.Empty<SyntaxNode>();
        }

        public override IEnumerable<SyntaxNode> GetObjectInitializerExpressionNodes(SyntaxNode node)
        {
            var empty = Enumerable.Empty<SyntaxNode>();
            if (node == null)
            {
                return empty;
            }

            SyntaxKind kind = node.Kind();
            if (kind != SyntaxKind.ObjectCreationExpression)
            {
                return empty;
            }

            var objectCreationNode = (ObjectCreationExpressionSyntax)node;
            if (objectCreationNode.Initializer == null)
            {
                return empty;
            }

            kind = objectCreationNode.Initializer.Kind();
            if (kind != SyntaxKind.ObjectMemberInitializer)
            {
                return empty;
            }
                
            var initializer = (ObjectMemberInitializerSyntax)objectCreationNode.Initializer;
                    
            return from fieldInitializer in initializer.Initializers
                           where fieldInitializer.Kind() == SyntaxKind.NamedFieldInitializer
                           select (NamedFieldInitializerSyntax)fieldInitializer;
        }

        public override bool IsMethodInvocationNode(SyntaxNode node)
        {
            if (node == null)
            {
                return false;
            }
            SyntaxKind kind = node.Kind();
            return kind == SyntaxKind.InvocationExpression || kind == SyntaxKind.ObjectCreationExpression;
        }

        public override bool IsObjectCreationExpressionUnderFieldDeclaration(SyntaxNode node)
        {
            return node != null &&
                   node.Kind() == SyntaxKind.ObjectCreationExpression &&
                   node.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().FirstOrDefault() != null;
        }

        public override SyntaxNode GetVariableDeclaratorOfAFieldDeclarationNode(SyntaxNode node)
        {
            if (IsObjectCreationExpressionUnderFieldDeclaration(node))
            {
                return node.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public override IEnumerable<SyntaxNode> GetDescendantAssignmentExpressionNodes(SyntaxNode node)
        {
            var empty = Enumerable.Empty<SyntaxNode>();
            if (node == null)
            {
                return empty;
            }

            return node.DescendantNodesAndSelf().OfType<AssignmentStatementSyntax>();
        }

        public override IEnumerable<SyntaxNode> GetDescendantMemberAccessExpressionNodes(SyntaxNode node)
        {
            var empty = Enumerable.Empty<SyntaxNode>();
            if (node == null)
            {
                return empty;
            }

            return node.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>();
        }
    }
}
