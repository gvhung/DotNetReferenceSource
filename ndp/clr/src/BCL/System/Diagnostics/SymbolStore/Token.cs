// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  SymbolToken
**
** Small value class used by the SymbolStore package for passing
** around metadata tokens.
**
===========================================================*/
namespace System.Diagnostics.SymbolStore {
    
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public struct SymbolToken
    {
        internal int m_token;
        
        public SymbolToken(int val) {m_token=val;}
    
        public int GetToken() {return m_token;}
        
        public override int GetHashCode() {return m_token;}
        
        public override bool Equals(Object obj)
        {
            if (obj is SymbolToken)
                return Equals((SymbolToken)obj);
            else
                return false;
        }
    
        public bool Equals(SymbolToken obj)
        {
            return obj.m_token == m_token;
        }
    
        public static bool operator ==(SymbolToken a, SymbolToken b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(SymbolToken a, SymbolToken b)
        {
            return !(a == b);
        }
    }
}
