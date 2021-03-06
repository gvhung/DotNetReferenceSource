//-----------------------------------------------------------------------------
// <copyright file="Activity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Transactions.Diagnostics
{
    using System;

    internal class Activity : IDisposable
    {
        Guid oldGuid;
        Guid newGuid;
        bool emitTransfer = false;
        bool mustDispose = false;

        Activity(ref Guid newGuid, bool emitTransfer)
        {
            this.emitTransfer = emitTransfer;
            if (DiagnosticTrace.ShouldCorrelate && newGuid != Guid.Empty)
            {
                this.newGuid = newGuid;
                this.oldGuid = DiagnosticTrace.GetActivityId();
                if (oldGuid != newGuid)
                {
                    this.mustDispose = true;
                    if (this.emitTransfer)
                    {
                        DiagnosticTrace.TraceTransfer(newGuid);
                    }
                    DiagnosticTrace.SetActivityId(newGuid);
                }
            }
        }

        internal static Activity CreateActivity(Guid newGuid, bool emitTransfer)
        {
            Activity retval = null;
            if (DiagnosticTrace.ShouldCorrelate &&
                (newGuid != Guid.Empty) &&
                (newGuid != DiagnosticTrace.GetActivityId()))
            {
                retval = new Activity(ref newGuid, emitTransfer);
            }
            return retval;
        }

        public void Dispose()
        {
            if (this.mustDispose)
            {
                this.mustDispose = false;
                if (this.emitTransfer)
                {
                    DiagnosticTrace.TraceTransfer(oldGuid);
                }
                DiagnosticTrace.SetActivityId(oldGuid);
            }
        }
    }
}
