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
using System.Diagnostics.Contracts;
using System.Text;
using System.IO;

namespace Microsoft.Cci
{
  /// <summary>
  /// Subclass this class and define public fields for options
  /// </summary>
  [ContractVerification(false)]
  public abstract class OptionParsing //TODO: move this to another assembly.
  {
    /// <summary>
    /// Base constructor for building parseable options
    /// </summary>
    public OptionParsing()
    {
      this.requiredOptions = GatherRequiredOptions();
    }

    [ContractInvariantMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
    private void ObjectInvariant()
    {
      Contract.Invariant(this.generalArguments != null);
      Contract.Invariant(this.errorMessages != null);
    }

    /// <summary>
    /// The number of errors discovered during command-line option parsing.
    /// </summary>
    protected int errors = 0;
    private bool helpRequested;

    /// <summary>
    /// True if and only if a question mark was given as a command-line option.
    /// </summary>
    public bool HelpRequested { get { return helpRequested; } }

    /// <summary>
    /// True if and only if some command-line option caused a parsing error, or specifies an option
    /// that does not exist.
    /// </summary>
    public bool HasErrors { get { return errors > 0; } }

    List<string> errorMessages = new List<string>();

    /// <summary>
    /// Allows a client to signal that there is an error in the command-line options.
    /// </summary>
    public void AddError() { this.errors++; }

    /// <summary>
    /// Allows a client to signal that there is an error in the command-line options.
    /// </summary>
    public void AddError(string format, params object[] args)
    {
      this.AddMessage(format, args);
      this.errors++;
    }

    /// <summary>
    /// Allows a client add a message to the output.
    /// </summary>
    public void AddMessage(string format, params object[] args)
    {
      this.errorMessages.Add(String.Format(format, args));
    }

    /// <summary>
    /// The list of errors or other messages produced during parsing
    /// </summary>
    protected IEnumerable<string> Messages
    {
      get
      {
        Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
        return this.errorMessages;
      }
    }

    /// <summary>
    /// Put this on fields if you want a more verbose help description
    /// </summary>
    protected class OptionDescription : Attribute
    {
      /// <summary>
      /// The text that is shown when the usage is displayed.
      /// </summary>
      readonly public string Description;
      /// <summary>
      /// Constructor for creating the information about an option.
      /// </summary>
      public OptionDescription(string s) { this.Description = s; }
      /// <summary>
      /// Indicates whether the associated option is required or not.
      /// </summary>
      public bool Required { get; set; }
      /// <summary>
      /// Indicates a short form for the option. Very useful for options
      /// whose names are reserved keywords.
      /// </summary>
      public string ShortForm { get; set; }
    }

    /// <summary>
    /// Put this on fields if you want the field to be relevant when hashing an Option object
    /// </summary>
    protected class OptionWitness : Attribute { }

    /// <summary>
    /// If a field has this attribute, then its value is inherited by all the family of analyses
    /// </summary>
    public class OptionValueOverridesFamily : Attribute { }

    /// <summary>
    /// A way to have a single option be a macro for several options.
    /// </summary>
    protected class OptionFor : Attribute
    {
      /// <summary>
      /// The field that this option is a macro for.
      /// </summary>
      readonly public string options;
      /// <summary>
      /// Constructor for specifying which field this is a macro option for.
      /// </summary>
      public OptionFor(string options)
      {
        this.options = options;
      }
    }

    /// <summary>
    /// Override and return false if options do not start with '-' or '/'
    /// </summary>
    protected virtual bool UseDashOptionPrefix { get { return true; } }

    /// <summary>
    /// This field will hold non-option arguments
    /// </summary>
    readonly List<string> generalArguments = new List<string>();

    private IList<string> requiredOptions;

    /// <summary>
    /// The non-option arguments provided on the command line.
    /// </summary>
    public List<string> GeneralArguments
    {
      get
      {
        Contract.Ensures(Contract.Result<List<string>>() != null);

        return this.generalArguments;
      }
    }

    #region Parsing and Reflection

    /// <summary>
    /// Called when reflection based resolution does not find option
    /// </summary>
    /// <param name="option">option name (no - or /)</param>
    /// <param name="args">all args being parsed</param>
    /// <param name="index">current index of arg</param>
    /// <param name="optionEqualsArgument">null, or the optionArgument when option was option=optionArgument</param>
    /// <returns>true if option is recognized, false otherwise</returns>
    protected virtual bool ParseUnknown(string option, string[] args, ref int index, string optionEqualsArgument)
    {
      return false;
    }

    /// <summary>
    /// Main method called by a client to process the command-line options.
    /// </summary>
    public void Parse(string[] args)
    {
      int index = 0;
      while (index < args.Length)
      {
        string arg = args[index];

        if (arg == "") { index++; continue; }

        if (arg[0] == '@')
        {
          var responseFile = arg.Substring(1);
          ParseResponseFile(responseFile);
        }
        else if (!UseDashOptionPrefix || arg[0] == '/' || arg[0] == '-')
        {
          if (UseDashOptionPrefix)
          {
            arg = arg.Remove(0, 1);
          }
          if (arg == "?")
          {
            this.helpRequested = true;
            index++;
            continue;
          }

          string equalArgument = null;
          int equalIndex = arg.IndexOf(':');
          if (equalIndex <= 0)
          {
            equalIndex = arg.IndexOf('=');
            if (equalIndex < 0) // Also add '!' as synonim for '=', as cmd.exe performs fuzzy things with '='
              equalIndex = arg.IndexOf('!');
          }
          if (equalIndex > 0)
          {
            equalArgument = arg.Substring(equalIndex + 1);
            arg = arg.Substring(0, equalIndex);
          }

          bool optionOK = this.FindOptionByReflection(arg, args, ref index, equalArgument);
          if (!optionOK)
          {
            optionOK = this.ParseUnknown(arg, args, ref index, equalArgument);
            if (!optionOK)
            {
              AddError("Unknown option '{0}'", arg);
            }
          }
        }
        else
        {
          this.generalArguments.Add(arg);
        }

        index++;
      }
      if (!helpRequested) CheckRequiredOptions();
    }

    private void ParseResponseFile(string responseFile)
    {
      if (!File.Exists(responseFile))
      {
        AddError("Response file '{0}' does not exist.", responseFile);
        return;
      }
      try
      {
        var lines = File.ReadAllLines(responseFile);
        for (int i = 0; i < lines.Length; i++)
        {
          var line = lines[i];
          if (line.Length == 0) continue;
          if (line[0] == '#') {
            // comment skip
            continue;
          }
          if (ContainsQuote(line))
          {
            Parse(SplitLineWithQuotes(responseFile, i, line));
          }
          else
          {
            // simple splitting
            Parse(line.Split(' '));
          }
        }
      }
      catch
      {
        AddError("Failure reading from response file '{0}'.", responseFile);
      }
    }

    private string[] SplitLineWithQuotes(string responseFileName, int lineNo, string line)
    {
      var start = 0;
      var args = new List<string>();
      bool inDoubleQuotes = false;
      int escaping = 0; // number of escape characters in sequence
      var currentArg = new StringBuilder();

      for (var index = 0; index < line.Length; index++)
      {
        var current = line[index];

        if (current == '\\')
        {
          // escape character
          escaping++;
          // swallow the escape character for now
          // grab everything prior to prior escape character
          if (index > start)
          {
            currentArg.Append(line.Substring(start, index - start));
          }
          start = index + 1;
          continue;
        }
        if (escaping > 0)
        {
          if (current == '"')
          {
            var backslashes = escaping / 2;
            for (int i = 0; i < backslashes; i++) { currentArg.Append('\\'); }
            if (escaping % 2 == 1)
            {
              // escapes the "
              currentArg.Append('"');
              escaping = 0;
              start = index + 1;
              continue;
            }
          }
          else
          {
            var backslashes = escaping;
            for (int i = 0; i < backslashes; i++) { currentArg.Append('\\'); }
          }
          escaping = 0;
        }
        if (inDoubleQuotes)
        {
          if (current == '"')
          {
            // ending argument
            FinishArgument(line, start, args, currentArg, index, true);
            start = index + 1;
            inDoubleQuotes = false;
            continue;
          }
        }
        else // not in quotes
        {
          if (Char.IsWhiteSpace(current))
          {
            // end previous, start new
            FinishArgument(line, start, args, currentArg, index, false);
            start = index + 1;
            continue;
          }
          if (current == '"')
          {
            // starting double quote
            if (index != start)
            {
              AddError("Response file '{0}' line {1}, char {2} contains '\"' not starting or ending an argument", responseFileName, lineNo, index);
            }
            start = index + 1;
            inDoubleQuotes = true;
            continue;
          }
        }
      }
      // add outstanding escape characters
      while (escaping > 0) { currentArg.Append('\\'); escaping--; }
      FinishArgument(line, start, args, currentArg, line.Length, inDoubleQuotes);
      return args.ToArray();
    }

    private static void FinishArgument(string line, int start, List<string> args, StringBuilder currentArg, int index, bool includeEmpty)
    {
      currentArg.Append(line.Substring(start, index - start));
      if (includeEmpty || currentArg.Length > 0)
      {
        args.Add(currentArg.ToString());
        currentArg.Length = 0;
      }
    }

    private bool ContainsQuote(string line)
    {
      var index = line.IndexOf('"');
      if (index >= 0) return true;
      index = line.IndexOf('\'');
      if (index >= 0) return true;
      return false;
    }


    private void CheckRequiredOptions()
    {
      foreach (var missed in this.requiredOptions)
      {
        AddError("Required option '-{0}' was not given.", missed);
      }
    }

    private IList<string> GatherRequiredOptions()
    {
      List<string> result = new List<string>();

      foreach (var field in this.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
      {
        var options = field.GetCustomAttributes(typeof(OptionDescription), false);
        foreach (OptionDescription attr in options)
        {
          if (attr.Required)
          {
            result.Add(field.Name.ToLowerInvariant());
          }
        }
      }
      return result;
    }

    private bool ParseValue<T>(Converter<string, T> parser, string equalArgument, string[] args, ref int index, ref object result)
    {
      if (equalArgument == null)
      {
        if (index + 1 < args.Length)
        {
          equalArgument = args[++index];
        }
      }
      bool success = false;
      if (equalArgument != null)
      {
        try
        {
          result = parser(equalArgument);
          success = true;
        }
        catch
        {
        }
      }
      return success;
    }

    private bool ParseValue<T>(Converter<string, T> parser, string argument, ref object result)
    {
      bool success = false;
      if (argument != null)
      {
        try
        {
          result = parser(argument);
          success = true;
        }
        catch
        {
        }
      }
      return success;
    }

    /// <summary>
    /// A delegate that represents the various TryParse methods from int, bool, etc.
    /// </summary>
    public delegate bool TryParseConverter<in TInput, TOutput>(TInput input, out TOutput output);

    private bool TryParseValue<T>(TryParseConverter<string, T> tryParser, string argument, ref T result) {
      return (argument != null) ? tryParser(argument, out result) : false;
    }

    private object ParseValue(Type type, string argument, string option)
    {
      object result = null;
      if (type == typeof(bool))
      {
        bool boolResult = false;
        if (argument != null)
        {
          if (!TryParseValue<bool>(Boolean.TryParse, argument, ref boolResult))
          {
            // Allow "+/-" to turn on/off boolean options
            if (argument.Equals("-"))
            {
              result = false;
            }
            else if (argument.Equals("+"))
            {
              result = true;
            }
            else
            {
              AddError("option -{0} requires a bool argument", option);
            }
          }
          else
          {
            result = boolResult;
          }
        }
        else
        {
          result = true;
        }
      }
      else if (type == typeof(string))
      {
        if (!ParseValue<string>(s => s, argument, ref result))
        {
          AddError("option -{0} requires a string argument", option);
        }
      }
      else if (type == typeof(int))
      {
        int intResult = 0;
        if (!TryParseValue<int>(Int32.TryParse, argument, ref intResult))
        {
          AddError("option -{0} requires an int argument", option);
        }
        else
        {
          result = intResult;
        }
      }
      else if (type.IsEnum)
      {
        if (!ParseValue<object>(ParseEnum(type), argument, ref result))
        {
          AddError("option -{0} expects one of", option);

          foreach (System.Reflection.FieldInfo enumConstant in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
          {
            if (enumConstant.IsLiteral)
            {
              AddMessage("  {0}", enumConstant.Name);
            }
          }
        }
      }
      return result;
    }

    string AdvanceArgumentIfNoExplicitArg(Type type, string explicitArg, string[] args, ref int index)
    {
      if (explicitArg != null) return explicitArg;
      if (type == typeof(bool))
      {
        // bool args don't grab the next arg
        return null;
      }
      if (index + 1 < args.Length)
      {
        return args[++index];
      }
      return null;
    }

    private bool FindOptionByReflection(string arg, string[] args, ref int index, string explicitArgument)
    {
      System.Reflection.FieldInfo fi = this.GetType().GetField(arg, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
      if (fi != null)
      {
        this.requiredOptions.Remove(arg.ToLowerInvariant());
        return ProcessOptionWithMatchingField(arg, args, ref index, explicitArgument, ref fi);
      }
      else
      {
        // derived options
        fi = this.GetType().GetField(arg, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        if (fi != null)
        {
          object derived = fi.GetValue(this);

          if (derived is string)
          {
            this.Parse(((string)derived).Split(' '));
            this.requiredOptions.Remove(arg.ToLowerInvariant());
            return true;
          }
        }

        // Try to see if the arg matches any ShortForm of an option
        var allFields = this.GetType().GetFields();
        System.Reflection.FieldInfo matchingField = null;
        foreach (var f in allFields)
        {
          matchingField = f;
          var options = matchingField.GetCustomAttributes(typeof(OptionDescription), false);
          foreach (OptionDescription attr in options)
          {
            if (attr.ShortForm != null)
            {
              var lower1 = attr.ShortForm.ToLowerInvariant();
              var lower2 = arg.ToLowerInvariant();
              if (lower1.Equals(lower2))
              {
                this.requiredOptions.Remove(matchingField.Name.ToLowerInvariant());
                return ProcessOptionWithMatchingField(arg, args, ref index, explicitArgument, ref matchingField);
              }
            }
          }
        }
      }
      return false;
    }

    private bool ProcessOptionWithMatchingField(string arg, string[] args, ref int index, string explicitArgument, ref System.Reflection.FieldInfo fi)
    {
      Type type = fi.FieldType;
      bool isList = false;
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
      {
        isList = true;
        type = type.GetGenericArguments()[0];
      }
      if (isList && explicitArgument == "!!")
      {
        // way to set list to empty
        System.Collections.IList listhandle = (System.Collections.IList)fi.GetValue(this);
        listhandle.Clear();
        return true;
      }
      string argument = AdvanceArgumentIfNoExplicitArg(type, explicitArgument, args, ref index);
      if (isList)
      {
        if (argument == null) {
          AddError("option -{0} requires an argument", arg);
          return true;
        }
        string[] listargs = argument.Split(';');
        for (int i = 0; i < listargs.Length; i++)
        {
          if (listargs[i].Length == 0) continue; // skip empty values
          bool remove = listargs[i][0] == '!';
          string listarg = remove ? listargs[i].Substring(1) : listargs[i];
          object value = ParseValue(type, listarg, arg);
          if (value != null)
          {
            if (remove)
            {
              this.GetListField(fi).Remove(value);
            }
            else
            {
              this.GetListField(fi).Add(value);
            }
          }
        }
      }
      else
      {
        object value = ParseValue(type, argument, arg);
        if (value != null)
        {
          fi.SetValue(this, value);

          string argname;
          if (value is Int32 && HasOptionForAttribute(fi, out argname))
          {
            this.Parse(DerivedOptionFor(argname, (Int32)value).Split(' '));
          }

        }
      }
      return true;
    }

    private bool HasOptionForAttribute(System.Reflection.FieldInfo fi, out string argname)
    {
      var options = fi.GetCustomAttributes(typeof(OptionFor), true);
      if (options != null && options.Length == 1)
      {
        argname = ((OptionFor)options[0]).options;
        return true;
      }

      argname = null;
      return false;
    }

    /// <summary>
    /// For the given field, returns the derived option that is indexed by
    /// option in the list of derived options.
    /// </summary>
    protected string DerivedOptionFor(string fieldWithOptions, int option)
    {
      string[] options;
      if (TryGetOptions(fieldWithOptions, out options))
      {
        if (option < 0 || option >= options.Length)
        {
          return "";
        }

        return options[option];
      }

      return "";
    }

    /// <summary>
    /// Returns the options associated with the field, specified as a string.
    /// If there are none, options is set to null and false is returned.
    /// </summary>
    protected bool TryGetOptions(string fieldWithOptions, out string[] options)
    {
      var fi = this.GetType().GetField(fieldWithOptions,
        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Public);

      if (fi != null)
      {
        var obj = fi.GetValue(this);

        if (obj is string[])
        {
          options = (string[])obj;
          return true;
        }
      }

      options = null;
      return false;
    }

    /// <summary>
    /// Use this 
    /// </summary>
    public long GetCheckCode()
    {
      var res = 0L;
      foreach (var f in this.GetType().GetFields())
      {
        foreach (var a in f.GetCustomAttributes(true))
        {
          if (a is OptionWitness)
          {
            res += (f.GetValue(this).GetHashCode()) * f.GetHashCode();

            break;
          }
        }
      }

      return res;
    }

    Converter<string, object> ParseEnum(Type enumType)
    {
      return delegate(string s) { return Enum.Parse(enumType, s, true); };
    }

    System.Collections.IList GetListField(System.Reflection.FieldInfo fi)
    {
      object obj = fi.GetValue(this);
      if (obj != null) { return (System.Collections.IList)obj; }
      System.Collections.IList result = (System.Collections.IList)fi.FieldType.GetConstructor(new Type[] { }).Invoke(new object[] { });
      fi.SetValue(this, result);
      return result;
    }

    /// <summary>
    /// Writes all of the options out to the console.
    /// </summary>
    public void PrintOptions(string indent)
    {
      foreach (System.Reflection.FieldInfo f in this.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
      {
        System.Type opttype = f.FieldType;
        bool isList;
        if (opttype.IsGenericType && opttype.GetGenericTypeDefinition() == typeof(List<>))
        {
          opttype = opttype.GetGenericArguments()[0];
          isList = true;
        }
        else
        {
          isList = false;
        }
        string description = GetOptionAttribute(f);
        string option = null;
        if (opttype == typeof(bool))
        {
          if (!isList && f.GetValue(this).Equals(true))
          {
            option = String.Format("{0} (default=true)", f.Name);
          }
          else
          {
            option = f.Name;
          }
        }
        else if (opttype == typeof(string))
        {
          if (!f.IsLiteral)
          {
            object defaultValue = f.GetValue(this);
            if (!isList && defaultValue != null)
            {
              option = String.Format("{0} <string-arg> (default={1})", f.Name, defaultValue);
            }
            else
            {
              option = String.Format("{0} <string-arg>", f.Name);
            }
          }
        }
        else if (opttype == typeof(int))
        {
          if (!isList)
          {
            option = String.Format("{0} <int-arg> (default={1})", f.Name, f.GetValue(this));
          }
          else
          {
            option = String.Format("{0} <int-arg>", f.Name);
          }
        }
        else if (opttype.IsEnum)
        {
          StringBuilder sb = new StringBuilder();
          sb.Append(f.Name).Append(" (");
          bool first = true;
          foreach (System.Reflection.FieldInfo enumConstant in opttype.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
          {
            if (enumConstant.IsLiteral)
            {
              if (!first)
              {
                if (isList)
                {
                  sb.Append(" + ");
                }
                else
                {
                  sb.Append(" | ");
                }
              }
              else
              {
                first = false;
              }
              sb.Append(enumConstant.Name);
            }
          }
          sb.Append(") ");
          if (!isList)
          {
            sb.AppendFormat("(default={0})", f.GetValue(this));
          }
          else
          {
            sb.Append("(default=");
            bool first2 = true;
            foreach (object eval in (System.Collections.IEnumerable)f.GetValue(this))
            {
              if (!first2)
              {
                sb.Append(',');
              }
              else
              {
                first2 = false;
              }
              sb.Append(eval.ToString());
            }
            sb.Append(')');
          }
          option = sb.ToString();
        }
        if (option != null)
        {
          Console.Write("{1}   -{0,-30}", option, indent);

          if (description != null)
          {
            Console.WriteLine(" : {0}", description);
          }
          else
          {
            Console.WriteLine();
          }
        }
      }
      Console.WriteLine(Environment.NewLine + "To clear a list, use -<option>=!!");
      Console.WriteLine(Environment.NewLine + "To remove an item from a list, use -<option> !<item>");

    }

    /// <summary>
    /// Prints all of the derived options to the console.
    /// </summary>
    public void PrintDerivedOptions(string indent)
    {
      foreach (System.Reflection.FieldInfo f in this.GetType().GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
      {
        if (f.IsLiteral)
        {
          Console.WriteLine("{2}   -{0} is '{1}'", f.Name, f.GetValue(this), indent);
        }
      }
    }

    private string GetOptionAttribute(System.Reflection.FieldInfo f)
    {
      object[] attrs = f.GetCustomAttributes(typeof(OptionDescription), true);
      if (attrs != null && attrs.Length == 1)
      {
        StringBuilder result = new StringBuilder();
        OptionDescription descr = (OptionDescription)attrs[0];
        if (descr.Required)
        {
          result.Append("(required) ");
        }
        result.Append(descr.Description);
        if (descr.ShortForm != null)
        {
          result.Append("[short form: " + descr.ShortForm + "]");
        }
        object[] optionsFor = f.GetCustomAttributes(typeof(OptionFor), true);
        string[] options;

        if (optionsFor != null && optionsFor.Length == 1 && TryGetOptions(((OptionFor)optionsFor[0]).options, out options))
        {
          result.AppendLine(Environment.NewLine + "Detailed explanation:");
          for (int i = 0; i < options.Length; i++)
          {
            result.Append(string.Format("{0} : {1}" + Environment.NewLine, i, options[i]));
          }
        }

        return result.ToString();
      }
      return null;
    }


    #endregion

    /// <summary>
    /// Call this when HasErrors is true
    /// It will print the errors on the given output and then exit the process with Environment.Exit(-1)
    /// </summary>
    /// <param name="output">Output stream for messages</param>
    public void PrintErrorsAndExit(TextWriter output)
    {
      foreach (string message in this.errorMessages)
      {
        output.WriteLine(message);
      }
      output.WriteLine();
      output.WriteLine("  use /? to see a list of options");
      Environment.Exit(-1);
    }
  }
}
