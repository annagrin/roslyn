// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public partial class DoNotCatchCorruptedStateExceptionsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotCatchCorruptedStateExceptionsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotLockOnObjectsWithWeakIdentity();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Diagnostics)]
        public void CA2153TestCatchExceptionInMethodWithSecurityCriticalAttribute()
        {
            VerifyCSharp(@"
            using System;
            using System.IO;
            using System.Security;

            namespace TestNamespace
            {
                class TestClass
                {
                    [SecurityCritical]
                    public static void TestMethod()
                    {
                        try 
                        {
                            FileStream fileStream = new FileStream(""name"", FileMode.Create);
                        }
                        catch (Exception e)
                        {}
                    }
                }
            }");

            VerifyBasic(@"
            Imports System.IO
            Imports System.Security

            Namespace TestNamespace
                Class TestClass
                    <SecurityCritical> _
                    Public Shared Sub TestMethod()
                        Try
                            Dim fileStream As New FileStream(""name"", FileMode.Create)
                        Catch e As System.Exception
                        End Try
                    End Sub
                End Class
            End Namespace
            ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Diagnostics)]
        public void CA2153TestCatchExceptionInMethodWithHpcseAttribute()
        {
            // Note this is a change from FxCop's previous behavior since we no longer consider SystemCritical.

            VerifyCSharp(@"
            using System;
            using System.IO;
            using System.Runtime.ExceptionServices;

            namespace TestNamespace
            {
                class TestClass
                {
                    [HandleProcessCorruptedStateExceptions] 
                    public static void TestMethod()
                    {
                        try 
                        {
                            FileStream fileStream = new FileStream(""name"", FileMode.Create);
                        }
                        catch (Exception e)
                        {}
                    }
                }
            }",
            GetCA2153BasicResultAt(17, 25, "catch")
            );

            VerifyBasic(@"
            Imports System.IO
            Imports System.Runtime.ExceptionServices

            Namespace TestNamespace
                Class TestClass
                    <HandleProcessCorruptedStateExceptions> _
                    Public Shared Sub TestMethod()
                        Try
                            Dim fileStream As New FileStream(""name"", FileMode.Create)
                        Catch e As System.Exception
                        End Try
                    End Sub
                End Class
            End Namespace
            ",
            GetCA2153BasicResultAt(12, 25, "Catch")
            );
        }
/*

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicFunctionCatchExceptionInMethodWithHpcseAttributeShouldGenerateDiagnostic()
        {
            var sourceString0 = @"

Imports System.IO
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        Public Shared Function TestMethod() As Double
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
            End Try
            Return 0
        End Function
    End Class
End Namespace
";

            // Note this is a change from FxCop's previous behavior since we no longer consider SystemCritical.
            DiagnosticResult[] expected =
                {
                    new DiagnosticResult
                        {
                            Id = "CA2153",
                            Severity = CA2153Severity,
                            Locations =
                                new[]
                                    {
                                        new DiagnosticResultLocation(
                                            "SourceString0.vb",
                                            12,
                                            13)
                                    }
                        }
                };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void RethrowInCatchExceptionInMethodWithBothAttributeShouldNotGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        [SecurityCritical]
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream = new FileStream(""name"", FileMode.Create);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}";
            VerifyCSharpDiagnostic(sourceString0);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicRethrowInCatchExceptionInMethodWithBothAttributeShouldNotGenerateDiagnostic()
        {
            var sourceString0 = @"

Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Sub TestMethod()
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
                Throw
            End Try
        End Sub
    End Class
End Namespace
";
            VerifyBasicDiagnostic(sourceString0);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicFunctionRethrowInCatchExceptionInMethodWithBothAttributeShouldNotGenerateDiagnostic()
        {
            var sourceString0 = @"

Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Function TestMethod() As Double
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
                Throw
            End Try
            Return 0
        End Function
    End Class
End Namespace
";
            VerifyBasicDiagnostic(sourceString0);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        [SecurityCritical]
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream = new FileStream(""name"", FileMode.Create);
            }
            catch (Exception e)
            {}
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 19, 13)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicCatchExceptionInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Sub TestMethod()
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
            End Try
        End Sub
    End Class
End Namespace
";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 13, 13)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInMethodWithAttributeOnClassEverythingShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    [SecurityCritical(SecurityCriticalScope.Everything)]
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream = new FileStream(""name"", FileMode.Create);
            }
            catch (Exception e)
            {}
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 19, 13)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInMethodWithAttributeOnClassNotEverythingShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    [SecurityCritical]
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream = new FileStream(""name"", FileMode.Create);
            }
            catch (Exception e)
            {}
        }
    }
}";
            // Note this is a change from FxCop's previous behavior since we no longer consider SystemCritical.
            DiagnosticResult[] expected =
                {
                    new DiagnosticResult
                        {
                            Id = "CA2153",
                            Severity = CA2153Severity,
                            Locations =
                                new[]
                                    {
                                        new DiagnosticResultLocation(
                                            "SourceString0.cs",
                                            19,
                                            13)
                                    }
                        }
                };

            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInMethodWithAttributeOnClassExplicitShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    [SecurityCritical(SecurityCriticalScope.Explicit)]
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream = new FileStream(""name"", FileMode.Create);
            }
            catch (Exception e)
            {}
        }
    }
}";
            // Note this is a change from FxCop's previous behavior since we no longer consider SystemCritical.
            DiagnosticResult[] expected =
                {
                    new DiagnosticResult
                        {
                            Id = "CA2153",
                            Severity = CA2153Severity,
                            Locations =
                                new[]
                                    {
                                        new DiagnosticResultLocation(
                                            "SourceString0.cs",
                                            19,
                                            13)
                                    }
                        }
                };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicCatchExceptionInMethodWithAttributeOnClassShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    <SecurityCritical(SecurityCriticalScope.Everything)> _
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        Public Shared Sub TestMethod()
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
            End Try
        End Sub
    End Class
End Namespace
";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 13, 13)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInMethodWithAttributeOnAssemblyL1ShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

[assembly:SecurityCritical(SecurityCriticalScope.Everything)]
[assembly:SecurityRules(SecurityRuleSet.Level1)]
namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream = new FileStream(""name"", FileMode.Create);
            }
            catch (Exception e)
            {}
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 20, 13)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInMethodWithAttributeOnAssemblyL2ShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

[assembly:SecurityCritical(SecurityCriticalScope.Everything)]
[assembly:SecurityRules(SecurityRuleSet.Level2)]
namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream = new FileStream(""name"", FileMode.Create);
            }
            catch (Exception e)
            {}
        }
    }
}";
            // Note this is a change from FxCop's previous behavior since we no longer consider SystemCritical.
            DiagnosticResult[] expected =
                {
                    new DiagnosticResult
                        {
                            Id = "CA2153",
                            Severity = CA2153Severity,
                            Locations =
                                new[]
                                    {
                                        new DiagnosticResultLocation(
                                            "SourceString0.cs",
                                            20,
                                            13)
                                    }
                        }
                };

            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInMethodWithAttributeOnNestedEverythingClassOuterShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    [SecurityCritical(SecurityCriticalScope.Everything)]
    class TestClass
    {
        class NestedClass
        {
            [HandleProcessCorruptedStateExceptions] 
            public static void TestMethod()
            {
                try 
                {
                    FileStream fileStream = new FileStream(""name"", FileMode.Create);
                }
                catch (Exception e)
                {}
            }
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 21, 17)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInMethodWithAttributeOnNestedClassInnerShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [SecurityCritical(SecurityCriticalScope.Everything)]
        class NestedClass
        {
            [HandleProcessCorruptedStateExceptions] 
            public static void TestMethod()
            {
                try 
                {
                    FileStream fileStream = new FileStream(""name"", FileMode.Create);
                }
                catch (Exception e)
                {}
            }
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 21, 17)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }



        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicFunctionCatchExceptionInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Function TestMethod() As Double
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
            End Try
            Return 0
        End Function
    End Class
End Namespace
";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 13, 13)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        [SecurityCritical]
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream= new FileStream(""name"", FileMode.Create);
            }
            catch {}
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 19, 13)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchExceptionInGetAccessorShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {      
        public string SaveNewFile3
        {
            [HandleProcessCorruptedStateExceptions]
            get
            {
                try
                {
                    AccessViolation();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(""CATCH"");
                }
                return ""asdf"";
            }
        }
        private static void AccessViolation(){}
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 20, 17)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchInGetAccessorShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {      
        public string SaveNewFile3
        {
            [HandleProcessCorruptedStateExceptions]
            get
            {
                try
                {
                    AccessViolation();
                }
                catch
                {
                    Console.WriteLine(""CATCH"");
                }
                return ""asdf"";
            }
        }
        private static void AccessViolation(){}
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 20, 17)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchSystemExceptionInGetAccessorShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {      
        public string SaveNewFile3
        {
            [HandleProcessCorruptedStateExceptions]
            get
            {
                try
                {
                    AccessViolation();
                }
                catch (SystemException ex)
                {
                    Console.WriteLine(""CATCH"");
                }
                return ""asdf"";
            }
        }
        private static void AccessViolation(){}
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 20, 17)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchInSetAccessorShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {      
        private string file;
        public string SaveNewFile3
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                try
                {
                    AccessViolation();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(""CATCH"");
                }
                file = value;
            }
        } 
        private static void AccessViolation(){}
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 21, 17)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicCatchInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Sub TestMethod()
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch
            End Try
        End Sub
    End Class
End Namespace
";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 13, 13)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchSystemExceptionInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        [SecurityCritical]
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream= new FileStream(""name"", FileMode.Create);
            }
            catch (SystemException e)
            {}
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 19, 13)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchIOExceptionInMethodWithCSEAttributeShouldNotGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions]
        public static void TestMethod()
        { 
            try
            {
                FileStream fs = new FileStream(""fileName"", FileMode.Create);
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch
            {
                throw;
            }
            finally { }
        }
    }
}";
            VerifyCSharpDiagnostic(sourceString0);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchIOExceptionSwallowOtherExceptionInMethodWithCSEAttributeShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        public static void TestMethod()
        { 
            try
            {
                FileStream fs = new FileStream(""fileName"", FileMode.Create);
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch {}
            finally { }
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 22, 13)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }


        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void SwallowAccessViolationExceptionInMethodWithCSEAttributesShouldNotGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {   
        [HandleProcessCorruptedStateExceptions]
        public static void SaveNewFile7(string fileName)
        {
            try
            {
                unsafe
                {
                    byte b = *(byte*)(8762765876); // some code that causes access violation
                }
            }
            catch (AccessViolationException ex)
            {
                // the AV is ignored here
            }
            finally {}
        }
    }
}";
            VerifyCSharpDiagnostic(sourceString0);
        }


        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void SwallowAccessViolationExceptionThenSwallowOtherExceptionInMethodWithCSEAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {   
        [HandleProcessCorruptedStateExceptions]
        public static void SaveNewFile7(string fileName)
        {
            try
            {
                unsafe
                {
                    byte b = *(byte*)(8762765876); // some code that causes access violation
                }
            }
            catch (AccessViolationException ex)
            {
                // the AV is ignored here
            }
            catch {}
            finally {}
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 25, 13)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicCatchSystemExceptionInMethodWithBothAttributesShouldGenerateDiagnosticWithPartiallyQualifiedExceptionName()
        {
            var sourceString0 = @"
Imports System;
Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Sub TestMethod()
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As Exception
            End Try
        End Sub
    End Class
End Namespace
";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 14, 13)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicCatchSystemExceptionInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Sub TestMethod()
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
            End Try
        End Sub
    End Class
End Namespace
";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 13, 13)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void ThrowNotImplementedExceptionInCatchExceptionInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        [SecurityCritical]
        public static void TestMethod()
        {
            try 
            {
                FileStream fileStream= new FileStream(""name"", FileMode.Create);
            }
            catch (Exception e)
            {
                throw new NotImplementedException();
            }
        }
    }
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 19, 13)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicThrowNotImplementedExceptionInCatchExceptionInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Sub TestMethod()
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
                Throw New NotImplementedException()
            End Try
        End Sub
    End Class
End Namespace";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 12, 13)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicFunctionThrowNotImplementedExceptionInCatchExceptionInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Function TestMethod() As Double
            Try
                Dim fileStream As New FileStream(""name"", FileMode.Create)
            Catch e As System.Exception
                Throw New NotImplementedException()
            End Try
            Return 0
        End Function
    End Class
End Namespace";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 12, 13)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }
        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void RethrowInCatchIOExceptionInCatchExceptionWithInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Security;
using System.Runtime.ExceptionServices;

namespace TestNamespace
{
    class TestClass
    {
        [HandleProcessCorruptedStateExceptions] 
        [SecurityCritical]
        public static void TestMethod()
        {
            FileStream fileStream= null;
            try
            {
                fileStream= new FileStream(""name"", FileMode.Create);
            }
            catch (Exception)
            {
                try
                {
                    FileStream  anotherFileStream = new FileStream(""newName"", FileMode.Create);
                }
                catch (IOException)
                {
                    throw;
                }
            }
        }
    }
}";
            //FxCop doesn't generate this warning
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 20, 13)}
                }
            };
            
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicRethrowInCatchIOExceptionInCatchExceptionWithInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Sub TestMethod()
            Dim fileStream As FileStream = Nothing
            Try
                fileStream= New FileStream(""name"", FileMode.Create)
            Catch outterException As System.Exception
                Try
                    Dim anotherFileStream = New FileStream(""newName"", FileMode.Create)
                Catch innerException As IOException
                    Throw
                End Try
            End Try
        End Sub
    End Class
End Namespace";
            //FxCop doesn't generate this warning
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 14, 13)}
                }
            };

            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicFunctionRethrowInCatchIOExceptionInCatchExceptionWithInMethodWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"
Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        <HandleProcessCorruptedStateExceptions> _
        <SecurityCritical> _
        Public Shared Function TestMethod() As Double
            Dim fileStream As FileStream = Nothing
            Try
                fileStream= New FileStream(""name"", FileMode.Create)
            Catch outterException As System.Exception
                Try
                    Dim anotherFileStream = New FileStream(""newName"", FileMode.Create)
                Catch innerException As IOException
                    Throw
                End Try
            End Try
            Return 0
        End Function
    End Class
End Namespace";
            //FxCop doesn't generate this warning
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 14, 13)}
                }
            };

            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void BasicThrowNotImplementedExceptionInCatchExceptionInSetAccessorWithBothAttributesShouldGenerateDiagnostic()
        {
            var sourceString0 = @"Imports System.IO
Imports System.Security
Imports System.Runtime.ExceptionServices

Namespace TestNamespace
    Class TestClass
        private x As Integer
        Public Property X() As Integer
            <HandleProcessCorruptedStateExceptions> _
            <SecurityCritical> _
            Get
                Try
                    Dim fileStream As New FileStream(""name"", FileMode.Create)
                Catch e As System.Exception
                    Throw New NotImplementedException()
                End Try
                Return x
            End Get
        End Property
    End Class
End Namespace";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 14, 17)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchingGeneralExceptionWithoutHpcseShouldNotFire_CS()
        {
            var sourceString0 = @"
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security;
using System.IdentityModel;
using System.Threading;

[SecurityCritical]
struct Program
{
    [HandleProcessCorruptedStateExceptions]
    public static explicit operator Program(int i)
    {
        try
        {
            return new Program(DoSomethingBad(i));
        }
        catch (Exception ex)
        {
            throw (ex != null) ? null : ex;
        }
        catch
        {
            throw;
        }
    }

    [HandleProcessCorruptedStateExceptions]
    public static void Test()
    {
        Func<int,int> f = (int n) =>
        {
            try
            {
                return DoSomethingBad(n);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        };
        int n = f(0);

        Action d = delegate()
        {
            try
            {
                int m = DoSomethingBad();
                if (m != n)
                {
                }
            }
            catch
            {
                throw;
            }
        };

        try
        {
            d();
        }
        catch (SystemException ex)
        {
            if (n != 0)
            {
                throw ex;
            }
            else if ((ex == null) || (ex.Data == null))
            {
                using (Stream s = File.Open(""a.txt"", FileMode.Open))
                {
                    throw;
                }
            }

            throw new NotImplementedException("""", ex);
        }
    }

    private static int DoSomethingBad(int n = 0)
    {
        // do something that may cause a corrupted state exception
        return n;
    }

    private Program(int i)
    {
        number = i;
    }

    private int number;
}";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.cs", 19, 9)}
                }
            };
            VerifyCSharpDiagnostic(sourceString0, expected);
        }

        [TestMethod]
        [TestCategory(TestCategories.Gated)]
        public void CatchingGeneralExceptionWithoutHpcseShouldNotFire_VB()
        {
            var sourceString0 = @"
Imports System
Imports System.IO
Imports System.Runtime.ExceptionServices
Imports System.Security
Imports System.IdentityModel
Imports System.Threading

<SecurityCritical> _
Struct Program
    <HandleProcessCorruptedStateExceptions> _
    Public Shared Widening Operator CType(i As Integer) As Program
        Try
            Return New Program(DoSomethingBad(i))
        Catch ex As Exception
            Throw If((ex Is Nothing), ex, Nothing)
        Catch
            Throw
        End Try
    End Operator

    <HandleProcessCorruptedStateExceptions> _
    Public Shared Sub Test
        Dim f = Function(n As Integer)
            Try
                Return DoSomethingBad(n)
            Catch ex As Exception
                Throw ex
            End Try
        End Function

        Dim n = f(0)

        Dim d = Sub()
            Try
                Dim m = DoSomethingBad()
                If (m <> n) Then
                    Console.WriteLine(""Unbelievable"")
                End If
            Catch
                Throw
            End Try
        End Sub

        Try
            d()
        Catch ex As SystemException
            If (n <> 0) Then
                Throw ex
            ElseIf ((ex Is Nothing) OR (ex.Data Is Nothing)) Then
                Using s As Stream = File.Open(""a.txt"", FileMode.Open))
                    Throw
                End Using
            End If

            Throw New NotImplementedException("""", ex)
        }
    End Sub

    Private Shared Function DoSomethingBad(Optional n As Integer = 0) As Integer
        ' do something that may cause a corrupted state exception
        Return n
    End Function

    Private Sub New(i As Integer)
        number = i
    End Sub

    Private number As Integer
End Struct";
            DiagnosticResult[] expected = {
                new DiagnosticResult
                {
                    Id = "CA2153",
                    Severity = CA2153Severity,
                    Locations = new[] { new DiagnosticResultLocation("SourceString0.vb", 15, 9)}
                }
            };
            VerifyBasicDiagnostic(sourceString0, expected);
        }
        */
        private const string CA2153RuleName = "CA2153";

        private DiagnosticResult GetCA2153CSharpResultAt(int line, int column, string typeName)
        {
            return GetCSharpResultAt(line, column, CA2153RuleName, string.Format(SystemRuntimeAnalyzersResources.DoNotCatchCorruptedStateExceptions, typeName));
        }

        private DiagnosticResult GetCA2153BasicResultAt(int line, int column, string typeName)
        {
            return GetBasicResultAt(line, column, CA2153RuleName, string.Format(SystemRuntimeAnalyzersResources.DoNotCatchCorruptedStateExceptions, typeName));
        }
    }
}
