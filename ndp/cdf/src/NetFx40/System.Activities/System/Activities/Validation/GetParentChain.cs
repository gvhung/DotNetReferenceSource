//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;   
    
    public sealed class GetParentChain : CodeActivity<IEnumerable<Activity>>
    {
        public GetParentChain()
            : base()
        {
        }

        public InArgument<ValidationContext> ValidationContext
        {
            get;
            set;
        }

        protected override IEnumerable<Activity> Execute(CodeActivityContext context)
        {
            Fx.Assert(this.ValidationContext != null, "ValidationContext must not be null");

            ValidationContext currentContext = this.ValidationContext.Get(context);
            if (currentContext != null)
            {
                return currentContext.GetParents();
            }
            else
            {
                return ActivityValidationServices.EmptyChildren;
            }
        }
    }
}
