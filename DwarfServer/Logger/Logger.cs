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
		
		public List<LogElement> log_elements = new List<LogElement>();
		public String Path { get; private set; }
		
		public Logger( string _path,
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
		
		#region GetInstance
		private static Logger logger_instance = null;
		
		public static Logger GetInstance() {
			if ( logger_instance == null )
				logger_instance = new Logger( HomePath+
				                             @"\DataDwarf\"+
				                             Assembly.GetEntryAssembly().GetName().Name+
				                             DateTime.Now.ToString( "__HH_mm_ss__dd.MM.yyyy" )+
				                             ".log",
				                             Assembly.GetEntryAssembly().FullName,
				                             Encoding.Default );
			return logger_instance;
		}
		#endregion
		
		/// <summary>
		/// Starts logging process
		/// </summary>
		private void StartLog()
		{
			var content = "Assembly: " + Assembly.GetEntryAssembly().GetName().Name + " \r\n Version:"+
				Assembly.GetEntryAssembly().GetName().Version;
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