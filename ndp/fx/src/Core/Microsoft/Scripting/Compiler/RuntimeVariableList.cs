/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Ast.Compiler;
#else
using System.Linq.Expressions.Compiler;
#endif

using System.ComponentModel;
using System.Diagnostics;

namespace System.Runtime.CompilerServices {

    /// <summary>
    /// This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.
    /// Contains helper methods called from dynamically generated methods.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough]
    public static partial class RuntimeOps {
        /// <summary>
        /// Creates an interface that can be used to modify closed over variables at runtime.
        /// </summary>
        /// <param name="data">The closure array.</param>
        /// <param name="indexes">An array of indicies into the closure array where variables are found.</param>
        /// <returns>An interface to access variables.</returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static IRuntimeVariables CreateRuntimeVariables(object[] data, long[] indexes) {
            return new RuntimeVariableList(data, indexes);
        }

        /// <summary>
        /// Creates an interface that can be used to modify closed over variables at runtime.
        /// </summary>
        /// <returns>An interface to access variables.</returns>
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static IRuntimeVariables CreateRuntimeVariables() {
            return new EmptyRuntimeVariables();
        }

        private sealed class EmptyRuntimeVariables : IRuntimeVariables {
            int IRuntimeVariables.Count {
                get { return 0; }
            }

            object IRuntimeVariables.this[int index] {
                get {
                    throw new ArgumentOutOfRangeException("index");
                }
                set {
                    throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        /// <summary>
        /// Provides a list of variables, supporing read/write of the values
        /// Exposed via RuntimeVariablesExpression
        /// </summary>
        private sealed class RuntimeVariableList : IRuntimeVariables {
            // The top level environment. It contains pointers to parent
            // environments, which are always in the first element
            private readonly object[] _data;

            // An array of (int, int) pairs, each representing how to find a
            // variable in the environment data struction.
            //
            // The first integer indicates the number of times to go up in the
            // closure chain, the second integer indicates the index into that
            // closure chain.
            private readonly long[] _indexes;

            internal RuntimeVariableList(object[] data, long[] indexes) {
                Debug.Assert(data != null);
                Debug.Assert(indexes != null);

                _data = data;
                _indexes = indexes;
            }

            public int Count {
                get { return _indexes.Length; }
            }

            public object this[int index] {
                get {
                    return GetStrongBox(index).Value;
                }
                set {
                    GetStrongBox(index).Value = value;
                }
            }

            private IStrongBox GetStrongBox(int index) {
                // We lookup the closure using two ints:
                // 1. The high dword is the number of parents to go up
                // 2. The low dword is the index into that array
                long closureKey = _indexes[index];

                // walk up the parent chain to find the real environment
                object[] result = _data;
                for (int parents = (int)(closureKey >> 32); parents > 0; parents--) {
                    result = HoistedLocals.GetParent(result);
                }

                // Return the variable storage
                return (IStrongBox)result[(int)closureKey];
            }
        }
    }
}
