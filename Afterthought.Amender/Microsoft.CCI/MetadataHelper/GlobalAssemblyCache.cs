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
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci {
  /// <summary>
  /// Contains helper routines to query the GAC for the presence and locations of assemblies.
  /// </summary>
  [ContractVerification(false)]
  public static class GlobalAssemblyCache {
#if !COMPACTFX && !__MonoCS__
    private static bool FusionLoaded;
#endif

    //TODO: when loading Fusion, just enumerate the GAC and keep a static copy of the GAC. Release the assembly enumerator as soon as possible.

    /// <summary>
    /// Determines whether the GAC contains the specified code base URI.
    /// </summary>
    /// <param name="codeBaseUri">The code base URI.</param>
    public static bool Contains(Uri codeBaseUri) {
      Contract.Requires(codeBaseUri != null);
      
      lock (GlobalLock.LockingObject) {
#if COMPACTFX
        var gacKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"\Software\Microsoft\.NETCompactFramework\Installer\Assemblies\Global");
        if (gacKey == null) return false;
        var codeBase = codeBaseUri.AbsoluteUri;
        foreach (var gacName in gacKey.GetValueNames()) {
          var values = gacKey.GetValue(gacName) as string[];
          if (values == null || values.Length == 0) continue;
          if (string.Equals(values[0], codeBase, StringComparison.OrdinalIgnoreCase)) return true;
          if (values.Length > 1 && string.Equals(values[1], codeBase, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
#else
#if __MonoCS__
        IAssemblyEnum assemblyEnum = new MonoAssemblyEnum();
#else
        if (!GlobalAssemblyCache.FusionLoaded) {
          GlobalAssemblyCache.FusionLoaded = true;
          var systemAssembly = typeof(object).Assembly;
          var systemAssemblyLocation = systemAssembly.Location;
          string dir = Path.GetDirectoryName(systemAssemblyLocation)??"";
          GlobalAssemblyCache.LoadLibrary(Path.Combine(dir, "fusion.dll"));
        }
        IAssemblyEnum assemblyEnum;
        int rc = GlobalAssemblyCache.CreateAssemblyEnum(out assemblyEnum, null, null, ASM_CACHE.GAC, 0);
        if (rc < 0 || assemblyEnum == null) return false;
#endif
        IApplicationContext applicationContext;
        IAssemblyName currentName;
        while (assemblyEnum.GetNextAssembly(out applicationContext, out currentName, 0) == 0) {
          //^ assume currentName != null;
          AssemblyName assemblyName = new AssemblyName(currentName);
          string/*?*/ scheme = codeBaseUri.Scheme;
          if (scheme != null && assemblyName.CodeBase.StartsWith(scheme, StringComparison.OrdinalIgnoreCase)) {
            try {
              Uri foundUri = new Uri(assemblyName.CodeBase);
              if (codeBaseUri.Equals(foundUri)) return true;
            } catch (System.ArgumentNullException) {
            } catch (System.UriFormatException) {
            }
          }
        }
        return false;
#endif
      }
    }

    /// <summary>
    /// Returns the original location of the corresponding assembly if available, otherwise returns the location of the shadow copy.
    /// If the corresponding assembly is not in the GAC, null is returned.
    /// </summary>
    public static string/*?*/ GetLocation(AssemblyIdentity assemblyIdentity, IMetadataHost metadataHost) {
      lock (GlobalLock.LockingObject) {
#if COMPACTFX
        var gacKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"\Software\Microsoft\.NETCompactFramework\Installer\Assemblies\Global");
        foreach (var gacName in gacKey.GetValueNames()) {
          if (IdentityMatchesString(assemblyIdentity, gacName)) {
            var values = gacKey.GetValue(gacName) as string[];
            if (values == null || values.Length == 0) continue;
            return values[0];
          }
        }
        return null;
#else
#if __MonoCS__
        IAssemblyEnum assemblyEnum = new MonoAssemblyEnum();
#else
        if (!GlobalAssemblyCache.FusionLoaded) {
          GlobalAssemblyCache.FusionLoaded = true;
          var systemAssembly = typeof(object).Assembly;
          var systemAssemblyLocation = systemAssembly.Location;
          var dir = Path.GetDirectoryName(systemAssemblyLocation)??"";
          GlobalAssemblyCache.LoadLibrary(Path.Combine(dir, "fusion.dll"));
        }
        IAssemblyEnum assemblyEnum;
        int rc = CreateAssemblyEnum(out assemblyEnum, null, null, ASM_CACHE.GAC, 0);
        if (rc < 0 || assemblyEnum == null) return null;
#endif
        IApplicationContext applicationContext;
        IAssemblyName currentName;
        while (assemblyEnum.GetNextAssembly(out applicationContext, out currentName, 0) == 0) {
          //^ assume currentName != null;
          AssemblyName cn = new AssemblyName(currentName);
          if (assemblyIdentity.Equals(new AssemblyIdentity(metadataHost.NameTable.GetNameFor(cn.Name), cn.Culture, cn.Version, cn.PublicKeyToken, ""))) {
            string codeBase = cn.CodeBase;
            if (codeBase != null && codeBase.StartsWith("file://", StringComparison.OrdinalIgnoreCase)) {
              Uri u = new Uri(codeBase, UriKind.Absolute);
              return u.LocalPath;
            }
            return cn.GetLocation();
          }
        }
        return null;
#endif
      }
    }

#if COMPACTX
    private static bool IdentityMatchesString(AssemblyIdentity assemblyIdentity, string gacName) {
      int n = gacName.Length;
      var i = 0;
      if (!MatchIgnoringCase(assemblyIdentity.Name.Value, ',', gacName, ref i, n)) return false;
      while (i < n) {
        char ch = gacName[i];
        switch (ch) {
          case 'v':
          case 'V':
            if (!MatchIgnoringCase("Version", '=', gacName, ref i, n)) return false;
            if (!MatchDecimal(assemblyIdentity.Version.Major, '.', gacName, ref i, n)) return false;
            if (!MatchDecimal(assemblyIdentity.Version.MajorRevision, '.', gacName, ref i, n)) return false;
            if (!MatchDecimal(assemblyIdentity.Version.Minor, '.', gacName, ref i, n)) return false;
            if (!MatchDecimal(assemblyIdentity.Version.MinorRevision, ',', gacName, ref i, n)) return false;
            break;
          case 'C':
          case 'c':
            if (!MatchIgnoringCase("Culture", '=', gacName, ref i, n)) return false;
            var culture = assemblyIdentity.Culture;
            if (culture.Length == 0) culture = "neutral";
            if (!MatchIgnoringCase(culture, ',', gacName, ref i, n)) return false;
            break;
          case 'P':
          case 'p':
            if (!MatchIgnoringCase("PublicKeyToken", '=', gacName, ref i, n)) return false;
            if (!MatchHex(assemblyIdentity.PublicKeyToken, ',', gacName, ref i, n)) return false;
            break;
          default:
            return false;
        }
      }
      return true;
    }

    private static bool MatchHex(IEnumerable<byte> bytes, char delimiter, string gacName, ref int i, int n) {
      foreach (byte b in bytes) {
        if (i >= n-1) return false;
        var b1 = b >> 8;
        var b2 = b & 0xF;
        char c1 = b1 < 10 ? (char)(b1+'0') : (char)((b1-10)+'A');
        char c2 = b2 < 10 ? (char)(b2+'0') : (char)((b2-10)+'A');
        if (gacName[i++] != c1) return false;
        if (gacName[i++] != c2) return false;
      }
      SkipBlanks(gacName, ref i, n);
      if (i >= n) return true;
      if (gacName[i++] != delimiter) return false;
      SkipBlanks(gacName, ref i, n);
      return true;
    }

    private static void SkipBlanks(string gacName, ref int i, int n) {
      while (i < n && gacName[i] == ' ') i++;
    }

    private static bool MatchDecimal(int val, char delimiter, string gacName, ref int i, int n) {
      int num = 0;
      for (char ch = gacName[i++]; i < n; ) {
        int d = ch - '0';
        if (d < 0 || d > 9) return false;
        num = num*10 + d;
      }
      if (num != val) return false;
      SkipBlanks(gacName, ref i, n);
      if (i >= n) return true;
      if (gacName[i++] != delimiter) return false;
      SkipBlanks(gacName, ref i, n);
      return true;
    }

    private static bool MatchIgnoringCase(string str, char delimiter, string gacName, ref int i, int n) {
      var m = str.Length;
      int j = 0;
      while (j < m && i < n) {
        if (Char.ToLowerInvariant(str[j++]) != Char.ToLowerInvariant(gacName[i++])) return false;
      }
      if (j != m) return false;
      SkipBlanks(gacName, ref i, n);
      if (i >= n) return delimiter == ',';
      if (gacName[i++] != delimiter) return false;
      SkipBlanks(gacName, ref i, n);
      return true;
    }
#endif

#if !COMPACTFX && !__MonoCS__
    [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
    private static extern IntPtr LoadLibrary(string lpFileName);
    [DllImport("fusion.dll", CharSet=CharSet.Auto)]
    private static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum, IApplicationContext/*?*/ pAppCtx, IAssemblyName/*?*/ pName, uint dwFlags, int pvReserved);
    private class ASM_CACHE {
      private ASM_CACHE() { }
      public const uint ZAP = 1;
      public const uint GAC = 2;
      public const uint DOWNLOAD = 4;
    }
#endif
  }

#if !COMPACTFX
#pragma warning disable 1591
  public class AssemblyName {
    IAssemblyName assemblyName;

    internal AssemblyName(IAssemblyName assemblyName) {
      this.assemblyName = assemblyName;
      //^ base();
    }

    internal string Name {
      //set {this.WriteString(ASM_NAME.NAME, value);}
      get { return this.ReadString(ASM_NAME.NAME); }
    }
    internal Version Version {
      get {
        int major = this.ReadUInt16(ASM_NAME.MAJOR_VERSION);
        int minor = this.ReadUInt16(ASM_NAME.MINOR_VERSION);
        int build = this.ReadUInt16(ASM_NAME.BUILD_NUMBER);
        int revision = this.ReadUInt16(ASM_NAME.REVISION_NUMBER);
        return new Version(major, minor, build, revision);
      }
    }
    internal string Culture {
      get { return this.ReadString(ASM_NAME.CULTURE); }
    }
    internal byte[] PublicKeyToken {
      get { return this.ReadBytes(ASM_NAME.PUBLIC_KEY_TOKEN); }
    }
    internal string StrongName {
      get {
        uint usize = 0;
        this.assemblyName.GetDisplayName(null, ref usize, (uint)AssemblyNameDisplayFlags.ALL);
        int size = (int)usize;
        if (size <= 0) return "";
        StringBuilder strongName = new StringBuilder(size);
        this.assemblyName.GetDisplayName(strongName, ref usize, (uint)AssemblyNameDisplayFlags.ALL);
        return strongName.ToString();
      }
    }
    internal string CodeBase {
      get { return this.ReadString(ASM_NAME.CODEBASE_URL); }
    }
    //^ [Confined]
    public override string ToString() {
      return this.StrongName;
    }
    internal string/*?*/ GetLocation() {
#if __MonoCS__
      IAssemblyCache assemblyCache = new MonoAssemblyCache();
#else
      IAssemblyCache assemblyCache;
      CreateAssemblyCache(out assemblyCache, 0);
      if (assemblyCache == null) return null;
#endif
      ASSEMBLY_INFO assemblyInfo = new ASSEMBLY_INFO();
      assemblyInfo.cbAssemblyInfo = (uint)Marshal.SizeOf(typeof(ASSEMBLY_INFO));
      assemblyCache.QueryAssemblyInfo(ASSEMBLYINFO_FLAG.VALIDATE | ASSEMBLYINFO_FLAG.GETSIZE, this.StrongName, ref assemblyInfo);
      if (assemblyInfo.cbAssemblyInfo == 0) return null;
      assemblyInfo.pszCurrentAssemblyPathBuf = new string(new char[assemblyInfo.cchBuf]);
      assemblyCache.QueryAssemblyInfo(ASSEMBLYINFO_FLAG.VALIDATE | ASSEMBLYINFO_FLAG.GETSIZE, this.StrongName, ref assemblyInfo);
      String value = assemblyInfo.pszCurrentAssemblyPathBuf;
      return value;
    }
    private string ReadString(uint assemblyNameProperty) {
      uint size = 0;
      this.assemblyName.GetProperty(assemblyNameProperty, IntPtr.Zero, ref size);
      if (size == 0 || size > Int16.MaxValue) return String.Empty;
      IntPtr ptr = Marshal.AllocHGlobal((int)size);
      this.assemblyName.GetProperty(assemblyNameProperty, ptr, ref size);
      string/*?*/ str = Marshal.PtrToStringUni(ptr);
      //^ assume str != null;
      Marshal.FreeHGlobal(ptr);
      return str;
    }
    private ushort ReadUInt16(uint assemblyNameProperty) {
      uint size = 0;
      this.assemblyName.GetProperty(assemblyNameProperty, IntPtr.Zero, ref size);
      IntPtr ptr = Marshal.AllocHGlobal((int)size);
      this.assemblyName.GetProperty(assemblyNameProperty, ptr, ref size);
      ushort value = (ushort)Marshal.ReadInt16(ptr);
      Marshal.FreeHGlobal(ptr);
      return value;
    }
    private byte[] ReadBytes(uint assemblyNameProperty) {
      uint size = 0;
      this.assemblyName.GetProperty(assemblyNameProperty, IntPtr.Zero, ref size);
      IntPtr ptr = Marshal.AllocHGlobal((int)size);
      this.assemblyName.GetProperty(assemblyNameProperty, ptr, ref size);
      byte[] value = new byte[(int)size];
      Marshal.Copy(ptr, value, 0, (int)size);
      Marshal.FreeHGlobal(ptr);
      return value;
    }

    [DllImport("fusion.dll", CharSet=CharSet.Auto)]
    private static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);
    private class CREATE_ASM_NAME_OBJ_FLAGS {
      private CREATE_ASM_NAME_OBJ_FLAGS() { }
      public const uint CANOF_PARSE_DISPLAY_NAME = 0x1;
      public const uint CANOF_SET_DEFAULT_VALUES = 0x2;
    }
    private class ASM_NAME {
      private ASM_NAME() { }
      public const uint PUBLIC_KEY = 0;
      public const uint PUBLIC_KEY_TOKEN = 1;
      public const uint HASH_VALUE = 2;
      public const uint NAME = 3;
      public const uint MAJOR_VERSION = 4;
      public const uint MINOR_VERSION = 5;
      public const uint BUILD_NUMBER = 6;
      public const uint REVISION_NUMBER = 7;
      public const uint CULTURE = 8;
      public const uint PROCESSOR_ID_ARRAY = 9;
      public const uint OSINFO_ARRAY = 10;
      public const uint HASH_ALGID = 11;
      public const uint ALIAS = 12;
      public const uint CODEBASE_URL = 13;
      public const uint CODEBASE_LASTMOD = 14;
      public const uint NULL_PUBLIC_KEY = 15;
      public const uint NULL_PUBLIC_KEY_TOKEN = 16;
      public const uint CUSTOM = 17;
      public const uint NULL_CUSTOM = 18;
      public const uint MVID = 19;
      public const uint _32_BIT_ONLY = 20;
    }
    [Flags]
    internal enum AssemblyNameDisplayFlags {
      VERSION=0x01,
      CULTURE=0x02,
      PUBLIC_KEY_TOKEN=0x04,
      PROCESSORARCHITECTURE=0x20,
      RETARGETABLE=0x80,
      ALL=VERSION | CULTURE | PUBLIC_KEY_TOKEN | PROCESSORARCHITECTURE | RETARGETABLE
    }
    private class ASSEMBLYINFO_FLAG {
      private ASSEMBLYINFO_FLAG() { }
      public const uint VALIDATE = 1;
      public const uint GETSIZE = 2;
    }
    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct ASSEMBLY_INFO {
      public uint cbAssemblyInfo;
      public uint dwAssemblyFlags;
      public ulong uliAssemblySizeInKB;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string pszCurrentAssemblyPathBuf;
      public uint cchBuf;
    }
    [ComImport(), Guid("E707DCDE-D1CD-11D2-BAB9-00C04F8ECEAE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyCache {
      [PreserveSig()]
      int UninstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, IntPtr pvReserved, int pulDisposition);
      [PreserveSig()]
      int QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, ref ASSEMBLY_INFO pAsmInfo);
      [PreserveSig()]
      int CreateAssemblyCacheItem(uint dwFlags, IntPtr pvReserved, out object ppAsmItem, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName);
      [PreserveSig()]
      int CreateAssemblyScavenger(out object ppAsmScavenger);
      [PreserveSig()]
      int InstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath, IntPtr pvReserved);
    }
  }
