// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Contract
** 
** <OWNER>briangru,mbarnett</OWNER>
**
** Purpose: The contract class allows for expressing preconditions,
** postconditions, and object invariants about methods in source
** code for runtime checking & static analysis.
**
===========================================================*/
#define CONTRACTS_FULL // this file should always have everything.

// SPUR: define SILVERLIGHT to remove most of the stuff you don't want
#define SILVERLIGHT

#define DEBUG // The behavior of this contract library should be consistent regardless of build type.
#if !SILVERLIGHT
#define FEATURE_SERIALIZATION
#endif
#define USE_DEFAULT_TRACE_LISTENER

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
#if FEATURE_SERIALIZATION
using System.Runtime.Serialization;
#endif
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

[assembly:CLSCompliant(true)]


namespace System.Diagnostics.Contracts {

  #region Attributes

  /// <summary>
  /// Methods and classes marked with this attribute can be used within calls to Contract methods. Such methods not make any visible state changes.
  /// </summary>
  [Conditional("CONTRACTS_FULL")]
  [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Delegate | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
  public sealed class PureAttribute : Attribute {
  }

  /// <summary>
  /// Types marked with this attribute specify that a separate type contains the contracts for this type.
  /// </summary>
  [Conditional("CONTRACTS_FULL")]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
  public sealed class ContractClassAttribute : Attribute {
    private Type _typeWithContracts;

    public ContractClassAttribute(Type typeContainingContracts)
    {
      _typeWithContracts = typeContainingContracts;
    }

    public Type TypeContainingContracts {
      get { return _typeWithContracts; }
    }
  }

  /// <summary>
  /// Types marked with this attribute specify that they are a contract for the type that is the argument of the constructor.
  /// </summary>
  [Conditional("CONTRACTS_FULL")]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class ContractClassForAttribute : Attribute {
    private Type _typeIAmAContractFor;

    public ContractClassForAttribute(Type typeContractsAreFor)
    {
      _typeIAmAContractFor = typeContractsAreFor;
    }

    public Type TypeContractsAreFor {
      get { return _typeIAmAContractFor; }
    }
  }

  /// <summary>
  /// This attribute is used to mark a method as being the invariant
  /// method for a class. The method can have any name, but it must
  /// return "void" and take no parameters. The body of the method
  /// must consist solely of one or more calls to the method
  /// Contract.Invariant. A suggested name for the method is 
  /// "ObjectInvariant".
  /// </summary>
  [Conditional("CONTRACTS_FULL")]
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public sealed class ContractInvariantMethodAttribute : Attribute {
  }

  /// <summary>
  /// Attribute that specifies that an assembly is a reference assembly.
  /// </summary>
  [AttributeUsage(AttributeTargets.Assembly)]
  public sealed class ContractReferenceAssemblyAttribute : Attribute {
  }

  /// <summary>
  /// Methods (and properties) marked with this attribute can be used within calls to Contract methods, but have no runtime behavior associated with them.
  /// </summary>
  [Conditional("CONTRACTS_FULL")]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class ContractRuntimeIgnoredAttribute : Attribute {
  }

#if FEATURE_SERIALIZATION
  [Serializable]
#endif
  internal enum Mutability {
    Immutable,    // read-only after construction, except for lazy initialization & caches
    // Do we need a "deeply immutable" value?
    Mutable,
    HasInitializationPhase,  // read-only after some point.  
    // Do we need a value for mutable types with read-only wrapper subclasses?
  }
  // Note: This hasn't been thought through in any depth yet.  Consider it experimental.
  [Conditional("CONTRACTS_FULL")]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
  [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Thank you very much, but we like the names we've defined for the accessors")]
  internal sealed class MutabilityAttribute : Attribute {
    private Mutability _mutabilityMarker;

    public MutabilityAttribute(Mutability mutabilityMarker)
    {
      _mutabilityMarker = mutabilityMarker;
    }

    public Mutability Mutability {
      get { return _mutabilityMarker; }
    }
  }

  /// <summary>
  /// Instructs downstream tools whether to assume the correctness of this assembly, type or member without performing any verification or not.
  /// Can use [ContractVerification(false)] to explicitly mark assembly, type or member as one to *not* have verification performed on it.
  /// Most specific element found (member, type, then assembly) takes precedence.
  /// (That is useful if downstream tools allow a user to decide which polarity is the default, unmarked case.)
  /// </summary>
  /// <remarks>
  /// Apply this attribute to a type to apply to all members of the type, including nested types.
  /// Apply this attribute to an assembly to apply to all types and members of the assembly.
  /// Apply this attribute to a property to apply to both the getter and setter.
  /// </remarks>
  [Conditional("CONTRACTS_FULL")]
  [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
  public sealed class ContractVerificationAttribute : Attribute {
    private bool _value;

    public ContractVerificationAttribute(bool value) { _value = value; }

    public bool Value {
      get { return _value; }
    }
  }

  /// <summary>
  /// Allows a field f to be used in the method contracts for a method m when f has less visibility than m.
  /// For instance, if the method is public, but the field is private.
  /// </summary>
  [Conditional("CONTRACTS_FULL")]
  [AttributeUsage(AttributeTargets.Field)]
  [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Thank you very much, but we like the names we've defined for the accessors")]
  public sealed class ContractPublicPropertyNameAttribute : Attribute {
    private String _publicName;

    public ContractPublicPropertyNameAttribute(String name)
    {
      _publicName = name;
    }

    public String Name {
      get { return _publicName; }
    }
  }

  #endregion Attributes

  /// <summary>
  /// Contains static methods for representing program contracts such as preconditions, postconditions, and invariants.
  /// </summary>
  /// <remarks>
  /// WARNING: A binary rewriter must be used to insert runtime enforcement of these contracts.
  /// Otherwise some contracts like Ensures can only be checked statically and will not throw exceptions during runtime when contracts are violated.
  /// Please note this class uses conditional compilation to help avoid easy mistakes.  Defining the preprocessor
  /// symbol CONTRACTS_PRECONDITIONS will include all preconditions expressed using Contract.Requires in your 
  /// build.  The symbol CONTRACTS_FULL will include postconditions and object invariants, and requires the binary rewriter.
  /// </remarks>
  public static class Contract {

    #region Private Methods

    /// <summary>
    /// This method is used internally to trigger a failure indicating to the "programmer" that he is using the interface incorrectly.
    /// It is NEVER used to indicate failure of actual contracts at runtime. That is done by Internal.ContractHelper.DefaultFailure
    /// </summary>
    private static void AssertMustUseRewriter(ContractFailureKind kind, String contractKind)
    {
      // @TODO: localize this
      Internal.ContractHelper.TriggerFailure(kind, "Must use the rewriter when using Contract." + contractKind, null, null, null);
    }


    #endregion Private Methods

    #region User Methods

    #region Assume

    /// <summary>
    /// Instructs code analysis tools to assume the expression <paramref name="condition"/> is true even if it can not be statically proven to always be true.
    /// </summary>
    /// <param name="condition">Expression to assume will always be true.</param>
    /// <remarks>
    /// At runtime this is equivalent to an <seealso cref="System.Diagnostics.Contracts.Contract.Assert(bool)"/>.
    /// </remarks>
    [Pure]
    [Conditional("DEBUG")]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Assume(bool condition)
    {
      if (!condition) {
        Internal.ContractHelper.ReportFailure(ContractFailureKind.Assume, null, null, null);
      }
    }
    /// <summary>
    /// Instructs code analysis tools to assume the expression <paramref name="condition"/> is true even if it can not be statically proven to always be true.
    /// </summary>
    /// <param name="condition">Expression to assume will always be true.</param>
    /// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
    /// <remarks>
    /// At runtime this is equivalent to an <seealso cref="System.Diagnostics.Contracts.Contract.Assert(bool)"/>.
    /// </remarks>
    [Pure]
    [Conditional("DEBUG")]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Assume(bool condition, String userMessage)
    {
      if (!condition) {
        Internal.ContractHelper.ReportFailure(ContractFailureKind.Assume, userMessage, null, null);
      }
    }

    #endregion Assume

    #region Assert

    /// <summary>
    /// In debug builds, perform a runtime check that <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">Expression to check to always be true.</param>
    [Pure]
    [Conditional("DEBUG")]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Assert(bool condition)
    {
      if (!condition)
        Internal.ContractHelper.ReportFailure(ContractFailureKind.Assert, null, null, null);
    }
    /// <summary>
    /// In debug builds, perform a runtime check that <paramref name="condition"/> is true.
    /// </summary>
    /// <param name="condition">Expression to check to always be true.</param>
    /// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
    [Pure]
    [Conditional("DEBUG")]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Assert(bool condition, String userMessage)
    {
      if (!condition)
        Internal.ContractHelper.ReportFailure(ContractFailureKind.Assert, userMessage, null, null);
    }

    #endregion Assert

    #region Requires

    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// Use this form when backward compatibility does not force you to throw a particular exception.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Requires(bool condition)
    {
      AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires");
    }

    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// Use this form when backward compatibility does not force you to throw a particular exception.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Requires(bool condition, String userMessage)
    {
      AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires");
    }

    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// Use this form when you want to throw a particular exception.
    /// </remarks>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "condition")]
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif

    public static void Requires<TException>(bool condition) where TException : Exception
    {
      AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires<TException>");
    }

    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> must be true before the enclosing method or property is invoked.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// Use this form when you want to throw a particular exception.
    /// </remarks>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "userMessage")]
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "condition")]
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Requires<TException>(bool condition, String userMessage) where TException : Exception
    {
      AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires<TException>");
    }


    #endregion Requires

    #region Ensures

    /// <summary>
    /// Specifies a public contract such that the expression <paramref name="condition"/> will be true when the enclosing method or property returns normally.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue"/> and <seealso cref="Result"/>.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// The contract rewriter must be used for runtime enforcement of this postcondition.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Ensures(bool condition)
    {
      AssertMustUseRewriter(ContractFailureKind.Postcondition, "Ensures");
    }
    /// <summary>
    /// Specifies a public contract such that the expression <paramref name="condition"/> will be true when the enclosing method or property returns normally.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue"/> and <seealso cref="Result"/>.</param>
    /// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference members at least as visible as the enclosing method.
    /// The contract rewriter must be used for runtime enforcement of this postcondition.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Ensures(bool condition, String userMessage)
    {
      AssertMustUseRewriter(ContractFailureKind.Postcondition, "Ensures");
    }

    /// <summary>
    /// Specifies a contract such that if an exception of type <typeparamref name="TException"/> is thrown then the expression <paramref name="condition"/> will be true when the enclosing method or property terminates abnormally.
    /// </summary>
    /// <typeparam name="TException">Type of exception related to this postcondition.</typeparam>
    /// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue"/> and <seealso cref="Result"/>.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference types and members at least as visible as the enclosing method.
    /// The contract rewriter must be used for runtime enforcement of this postcondition.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_FULL")]
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Exception type used in tools.")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void EnsuresOnThrow<TException>(bool condition) where TException : Exception {
      AssertMustUseRewriter(ContractFailureKind.PostconditionOnException, "EnsuresOnThrow");
    }
    /// <summary>
    /// Specifies a contract such that if an exception of type <typeparamref name="TException"/> is thrown then the expression <paramref name="condition"/> will be true when the enclosing method or property terminates abnormally.
    /// </summary>
    /// <typeparam name="TException">Type of exception related to this postcondition.</typeparam>
    /// <param name="condition">Boolean expression representing the contract.  May include <seealso cref="OldValue"/> and <seealso cref="Result"/>.</param>
    /// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
    /// <remarks>
    /// This call must happen at the beginning of a method or property before any other code.
    /// This contract is exposed to clients so must only reference types and members at least as visible as the enclosing method.
    /// The contract rewriter must be used for runtime enforcement of this postcondition.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_FULL")]
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Exception type used in tools.")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void EnsuresOnThrow<TException>(bool condition, String userMessage) where TException : Exception {
      AssertMustUseRewriter(ContractFailureKind.PostconditionOnException, "EnsuresOnThrow");
    }

    #region Old, Result, and Out Parameters

    /// <summary>
    /// Represents the result (a.k.a. return value) of a method or property.
    /// </summary>
    /// <typeparam name="T">Type of return value of the enclosing method or property.</typeparam>
    /// <returns>Return value of the enclosing method or property.</returns>
    /// <remarks>
    /// This method can only be used within the argument to the <seealso cref="Ensures(bool)"/> contract.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Not intended to be called at runtime.")]
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
    public static T Result<T>() { return default(T); }

    /// <summary>
    /// Represents the final (output) value of an out parameter when returning from a method.
    /// </summary>
    /// <typeparam name="T">Type of the out parameter.</typeparam>
    /// <param name="value">The out parameter.</param>
    /// <returns>The output value of the out parameter.</returns>
    /// <remarks>
    /// This method can only be used within the argument to the <seealso cref="Ensures(bool)"/> contract.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Justification = "Not intended to be called at runtime.")]
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
    public static T ValueAtReturn<T>(out T value) { value = default(T); return value; }

    /// <summary>
    /// Represents the value of <paramref name="value"/> as it was at the start of the method or property.
    /// </summary>
    /// <typeparam name="T">Type of <paramref name="value"/>.  This can be inferred.</typeparam>
    /// <param name="value">Value to represent.  This must be a field or parameter.</param>
    /// <returns>Value of <paramref name="value"/> at the start of the method or property.</returns>
    /// <remarks>
    /// This method can only be used within the argument to the <seealso cref="Ensures(bool)"/> contract.
    /// </remarks>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
    public static T OldValue<T>(T value) { return default(T); }

    #endregion Old, Result, and Out Parameters

    #endregion Ensures

    #region Invariant

    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> will be true after every method or property on the enclosing class.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <remarks>
    /// This contact can only be specified in a dedicated invariant method declared on a class.
    /// This contract is not exposed to clients so may reference members less visible as the enclosing method.
    /// The contract rewriter must be used for runtime enforcement of this invariant.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Invariant(bool condition)
    {
      AssertMustUseRewriter(ContractFailureKind.Invariant, "Invariant");
    }
    /// <summary>
    /// Specifies a contract such that the expression <paramref name="condition"/> will be true after every method or property on the enclosing class.
    /// </summary>
    /// <param name="condition">Boolean expression representing the contract.</param>
    /// <param name="userMessage">If it is not a constant string literal, then the contract may not be understood by tools.</param>
    /// <remarks>
    /// This contact can only be specified in a dedicated invariant method declared on a class.
    /// This contract is not exposed to clients so may reference members less visible as the enclosing method.
    /// The contract rewriter must be used for runtime enforcement of this invariant.
    /// </remarks>
    [Pure]
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static void Invariant(bool condition, String userMessage)
    {
      AssertMustUseRewriter(ContractFailureKind.Invariant, "Invariant");
    }

    #endregion Invariant

    #region Quantifiers

    #region ForAll

    /// <summary>
    /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
    /// for all integers starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.
    /// </summary>
    /// <param name="fromInclusive">First integer to pass to <paramref name="predicate"/>.</param>
    /// <param name="toExclusive">One greater than the last integer to pass to <paramref name="predicate"/>.</param>
    /// <param name="predicate">Function that is evaluated from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</param>
    /// <returns><c>true</c> if <paramref name="predicate"/> returns <c>true</c> for all integers 
    /// starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</returns>
    /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.TrueForAll"/>
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]  // Assumes predicate obeys CER rules.
#endif
    public static bool ForAll(int fromInclusive, int toExclusive, Predicate<int> predicate)
    {
      if (fromInclusive > toExclusive)
#if INSIDE_CLR
        throw new ArgumentException(Environment.GetResourceString("Argument_ToExclusiveLessThanFromExclusive"));
#else
        throw new ArgumentException("fromInclusive must be less or equal to toExclusive");
#endif
      if (predicate == null)
        throw new ArgumentNullException("predicate"); 
      Contract.EndContractBlock();

      for (int i = fromInclusive; i < toExclusive; i++)
        if (!predicate(i)) return false;
      return true;
    }
    /// <summary>
    /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
    /// for all elements in the <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">The collection from which elements will be drawn from to pass to <paramref name="predicate"/>.</param>
    /// <param name="predicate">Function that is evaluated on elements from <paramref name="collection"/>.</param>
    /// <returns><c>true</c> if and only if <paramref name="predicate"/> returns <c>true</c> for all elements in
    /// <paramref name="collection"/>.</returns>
    /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.TrueForAll"/>
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]  // Assumes predicate & collection enumerator obey CER rules.
#endif
    public static bool ForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
    {
      if (collection == null)
        throw new ArgumentNullException("collection"); 
      if (predicate == null)
        throw new ArgumentNullException("predicate"); 
      Contract.EndContractBlock();

      foreach (T t in collection)
        if (!predicate(t)) return false;
      return true;
    }

    #endregion ForAll

    #region Exists

    /// <summary>
    /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
    /// for any integer starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.
    /// </summary>
    /// <param name="fromInclusive">First integer to pass to <paramref name="predicate"/>.</param>
    /// <param name="toExclusive">One greater than the last integer to pass to <paramref name="predicate"/>.</param>
    /// <param name="predicate">Function that is evaluated from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</param>
    /// <returns><c>true</c> if <paramref name="predicate"/> returns <c>true</c> for any integer
    /// starting from <paramref name="fromInclusive"/> to <paramref name="toExclusive"/> - 1.</returns>
    /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.Exists"/>
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]  // Assumes predicate obeys CER rules.
#endif
    public static bool Exists(int fromInclusive, int toExclusive, Predicate<int> predicate)
    {
      if (fromInclusive > toExclusive)
#if INSIDE_CLR
        throw new ArgumentException(Environment.GetResourceString("Argument_ToExclusiveLessThanFromExclusive"));
#else
        throw new ArgumentException("fromInclusive must be less or equal to toExclusive");
#endif
      if (predicate == null)
        throw new ArgumentNullException("predicate"); 
      Contract.EndContractBlock();

      for (int i = fromInclusive; i < toExclusive; i++)
        if (predicate(i)) return true;
      return false;
    }
    /// <summary>
    /// Returns whether the <paramref name="predicate"/> returns <c>true</c> 
    /// for any element in the <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">The collection from which elements will be drawn from to pass to <paramref name="predicate"/>.</param>
    /// <param name="predicate">Function that is evaluated on elements from <paramref name="collection"/>.</param>
    /// <returns><c>true</c> if and only if <paramref name="predicate"/> returns <c>true</c> for an element in
    /// <paramref name="collection"/>.</returns>
    /// <seealso cref="System.Collections.Generic.List&lt;T&gt;.Exists"/>
    [Pure]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]  // Assumes predicate & collection enumerator obey CER rules.
