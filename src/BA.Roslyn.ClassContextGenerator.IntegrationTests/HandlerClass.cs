using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BA.Roslyn.ClassContextGenerator.Abstractions;

namespace BA.Roslyn.ClassContextGenerator.IntegrationTests
{
    [ApplicationMode(ApplicationMode.Free)]
    public class HandlerClass
    {
        void SomeMethod()
        {
            var test = new TestClass();
        }
    }
}
