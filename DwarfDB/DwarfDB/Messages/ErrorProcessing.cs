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
	public class Messages
	{
		public Messages()
		{
		}
		
		private static Logger.Logger logger = Logger.Logger.GetInstance();
		
		public static void Display( String _error_text ) {
			System.Console.WriteLine( String.Format( "Error: {0} When: {1}", _error_text, DateTime.Now.ToLocalTime()));
		}
		
		public static void DisplayError( String _error_text,
		                           String _where,
		                           String _advices,
		                           DateTime date_time ) {
			logger.WriteError( String.Format( "Error: {0} in: {1}. When: {2}. To Fix: {3}",
			                                 _error_text,
			                                 _where,
			                                 date_time.ToLocalTime(),
			                                 _advices ));
		}
		
		public static void DisplayError( Stream out_str,
		                           String _error_text,
		                           String _where,
		                           String _advices,
		                           DateTime date_time ) {
			logger.WriteError( String.Format( "Error: {0} in: {1}. When: {2}. To Fix: {3}",
			                                 _error_text,
			                                 _where,
			                                 date_time.ToLocalTime(),
			                                 _advices ));			
		}
	}
}
