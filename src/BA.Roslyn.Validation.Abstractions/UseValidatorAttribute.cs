// <copyright file="UseValidatorAttribute.cs" company="DevIT in Gründung">
// Copyright (c) DevIT. All rights reserved.
// </copyright>

namespace BA.Roslyn.Validation.Abstractions
{
    using System;

    public class UseValidatorAttribute : Attribute
    {
        public UseValidatorAttribute(Type validator)
        {
            Validator = validator;
        }

        public Type Validator { get; }
    }
}