/*
 * Пользователь: Igor.Evdokimov
 * Дата: 26.08.2015
 * Время: 12:04
 */
using System;
using System.IO;
using System.Text;

namespace DwarfDB.Errors
{
	/// <summary>
	/// A class for errors output
	/// </summary>
	public class ErrorProcessing
	{
		public ErrorProcessing()
		{
		}
		
		public static void Display( String _error_text ) {
			System.Console.WriteLine( String.Format( "Error: {0} When: {1}", _error_text, DateTime.Now.ToLocalTime()));
		}
		
		public static void Display( String _error_text,
		                           String _where,
		                           String _advices,
		                           DateTime date_time ) {
			System.Console.WriteLine( String.Format( "Error: {0} in: {1}. When: {2}. To Fix: {3}",
			                                        _error_text, _where, date_time.ToLocalTime().ToString(), _advices ));
		}
		
		public static ConsoleColor GetConsoleFontColor() {
			return Console.ForegroundColor;
		}
		public static void SetConsoleFontColor( ConsoleColor cc ) {
			Console.ForegroundColor = cc;
		}
		
		public static void Display( Stream out_str,
		                           String _error_text,
		                           String _where,
		                           String _advices,
		                           DateTime date_time ) {
			var out_sw = new StreamWriter( out_str );
			
			var old_color = GetConsoleFontColor();
			
			SetConsoleFontColor( ConsoleColor.Red );
			
			out_sw.WriteLine( String.Format( "Error: {0} in: {1}. When: {2}. To Fix: {3}",
			                                _error_text, _where, date_time.ToLocalTime().ToString(), _advices ));
			
			out_sw.Close();
			SetConsoleFontColor( old_color );
		}
	}
}
