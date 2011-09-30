using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Diagnostics.Contracts
{
  public class RuntimeContracts
  {
    public static void Requires(bool cond, string userMessage, string conditionText)
    {
      var kind = ContractFailureKind.Precondition;
      CountCheck(kind);
      if (cond) return;
      ReportFailure(kind, userMessage, conditionText, null);
    }
    public static void Ensures(bool cond, string userMessage, string conditionText)
    {
      var kind = ContractFailureKind.Postcondition;
      CountCheck(kind);
      if (cond) return;
      ReportFailure(kind, userMessage, conditionText, null);
    }
    public static void EnsuresOnThrow(bool cond, string userMessage, string conditionText, Exception innerException)
    {
      var kind = ContractFailureKind.PostconditionOnException;
      CountCheck(kind);
      if (cond) return;
      ReportFailure(kind, userMessage, conditionText, innerException);
    }
    public static void Invariant(bool cond, string userMessage, string conditionText)
    {
      var kind = ContractFailureKind.Invariant;
      CountCheck(kind);
      if (cond) return;
      ReportFailure(kind, userMessage, conditionText, null);
    }
    public static void Assert(bool cond, string userMessage, string conditionText)
    {
      var kind = ContractFailureKind.Assert;
      CountCheck(kind);
      if (cond) return;
      ReportFailure(kind, userMessage, conditionText, null);
    }
    public static void Assume(bool cond, string userMessage, string conditionText)
    {
      var kind = ContractFailureKind.Assume;
      CountCheck(kind);
      if (cond) return;
      ReportFailure(kind, userMessage, conditionText, null);
    }

    public static void ReportFailure(ContractFailureKind kind, string userMessage, string conditionText, Exception innerException)
    {
      var msg = RaiseContractFailedEvent(kind, userMessage, conditionText, innerException);
      if (msg == null) return;
      TriggerFailure(kind, msg, userMessage, conditionText, innerException);
    }

    public static string RaiseContractFailedEvent(ContractFailureKind kind, string userMessage, string conditionText, Exception innerException)
    {
      CountFailure(kind);
#if DEBUG
      var kindString = KindToString(kind);
      var message = String.Format("{0} failed: {1} {2}", kindString, conditionText, userMessage);
      Console.WriteLine(message);
      return message;
#else
      // handled
      return null;
#endif
    }

    private static string KindToString(ContractFailureKind kind)
    {
      var kindString = "unknown";
      switch (kind)
      {
        case ContractFailureKind.Assert:
          kindString = "Contract.Assert";
          break;
        case ContractFailureKind.Assume:
          kindString = "Contract.Assume";
          break;
        case ContractFailureKind.Invariant:
          kindString = "Contract.Invariant";
          break;
        case ContractFailureKind.Postcondition:
          kindString = "Contract.Postcondition";
          break;
        case ContractFailureKind.PostconditionOnException:
          kindString = "Contract.PostconditionOnException";
          break;
        case ContractFailureKind.Precondition:
          kindString = "Contract.Precondition";
          break;
      }
      return kindString;
    }

    public static void TriggerFailure(ContractFailureKind kind, String displayMessage, String userMessage, String conditionText, Exception innerException)
    {
      throw new ContractException(kind, displayMessage, userMessage, conditionText, innerException);
    }

    #region Statistics
    struct Count
    {
      int numChecked;
      int numFailed;

      public void IncrementCheck()
      {
        numChecked++;
      }
      public void IncrementFail()
      {
        numFailed++;
      }
      public void Show(TextWriter tw, string kind) {
        tw.WriteLine("  {0}: {1} checked, {2} failed", kind, this.numChecked, this.numFailed);
      }
    }

    static void CountFailure(ContractFailureKind kind)
    {
      switch (kind)
      {
        case ContractFailureKind.Precondition:
          RequiresCount.IncrementFail();
          break;
        case ContractFailureKind.Assert:
          AssertCount.IncrementFail();
          break;
        case ContractFailureKind.Assume:
          AssumeCount.IncrementFail();
          break;
        case ContractFailureKind.Postcondition:
          EnsuresCount.IncrementFail();
          break;
        case ContractFailureKind.Invariant:
          InvariantCount.IncrementFail();
          break;
        case ContractFailureKind.PostconditionOnException:
          EnsuresOnThrowCount.IncrementFail();
          break;
      }
    }
    static void CountCheck(ContractFailureKind kind)
    {
      switch (kind)
      {
        case ContractFailureKind.Precondition:
          RequiresCount.IncrementCheck();
          break;
        case ContractFailureKind.Assert:
          AssertCount.IncrementCheck();
          break;
        case ContractFailureKind.Assume:
          AssumeCount.IncrementCheck();
          break;
        case ContractFailureKind.Postcondition:
          EnsuresCount.IncrementCheck();
          break;
        case ContractFailureKind.Invariant:
          InvariantCount.IncrementCheck();
          break;
        case ContractFailureKind.PostconditionOnException:
          EnsuresOnThrowCount.IncrementCheck();
          break;
      }
    }
    static Count RequiresCount;
    static Count EnsuresCount;
    static Count EnsuresOnThrowCount;
    static Count AssertCount;
    static Count AssumeCount;
    static Count InvariantCount;

    public static void ShowStats(TextWriter tw)
    {
      Console.Error.WriteLine("=== RuntimeConstracts");
      RequiresCount.Show(tw, "Requires");
      EnsuresCount.Show(tw, "Ensures");
      EnsuresOnThrowCount.Show(tw, "EnsuresOnThrow");
      InvariantCount.Show(tw, "Invariants");
      AssertCount.Show(tw, "Asserts");
      AssumeCount.Show(tw, "Assumes");
    }
    #endregion
  }
}
