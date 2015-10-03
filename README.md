# CodeFault

A library to check that compilation fails on code fragments:

    using (var code = new Compiler("C#"))
    {
        code.Reference<int>()	// reference System.dll assembly
            .Compile(@"
    using System;

    class Foo<T> where T : Delegate
    {
    }")
            .FailsWith(code.Error("CS0702", 4)); // Constraint cannot be special class 'Delegate'
    }

If the compilation does not have that error at the given line,
it will throw an exception. This allows unit testing abstractions
that are supposed to prevent certain errors at compile-time.