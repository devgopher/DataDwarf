/*
 * Пользователь: Igor.Evdokimov
 * Дата: 21.05.2014
 * Время: 13:52
 */
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace Logger
{
	/// <summary>
	/// Logger main class.
	/// </summary>
	public class Logger : IDisposable
	{
		FileStream log_fs;
		StreamWriter log_sw;
		String application_name;
		readonly Encoding encoding;
		String log_text = String.Empty;
		
		static Assembly assembly = Assembly.GetEntryAssembly();
		static String assembly_name = "tmp";
		static String assembly_fullname = "tmp";
		static String assembly_ver = "0.0.0";
		
		public readonly List<LogElement> log_elements = new List<LogElement>();
		public String Path { get; private set; }
				
		static Logger() {
			if ( assembly != null ) {
				assembly_name = assembly.GetName().Name;
				assembly_fullname = Assembly.GetEntryAssembly().FullName;
				assembly_ver = Assembly.GetEntryAssembly().GetName().Version.ToString();
			}
		}
		
		protected Logger( string _path,
		                 string _application_name,
		                 Encoding _encoding )
		{
			Path = _path;
			application_name = _application_name;
			encoding = _encoding;
			
			
			log_elements.Add( FileLogElement.GetInstance( encoding, Path ) );
			log_elements.Add( ConsoleLogElement.GetInstance() );
			
			StartLog();
			WriteEntry("Start logging...");
		}
		
		/// <summary>
		/// User's HOME path
		/// </summary>
		private static string HomePath {
			get {
				return (Environment.OSVersion.Platform == PlatformID.Unix)
					? Environment.GetEnvironmentVariable("HOME")
					: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			}
		}
		
		/// <summary>
		/// User's TEMP path
		/// </summary>
		private static string TempPath {
			get {
				return System.IO.Path.GetTempPath();
			}
		}
		
		#region GetInstance
		private static ConcurrentDictionary< String, Logger > instances = 
			new ConcurrentDictionary< String, Logger >();
		
		/// <summary>
		/// Getting/defining a Logger instance
		/// </summary>
		/// <param name="log_dir">A directory for logs</param>
		/// <param name="filename">Log file name</param>
		/// <returns></returns>
		public static Logger GetInstance( string log_dir = null, string filename = null ) {
			Logger ret = null;
			String filepath = "";
			
			try {
				if ( log_dir == null  ) {
					if ( assembly != null )
						log_dir = HomePath+@"\"+ assembly_name+@"\";
					else
						log_dir =  TempPath+@"\";					
				}
				
				Directory.CreateDirectory( log_dir );
				
				if ( filename != null )
					filepath = log_dir + @"\"+filename;
				else {
					if ( assembly != null )
						filepath = log_dir +
							assembly_name+
							DateTime.Now.ToString( "__HH_mm_ss__dd.MM.yyyy" )+
							".log";
					else
						filepath = log_dir +
							"tmp_log"+
							DateTime.Now.ToString( "__HH_mm_ss__dd.MM.yyyy" )+
							".log";
				}
				
				var encoding = Encoding.Default;
				
				if ( !instances.ContainsKey(filepath) ) {
					ret = new Logger( filepath,
					                 assembly_fullname,
					                 encoding );
					instances[filepath] = ret;
				} else {
					ret = instances[filepath];
				}
			}  catch ( Exception ex ) {
				throw new IOException( " Error while defining a new logger instance: "+ex.Message, ex );
			}
			return ret;
		}
		#endregion
		
		/// <summary>
		/// Starts logging process
		/// </summary>
		private void StartLog()
		{
			var content = "Assembly: " + assembly_name + " \r\n Version:"+
				assembly_ver;
			log_text+="\r\n "+content;
			foreach ( var log_elem in log_elements ) {
				log_elem.Output( content, "INFO");
			}
		}
		
		public String GetText() {
			return log_text;
		}
		
		/// <summary>
		/// Writes a simple entry
		/// </summary>
		/// <param name="content"></param>
		public void WriteEntry( string content )
		{
			log_text+="\r\n "+content;
			foreach ( var log_elem in log_elements ) {
				log_elem.Output( content, "MESSAGE", ConsoleColor.DarkGreen );
			}
		}
		
		/// <summary>
		/// Writes an error message
		/// </summary>
		/// <param name="content"></param>
		public void WriteError(string content)
		{
			log_text+="\r\n "+content;
			foreach ( var log_elem in log_elements ) {
				log_elem.Output( content, "ERROR", ConsoleColor.Red );
			}
		}
		
		/// <summary>
		/// Writes a warning message
		/// </summary>
		/// <param name="content"></param>
		public void WriteWarning(string content)
		{
			log_text+="\r\n "+content;
			foreach ( var log_elem in log_elements ) {
				log_elem.Output( content, "WARNING", ConsoleColor.Yellow );
			}
		}
		
		#region IDisposable
		public void Dispose()
		{
			WriteEntry("Exit");
		}
		#endregion
	}
}