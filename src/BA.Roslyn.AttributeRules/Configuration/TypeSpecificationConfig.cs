using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BA.Roslyn.AttributeRules.Configuration
{
    public class TypeSpecificationConfig
    {
        public string TypeName { get; set; } = null!;

        public List<TypeSpecificationConfig> TypeArguments { get; set; } = new List<TypeSpecificationConfig>();

        public INamedTypeSymbol? Build(Compilation compilation)
        {
            var type = compilation.GetTypeByMetadataName(TypeName);

            if (type == null)
            {
                return null;
            }
            
            if (type.Arity == 0)
            {
                return type;
            }


            if (type.Arity > 0 && !TypeArguments.Any())
            {
                return type.ConstructUnboundGenericType();
            }

            var typeArguments = new List<ITypeSymbol>();

            foreach (var arg in TypeArguments)
            {
                var newArg = arg.Build(compilation);

                if (newArg == null)
                {
                    return null;
                }

                typeArguments.Add(newArg);
            }

            return type.Construct(typeArguments.ToArray());
        }
    }
}
