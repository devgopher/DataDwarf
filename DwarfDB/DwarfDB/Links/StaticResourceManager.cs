﻿/*
 * Пользователь: igor.evdokimov
 * Дата: 02.11.2015
 * Время: 15:28
 */
using System;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.IO;

namespace DwarfDB.Global
{
	/// <summary>
	/// A Static (global) resource manager for a simple access to .resx and .resource filesss
	/// </summary>
	public static class StaticResourceManager
	{
		static StaticResourceManager()
		{
			StringManager = ResourceManager.CreateFileBasedResourceManager( "CommonStrings",
			                                                               Directory.GetCurrentDirectory()+@"\resources",
			                                                               null
			                                                              );
		}

		public static String Language { 
			get ; private set; 
		}

		public static ResourceManager StringManager {
			get; private set;
		}
		
		public static string GetStringResource( string res_name ) {
			return StringManager.GetString( res_name );
		}

		public static Object GetObjectResource( string res_name ) {
			return StringManager.GetObject( res_name );
		}
	}
}
