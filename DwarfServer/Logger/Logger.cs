/*
 * Пользователь: Igor.Evdokimov
 * Дата: 21.05.2014
 * Время: 13:52
 */
using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Logger
{
	/// <summary>
	/// Logger main class.
	/// </summary>
	public class Logger : IDisposable
	{
		public String Path { get; private set; }
		FileStream log_fs;
		StreamWriter log_sw;
		StreamReader log_sr;
		String application_name;
		readonly Encoding encoding;
		
		public Logger( string _path,
		              string _application_name,
		              Encoding _encoding )
		{
			Path = _path;
			//CheckPath();
			application_name = _application_name;
			encoding = _encoding;
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
		
		private void CheckPath() {
			if ( !Directory.Exists(this.Path) ) {
				Directory.CreateDirectory( this.Path );
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
	
		private void StartLog()
		{
			WriteIn("Assembly: " + Assembly.GetEntryAssembly().GetName().Name + " \r\n Version:"+
			        Assembly.GetEntryAssembly().GetName().Version +"\r\n");
		}
		
		public void WriteEntry(string content)
		{
			WriteIn(DateTime.Now.ToString("\r\ndd.MM.yyyy HH:mm:ss") + ": " + content);
		}
		
		public void WriteError(string content)
		{
			var ex_color = ConsoleUtils.GetConsoleFontColor();
			ConsoleUtils.SetConsoleFontColor( ConsoleColor.Red );
			WriteIn(DateTime.Now.ToString("\r\ndd.MM.yyyy HH:mm:ss") + ": ERROR:" + content);
			ConsoleUtils.SetConsoleFontColor( ex_color );
		}
		
		public void WriteWarning(string content)
		{
			var ex_color = ConsoleUtils.GetConsoleFontColor();
			ConsoleUtils.SetConsoleFontColor( ConsoleColor.Yellow );
			WriteIn(DateTime.Now.ToString("\r\ndd.MM.yyyy HH:mm:ss") + ": WARNING: " + content);
			ConsoleUtils.SetConsoleFontColor( ex_color );
		}

		public string GetText() {
			string rd_text = String.Empty;
			if (File.Exists(Path)) {
				using (log_fs = File.OpenRead(Path)) {
					using (log_sr = new StreamReader(log_fs, encoding)) {
						rd_text = log_sr.ReadToEnd();
					}
				}
			}
			return rd_text;
		}
		
		private void WriteIn( string input )
		{
			try {
				Console.WriteLine(input);
				if (!File.Exists(Path)) {
					using (log_fs = File.Open(Path, FileMode.CreateNew)) {
						using (log_sw = new StreamWriter(log_fs, encoding)) {
							log_sw.WriteLine(input);
						}
					}
				} else {
					using (log_fs = File.Open(Path, FileMode.Append)) {
						using (log_sw = new StreamWriter(log_fs, encoding)) {
							log_sw.WriteLine(input);
						}
					}
				}
			} catch ( IOException ex ) {
				var rand = new Random(DateTime.Now.Millisecond);
				var new_path = Path + "_" + rand.Next().ToString();
				while (File.Exists(new_path)) {
					new_path = Path + "_" + rand.Next().ToString();
				}
				Path = new_path;
				WriteIn(input);
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