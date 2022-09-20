using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.ClassContextGenerator.Abstractions
{
    public class ApplicationModeAttribute : Attribute
    {
        public ApplicationModeAttribute(params ApplicationMode[] applicationModes)
        {
            ApplicationModes = applicationModes;
        }

        public ApplicationMode[] ApplicationModes { get; }
    }
}