#endif
    public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");
      if (predicate == null)
        throw new ArgumentNullException("predicate");
      Contract.EndContractBlock();

      foreach (T t in collection)
        if (predicate(t)) return true;
      return false;
    }

    #endregion Exists

    #endregion Quantifiers

    #region Pointers
#if FEATURES_UNSAFE
    /// <summary>
    /// Runtime checking for pointer bounds is not currently feasible. Thus, at runtime, we just return
    /// a very long extent for each pointer that is writable. As long as assertions are of the form
    /// WritableBytes(ptr) >= ..., the runtime assertions will not fail.
    /// The runtime value is 2^64 - 1 or 2^32 - 1.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1802", Justification = "FxCop is confused")]
    static readonly ulong MaxWritableExtent = (UIntPtr.Size == 4) ? UInt32.MaxValue : UInt64.MaxValue;

    /// <summary>
    /// Allows specifying a writable extent for a UIntPtr, similar to SAL's writable extent.
    /// NOTE: this is for static checking only. No useful runtime code can be generated for this
    /// at the moment.
    /// </summary>
    /// <param name="startAddress">Start of memory region</param>
    /// <returns>The result is the number of bytes writable starting at <paramref name="startAddress"/></returns>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startAddress", Justification = "Not intended to be called at runtime.")]
    [CLSCompliant(false)]
    [Pure]
    [ContractRuntimeIgnored]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static ulong WritableBytes(UIntPtr startAddress) { return MaxWritableExtent; }

    /// <summary>
    /// Allows specifying a writable extent for a UIntPtr, similar to SAL's writable extent.
    /// NOTE: this is for static checking only. No useful runtime code can be generated for this
    /// at the moment.
    /// </summary>
    /// <param name="startAddress">Start of memory region</param>
    /// <returns>The result is the number of bytes writable starting at <paramref name="startAddress"/></returns>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startAddress", Justification = "Not intended to be called at runtime.")]
    [CLSCompliant(false)]
    [Pure]
    [ContractRuntimeIgnored]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static ulong WritableBytes(IntPtr startAddress) { return MaxWritableExtent; }

    /// <summary>
    /// Allows specifying a writable extent for a UIntPtr, similar to SAL's writable extent.
    /// NOTE: this is for static checking only. No useful runtime code can be generated for this
    /// at the moment.
    /// </summary>
    /// <param name="startAddress">Start of memory region</param>
    /// <returns>The result is the number of bytes writable starting at <paramref name="startAddress"/></returns>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startAddress", Justification = "Not intended to be called at runtime.")]
    [CLSCompliant(false)]
    [Pure]
    [ContractRuntimeIgnored]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    unsafe public static ulong WritableBytes(void* startAddress) { return MaxWritableExtent; }

    /// <summary>
    /// Allows specifying a readable extent for a UIntPtr, similar to SAL's readable extent.
    /// NOTE: this is for static checking only. No useful runtime code can be generated for this
    /// at the moment.
    /// </summary>
    /// <param name="startAddress">Start of memory region</param>
    /// <returns>The result is the number of bytes readable starting at <paramref name="startAddress"/></returns>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startAddress", Justification = "Not intended to be called at runtime.")]
    [CLSCompliant(false)]
    [Pure]
    [ContractRuntimeIgnored]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static ulong ReadableBytes(UIntPtr startAddress) { return MaxWritableExtent; }

    /// <summary>
    /// Allows specifying a readable extent for a UIntPtr, similar to SAL's readable extent.
    /// NOTE: this is for static checking only. No useful runtime code can be generated for this
    /// at the moment.
    /// </summary>
    /// <param name="startAddress">Start of memory region</param>
    /// <returns>The result is the number of bytes readable starting at <paramref name="startAddress"/></returns>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startAddress", Justification = "Not intended to be called at runtime.")]
    [CLSCompliant(false)]
    [Pure]
    [ContractRuntimeIgnored]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static ulong ReadableBytes(IntPtr startAddress) { return MaxWritableExtent; }

    /// <summary>
    /// Allows specifying a readable extent for a UIntPtr, similar to SAL's readable extent.
    /// NOTE: this is for static checking only. No useful runtime code can be generated for this
    /// at the moment.
    /// </summary>
    /// <param name="startAddress">Start of memory region</param>
    /// <returns>The result is the number of bytes readable starting at <paramref name="startAddress"/></returns>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "startAddress", Justification = "Not intended to be called at runtime.")]
    [CLSCompliant(false)]
    [Pure]
    [ContractRuntimeIgnored]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    unsafe public static ulong ReadableBytes(void* startAddress) { return MaxWritableExtent; }
