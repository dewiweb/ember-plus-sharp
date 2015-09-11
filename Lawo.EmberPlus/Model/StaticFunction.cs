﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    using Lawo.EmberPlus.Ember;

    /// <summary>Provides the common functionality for all static functions.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public abstract class StaticFunction<TMostDerived> : FunctionBase<TMostDerived>
        where TMostDerived : StaticFunction<TMostDerived>
    {
        private static KeyValuePair<string, ParameterType>[] argumentsTemplate;
        private static KeyValuePair<string, ParameterType>[] resultTemplate;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal StaticFunction() : base(
            (KeyValuePair<string, ParameterType>[])ArgumentsTemplate.Clone(),
            (KeyValuePair<string, ParameterType>[])ResultTemplate.Clone())
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override KeyValuePair<string, ParameterType>[] ReadTupleDescription(
            EmberReader reader, KeyValuePair<string, ParameterType>[] expectedTypes)
        {
            var descriptionCount = this.ReadTupleDescription(reader, expectedTypes, (i, d) => expectedTypes[i] = d);

            if (descriptionCount < expectedTypes.Length)
            {
                throw this.CreateSignatureMismatchException();
            }

            return expectedTypes;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override KeyValuePair<string, ParameterType> ReadTupleItemDescription(
            EmberReader reader, KeyValuePair<string, ParameterType>[] expectedTypes, int index)
        {
            if (index >= expectedTypes.Length)
            {
                throw this.CreateSignatureMismatchException();
            }

            var description = base.ReadTupleItemDescription(reader, expectedTypes, index);

            if (description.Value != expectedTypes[index].Value)
            {
                throw this.CreateSignatureMismatchException();
            }

            return description;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static KeyValuePair<string, ParameterType>[] ArgumentsTemplate
        {
            get { return LazyInitializer.EnsureInitialized(ref argumentsTemplate, CreateArgumentsTemplate); }
        }

        private static KeyValuePair<string, ParameterType>[] ResultTemplate
        {
            get { return LazyInitializer.EnsureInitialized(ref resultTemplate, CreateResultTemplate); }
        }

        private static KeyValuePair<string, ParameterType>[] CreateArgumentsTemplate()
        {
            var genericArguments = typeof(TMostDerived).GenericTypeArguments;
            return CreateTypeArray(genericArguments.Where((t, i) => i < genericArguments.Length - 1));
        }

        private static KeyValuePair<string, ParameterType>[] CreateResultTemplate()
        {
            var genericArguments = typeof(TMostDerived).GenericTypeArguments;
            var resultType = genericArguments[genericArguments.Length - 1];
            return CreateTypeArray(
                resultType.IsConstructedGenericType ? resultType.GenericTypeArguments : Enumerable.Empty<Type>());
        }

        private static KeyValuePair<string, ParameterType>[] CreateTypeArray(IEnumerable<Type> argumentTypes)
        {
            return argumentTypes.Select(t => new KeyValuePair<string, ParameterType>(null, GetType(t))).ToArray();
        }

        private static ParameterType GetType(Type type)
        {
            if (type == typeof(long))
            {
                return ParameterType.Integer;
            }
            else if (type == typeof(double))
            {
                return ParameterType.Real;
            }
            else if (type == typeof(string))
            {
                return ParameterType.String;
            }
            else if (type == typeof(bool))
            {
                return ParameterType.Boolean;
            }
            else if (type == typeof(byte[]))
            {
                return ParameterType.Octets;
            }
            else
            {
                const string Format = "Unsupported type in function signature {0}.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, typeof(TMostDerived)));
            }
        }
    }
}