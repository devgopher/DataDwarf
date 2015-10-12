/*
 * User: Igor
 * Date: 03.09.2015
 * Time: 23:33
 */
using System;

namespace DwarfDB.Config
{
	/// <summary>
	/// Description of Config.
	/// </summary>
	public sealed class Config
	{
		//#if DEBUG
		Config() {
			DataDirectory = HomePath+"/DataDwarf/";
		}
		//#endif
		
		/// <summary>
		/// A directory for databases
		/// </summary>
		public String DataDirectory {
			get; set;
		}
		
		private static Config instance = new Config();
		
		/// <summary>
		/// User's HOME path
		/// </summary>
		public string HomePath {
			get {
				return (Environment.OSVersion.Platform == PlatformID.Unix)
					? Environment.GetEnvironmentVariable("HOME")
					: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			}
		}
		
		public static Config Instance {
			get {
				return instance;
			}
		}
	}
}
