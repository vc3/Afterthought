//Contributed by YodaSkywalker under the MSPL license.

#if __MonoCS__
using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Cci;

	class MonoGACHelpers
	{
		public static string GetGACDir()
		{
			System.Reflection.PropertyInfo gac = typeof (System.Environment).GetProperty ("GacPath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic );
            if ( gac == null ) 
			{
				Console.WriteLine( "Can't find the GAC!" );
				Environment.Exit( 1 );
			}

			System.Reflection.MethodInfo get_gac = gac.GetGetMethod (true);
            string sGACDir = (string) get_gac.Invoke (null, null);
		
			return sGACDir;
		}
	}
	
	// Since Mono doesn't have a fusion.dll to access the GAC, this is a MINIMAL implementation
	// of IAssemblyCache. It only has enough to satisfy GlobalAssemblyCache's usage above!
	class MonoAssemblyCache : AssemblyName.IAssemblyCache
	{
		public MonoAssemblyCache()
		{
		}
		
		public int UninstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, IntPtr pvReserved, int pulDisposition)
		{
			Debug.Assert( false );
			return 0;
		}

		public int QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, ref AssemblyName.ASSEMBLY_INFO pAsmInfo)
		{
			pAsmInfo = new AssemblyName.ASSEMBLY_INFO();
			pAsmInfo.cbAssemblyInfo = 1;	// just needs to be nonzero for our purposes
			
			// All they need here is pszCurrentAssemblyPathBuf to be filled out.
			// pszAssemblyName is the strong name (as returned from MonoAssemblyName.GetDisplayName), 
			// like "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
			//
			// Mono stores them in monoDir\lib\mono\gac\ASSEMBLY_NAME\VERSION__PUBLICKEYTOKEN\ASSEMBLY_NAME.dll
			//
			// .. so this strong name  : "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
			// .. would be located here: monoDir\lib\mono\gac\System.Core\4.0.0.0__b77a5c561934e089\System.Core.dll
			string [] parts = pszAssemblyName.Split( new string[] {", "}, StringSplitOptions.RemoveEmptyEntries );

			string sAssemblyName = parts[0];
			string sVersion = parts[1].Split( new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries )[1];
			string sPublicKeyToken = parts[3].Split( new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries )[1];

			string sGACDir = MonoGACHelpers.GetGACDir();
			sGACDir = Path.Combine( sGACDir, sAssemblyName );
			sGACDir = Path.Combine( sGACDir, sVersion + "__" + sPublicKeyToken );
			
			pAsmInfo.pszCurrentAssemblyPathBuf = Path.Combine( sGACDir, sAssemblyName + ".dll" );
			
			Debug.Assert( false );
			return 0;
		}

		public int CreateAssemblyCacheItem(uint dwFlags, IntPtr pvReserved, out object ppAsmItem, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName)
		{
			ppAsmItem = null;
			Debug.Assert( false );
			return 0;
		}

		public int CreateAssemblyScavenger(out object ppAsmScavenger)
		{
			ppAsmScavenger = null;
			Debug.Assert( false );
			return 0;
		}

		public int InstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath, IntPtr pvReserved)
		{
			Debug.Assert( false );
			return 0;
		}
	}
		
	// Since Mono doesn't have a fusion.dll to access the GAC, this is a MINIMAL implementation
	// of IAssemblyName. It only has enough to satisfy GlobalAssemblyCache's usage above!
	class MonoAssemblyName : IAssemblyName
	{
		public MonoAssemblyName( string sAssemblyName, string sVersion, string sCulture, string sPublicKeyToken, string sDisplayName )
		{
			m_sAssemblyName = sAssemblyName;
			m_sVersion = sVersion;
			m_sCulture = sCulture;
			m_sPublicKeyToken = sPublicKeyToken;
			m_sDisplayName  = sDisplayName;
		}
		
	    public int SetProperty(uint PropertyId, IntPtr pvProperty, uint cbProperty)
		{
			Debug.Assert( false );
			return 0;
		}
		
	    public int GetProperty(uint PropertyId, IntPtr pvProperty, ref uint pcbProperty)
		{
			string sSource = null;
			if ( PropertyId == MY_ASM_NAME.NAME )
				sSource = m_sAssemblyName; 
			else if ( PropertyId == MY_ASM_NAME.CULTURE )
				sSource = "";   // The Visual Studio version gets an empty string for culture.. otherwise it'd be m_sCulture.
			else if ( PropertyId == MY_ASM_NAME.CODEBASE_URL )
				sSource = "";

			if ( sSource != null )
			{
				// They're asking for a string.
				int nBytes = sSource.Length * 2 + 2;
				pcbProperty = (uint)nBytes;
				if ( pvProperty == IntPtr.Zero )
					return 0;
				
				// Get the unicode string into bytes.
				IntPtr pString = Marshal.StringToHGlobalUni( sSource );
				byte [] bytes = new byte[nBytes];
				Marshal.Copy( pString, bytes, 0, nBytes );
				Marshal.FreeHGlobal( pString );
				
				// Get the bytes into their dest array.
				Marshal.Copy( bytes, 0, pvProperty, nBytes );
				
				return 0;
			}
			
			
			// Do they want an int16?
			if ( PropertyId == MY_ASM_NAME.MAJOR_VERSION || PropertyId == MY_ASM_NAME.MINOR_VERSION || PropertyId == MY_ASM_NAME.BUILD_NUMBER || PropertyId == MY_ASM_NAME.REVISION_NUMBER )
			{
				pcbProperty = 2;
				
				// Do they only want the size?
				if ( pvProperty == IntPtr.Zero )
				{
					return 0;
				}
				
				string [] versionParts = m_sVersion.Split( new char[] {'.'} );
				string sValuePart = ( PropertyId == MY_ASM_NAME.MAJOR_VERSION ) ? versionParts[0] :
					(
						( PropertyId == MY_ASM_NAME.MINOR_VERSION ) ? versionParts[1] : 
							(
								( PropertyId == MY_ASM_NAME.BUILD_NUMBER ) ? versionParts[2] : versionParts[3]
							)
					);
				
				UInt16 value = Convert.ToUInt16( sValuePart );
				Marshal.WriteInt16( pvProperty, (Int16)value );
				return 0;
			}
			
			
			// Do they want raw bytes?
			if ( PropertyId == MY_ASM_NAME.PUBLIC_KEY_TOKEN )
			{
				pcbProperty = 8;
				
				if ( pvProperty == IntPtr.Zero )
					return 0;
				
				byte [] bytes = new byte[8];
				for ( int i=0; i < 8; i++ )
				{
					string s = m_sPublicKeyToken.Substring( i*2, 2 );
					bytes[i] = Convert.ToByte( s, 16 );
				}
				
				Marshal.Copy( bytes, 0, pvProperty, 8 );
				return 0;
			}
			
			Debug.Assert( false );
			return 0;
		}
		
	    public int Finalize()
		{
			Debug.Assert( false );
			return 0;
		}
		
	    public int GetDisplayName(StringBuilder/*?*/ szDisplayName, ref uint pccDisplayName, uint dwDisplayFlags)
		{
			pccDisplayName = 1; // They only care if this is nonzero.
			
			if ( szDisplayName != null )
			{
				szDisplayName.Append( m_sDisplayName );	
			}
			
			return 1;
		}
		
	    public int BindToObject(object refIID, object pAsmBindSink, IApplicationContext pApplicationContext, [MarshalAs(UnmanagedType.LPWStr)] string szCodeBase, long llFlags, int pvReserved, uint cbReserved, out int ppv)
		{
			ppv = 0;
			Debug.Assert( false );
			return 0;
		}
		
	    public int GetName(out uint lpcwBuffer, out int pwzName)
		{
			lpcwBuffer = 0;
			pwzName = 0;
			Debug.Assert( false );
			return 0;
		}
		
	    public int GetVersion(out uint pdwVersionHi, out uint pdwVersionLow)
		{
			pdwVersionHi = pdwVersionLow = 0;
			Debug.Assert( false );
			return 0;
		}
		
	    public int IsEqual(IAssemblyName pName, uint dwCmpFlags)
		{
			Debug.Assert( false );
			return 0;
		}
		
	    public int Clone(out IAssemblyName pName)
		{
			pName = null;
			Debug.Assert( false );
			return 0;
		}
	    
		private class MY_ASM_NAME {
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
		
		string m_sAssemblyName;
		string m_sVersion;
		string m_sCulture;
		string m_sPublicKeyToken;
		string m_sDisplayName;
	}
	
	// Since Mono doesn't have a fusion.dll to access the GAC, this is a MINIMAL implementation
	// of IAssemblyEnum. It only has enough to satisfy GlobalAssemblyCache's usage above!
	class MonoAssemblyEnum : IAssemblyEnum
	{
		public MonoAssemblyEnum()
		{
			string binDir = MonoGACHelpers.GetGACDir();
			binDir = Path.GetDirectoryName( binDir ); 
			binDir = Path.GetDirectoryName( binDir ); 
			binDir = Path.GetDirectoryName( binDir ); 
			
			// Run gacutil to get the list.
			ProcessStartInfo info = new ProcessStartInfo();
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			
			if ( (int)Environment.OSVersion.Platform == 4 || (int)Environment.OSVersion.Platform == 128 )
			{
				// Running under Unix.
				info.FileName = "gacutil";
				info.Arguments = "-l";
			}
			else
			{
				// Running under Windows.
				info.FileName = "cmd.exe";
				info.Arguments = "/C \"" + Path.Combine( Path.Combine(binDir, "bin"), "gacutil.bat" ) + "\" -l";
			}
			
			Process p = Process.Start( info );
			string sOutput = p.StandardOutput.ReadToEnd();
			p.Close();
			
			string [] lines = sOutput.Split( new char[] { '\n' } );
			m_Names = new MonoAssemblyName[lines.Length - 3];
			for ( int i=1; i < lines.Length-2; i++ )
			{
				string sLine = lines[i].Replace( "\r", "" );
				string [] parts = sLine.Split( new string[] {", "}, StringSplitOptions.RemoveEmptyEntries );
				string sAssemblyName = parts[0];
				string sVersion = parts[1].Split( new char[] {'='} )[1];
				string sCulture = parts[2].Split( new char[] {'='} )[1];
				string sPublicKeyToken = parts[3].Split( new char[] {'='} )[1];
				
				m_Names[i-1] = new MonoAssemblyName( sAssemblyName, sVersion, sCulture, sPublicKeyToken, sLine );
			}
			
			m_nCurrentName = 0;
		}
		
	    public int GetNextAssembly(out IApplicationContext ppAppCtx, out IAssemblyName ppName, uint dwFlags)
		{
			ppAppCtx = null;
			ppName = null;
			
			if ( m_nCurrentName >= m_Names.Length )
				return 1;
			
			ppName = m_Names[m_nCurrentName++];
			return 0;
		}
		
    	public int Reset()
		{
			Debug.Assert( false );
			return 0;
		}
		
    	public int Clone(out IAssemblyEnum ppEnum)
		{
			ppEnum = null;
			Debug.Assert( false );
			return 0;
		}
		
		MonoAssemblyName [] m_Names;
		int m_nCurrentName;
	}
#endif // __MonoCS__
