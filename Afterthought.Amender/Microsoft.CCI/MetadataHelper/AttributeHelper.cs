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

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {
  /// <summary>
  /// Class containing helper routines for Attributes
  /// </summary>
  public static class AttributeHelper {
    /// <summary>
    /// Returns true if the type definition is an attribute. Typedefinition is said to be attribute when it inherits from [mscorlib]System.Attribute
    /// </summary>
    public static bool IsAttributeType(ITypeDefinition typeDefinition) {
      Contract.Requires(typeDefinition != null);

      return TypeHelper.Type1DerivesFromType2(typeDefinition, typeDefinition.PlatformType.SystemAttribute);
    }

    /// <summary>
    /// Returns true if the given collection of attributes contains an attribute of the given type.
    /// </summary>
    public static bool Contains(IEnumerable<ICustomAttribute> attributes, ITypeReference attributeType) {
      Contract.Requires(attributes != null);
      Contract.Requires(attributeType != null);

      foreach (ICustomAttribute attribute in attributes) {
        if (attribute == null) continue;
        if (TypeHelper.TypesAreEquivalent(attribute.Type, attributeType)) return true;
      } 
      return false;
    }

    /// <summary>
    /// Specifies whether more than one instance of this type of attribute is allowed on same element.
    /// This information is obtained from an attribute on the attribute type definition.
    /// </summary>
    public static bool AllowMultiple(ITypeDefinition attributeType, INameTable nameTable) {
      Contract.Requires(attributeType != null);
      Contract.Requires(nameTable != null);

      foreach (ICustomAttribute ca in attributeType.Attributes) {
        if (!TypeHelper.TypesAreEquivalent(ca.Type, attributeType.PlatformType.SystemAttributeUsageAttribute))
          continue;
        foreach (IMetadataNamedArgument namedArgument in ca.NamedArguments) {
          if (namedArgument.ArgumentName.UniqueKey == nameTable.AllowMultiple.UniqueKey) {
            IMetadataConstant/*?*/ compileTimeConst = namedArgument.ArgumentValue as IMetadataConstant;
            if (compileTimeConst == null || compileTimeConst.Value == null || !(compileTimeConst.Value is bool))
              continue;
            //^ assume false; //Unboxing cast might fail
            return (bool)compileTimeConst.Value;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Specifies whether this attribute applies to derived types and/or overridden methods.
    /// This information is obtained from an attribute on the attribute type definition.
    /// </summary>
    public static bool Inherited(ITypeDefinition attributeType, INameTable nameTable) {
      Contract.Requires(attributeType != null);
      Contract.Requires(nameTable != null);

      foreach (ICustomAttribute ca in attributeType.Attributes) {
        if (!TypeHelper.TypesAreEquivalent(ca.Type, attributeType.PlatformType.SystemAttributeUsageAttribute))
          continue;
        foreach (IMetadataNamedArgument namedArgument in ca.NamedArguments) {
          if (namedArgument.ArgumentName.UniqueKey == nameTable.AllowMultiple.UniqueKey) {
            IMetadataConstant/*?*/ compileTimeConst = namedArgument.ArgumentValue as IMetadataConstant;
            if (compileTimeConst == null || compileTimeConst.Value == null || !(compileTimeConst.Value is bool))
              continue;
            //^ assume false; //Unboxing cast might fail
            return (bool)compileTimeConst.Value;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Specifies the symbol table elements on which it is valid to apply this attribute.
    /// This information is obtained from an attribute on the attribute type definition.
    /// </summary>
    public static AttributeTargets ValidOn(ITypeDefinition attributeType) {
      Contract.Requires(attributeType != null);

      foreach (ICustomAttribute ca in attributeType.Attributes) {
        if (!TypeHelper.TypesAreEquivalent(ca.Type, attributeType.PlatformType.SystemAttributeUsageAttribute))
          continue;
        foreach (IMetadataExpression expr in ca.Arguments) {
          IMetadataConstant/*?*/ ctorParam = expr as IMetadataConstant;
          if (ctorParam == null || ctorParam.Value == null || !(ctorParam.Value is int))
            break;
          //^ assume false; //Unboxing cast might fail
          int val = (int)ctorParam.Value;
          return (AttributeTargets)val;
        }
      }
      return AttributeTargets.All;
    }
  }
}
