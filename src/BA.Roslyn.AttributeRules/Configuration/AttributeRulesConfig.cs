using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Configuration
{
    public class AttributeRulesConfig
    {
        public Dictionary<string, AttributeRuleBaseConfig> Rules { get; set; } = new Dictionary<string, AttributeRuleBaseConfig>();
    }
}
