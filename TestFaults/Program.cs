using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFault;

namespace TestFaults
{
    class Program
    {
        static void TestInt()
        {
            using (var code = new Compiler("C#"))
            {
                code.Reference<int>()
                    .Compile(@"
using System;

static class Foo
{
    static void Bar()
    {
        int i = 3.4;
    }
}").FailsWith(code.Error("CS0266", 8));
            }
        }

        static void SuccessInt()
        {
            using (var code = new Compiler("C#"))
            {
                code.Reference<int>()
                    .Compile(@"
using System;

static class Foo
{
    static void Bar()
    {
        int i = 3;
    }
}").Success();
            }
        }

        static void DelegateConstraint()
        {
            using (var code = new Compiler("C#"))
            {
                code.Reference<int>()
                    .Compile(@"
using System;

class Foo<T> where T : Delegate
{
}")
                    .FailsWith(code.Error("CS0702", 4));
            }
        }

        static void Main(string[] args)
        {
            TestInt();
            SuccessInt();
            DelegateConstraint();
        }
    }
}
