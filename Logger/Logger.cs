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
using Global;

namespace Logger
{
	/// <summary>
	/// Logger main class.
	/// </summary>
	public class Logger : IDisposable
	{
		String application_name;
		readonly Encoding encoding;
		String log_text = String.Empty;
		
		static Assembly assembly = Assembly.GetEntryAssembly();
		static String assembly_name = "tmp";
		static String assembly_fullname = "tmp";
		static String assembly_ver = "0.0.0";
		
		private readonly List<ILogElement> log_elements = new List<ILogElement>();
		public String Path { get; private set; }
		
		/// <summary>
		/// This property means a common amount of Logger objects, we can produce
		/// true = single-object mode
		/// false- multi object moe
		/// </summary>
		public static bool IsSingle { get; set; }
		
		static Logger()
		{
			if ( assembly != null ) {
				assembly_name = assembly.GetName().Name;
				assembly_fullname = Assembly.GetEntryAssembly().FullName;
				assembly_ver = Assembly.GetEntryAssembly().GetName().Version.ToString();
			}
			IsSingle = true;
		}
		
		protected Logger( string _path,
		                 string _application_name,
		                 Encoding _encoding,
		                 bool is_single = true )
		{
			Path = _path;
			application_name = _application_name;
			encoding = _encoding;
			
			#if DEBUG
			log_elements.Add( FileLogElement.GetInstance( encoding, Path ) );
			log_elements.Add( ConsoleLogElement.GetInstance() );
			#endif
			
			IsSingle = is_single;
			StartLog();
			WriteEntry("Start logging...");
		}
		
		/// <summary>
		/// Adds a new log output element
		/// </summary>
		/// <param name="elem"></param>
		public void AddLogElement( ILogElement elem ) {
			if ( elem != null )
				log_elements.Add( elem );
		}
		
		/// <summary>
		/// User's HOME path
		/// </summary>
		private static string HomePath
		{
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
		
		private static Logger singleton_instance = null;
		
		/// <summary>
		/// Getting/defining a Logger instance
		/// </summary>
		/// <param name="log_dir">A directory for logs</param>
		/// <param name="filename">Log file name</param>
		/// <returns></returns>
		public static Logger GetInstance( string log_dir = null, string filename = null )
		{
			if ( IsSingle )
				return GetInstanceSingle( log_dir, filename );
			return GetInstanceMulti( log_dir, filename );
		}
		
		private static Logger GetInstanceSingle( string log_dir = null, string filename = null ) {
			if ( singleton_instance == null ) {
				try {
					var new_log_dir = CreateLogDir( log_dir );
					var filepath = CreateLogFilepath( new_log_dir, filename );
					singleton_instance = new Logger( filepath,
					                                assembly_fullname,
					                                Encoding.Default );
				}  catch ( Exception ex ) {
					throw new IOException( " Error while defining a new logger instance: "+ex.Message, ex );
				}
			}
			return singleton_instance;
		}
		
		private static string CreateLogDir( string log_dir = null ) {
			var new_log_dir = String.Empty;
			if ( log_dir == null  ) {
				if ( assembly != null )
					new_log_dir = HomePath+
						System.IO.Path.DirectorySeparatorChar+ assembly_name+
						System.IO.Path.DirectorySeparatorChar;
				else
					new_log_dir =  TempPath+System.IO.Path.DirectorySeparatorChar;
			} else
				new_log_dir = log_dir;
			
			Directory.CreateDirectory( new_log_dir );
			
			return new_log_dir;
		}
		
		private static string CreateLogFilepath( string log_dir, string filename = null ) {
			var filepath = String.Empty;
			if ( filename != null )
				filepath = log_dir + System.IO.Path.DirectorySeparatorChar +filename;
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
			return filepath;
		}
		
		private static Logger GetInstanceMulti( string log_dir = null, string filename = null ) {
			Logger ret = null;

			try {
				var new_log_dir = CreateLogDir( log_dir );
				var filepath = CreateLogFilepath( new_log_dir, filename );
				
				if ( !instances.ContainsKey(filepath) ) {
					ret = new Logger( filepath,
					                 assembly_fullname,
					                 Encoding.Default );
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
				log_elem.Output( content, StaticResourceManager.GetStringResource("LOGGER_OUTPUT_INFO"));
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
				log_elem.Output( content,
				                StaticResourceManager.GetStringResource("LOGGER_OUTPUT_ENTRY"),
				                ConsoleColor.DarkGreen );
			}
		}
		
		/// <summary>
		/// Writes a debug message
		/// </summary>
		/// <param name="content"></param>
		public void WriteDebug( string content )
		{
			log_text+="\r\n "+content;
			foreach ( var log_elem in log_elements ) {
				log_elem.Output( content,
				                StaticResourceManager.GetStringResource("LOGGER_OUTPUT_DEBUG"),
				                ConsoleColor.Magenta );
			}
		}
		
		/// <summary>
		/// Writes an error message
		/// </summary>
		/// <param name="content"></param>
		public void WriteError( string content )
		{
			log_text+="\r\n "+content;
			foreach ( var log_elem in log_elements ) {
				log_elem.Output( content,
				                StaticResourceManager.GetStringResource("LOGGER_OUTPUT_ERROR"),
				                ConsoleColor.Red );
			}
		}
		
		/// <summary>
		/// Writes an error message
		/// </summary>
		/// <param name="exception"></param>
		public void WriteError( Exception exception )
		{
			string content = String.Empty;
			#if DEBUG
			content = exception.Message + ":"+exception.StackTrace;
			#else
			content = exception.Message;
			#endif
			log_text+="\r\n "+content;
			
			foreach ( var log_elem in log_elements ) {
				log_elem.Output( content,
				                StaticResourceManager.GetStringResource("LOGGER_OUTPUT_ERROR"),
				                ConsoleColor.Red );
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
				log_elem.Output( content,
				                StaticResourceManager.GetStringResource("LOGGER_OUTPUT_WARNING"),
				                ConsoleColor.Yellow );
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