#endif // FEATURES_UNSAFE
    #endregion

    #region Misc.

    /// <summary>
    /// Marker to indicate the end of the contract section of a method.
    /// </summary>
    [Conditional("CONTRACTS_FULL")]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
    public static void EndContractBlock() { }

    #endregion

    #endregion User Methods

    #region Failure Behavior

#if false
    /// <summary>
    /// Allows a managed application environment such as an interactive interpreter (IronPython) or a
    /// web browser host (Jolt hosting Silverlight in IE) to be notified of contract failures and 
    /// potentially "handle" them, either by throwing a particular exception type, etc.  If any of the
    /// event handlers sets the Cancel flag in the ContractFailedEventArgs, then the Contract class will
    /// not pop up an assert dialog box or trigger escalation policy.  Hooking this event requires 
    /// full trust.
    /// </summary>
    public static event EventHandler<ContractFailedEventArgs> ContractFailed {
#if !SILVERLIGHT
      [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
#endif
      add {
        Internal.ContractHelper.InternalContractFailed += value;
      }

#if !SILVERLIGHT
      [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
#endif
      remove {
        Internal.ContractHelper.InternalContractFailed -= value;
      }
    }
#endif
    #endregion FailureBehavior

  }


  public enum ContractFailureKind {
    Precondition,
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Postcondition")]
    Postcondition,
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Postcondition")]
    PostconditionOnException,
    Invariant,
    Assert,
    Assume,
  }

  public sealed class ContractFailedEventArgs : EventArgs {
    private ContractFailureKind _failureKind;
    private String _message;
    private String _condition;
    private Exception _originalException;
    private bool _handled;
    private bool _unwind;

#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public ContractFailedEventArgs(ContractFailureKind failureKind, String message, String condition, Exception originalException)
    {
      Contract.Requires(originalException == null || failureKind == ContractFailureKind.PostconditionOnException);

      _failureKind = failureKind;
      _message = message;
      _condition = condition;
      _originalException = originalException;
    }

    public String Message { get { return _message; } }
    public String Condition { get { return _condition; } }
    public ContractFailureKind FailureKind { get { return _failureKind; } }
    public Exception OriginalException { get { return _originalException; } }

    // Whether the event handler "handles" this contract failure, or to fail via escalation policy.
    public bool Handled {
      get { return _handled; }
    }
#if !SILVERLIGHT
    [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
#endif
    public void SetHandled()
    {
      _handled = true;
    }

    public bool Unwind {
      get { return _unwind; }
    }

#if !SILVERLIGHT
    [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
#endif
    public void SetUnwind()
    {
      _unwind = true;
    }
    
  }

#if FEATURE_SERIALIZATION
  [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
  [Serializable]
#endif
  internal sealed class ContractException : Exception {
    readonly ContractFailureKind _Kind;
    readonly string _UserMessage;
    readonly string _Condition;

#if false
    public ContractFailureKind Kind { get { return _Kind; } }
    public string Failure { get { return this.Message; } }
    public string UserMessage { get { return _UserMessage; } }
    public string Condition { get { return _Condition; } }
#endif

    public ContractException() { }

#if false
    public ContractException(string msg) : base(msg) 
    {
      _Kind = ContractFailureKind.Precondition;
    }

    public ContractException(string msg, Exception inner)
      : base(msg, inner)
    {
    }
#endif

    public ContractException(ContractFailureKind kind, string failure, string userMessage, string condition, Exception innerException)
      : base(failure, innerException)
    {
      this._Kind = kind;
      this._UserMessage = userMessage;
      this._Condition = condition;
    }

#if FEATURE_SERIALIZATION
    private ContractException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      _Kind = (ContractFailureKind)info.GetInt32("Kind");
      _UserMessage = info.GetString("UserMessage");
      _Condition = info.GetString("Condition");
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);

      info.AddValue("Kind", _Kind);
      info.AddValue("UserMessage", _UserMessage);
      info.AddValue("Condition", _Condition);
    }
#endif
  }

}


namespace System.Diagnostics.Contracts.Internal
{
  public static class ContractHelper
  {
    #region Private fields
#if false
    private static EventHandler<ContractFailedEventArgs> contractFailedEvent;
#endif
    private static readonly Object lockObject = new Object();
    private static System.Resources.ResourceManager myResourceManager = new System.Resources.ResourceManager("mscorlib", typeof(Object).Assembly);

    #endregion

#if false
    /// <summary>
    /// Allows a managed application environment such as an interactive interpreter (IronPython) or a
    /// web browser host (Jolt hosting Silverlight in IE) to be notified of contract failures and 
    /// potentially "handle" them, either by throwing a particular exception type, etc.  If any of the
    /// event handlers sets the Cancel flag in the ContractFailedEventArgs, then the Contract class will
    /// not pop up an assert dialog box or trigger escalation policy.  Hooking this event requires 
    /// full trust.
    /// </summary>
    internal static event EventHandler<ContractFailedEventArgs> InternalContractFailed
    {
#if !SILVERLIGHT
      [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
#endif
      add
      {
        // Eagerly prepare each event handler _marked with a reliability contract_, to 
        // attempt to reduce out of memory exceptions while reporting contract violations.
        // This only works if the new handler obeys the constraints placed on 
        // constrained execution regions.  Eagerly preparing non-reliable event handlers
        // would be a perf hit and wouldn't significantly improve reliability.
        // UE: Please mention reliable event handlers should also be marked with the 
        // PrePrepareMethodAttribute to avoid CER eager preparation work when ngen'ed.
#if !FEATURE_CORECLR && !SILVERLIGHT
        RuntimeHelpers.PrepareDelegate(value);
#endif
        lock (lockObject)
        {
          contractFailedEvent += value;
        }
      }
#if !SILVERLIGHT
      [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
#endif
      remove
      {
        lock (lockObject)
        {
          contractFailedEvent -= value;
        }
      }
    }
#endif

    /// <summary>
    /// Rewriter will call this method on a contract failure to allow listeners to be notified.
    /// The method should not perform any failure (assert/throw) itself.
    /// </summary>
    /// <returns>null if the event was handled and should not trigger a failure.
    ///          Otherwise, returns the localized failure message</returns>
    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Security.SecuritySafeCriticalAttribute")]
    [System.Diagnostics.DebuggerNonUserCode]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    public static string RaiseContractFailedEvent(ContractFailureKind failureKind, String userMessage, String conditionText, Exception innerException)
    {
      if (failureKind < ContractFailureKind.Precondition || failureKind > ContractFailureKind.Assume) throw new ArgumentException("failureKind is not in range", "failureKind");
      Contract.EndContractBlock();

      String displayMessage = "contract failed.";  // Incomplete, but in case of OOM during resource lookup...
      ContractFailedEventArgs eventArgs = null;  // In case of OOM.
      string returnValue;
#if !SILVERLIGHT
      RuntimeHelpers.PrepareConstrainedRegions();
#endif
      try
      {
        displayMessage = GetDisplayMessage(failureKind, userMessage, conditionText);
#if false
        if (contractFailedEvent != null)
        {
          eventArgs = new ContractFailedEventArgs(failureKind, displayMessage, conditionText, innerException);
          foreach (EventHandler<ContractFailedEventArgs> handler in contractFailedEvent.GetInvocationList())
          {
            try
            {
              handler(null, eventArgs);
            }
            catch
            { // swallow all exceptions from handlers }
            }
          }
          if (eventArgs.Unwind)
          {
#if INSIDE_CLR
              if (Environment.IsCLRHosted) TriggerEscalationPolicy();
#endif
            // unwind
            throw new ContractException(failureKind, displayMessage, userMessage, conditionText, innerException);
          }
        }
#else
        throw new ContractException(failureKind, displayMessage, userMessage, conditionText, innerException);
#endif
      }
      finally
      {
        if (eventArgs != null && eventArgs.Handled)
        {
          returnValue = null; // handled
        }
        else
        {
          returnValue = displayMessage;
        }
      }
    }

    /// <summary>
    /// Rewriter calls this method to get the default failure behavior.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "conditionText")]
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "userMessage")]
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "kind")]
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "innerException")]
    [SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Security.SecuritySafeCriticalAttribute")]
    [System.Diagnostics.DebuggerNonUserCode]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
    public static void TriggerFailure(ContractFailureKind kind, String displayMessage, String userMessage, String conditionText, Exception innerException)
    {
#if false
      if (System.Diagnostics.Debugger.IsAttached)
      {
        System.Diagnostics.Debugger.Break();
      }
#endif
#if INSIDE_CLR
      if (Environment.IsCLRHosted)
      {
        TriggerEscalationPolicy(kind, displayMessage, innerException);
      }
#endif
#if !SILVERLIGHT
      if (!Environment.UserInteractive)
      {
        Environment.FailFast(displayMessage);
      }
#endif
      // @TODO: The BCL needs to use their internal assert here.
      Console.WriteLine(displayMessage);
      Debug.Assert(false, displayMessage);
    }

#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    private static String GetDisplayMessage(ContractFailureKind failureKind, String userMessage, String conditionText)
    {
      String resourceName = null;
      switch (failureKind)
      {
        case ContractFailureKind.Assert:
          resourceName = "AssertionFailed";
          break;

        case ContractFailureKind.Assume:
          resourceName = "AssumptionFailed";
          break;

        case ContractFailureKind.Precondition:
          resourceName = "PreconditionFailed";
          break;

        case ContractFailureKind.Postcondition:
          resourceName = "PostconditionFailed";
          break;

        case ContractFailureKind.Invariant:
          resourceName = "InvariantFailed";
          break;

        case ContractFailureKind.PostconditionOnException:
          resourceName = "PostconditionOnExceptionFailed";
          break;

        default:
          Contract.Assume(false, "Unreachable code");
          resourceName = "AssumptionFailed";
          break;
      }
      var failureMessage = myResourceManager.GetString(resourceName);
      if (failureMessage == null)
      { // Hack for pre-V4 CLR’s
        failureMessage = String.Format(CultureInfo.CurrentUICulture, "{0} failed", failureKind);
      }
      // Now format based on presence of condition/userProvidedMessage
      if (conditionText != null)
      {
        if (userMessage != null)
        {
          // both != null
          return String.Format(CultureInfo.CurrentUICulture, "{0}: {1} {2}", failureMessage, conditionText, userMessage);
        }
        else
        {
          // condition != null, userProvidedMessage == null
          return String.Format(CultureInfo.CurrentUICulture, "{0}: {1}", failureMessage, conditionText);
        }
      }
      else
      {
        if (userMessage != null)
        {
          // condition null, userProvidedMessage != null
          return String.Format(CultureInfo.CurrentUICulture, "{0}: {1}", failureMessage, userMessage);
        }
        else
        {
          // both null
          return failureMessage;
        }
      }
    }

    /// <summary>
    /// Rewriter never calls this method. Only failures triggered when using Assert/Assume without rewriting will call this.
    /// </summary>
    [SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Security.SecuritySafeCriticalAttribute")]
    [System.Diagnostics.DebuggerNonUserCode]
#if !SILVERLIGHT
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
    internal static void ReportFailure(ContractFailureKind failureKind, String userMessage, String conditionText, Exception innerException)
    {
      if (failureKind < ContractFailureKind.Precondition || failureKind > ContractFailureKind.Assume) throw new ArgumentException("failureKind is not in range", "failureKind");
      Contract.EndContractBlock();

      // displayMessage == null means: yes we handled it. Otherwise it is the localized failure message
      var displayMessage = RaiseContractFailedEvent(failureKind, userMessage, conditionText, innerException);

      if (displayMessage == null) return;

      TriggerFailure(failureKind, displayMessage, userMessage, conditionText, innerException);
    }


  }
}
