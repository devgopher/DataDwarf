/*
 * User: Igor
 * Date: 08.01.2016
 * Time: 19:11
 */
using System;

namespace Logger
{
	public static class ConsoleUtils
	{
		public static ConsoleColor GetConsoleFontColor() {
			return Console.ForegroundColor;
		}
		
		public static void SetConsoleFontColor( ConsoleColor cc ) {
			Console.ForegroundColor = cc;
		}
	}
}
