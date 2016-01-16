/*
 * Пользователь: igor.evdokimov
 * Дата: 11.01.2016
 * Время: 10:10
 */
using System;

namespace Logger
{
	/// <summary>
	/// This class provides console logging functions
	/// </summary>
	public class ConsoleLogElement : LogElement
	{
		public ConsoleLogElement()
		{
		}
		
		/// <summary>
		/// Outputs content
		/// </summary>
		/// <param name="input">Content for output</param>
		/// <param name="msg_type">Message type</param>
		public override void Output( string input, string msg_type )  {
			Output( input, msg_type, null );
		}
		
		/// <summary>
		/// Outputs content
		/// </summary>
		/// <param name="input">Content for output</param>
		/// <param name="msg_type">Message type</param>
		/// <param name="pars">Parameters array ( ConsoleColor )</param>
		public override void Output( string input, string msg_type, params object[] pars ) {
			var content = String.Format( "{0}: {1}: {2}",
			                            DateTime.Now.ToString("\r\ndd.MM.yyyy HH:mm:ss"),
			                            msg_type,
			                            input );
			if ( pars != null ) {
				if ( pars.Length == 1 ) {
					if (pars[0] is ConsoleColor ) {
						var ex_color = ConsoleUtils.GetConsoleFontColor();
						ConsoleUtils.SetConsoleFontColor( (ConsoleColor)(pars[0]));
						Console.WriteLine( content );
						ConsoleUtils.SetConsoleFontColor( ex_color );
					}
				}
			} else
				Console.WriteLine( content );
		}
		
		/// <summary>
		/// Getting a LogElement instance
		/// </summary>
		/// <returns>Log element</returns>
		public static ConsoleLogElement GetInstance() {
			if ( instance == null ) {
				instance  = new ConsoleLogElement();
			}
			return instance;
		}
		
		protected static ConsoleLogElement instance = null;
	}
}
