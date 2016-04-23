/*
 * Пользователь: Igor.Evdokimov
 * Дата: 26.08.2015
 * Время: 12:04
 */
using System;

namespace DwarfDB.Errors
{
	/// <summary>
	/// A class for errors output
	/// </summary>
	public class Messages
	{
		private static Logger.Logger logger = 
			Logger.Logger.GetInstance();
		
		public static void DisplayError( String _error_text ) {
			Console.WriteLine( String.Format( "Error: {0} When: {1}", 
			                                        _error_text, 
			                                        DateTime.Now.ToLocalTime()));
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
	}
}
