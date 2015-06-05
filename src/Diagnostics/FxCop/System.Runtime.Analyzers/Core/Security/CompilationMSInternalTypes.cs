// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace System.Runtime.Analyzers
{
    public class CompilationMSInternalTypes
    {
        public INamedTypeSymbol HandleProcessCorruptedStateExceptionsAttribute { get; private set; }
        public INamedTypeSymbol SystemObject { get; private set; }
        public INamedTypeSymbol SystemException { get; private set; }
        public INamedTypeSymbol SystemSystemException { get; private set; }

        public CompilationMSInternalTypes(Compilation compilation)
        {
            this.HandleProcessCorruptedStateExceptionsAttribute = 
                MSInternalTypes.HandleProcessCorruptedStateExceptionsAttribute(compilation);
            this.SystemObject = MSInternalTypes.SystemObject(compilation);
            this.SystemException = MSInternalTypes.SystemException(compilation);
            this.SystemSystemException = MSInternalTypes.SystemSystemException(compilation);
        }
    }
}
