//---------------------------------------------------------------------------
//
// <copyright file="PolyQuadraticBezierSegment.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// This file was generated, please do not edit it directly.
//
// Please see http://wiki/default.aspx/Microsoft.Projects.Avalon/MilCodeGen.html for more information.
//
//---------------------------------------------------------------------------

using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.Collections;
using MS.Internal.PresentationCore;
using MS.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel.Design.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows.Media.Converters;
using System.Security;
using System.Security.Permissions;
using SR=MS.Internal.PresentationCore.SR;
using SRID=MS.Internal.PresentationCore.SRID;
// These types are aliased to match the unamanaged names used in interop
using BOOL = System.UInt32;
using WORD = System.UInt16;
using Float = System.Single;

namespace System.Windows.Media
{



    sealed partial class PolyQuadraticBezierSegment : PathSegment
    {
        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        /// <summary>
        ///     Shadows inherited Clone() with a strongly typed
        ///     version for convenience.
        /// </summary>
        public new PolyQuadraticBezierSegment Clone()
        {
            return (PolyQuadraticBezierSegment)base.Clone();
        }

        /// <summary>
        ///     Shadows inherited CloneCurrentValue() with a strongly typed
        ///     version for convenience.
        /// </summary>
        public new PolyQuadraticBezierSegment CloneCurrentValue()
        {
            return (PolyQuadraticBezierSegment)base.CloneCurrentValue();
        }




        #endregion Public Methods

        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------




        #region Public Properties

        /// <summary>
        ///     Points - PointCollection.  Default value is new FreezableDefaultValueFactory(PointCollection.Empty).
        /// </summary>
        public PointCollection Points
        {
            get
            {
                return (PointCollection) GetValue(PointsProperty);
            }
            set
            {
                SetValueInternal(PointsProperty, value);
            }
        }

        #endregion Public Properties

        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// Implementation of <see cref="System.Windows.Freezable.CreateInstanceCore">Freezable.CreateInstanceCore</see>.
        /// </summary>
        /// <returns>The new Freezable.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new PolyQuadraticBezierSegment();
        }



        #endregion ProtectedMethods

        //------------------------------------------------------
        //
        //  Internal Methods
        //
        //------------------------------------------------------

        #region Internal Methods









        #endregion Internal Methods

        //------------------------------------------------------
        //
        //  Internal Properties
        //
        //------------------------------------------------------

        #region Internal Properties

        //
        //  This property finds the correct initial size for the _effectiveValues store on the
        //  current DependencyObject as a performance optimization
        //
        //  This includes:
        //    Points
        //
        internal override int EffectiveValuesInitialSize
        {
            get
            {
                return 1;
            }
        }



        #endregion Internal Properties

        //------------------------------------------------------
        //
        //  Dependency Properties
        //
        //------------------------------------------------------

        #region Dependency Properties

        /// <summary>
        ///     The DependencyProperty for the PolyQuadraticBezierSegment.Points property.
        /// </summary>
        public static readonly DependencyProperty PointsProperty;

        #endregion Dependency Properties

        //------------------------------------------------------
        //
        //  Internal Fields
        //
        //------------------------------------------------------

        #region Internal Fields





        internal static PointCollection s_Points = PointCollection.Empty;

        #endregion Internal Fields



        #region Constructors

        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        static PolyQuadraticBezierSegment()
        {
            // We check our static default fields which are of type Freezable
            // to make sure that they are not mutable, otherwise we will throw
            // if these get touched by more than one thread in the lifetime
            // of your app.  (Windows OS Bug #947272)
            //
            Debug.Assert(s_Points == null || s_Points.IsFrozen,
                "Detected context bound default value PolyQuadraticBezierSegment.s_Points (See OS Bug #947272).");


            // Initializations
            Type typeofThis = typeof(PolyQuadraticBezierSegment);
            PointsProperty =
                  RegisterProperty("Points",
                                   typeof(PointCollection),
                                   typeofThis,
                                   new FreezableDefaultValueFactory(PointCollection.Empty),
                                   null,
                                   null,
                                   /* isIndependentlyAnimated  = */ false,
                                   /* coerceValueCallback */ null);
        }



        #endregion Constructors

    }
}
