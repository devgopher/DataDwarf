/*
 * Пользователь: igor.evdokimov
 * Дата: 02.11.2015
 * Время: 15:28
 */
using System;
using System.Resources;
using System.Reflection;

namespace DwarfDB.Global
{
	/// <summary>
	/// Description of StaticResourceManager.
	/// </summary>
	public static class StaticResourceManager
	{
		private static readonly ResourceManager string_manager = new ResourceManager("DwarfDB.Strings", Assembly.GetExecutingAssembly());
		
		static StaticResourceManager()
		{
			StringManager = string_manager;
		}
		
		public static ResourceManager StringManager {
			get; private set;
		}
		
		public static string GetStringResource( string res_name ) {
			return string_manager.GetString(res_name);
		}		
	}
}
