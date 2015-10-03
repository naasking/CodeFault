using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace CodeFault
{
    /// <summary>
    /// Indicates a error with the expected compilation.
    /// </summary>
    public sealed class ExpectedCompileException : Exception
    {
        public ExpectedCompileException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Checks for expected errors in code fragments.
    /// </summary>
    public class Compiler : IDisposable
    {
        CodeDomProvider provider;
        CompilerParameters options = new CompilerParameters { GenerateInMemory = true };

        /// <summary>
        /// Construct an instance for the given language.
        /// </summary>
        /// <param name="language"></param>
        public Compiler(string language)
        {
            provider = CodeDomProvider.CreateProvider(language);
        }

        /// <summary>
        /// Add a referenced assembly.
        /// </summary>
        /// <param name="assembly">The assembly string.</param>
        /// <returns>The current instance.</returns>
        public Compiler Reference(string assembly)
        {
            options.ReferencedAssemblies.Add(assembly);
            return this;
        }

        /// <summary>
        /// Add the assembly for the given type.
        /// </summary>
        /// <typeparam name="T">The type whose assembly we're adding.</typeparam>
        /// <returns>The current instance.</returns>
        public Compiler Reference<T>()
        {
            var asm = typeof(T).Assembly;
            return asm == typeof(int).Assembly ? Reference("System.dll") : Reference(typeof(T).Assembly.FullName);
        }

        /// <summary>
        /// Add a series of assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to add.</param>
        /// <returns>The current instance.</returns>
        public Compiler Reference(params string[] assemblies)
        {
            options.ReferencedAssemblies.AddRange(assemblies);
            return this;
        }

        /// <summary>
        /// Create an expected error.
        /// </summary>
        /// <param name="errorNumber"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public CompilerError Error(string errorNumber, int line, int column = -1)
        {
            return new CompilerError { Line = line, Column = column, ErrorNumber = errorNumber };
        }

        /// <summary>
        /// Attempt to compile a program and check for the expected errors.
        /// </summary>
        /// <param name="program">The program to run.</param>
        /// <param name="expected">The minimal set of errors we expect to see.</param>
        /// <returns>The current instance.</returns>
        public Compiler FailsWith(string program, params CompilerError[] expected)
        {
            if (expected == null || expected.Length == 0) throw new ArgumentException("CCheck only checks for code that has compile errors.");
            Compile(program).FailsWith(expected);
            return this;
        }

        /// <summary>
        /// Compile a program.
        /// </summary>
        /// <param name="program">The program to run.</param>
        /// <returns>The compilation results.</returns>
        public CompilerResults Compile(string program)
        {
            return provider.CompileAssemblyFromSource(options, program);
        }

        /// <summary>
        /// Dispose of the current instance.
        /// </summary>
        public void Dispose()
        {
            var x = Interlocked.CompareExchange(ref provider, null, provider);
            if (x != null) x.Dispose();
        }
    }

    /// <summary>
    /// Extensions on CodeDom.
    /// </summary>
    public static class CodeFaults
    {
        /// <summary>
        /// Checks the compiler results for the expected errors.
        /// </summary>
        /// <param name="results">The results to check.</param>
        /// <param name="expected">The errors we expect.</param>
        public static void FailsWith(this CompilerResults results, params CompilerError[] expected)
        {
            if (expected == null || expected.Length == 0) throw new ArgumentException("FailsWith only checks for code that has compile errors.");
            if (results.Errors.Count == 0) throw new ExpectedCompileException("Compilation did not fail.");
            var errors = results.Errors.Cast<CompilerError>();
            foreach (var x in expected)
            {
                if (!errors.Any(e => e.Line == x.Line && (x.Column < 0 || e.Column == x.Column) && e.ErrorNumber.Equals(x.ErrorNumber, StringComparison.Ordinal)))
                {
                    throw new ExpectedCompileException("Not found: " + x.ToString());
                }
            }
        }

        /// <summary>
        /// Checks that compilation succeeded.
        /// </summary>
        /// <param name="results">The results to check.</param>
        public static void Success(this CompilerResults results)
        {
            if (results.Errors.Count != 0)
                throw new ExpectedCompileException("Compilation had errors.");
        }
    }
}
