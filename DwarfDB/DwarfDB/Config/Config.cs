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
			DataDirectory = @"C:\TestBed\";
		}
		//#endif
		
		public String DataDirectory {
			get; set;
		}
		
		private static Config instance = new Config();
		
		public static Config Instance {
			get {
				return instance;
			}
		}
	}
}
