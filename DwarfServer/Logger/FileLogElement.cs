/*
 * Пользователь: igor.evdokimov
 * Дата: 11.01.2016
 * Время: 10:10
 */
using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Logger
{
	/// <summary>
	/// This class provides file logging functions
	/// </summary>
	public class FileLogElement : LogElement
	{
		FileStream log_fs;
		StreamWriter log_sw;
		
		public Encoding UseEncoding { get; private set; }
		public String Path { get; private set; }
		
		/// <summary>
		/// Outputs content
		/// </summary>
		/// <param name="input"></param>
		/// <param name="msg_type"></param>
		public override  void Output( string input, string msg_type )  {
			Output( input, msg_type, null );
		}
		
		/// <summary>
		/// Outputs content
		/// </summary>
		/// <param name="input">Content for output</param>
		/// <param name="msg_type">Message type</param>
		/// <param name="pars">Parameters array</param>
		public override  void Output( string input, string msg_type, params object[] pars ) {
			try {
				if ( Path == null )
					throw new IOException( "Path is null!" );
				if ( UseEncoding == null )
					throw new IOException( "UseEncoding is null!" );
				
				var content = String.Format( "{0}: {1}: {2}",
				                            DateTime.Now.ToString("\r\ndd.MM.yyyy HH:mm:ss"),
				                            msg_type,
				                            input);
				
				if (!File.Exists(Path)) {
					using (log_fs = File.Open(Path, FileMode.CreateNew)) {
						using (log_sw = new StreamWriter(log_fs, UseEncoding)) {
							log_sw.WriteLine(content);
						}
					}
				} else {
					using (log_fs = File.Open(Path, FileMode.Append)) {
						using (log_sw = new StreamWriter(log_fs, UseEncoding)) {
							log_sw.WriteLine(content);
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
				Output(input, msg_type);
			}
		}
		
		/// <summary>
		/// Getting a LogElement instance
		/// </summary>
		/// <param name="pars">Params array ( Encoding, Path )</param>
		/// <returns>Log element</returns>
		public static FileLogElement GetInstance( params object[] pars ) {
			if ( instance == null ) {
				instance  = new FileLogElement();
				if ( pars != null  ) {
					if ( pars.Length >= 1 ) {
						instance.UseEncoding = pars[0] as Encoding;
						if ( pars.Length == 2 )
							instance.Path =  pars[1] as String;
					}
				}
			}
			return instance;
		}
		
		protected static FileLogElement instance = null;
	}
}
