// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System {

    using System.Diagnostics.Contracts;
    using System.Threading;

    // The IConvertible interface represents an object that contains a value. This
    // interface is implemented by the following types in the System namespace:
    // Boolean, Char, SByte, Byte, Int16, UInt16, Int32, UInt32, Int64, UInt64,
    // Single, Double, Decimal, DateTime, TimeSpan, and String. The interface may
    // be implemented by other types that are to be considered values. For example,
    // a library of nullable database types could implement IConvertible.
    //
    // Note: The interface was originally proposed as IValue.
    //
    // The implementations of IConvertible provided by the System.XXX value classes
    // simply forward to the appropriate Value.ToXXX(YYY) methods (a description of
    // the Value class follows below). In cases where a Value.ToXXX(YYY) method
    // does not exist (because the particular conversion is not supported), the
    // IConvertible implementation should simply throw an InvalidCastException.

    [CLSCompliant(false)]
    [System.Runtime.InteropServices.ComVisible(true)]
#if CONTRACTS_FULL
    [ContractClass(typeof(IConvertibleContract))]
#endif // CONTRACTS_FULL
    public interface IConvertible
    {
        // Returns the type code of this object. An implementation of this method
        // must not return TypeCode.Empty (which represents a null reference) or
        // TypeCode.Object (which represents an object that doesn't implement the
        // IConvertible interface). An implementation of this method should return
        // TypeCode.DBNull if the value of this object is a database null. For
        // example, a nullable integer type should return TypeCode.DBNull if the
        // value of the object is the database null. Otherwise, an implementation
        // of this method should return the TypeCode that best describes the
        // internal representation of the object.

        TypeCode GetTypeCode();

        // The ToXXX methods convert the value of the underlying object to the
        // given type. If a particular conversion is not supported, the
        // implementation must throw an InvalidCastException. If the value of the
        // underlying object is not within the range of the target type, the
        // implementation must throw an OverflowException.  The 
        // IFormatProvider will be used to get a NumberFormatInfo or similar
        // appropriate service object, and may safely be null.

        bool ToBoolean(IFormatProvider provider);
        char ToChar(IFormatProvider provider);
        sbyte ToSByte(IFormatProvider provider);
        byte ToByte(IFormatProvider provider);
        short ToInt16(IFormatProvider provider);
        ushort ToUInt16(IFormatProvider provider);
        int ToInt32(IFormatProvider provider);
        uint ToUInt32(IFormatProvider provider);
        long ToInt64(IFormatProvider provider);
        ulong ToUInt64(IFormatProvider provider);
        float ToSingle(IFormatProvider provider);
        double ToDouble(IFormatProvider provider);
        Decimal ToDecimal(IFormatProvider provider);
        DateTime ToDateTime(IFormatProvider provider);
        String ToString(IFormatProvider provider);
        Object ToType(Type conversionType, IFormatProvider provider);
    }

#if CONTRACTS_FULL
    [ContractClassFor(typeof(IConvertible))]
    internal abstract class IConvertibleContract : IConvertible
    {
        #region IConvertible Members

        [Pure]
        TypeCode IConvertible.GetTypeCode()
        {
            throw new NotImplementedException();
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        [Pure]
        String IConvertible.ToString(IFormatProvider provider)
        {
            Contract.Ensures(Contract.Result<String>() != null);
            throw new NotImplementedException();
        }

        Object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
#endif // CONTRACTS_FULL

}

