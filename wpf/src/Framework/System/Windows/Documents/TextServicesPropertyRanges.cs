//---------------------------------------------------------------------------
//
// <copyright file=TextServicesPropertyRanges.cs company=Microsoft>
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
//
// Description: TextServicesPropertyRanges.cs
//
// History:
//  08/01/2003 : yutakas - Ported from dotnet tree.
//
//---------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Security;
using System.Security.Permissions;

using System.Collections;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Documents;
using MS.Win32;

namespace System.Windows.Documents
{
    //------------------------------------------------------
    //
    //  TextServicesPropertyRanges class
    //
    //------------------------------------------------------

    /// <summary>
    ///   The base class for property ranges of EditRecord
    /// </summary>
    internal class TextServicesPropertyRanges
    {
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        internal TextServicesPropertyRanges(
            TextStore textstore,
            Guid guid)
        {
            _guid = guid;
            _textstore = textstore;
        }

        //------------------------------------------------------
        //
        //  Internal Method
        //
        //------------------------------------------------------

        /// <summary>
        ///    OnRange virtual method.
        /// </summary>
        /// <SecurityNote>
        /// Critical - Accepts critical argument of type ITfProperty and ITfRange
        /// </SecurityNote>
        [SecurityCritical]
        internal virtual void OnRange(UnsafeNativeMethods.ITfProperty property,
                                      int ecReadonly, 
                                      UnsafeNativeMethods.ITfRange range)
        {
        }

        /// <summary>
        ///    Calback function for TextEditSink
        ///    we track the property change here.
        /// </summary>
        /// <SecurityNote>
        /// Critical - calls unmanaged code, commits raw input to the system
        /// </SecurityNote>
        [SecurityCritical]
        internal virtual void OnEndEdit(UnsafeNativeMethods.ITfContext context,
                                        int ecReadOnly, 
                                        UnsafeNativeMethods.ITfEditRecord editRecord) 
        {
            int fetched;
            UnsafeNativeMethods.IEnumTfRanges ranges;
            UnsafeNativeMethods.ITfProperty prop = null;

            ranges = GetPropertyUpdate(editRecord);

            UnsafeNativeMethods.ITfRange [] outRanges;
            outRanges = new UnsafeNativeMethods.ITfRange[1];
            while (ranges.Next(1, outRanges, out fetched) == 0)
            {
                //
                // check the element has enabled dynamic property.
                //
                ITextPointer start;
                ITextPointer end;

                ConvertToTextPosition(outRanges[0], out start, out end);

                if (prop == null)
                    context.GetProperty(ref _guid, out prop);

                UnsafeNativeMethods.IEnumTfRanges rangesProp;

                if (prop.EnumRanges(ecReadOnly, out rangesProp, outRanges[0]) == 0)
                {
                    UnsafeNativeMethods.ITfRange [] outRangesProp;
                    outRangesProp = new UnsafeNativeMethods.ITfRange[1];
                    while (rangesProp.Next(1, outRangesProp, out fetched) == 0)
                    {
                        OnRange(prop, ecReadOnly, outRangesProp[0]);
                        Marshal.ReleaseComObject(outRangesProp[0]);
                    }
                    Marshal.ReleaseComObject(rangesProp);
                }

                Marshal.ReleaseComObject(outRanges[0]);

            }
            Marshal.ReleaseComObject(ranges);
            if (prop != null)
                Marshal.ReleaseComObject(prop);
        }

        //------------------------------------------------------
        //
        //  Protected Method
        //
        //------------------------------------------------------

        /// <summary>
        ///    Convert ITfRange to two TextPositions.
        /// </summary>
        /// <SecurityNote>
        /// Critical - calls unmanaged code (GetExtent)
        /// </SecurityNote>
        [SecurityCritical]
        protected void ConvertToTextPosition(UnsafeNativeMethods.ITfRange range,
                                               out ITextPointer start, 
                                               out ITextPointer end)
        {
            UnsafeNativeMethods.ITfRangeACP rangeACP;
            int startIndex;
            int length;

            rangeACP = range as UnsafeNativeMethods.ITfRangeACP;
            rangeACP.GetExtent(out startIndex, out length);

            if (length < 0)
            {
                // Cicero bug!  Length should never be less than zero.
                start = null;
                end = null;
            }
            else
            {
                start = _textstore.CreatePointerAtCharOffset(startIndex, LogicalDirection.Forward);
                end = _textstore.CreatePointerAtCharOffset(startIndex + length, LogicalDirection.Forward);
            }
        }

        /// <summary>
        ///    Get Cicero property value.
        /// </summary>
        /// <SecurityNote>
        /// Critical - calls unmanaged code (GetValue) and expose the returned object directly.
        ///            It could be any variant in a Cicero property in general.
        ///            non Avalon derived class is blocked by link demand
        /// </SecurityNote>
        [UIPermissionAttribute(SecurityAction.LinkDemand, Unrestricted= true)]
        [SecurityCritical]
        protected static Object GetValue(int ecReadOnly, UnsafeNativeMethods.ITfProperty property, UnsafeNativeMethods.ITfRange range)
        {
            if (property == null)
                return null;

            Object obj;
            property.GetValue(ecReadOnly, range, out obj);
 
            return obj;
        }

        /// <summary>
        ///    Get ranges that the property is changed.
        /// </summary>
        /// <SecurityNote>
        /// Critical - unsafe block to manipulate a pointer of a pointer to GUID.
        /// </SecurityNote>
        [SecurityCritical]
        private UnsafeNativeMethods.IEnumTfRanges GetPropertyUpdate(
                                UnsafeNativeMethods.ITfEditRecord editRecord) 
        {
            UnsafeNativeMethods.IEnumTfRanges ranges;

            unsafe 
            {
                fixed (Guid *pguid = &_guid)
                {
                    //
                    // 













                    IntPtr p = (IntPtr)pguid;
                    editRecord.GetTextAndPropertyUpdates(0, ref p, 1, out ranges);
                }
            }
            return ranges;
        }

        //------------------------------------------------------
        //
        //  Protected Properties
        //
        //------------------------------------------------------

        /// <summary>
        ///    GUID of ITfProperty
        /// </summary>
        protected Guid Guid 
        {
            get
            {
                return _guid;
            }
        }

        /// <summary>
        ///    Return TextStore that is associated to this property.
        /// </summary>
        protected TextStore TextStore 
        {
            get
            {
                return _textstore;
            }
        }

        //------------------------------------------------------
        //
        //  Private Field
        //
        //------------------------------------------------------

        // GUID of ITfProperty
        private Guid _guid;

        // The refrence of TextStore.
        private TextStore _textstore;
    }
}
