//-----------------------------------------------------------------------------
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;
using Microsoft.Cci.Immutable;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {

  /// <summary>
  /// Represents CLR Operand stack types
  /// </summary>
  public enum ClrOperandStackType {
    /// <summary>
    /// Operand stack is 32 bit value. It will be treated independent of sign on the stack.
    /// </summary>
    Int32,
    /// <summary>
    /// Operand stack is 64 bit value. It will be treated independent of sign on the stack.
    /// </summary>
    Int64,
    /// <summary>
    /// Operand stack is platform dependent int value. It will be treated independent of sign on the stack.
    /// </summary>
    NativeInt,
    /// <summary>
    /// Operand stack represents a real number. It can be converted to either float or double.
    /// </summary>
    Float,
    /// <summary>
    /// Operand stack is a reference to some type.
    /// </summary>
    Reference,
    /// <summary>
    /// Operand stack is a reference or value type.
    /// </summary>
    Object,
    /// <summary>
    /// Operand stack is a pointer type
    /// </summary>
    Pointer,
    /// <summary>
    /// Operand stack is of invalid type
    /// </summary>
    Invalid,
  }

  /// <summary>
  /// Helper class to get CLR Type manipulation information.
  /// </summary>
  public static class ClrHelper {

    /// <summary>
    /// Gives the Clr operand stack type corresponding to the typeDefinition
    /// </summary>
    /// <param name="typeReference"></param>
    /// <returns></returns>
    public static ClrOperandStackType ClrOperandStackTypeFor(ITypeReference typeReference)
      //^ ensures result >= ClrOperandStackType.Int32 && result <= ClrOperandStackType.Invalid;
    {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<ClrOperandStackType>() >= ClrOperandStackType.Int32 && Contract.Result<ClrOperandStackType>() <= ClrOperandStackType.Invalid);

      var typeDefinition = typeReference.ResolvedType;
      if (typeDefinition.IsEnum) //should only be true for types that can be resolved
        return ClrOperandStackTypeFor(typeDefinition.UnderlyingType.TypeCode);
      else
        return ClrOperandStackTypeFor(typeReference.TypeCode);
    }

    /// <summary>
    /// Gives the Clr operand stack type corresponding to the PrimitiveTypeCode
    /// </summary>
    /// <param name="typeCode"></param>
    /// <returns></returns>
    public static ClrOperandStackType ClrOperandStackTypeFor(PrimitiveTypeCode typeCode)
      //^ ensures result >= ClrOperandStackType.Int32 && result <= ClrOperandStackType.Invalid;
    {
      Contract.Ensures(Contract.Result<ClrOperandStackType>() >= ClrOperandStackType.Int32 && Contract.Result<ClrOperandStackType>() < ClrOperandStackType.Invalid);

      switch (typeCode) {
        case PrimitiveTypeCode.Boolean:
        case PrimitiveTypeCode.Char:
        case PrimitiveTypeCode.Int16:
        case PrimitiveTypeCode.Int32:
        case PrimitiveTypeCode.Int8:
        case PrimitiveTypeCode.UInt16:
        case PrimitiveTypeCode.UInt32:
        case PrimitiveTypeCode.UInt8:
          return ClrOperandStackType.Int32;

        case PrimitiveTypeCode.Int64:
        case PrimitiveTypeCode.UInt64:
          return ClrOperandStackType.Int64;

        case PrimitiveTypeCode.IntPtr:
        case PrimitiveTypeCode.UIntPtr:
          return ClrOperandStackType.NativeInt;

        case PrimitiveTypeCode.Float32:
        case PrimitiveTypeCode.Float64:
          return ClrOperandStackType.Float;

        case PrimitiveTypeCode.Reference:
          return ClrOperandStackType.Reference;

        case PrimitiveTypeCode.Pointer:
          return ClrOperandStackType.Pointer;

        case PrimitiveTypeCode.Invalid:
          return ClrOperandStackType.Invalid;

        default:
          return ClrOperandStackType.Object;
      }
    }

    /// <summary>
    /// Gives the primitive type code corresponding to the ClrOperandStackType
    /// </summary>
    /// <param name="numericType"></param>
    /// <returns></returns>
    public static PrimitiveTypeCode PrimitiveTypeCodeFor(ClrOperandStackType numericType) {
      switch (numericType) {
        case ClrOperandStackType.Int32: return PrimitiveTypeCode.Int32;
        case ClrOperandStackType.Int64: return PrimitiveTypeCode.Int64;
        case ClrOperandStackType.NativeInt: return PrimitiveTypeCode.IntPtr;
        case ClrOperandStackType.Float: return PrimitiveTypeCode.Float64;
        case ClrOperandStackType.Reference: return PrimitiveTypeCode.Reference;
        case ClrOperandStackType.Pointer: return PrimitiveTypeCode.Pointer;
        default: return PrimitiveTypeCode.NotPrimitive;
      }
    }

    /// <summary>
    /// Conversion is possible from value stored on stack of type ClrOpernadStackType to given PrimitiveTypeCode.
    /// </summary>
    /// <param name="fromType"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static bool ConversionPossible(ClrOperandStackType fromType, PrimitiveTypeCode toType) {
      switch (fromType) {
        case ClrOperandStackType.Int32:
        case ClrOperandStackType.Int64:
        case ClrOperandStackType.NativeInt:
        case ClrOperandStackType.Float:
        case ClrOperandStackType.Pointer:
          return true;
        case ClrOperandStackType.Reference:
        case ClrOperandStackType.Object:
          return toType == PrimitiveTypeCode.Int64 || toType == PrimitiveTypeCode.UInt64 || toType == PrimitiveTypeCode.IntPtr || toType == PrimitiveTypeCode.UIntPtr;
        case ClrOperandStackType.Invalid:
        default:
          return false;
      }
    }

    /// <summary>
    /// Table representing the result of add operation with respect to ClrOperand stack.
    /// </summary>
    public static readonly ClrOperandStackType[,] AddResult = new ClrOperandStackType[,]{
      //      Int32                             Int64                       NativeInt                     Float                         Reference                         Object                    Pointer                       Invalid
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Reference, ClrOperandStackType.Invalid, ClrOperandStackType.Pointer,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Int64,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Reference, ClrOperandStackType.Invalid, ClrOperandStackType.Pointer,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Float,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Reference, ClrOperandStackType.Invalid, ClrOperandStackType.Reference, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Pointer,   ClrOperandStackType.Invalid, ClrOperandStackType.Pointer,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
    };

    /// <summary>
    /// Table representing the result of division, multiplication and reminder operation with respect to ClrOperand stack.
    /// </summary>
    public static readonly ClrOperandStackType[,] DivMulRemResult = new ClrOperandStackType[,]{
      //      Int32                             Int64                       NativeInt                     Float                         Reference                         Object                    Pointer                       Invalid
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Int64,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Float,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
    };

    /// <summary>
    /// Table representing the result of substraction operation with respect to ClrOperand stack.
    /// </summary>
    public static readonly ClrOperandStackType[,] SubResult = new ClrOperandStackType[,]{
      //      Int32                             Int64                       NativeInt                     Float                         Reference                         Object                    Pointer                       Invalid
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Int64,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Float,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Reference, ClrOperandStackType.Invalid, ClrOperandStackType.Reference, ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Pointer,   ClrOperandStackType.Invalid, ClrOperandStackType.Pointer,   ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
    };

    /// <summary>
    /// Table representing the result of negation and not operation with respect to ClrOperand stack.
    /// </summary>
    public static readonly ClrOperandStackType[] UnaryResult = new ClrOperandStackType[]
      //      Int32                             Int64                       NativeInt                     Float                         Reference                         Object                    Pointer                       Invalid
      { ClrOperandStackType.Int32, ClrOperandStackType.Int64, ClrOperandStackType.NativeInt, ClrOperandStackType.Float, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid }
    ;

    /// <summary>
    /// Table representing the result of comparision operation with respect to ClrOperand stack.
    /// </summary>
    public static readonly ClrOperandStackType[,] CompResult = new ClrOperandStackType[,]{
      //      Int32                             Int64                       NativeInt                     Float                         Reference                         Object                    Pointer                       Invalid
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Int32,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Int32,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
    };

    /// <summary>
    /// Table representing the result of equality comparision operation with respect to ClrOperand stack.
    /// </summary>
    public static readonly ClrOperandStackType[,] EqCompResult = new ClrOperandStackType[,]{
      //      Int32                             Int64                       NativeInt                     Float                         Reference                         Object                    Pointer                       Invalid
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Int32,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Int32,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Int32,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid},
    };

    /// <summary>
    /// Table representing the result of integer operation (bitwise and, or, xor) with respect to ClrOperand stack.
    /// </summary>
    public static readonly ClrOperandStackType[,] IntOperationResult = new ClrOperandStackType[,]{
      //      Int32                             Int64                       NativeInt                     Float                         Reference                         Object                    Pointer                       Invalid
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Int64,   ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt,   ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
    };

    /// <summary>
    /// Table representing the result of bit shift operation with respect to ClrOperand stack.
    /// </summary>
    public static readonly ClrOperandStackType[,] ShiftOperationResult = new ClrOperandStackType[,]{
      //      Int32                             Int64                       NativeInt                     Float                         Reference                         Object                    Pointer                       Invalid
      {ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Int32,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Int64,     ClrOperandStackType.Invalid, ClrOperandStackType.Int64,     ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.NativeInt, ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
      {ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,   ClrOperandStackType.Invalid, ClrOperandStackType.Invalid,     ClrOperandStackType.Invalid},
    };

    /// <summary>
    /// Table representing the implicit conversion for the purpose of method calls with respect to ClrOperand stack.
    /// </summary>
    public static readonly bool[,] ImplicitConversionPossibleArr = new bool[,]{
      //                    Int32   Int64   NativeInt Float   Reference Object    Pointer   Invalid
      /* Boolean */       { true,   false,  true,     false,  false,    false,    true,     false },
      /* Char */          { true,   false,  true,     false,  false,    false,    true,     false },
      /* Int8 */          { true,   false,  true,     false,  false,    false,    true,     false },
      /* UInt16 */        { true,   false,  true,     false,  false,    false,    true,     false },
      /* Int8 */          { true,   false,  true,     false,  false,    false,    true,     false },
      /* Float32 */       { false,  false,  false,    true,   false,    false,    false,    false },
      /* Float64 */       { false,  false,  false,    true,   false,    false,    false,    false },
      /* Int16 */         { true,   false,  true,     false,  false,    false,    true,     false },
      /* Int32 */         { true,   false,  true,     false,  false,    false,    true,     false },
      /* Int64 */         { false,  true,   false,    false,  false,    false,    false,    false },
      /* IntPtr */        { true,   false,  true,     false,  false,    false,    true,     false },
      /* Pointer */       { true,   false,  true,     false,  false,    false,    true,     false },
      /* Reference */     { false,  false,  true,     false,  true,     false,    true,     false },
      /* UInt8 */         { true,   false,  true,     false,  false,    false,    true,     false },
      /* UInt16 */        { true,   false,  true,     false,  false,    false,    true,     false },
      /* UInt32 */        { true,   false,  true,     false,  false,    false,    true,     false },
      /* UInt64 */        { false,  true,   false,    false,  false,    false,    false,    false },
      /* UIntPtr */       { true,   false,  true,     false,  false,    false,    true,     false },
      /* Void */          { false,  false,  false,    false,  false,    false,    false,    false },
      /* NotPrimitive */  { false,  false,  false,    false,  false,    true,     false,    false },
      /* Invalid */       { false,  false,  false,    false,  false,    false,    false,    false },
    };
  }

  /// <summary>
  /// Options that specify how type and namespace member names should be formatted.
  /// </summary>
  [Flags]
  public enum NameFormattingOptions {
    /// <summary>
    /// Format the name with default options.
    /// </summary>
    None=0,

    /// <summary>
    /// If the type is an instance of System.Nullable&lt;T&gt; format it using a short form, such as T?.
    /// </summary>
    ContractNullable=1,

    /// <summary>
    /// Format for a unique id string like the ones generated in XML reference files. 
    /// <remarks>To generate a truly unique and compliant id, this option should not be used in conjunction with other NameFormattingOptions.</remarks>
    /// </summary>
    DocumentationId=NameFormattingOptions.FormattingForDocumentationId|NameFormattingOptions.DocumentationIdMemberKind|NameFormattingOptions.PreserveSpecialNames|NameFormattingOptions.TypeParameters|NameFormattingOptions.UseGenericTypeNameSuffix|NameFormattingOptions.Signature|NameFormattingOptions.OmitWhiteSpaceAfterListDelimiter,

    /// <summary>
    /// Prefix the kind of member or type to the name. For example "T:System.AppDomain" or "M:System.Object.Equals".
    /// <para>Full list of prefixes: "T:" = Type, "M:" = Method, "F:" = Field, "E:" = Event, "P:" = Property.</para>
    /// </summary>
    DocumentationIdMemberKind=ContractNullable << 1,

    /// <summary>
    /// Include empty type parameter lists with the names of generic types.
    /// </summary>
    EmptyTypeParameterList=DocumentationIdMemberKind << 1,

    /// <summary>
    /// If the name of the member is the same as keyword, format the name using the keyword escape syntax. For example: "@if" rather than just "if".
    /// </summary>
    EscapeKeyword=EmptyTypeParameterList << 1,

    /// <summary>
    /// Perform multiple miscellaneous formatting changes needed for a documentation id.
    /// <remarks>This option does not perform all formatting necessary for a documentation id; instead use the <see cref="DocumentationId"/> option for a complete id string like the ones generated in XML reference files.</remarks>
    /// </summary>
    FormattingForDocumentationId=EscapeKeyword << 1,

    /// <summary>
    /// Prefix the kind of member or type to the name. For example "class System.AppDomain".
    /// </summary>
    MemberKind=FormattingForDocumentationId << 1,

    /// <summary>
    /// Include the type constraints of generic methods in their names.
    /// </summary>
    MethodConstraints=MemberKind << 1,

    /// <summary>
    /// Include modifiers, such as "static" with the name of the member.
    /// </summary>
    Modifiers=MethodConstraints << 1,

    /// <summary>
    /// Do not include the name of the containing namespace in the name of a namespace member.
    /// </summary>
    OmitContainingNamespace=Modifiers << 1,

    /// <summary>
    /// Do not include the name of the containing type in the name of a type member.
    /// </summary>
    OmitContainingType=OmitContainingNamespace << 1,

    /// <summary>
    /// Do not include optional and required custom modifiers.
    /// </summary>
    OmitCustomModifiers = OmitContainingType << 1,

    /// <summary>
    /// If the type member explicitly implements an interface, do not include the name of the interface in the name of the member.
    /// </summary>
    OmitImplementedInterface=OmitCustomModifiers << 1,

    /// <summary>
    /// Do not include type argument names with the names of generic type instances.
    /// </summary>
    OmitTypeArguments=OmitImplementedInterface << 1,

    /// <summary>
    /// Don't insert a space after the delimiter in a list. For example (one,two) rather than (one, two).
    /// </summary>
    OmitWhiteSpaceAfterListDelimiter=OmitTypeArguments << 1,

    /// <summary>
    /// Include the names of parameters in the signatures of methods and indexers.
    /// </summary>
    ParameterName=OmitWhiteSpaceAfterListDelimiter << 1,

    /// <summary>
    /// Include modifiers such as "ref" and "out" in the signatures of methods and indexers.
    /// </summary>
    ParameterModifiers=ParameterName << 1,

    /// <summary>
    /// Do not transform special names such as .ctor and get_PropertyName into language specific notation.
    /// </summary>
    PreserveSpecialNames=ParameterModifiers << 1,

    /// <summary>
    /// Include the name of the return types in the signatures of methods and indexers.
    /// </summary>
    ReturnType=PreserveSpecialNames << 1,

    /// <summary>
    /// Include the parameter types and optionally additional information such as parameter names.
    /// </summary>
    Signature=ReturnType << 1,

    /// <summary>
    /// Include the name of the containing type only if it is needed because of ambiguity or hiding. Include only as much as is needed to resolve this.
    /// Please note: this needs source level information to implement. The default formatters in MetdataHelper ignore this bit.
    /// </summary>
    SmartTypeName=Signature << 1,

    /// <summary>
    /// Include the name of the containing namespace only if it is needed because of ambiguity or hiding. Include only as much as is needed to resolve this.
    /// Please note: this needs source level information to implement. The default formatters in MetdataHelper ignore this bit.
    /// </summary>
    SmartNamespaceName=SmartTypeName << 1,

    /// <summary>
    /// Do not include the "Attribute" suffix in the name of a custom attribute type.
    /// </summary>
    SupressAttributeSuffix=SmartNamespaceName << 1,

    /// <summary>
    /// Include the type parameter constraints of generic types in their names.
    /// </summary>
    TypeConstraints=SupressAttributeSuffix << 1,

    /// <summary>
    /// Include type parameters names with the names of generic types.
    /// </summary>
    TypeParameters=TypeConstraints << 1,

    /// <summary>
    /// Append `n where n is the number of type parameters to the type name.
    /// </summary>
    UseGenericTypeNameSuffix=TypeParameters << 1,

    /// <summary>
    /// Prepend "global::" to all namespace type names whose containing namespace is not omitted, including the case where the namespace type name qualifies a nested type name.
    /// </summary>
    UseGlobalPrefix=UseGenericTypeNameSuffix << 1,

    /// <summary>
    /// Use '+' instead of '.' to delimit the boundary between a containing type name and a nested type name.
    /// </summary>
    UseReflectionStyleForNestedTypeNames=UseGlobalPrefix << 1,

    /// <summary>
    /// If the type corresponds to a keyword use the keyword rather than the type name.
    /// </summary>
    UseTypeKeywords=UseReflectionStyleForNestedTypeNames << 1,

    /// <summary>
    /// Include the visibility of the member in its name.
    /// </summary>
    Visibility=UseTypeKeywords << 1,

  }

  /// <summary>
  /// Helper class for computing information from the structure of ITypeDefinition instances.
  /// </summary>
  public static class TypeHelper {
    /// <summary>
    /// Returns the Base class. If there is no base type it returns null.
    /// </summary>
    /// <param name="typeDef">The type whose base class is to be returned.</param>
    public static ITypeDefinition/*?*/ BaseClass(ITypeDefinition typeDef)
      //^ ensures result == null || result.IsClass;
    {
      Contract.Requires(typeDef != null);
      Contract.Ensures(Contract.Result<ITypeDefinition>() == null || Contract.Result<ITypeDefinition>().IsClass);

      foreach (ITypeReference baseClass in typeDef.BaseClasses) {
        ITypeDefinition bc = baseClass.ResolvedType;
        if (bc.IsClass) return bc;
      }
      //TODO: what about types with more than one base class?
      //Need some way to tell managed types from unmanaged types.
      return null;
    }

    /// <summary>
    /// True if the given type member may be accessed by (code in) the type definition.
    /// For example, if the member is private and the type definition is the containing type of the member,
    /// or is a nested type of the containing type of the member, the result is true.
    /// If the member is internal and the type is defined in a different assembly, then the result is false.
    /// </summary>
    /// <param name="typeDefinition">The type definition from which one wants to access the <paramref name="member"/>.</param>
    /// <param name="member">The type member to check.</param>
    public static bool CanAccess(ITypeDefinition typeDefinition, ITypeDefinitionMember member) {
      Contract.Requires(typeDefinition != null);
      Contract.Requires(member != null);

      if (TypeHelper.TypesAreEquivalent(typeDefinition, member.ContainingTypeDefinition)) return true;
      if (typeDefinition.IsGeneric && TypeHelper.TypesAreEquivalent(typeDefinition.InstanceType, member.ContainingTypeDefinition))
        return true;
      var geninst = member.ContainingTypeDefinition as IGenericTypeInstance;
      if (geninst != null && TypeHelper.TypesAreEquivalent(typeDefinition, geninst.GenericType.ResolvedType)) return true;
      if (!CanAccess(typeDefinition, member.ContainingTypeDefinition)) return false;
      switch (member.Visibility) {
        case TypeMemberVisibility.Assembly:
          return TypeHelper.GetDefiningUnit(typeDefinition).UnitIdentity.Equals(TypeHelper.GetDefiningUnit(member.ContainingTypeDefinition).UnitIdentity);
        //TODO: friend assemblies
        case TypeMemberVisibility.Family:
          return TypeHelper.Type1DerivesFromOrIsTheSameAsType2(typeDefinition, member.ContainingTypeDefinition);
        case TypeMemberVisibility.FamilyAndAssembly:
          return TypeHelper.GetDefiningUnit(typeDefinition).UnitIdentity.Equals(TypeHelper.GetDefiningUnit(member.ContainingTypeDefinition).UnitIdentity) &&
            TypeHelper.Type1DerivesFromOrIsTheSameAsType2(typeDefinition, member.ContainingTypeDefinition);
        //TODO: friend assemblies
        case TypeMemberVisibility.FamilyOrAssembly:
          return TypeHelper.GetDefiningUnit(typeDefinition).UnitIdentity.Equals(TypeHelper.GetDefiningUnit(member.ContainingTypeDefinition)) ||
            TypeHelper.Type1DerivesFromOrIsTheSameAsType2(typeDefinition, member.ContainingTypeDefinition);
        //TODO: friend assemblies
        case TypeMemberVisibility.Public:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// True if the given type <paramref name="typeDefinition"/> (i.e., code within that type) may access the type <paramref name="otherTypeDefinition"/>.
    /// </summary>
    /// <param name="typeDefinition">The type definition from which one wants to access the <paramref name="otherTypeDefinition"/>.</param>
    /// <param name="otherTypeDefinition">The type to check.</param>
    public static bool CanAccess(ITypeDefinition typeDefinition, ITypeDefinition otherTypeDefinition) {
      Contract.Requires(typeDefinition != null);
      Contract.Requires(otherTypeDefinition != null);

      if (TypeHelper.TypesAreEquivalent(typeDefinition, otherTypeDefinition)) return true;
      if (typeDefinition.IsGeneric && TypeHelper.TypesAreEquivalent(typeDefinition.InstanceType, typeDefinition))
        return true;
      var nsTypeDef = otherTypeDefinition as INamespaceTypeDefinition;
      if (nsTypeDef != null) {
        if (nsTypeDef.IsPublic) return true;
        return TypeHelper.GetDefiningUnit(nsTypeDef).Equals(TypeHelper.GetDefiningUnit(typeDefinition)); //TODO: worry about /addmodule
      }
      INestedTypeDefinition/*?*/ nestedTypeDef = otherTypeDefinition as INestedTypeDefinition;
      if (nestedTypeDef != null) return CanAccess(typeDefinition, (ITypeDefinitionMember)nestedTypeDef);
      IManagedPointerType/*?*/ managedPointerType = otherTypeDefinition as IManagedPointerType;
      if (managedPointerType != null) return CanAccess(typeDefinition, managedPointerType.TargetType.ResolvedType);
      IPointerType/*?*/ pointerType = otherTypeDefinition as IPointerType;
      if (pointerType != null) return CanAccess(typeDefinition, pointerType.TargetType.ResolvedType);
      IArrayType/*?*/ arrayType = otherTypeDefinition as IArrayType;
      if (arrayType != null) return CanAccess(typeDefinition, arrayType.ElementType.ResolvedType);
      IGenericTypeInstance/*?*/ genericTypeInstance = otherTypeDefinition as IGenericTypeInstance;
      if (genericTypeInstance != null) {
        if (!CanAccess(typeDefinition, genericTypeInstance.GenericType.ResolvedType)) return false;
        foreach (var typeRef in genericTypeInstance.GenericArguments) {
          if (!CanAccess(typeDefinition, typeRef.ResolvedType)) return false;
        }
        return true;
      }
      IGenericTypeParameter/*?*/ genericParameter = otherTypeDefinition as IGenericTypeParameter;
      if (genericParameter != null) return TypeHelper.TypesAreEquivalent(genericParameter.DefiningType, typeDefinition);
      return false;
    }

    /// <summary>
    /// Returns the most derived common base class that all types that satisfy the constraints of the given
    /// generic parameter must derive from.
    /// </summary>
    [Pure]
    public static ITypeDefinition EffectiveBaseClass(IGenericParameter genericParameter) {
      Contract.Requires(genericParameter != null);
      Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
      Contract.Ensures(Contract.Result<ITypeDefinition>() == Dummy.TypeDefinition || Contract.Result<ITypeDefinition>().IsClass);

      ITypeDefinition result = Dummy.TypeDefinition;
      if (genericParameter.MustBeValueType) {
        result = genericParameter.PlatformType.SystemValueType.ResolvedType;
        if (result is Dummy || !result.IsClass) return Dummy.TypeDefinition;
        return result;
      }

      Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
      foreach (ITypeReference cref in genericParameter.Constraints) {
        Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
        ITypeDefinition constraint = cref.ResolvedType;
        ITypeDefinition baseClass;
        if (constraint.IsClass) {
          baseClass = constraint;
        } else {
          IGenericParameter/*?*/ tpConstraint = constraint as IGenericParameter;
          if (tpConstraint == null) {
            Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
            continue;
          }
          baseClass = TypeHelper.EffectiveBaseClass(tpConstraint);
          Contract.Assert(baseClass == Dummy.TypeDefinition || baseClass.IsClass);
          if (TypeHelper.TypesAreEquivalent(baseClass, genericParameter.PlatformType.SystemObject)) {
            Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
            continue;
          }
        }
        Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
        Contract.Assert(baseClass == Dummy.TypeDefinition || baseClass.IsClass);
        if (result is Dummy) {
          result = baseClass;
          Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
        } else if (!(baseClass is Dummy)) {
          Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
          ITypeDefinition/*?*/ bc = TypeHelper.MostDerivedCommonBaseClass(result, baseClass);
          Contract.Assert(bc == null || bc.IsClass); //Could be null if System.Object cannot be resolved
          Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
          if (bc != null) {
            Contract.Assert(bc.IsClass);
            result = bc;
          }
          Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
        }
        Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
      }
      Contract.Assert(result == Dummy.TypeDefinition || result.IsClass);
      if (result is Dummy) {
        result = genericParameter.PlatformType.SystemObject.ResolvedType;
        Contract.Assume(result == Dummy.TypeDefinition || result.IsClass);
      }
      return result;
    }

    /// <summary>
    /// Returns true if the given type, one of its containing types, has the System.Runtime.CompilerServices.CompilerGeneratedAttribute.
    /// </summary>
    public static bool IsCompilerGenerated(ITypeDefinition type) {
      Contract.Requires(type != null);

      while (true) {
        if (AttributeHelper.Contains(type.Attributes, type.PlatformType.SystemRuntimeCompilerServicesCompilerGeneratedAttribute)) return true;
        var nestedType = type as INestedTypeDefinition;
        if (nestedType == null) return false;
        type = nestedType.ContainingTypeDefinition;
      }
    }

    /// <summary>
    /// Returns true if the given type, one of its containing types, has the System.Runtime.CompilerServices.TypeIdentifierAttribute.
    /// </summary>
    public static bool IsEmbeddedInteropType(ITypeDefinition type) {
      Contract.Requires(type != null);

      while (true) {
        if (AttributeHelper.Contains(type.Attributes, type.PlatformType.SystemRuntimeInteropServicesTypeIdentifierAttribute)) return true;
        var nestedType = type as INestedTypeDefinition;
        if (nestedType == null) return false;
        type = nestedType.ContainingTypeDefinition;
      }
    }

    /// <summary>
    /// Returns true a value of this type can be treated as a compile time constant.
    /// Such values need not be stored in memory in order to be representable. For example, they can appear as part of a CLR instruction.
    /// </summary>
    public static bool IsCompileTimeConstantType(ITypeReference type) {
      Contract.Requires(type != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Boolean:
        case PrimitiveTypeCode.Char:
        case PrimitiveTypeCode.Int16:
        case PrimitiveTypeCode.Int32:
        case PrimitiveTypeCode.Int64:
        case PrimitiveTypeCode.Int8:
        case PrimitiveTypeCode.UInt16:
        case PrimitiveTypeCode.UInt32:
        case PrimitiveTypeCode.UInt64:
        case PrimitiveTypeCode.UInt8:
        case PrimitiveTypeCode.Float32:
        case PrimitiveTypeCode.Float64:
        case PrimitiveTypeCode.String:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Returns true if the given type is a generic parameter or if it is a structural type that contains a reference to a generic parameter.
    /// </summary>
    public static bool IsOpen(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);

      var genericParameter = typeReference as IGenericParameterReference;
      if (genericParameter != null) return true;
      var arrayType = typeReference as IArrayTypeReference;
      if (arrayType != null) return IsOpen(arrayType.ElementType);
      var managedPointerType = typeReference as IManagedPointerTypeReference;
      if (managedPointerType != null) return IsOpen(managedPointerType.TargetType);
      var modifiedPointer = typeReference as ModifiedPointerType;
      if (modifiedPointer != null) return IsOpen(modifiedPointer.TargetType);
      var modifiedType = typeReference as IModifiedTypeReference;
      if (modifiedType != null) return IsOpen(modifiedType.UnmodifiedType);
      var pointerType = typeReference as IPointerTypeReference;
      if (pointerType != null) return IsOpen(pointerType.TargetType);
      var nestedType = typeReference as ISpecializedNestedTypeReference;
      if (nestedType != null) return IsOpen(nestedType.ContainingType);
      var genericTypeInstance = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) {
        foreach (var genArg in genericTypeInstance.GenericArguments) {
          if (IsOpen(genArg)) return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Returns true if the CLR allows integer operators to be applied to values of the given type.
    /// </summary>
    public static bool IsPrimitiveInteger(ITypeReference type) {
      Contract.Requires(type != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Char:
        case PrimitiveTypeCode.Int16:
        case PrimitiveTypeCode.Int32:
        case PrimitiveTypeCode.Int64:
        case PrimitiveTypeCode.Int8:
        case PrimitiveTypeCode.UInt16:
        case PrimitiveTypeCode.UInt32:
        case PrimitiveTypeCode.UInt64:
        case PrimitiveTypeCode.UInt8:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Returns true if the CLR allows signed integer operators to be applied to values of the given type.
    /// </summary>
    public static bool IsSignedPrimitiveInteger(ITypeReference type) {
      Contract.Requires(type != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Int16:
        case PrimitiveTypeCode.Int32:
        case PrimitiveTypeCode.Int64:
        case PrimitiveTypeCode.Int8:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Returns true if the CLR allows signed comparison operators to be applied to values of the given type.
    /// </summary>
    public static bool IsSignedPrimitive(ITypeReference type) {
      Contract.Requires(type != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Int16:
        case PrimitiveTypeCode.Int32:
        case PrimitiveTypeCode.Int64:
        case PrimitiveTypeCode.Int8:
        case PrimitiveTypeCode.IntPtr:
        case PrimitiveTypeCode.Float32:
        case PrimitiveTypeCode.Float64:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Returns true if the CLR allows unsigned integer operators to be applied to values of the given type.
    /// </summary>
    public static bool IsUnsignedPrimitiveInteger(ITypeReference type) {
      Contract.Requires(type != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Char:
        case PrimitiveTypeCode.UInt16:
        case PrimitiveTypeCode.UInt32:
        case PrimitiveTypeCode.UInt64:
        case PrimitiveTypeCode.UInt8:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Returns true if the CLR allows unsigned comparison operators to be applied to values of the given type.
    /// </summary>
    public static bool IsUnsignedPrimitive(ITypeReference type) {
      Contract.Requires(type != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Char:
        case PrimitiveTypeCode.UInt16:
        case PrimitiveTypeCode.UInt32:
        case PrimitiveTypeCode.UInt64:
        case PrimitiveTypeCode.UInt8:
        case PrimitiveTypeCode.UIntPtr:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Decides if the given type definition is visible to assemblies other than the assembly it is defined in (and other than its friends).
    /// </summary>
    public static bool IsVisibleOutsideAssembly(ITypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);

      var nestedType = typeDefinition as INestedTypeDefinition;
      if (nestedType != null && !TypeHelper.IsVisibleOutsideAssembly(nestedType.ContainingTypeDefinition)) return false;
      switch (TypeHelper.TypeVisibilityAsTypeMemberVisibility(typeDefinition)) {
        case TypeMemberVisibility.Public:
        case TypeMemberVisibility.Family:
        case TypeMemberVisibility.FamilyOrAssembly:
          return true;
      }
      return false;
    }

    /// <summary>
    /// Decides if the given type definition would visible to an assembly listed in an InternalsVisibleTo attribute of the assembly defining
    /// the given type. It is not necessary for the assembly to actually have such an attribute.
    /// </summary>
    public static bool IsVisibleToFriendAssemblies(ITypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);

      var nestedType = typeDefinition as INestedTypeDefinition;
      if (nestedType != null && !TypeHelper.IsVisibleToFriendAssemblies(nestedType.ContainingTypeDefinition)) return false;
      switch (TypeHelper.TypeVisibilityAsTypeMemberVisibility(typeDefinition)) {
        case TypeMemberVisibility.Public:
        case TypeMemberVisibility.Family:
        case TypeMemberVisibility.FamilyOrAssembly:
        case TypeMemberVisibility.FamilyAndAssembly:
        case TypeMemberVisibility.Assembly:
          return true;
      }
      return false;
    }

    /// <summary>
    /// If both type references can be resolved, this returns the merged type of two types as per the verification algorithm in CLR.
    /// Otherwise it returns either type1, or type2 or System.Object, depending on how much is known about either type.
    /// </summary>
    [Pure]
    public static ITypeReference MergedType(ITypeReference type1, ITypeReference type2) {
      Contract.Requires(type1 != null);
      Contract.Requires(type2 != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      if (TypesAreEquivalent(type1, type2)) return type1;
      if (StackTypesAreEquivalent(type1, type2)) return StackType(type1);
      var typeDef1 = type1.ResolvedType;
      var typeDef2 = type2.ResolvedType;
      if (!(typeDef1 is Dummy || typeDef2 is Dummy))
        return MergedType(typeDef1, typeDef2);
      if (!(typeDef1 is Dummy)) {
        if (Type1ImplementsType2(typeDef1, type2)) return type2;
      } else if (!(typeDef2 is Dummy)) {
        if (Type1ImplementsType2(typeDef2, type1)) return type1;
      }
      return type1.PlatformType.SystemObject;
    }

    /// <summary>
    /// Returns the merged type of two types as per the verification algorithm in CLR.
    /// If the types cannot be merged, then it returns System.Object.
    /// </summary>
    [Pure]
    public static ITypeReference MergedType(ITypeDefinition type1, ITypeDefinition type2) {
      Contract.Requires(type1 != null);
      Contract.Requires(type2 != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      if (TypesAreEquivalent(type1, type2)) return type1;
      if (StackTypesAreEquivalent(type1, type2)) return StackType(type1);
      if (TypeHelper.TypesAreAssignmentCompatible(type1, type2))
        return type2;
      if (TypeHelper.TypesAreAssignmentCompatible(type2, type1))
        return type1;
      ITypeDefinition/*?*/ lcbc = TypeHelper.MostDerivedCommonBaseClass(type1, type2);
      if (lcbc != null) return lcbc;
      return type1.PlatformType.SystemObject;
    }

    /// <summary>
    /// Returns the most accessible visibility that is not greater than the given visibility and the visibilities of each of the given typeArguments.
    /// For the purpose of computing the intersection, namespace types are treated as being TypeMemberVisibility.Public or TypeMemberVisibility.Assembly.
    /// Generic type instances are treated as having a visibility that is the intersection of the generic type's visibility and all of the type arguments' visibilities.
    /// </summary>
    public static TypeMemberVisibility GenericInstanceVisibilityAsTypeMemberVisibility(TypeMemberVisibility templateVisibility, IEnumerable<ITypeReference> typeArguments) {
      Contract.Requires(typeArguments != null);
      Contract.Requires(Contract.ForAll(typeArguments, x => x != null));

      TypeMemberVisibility result = templateVisibility & TypeMemberVisibility.Mask;
      foreach (ITypeReference typeArgument in typeArguments) {
        TypeMemberVisibility argumentVisibility = TypeVisibilityAsTypeMemberVisibility(typeArgument.ResolvedType);
        result = VisibilityIntersection(result, argumentVisibility);
      }
      return result;
    }

    /// <summary>
    /// Returns a TypeMemberVisibility value that corresponds to the visibility of the given type definition.
    /// Namespace types are treated as being TypeMemberVisibility.Public or TypeMemberVisibility.Assembly.
    /// Generic type instances are treated as having a visibility that is the intersection of the generic type's visibility and all of the type arguments' visibilities.
    /// </summary>
    [Pure]
    public static TypeMemberVisibility TypeVisibilityAsTypeMemberVisibility(ITypeDefinition type) {
      Contract.Requires(type != null);

      TypeMemberVisibility result = TypeMemberVisibility.Public; // supposedly the only thing that doesn't meet any of the below tests are type parameters and their "default" is public.
      INamespaceTypeDefinition/*?*/ nsType = type as INamespaceTypeDefinition;
      if (nsType != null)
        result = nsType.IsPublic ? TypeMemberVisibility.Public : TypeMemberVisibility.Assembly;
      else {
        INestedTypeDefinition/*?*/ neType = type as INestedTypeDefinition;
        if (neType != null) {
          result = neType.Visibility & TypeMemberVisibility.Mask;
        } else {
          IGenericTypeInstanceReference/*?*/ genType = type as IGenericTypeInstanceReference;
          if (genType != null) {
            result = TypeHelper.GenericInstanceVisibilityAsTypeMemberVisibility(TypeVisibilityAsTypeMemberVisibility(genType.GenericType.ResolvedType), genType.GenericArguments);
          }
        }
      }
      return result;
    }

    /// <summary>
    /// If the given type is a generic type instance, return the unspecialized version of the generic type.
    /// If the given type is a specialized nested type, return its unspecialized version. 
    /// Otherwise just return the type itself.
    /// </summary>
    public static ITypeReference UninstantiateAndUnspecialize(ITypeReference type) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);
      Contract.Ensures(!(Contract.Result<ITypeReference>() is IGenericTypeInstanceReference));
      Contract.Ensures(!(Contract.Result<ITypeReference>() is ISpecializedNestedTypeReference));

      var genericTypeInstance = type as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) return TypeHelper.UninstantiateAndUnspecialize(genericTypeInstance.GenericType);
      var specializedNestedType = type as ISpecializedNestedTypeReference;
      if (specializedNestedType != null) return specializedNestedType.UnspecializedVersion;
      return type;
    }

    /// <summary>
    /// If the given type is a specialized nested type, return its unspecialized version. Otherwise just return the type itself.
    /// </summary>
    public static INestedTypeReference Unspecialize(INestedTypeReference nestedType) {
      var specializedNestedType = nestedType as ISpecializedNestedTypeReference;
      if (specializedNestedType != null) return specializedNestedType.UnspecializedVersion;
      return nestedType;
    }

    /// <summary>
    /// Returns a TypeMemberVisibility value that is as accessible as possible while being no more accessible than either of the two given visibilities.
    /// </summary>
    [Pure]
    public static TypeMemberVisibility VisibilityIntersection(TypeMemberVisibility visibility1, TypeMemberVisibility visibility2) {
      TypeMemberVisibility result = TypeMemberVisibility.Default;
      switch (visibility1) {
        case TypeMemberVisibility.Assembly:
          switch (visibility2) {
            case TypeMemberVisibility.Assembly: result = TypeMemberVisibility.Assembly; break;
            case TypeMemberVisibility.Family: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.FamilyAndAssembly: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.FamilyOrAssembly: result = TypeMemberVisibility.Assembly; break;
            case TypeMemberVisibility.Private: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.Public: result = TypeMemberVisibility.Assembly; break;
            default: break;
          }
          break;
        case TypeMemberVisibility.Family:
          switch (visibility2) {
            case TypeMemberVisibility.Assembly: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.Family: result = TypeMemberVisibility.Family; break;
            case TypeMemberVisibility.FamilyAndAssembly: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.FamilyOrAssembly: result = TypeMemberVisibility.Family; break;
            case TypeMemberVisibility.Private: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.Public: result = TypeMemberVisibility.Family; break;
            default: break;
          }
          break;
        case TypeMemberVisibility.FamilyAndAssembly:
          switch (visibility2) {
            case TypeMemberVisibility.Assembly: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.Family: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.FamilyAndAssembly: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.FamilyOrAssembly: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.Private: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.Public: result = TypeMemberVisibility.FamilyAndAssembly; break;
            default: break;
          }
          break;
        case TypeMemberVisibility.FamilyOrAssembly:
          switch (visibility2) {
            case TypeMemberVisibility.Assembly: result = TypeMemberVisibility.Assembly; break;
            case TypeMemberVisibility.Family: result = TypeMemberVisibility.Family; break;
            case TypeMemberVisibility.FamilyAndAssembly: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.FamilyOrAssembly: result = TypeMemberVisibility.FamilyOrAssembly; break;
            case TypeMemberVisibility.Private: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.Public: result = TypeMemberVisibility.FamilyOrAssembly; break;
            default: break;
          }
          break;
        case TypeMemberVisibility.Private:
          switch (visibility2) {
            case TypeMemberVisibility.Assembly: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.Family: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.FamilyAndAssembly: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.FamilyOrAssembly: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.Private: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.Public: result = TypeMemberVisibility.Private; break;
            default: break;
          }
          break;
        case TypeMemberVisibility.Public:
          switch (visibility2) {
            case TypeMemberVisibility.Assembly: result = TypeMemberVisibility.Assembly; break;
            case TypeMemberVisibility.Family: result = TypeMemberVisibility.Family; break;
            case TypeMemberVisibility.FamilyAndAssembly: result = TypeMemberVisibility.FamilyAndAssembly; break;
            case TypeMemberVisibility.FamilyOrAssembly: result = TypeMemberVisibility.FamilyOrAssembly; break;
            case TypeMemberVisibility.Private: result = TypeMemberVisibility.Private; break;
            case TypeMemberVisibility.Public: result = TypeMemberVisibility.Public; break;
            default: break;
          }
          break;
        default:
          result = visibility2;
          break;
      }
      return result;
    }

    /// <summary>
    /// Returns the most nested unit namespace that encloses the given named type definition.
    /// </summary>
    public static IUnitNamespace GetDefiningNamespace(INamedTypeDefinition namedTypeDefinition) {
      Contract.Requires(namedTypeDefinition != null);

      var genericMethodParameter = namedTypeDefinition as IGenericMethodParameter;
      if (genericMethodParameter != null)
        namedTypeDefinition = (INamedTypeDefinition)genericMethodParameter.DefiningMethod.ContainingTypeDefinition;
      else {
        var genericTypeParameter = namedTypeDefinition as IGenericTypeParameter;
        if (genericTypeParameter != null)
          namedTypeDefinition = (INamedTypeDefinition)genericTypeParameter.DefiningType;
      }
      var nestedTypeDefinition = namedTypeDefinition as INestedTypeDefinition;
      while (nestedTypeDefinition != null) {
        namedTypeDefinition = (INamedTypeDefinition)nestedTypeDefinition.ContainingTypeDefinition;
        nestedTypeDefinition = namedTypeDefinition as INestedTypeDefinition;
      }
      var namespaceTypeDefinition = namedTypeDefinition as INamespaceTypeDefinition;
      if (namespaceTypeDefinition != null) return namespaceTypeDefinition.ContainingUnitNamespace;
      return Dummy.UnitNamespace;
    }

    /// <summary>
    /// Returns the unit that defines the given type. If the type is a structural type, such as a pointer the result is 
    /// the defining unit of the element type, or in the case of a generic type instance, the definining type of the generic template type.
    /// </summary>
    public static IUnit GetDefiningUnit(ITypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);

      INestedTypeDefinition/*?*/ nestedTypeDefinition = typeDefinition as INestedTypeDefinition;
      while (nestedTypeDefinition != null) {
        typeDefinition = nestedTypeDefinition.ContainingTypeDefinition;
        nestedTypeDefinition = typeDefinition as INestedTypeDefinition;
      }
      INamespaceTypeDefinition/*?*/ namespaceTypeDefinition = typeDefinition as INamespaceTypeDefinition;
      if (namespaceTypeDefinition != null) return namespaceTypeDefinition.ContainingUnitNamespace.Unit;
      IGenericTypeInstance/*?*/ genericTypeInstance = typeDefinition as IGenericTypeInstance;
      if (genericTypeInstance != null) return TypeHelper.GetDefiningUnit(genericTypeInstance.GenericType.ResolvedType);
      IManagedPointerType/*?*/ managedPointerType = typeDefinition as IManagedPointerType;
      if (managedPointerType != null) return TypeHelper.GetDefiningUnit(managedPointerType.TargetType.ResolvedType);
      IPointerType/*?*/ pointerType = typeDefinition as IPointerType;
      if (pointerType != null) return TypeHelper.GetDefiningUnit(pointerType.TargetType.ResolvedType);
      IArrayType/*?*/ arrayType = typeDefinition as IArrayType;
      if (arrayType != null) return TypeHelper.GetDefiningUnit(arrayType.ElementType.ResolvedType);
      IGenericTypeParameter/*?*/ genericTypeParameter = typeDefinition as IGenericTypeParameter;
      if (genericTypeParameter != null) return TypeHelper.GetDefiningUnit(genericTypeParameter.DefiningType);
      IGenericMethodParameter/*?*/ genericMethodParameter = typeDefinition as IGenericMethodParameter;
      if (genericMethodParameter != null) return TypeHelper.GetDefiningUnit(genericMethodParameter.DefiningMethod.ContainingType.ResolvedType);
      IFunctionPointer/*?*/ functionPointer = typeDefinition as IFunctionPointer;
      if (functionPointer != null) return TypeHelper.GetDefiningUnit(functionPointer.Type.ResolvedType);
      Contract.Assume(false);
      return Dummy.Unit;
    }

    /// <summary>
    /// Returns a reference to the unit that defines the given type. If the reference type is a reference to a structural type, such as a pointer the result is 
    /// the a reference to the defining unit of the element type, or in the case of a generic type instance, the definining type of the generic template type.
    /// </summary>
    public static IUnitReference GetDefiningUnitReference(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<IUnitReference>() != null);

      INestedTypeReference/*?*/ nestedTypeReference = typeReference as INestedTypeReference;
      while (nestedTypeReference != null) {
        typeReference = nestedTypeReference.ContainingType;
        nestedTypeReference = typeReference as INestedTypeReference;
      }
      INamespaceTypeReference/*?*/ namespaceTypeReference = typeReference as INamespaceTypeReference;
      if (namespaceTypeReference != null) return namespaceTypeReference.ContainingUnitNamespace.Unit;
      IGenericTypeInstanceReference/*?*/ genericTypeInstanceReference = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstanceReference != null) return TypeHelper.GetDefiningUnitReference(genericTypeInstanceReference.GenericType);
      IManagedPointerTypeReference/*?*/ managedPointerTypeReference = typeReference as IManagedPointerTypeReference;
      if (managedPointerTypeReference != null) return TypeHelper.GetDefiningUnitReference(managedPointerTypeReference.TargetType);
      IPointerTypeReference/*?*/ pointerTypeReference = typeReference as IPointerTypeReference;
      if (pointerTypeReference != null) return TypeHelper.GetDefiningUnitReference(pointerTypeReference.TargetType);
      IArrayTypeReference/*?*/ arrayTypeReference = typeReference as IArrayTypeReference;
      if (arrayTypeReference != null) return TypeHelper.GetDefiningUnitReference(arrayTypeReference.ElementType);
      IGenericTypeParameterReference/*?*/ genericTypeParameterReference = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameterReference != null) return TypeHelper.GetDefiningUnitReference(genericTypeParameterReference.DefiningType);
      IGenericMethodParameterReference/*?*/ genericMethodParameterReference = typeReference as IGenericMethodParameterReference;
      if (genericMethodParameterReference != null) return TypeHelper.GetDefiningUnitReference(genericMethodParameterReference.DefiningMethod.ContainingType);
      IFunctionPointerTypeReference/*?*/ functionPointerTypeReference = typeReference as IFunctionPointerTypeReference;
      if (functionPointerTypeReference != null) return TypeHelper.GetDefiningUnitReference(functionPointerTypeReference.Type);
      Contract.Assume(false);
      return Dummy.UnitReference;
    }

    /// <summary>
    /// Returns an event of the given declaring type that has the given name.
    /// If no such event can be found, Dummy.EventDefinition is returned.
    /// </summary>
    /// <param name="declaringType">The type thats declares the event.</param>
    /// <param name="eventName">The name of the event.</param>
    public static IEventDefinition GetEvent(ITypeDefinition declaringType, IName eventName) {
      Contract.Requires(declaringType != null);
      Contract.Requires(eventName != null);

      foreach (ITypeDefinitionMember member in declaringType.GetMembersNamed(eventName, false)) {
        IEventDefinition/*?*/ eventDef = member as IEventDefinition;
        if (eventDef != null) return eventDef;
      }
      return Dummy.EventDefinition;
    }

    /// <summary>
    /// Returns a field of the given declaring type that has the given name.
    /// If no such field can be found, Dummy.FieldDefinition is returned.
    /// </summary>
    /// <param name="declaringType">The type thats declares the field.</param>
    /// <param name="fieldName">The name of the field.</param>
    public static IFieldDefinition GetField(ITypeDefinition declaringType, IName fieldName) {
      Contract.Requires(declaringType != null);
      Contract.Requires(fieldName != null);
      Contract.Ensures(Contract.Result<IFieldDefinition>() != null);

      foreach (ITypeDefinitionMember member in declaringType.GetMembersNamed(fieldName, false)) {
        IFieldDefinition/*?*/ field = member as IFieldDefinition;
        if (field != null) return field;
      }
      return Dummy.FieldDefinition;
    }

    /// <summary>
    /// Returns a field of the given declaring type that has the same name and signature as the given field reference.
    /// If no such field can be found, Dummy.FieldDefinition is returned.
    /// </summary>
    /// <param name="declaringType">The type thats declares the field.</param>
    /// <param name="fieldReference">A reference to the field.</param>
    /// <param name="resolveTypes">True if type references should be resolved during signature matching.</param>
    public static IFieldDefinition GetField(ITypeDefinition declaringType, IFieldReference fieldReference, bool resolveTypes = false) {
      Contract.Requires(declaringType != null);
      Contract.Requires(fieldReference != null);
      Contract.Ensures(Contract.Result<IFieldDefinition>() != null);

      foreach (ITypeDefinitionMember member in declaringType.GetMembersNamed(fieldReference.Name, false)) {
        IFieldDefinition/*?*/ field = member as IFieldDefinition;
        if (field == null) continue;
        if (!TypeHelper.TypesAreEquivalent(field.Type, fieldReference.Type, resolveTypes)) continue;
        //TODO: check that custom modifiers are the same
        return field;
      }
      foreach (ITypeDefinitionMember member in declaringType.PrivateHelperMembers) {
        IFieldDefinition/*?*/ field = member as IFieldDefinition;
        if (field == null) continue;
        if (field.Name.UniqueKey != fieldReference.Name.UniqueKey) continue;
        if (!TypeHelper.TypesAreEquivalent(field.Type, fieldReference.Type, resolveTypes)) continue;
        //TODO: check that custom modifiers are the same
        return field;
      }
      return Dummy.FieldDefinition;
    }

    /// <summary>
    /// Returns a method of the given declaring type that has the given name and that matches the given parameter types.
    /// If no such method can be found, Dummy.MethodDefinition is returned.
    /// </summary>
    /// <param name="declaringType">The type that declares the method to be returned.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="parameterTypes">A list of types that should correspond to the parameter types of the returned method.</param>
    [Pure]
    public static IMethodDefinition GetMethod(ITypeDefinition declaringType, IName methodName, params ITypeReference[] parameterTypes) {
      Contract.Requires(declaringType != null);
      Contract.Requires(methodName != null);
      Contract.Requires(parameterTypes != null);
      //Contract.Requires(Contract.ForAll(parameterTypes, x => x != null));
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);

      return TypeHelper.GetMethod(declaringType.GetMembersNamed(methodName, false), methodName, parameterTypes);
    }

    /// <summary>
    /// Returns the first method, if any, of the given list of type members that has the given name and that matches the given parameter types.
    /// If no such method can be found, Dummy.MethodDefinition is returned.
    /// </summary>
    /// <param name="members">A list of type members.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="parameterTypes">A list of types that should correspond to the parameter types of the returned method.</param>
    [Pure]
    public static IMethodDefinition GetMethod(IEnumerable<ITypeDefinitionMember> members, IName methodName, params ITypeReference[] parameterTypes) {
      Contract.Requires(members != null);
      Contract.Requires(Contract.ForAll(members, x => x != null));
      Contract.Requires(methodName != null);
      Contract.Requires(parameterTypes != null);
      //Contract.Requires(Contract.ForAll(parameterTypes, x => x != null));
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);

      foreach (ITypeDefinitionMember member in members) {
        IMethodDefinition/*?*/ meth = member as IMethodDefinition;
        if (meth != null && meth.Name.UniqueKey == methodName.UniqueKey && meth.ParameterCount == parameterTypes.Length) {
          bool parametersMatch = true;
          int i = 0;
          foreach (IParameterDefinition parDef in meth.Parameters) {
            if (!TypeHelper.TypesAreEquivalent(parDef.Type, parameterTypes[i++])) {
              parametersMatch = false;
              break;
            }
            if (parDef.IsByReference || parDef.IsOut || parDef.IsModified) {
              parametersMatch = false;
              break;
            }
          }
          if (parametersMatch) return meth;
        }
      }
      return Dummy.MethodDefinition;
    }

    /// <summary>
    /// Returns a method of the given declaring type that matches the given method reference.
    /// If no such method can be found, Dummy.MethodDefinition is returned.
    /// </summary>
    /// <param name="declaringType">The type that declares the method to be returned.</param>
    /// <param name="methodReference">A method reference whose name and signature matches that of the desired result.</param>
    /// <param name="resolveTypes">True if type references should be resolved during signature matching.</param>
    public static IMethodDefinition GetMethod(ITypeDefinition declaringType, IMethodReference methodReference, bool resolveTypes = false) {
      Contract.Requires(declaringType != null);
      Contract.Requires(methodReference != null);
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);

      IMethodDefinition result = TypeHelper.GetMethod(declaringType.GetMembersNamed(methodReference.Name, false), methodReference, resolveTypes);
      if (result is Dummy) {
        foreach (ITypeDefinitionMember member in declaringType.PrivateHelperMembers) {
          IMethodDefinition/*?*/ meth = member as IMethodDefinition;
          if (meth == null) continue;
          if (meth.Name.UniqueKey != methodReference.Name.UniqueKey) continue;
          if (meth.GenericParameterCount != methodReference.GenericParameterCount) continue;
          if (meth.ParameterCount != methodReference.ParameterCount) continue;
          if (MemberHelper.SignaturesAreEqual(meth, methodReference, resolveTypes)) return meth;
        }
      }
      return result;
    }

    /// <summary>
    /// Gets the Invoke method from the delegate. Returns Dummy.MethodDefinition if the delegate type is malformed.
    /// </summary>
    /// <param name="delegateType">A delegate type.</param>
    /// <param name="host">The host application that provided the nametable used by delegateType.</param>
    public static IMethodDefinition GetInvokeMethod(ITypeDefinition delegateType, IMetadataHost host)
      //^ requires delegateType.IsDelegate;
    {
      Contract.Requires(delegateType != null);
      Contract.Requires(host != null);
      Contract.Requires(delegateType.IsDelegate);
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);

      foreach (ITypeDefinitionMember member in delegateType.GetMembersNamed(host.NameTable.Invoke, false)) {
        IMethodDefinition/*?*/ method = member as IMethodDefinition;
        if (method != null) return method;
      }
      return Dummy.MethodDefinition; //Should get here only when the delegate type is obtained from a malformed or malicious referenced assembly.
    }

    /// <summary>
    /// Returns the first method, if any, of the given list of type members that matches the signature of the given method.
    /// If no such method can be found, Dummy.MethodDefinition is returned.
    /// </summary>
    /// <param name="members">A list of type members.</param>
    /// <param name="methodSignature">A method whose signature matches that of the desired result.</param>
    /// <param name="resolveTypes">True if type references should be resolved during signature matching.</param>
    public static IMethodDefinition GetMethod(IEnumerable<ITypeDefinitionMember> members, IMethodReference methodSignature, bool resolveTypes = false) {
      Contract.Requires(members != null);
      Contract.Requires(Contract.ForAll(members, x => x != null));
      Contract.Requires(methodSignature != null);
      Contract.Ensures(Contract.Result<IMethodDefinition>() != null);

      foreach (ITypeDefinitionMember member in members) {
        IMethodDefinition/*?*/ meth = member as IMethodDefinition;
        if (meth == null) continue;
        if (meth.GenericParameterCount != methodSignature.GenericParameterCount) continue;
        if (meth.ParameterCount != methodSignature.ParameterCount) continue;
        if (meth.IsGeneric) {
          if (MemberHelper.GenericMethodSignaturesAreEqual(meth, methodSignature, resolveTypes)) return meth;
        } else {
          if (MemberHelper.SignaturesAreEqual(meth, methodSignature, resolveTypes)) return meth;
        }
      }
      return Dummy.MethodDefinition;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given namespace definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    public static string GetNamespaceName(IUnitSetNamespace namespaceDefinition, NameFormattingOptions formattingOptions) {
      Contract.Requires(namespaceDefinition != null);
      Contract.Ensures(Contract.Result<string>() != null);

      return (new TypeNameFormatter()).GetNamespaceName(namespaceDefinition, formattingOptions);
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given namespace definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    public static string GetNamespaceName(IUnitNamespaceReference namespaceReference, NameFormattingOptions formattingOptions) {
      Contract.Requires(namespaceReference != null);
      Contract.Ensures(Contract.Result<string>() != null);

      return (new TypeNameFormatter()).GetNamespaceName(namespaceReference, formattingOptions);
    }

    /// <summary>
    /// Returns the nested type, if any, of the given declaring type with the given name and given generic parameter count.
    /// If no such type is found, Dummy.NestedTypeDefinition is returned.
    /// </summary>
    /// <param name="declaringType">The type to search for a nested type with the given name and number of generic parameters.</param>
    /// <param name="typeName">The name of the nested type to return.</param>
    /// <param name="genericParameterCount">The number of generic parameters. Zero if the type is not generic, larger than zero otherwise.</param>
    /// <returns></returns>
    public static INestedTypeDefinition GetNestedType(ITypeDefinition declaringType, IName typeName, int genericParameterCount) {
      Contract.Requires(declaringType != null);
      Contract.Requires(typeName != null);
      Contract.Ensures(Contract.Result<INestedTypeDefinition>() != null);

      foreach (var member in declaringType.GetMembersNamed(typeName, false)) {
        var nestedType = member as INestedTypeDefinition;
        if (nestedType == null) continue;
        if (nestedType.GenericParameterCount != genericParameterCount) continue;
        return nestedType;
      }
      return Dummy.NestedTypeDefinition;
    }

    /// <summary>
    /// Returns a property of the given declaring type that has the given name.
    /// If no such property can be found, Dummy.PropertyDefinition is returned.
    /// </summary>
    /// <param name="declaringType">The type thats declares the property.</param>
    /// <param name="propertyName">The name of the property.</param>
    public static IPropertyDefinition GetProperty(ITypeDefinition declaringType, IName propertyName) {
      Contract.Requires(declaringType != null);
      Contract.Requires(propertyName != null);

      foreach (ITypeDefinitionMember member in declaringType.GetMembersNamed(propertyName, false)) {
        var/*?*/ propertyDef = member as IPropertyDefinition;
        if (propertyDef != null) return propertyDef;
      }
      return Dummy.PropertyDefinition;
    }

    /// <summary>
    /// Try to compute the self instance of a type, that is, a fully instantiated and specialized type reference. 
    /// For example, use T and T1 to instantiate A&lt;T&gt;.B.C&lt;T1&gt;. If successful, result is set to a 
    /// IGenericTypeInstance if type definition is generic, or a specialized nested type reference if one of
    /// the parent of typeDefinition is generic, or typeDefinition if none of the above. Failure happens when 
    /// one of its parent's members is not properly initialized. 
    /// </summary>
    /// <param name="typeDefinition">A type definition whose self instance is to be computed.</param>
    /// <param name="result">The self instantiated reference to typeDefinition. Valid only when returning true. </param>
    /// <returns>True if the instantiation succeeded. False if typeDefinition is a nested type and we cannot find such a nested type definition 
    /// in its parent's self instance.</returns>
    public static bool TryGetFullyInstantiatedSpecializedTypeReference(ITypeDefinition typeDefinition, out ITypeReference result) {
      Contract.Requires(typeDefinition != null);
      Contract.Ensures(Contract.ValueAtReturn<ITypeReference>(out result) != null);

      result = typeDefinition;
      if (typeDefinition.IsGeneric) {
        result = typeDefinition.InstanceType;
        return true;
      }
      INestedTypeDefinition nestedType = typeDefinition as INestedTypeDefinition;
      if (nestedType != null) {
        ITypeReference containingTypeReference;
        if (TryGetFullyInstantiatedSpecializedTypeReference(nestedType.ContainingTypeDefinition, out containingTypeReference)) {
          foreach (var t in containingTypeReference.ResolvedType.NestedTypes) {
            if (t.Name == nestedType.Name && t.GenericParameterCount == nestedType.GenericParameterCount) {
              result = t;
              return true;
            }
          }
          return false;
        } else return false;
      }
      return true;
    }

    /// <summary>
    /// Returns the value of System.TypeCode that corresponds to the given type.
    /// </summary>
    public static TypeCode GetSytemTypeCodeFor(ITypeDefinition type) {
      Contract.Requires(type != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Boolean: return TypeCode.Boolean;
        case PrimitiveTypeCode.Char: return TypeCode.Char;
        case PrimitiveTypeCode.Float32: return TypeCode.Single;
        case PrimitiveTypeCode.Float64: return TypeCode.Double;
        case PrimitiveTypeCode.Int16: return TypeCode.Int16;
        case PrimitiveTypeCode.Int32: return TypeCode.Int32;
        case PrimitiveTypeCode.Int64: return TypeCode.Int64;
        case PrimitiveTypeCode.Int8: return TypeCode.SByte;
        case PrimitiveTypeCode.IntPtr: return TypeCode.Object;
        case PrimitiveTypeCode.Pointer: return TypeCode.Object;
        case PrimitiveTypeCode.Reference: return TypeCode.Object;
        case PrimitiveTypeCode.String: return TypeCode.String;
        case PrimitiveTypeCode.UInt16: return TypeCode.UInt16;
        case PrimitiveTypeCode.UInt32: return TypeCode.UInt32;
        case PrimitiveTypeCode.UInt64: return TypeCode.UInt64;
        case PrimitiveTypeCode.UInt8: return TypeCode.Byte;
        case PrimitiveTypeCode.UIntPtr: return TypeCode.Object;
        case PrimitiveTypeCode.Void: return TypeCode.Object;
        default:
          if (TypeHelper.TypesAreEquivalent(type, type.PlatformType.SystemDateTime))
            return TypeCode.DateTime;
          if (TypeHelper.TypesAreEquivalent(type, type.PlatformType.SystemDBNull))
            return TypeCode.DBNull;
          if (TypeHelper.TypesAreEquivalent(type, type.PlatformType.SystemDecimal))
            return TypeCode.Decimal;
          return TypeCode.Object;
      }
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to a source expression that would bind to the given type definition when appearing in an appropriate context.
    /// </summary>
    [Pure]
    public static string GetTypeName(ITypeReference type) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<string>() != null);

      return TypeHelper.GetTypeName(type, NameFormattingOptions.None);
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given type definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    public static string GetTypeName(ITypeReference type, NameFormattingOptions formattingOptions) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<string>() != null);

      return (new TypeNameFormatter()).GetTypeName(type, formattingOptions);
    }

    /// <summary>
    /// Returns the most derived base class that both given types have in common. Returns null if no such class exists.
    /// For example: if either or both are interface types, then the result is null.
    /// A class is considered its own base class for this algorithm, so if type1 derives from type2 the result is type2
    /// and if type2 derives from type1 the result is type1.
    /// </summary>
    [Pure]
    public static ITypeDefinition/*?*/ MostDerivedCommonBaseClass(ITypeDefinition type1, ITypeDefinition type2)
      //^ ensures result == null || result.IsClass;
    {
      Contract.Requires(type1 != null);
      Contract.Requires(type2 != null);
      Contract.Ensures(Contract.Result<ITypeDefinition>() == null || Contract.Result<ITypeDefinition>().IsClass);

      if (type1.IsInterface || type2.IsInterface) return null;
      if (type1.IsClass && TypeHelper.TypesAreEquivalent(type1, type2, true)) return type1;

      ITypeDefinition/*?*/ typeIter = TypeHelper.BaseClass(type1);
      if (typeIter == null) return null; //type1 has no base classes
      if (TypeHelper.TypesAreEquivalent(typeIter, type2, true)) return typeIter; //type1 is derived from type2.
      int depth1 = 0;
      while (typeIter != null) {
        typeIter = TypeHelper.BaseClass(typeIter);
        depth1++;
      }
      Contract.Assert(depth1 > 0);

      typeIter = TypeHelper.BaseClass(type2);
      if (typeIter == null) return null; //type2 has no base classes
      if (TypeHelper.TypesAreEquivalent(typeIter, type1, true)) return typeIter; //type2 is derived from type1.
      int depth2 = 0;
      while (typeIter != null) {
        typeIter = TypeHelper.BaseClass(typeIter);
        depth2++;
      }
      Contract.Assert(depth2 > 0);

      while (depth1 > depth2) {
        type1 = TypeHelper.BaseClass(type1);
        Contract.Assume(type1 != null); //Because depth2 > 0 and we were able to call TypeHelper.BaseClass depth1 times before getting null.
        depth1--;
      }
      Contract.Assert(depth1 > 0);

      while (depth2 > depth1) {
        type2 = TypeHelper.BaseClass(type2);
        Contract.Assume(type2 != null); //Because depth1 > 0 and we were able to call TypeHelper.BaseClass depth2 times before getting null.
        depth2--;
      }
      Contract.Assert(depth2 > 0);

      Contract.Assume(depth1 == depth2);

      while (depth1 > 0) {
        //If type1 and type2 at method entry were both structs, depth1 == depth2 == 1 and neither type1 nor type2 is a class during the first iteration of the loop
        if (type1.IsClass && TypeHelper.TypesAreEquivalent(type1, type2, true))
          return type1;
        type1 = TypeHelper.BaseClass(type1);
        Contract.Assume(type1 != null); //Because depth1 > 0 and we were able to call TypeHelper.BaseClass depth1 times before getting null.
        type2 = TypeHelper.BaseClass(type2);
        Contract.Assume(type2 != null); //Because depth1 > 0 and we were able to call TypeHelper.BaseClass depth1 times before getting null.
        depth1--;
      }
      return null;
    }

    /// <summary>
    /// Returns true if two parameters are equivalent.
    /// </summary>
    [Pure]
    public static bool ParametersAreEquivalent(IParameterTypeInformation param1, IParameterTypeInformation param2, bool resolveTypes = false) {
      Contract.Requires(param1 != null);
      Contract.Requires(param2 != null);

      if (param1.IsByReference != param2.IsByReference || param1.IsModified != param1.IsModified || !TypeHelper.TypesAreEquivalent(param1.Type, param2.Type, resolveTypes))
        return false;

      if (param1.IsModified) {
        Contract.Assert(param1.IsModified == param1.IsModified);
        Contract.Assume(param2.IsModified);
        IEnumerator<ICustomModifier> customModifier2enumerator = param2.CustomModifiers.GetEnumerator();
        foreach (ICustomModifier customModifier1 in param1.CustomModifiers) {
          if (!customModifier2enumerator.MoveNext())
            return false;
          ICustomModifier customModifier2 = customModifier2enumerator.Current;
          if (!TypeHelper.TypesAreEquivalent(customModifier1.Modifier, customModifier2.Modifier, resolveTypes))
            return false;
          if (customModifier1.IsOptional != customModifier2.IsOptional)
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Returns true if two parameters are equivalent, assuming that the type parameters of generic methods are equivalent if their indices match.
    /// </summary>
    [Pure]
    public static bool ParametersAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(IParameterTypeInformation param1, IParameterTypeInformation param2, bool resolveTypes = false) {
      Contract.Requires(param1 != null);
      Contract.Requires(param2 != null);

      if (param1.IsByReference != param2.IsByReference || param1.IsModified != param1.IsModified || 
        !TypeHelper.TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(param1.Type, param2.Type, resolveTypes))
        return false;

      if (param1.IsModified) {
        Contract.Assert(param1.IsModified == param1.IsModified);
        Contract.Assume(param2.IsModified);
        IEnumerator<ICustomModifier> customModifier2enumerator = param2.CustomModifiers.GetEnumerator();
        foreach (ICustomModifier customModifier1 in param1.CustomModifiers) {
          if (!customModifier2enumerator.MoveNext())
            return false;
          ICustomModifier customModifier2 = customModifier2enumerator.Current;
          if (!TypeHelper.TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(customModifier1.Modifier, customModifier2.Modifier, resolveTypes))
            return false;
          if (customModifier1.IsOptional != customModifier2.IsOptional)
            return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Returns true if two parameter lists are equivalent.
    /// </summary>
    [Pure]
    public static bool ParameterListsAreEquivalent(IEnumerable<IParameterTypeInformation> paramList1, IEnumerable<IParameterTypeInformation> paramList2, bool resolveTypes = false) {
      Contract.Requires(paramList1 != null);
      Contract.Requires(paramList2 != null);
      Contract.Requires(Contract.ForAll(paramList1, x => x != null));
      Contract.Requires(Contract.ForAll(paramList2, x => x != null));

      IEnumerator<IParameterTypeInformation> parameterEnumerator2 = paramList2.GetEnumerator();
      foreach (IParameterTypeInformation parameter1 in paramList1) {
        if (!parameterEnumerator2.MoveNext()) {
          return false;
        }
        IParameterTypeInformation parameter2 = parameterEnumerator2.Current;
        if (!TypeHelper.ParametersAreEquivalent(parameter1, parameter2, resolveTypes))
          return false;
      }
      if (parameterEnumerator2.MoveNext())
        return false;
      return true;
    }

    /// <summary>
    /// Returns true if two parameter lists are equivalent, assuming that the type parameters of generic methods are equivalent if their indices match.
    /// </summary>
    [Pure]
    public static bool ParameterListsAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(IEnumerable<IParameterTypeInformation> paramList1, IEnumerable<IParameterTypeInformation> paramList2, bool resolveTypes = false) {
      Contract.Requires(paramList1 != null);
      Contract.Requires(Contract.ForAll(paramList1, x => x != null));
      Contract.Requires(paramList2 != null);
      Contract.Requires(Contract.ForAll(paramList2, x => x != null));

      IEnumerator<IParameterTypeInformation> parameterEnumerator2 = paramList2.GetEnumerator();
      foreach (IParameterTypeInformation parameter1 in paramList1) {
        if (!parameterEnumerator2.MoveNext()) {
          return false;
        }
        IParameterTypeInformation parameter2 = parameterEnumerator2.Current;
        if (!TypeHelper.ParametersAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(parameter1, parameter2, resolveTypes))
          return false;
      }
      if (parameterEnumerator2.MoveNext())
        return false;
      return true;
    }

    /// <summary>
    /// Returns true if two parameter lists of type IParameterDefinition are equivalent, assuming that the type parameters of generic methods are equivalent if their indices match.
    /// </summary>
    [Pure]
    public static bool ParameterListsAreEquivalent(IEnumerable<IParameterDefinition> paramList1, IEnumerable<IParameterDefinition> paramList2, bool resolveTypes = false) {
      Contract.Requires(paramList1 != null);
      Contract.Requires(Contract.ForAll(paramList1, x => x != null));
      Contract.Requires(paramList2 != null);
      Contract.Requires(Contract.ForAll(paramList2, x => x != null));

      IEnumerator<IParameterDefinition> parameterEnumerator2 = paramList2.GetEnumerator();
      foreach (IParameterDefinition parameter1 in paramList1) {
        if (!parameterEnumerator2.MoveNext()) {
          return false;
        }
        IParameterTypeInformation parameter2 = parameterEnumerator2.Current;
        if (!TypeHelper.ParametersAreEquivalent(parameter1, parameter2, resolveTypes)) {
          return false;
        }
      }
      if (parameterEnumerator2.MoveNext())
        return false;
      return true;
    }

    /// <summary>
    /// Returns a type definition, nested in the given (generic) method if possible, otherwiwe nested in the given type if possible, otherwise nested in the given unitNamespace if possible,
    /// otherwise nested in the given unit if possible, otherwise nested in the given host if possible, otherwise it returns Dummy.TypeDefinition.
    /// </summary>
    public static ITypeDefinition Resolve(ITypeReference typeReference, IMetadataHost host, IUnit unit = null, IUnitNamespace unitNamespace = null, ITypeDefinition type = null, IMethodDefinition method = null) {
      Contract.Requires(typeReference != null);
      Contract.Requires(host != null);
      Contract.Requires(unit != null || unitNamespace == null);
      Contract.Requires(unitNamespace != null || type == null);
      Contract.Requires(method == null || method.IsGeneric);
      Contract.Ensures(Contract.Result<ITypeDefinition>() != null);

      var nsTypeRef = typeReference as INamespaceTypeReference;
      if (nsTypeRef != null) {
        var containingNamespaceDef = UnitHelper.Resolve(nsTypeRef.ContainingUnitNamespace, host, unit, unitNamespace);
        if (containingNamespaceDef is Dummy) return Dummy.TypeDefinition;
        foreach (var nsMember in containingNamespaceDef.GetMembersNamed(nsTypeRef.Name, ignoreCase: false)) {
          var nsTypeDef = nsMember as INamespaceTypeDefinition;
          if (nsTypeDef != null && nsTypeRef.GenericParameterCount == nsTypeDef.GenericParameterCount &&
              (!nsTypeRef.IsEnum || nsTypeDef.IsEnum) && (!nsTypeRef.IsValueType || nsTypeDef.IsValueType) &&
              nsTypeRef.TypeCode == nsTypeDef.TypeCode) return nsTypeDef;
          var nsAlias = nsMember as INamespaceAliasForType;
          if (nsAlias != null) return Resolve(nsAlias.AliasedType, host, unit, unitNamespace, type, method);
        }
        return Dummy.TypeDefinition;
      }
      var nestedTypeRef = typeReference as INestedTypeReference;
      if (nestedTypeRef != null) {
        if (type != null) {
          foreach (var tyMember in type.GetMembersNamed(nestedTypeRef.Name, ignoreCase: false)) {
            var neTypeDef = tyMember as INestedTypeDefinition;
            if (neTypeDef != null && nestedTypeRef.GenericParameterCount == neTypeDef.GenericParameterCount &&
              (!nestedTypeRef.IsEnum || neTypeDef.IsEnum) && (!nestedTypeRef.IsValueType || neTypeDef.IsValueType) &&
              nestedTypeRef.TypeCode == neTypeDef.TypeCode) 
              return neTypeDef;
            var neAlias = tyMember as INestedAliasForType;
            if (neAlias != null) return Resolve(neAlias.AliasedType, host, unit, unitNamespace, type, method);
          }
        }
        var containingTypeDef = TypeHelper.Resolve(nestedTypeRef.ContainingType, host, unit, unitNamespace, type, method);
        if (containingTypeDef is Dummy) return Dummy.TypeDefinition;
        foreach (var tyMember in containingTypeDef.GetMembersNamed(nestedTypeRef.Name, ignoreCase: false)) {
          var neTypeDef = tyMember as INestedTypeDefinition;
          if (neTypeDef != null) return neTypeDef;
          var neAlias = tyMember as INestedAliasForType;
          if (neAlias != null) return Resolve(neAlias.AliasedType, host, unit, unitNamespace, type, method);
        }
        return Dummy.TypeDefinition;
      }
      var arrayType = typeReference as IArrayTypeReference;
      if (arrayType != null) {
        var elemTypeDef = TypeHelper.Resolve(arrayType.ElementType, host, unit, unitNamespace, type, method);
        if (elemTypeDef is Dummy) return Dummy.TypeDefinition;
        if (arrayType.IsVector) return Vector.GetVector(elemTypeDef, host.InternFactory);
        return Matrix.GetMatrix(elemTypeDef, arrayType.Rank, arrayType.LowerBounds, arrayType.Sizes, host.InternFactory);
      }
      var genericTypeParameterRef = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameterRef != null) {
        var definingType = TypeHelper.Resolve(genericTypeParameterRef.DefiningType, host, unit, unitNamespace, type, method);
        if (!definingType.IsGeneric) return Dummy.TypeDefinition;
        foreach (var genParDef in definingType.GenericParameters) {
          if (genParDef.Index == genericTypeParameterRef.Index) return genParDef;
        }
        return Dummy.TypeDefinition;
      }
      var genericMethodTypeParameterRef = typeReference as IGenericMethodParameterReference;
      if (genericMethodTypeParameterRef != null) {
        if (method != null) {
          foreach (var genParDef in method.GenericParameters) {
            if (genParDef.Index == genericMethodTypeParameterRef.Index) return genParDef;
          }
        }
        var definingMethod = MemberHelper.ResolveMethod(genericMethodTypeParameterRef.DefiningMethod, host, unit, unitNamespace, type);
        if (!definingMethod.IsGeneric) return Dummy.TypeDefinition;
        foreach (var genParDef in definingMethod.GenericParameters) {
          if (genParDef.Index == genericMethodTypeParameterRef.Index) return genParDef;
        }
        return Dummy.TypeDefinition;
      }
      var genericTypeInstanceRef = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstanceRef != null) {
        var genericTypeDef = TypeHelper.Resolve(genericTypeInstanceRef.GenericType, host, unit, unitNamespace, type, method) as INamedTypeDefinition;
        if (genericTypeDef == null || !genericTypeDef.IsGeneric) return Dummy.TypeDefinition;
        ITypeReference[] genericArguments = new ITypeReference[genericTypeDef.GenericParameterCount];
        var i = 0;
        foreach (var genericArgRef in genericTypeInstanceRef.GenericArguments) {
          if (i >= genericArguments.Length) return Dummy.TypeDefinition;
          var genArgDef = TypeHelper.Resolve(genericArgRef, host, unit, unitNamespace, type, method);
          if (genArgDef is Dummy) return Dummy.TypeDefinition;
          genericArguments[i++] = genArgDef;
        }
        if (i < genericArguments.Length) return Dummy.TypeDefinition;
        return GenericTypeInstance.GetGenericTypeInstance(genericTypeDef, IteratorHelper.GetReadonly(genericArguments), host.InternFactory);
      }
      var managedPointerTypeRef = typeReference as IManagedPointerTypeReference;
      if (managedPointerTypeRef != null) {
        var targetTypeDef = TypeHelper.Resolve(managedPointerTypeRef.TargetType, host, unit, unitNamespace, type, method);
        if (targetTypeDef is Dummy) return Dummy.TypeDefinition;
        return ManagedPointerType.GetManagedPointerType(targetTypeDef, host.InternFactory);
      }
      var modifiedPointerRef = typeReference as ModifiedPointerType;
      if (modifiedPointerRef != null) {
        var targetTypeDef = TypeHelper.Resolve(modifiedPointerRef.TargetType, host, unit, unitNamespace, type, method);
        if (targetTypeDef is Dummy) return Dummy.TypeDefinition;
        var customModifiers = new CustomModifier[IteratorHelper.EnumerableCount(modifiedPointerRef.CustomModifiers)];
        var i = 0;
        foreach (var customModifier in modifiedPointerRef.CustomModifiers) {
          var modifierDef = TypeHelper.Resolve(customModifier.Modifier, host, unit, unitNamespace, type, method);
          if (modifierDef is Dummy) return Dummy.TypeDefinition;
          Contract.Assume(i < customModifiers.Length);
          customModifiers[i++] = new CustomModifier(customModifier.IsOptional, modifierDef);
        }
        return ModifiedPointerType.GetModifiedPointerType(targetTypeDef, IteratorHelper.GetReadonly(customModifiers), host.InternFactory);
      }
      var modifiedTypeRef = typeReference as IModifiedTypeReference;
      if (modifiedTypeRef != null) {
        return TypeHelper.Resolve(modifiedTypeRef.UnmodifiedType, host, unit, unitNamespace, type, method);
      }
      var pointerTypeRef = typeReference as IPointerTypeReference;
      if (pointerTypeRef != null) {
        var targetTypeDef = TypeHelper.Resolve(pointerTypeRef.TargetType, host, unit, unitNamespace, type, method);
        if (targetTypeDef is Dummy) return Dummy.TypeDefinition;
        return PointerType.GetPointerType(targetTypeDef, host.InternFactory);
      }
      return Dummy.TypeDefinition;
    }

    /// <summary>
    /// If the given type is a unsigned integer type, return the equivalent signed integer type.
    /// Otherwise return the given type.
    /// </summary>
    /// <param name="typeReference">A reference to a type.</param>
    public static ITypeReference SignedEquivalent(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      switch (typeReference.TypeCode) {
        case PrimitiveTypeCode.UInt8: return typeReference.PlatformType.SystemInt8;
        case PrimitiveTypeCode.UInt16: return typeReference.PlatformType.SystemInt16;
        case PrimitiveTypeCode.UInt32: return typeReference.PlatformType.SystemInt32;
        case PrimitiveTypeCode.UInt64: return typeReference.PlatformType.SystemInt64;
        case PrimitiveTypeCode.UIntPtr: return typeReference.PlatformType.SystemIntPtr;
        default: return typeReference;
      }
    }

    /// <summary>
    /// Returns the computed size (number of bytes) of a type. May call the SizeOf property of the type.
    /// Use SizeOfType(ITypeReference, bool) to suppress the use of the SizeOf property.
    /// </summary>
    /// <param name="type">The type whose size is wanted. If not a reference to a primitive type, this type must be resolvable.</param>
    public static uint SizeOfType(ITypeReference type) {
      Contract.Requires(type != null);

      return SizeOfType(type, mayUseSizeOfProperty: true);
    }

    /// <summary>
    /// Returns the computed size (number of bytes) of a type. 
    /// </summary>
    /// <param name="type">The type whose size is wanted. If not a reference to a primitive type, this type must be resolvable.</param>
    /// <param name="mayUseSizeOfProperty">If true the SizeOf property of the given type may be evaluated and used
    /// as the result of this routine if not 0. Remember to specify false for this parameter when using this routine in the implementation
    /// of the ITypeDefinition.SizeOf property.</param>
    public static uint SizeOfType(ITypeReference type, bool mayUseSizeOfProperty) {
      Contract.Requires(type != null);

      return SizeOfType(type, type, mayUseSizeOfProperty);
    }

    private static uint SizeOfType(ITypeReference type, ITypeReference rootType, bool mayUseSizeOfProperty) {
      Contract.Requires(type != null);
      Contract.Requires(rootType != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Boolean:
          return sizeof(Boolean);
        case PrimitiveTypeCode.Char:
          return sizeof(Char);
        case PrimitiveTypeCode.Int16:
          return sizeof(Int16);
        case PrimitiveTypeCode.Int32:
          return sizeof(Int32);
        case PrimitiveTypeCode.Int8:
          return sizeof(SByte);
        case PrimitiveTypeCode.UInt16:
          return sizeof(UInt16);
        case PrimitiveTypeCode.UInt32:
          return sizeof(UInt32);
        case PrimitiveTypeCode.UInt8:
          return sizeof(Byte);
        case PrimitiveTypeCode.Int64:
          return sizeof(Int64);
        case PrimitiveTypeCode.UInt64:
          return sizeof(UInt64);
        case PrimitiveTypeCode.IntPtr:
          return type.PlatformType.PointerSize;
        case PrimitiveTypeCode.UIntPtr:
          return type.PlatformType.PointerSize;
        case PrimitiveTypeCode.Float32:
          return sizeof(Single);
        case PrimitiveTypeCode.Float64:
          return sizeof(Double);
        case PrimitiveTypeCode.Pointer:
          return type.PlatformType.PointerSize;
        case PrimitiveTypeCode.Invalid:
          return 1;
        default:
          if (type.IsEnum && type.ResolvedType.IsEnum) {
            if (TypeHelper.TypesAreEquivalent(rootType, type.ResolvedType.UnderlyingType, true)) return 0;
            return TypeHelper.SizeOfType(type.ResolvedType.UnderlyingType);
          }
          if (type is IGenericParameter) return 1; // don't know the exact size, but it must be greater than zero
          uint result = mayUseSizeOfProperty ? type.ResolvedType.SizeOf : 0;
          if (result > 0) return result;
          IEnumerable<ITypeDefinitionMember> members = type.ResolvedType.Members;
          if (type.ResolvedType.Layout == LayoutKind.Sequential) {
            List<IFieldDefinition> fields = new List<IFieldDefinition>(IteratorHelper.GetFilterEnumerable<ITypeDefinitionMember, IFieldDefinition>(members));
            fields.Sort(delegate(IFieldDefinition f1, IFieldDefinition f2) { return f1.SequenceNumber - f2.SequenceNumber; });
            members = IteratorHelper.GetConversionEnumerable<IFieldDefinition, ITypeDefinitionMember>(fields);
            Contract.Assume(members != null);
          }
          //Sum up the bit sizes
          result = 0;
          uint bitOffset = 0;
          ushort bitFieldAlignment = 0;
          Contract.Assume(members != null);
          foreach (ITypeDefinitionMember member in members) {
            IFieldDefinition/*?*/ field = member as IFieldDefinition;
            if (field == null || field.IsStatic) continue;
            ITypeDefinition fieldType = field.Type.ResolvedType;
            ushort fieldAlignment;
            if (rootType == fieldType || fieldType.IsReferenceType)
              fieldAlignment = type.PlatformType.PointerSize;
            else
              fieldAlignment = (ushort)(TypeHelper.TypeAlignment(fieldType)*8);
            uint fieldSize;
            if (field.IsBitField) {
              bitFieldAlignment = fieldAlignment;
              fieldSize = field.BitLength;
              if (bitOffset > 0 && bitOffset+fieldSize > fieldAlignment)
                bitOffset = 0;
              if (bitOffset == 0 || fieldSize == 0) {
                result = ((result+fieldAlignment-1)/fieldAlignment) * fieldAlignment;
                bitOffset = 0;
              }
              bitOffset += fieldSize;
            } else {
              if (bitFieldAlignment > fieldAlignment) fieldAlignment = bitFieldAlignment;
              bitFieldAlignment = 0; bitOffset = 0;
              result = ((result+fieldAlignment-1)/fieldAlignment) * fieldAlignment;
              if (rootType == fieldType || fieldType.IsReferenceType)
                fieldSize = type.PlatformType.PointerSize*8u;
              else
                fieldSize = TypeHelper.SizeOfType(fieldType, rootType, mayUseSizeOfProperty: true)*8;
            }
            result += fieldSize;
          }
          //Convert bit size to bytes and pad to be a multiple of the type alignment.
          result = (result+7)/8;
          uint typeAlignment = TypeHelper.TypeAlignment(type);
          return ((result+typeAlignment-1)/typeAlignment) * typeAlignment;
      }
    }

    /// <summary>
    /// Returns the stack state type used by the CLR verification algorithm when merging control flow
    /// paths. For example, both signed and unsigned 16-bit integers are treated as the same as signed 32-bit
    /// integers for the purposes of verifying that stack state merges are safe.
    /// </summary>
    [Pure]
    public static ITypeReference StackType(ITypeReference type) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Boolean:
        case PrimitiveTypeCode.Char:
        case PrimitiveTypeCode.Int16:
        case PrimitiveTypeCode.Int32:
        case PrimitiveTypeCode.Int8:
        case PrimitiveTypeCode.UInt16:
        case PrimitiveTypeCode.UInt32:
        case PrimitiveTypeCode.UInt8:
          return type.PlatformType.SystemInt32;
        case PrimitiveTypeCode.Int64:
        case PrimitiveTypeCode.UInt64:
          return type.PlatformType.SystemInt64;
        case PrimitiveTypeCode.Float32:
        case PrimitiveTypeCode.Float64:
          return type.PlatformType.SystemFloat64;
        case PrimitiveTypeCode.IntPtr:
        case PrimitiveTypeCode.UIntPtr:
          return type.PlatformType.SystemIntPtr;
        case PrimitiveTypeCode.NotPrimitive:
          if (type.IsEnum)
            return StackType(type.ResolvedType.UnderlyingType);
          break;
      }
      return type;
    }

    /// <summary>
    /// Returns true if the stack state types of the given two types are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    public static bool StackTypesAreEquivalent(ITypeReference type1, ITypeReference type2) {
      return TypeHelper.StackType(type1).InternedKey == TypeHelper.StackType(type2).InternedKey;
    }

    /// <summary>
    /// Returns the byte alignment that values of the given type ought to have. The result is a power of two and greater than zero.
    /// May call the Alignment property of the type.
    /// Use TypeAlignment(ITypeDefinition, bool) to suppress the use of the Alignment property.    
    /// </summary>
    /// <param name="type">The type whose size is wanted. If not a reference to a primitive type, this type must be resolvable.</param>
    public static ushort TypeAlignment(ITypeReference type) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ushort>() > 0);

      return TypeAlignment(type, true);
    }

    /// <summary>
    /// Returns the byte alignment that values of the given type ought to have. The result is a power of two and greater than zero.
    /// </summary>
    /// <param name="type">The type whose size is wanted. If not a reference to a primitive type, this type must be resolvable.</param>
    /// <param name="mayUseAlignmentProperty">If true the Alignment property of the given type may be inspected and used
    /// as the result of this routine if not 0. Rembmer to specify false for this parameter when using this routine in the implementation
    /// of the ITypeDefinition.Alignment property.</param>
    public static ushort TypeAlignment(ITypeReference type, bool mayUseAlignmentProperty) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ushort>() > 0);

      return TypeAlignment(type, type, mayUseAlignmentProperty);
    }

    private static ushort TypeAlignment(ITypeReference type, ITypeReference rootType, bool mayUseAlignmentProperty) {
      Contract.Requires(type != null);
      Contract.Requires(rootType != null);
      Contract.Ensures(Contract.Result<ushort>() > 0);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Boolean:
          return sizeof(Boolean);
        case PrimitiveTypeCode.Char:
          return sizeof(Char);
        case PrimitiveTypeCode.Int16:
          return sizeof(Int16);
        case PrimitiveTypeCode.Int32:
          return sizeof(Int32);
        case PrimitiveTypeCode.Int8:
          return sizeof(SByte);
        case PrimitiveTypeCode.UInt16:
          return sizeof(UInt16);
        case PrimitiveTypeCode.UInt32:
          return sizeof(UInt32);
        case PrimitiveTypeCode.UInt8:
          return sizeof(Byte);
        case PrimitiveTypeCode.Int64:
          return sizeof(Int64);
        case PrimitiveTypeCode.UInt64:
          return sizeof(UInt64);
        case PrimitiveTypeCode.IntPtr:
          return type.PlatformType.PointerSize;
        case PrimitiveTypeCode.UIntPtr:
          return type.PlatformType.PointerSize;
        case PrimitiveTypeCode.Float32:
          return sizeof(Single);
        case PrimitiveTypeCode.Float64:
          return sizeof(Double);
        case PrimitiveTypeCode.Pointer:
          return type.PlatformType.PointerSize;
        case PrimitiveTypeCode.Invalid:
          return 1;
        default:
          if (type.IsEnum && type.ResolvedType.IsEnum) {
            if (TypeHelper.TypesAreEquivalent(rootType, type.ResolvedType.UnderlyingType, true)) return 1;
            return TypeHelper.TypeAlignment(type.ResolvedType.UnderlyingType, rootType, mayUseAlignmentProperty);
          }
          ushort alignment = mayUseAlignmentProperty ? type.ResolvedType.Alignment : (ushort)0;
          if (alignment > 0) return alignment;
          foreach (ITypeDefinitionMember member in type.ResolvedType.Members) {
            IFieldDefinition/*?*/ field = member as IFieldDefinition;
            if (field == null || field.IsStatic) continue;
            ITypeDefinition fieldType = field.Type.ResolvedType;
            ushort fieldAlignment;
            if (fieldType == rootType || fieldType.IsReferenceType)
              fieldAlignment = type.PlatformType.PointerSize;
            else
              fieldAlignment = TypeHelper.TypeAlignment(fieldType);
            if (fieldAlignment > alignment) alignment = fieldAlignment;
          }
          if (alignment <= 0) alignment = 1;
          return alignment;
      }
    }

    /// <summary>
    /// Returns true if the given two array types are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    public static bool ArrayTypesAreEquivalent(IArrayTypeReference/*?*/ arrayTypeRef1, IArrayTypeReference/*?*/ arrayTypeRef2, bool resolveTypes) {
      if (arrayTypeRef1 == null || arrayTypeRef2 == null)
        return false;
      if (arrayTypeRef1 == arrayTypeRef2)
        return true;
      if (arrayTypeRef1.IsVector != arrayTypeRef2.IsVector || arrayTypeRef1.Rank != arrayTypeRef2.Rank)
        return false;
      if (!TypeHelper.TypesAreEquivalent(arrayTypeRef1.ElementType, arrayTypeRef2.ElementType, resolveTypes))
        return false;
      if (
        !IteratorHelper.EnumerablesAreEqual<ulong>(arrayTypeRef1.Sizes, arrayTypeRef2.Sizes)
        || !IteratorHelper.EnumerablesAreEqual<int>(arrayTypeRef1.LowerBounds, arrayTypeRef2.LowerBounds)
      ) {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Returns true if the given two generic instance types are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    public static bool GenericTypeInstancesAreEquivalent(IGenericTypeInstanceReference/*?*/ genericTypeInstRef1, IGenericTypeInstanceReference/*?*/ genericTypeInstRef2, bool resolveTypes = false) {
      if (genericTypeInstRef1 == null || genericTypeInstRef2 == null)
        return false;
      if (genericTypeInstRef1 == genericTypeInstRef2)
        return true;
      if (!TypeHelper.TypesAreEquivalent(genericTypeInstRef1.GenericType, genericTypeInstRef2.GenericType, resolveTypes))
        return false;
      IEnumerator<ITypeReference> genericArguments2enumerator = genericTypeInstRef2.GenericArguments.GetEnumerator();
      foreach (ITypeReference genericArgument1 in genericTypeInstRef1.GenericArguments) {
        if (!genericArguments2enumerator.MoveNext())
          return false;
        ITypeReference genericArgument2 = genericArguments2enumerator.Current;
        if (!TypeHelper.TypesAreEquivalent(genericArgument1, genericArgument2, resolveTypes))
          return false;
      }
      return true;
    }

    /// <summary>
    /// If the type is generic, the result is its instance type. If the type is nested in generic type, this result is the
    /// corresponding specialized nested type of its containing nested generic type. And so on for type that are transitively
    /// nested in generic types.
    /// </summary>
    public static ITypeDefinition GetInstanceOrSpecializedNestedType(ITypeDefinition type) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ITypeDefinition>() != null);
      if (type.IsGeneric) return type.InstanceType.ResolvedType;
      var nestedType = type as INestedTypeDefinition;
      if (nestedType != null) {
        var containerInstance = GetInstanceOrSpecializedNestedType(nestedType.ContainingTypeDefinition);
        if (containerInstance == nestedType.ContainingTypeDefinition) return nestedType;
        return TypeHelper.GetNestedType(containerInstance, nestedType.Name, nestedType.GenericParameterCount);
      }
      return type;
    }

    /// <summary>
    /// True if the type is generic or it has an outer type that is generic.
    /// </summary>
    public static bool HasOwnOrInheritedTypeParameters(ITypeDefinition type) {
      Contract.Requires(type != null);
      if (type.IsGeneric) return true;
      var nestedType = type as INestedTypeDefinition;
      while (nestedType != null) {
        type = nestedType.ContainingTypeDefinition;
        if (type.IsGeneric) return true;
        nestedType = type as INestedTypeDefinition;
      }
      return false;
    }

    /// <summary>
    /// Returns true if the given type extends System.Attribute.
    /// </summary>
    public static bool IsAttributeType(ITypeDefinition type) {
      Contract.Requires(type != null);

      foreach (ITypeReference baseClass in type.BaseClasses) {
        if (TypeHelper.TypesAreEquivalent(baseClass, type.PlatformType.SystemAttribute, true)) return true;
      }
      return false;
    }

    /// <summary>
    /// Returns true if the given two pointer types are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    public static bool PointerTypesAreEquivalent(IPointerTypeReference/*?*/ pointerTypeRef1, IPointerTypeReference/*?*/ pointerTypeRef2, bool resolveTypes = false) {
      if (pointerTypeRef1 == null || pointerTypeRef2 == null)
        return false;
      if (pointerTypeRef1 == pointerTypeRef2)
        return true;
      return TypeHelper.TypesAreEquivalent(pointerTypeRef1.TargetType, pointerTypeRef2.TargetType, resolveTypes);
    }

    /// <summary>
    /// Returns true if the given two generic type parameters are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    public static bool GenericTypeParametersAreEquivalent(IGenericTypeParameterReference/*?*/ genericTypeParam1, IGenericTypeParameterReference/*?*/ genericTypeParam2, bool resolveTypes = false) {
      if (genericTypeParam1 == null || genericTypeParam2 == null)
        return false;
      if (genericTypeParam1 == genericTypeParam2)
        return true;
      if (!TypeHelper.TypesAreEquivalent(genericTypeParam1.DefiningType, genericTypeParam2.DefiningType, resolveTypes))
        return false;
      return genericTypeParam1.Index == genericTypeParam2.Index;
    }

    /// <summary>
    /// Returns true if the given two generic method parameter are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    public static bool GenericMethodParametersAreEquivalent(IGenericMethodParameterReference/*?*/ genericMethodParam1, IGenericMethodParameterReference/*?*/ genericMethodParam2) {
      if (genericMethodParam1 == null || genericMethodParam2 == null)
        return false;
      if (genericMethodParam1 == genericMethodParam2)
        return true;
      return genericMethodParam1.Index == genericMethodParam2.Index;
    }

    /// <summary>
    /// Returns true if the given two function pointer types are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    public static bool FunctionPointerTypesAreEquivalent(IFunctionPointerTypeReference/*?*/ functionPointer1, IFunctionPointerTypeReference/*?*/ functionPointer2, bool resolveTypes = false) {
      if (functionPointer1 == null || functionPointer2 == null)
        return false;
      if (functionPointer1 == functionPointer2)
        return true;
      if (functionPointer1.CallingConvention != functionPointer2.CallingConvention)
        return false;
      if (functionPointer1.ReturnValueIsByRef != functionPointer2.ReturnValueIsByRef)
        return false;
      if (!TypeHelper.TypesAreEquivalent(functionPointer1.Type, functionPointer2.Type, resolveTypes))
        return false;
      if (!TypeHelper.ParameterListsAreEquivalent(functionPointer1.Parameters, functionPointer2.Parameters, resolveTypes))
        return false;
      return TypeHelper.ParameterListsAreEquivalent(functionPointer1.ExtraArgumentTypes, functionPointer2.ExtraArgumentTypes, resolveTypes);
    }

    /// <summary>
    /// Returns true if the given two function pointer types are to be considered equivalent for the purpose of signature matching and so on,
    /// assuming that the type parameters of generic methods are equivalent if their indices match.
    /// </summary>
    [Pure]
    public static bool FunctionPointerTypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(
      IFunctionPointerTypeReference/*?*/ functionPointer1, IFunctionPointerTypeReference/*?*/ functionPointer2, bool resolveTypes = false) {
      if (functionPointer1 == null || functionPointer2 == null)
        return false;
      if (functionPointer1 == functionPointer2)
        return true;
      if (functionPointer1.CallingConvention != functionPointer2.CallingConvention)
        return false;
      if (functionPointer1.ReturnValueIsByRef != functionPointer2.ReturnValueIsByRef)
        return false;
      if (!TypeHelper.TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(functionPointer1.Type, functionPointer2.Type, resolveTypes))
        return false;
      if (!TypeHelper.ParameterListsAreEquivalent(functionPointer1.Parameters, functionPointer2.Parameters, resolveTypes))
        return false;
      return TypeHelper.ParameterListsAreEquivalent(functionPointer1.ExtraArgumentTypes, functionPointer2.ExtraArgumentTypes, resolveTypes);
    }

    /// <summary>
    /// Returns true if the given two function pointer types are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    [Obsolete("Please use TypeHelper.TypesAreEquivalent instead")]
    public static bool NamespaceTypesAreEquivalent(INamespaceTypeReference/*?*/ nsType1, INamespaceTypeReference/*?*/ nsType2) {
      return TypeHelper.TypesAreEquivalent(nsType1, nsType2);
    }

    /// <summary>
    /// Returns true if the given two function pointer types are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    [Obsolete("Please use TypeHelper.TypesAreEquivalent instead")]
    public static bool NestedTypesAreEquivalent(INestedTypeReference/*?*/ nstType1, INestedTypeReference/*?*/ nstType2) {
      return TypeHelper.TypesAreEquivalent(nstType1, nstType2);
    }

    /// <summary>
    /// Specialize a given type reference to a given context
    /// </summary>
    public static ITypeReference SpecializeTypeReference(ITypeReference typeReference, ITypeReference context, IInternFactory internFactory) {
      Contract.Requires(typeReference != null);
      Contract.Requires(context != null);
      Contract.Requires(internFactory != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      var arrayType = typeReference as IArrayTypeReference;
      if (arrayType != null) {
        if (arrayType.IsVector) return Vector.SpecializeTypeReference(arrayType, context, internFactory);
        return Matrix.SpecializeTypeReference(arrayType, context, internFactory);
      }
      var genericTypeParameter = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameter != null) return GenericParameter.SpecializeTypeReference(genericTypeParameter, context);
      var genericTypeInstance = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) return GenericTypeInstance.SpecializeTypeReference(genericTypeInstance, context, internFactory);
      var managedPointerType = typeReference as IManagedPointerTypeReference;
      if (managedPointerType != null) return ManagedPointerType.SpecializeTypeReference(managedPointerType, context, internFactory);
      var modifiedPointer = typeReference as ModifiedPointerType;
      if (modifiedPointer != null) return ModifiedPointerType.SpecializeTypeReference(modifiedPointer, context, internFactory);
      var modifiedType = typeReference as IModifiedTypeReference;
      if (modifiedType != null) return ModifiedTypeReference.SpecializeTypeReference(modifiedType, context, internFactory);
      var nestedType = typeReference as INestedTypeReference;
      if (nestedType != null) return SpecializedNestedTypeDefinition.SpecializeTypeReference(nestedType, context, internFactory);
      var pointerType = typeReference as IPointerTypeReference;
      if (pointerType != null) return PointerType.SpecializeTypeReference(pointerType, context, internFactory);
      return typeReference;
    }

    /// <summary>
    /// Specialize a given type reference to a given context
    /// </summary>
    public static ITypeReference SpecializeTypeReference(ITypeReference typeReference, IMethodReference context, IInternFactory internFactory) {
      Contract.Requires(typeReference != null);
      Contract.Requires(context != null);
      Contract.Requires(internFactory != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      var arrayType = typeReference as IArrayTypeReference;
      if (arrayType != null) {
        if (arrayType.IsVector) return Vector.SpecializeTypeReference(arrayType, context, internFactory);
        return Matrix.SpecializeTypeReference(arrayType, context, internFactory);
      }
      var genericTypeParameter = typeReference as IGenericTypeParameterReference;
      if (genericTypeParameter != null) return GenericParameter.SpecializeTypeReference(genericTypeParameter, context);
      var genericMethodTypeParameter = typeReference as IGenericMethodParameterReference;
      if (genericMethodTypeParameter != null) return GenericParameter.SpecializeTypeReference(genericMethodTypeParameter, context, internFactory);
      var genericTypeInstance = typeReference as IGenericTypeInstanceReference;
      if (genericTypeInstance != null) return GenericTypeInstance.SpecializeTypeReference(genericTypeInstance, context, internFactory);
      var managedPointerType = typeReference as IManagedPointerTypeReference;
      if (managedPointerType != null) return ManagedPointerType.SpecializeTypeReference(managedPointerType, context, internFactory);
      var modifiedPointer = typeReference as ModifiedPointerType;
      if (modifiedPointer != null) return ModifiedPointerType.SpecializeTypeReference(modifiedPointer, context, internFactory);
      var modifiedType = typeReference as IModifiedTypeReference;
      if (modifiedType != null) return ModifiedTypeReference.SpecializeTypeReference(modifiedType, context, internFactory);
      var nestedType = typeReference as INestedTypeReference;
      if (nestedType != null) return SpecializedNestedTypeDefinition.SpecializeTypeReference(nestedType, context, internFactory);
      var pointerType = typeReference as IPointerTypeReference;
      if (pointerType != null) return PointerType.SpecializeTypeReference(pointerType, context, internFactory);
      return typeReference;
    }

    /// <summary>
    /// Returns true if the given two types are to be considered equivalent for the purpose of signature matching and so on.
    /// </summary>
    [Pure]
    public static bool TypesAreEquivalent(ITypeReference/*?*/ type1, ITypeReference/*?*/ type2, bool resolveTypes = false) {
      if (type1 == null || type2 == null) return false;
      if (type1 == type2) return true;
      if (type1.InternedKey == type2.InternedKey) return true;
      if (!resolveTypes) return false;
      return type1.ResolvedType.InternedKey == type2.ResolvedType.InternedKey;
    }

    /// <summary>
    /// Returns true if the given two types are to be considered equivalent for the purpose of generic method signature matching. This differs from
    /// TypeHelper.TypesAreEquivalent in that two generic method type parameters are considered equivalent if their parameter list indices are the same.
    /// </summary>
    [Pure]
    public static bool TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(ITypeReference/*?*/ type1, ITypeReference/*?*/ type2, bool resolveTypes = false) {
      if (type1 == null || type2 == null) return false;
      if (type1 == type2) return true;
      if (type1.InternedKey == type2.InternedKey) return true;

      if (resolveTypes) {
        var td1 = type1.ResolvedType;
        var td2 = type2.ResolvedType;
        if (!(td1 is Dummy)) type1 = td1;
        if (!(td2 is Dummy)) type2 = td2;
        if (type1 == type2) return true;
        if (type1.InternedKey == type2.InternedKey) return true;
      }

      var genMethPar1 = type1 as IGenericMethodParameterReference;
      var genMethPar2 = type2 as IGenericMethodParameterReference;
      if (genMethPar1 != null || genMethPar2 != null) {
        if (genMethPar1 == null || genMethPar2 == null) return false;
        return genMethPar1.Index == genMethPar2.Index;
      }

      var inst1 = type1 as IGenericTypeInstanceReference;
      var inst2 = type2 as IGenericTypeInstanceReference;
      if (inst1 != null || inst2 != null) {
        if (inst1 == null || inst2 == null) return false;
        if (!TypeHelper.TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(inst1.GenericType, inst2.GenericType, resolveTypes)) return false;
        return IteratorHelper.EnumerablesAreEqual<ITypeReference>(inst1.GenericArguments, inst2.GenericArguments,
          resolveTypes ? RelaxedTypeEquivalenceComparer.resolvingInstance : RelaxedTypeEquivalenceComparer.instance);
      }

      var array1 = type1 as IArrayTypeReference;
      var array2 = type2 as IArrayTypeReference;
      if (array1 != null || array2 != null) {
        if (array1 == null || array2 == null) return false;
        return TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(array1.ElementType, array2.ElementType, resolveTypes);
      }

      var pointer1 = type1 as IPointerTypeReference;
      var pointer2 = type2 as IPointerTypeReference;
      if (pointer1 != null || pointer2 != null) {
        if (pointer1 == null || pointer2 == null) return false;
        return TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(pointer1.TargetType, pointer2.TargetType, resolveTypes);
      }

      var mpointer1 = type1 as IManagedPointerTypeReference;
      var mpointer2 = type2 as IManagedPointerTypeReference;
      if (mpointer1 != null || mpointer2 != null) {
        if (mpointer1 == null || mpointer2 == null) return false;
        return TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(mpointer1.TargetType, mpointer2.TargetType, resolveTypes);
      }

      var fpointer1 = type1 as IFunctionPointerTypeReference;
      var fpointer2 = type2 as IFunctionPointerTypeReference;
      if (fpointer1 != null || fpointer2 != null) {
        return TypeHelper.FunctionPointerTypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(fpointer1, fpointer2, resolveTypes);
      }

      return false;
    }

    /// <summary>
    /// Considers two types to be equivalent even if TypeHelper.TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch returns
    /// true, as opposed to the stricter rules applied by TypeHelper.TypesAreEquivalent.
    /// </summary>
    private class RelaxedTypeEquivalenceComparer : IEqualityComparer<ITypeReference> {

      private RelaxedTypeEquivalenceComparer(bool resolveTypes = false) {
        this.resolveTypes = resolveTypes;
      }

      bool resolveTypes;

      /// <summary>
      /// A singleton instance of RelaxedTypeEquivalenceComparer that is safe to use in all contexts.
      /// </summary>
      internal static RelaxedTypeEquivalenceComparer instance = new RelaxedTypeEquivalenceComparer();

      /// <summary>
      /// A singleton instance of RelaxedTypeEquivalenceComparer that is safe to use in all contexts.
      /// </summary>
      internal static RelaxedTypeEquivalenceComparer resolvingInstance = new RelaxedTypeEquivalenceComparer(true);

      /// <summary>
      /// Determines whether the specified objects are equal.
      /// </summary>
      /// <param name="x">The first object to compare.</param>
      /// <param name="y">The second object to compare.</param>
      /// <returns>
      /// true if the specified objects are equal; otherwise, false.
      /// </returns>
      public bool Equals(ITypeReference x, ITypeReference y) {
        if (x == null) return y == null;
        return TypeHelper.TypesAreEquivalentAssumingGenericMethodParametersAreEquivalentIfTheirIndicesMatch(x, y, this.resolveTypes);
      }

      /// <summary>
      /// Returns a hash code for this instance.
      /// </summary>
      /// <param name="r">The r.</param>
      /// <returns>
      /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
      /// </returns>
      public int GetHashCode(ITypeReference r) {
        return (int)r.InternedKey;
      }

    }

    /// <summary>
    /// Returns true if type1 is the same as type2 or if it is derives from type2.
    /// Type1 derives from type2 if the latter is a direct or indirect base class.
    /// </summary>
    [Pure]
    public static bool Type1DerivesFromOrIsTheSameAsType2(ITypeDefinition type1, ITypeReference type2, bool resolveTypes = false) {
      Contract.Requires(type1 != null);
      Contract.Requires(type2 != null);

      if (TypeHelper.TypesAreEquivalent(type1, type2, resolveTypes)) return true;
      return TypeHelper.Type1DerivesFromType2(type1, type2, resolveTypes);
    }

    /// <summary>
    /// Type1 derives from type2 if the latter is a direct or indirect base class.
    /// </summary>
    [Pure]
    public static bool Type1DerivesFromType2(ITypeDefinition type1, ITypeReference type2, bool resolveTypes = false) {
      Contract.Requires(type1 != null);
      Contract.Requires(type2 != null);

      foreach (ITypeReference baseClass in type1.BaseClasses) {
        if (TypeHelper.TypesAreEquivalent(baseClass, type2, resolveTypes)) return true;
        if (TypeHelper.Type1DerivesFromType2(baseClass.ResolvedType, type2, resolveTypes)) return true;
      }
      return false;
    }

    /// <summary>
    /// Returns true if the given type definition, or one of its base types, implements the given interface or an interface
    /// that derives from the given interface.
    /// </summary>
    [Pure]
    public static bool Type1ImplementsType2(ITypeDefinition type1, ITypeReference type2, bool resolveTypes = false) {
      Contract.Requires(type1 != null);
      Contract.Requires(type2 != null);

      foreach (ITypeReference implementedInterface in type1.Interfaces) {
        if (TypeHelper.TypesAreEquivalent(implementedInterface, type2, resolveTypes)) return true;
        if (TypeHelper.Type1ImplementsType2(implementedInterface.ResolvedType, type2, resolveTypes)) return true;
      }
      foreach (ITypeReference baseClass in type1.BaseClasses) {
        if (TypeHelper.Type1ImplementsType2(baseClass.ResolvedType, type2, resolveTypes)) return true;
      }
      return false;
    }

    /// <summary>
    /// Returns true if Type1 is CovariantWith Type2 as per CLR.
    /// </summary>
    [Pure]
    public static bool Type1IsCovariantWithType2(ITypeDefinition type1, ITypeReference type2, bool resolveTypes = false) {
      Contract.Requires(type1 != null);
      Contract.Requires(type2 != null);

      IArrayTypeReference/*?*/ arrType1 = type1 as IArrayTypeReference;
      IArrayTypeReference/*?*/ arrType2 = type2 as IArrayTypeReference;
      if (arrType1 == null || arrType2 == null) return false;
      if (arrType1.Rank != arrType2.Rank || arrType1.IsVector != arrType2.IsVector) return false;
      return TypeHelper.TypesAreAssignmentCompatible(arrType1.ElementType.ResolvedType, arrType2.ElementType.ResolvedType, resolveTypes);
    }

    /// <summary>
    /// Returns true if a CLR supplied implicit reference conversion is available to convert a value of the given source type to a corresponding value of the given target type.
    /// </summary>
    [Pure]
    public static bool TypesAreAssignmentCompatible(ITypeDefinition sourceType, ITypeDefinition targetType, bool resolveTypes = false) {
      Contract.Requires(sourceType != null);
      Contract.Requires(targetType != null);

      if (TypeHelper.TypesAreEquivalent(sourceType, targetType, resolveTypes)) return true;
      if (sourceType.IsReferenceType && TypeHelper.Type1DerivesFromOrIsTheSameAsType2(sourceType, targetType, resolveTypes)) return true;
      if (targetType.IsInterface && TypeHelper.Type1ImplementsType2(sourceType, targetType, resolveTypes)) return true;
      if (sourceType.IsInterface && TypeHelper.TypesAreEquivalent(targetType, targetType.PlatformType.SystemObject, resolveTypes)) return true;
      if (TypeHelper.Type1IsCovariantWithType2(sourceType, targetType, resolveTypes)) return true;
      return false;
    }

    /// <summary>
    /// If the given type is a signed integer type, return the equivalent unsigned integer type.
    /// Otherwise return the given type.
    /// </summary>
    /// <param name="typeReference">A reference to a type.</param>
    public static ITypeReference UnsignedEquivalent(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      switch (typeReference.TypeCode) {
        case PrimitiveTypeCode.Int8: return typeReference.PlatformType.SystemUInt8;
        case PrimitiveTypeCode.Int16: return typeReference.PlatformType.SystemUInt16;
        case PrimitiveTypeCode.Int32: return typeReference.PlatformType.SystemUInt32;
        case PrimitiveTypeCode.Int64: return typeReference.PlatformType.SystemUInt64;
        case PrimitiveTypeCode.IntPtr: return typeReference.PlatformType.SystemUIntPtr;
        default: return typeReference;
      }
    }

  }

  /// <summary>
  /// A collection of methods that format types as strings. The methods are virtual and reference each other. 
  /// By default, types are formatting according to C# conventions. However, by overriding one or more of the
  /// methods, the formatting can be customized for other languages.
  /// </summary>
  public class TypeNameFormatter {

    /// <summary>
    /// Returns the given type name unless genericParameterCount is greater than zero and NameFormattingOptions.TypeParameters has been specified and the
    /// type can be resolved. In the latter case, return the type name augmented with the type parameters 
    /// (or, if NameFormatting.UseGenericTypeNameSuffix has been specified, the type name is agumented with `n where n is the number of parameters).
    /// </summary>
    /// <param name="type">A reference to a named type.</param>
    /// <param name="genericParameterCount">The number of generic parameters the type has.</param>
    /// <param name="formattingOptions">A set of flags that specify how the type name is to be formatted.</param>
    /// <param name="typeName">The unmangled, unaugmented name of the type.</param>
    protected virtual string AddGenericParametersIfNeeded(ITypeReference type, ushort genericParameterCount, NameFormattingOptions formattingOptions, string typeName) {
      Contract.Requires(type != null);
      Contract.Requires(typeName != null);
      Contract.Ensures(Contract.Result<string>() != null);

      string delim = ((formattingOptions & NameFormattingOptions.OmitWhiteSpaceAfterListDelimiter) == 0) ? ", " : ",";
      if ((formattingOptions & NameFormattingOptions.TypeParameters) != 0 && (formattingOptions & NameFormattingOptions.FormattingForDocumentationId) == 0 && genericParameterCount > 0 && !(type.ResolvedType is Dummy)) {
        StringBuilder sb = new StringBuilder(typeName);
        sb.Append("<");
        if ((formattingOptions & NameFormattingOptions.EmptyTypeParameterList) == 0) {
          bool first = true;
          foreach (var parameter in type.ResolvedType.GenericParameters) {
            if (first) first = false; else sb.Append(delim);
            sb.Append(this.GetTypeName(parameter, formattingOptions));
          }
        } else {
          sb.Append(',', genericParameterCount - 1);
        }
        sb.Append(">");
        if ((formattingOptions & NameFormattingOptions.TypeConstraints) != 0) {
          foreach (var parameter in type.ResolvedType.GenericParameters) {
            if (!parameter.MustBeReferenceType && !parameter.MustBeValueType && !parameter.MustHaveDefaultConstructor && IteratorHelper.EnumerableIsEmpty(parameter.Constraints)) continue;
            sb.Append(" where ");
            sb.Append(parameter.Name.Value);
            sb.Append(" : ");
            bool first = true;
            if (parameter.MustBeReferenceType) { sb.Append("class"); first = false; }
            if (parameter.MustBeValueType) { sb.Append("struct"); first = false; }
            foreach (var constraint in parameter.Constraints) {
              if (first) first = false; else sb.Append(delim);
              sb.Append(this.GetTypeName(constraint, NameFormattingOptions.None));
            }
            if (parameter.MustHaveDefaultConstructor) {
              if (!first) { sb.Append(delim); }
              sb.Append("new ()");
            }
          }
        }
        typeName = sb.ToString();
      } else if ((formattingOptions & NameFormattingOptions.UseGenericTypeNameSuffix) != 0 && genericParameterCount > 0) {
        typeName = typeName + "`" + genericParameterCount;
      }
      return typeName;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given type reference and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    protected virtual string GetArrayTypeName(IArrayTypeReference arrayType, NameFormattingOptions formattingOptions) {
      Contract.Requires(arrayType != null);
      Contract.Ensures(Contract.Result<string>() != null);

      StringBuilder sb = new StringBuilder();
      ITypeReference elementType = arrayType.ElementType;
      IArrayTypeReference elementAsArray = elementType as IArrayTypeReference;
      while (elementAsArray != null) {
        elementType = elementAsArray.ElementType;
        elementAsArray = elementType as IArrayTypeReference;
      }
      sb.Append(this.GetTypeName(elementType, formattingOptions));
      this.AppendArrayDimensions(arrayType, sb, formattingOptions);
      return sb.ToString();
    }

    /// <summary>
    /// Appends a C#-like specific string of the dimensions of the given array type reference to the given StringBuilder.
    /// <example>For example, this appends the "[][,]" part of an array like "int[][,]".</example>
    /// </summary>
    protected virtual void AppendArrayDimensions(IArrayTypeReference arrayType, StringBuilder sb, NameFormattingOptions formattingOptions) {
      Contract.Requires(arrayType != null);
      Contract.Requires(sb != null);

      IArrayTypeReference/*?*/ elementArrayType = arrayType.ElementType as IArrayTypeReference;
      bool formattingForDocumentationId = (formattingOptions & NameFormattingOptions.FormattingForDocumentationId) != 0;
      if (formattingForDocumentationId && elementArrayType != null) { //Append the outer dimensions of the array first
        this.AppendArrayDimensions(elementArrayType, sb, formattingOptions);
      }
      sb.Append("[");
      if (!arrayType.IsVector) {
        if (formattingForDocumentationId) {
          bool first = true;
          IEnumerator<int> lowerBounds = arrayType.LowerBounds.GetEnumerator();
          IEnumerator<ulong> sizes = arrayType.Sizes.GetEnumerator();
          for (int i = 0; i < arrayType.Rank; i++) {
            if (!first) sb.Append(","); first = false;
            if (lowerBounds.MoveNext()) {
              sb.Append(lowerBounds.Current);
              sb.Append(":");
              if (sizes.MoveNext()) sb.Append(sizes.Current);
            } else {
              if (sizes.MoveNext()) sb.Append("0:" + sizes.Current);
            }
          }
        } else {
          sb.Append(',', (int)arrayType.Rank-1);
        }
      }
      sb.Append("]");
      if (!formattingForDocumentationId && elementArrayType != null) { //Append the inner dimensions of the array first
        this.AppendArrayDimensions(elementArrayType, sb, formattingOptions);
      }
    }

    /// <summary>
    /// If the name matches a C# keyword, return "@"+name
    /// </summary>
    public virtual string EscapeKeyword(string name) {
      switch (name) {
        case "abstract": return "@abstract";
        case "as": return "@as";
        case "base": return "@base";
        case "bool": return "@bool";
        case "break": return "@break";
        case "byte": return "@byte";
        case "case": return "@case";
        case "catch": return "@catch";
        case "char": return "@char";
        case "checked": return "@checked";
        case "class": return "@class";
        case "const": return "@const";
        case "continue": return "@continue";
        case "decimal": return "@decimal";
        case "default": return "@default";
        case "delegate": return "@delegate";
        case "do": return "@do";
        case "double": return "@double";
        case "explicit": return "@explicit";
        case "event": return "@event";
        case "extern": return "@extern";
        case "else": return "@else";
        case "enum": return "@enum";
        case "false": return "@false";
        case "finally": return "@finally";
        case "fixed": return "@fixed";
        case "float": return "@float";
        case "for": return "@for";
        case "foreach": return "@foreach";
        case "goto": return "@goto";
        case "if": return "@if";
        case "in": return "@in";
        case "int": return "@int";
        case "interface": return "@interface";
        case "internal": return "@internal";
        case "is": return "@is";
        case "lock": return "@lock";
        case "long": return "@long";
        case "new": return "@new";
        case "null": return "@null";
        case "namespace": return "@namespace";
        case "object": return "@object";
        case "operator": return "@operator";
        case "out": return "@out";
        case "override": return "@override";
        case "params": return "@params";
        case "private": return "@private";
        case "protected": return "@protected";
        case "public": return "@public";
        case "readonly": return "@readonly";
        case "ref": return "@ref";
        case "return": return "@return";
        case "switch": return "@switch";
        case "struct": return "@struct";
        case "sbyte": return "@sbyte";
        case "sealed": return "@sealed";
        case "short": return "@short";
        case "sizeof": return "@sizeof";
        case "stackalloc": return "@stackalloc";
        case "static": return "@static";
        case "string": return "@string";
        case "this": return "@this";
        case "throw": return "@throw";
        case "true": return "@true";
        case "try": return "@try";
        case "typeof": return "@typeof";
        case "uint": return "@uint";
        case "ulong": return "@ulong";
        case "unchecked": return "@unchecked";
        case "unsafe": return "@unsafe";
        case "ushort": return "@ushort";
        case "using": return "@using";
        case "virtual": return "@virtual";
        case "volatile": return "@volatile";
        case "void": return "@void";
        case "while": return "@while";
      }
      return name;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given type definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    protected virtual string GetGenericMethodParameterName(IGenericMethodParameterReference genericMethodParameter, NameFormattingOptions formattingOptions) {
      Contract.Requires(genericMethodParameter != null);
      Contract.Ensures(Contract.Result<string>() != null);

      if ((formattingOptions & NameFormattingOptions.FormattingForDocumentationId) != 0) return "``" + genericMethodParameter.Index;
      return genericMethodParameter.Name.Value;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given type definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    protected virtual string GetGenericTypeParameterName(IGenericTypeParameterReference genericTypeParameter, NameFormattingOptions formattingOptions) {
      Contract.Requires(genericTypeParameter != null);
      Contract.Ensures(Contract.Result<string>() != null);

      if ((formattingOptions & NameFormattingOptions.FormattingForDocumentationId) != 0) return "`" + genericTypeParameter.Index;
      return genericTypeParameter.Name.Value;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to a source expression that would bind to the given managed pointer when appearing in an appropriate context.
    /// </summary>
    [Pure]
    protected virtual string GetManagedPointerTypeName(IManagedPointerTypeReference pointerType, NameFormattingOptions formattingOptions) {
      Contract.Requires(pointerType != null);
      Contract.Ensures(Contract.Result<string>() != null);

      return this.GetTypeName(pointerType.TargetType, formattingOptions) + "&";
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to a source expression that would bind to the given modified type when appearing in an appropriate context.
    /// C# does not actually have such an expression, but the components of this made up expression corresponds to C# syntax.
    /// </summary>
    protected virtual string GetModifiedTypeName(IModifiedTypeReference modifiedType, NameFormattingOptions formattingOptions) {
      Contract.Requires(modifiedType != null);
      Contract.Ensures(Contract.Result<string>() != null);

      StringBuilder sb = new StringBuilder();
      sb.Append(this.GetTypeName(modifiedType.UnmodifiedType, formattingOptions));
      if ((formattingOptions & NameFormattingOptions.OmitCustomModifiers) == 0) {
        foreach (ICustomModifier modifier in modifiedType.CustomModifiers) {
          sb.Append(modifier.IsOptional ? " optmod " : " reqmod ");
          sb.Append(this.GetTypeName(modifier.Modifier, formattingOptions));
        }
      }
      return sb.ToString();
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given type definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    protected virtual string GetNamespaceTypeName(INamespaceTypeReference nsType, NameFormattingOptions formattingOptions) {
      Contract.Requires(nsType != null);
      Contract.Ensures(Contract.Result<string>() != null);

      var tname = nsType.Name.Value;
      if ((formattingOptions & NameFormattingOptions.EscapeKeyword) != 0) tname = this.EscapeKeyword(tname);
      if ((formattingOptions & NameFormattingOptions.SupressAttributeSuffix) != 0 &&
      AttributeHelper.IsAttributeType(nsType.ResolvedType) & tname.EndsWith("Attribute", StringComparison.Ordinal))
        tname = tname.Substring(0, tname.Length-9);
      tname = this.AddGenericParametersIfNeeded(nsType, nsType.GenericParameterCount, formattingOptions, tname);
      if ((formattingOptions & NameFormattingOptions.OmitContainingNamespace) == 0 && !(nsType.ContainingUnitNamespace is IRootUnitNamespaceReference))
        tname = this.GetNamespaceName(nsType.ContainingUnitNamespace, formattingOptions) + "." + tname;
      if ((formattingOptions & NameFormattingOptions.DocumentationIdMemberKind) != 0)
        tname = "T:" + tname;
      else if ((formattingOptions & NameFormattingOptions.MemberKind) != 0)
        tname = this.GetTypeKind(nsType) + " " + tname;
      if ((formattingOptions & NameFormattingOptions.Visibility) != 0)
        tname = (nsType.ResolvedType.IsPublic ? "public " : "internal ") + tname;
      return tname;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given unit set namespace definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    public virtual string GetNamespaceName(IUnitSetNamespace namespaceDefinition, NameFormattingOptions formattingOptions) {
      Contract.Requires(namespaceDefinition != null);
      Contract.Ensures(Contract.Result<string>() != null);

      INestedUnitSetNamespace/*?*/ nestedUnitSetNamespace = namespaceDefinition as INestedUnitSetNamespace;
      if (nestedUnitSetNamespace != null) {
        if (nestedUnitSetNamespace.ContainingNamespace.Name.Value.Length == 0 || (formattingOptions & NameFormattingOptions.OmitContainingNamespace) != 0) {
          if ((formattingOptions & NameFormattingOptions.UseGlobalPrefix) != 0)
            return "global::"+nestedUnitSetNamespace.Name.Value;
          else
            return nestedUnitSetNamespace.Name.Value;
        } else
          return this.GetNamespaceName(nestedUnitSetNamespace.ContainingUnitSetNamespace, formattingOptions) + "." + nestedUnitSetNamespace.Name.Value;
      }
      return namespaceDefinition.Name.Value;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given referenced namespace definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    public virtual string GetNamespaceName(IUnitNamespaceReference unitNamespace, NameFormattingOptions formattingOptions) {
      Contract.Requires(unitNamespace != null);
      Contract.Ensures(Contract.Result<string>() != null);

      INestedUnitNamespaceReference/*?*/ nestedUnitNamespace = unitNamespace as INestedUnitNamespaceReference;
      if (nestedUnitNamespace != null) {
        if (nestedUnitNamespace.ContainingUnitNamespace is IRootUnitNamespaceReference || (formattingOptions & NameFormattingOptions.OmitContainingNamespace) != 0) {
          if ((formattingOptions & NameFormattingOptions.UseGlobalPrefix) != 0)
            return "global::"+nestedUnitNamespace.Name.Value;
          else
            return nestedUnitNamespace.Name.Value;
        } else
          return this.GetNamespaceName(nestedUnitNamespace.ContainingUnitNamespace, formattingOptions) + "." + nestedUnitNamespace.Name.Value;
      }
      return string.Empty;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given type definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    protected virtual string GetNestedTypeName(INestedTypeReference nestedType, NameFormattingOptions formattingOptions) {
      Contract.Requires(nestedType != null);
      Contract.Ensures(Contract.Result<string>() != null);

      var tname = nestedType.Name.Value;
      if ((formattingOptions & NameFormattingOptions.EscapeKeyword) != 0) tname = this.EscapeKeyword(tname);
      if ((formattingOptions & NameFormattingOptions.SupressAttributeSuffix) != 0 &&
      AttributeHelper.IsAttributeType(nestedType.ResolvedType) & tname.EndsWith("Attribute", StringComparison.Ordinal))
        tname = tname.Substring(0, tname.Length-9);
      tname = this.AddGenericParametersIfNeeded(nestedType, nestedType.GenericParameterCount, formattingOptions, tname);
      if ((formattingOptions & NameFormattingOptions.OmitContainingType) == 0) {
        string delim = ((formattingOptions & NameFormattingOptions.UseReflectionStyleForNestedTypeNames) == 0) ? "." : "+";
        tname = this.GetTypeName(nestedType.ContainingType, formattingOptions & ~(NameFormattingOptions.MemberKind|NameFormattingOptions.Visibility|NameFormattingOptions.TypeConstraints)) + delim + tname;
      }
      if ((formattingOptions & NameFormattingOptions.MemberKind) != 0)
        tname = this.GetTypeKind(nestedType) + " " + tname;
      if ((formattingOptions & NameFormattingOptions.Visibility) != 0)
        tname = this.GetVisibility(nestedType.ResolvedType) + " " + tname;
      return tname;
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given type definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    protected virtual string GetPointerTypeName(IPointerTypeReference pointerType, NameFormattingOptions formattingOptions) {
      Contract.Requires(pointerType != null);
      Contract.Ensures(Contract.Result<string>() != null);

      return this.GetTypeName(pointerType.TargetType, formattingOptions) + "*";
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the given type definition and that conforms to the specified formatting options.
    /// </summary>
    [Pure]
    public virtual string GetTypeName(ITypeReference type, NameFormattingOptions formattingOptions) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<string>() != null);

      if (type is Dummy) return "Microsoft.Cci.DummyTypeReference";
      if ((formattingOptions & NameFormattingOptions.UseTypeKeywords) != 0) {
        switch (type.TypeCode) {
          case PrimitiveTypeCode.Boolean: return "bool";
          case PrimitiveTypeCode.Char: return "char";
          case PrimitiveTypeCode.Float32: return "float";
          case PrimitiveTypeCode.Float64: return "double";
          case PrimitiveTypeCode.Int16: return "short";
          case PrimitiveTypeCode.Int32: return "int";
          case PrimitiveTypeCode.Int64: return "long";
          case PrimitiveTypeCode.Int8: return "sbyte";
          case PrimitiveTypeCode.String: return "string";
          case PrimitiveTypeCode.UInt16: return "ushort";
          case PrimitiveTypeCode.UInt32: return "uint";
          case PrimitiveTypeCode.UInt64: return "ulong";
          case PrimitiveTypeCode.UInt8: return "byte";
          case PrimitiveTypeCode.Void: return "void";
          case PrimitiveTypeCode.NotPrimitive:
            if (TypeHelper.TypesAreEquivalent(type, type.PlatformType.SystemDecimal)) return "decimal";
            if (TypeHelper.TypesAreEquivalent(type, type.PlatformType.SystemObject)) return "object";
            break;
        }
      }
      IArrayTypeReference/*?*/ arrayType = type as IArrayTypeReference;
      if (arrayType != null) return this.GetArrayTypeName(arrayType, formattingOptions);
      IFunctionPointerTypeReference/*?*/ functionPointerType = type as IFunctionPointerTypeReference;
      if (functionPointerType != null) return this.GetFunctionPointerTypeName(functionPointerType, formattingOptions);
      IGenericTypeParameterReference/*?*/ genericTypeParam = type as IGenericTypeParameterReference;
      if (genericTypeParam != null) return this.GetGenericTypeParameterName(genericTypeParam, formattingOptions);
      IGenericMethodParameterReference/*?*/ genericMethodParam = type as IGenericMethodParameterReference;
      if (genericMethodParam != null) return this.GetGenericMethodParameterName(genericMethodParam, formattingOptions);
      IGenericTypeInstanceReference/*?*/ genericInstance = type as IGenericTypeInstanceReference;
      if (genericInstance != null) return this.GetGenericTypeInstanceName(genericInstance, formattingOptions);
      INestedTypeReference/*?*/ ntTypeDef = type as INestedTypeReference;
      if (ntTypeDef != null) return this.GetNestedTypeName(ntTypeDef, formattingOptions);
      INamespaceTypeReference/*?*/ nsTypeDef = type as INamespaceTypeReference;
      if (nsTypeDef != null) return this.GetNamespaceTypeName(nsTypeDef, formattingOptions);
      IPointerTypeReference/*?*/ pointerType = type as IPointerTypeReference;
      if (pointerType != null) return this.GetPointerTypeName(pointerType, formattingOptions);
      IManagedPointerTypeReference/*?*/ managedPointerType = type as IManagedPointerTypeReference;
      if (managedPointerType != null) return this.GetManagedPointerTypeName(managedPointerType, formattingOptions);
      IModifiedTypeReference/*?*/ modifiedType = type as IModifiedTypeReference;
      if (modifiedType != null) return this.GetModifiedTypeName(modifiedType, formattingOptions);
      if (type.ResolvedType != type && !(type.ResolvedType is Dummy)) return this.GetTypeName(type.ResolvedType, formattingOptions);
      return "unknown type: "+type.GetType().ToString();
    }

    /// <summary>
    /// Returns a C#-like string that identifies the kind of the given type definition. For example, "class" or "delegate".
    /// </summary>
    [Pure]
    protected virtual string GetTypeKind(ITypeReference typeReference) {
      Contract.Requires(typeReference != null);
      Contract.Ensures(Contract.Result<string>() != null);

      if (typeReference.IsEnum) return "enum";
      if (typeReference.IsValueType) return "struct";
      ITypeDefinition typeDefinition = typeReference.ResolvedType;
      if (typeDefinition.IsDelegate) return "delegate";
      if (typeDefinition.IsInterface) return "interface";
      if (typeDefinition.IsClass) return "class";
      return "type";
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to the visibilty of the type (i.e. "public", "protected", "private" and so on).
    /// </summary>
    public virtual string GetVisibility(INestedTypeDefinition nestedType) {
      switch (nestedType.Visibility) {
        case TypeMemberVisibility.Assembly: return "internal";
        case TypeMemberVisibility.Family: return "protected";
        case TypeMemberVisibility.FamilyAndAssembly: return "protected and internal";
        case TypeMemberVisibility.FamilyOrAssembly: return "protected internal";
        case TypeMemberVisibility.Public: return "public";
        default: return "private";
      }
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to a source expression that would bind to the given funcion pointer type instance when appearing in an appropriate context,
    /// if course, C# actually had a function pointer type.
    /// </summary>
    [Pure]
    protected virtual string GetFunctionPointerTypeName(IFunctionPointerTypeReference functionPointerType, NameFormattingOptions formattingOptions) {
      Contract.Requires(functionPointerType != null);
      Contract.Ensures(Contract.Result<string>() != null);

      StringBuilder sb = new StringBuilder();
      sb.Append("function ");
      sb.Append(this.GetTypeName(functionPointerType.Type, formattingOptions));
      bool first = true;
      sb.Append(" (");
      string delim = ((formattingOptions & NameFormattingOptions.OmitWhiteSpaceAfterListDelimiter) == 0) ? ", " : ",";
      foreach (IParameterTypeInformation par in functionPointerType.Parameters) {
        if (first) first = false; else sb.Append(delim);
        sb.Append(this.GetTypeName(par.Type, formattingOptions));
      }
      sb.Append(')');
      return sb.ToString();
    }

    /// <summary>
    /// Returns a C#-like string that corresponds to a source expression that would bind to the given generic type instance when appearing in an appropriate context.
    /// </summary>
    [Pure]
    protected virtual string GetGenericTypeInstanceName(IGenericTypeInstanceReference genericTypeInstance, NameFormattingOptions formattingOptions) {
      Contract.Requires(genericTypeInstance != null);
      Contract.Ensures(Contract.Result<string>() != null);

      ITypeReference genericType = genericTypeInstance.GenericType;
      if ((formattingOptions & NameFormattingOptions.ContractNullable) != 0) {
        if (TypeHelper.TypesAreEquivalent(genericType, genericTypeInstance.PlatformType.SystemNullable)) {
          foreach (ITypeReference tref in genericTypeInstance.GenericArguments) {
            return this.GetTypeName(tref, formattingOptions) + "?";
          }
        }
      }
      if ((formattingOptions & NameFormattingOptions.OmitTypeArguments) == 0) {
        // Don't include the type parameters if we are to include the type arguments
        // If formatting for a documentation id, don't use generic type name suffixes.
        StringBuilder sb = new StringBuilder(this.GetTypeName(genericType, formattingOptions & ~(NameFormattingOptions.TypeParameters | ((formattingOptions & NameFormattingOptions.FormattingForDocumentationId) != 0 ? NameFormattingOptions.UseGenericTypeNameSuffix : NameFormattingOptions.None))));
        if ((formattingOptions & NameFormattingOptions.FormattingForDocumentationId) != 0) sb.Append("{"); else sb.Append("<");
        bool first = true;
        string delim = ((formattingOptions & NameFormattingOptions.OmitWhiteSpaceAfterListDelimiter) == 0) ? ", " : ",";
        foreach (ITypeReference argument in genericTypeInstance.GenericArguments) {
          if (first) first = false; else sb.Append(delim);
          sb.Append(this.GetTypeName(argument, formattingOptions & ~(NameFormattingOptions.MemberKind|NameFormattingOptions.DocumentationIdMemberKind)));
        }
        if ((formattingOptions & NameFormattingOptions.FormattingForDocumentationId) != 0) sb.Append("}"); else sb.Append(">");
        return sb.ToString();
      }
      //If type arguments are not wanted, then type parameters are not going to be welcome either.
      return this.GetTypeName(genericType, formattingOptions&~NameFormattingOptions.TypeParameters);
    }

    /// <summary>
    /// If the given type reference is to a signed integer type, return the corresponding unsigned integer type. 
    /// Otherwise just return the given type reference.
    /// </summary>
    public static ITypeReference GetUnsignedEquivalent(ITypeReference type) {
      Contract.Requires(type != null);
      Contract.Ensures(Contract.Result<ITypeReference>() != null);

      switch (type.TypeCode) {
        case PrimitiveTypeCode.Int16: return type.PlatformType.SystemUInt16;
        case PrimitiveTypeCode.Int32: return type.PlatformType.SystemUInt32;
        case PrimitiveTypeCode.Int64: return type.PlatformType.SystemUInt64;
        case PrimitiveTypeCode.Int8: return type.PlatformType.SystemUInt8;
        case PrimitiveTypeCode.IntPtr: return type.PlatformType.SystemUIntPtr;
      }
      return type;
    }
  }
}
