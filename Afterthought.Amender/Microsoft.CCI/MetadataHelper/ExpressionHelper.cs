using Microsoft.Cci;
using System;
using System.Diagnostics.Contracts;

/// <summary>
/// Class containing helper routines for IMetadataExpression expressions.
/// </summary>
public static class MetadataExpressionHelper {

  /// <summary>
  /// Returns true if the given constant expression contains a finite numeric value. In other words, infinities and NaN are excluded.
  /// </summary>
  /// <param name="constExpression"></param>
  /// <returns></returns>
  public static bool IsFiniteNumeric(IMetadataConstant constExpression) {
    IConvertible/*?*/ ic = constExpression.Value as IConvertible;
    if (ic == null) return false;
    switch (ic.GetTypeCode()) {
      case System.TypeCode.SByte:
      case System.TypeCode.Int16:
      case System.TypeCode.Int32:
      case System.TypeCode.Int64:
      case System.TypeCode.Byte:
      case System.TypeCode.UInt16:
      case System.TypeCode.UInt32:
      case System.TypeCode.UInt64:
        return true;
      case System.TypeCode.Double:
        var d = ic.ToDouble(null);
        return !(Double.IsNaN(d) || Double.IsInfinity(d));
      case System.TypeCode.Single:
        var s = ic.ToSingle(null);
        return !(Single.IsNaN(s) || Single.IsInfinity(s));
    }
    return false;
  }

  /// <summary>
  /// True if the value is a boxed -1 of type byte, int, long, sbyte, short, uint, ulong or ushort.
  /// </summary>
  [Pure]
  public static bool IsIntegralMinusOne(IMetadataConstant constExpression) {
    IConvertible/*?*/ ic = constExpression.Value as IConvertible;
    if (ic == null) return false;
    switch (ic.GetTypeCode()) {
      case System.TypeCode.SByte: return ic.ToSByte(null) == -1;
      case System.TypeCode.Int16: return ic.ToInt16(null) == -1;
      case System.TypeCode.Int32: return ic.ToInt32(null) == -1;
      case System.TypeCode.Int64: return ic.ToInt64(null) == -1;
      case System.TypeCode.Byte: return ic.ToByte(null) == byte.MaxValue;
      case System.TypeCode.UInt16: return ic.ToUInt16(null) == ushort.MaxValue;
      case System.TypeCode.UInt32: return ic.ToUInt32(null) == uint.MaxValue;
      case System.TypeCode.UInt64: return ic.ToUInt64(null) == ulong.MaxValue;
    }
    return false;
  }

  /// <summary>
  /// True if the value is a boxed zero of type byte, int, long, sbyte, short, uint, ulong, ushort or bool that is not equal to 0.
  /// </summary>
  [Pure]
  public static bool IsIntegralNonzero(IMetadataConstant constExpression) {
    IConvertible/*?*/ ic = constExpression.Value as IConvertible;
    if (ic == null) return false;
    switch (ic.GetTypeCode()) {
      case System.TypeCode.SByte: return ic.ToSByte(null) != 0;
      case System.TypeCode.Int16: return ic.ToInt16(null) != 0;
      case System.TypeCode.Int32: return ic.ToInt32(null) != 0;
      case System.TypeCode.Int64: return ic.ToInt64(null) != 0;
      case System.TypeCode.Byte: return ic.ToByte(null) != 0;
      case System.TypeCode.UInt16: return ic.ToUInt16(null) != 0;
      case System.TypeCode.UInt32: return ic.ToUInt32(null) != 0;
      case System.TypeCode.UInt64: return ic.ToUInt64(null) != 0;
      case System.TypeCode.Boolean: return ic.ToBoolean(null);
    }
    return false;
  }

  /// <summary>
  /// True if the value is a boxed 1 of type byte, int, long, sbyte, short, uint, ulong, ushort or bool.
  /// </summary>
  [Pure]
  public static bool IsIntegralOne(IMetadataConstant constExpression) {
    IConvertible/*?*/ ic = constExpression.Value as IConvertible;
    if (ic == null) return false;
    switch (ic.GetTypeCode()) {
      case System.TypeCode.SByte: return ic.ToSByte(null) == 1;
      case System.TypeCode.Int16: return ic.ToInt16(null) == 1;
      case System.TypeCode.Int32: return ic.ToInt32(null) == 1;
      case System.TypeCode.Int64: return ic.ToInt64(null) == 1;
      case System.TypeCode.Byte: return ic.ToByte(null) == 1;
      case System.TypeCode.UInt16: return ic.ToUInt16(null) == 1;
      case System.TypeCode.UInt32: return ic.ToUInt32(null) == 1;
      case System.TypeCode.UInt64: return ic.ToUInt64(null) == 1;
      case System.TypeCode.Boolean: return ic.ToBoolean(null);
    }
    return false;
  }

  /// <summary>
  /// True if the value is a boxed zero of type byte, int, long, sbyte, short, uint, ulong, ushort or bool.
  /// </summary>
  [Pure]
  public static bool IsIntegralZero(IMetadataConstant constExpression) {
    IConvertible/*?*/ ic = constExpression.Value as IConvertible;
    if (ic == null) return false;
    switch (ic.GetTypeCode()) {
      case System.TypeCode.SByte: return ic.ToSByte(null) == 0;
      case System.TypeCode.Int16: return ic.ToInt16(null) == 0;
      case System.TypeCode.Int32: return ic.ToInt32(null) == 0;
      case System.TypeCode.Int64: return ic.ToInt64(null) == 0;
      case System.TypeCode.Byte: return ic.ToByte(null) == 0;
      case System.TypeCode.UInt16: return ic.ToUInt16(null) == 0;
      case System.TypeCode.UInt32: return ic.ToUInt32(null) == 0;
      case System.TypeCode.UInt64: return ic.ToUInt64(null) == 0;
      case System.TypeCode.Boolean: return !ic.ToBoolean(null);
    }
    return false;
  }

}