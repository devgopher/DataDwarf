/*
 * Пользователь: igor.evdokimov
 * Дата: 02.11.2015
 * Время: 15:28
 */
using System;
using System.Resources;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;

namespace Global
{
	/// <summary>
	/// A Static (global) resource manager for a simple access to .resx and .resource files
	/// </summary>
	public static class StaticResourceManager
	{
		private static readonly List<ResourceManager> resource_mgrs = 
			new List<ResourceManager>();
		private static readonly string appdir = 
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		
		private static List<string> FindResourceTitles() {
			resource_mgrs.Clear();
			var ret = new List<string>();
			var resources_dir = Path.Combine(appdir,"Resources");
			
			var files = Directory.GetFiles( resources_dir, "*.resources" );
			const string regex_pattern = @"(.*)\.(.*)\.(.*)";
			
			foreach ( var file in files ) {
				var match = Regex.Match( Path.GetFileName(file), regex_pattern );
				if ( match.Groups.Count >= 3 ) {
					var resource_title = match.Groups[1].Value;
					if ( !ret.Contains( resource_title ) )
						ret.Add( resource_title );
				}
			}
			return ret;
		}
		
		static StaticResourceManager()
		{
			var resource_titles = FindResourceTitles();

			foreach ( var rt in resource_titles ) {
				var string_manager = ResourceManager.CreateFileBasedResourceManager( rt,
				                                                                    appdir +
				                                                                    Path.DirectorySeparatorChar+
				                                                                    "Resources",
				                                                                    null
				                                                                   );
				resource_mgrs.Add( string_manager );
			}
		}
		
		public static string GetStringResource( string res_name ) {
			foreach ( var rm in resource_mgrs ) {
				var tmp = rm.GetString( res_name );
				if ( tmp != null )
					return tmp;
			}
			return null;
		}

		public static Object GetObjectResource( string res_name ) {
			foreach ( var rm in resource_mgrs ) {
				var tmp = rm.GetObject( res_name );
				if ( tmp != null )
					return tmp;
			}
			return null;
		}
	}
}
