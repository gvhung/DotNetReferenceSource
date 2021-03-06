//---------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// Description: VideoDrawing represents a drawing operation that renders 
//              video into a destination rectangle.
//
// History:  
//
//  2004/11/17 : timothyc - Created it.
//
//---------------------------------------------------------------------------

using System.Diagnostics;

namespace System.Windows.Media
{
    /// <summary>
    /// The class definition for VideoDrawing
    /// </summary>
    public sealed partial class VideoDrawing : Drawing
    {
        #region Constructors

        /// <summary>
        /// Default VideoDrawing constructor.  
        /// Constructs an object with all properties set to their default values
        /// </summary>        
        public VideoDrawing()
        {
        }

        #endregion      

        #region Internal methods

        /// <summary>
        /// Calls methods on the DrawingContext that are equivalent to the
        /// Drawing with the Drawing's current value.
        /// </summary>        
        internal override void WalkCurrentValue(DrawingContextWalker ctx)
        {
            // We avoid unneccessary ShouldStopWalking checks based on assumptions
            // about when ShouldStopWalking is set.  Guard that assumption with an
            // assertion.  See DrawingGroup.WalkCurrentValue comment for more details.
            Debug.Assert(!ctx.ShouldStopWalking);
            
            ctx.DrawVideo(
                Player,
                Rect
                );            
        }      

        #endregion Internal methods        
    }
}

