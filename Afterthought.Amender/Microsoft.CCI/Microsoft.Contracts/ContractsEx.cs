#define CONTRACTS_FULL // this file should always have everything defined

namespace System.Diagnostics.Contracts
{
  /// <summary>
  /// Additional contracts not yet part of the mainstream contract library.
  /// </summary>
  public static class ContractEx
  {
    #region Unreachable

    /// <summary>
    /// Fails.
    /// </summary>
    [Pure]
    [System.Diagnostics.DebuggerNonUserCode]
    public static Exception UnreachableAlways(Exception exception, string message)
    {
      Internal.ContractHelper.TriggerFailure(ContractFailureKind.Assert, message, null, null, null);
      if (exception == null)
        exception = new InvalidOperationException(message);
      throw exception;
    }

    /// <summary>
    /// Fails.
    /// </summary>
    [Pure]
    [System.Diagnostics.DebuggerNonUserCode]
    public static Exception UnreachableAlways(Exception exception)
    {
      throw UnreachableAlways(exception, null);
    }

    /// <summary>
    /// Fails.
    /// </summary>
    [Pure]
    [System.Diagnostics.DebuggerNonUserCode]
    public static Exception UnreachableAlways(Exception ex, String format, params object[] args)
    {
      throw UnreachableAlways(ex, string.Format(format, args));
    }

    /// <summary>
    /// Fails.
    /// </summary>
    [Pure]
    [System.Diagnostics.DebuggerNonUserCode]
    public static Exception UnreachableAlways(string message)
    {
      throw UnreachableAlways(null, message);
    }

    /// <summary>
    /// Fails.
    /// </summary>
    [Pure]
    [System.Diagnostics.DebuggerNonUserCode]
    public static Exception UnreachableAlways()
    {
      throw UnreachableAlways((Exception)null);
    }

    /// <summary>
    /// Fails.
    /// </summary>
    [Pure]
    [System.Diagnostics.DebuggerNonUserCode]
    public static Exception UnreachableAlways(String format, params object[] args)
    {
      throw UnreachableAlways(null, format, args);
    }

    #endregion Unreachable
  }
}
