/*
 * Пользователь: Igor.Evdokimov
 * Дата: 16.09.2015
 * Время: 16:44
 */
using System;

namespace DwarfDB.Checks
{
	/// <summary>
	/// ExecutionTimeCheck - a class for making
	/// checks of code execution velocity
	/// </summary>
	public static class ExecutionTimeCheck
	{
		static ExecutionTimeCheck()
		{
		}
		
		public delegate void CFunc();
				
		/// <summary>
		/// Measuring execution time and returns it in milliseconds
		/// </summary>
		/// <param name="fnc"></param>
		/// <returns></returns>
		public static double DoCheck( CFunc fnc ) {
			var begin_time = DateTime.Now;
			fnc();
			var delta_time = DateTime.Now - begin_time;
			return delta_time.TotalMilliseconds;
		}
	}
}