#pragma warning restore 1591
  [ComImport(), Guid("CD193BC0-B4BC-11D2-9833-00C04FC31D2E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IAssemblyName {
    [PreserveSig()]
    int SetProperty(uint PropertyId, IntPtr pvProperty, uint cbProperty);
    [PreserveSig()]
    int GetProperty(uint PropertyId, IntPtr pvProperty, ref uint pcbProperty);
    [PreserveSig()]
    int Finalize();
    [PreserveSig()]
    int GetDisplayName(StringBuilder/*?*/ szDisplayName, ref uint pccDisplayName, uint dwDisplayFlags);
    [PreserveSig()]
    int BindToObject(object refIID, object pAsmBindSink, IApplicationContext pApplicationContext, [MarshalAs(UnmanagedType.LPWStr)] string szCodeBase, long llFlags, int pvReserved, uint cbReserved, out int ppv);
    [PreserveSig()]
    int GetName(out uint lpcwBuffer, out int pwzName);
    [PreserveSig()]
    int GetVersion(out uint pdwVersionHi, out uint pdwVersionLow);
    [PreserveSig()]
    int IsEqual(IAssemblyName pName, uint dwCmpFlags);
    [PreserveSig()]
    int Clone(out IAssemblyName pName);
  }
  [ComImport(), Guid("7C23FF90-33AF-11D3-95DA-00A024A85B51"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IApplicationContext {
    void SetContextNameObject(IAssemblyName pName);
    void GetContextNameObject(out IAssemblyName ppName);
    void Set([MarshalAs(UnmanagedType.LPWStr)] string szName, int pvValue, uint cbValue, uint dwFlags);
    void Get([MarshalAs(UnmanagedType.LPWStr)] string szName, out int pvValue, ref uint pcbValue, uint dwFlags);
    void GetDynamicDirectory(out int wzDynamicDir, ref uint pdwSize);
  }
  [ComImport(), Guid("21B8916C-F28E-11D2-A473-00C04F8EF448"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IAssemblyEnum {
    [PreserveSig()]
    int GetNextAssembly(out IApplicationContext ppAppCtx, out IAssemblyName ppName, uint dwFlags);
    [PreserveSig()]
    int Reset();
    [PreserveSig()]
    int Clone(out IAssemblyEnum ppEnum);
  }
#endif
}
