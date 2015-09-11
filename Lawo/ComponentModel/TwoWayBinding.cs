﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using Lawo.Reflection;

    /// <summary>Provides methods to create two-way <see cref="Binding{T, U, V, W}"/> instances.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: This type is thread safe.</para>
    /// </remarks>
    public static class TwoWayBinding
    {
        /// <summary>Creates a two-way binding between <paramref name="source"/> and <paramref name="target"/> by
        /// calling <see cref="Create{T, U, V, W}"/>(<paramref name="source"/>, v => v, <paramref name="target"/>, v => v).
        /// </summary>
        /// <typeparam name="TSourceOwner">The type of the object owning the source property.</typeparam>
        /// <typeparam name="TTargetOwner">The type of the object owning the target property.</typeparam>
        /// <typeparam name="TProperty">The type of the source and target properties.</typeparam>
        public static Binding<TSourceOwner, TProperty, TTargetOwner, TProperty> Create<TSourceOwner, TTargetOwner, TProperty>(
            IProperty<TSourceOwner, TProperty> source, IProperty<TTargetOwner, TProperty> target)
            where TSourceOwner : INotifyPropertyChanged
            where TTargetOwner : INotifyPropertyChanged
        {
            return Create(source, v => v, target, v => v);
        }

        /// <summary>Creates a two-way binding between <paramref name="source"/> and <paramref name="target"/>.
        /// </summary>
        /// <remarks>Firstly, the value of <paramref name="target"/>.<see cref="IProperty{T, U}.Value"/> is set to the
        /// one of <paramref name="toTarget"/>(<paramref name="source"/>.<see cref="IProperty{T, U}.Value"/>). Secondly,
        /// a separate handler is added to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of both
        /// properties. After establishing that a property participating in the binding has been changed, the respective
        /// handler sets the value of the other participating property to the (appropriately converted) value of the
        /// changed property.</remarks>
        /// <typeparam name="TSourceOwner">The type of the object owning the source property.</typeparam>
        /// <typeparam name="TSource">The type of the source property.</typeparam>
        /// <typeparam name="TTargetOwner">The type of the object owning the target property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="toTarget"/>,
        /// <paramref name="target"/> and/or <paramref name="toSource"/> equal <c>null</c>.</exception>
        public static Binding<TSourceOwner, TSource, TTargetOwner, TTarget> Create<TSourceOwner, TSource, TTargetOwner, TTarget>(
            IProperty<TSourceOwner, TSource> source,
            Func<TSource, TTarget> toTarget,
            IProperty<TTargetOwner, TTarget> target,
            Func<TTarget, TSource> toSource)
            where TSourceOwner : INotifyPropertyChanged
            where TTargetOwner : INotifyPropertyChanged
        {
            if (toSource == null)
            {
                throw new ArgumentNullException("toSource");
            }

            return new Binding<TSourceOwner, TSource, TTargetOwner, TTarget>(source, toTarget, target, toSource);
        }
    }
}