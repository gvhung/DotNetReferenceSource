//------------------------------------------------------------------------------
// <copyright file="DataGridViewContentAlignment.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.DataGridViewContentAlignment"]/*' />
    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    public enum DataGridViewContentAlignment
    {
        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.NotSet"]/*' />
        NotSet = 0x000,

        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.TopLeft"]/*' />
        /// <devdoc>
        ///    Content is vertically aligned at the top, and horizontally
        ///    aligned on the left.
        /// </devdoc>
        TopLeft = 0x001,
        
        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.TopCenter"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Content is vertically aligned at the top, and
        ///       horizontally aligned at the center.
        ///    </para>
        /// </devdoc>
        TopCenter = 0x002,
        
        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.TopRight"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Content is vertically aligned at the top, and
        ///       horizontally aligned on the right.
        ///    </para>
        /// </devdoc>
        TopRight = 0x004,
        
        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.MiddleLeft"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Content is vertically aligned in the middle, and
        ///       horizontally aligned on the left.
        ///    </para>
        /// </devdoc>
        MiddleLeft = 0x010,
        
        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.MiddleCenter"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Content is vertically aligned in the middle, and
        ///       horizontally aligned at the center.
        ///    </para>
        /// </devdoc>
        MiddleCenter = 0x020,
        
        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.MiddleRight"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Content is vertically aligned in the middle, and horizontally aligned on the
        ///       right.
        ///    </para>
        /// </devdoc>
        MiddleRight = 0x040,
        
        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.BottomLeft"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Content is vertically aligned at the bottom, and horizontally aligned on the
        ///       left.
        ///    </para>
        /// </devdoc>
        BottomLeft = 0x100,
        
        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.BottomCenter"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Content is vertically aligned at the bottom, and horizontally aligned at the
        ///       center.
        ///    </para>
        /// </devdoc>
        BottomCenter = 0x200,

        /// <include file='doc\DataGridViewContentAlignment.uex' path='docs/doc[@for="DataGridViewContentAlignment.BottomRight"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Content is vertically aligned at the bottom, and horizontally aligned on the
        ///       right.
        ///    </para>
        /// </devdoc>
        BottomRight = 0x400,
    }
}
