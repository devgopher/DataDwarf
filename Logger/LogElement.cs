/*
 * Пользователь: igor.evdokimov
 * Дата: 11.01.2016
 * Время: 10:45
 */
using System;

namespace Logger
{
	/// <summary>
	/// A common class for loggers
	/// </summary>
	public abstract class LogElement : ILogElement
	{
		protected LogElement() {}
		public abstract void Output( string input, string msg_type );
		public abstract void Output( string input, string msg_type, params object[] pars );
		
		protected object sync_object = new object();
	}
}
