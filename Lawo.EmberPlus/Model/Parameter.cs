﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Globalization;

    using Ember;

    /// <summary>This API is not intended to be used directly from your code.</summary>
    /// <remarks>Provides the common implementation for all not nullable parameters in the object tree accessible
    /// through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class Parameter<TMostDerived, TValue> : ParameterBase<TMostDerived, TValue>
        where TMostDerived : Parameter<TMostDerived, TValue>
    {
        internal Parameter()
        {
        }

        internal override ChildrenState ReadContents(EmberReader reader, ElementType actualType)
        {
            var result = base.ReadContents(reader, actualType);

            if (this.GetValue() == null)
            {
                const string Format = "No value field is available for the non-nullable parameter with the path {0}.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.GetPath()));
            }

            return result;
        }
    }
}